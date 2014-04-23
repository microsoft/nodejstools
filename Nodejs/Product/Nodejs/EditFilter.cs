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
using Microsoft.NodejsTools.Formatting;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.IncrementalSearch;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace Microsoft.NodejsTools {
    internal sealed class EditFilter : IOleCommandTarget {
        private readonly ITextView _textView;
        private readonly IEditorOperations _editorOps;
        private readonly IIntellisenseSessionStack _intellisenseStack;
        private readonly IComponentModel _compModel;
        private readonly IClassifier _classifier;
        private readonly IIncrementalSearch _incSearch;
        private readonly ICompletionBroker _broker;
        private readonly IEditorOptions _editorOptions;
        private IOleCommandTarget _next;
        private ICompletionSession _activeSession;

        public EditFilter(ITextView textView, IEditorOperations editorOps, IEditorOptions editorOptions, IIntellisenseSessionStack intellisenseStack, IComponentModel compModel) {
            _textView = textView;
            _editorOps = editorOps;            
            _intellisenseStack = intellisenseStack;
            _compModel = compModel;
            var agg = _compModel.GetService<IClassifierAggregatorService>();
            _classifier = agg.GetClassifier(textView.TextBuffer);
            _incSearch = _compModel.GetService<IIncrementalSearchFactoryService>().GetIncrementalSearch(_textView);
            _broker = _compModel.GetService<ICompletionBroker>();
            _editorOptions = editorOptions;
            textView.Closed += TextViewClosed;
            string path = GetFilePath();
            if (path != null) {
                System.Diagnostics.Debug.Assert(JavaScriptFormattingService.Instance != null);
                JavaScriptFormattingService.Instance.AddDocument(path, textView.TextBuffer);
            }
        }

        private void TextViewClosed(object sender, EventArgs e) {
            string path = GetFilePath();
            if (path != null) {
                JavaScriptFormattingService.Instance.RemoveDocument(path);
            }
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

        private void OnCompletionSessionDismissedOrCommitted(object sender, System.EventArgs e) {
            // We've just been told that our active session was dismissed.  We should remove all references to it.
            _activeSession.Committed -= OnCompletionSessionDismissedOrCommitted;
            _activeSession.Dismissed -= OnCompletionSessionDismissedOrCommitted;
            _activeSession = null;
        }


        private static HashSet<char> _commitChars = new HashSet<char>("{}[](),:;+-*%&|^~=<>#\\");

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            int hr;
            // disable JavaScript language services auto formatting features, this is because
            // they are not aware that we have an extra level of indentation
            if (pguidCmdGroup == VSConstants.VSStd2K) {
                switch ((VSConstants.VSStd2KCmdID)nCmdID) {
                    case VSConstants.VSStd2KCmdID.FORMATSELECTION: FormatSelection(); return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.FORMATDOCUMENT: FormatDocument(); return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.RETURN:
                        if (_intellisenseStack.TopSession != null && 
                            _intellisenseStack.TopSession is ICompletionSession &&
                            !_intellisenseStack.TopSession.IsDismissed) {
                            ((ICompletionSession)_intellisenseStack.TopSession).Commit();
                        } else {
                            hr = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
#if FALSE
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
#endif
                            FormatAfterTyping('\n');
                            return hr;
                        }
                        return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.TYPECHAR:
                        if (!_incSearch.IsActive) {
                            var ch = (char)(ushort)System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant(pvaIn);
#if FALSE
                            if (_activeSession != null && !_activeSession.IsDismissed) {
                                if (_activeSession.SelectedCompletionSet.SelectionStatus.IsSelected &&
                                    _commitChars.Contains(ch)) {
                                    _activeSession.Commit();
                                } else if (!CompletionSource.IsIdentifierChar(ch) && ch != '.' && ch != '/') {
                                    _activeSession.Dismiss();
                                }
                            }
#endif
                            int res = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                            switch(ch) {
#if FALSE
                                case '.':
                                    if (NodejsPackage.Instance.LangPrefs.AutoListMembers) {
                                        TriggerCompletionSession(false);
                                    }
                                    break;
#endif
                                case '}':
                                case ':':
                                    FormatAfterTyping(ch);
                                    break;
#if FALSE
                                case '\'':
                                case '"':
                                    if (CompletionSource.ShouldTriggerRequireIntellisense(_textView.Caret.Position.BufferPosition, _classifier, ch != '(')) {
                                        TriggerCompletionSession(false);
                                    }
                                    break;
#endif
#if FALSE
                                case '(':
                                    if (PythonToolsPackage.Instance.LangPrefs.AutoListParams) {
                                        OpenParenStartSignatureSession();
                                    }
                                    break;
                                case ')':
                                    if (_sigHelpSession != null) {
                                        _sigHelpSession.Dismiss();
                                        _sigHelpSession = null;
                                    }

                                    if (PythonToolsPackage.Instance.LangPrefs.AutoListParams) {
                                        // trigger help for outer call if there is one
                                        TriggerSignatureHelp();
                                    }
                                    break;
                                case '=':
                                case ',':
                                    if (_sigHelpSession == null) {
                                        if (PythonToolsPackage.Instance.LangPrefs.AutoListParams) {
                                            CommaStartSignatureSession();
                                        }
                                    } else {
                                        UpdateCurrentParameter();
                                    }
                                    break;
#endif
                            }

                            if (_activeSession != null && !_activeSession.IsDismissed) {
                                _activeSession.Filter();
                            }

                            return res;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.PASTE: 
                        var curVersion = _textView.TextBuffer.CurrentSnapshot;
                        hr = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
#if FALSE
                        if (SkipJsFilter != null) {
                            hr = SkipJsFilter.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                        } else {
                            if (_editorOps.Paste()) {
                                hr = VSConstants.S_OK;
                            } else {
                                hr = VSConstants.E_FAIL;
                            }
                        }
#endif

                        if (ErrorHandler.Succeeded(hr)) {
                            FormatAfterPaste(curVersion);
                        }
                        return hr;
#if FALSE
                    case VSConstants.VSStd2KCmdID.TAB:
                        if (_intellisenseStack.TopSession != null &&
                            _intellisenseStack.TopSession is ICompletionSession &&
                            !_intellisenseStack.TopSession.IsDismissed) {
                            ((ICompletionSession)_intellisenseStack.TopSession).Commit();
                        } else {
                            return _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
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
#endif
                }
            } else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                switch ((VSConstants.VSStd97CmdID)nCmdID) {
                    case VSConstants.VSStd97CmdID.Paste:
                        var curVersion = _textView.TextBuffer.CurrentSnapshot;
                        hr = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                        if (ErrorHandler.Succeeded(hr)) {
                            FormatAfterPaste(curVersion);
                        }
                        return hr;
                }
            }

            return _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void Dismiss() {
            while (_intellisenseStack.TopSession != null) {
                _intellisenseStack.TopSession.Dismiss();
            }
        }
#if FALSE
        internal bool TriggerCompletionSession(bool completeWord) {
            Dismiss();

            _activeSession = _broker.TriggerCompletion(_textView);

            if (_activeSession != null) {
                FuzzyCompletionSet set;
                if (completeWord &&
                    _activeSession.CompletionSets.Count == 1 &&
                    (set = _activeSession.CompletionSets[0] as FuzzyCompletionSet) != null &&
                    set.SelectSingleBest()) {
                    _activeSession.Commit();
                    _activeSession = null;
                } else {
                    _activeSession.Filter();
                    _activeSession.Dismissed += OnCompletionSessionDismissedOrCommitted;
                    _activeSession.Committed += OnCompletionSessionDismissedOrCommitted;
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
#endif
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
#if FALSE
                for (int i = 0; i < cCmds; i++) {
                    switch ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd97CmdID.GotoDefn:
                            // disable goto definition, it goes to the wrong location.
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
#endif
            } else if (pguidCmdGroup == VSConstants.VSStd2K) {
                for (int i = 0; i < cCmds; i++) {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
                        case VSConstants.VSStd2KCmdID.FORMATSELECTION:
                            string path = GetFilePath();
                            if (path != null) {
                                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                                return VSConstants.S_OK;
                            }
                            break;
                    }
                }
            }
            return _next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #region Formatting support

        private FormatCodeOptions CreateFormatOptions() {
            FormatCodeOptions res = new FormatCodeOptions();
            res.TabSize = _editorOptions.GetTabSize();
            res.IndentSize = _editorOptions.GetIndentSize();
            res.ConvertTabsToSpaces = _editorOptions.IsConvertTabsToSpacesEnabled();
            res.NewLineCharacter = _editorOptions.GetNewLineCharacter();

            res.InsertSpaceAfterCommaDelimiter = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterComma;
            res.InsertSpaceAfterSemicolonInForStatements = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterSemicolonInFor;
            res.InsertSpaceBeforeAndAfterBinaryOperators = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceBeforeAndAfterBinaryOperator;
            res.InsertSpaceAfterKeywordsInControlFlowStatements = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterKeywordsInControlFlow;
            res.InsertSpaceAfterFunctionKeywordForAnonymousFunctions = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterFunctionKeywordForAnonymousFunctions;
            res.InsertSpaceAfterOpeningAndBeforeClosingNonemptyParenthesis = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterOpeningAndBeforeClosingNonEmptyParens;

            res.PlaceOpenBraceOnNewLineForFunctions = NodejsPackage.Instance.FormattingBracesOptionsPage.BraceOnNewLineForFunctions;
            res.PlaceOpenBraceOnNewLineForControlBlocks = NodejsPackage.Instance.FormattingBracesOptionsPage.BraceOnNewLineForControlBlocks;

            return res;
        }

        private void FormatSelection() {
            string path = GetFilePath();
            if (path != null) {
                ApplyEdits(
                    _textView.TextBuffer,
                    JavaScriptFormattingService.Instance.GetFormattingEditsForRange(
                        path,
                        _textView.Selection.Start.Position,
                        _textView.Selection.End.Position,
                        CreateFormatOptions()
                    )
                );
            }
        }

        private void FormatDocument() {
            string path;
            path = GetFilePath();
            if (path != null) {
                ApplyEdits(
                    _textView.TextBuffer,
                    JavaScriptFormattingService.Instance.GetFormattingEditsForDocument(
                        path,
                        0,
                        _textView.TextBuffer.CurrentSnapshot.Length,
                        CreateFormatOptions()
                    )
                );
            }
        }

        private void FormatAfterTyping(char ch) {
            if (ShouldFormatOnCharacter(ch)) {
                string path = GetFilePath();
                if (path != null) {
                    ApplyEdits(
                        _textView.TextBuffer,
                        JavaScriptFormattingService.Instance.GetFormattingEditsAfterKeystroke(
                            path,
                            _textView.Caret.Position.BufferPosition.Position,
                            ch.ToString(),
                            CreateFormatOptions()
                        )
                    );
                }
            }
        }

        private bool ShouldFormatOnCharacter(char ch) {
            switch (ch) {
                case '}':
                    return NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnCloseBrace;
                case ';':
                    return NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnSemiColon;
                case '\n':
                    return NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnEnter;
            }
            return false;
        }

        private void FormatAfterPaste(ITextSnapshot curVersion) {
            if (NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnPaste) {
                // calculate the range for the paste...
                var afterVersion = _textView.TextBuffer.CurrentSnapshot;
                int start = afterVersion.Length, end = 0;
                for (var version = curVersion.Version;
                    version != afterVersion.Version;
                    version = version.Next) {
                    foreach (var change in version.Changes) {
                        int oldStart = version.CreateTrackingPoint(
                            change.OldPosition,
                            PointTrackingMode.Negative
                        ).GetPosition(afterVersion.Version);

                        start = Math.Min(oldStart, start);

                        int newEnd = version.Next.CreateTrackingPoint(
                            change.NewSpan.End,
                            PointTrackingMode.Positive
                        ).GetPosition(afterVersion.Version);

                        end = Math.Max(newEnd, end);
                    }
                }

                if (start < end) {
                    // then format it
                    string path = GetFilePath();
                    if (path != null) {
                        ApplyEdits(
                            _textView.TextBuffer,
                            JavaScriptFormattingService.Instance.GetFormattingEditsOnPaste(
                                path,
                                start,
                                end,
                                CreateFormatOptions()
                            )
                        );
                    }
                }
            }
        }

        private string GetFilePath() {
            return _textView.TextBuffer.GetFilePath();
        }

        internal static void ApplyEdits(ITextBuffer buffer, TextEdit[] edits) {
            using (var edit = buffer.CreateEdit()) {
                foreach (var toApply in edits) {
                    edit.Replace(
                        Span.FromBounds(toApply.MinChar, toApply.LimChar),
                        toApply.Text
                    );
                }
                edit.Apply();
            }
        }

        #endregion

    }
}
