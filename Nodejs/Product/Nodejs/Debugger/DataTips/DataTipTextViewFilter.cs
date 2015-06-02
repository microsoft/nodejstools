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
using Microsoft.NodejsTools.Intellisense;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.NodejsTools.Debugger.DataTips {
    internal sealed class DataTipTextViewFilter : IOleCommandTarget, IVsTextViewFilter {
        private readonly IVsDebugger _debugger;
        private readonly IVsTextLines _vsTextLines;
        private readonly IWpfTextView _wpfTextView;
        private readonly IOleCommandTarget _next;

        public DataTipTextViewFilter(System.IServiceProvider serviceProvider, IVsTextView vsTextView) {
            _debugger = (IVsDebugger)NodejsPackage.GetGlobalService(typeof(IVsDebugger));
            vsTextView.GetBuffer(out _vsTextLines);

            var editorAdaptersFactory = serviceProvider.GetComponentModel().GetService<IVsEditorAdaptersFactoryService>();
            _wpfTextView = editorAdaptersFactory.GetWpfTextView(vsTextView);

            ErrorHandler.ThrowOnFailure(vsTextView.AddCommandFilter(this, out _next));
        }

        #region IOleCommandTarget Members

        /// <summary>
        /// Called from VS when we should handle a command or pass it on.
        /// </summary>
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            return _next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        /// <summary>
        /// Called from VS to see what commands we support.  
        /// </summary>
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            return _next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #endregion

        // Assume that all function calls have side effects, and nothing else does.
        private class SideEffectsDetectingVisitor : AstVisitor {
            public bool HasSideEffects { get; private set; }

            public override void PostWalk(CallNode node) {
                base.PostWalk(node);
                if (!node.InBrackets) {
                    HasSideEffects = true;
                }
            }
        }

        internal static TextSpan? GetDataTipSpan(IWpfTextView wpfTextView, TextSpan selection) {
            // Adjust the span to expression boundaries.
            var snapshot = wpfTextView.TextSnapshot;
            var start = LineAndColumnNumberToSnapshotPoint(snapshot, selection.iStartLine, selection.iStartIndex);
            var end = LineAndColumnNumberToSnapshotPoint(snapshot, selection.iEndLine, selection.iEndIndex);

            // If this is a zero-length span (which it usually is, unless there's selection), adjust it
            // to cover one char to the right, since an empty span at the beginning of the expression does
            // not count as belonging to that expression;
            if (start == end && start.Position != snapshot.Length) {
                end += 1;
            }

            var snapshotSpan = new SnapshotSpan(start, end);
            var trackingSpan = snapshot.CreateTrackingSpan(snapshotSpan.Span, SpanTrackingMode.EdgeExclusive);
            var rep = new ReverseExpressionParser(snapshot, wpfTextView.TextBuffer, trackingSpan);
            var exprSpan = rep.GetExpressionRange(forCompletion: false);
            if (exprSpan == null) {
                return null;
            }

            // Check whether this is an expression with side effects - if it does, we don't want to show a data tip for it.
            string text = exprSpan.Value.GetText();
            var ast = new JSParser(text).Parse(new CodeSettings());

            var sideEffectsDetectingVisitor = new SideEffectsDetectingVisitor();
            ast.Walk(sideEffectsDetectingVisitor);
            if (sideEffectsDetectingVisitor.HasSideEffects) {
                return null;
            }

            TextSpan dataTipSpan;
            SnapshotPointToLineAndColumnNumber(exprSpan.Value.Start, out dataTipSpan.iStartLine, out dataTipSpan.iStartIndex);
            SnapshotPointToLineAndColumnNumber(exprSpan.Value.End, out dataTipSpan.iEndLine, out dataTipSpan.iEndIndex);
            return dataTipSpan;
        }

        public int GetDataTipText(TextSpan[] pSpan, out string pbstrText) {
            pbstrText = null;
            if (!_wpfTextView.TextBuffer.ContentType.IsOfType(NodejsConstants.Nodejs)) {
                return VSConstants.E_NOTIMPL;
            }
            if (pSpan.Length != 1) {
                throw new ArgumentException("Array parameter should contain exactly one TextSpan", "pSpan");
            }

            var dataTipSpan = GetDataTipSpan(_wpfTextView, pSpan[0]);
            if (dataTipSpan == null) {
                return VSConstants.E_FAIL;
            }

            pSpan[0] = dataTipSpan.Value;
            return _debugger.GetDataTipValue(_vsTextLines, pSpan, null, out pbstrText);
        }

        public int GetPairExtents(int iLine, int iIndex, TextSpan[] pSpan) {
            return VSConstants.E_NOTIMPL;
        }

        public int GetWordExtent(int iLine, int iIndex, uint dwFlags, TextSpan[] pSpan) {
            return VSConstants.E_NOTIMPL;
        }

        private static SnapshotPoint LineAndColumnNumberToSnapshotPoint(ITextSnapshot snapshot, int lineNumber, int columnNumber) {
            var line = snapshot.GetLineFromLineNumber(lineNumber);
            var snapshotPoint = new SnapshotPoint(snapshot, line.Start + columnNumber);
            return snapshotPoint;
        }

        private static void SnapshotPointToLineAndColumnNumber(SnapshotPoint snapshotPoint, out int lineNumber, out int columnNumber) {
            var line = snapshotPoint.GetContainingLine();
            lineNumber = line.LineNumber;
            columnNumber = snapshotPoint.Position - line.Start.Position;
        }
    }
}
