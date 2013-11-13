/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.IncrementalSearch;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.NodejsTools {
    internal sealed class EditFilter : IOleCommandTarget {
        private readonly ITextView _textView;
        private readonly IEditorOperations _editorOps;
        private readonly IIntellisenseSessionStack _intellisenseStack;
        private readonly IComponentModel _compModel;
        private readonly IClassifier _classifier;
        private readonly IIncrementalSearch _incSearch;
        private readonly ICompletionBroker _broker;
        private IOleCommandTarget _next;
        private ICompletionSession _activeSession;

        public EditFilter(ITextView textView, IEditorOperations editorOps, IIntellisenseSessionStack intellisenseStack, IComponentModel compModel) {
            _textView = textView;
            _editorOps = editorOps;
            _intellisenseStack = intellisenseStack;
            _compModel = compModel;
            var agg = _compModel.GetService<IClassifierAggregatorService>();
            _classifier = agg.GetClassifier(textView.TextBuffer);
            _incSearch = _compModel.GetService<IIncrementalSearchFactoryService>().GetIncrementalSearch(_textView);
            _broker = _compModel.GetService<ICompletionBroker>();
        }

        private bool ShouldTriggerRequireIntellisense() {
            return CompletionSource.ShouldTriggerRequireIntellisense(
                _textView.Caret.Position.BufferPosition,
                _classifier,
                false
            );
        }

        internal void AttachKeyboardFilter(IVsTextView vsTextView) {
            if (_next == null) {
                ErrorHandler.ThrowOnFailure(vsTextView.AddCommandFilter(this, out _next));
            }
        }

        private SkipJsLsFilter SkipJsFilter {
            get {
                SkipJsLsFilter skipJsFilter;
                if (_textView.Properties.TryGetProperty<SkipJsLsFilter>(typeof(SkipJsLsFilter), out skipJsFilter)) {
                    return skipJsFilter;
                }
                return null;
            }
        }

        private static HashSet<char> _commitChars = new HashSet<char>("{}[](),:;+-*%&|^~=<>#\\");

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            // disable JavaScript language services auto formatting features, this is because
            // they are not aware that we have an extra level of indentation
            if (pguidCmdGroup == VSConstants.VSStd2K) {
                switch ((VSConstants.VSStd2KCmdID)nCmdID) {
                    case VSConstants.VSStd2KCmdID.RETURN:
                        if (_intellisenseStack.TopSession != null && 
                            _intellisenseStack.TopSession is ICompletionSession &&
                            !_intellisenseStack.TopSession.IsDismissed) {
                            ((ICompletionSession)_intellisenseStack.TopSession).Commit();
                        } else {
                            if (SkipJsFilter != null) {                                
                                var res = SkipJsFilter.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                                SmartIndent smartIndent;
                                if (ErrorHandler.Succeeded(res) && _textView.Properties.TryGetProperty<SmartIndent>(typeof(SmartIndent), out smartIndent)) {
                                    var indentation = smartIndent.GetDesiredIndentation(_textView.Caret.Position.BufferPosition.GetContainingLine());
                                    if (indentation != null) {
                                        _textView.Caret.MoveTo(
                                            new VisualStudio.Text.VirtualSnapshotPoint(
                                                _textView.Caret.Position.BufferPosition.GetContainingLine(),
                                                indentation.Value
                                            )
                                        );
                                    }
                                }
                                return res;
                            }

                            _editorOps.InsertNewLine();
                        }
                        return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.TYPECHAR:
                        if (!_incSearch.IsActive) {
                            var ch = (char)(ushort)System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant(pvaIn);

                            if (_activeSession != null && !_activeSession.IsDismissed) {
                                if (_activeSession.SelectedCompletionSet.SelectionStatus.IsSelected &&
                                    _commitChars.Contains(ch)) {
                                    _activeSession.Commit();
                                } else if (!CompletionSource.IsIdentifierChar(ch) && ch != '.' && ch != '/') {
                                    _activeSession.Dismiss();
                                }
                            }

                            if ((ch == '.' && !NodejsPackage.Instance.LangPrefs.AutoListMembers) ||
                                (ch == '(' && !NodejsPackage.Instance.LangPrefs.AutoListParams)) {
                                return (SkipJsFilter ?? _next).Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            } else if (ch == '}' || ch == ';') {
                                if (SkipJsFilter != null) {
                                    return SkipJsFilter.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                                }
                                _editorOps.InsertText(ch.ToString());
                                return VSConstants.S_OK;
                            } else if ((ch == '(' || ch == '"' || ch == '\'') && 
                                CompletionSource.ShouldTriggerRequireIntellisense(_textView.Caret.Position.BufferPosition, _classifier, ch != '(')) {
                                // we don't want to forward the ( down to JS as it'll trigger a signature help
                                // session.
                                _editorOps.InsertText(ch.ToString());
                                TriggerCompletionSession(false);
                                return VSConstants.S_OK;
                            }

                            int hr;
                            if (!NodejsPackage.Instance.LangPrefs.AutoListMembers) {
                                hr = (SkipJsFilter ?? _next).Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            } else {
                                hr = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            }
                            
                            if (ErrorHandler.Succeeded(hr) && _activeSession != null && !_activeSession.IsDismissed) {
                                _activeSession.Filter();
                            }
                            return hr;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.PASTE:
                        if (SkipJsFilter != null) {
                            return SkipJsFilter.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                        }
                        _editorOps.Paste();
                        return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.TAB:
                        if (_intellisenseStack.TopSession != null &&
                            _intellisenseStack.TopSession is ICompletionSession &&
                            !_intellisenseStack.TopSession.IsDismissed) {
                            ((ICompletionSession)_intellisenseStack.TopSession).Commit();
                        } else {
                            if (SkipJsFilter != null) {
                                return SkipJsFilter.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            }

                            _editorOps.Indent();
                        }
                        return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                    case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                        if (TriggerCompletionSession((VSConstants.VSStd2KCmdID)nCmdID == VSConstants.VSStd2KCmdID.COMPLETEWORD)) {
                            return VSConstants.S_OK;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.BACKSPACE:
                    case VSConstants.VSStd2KCmdID.DELETE:
                    case VSConstants.VSStd2KCmdID.DELETEWORDLEFT:
                    case VSConstants.VSStd2KCmdID.DELETEWORDRIGHT: {
                            int res = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            if (_activeSession != null && !_activeSession.IsDismissed) {
                                _activeSession.Filter();
                            }
                            return res;
                        }
                }
            } else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                switch ((VSConstants.VSStd97CmdID)nCmdID) {
                    case VSConstants.VSStd97CmdID.Paste:
                        _editorOps.Paste();
                        return VSConstants.S_OK;
                }
            }

            return _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void Dismiss() {
            while (_intellisenseStack.TopSession != null) {
                _intellisenseStack.TopSession.Dismiss();
            }
        }

        internal bool TriggerCompletionSession(bool completeWord) {
            Dismiss();

            _activeSession = _broker.TriggerCompletion(_textView);

            if (_activeSession != null) {
                if (completeWord &&
                    _activeSession.CompletionSets.Count == 1 &&
                    _activeSession.CompletionSets[0].Completions.Count == 1) {
                    _activeSession.Commit();
                    _activeSession = null;
                } else {
                    _activeSession.Dismissed += OnCompletionSessionDismissed;
                    _activeSession.Committed += OnCompletionSessionDismissed;
                }
                return true;
            }
            return false;
        }

        private void OnCompletionSessionDismissed(object sender, System.EventArgs e) {
            // We've just been told that our active session was dismissed.  We should remove all references to it.
            _activeSession.Dismissed -= OnCompletionSessionDismissed;
            _activeSession.Committed -= OnCompletionSessionDismissed;
            _activeSession = null;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                for (int i = 0; i < cCmds; i++) {
                    switch ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd97CmdID.GotoDefn:
                            // disable goto definition, it goes to the wrong location.
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }
            return _next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
