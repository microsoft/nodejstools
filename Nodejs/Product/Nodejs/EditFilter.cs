//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Editor.Core;
using Microsoft.NodejsTools.Formatting;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.IncrementalSearch;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Navigation;

namespace Microsoft.NodejsTools {
    internal sealed class EditFilter : IOleCommandTarget {
        private readonly ITextView _textView;
        private readonly System.IServiceProvider _serviceProvider;
        private readonly IIntellisenseSessionStack _intellisenseStack;
        private readonly IComponentModel _compModel;
        private readonly IIncrementalSearch _incSearch;
        private readonly IEditorOptions _editorOptions;
        private IOleCommandTarget _next;
        private ICompletionSession _activeSession;

        public EditFilter(System.IServiceProvider serviceProvider, ITextView textView, IEditorOperations editorOps, IEditorOptions editorOptions, IIntellisenseSessionStack intellisenseStack, IComponentModel compModel) {
            _serviceProvider = serviceProvider;
            _textView = textView;
            _intellisenseStack = intellisenseStack;
            _compModel = compModel;
            var agg = _compModel.GetService<IClassifierAggregatorService>();
            _incSearch = _compModel.GetService<IIncrementalSearchFactoryService>().GetIncrementalSearch(_textView);
            _editorOptions = editorOptions;
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
                            SnapshotPoint start, end;
                            var startEndFound = GetStartAndEndOfCurrentLine(out start, out end);

                            hr = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                            if (startEndFound) {
                                FormatOnEnter(start, end);
                            }
                            return hr;
                        }
                        return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.TYPECHAR:
                        if (!_incSearch.IsActive) {
                            var ch = (char)(ushort)System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant(pvaIn);
                            int res = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                            switch (ch) {
                                case '}':
                                case ';':
                                    FormatAfterTyping(ch);
                                    break;
                            }

                            if (_activeSession != null && !_activeSession.IsDismissed) {
                                _activeSession.Filter();
                            }

                            return res;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.PASTE:
                        return Paste(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut, out hr);

                    case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        if (_textView.CommentOrUncommentBlock(comment: true)) {
                            return VSConstants.S_OK;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        if (_textView.CommentOrUncommentBlock(comment: false)) {
                            return VSConstants.S_OK;
                        }
                        break;
                }
            } else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                switch ((VSConstants.VSStd97CmdID)nCmdID) {
                    case VSConstants.VSStd97CmdID.Paste:
                        return Paste(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut, out hr);
                }
            }

