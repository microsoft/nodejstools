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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools;
using MSXML;

namespace Microsoft.NodejsTools.Intellisense {
    internal sealed class ExpansionClient : IVsExpansionClient {
        internal IServiceProvider serviceProvider;

        private readonly IVsTextLines _lines;
        private readonly IVsExpansion _expansion;
        private readonly IVsTextView _view;
        private readonly ITextView _textView;
        private readonly IVsEditorAdaptersFactoryService _adapterFactory;
        private readonly IServiceProvider _serviceProvider;
        private IVsExpansionSession _session;
        private bool _sessionEnded, _selectEndSpan;
        private ITrackingPoint _selectionStart, _selectionEnd;

        public const string SurroundsWith = "SurroundsWith";
        public const string Expansion = "Expansion";

        public ExpansionClient(ITextView textView, IVsEditorAdaptersFactoryService adapterFactory, IServiceProvider serviceProvider) {
            _textView = textView;
            _serviceProvider = serviceProvider;
            _adapterFactory = adapterFactory;
            _view = adapterFactory.GetViewAdapter(textView);
            _lines = (IVsTextLines)adapterFactory.GetBufferAdapter(textView.TextBuffer);
            _expansion = _lines as IVsExpansion;
            if (_expansion == null) {
                throw new ArgumentException("TextBuffer does not support expansions");
            }
        }

        public bool InSession {
            get {
                return _session != null;
            }
        }

        public int EndExpansion() {
            _session = null;
            _sessionEnded = true;
            _selectionStart = _selectionEnd = null;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Format the content inserted by code snippet.
        /// </summary>
        /// <param name="pBuffer">The text buffer which contains the text to be formatted.</param>
        /// <param name="ts">The span (a pair of beginning and ending positions) of text that is to be formatted.</param>
        public int FormatSpan(IVsTextLines pBuffer, TextSpan[] ts) {
            int startPos = 0, endPos = 0;
            pBuffer.GetPositionOfLineIndex(ts[0].iStartLine, ts[0].iStartIndex, out startPos);
            pBuffer.GetPositionOfLineIndex(ts[0].iEndLine, ts[0].iEndIndex, out endPos);

            Formatting.FormattingOptions options = EditFilter.CreateFormattingOptions(_textView.Options, _textView.TextBuffer.CurrentSnapshot);
            string text = _textView.TextBuffer.CurrentSnapshot.GetText();
            var edits = Formatting.Formatter.GetEditsForRange(text, startPos, endPos, options);
            EditFilter.ApplyEdits(_textView.TextBuffer, edits);

            return VSConstants.S_OK;
        }

        public int InsertNamedExpansion(string pszTitle, string pszPath, TextSpan textSpan) {
            if (_session != null) {
                // if the user starts an expansion session while one is in progress
                // then abort the current expansion session
                _session.EndCurrentExpansion(1);
                _session = null;
            }

            var selection = _textView.Selection;
            var snapshot = selection.Start.Position.Snapshot;

            _selectionStart = snapshot.CreateTrackingPoint(selection.Start.Position, VisualStudio.Text.PointTrackingMode.Positive);
            _selectionEnd = snapshot.CreateTrackingPoint(selection.End.Position, VisualStudio.Text.PointTrackingMode.Negative);
            _selectEndSpan = _sessionEnded = false;

            int hr = _expansion.InsertNamedExpansion(
                pszTitle,
                pszPath,
                textSpan,
                this,
                Guids.NodejsLanguageInfo,
                0,
                out _session
            );

            if (ErrorHandler.Succeeded(hr)) {
                if (_sessionEnded) {
                    _session = null;
                }
            }
            return hr;
        }

        public int GetExpansionFunction(IXMLDOMNode xmlFunctionNode, string bstrFieldName, out IVsExpansionFunction pFunc) {
            pFunc = null;
            return VSConstants.S_OK;
        }

        public int IsValidKind(IVsTextLines pBuffer, TextSpan[] ts, string bstrKind, out int pfIsValidKind) {
            pfIsValidKind = 1;
            return VSConstants.S_OK;
        }

        public int IsValidType(IVsTextLines pBuffer, TextSpan[] ts, string[] rgTypes, int iCountTypes, out int pfIsValidType) {
            pfIsValidType = 1;
            return VSConstants.S_OK;
        }

        public int OnAfterInsertion(IVsExpansionSession pSession) {
            return VSConstants.S_OK;
        }

        public int OnBeforeInsertion(IVsExpansionSession pSession) {
            _session = pSession;
            return VSConstants.S_OK;
        }

        public int OnItemChosen(string pszTitle, string pszPath) {
            int caretLine, caretColumn;
            GetCaretPosition(out caretLine, out caretColumn);

            var textSpan = new TextSpan() { iStartLine = caretLine, iStartIndex = caretColumn, iEndLine = caretLine, iEndIndex = caretColumn };
            return InsertNamedExpansion(pszTitle, pszPath, textSpan);

        }

        public int NextField() {
            return _session.GoToNextExpansionField(0);
        }

        public int PreviousField() {
            return _session.GoToPreviousExpansionField();
        }

        public int EndCurrentExpansion(bool leaveCaret) {
            if (_selectEndSpan) {
                TextSpan[] endSpan = new TextSpan[1];
                if (ErrorHandler.Succeeded(_session.GetEndSpan(endSpan))) {
                    var snapshot = _textView.TextBuffer.CurrentSnapshot;
                    var startLine = snapshot.GetLineFromLineNumber(endSpan[0].iStartLine);
                    var selectionLength = _selectionEnd.GetPosition(_textView.TextBuffer.CurrentSnapshot) - _selectionStart.GetPosition(_textView.TextBuffer.CurrentSnapshot);
                    var span = new Span(startLine.Start + endSpan[0].iStartIndex + selectionLength, 0);
                    _textView.Caret.MoveTo(new SnapshotPoint(snapshot, span.Start));
                    return _session.EndCurrentExpansion(1);
                }
            }
            return _session.EndCurrentExpansion(leaveCaret ? 1 : 0);
        }

        public int PositionCaretForEditing(IVsTextLines pBuffer, TextSpan[] ts) {
            return VSConstants.S_OK;
        }

        private void GetCaretPosition(out int caretLine, out int caretColumn) {
            ErrorHandler.ThrowOnFailure(_view.GetCaretPos(out caretLine, out caretColumn));

            // Handle virtual space
            int lineLength;
            ErrorHandler.ThrowOnFailure(_lines.GetLengthOfLine(caretLine, out lineLength));

            if (caretColumn > lineLength) {
                caretColumn = lineLength;
            }
        }

    }
}