            return _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private int Paste(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut, out int hr) {
            var insertionPoint = _textView.BufferGraph.MapDownToInsertionPoint(
                _textView.Caret.Position.BufferPosition,
                PointTrackingMode.Negative,
                x => x.ContentType.IsOfType(NodejsConstants.Nodejs)
            );

            ITextSnapshot curVersion = null;
            if (insertionPoint != null) {
                curVersion = insertionPoint.Value.Snapshot;
            }
            hr = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            if (insertionPoint != null && ErrorHandler.Succeeded(hr)) {
                FormatAfterPaste(curVersion);
            }
            return hr;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                for (int i = 0; i < cCmds; i++) {
                    switch ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd97CmdID.GotoDefn:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                        case VSConstants.VSStd97CmdID.FindReferences:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
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

                        case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }
            return _next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #region Formatting support

        private FormattingOptions CreateFormattingOptions(){
            return CreateFormattingOptions(_editorOptions, _textView.TextSnapshot);
        }

        internal static FormattingOptions CreateFormattingOptions(IEditorOptions editorOptions, ITextSnapshot snapshot) {
            FormattingOptions res = new FormattingOptions();
            if (editorOptions.IsConvertTabsToSpacesEnabled()) {
                res.SpacesPerIndent = editorOptions.GetIndentSize();
            } else {
                res.SpacesPerIndent = null;
            }

            res.NewLine = VsExtensions.GetNewLineText(snapshot);

            res.SpaceAfterComma = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterComma;
            res.SpaceAfterSemiColonInFor = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterSemicolonInFor;
            res.SpaceBeforeAndAfterBinaryOperator = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceBeforeAndAfterBinaryOperator;
            res.SpaceAfterKeywordsInControlFlowStatements = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterKeywordsInControlFlow;
            res.SpaceAfterFunctionInAnonymousFunctions = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterFunctionKeywordForAnonymousFunctions;
            res.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterOpeningAndBeforeClosingNonEmptyParens;

            res.OpenBracesOnNewLineForFunctions = NodejsPackage.Instance.FormattingBracesOptionsPage.BraceOnNewLineForFunctions;
            res.OpenBracesOnNewLineForControl = NodejsPackage.Instance.FormattingBracesOptionsPage.BraceOnNewLineForControlBlocks;

            return res;
        }

        private void FormatSelection() {
            var insertionPoint = _textView.BufferGraph.MapDownToInsertionPoint(
                _textView.Selection.Start.Position,
                PointTrackingMode.Negative,
                x => x.ContentType.IsOfType(NodejsConstants.Nodejs)
            );
            var endPoint = _textView.BufferGraph.MapDownToInsertionPoint(
                _textView.Selection.End.Position,
                PointTrackingMode.Negative,
                x => x.ContentType.IsOfType(NodejsConstants.Nodejs)
            );

            if (insertionPoint != null && endPoint != null) {
                var buffer = insertionPoint.Value.Snapshot.TextBuffer;

                ApplyEdits(
                    buffer,
                    Formatter.GetEditsForRange(
                        buffer.CurrentSnapshot.GetText(),
                        insertionPoint.Value.Position,
                        endPoint.Value.Position,
                        CreateFormattingOptions()
                    )
                );
            }
        }

        private void FormatDocument() {
            foreach (var buffer in _textView.BufferGraph.GetTextBuffers(x => x.ContentType.IsOfType(NodejsConstants.Nodejs))) {
                ApplyEdits(
                    buffer,
                    Formatter.GetEditsForDocument(
                        buffer.CurrentSnapshot.GetText(),
                        CreateFormattingOptions()
                    )
                );
            }
        }

        private bool GetStartAndEndOfCurrentLine(out SnapshotPoint start, out SnapshotPoint end) {
            var insertionPoint = _textView.BufferGraph.MapDownToInsertionPoint(
                    _textView.Caret.Position.BufferPosition,
                    PointTrackingMode.Negative,
                    x => x.ContentType.IsOfType(NodejsConstants.Nodejs)
                );

            if (insertionPoint != null) {
                var buffer = insertionPoint.Value.Snapshot.TextBuffer;
                var line = insertionPoint.Value.GetContainingLine();

                start = buffer.CurrentSnapshot.GetLineFromLineNumber(line.LineNumber).Start;
                end = line.End;

                return true;
            }

            start = new SnapshotPoint();
            end = new SnapshotPoint();
            return false;
        }

        private void FormatOnEnter(SnapshotPoint start, SnapshotPoint end) {
            if (NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnEnter) {
                var insertionPoint = _textView.BufferGraph.MapDownToInsertionPoint(
                    _textView.Caret.Position.BufferPosition,
                    PointTrackingMode.Negative,
                    x => x.ContentType.IsOfType(NodejsConstants.Nodejs)
                );

                if (insertionPoint != null) {
                    var buffer = insertionPoint.Value.Snapshot.TextBuffer;
                    var line = insertionPoint.Value.GetContainingLine();

                    if (line.LineNumber > 0) {
                        SnapshotSpan commentSpan;
                        if (insertionPoint.Value.IsMultilineComment(out commentSpan)) {
                            _textView.FormatMultilineComment(commentSpan.Start, insertionPoint.Value);
                        } else {
                            ApplyEdits(
                                buffer,
                                Formatter.GetEditsAfterEnter(
                                    buffer.CurrentSnapshot.GetText(),
                                    start.TranslateTo(buffer.CurrentSnapshot, PointTrackingMode.Negative),
                                    end.TranslateTo(buffer.CurrentSnapshot, PointTrackingMode.Positive),
                                    CreateFormattingOptions()
                                )
                            );
                        }
                    }
                }
            }
        }

        private void FormatAfterTyping(char ch) {
            if (ShouldFormatOnCharacter(ch)) {

                var insertionPoint = _textView.BufferGraph.MapDownToInsertionPoint(
                    _textView.Caret.Position.BufferPosition,
                    PointTrackingMode.Negative,
                    x => x.ContentType.IsOfType(NodejsConstants.Nodejs)
                );

                if (insertionPoint != null) {
                    var buffer = insertionPoint.Value.Snapshot.TextBuffer;

                    ApplyEdits(
                        buffer,
                        Formatter.GetEditsAfterKeystroke(
                            buffer.CurrentSnapshot.GetText(),
                            insertionPoint.Value.Position,
                            ch,
                            CreateFormattingOptions()
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
            }
            return false;
        }

        private void FormatAfterPaste(ITextSnapshot curVersion) {
            if (NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnPaste) {
                // calculate the range for the paste...
                var afterVersion = curVersion.TextBuffer.CurrentSnapshot;
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
                    ApplyEdits(
                        curVersion.TextBuffer,
                        Formatter.GetEditsForRange(
                            curVersion.TextBuffer.CurrentSnapshot.GetText(),
                            start,
                            end,
                            CreateFormattingOptions()
                        )
                    );
                }
            }
        }

        private string GetFilePath() {
            return _textView.TextBuffer.GetFilePath();
        }

        internal static void ApplyEdits(ITextBuffer buffer, Edit[] edits) {
            using (var edit = buffer.CreateEdit()) {
                foreach (var toApply in edits) {
                    edit.Replace(
                        new Span(toApply.Start, toApply.Length),
                        toApply.Text
                    );
                }
                edit.Apply();
            }
        }

        #endregion

    }
}
