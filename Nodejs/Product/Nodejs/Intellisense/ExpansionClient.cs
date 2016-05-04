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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.TextManager.Interop;
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
        /// Format the content inserted by a code snippet
        /// </summary>
        public int FormatSpan(IVsTextLines pBuffer, TextSpan[] ts) {
            // Example of a snippet template (taken from forprops.snippet and use ... to represent
            // indent spaces):
            //
            // for (var $property$ in $object$) {
            // ....if ($object$.hasOwnProperty($property$)) {
            // ........$selected$$end$
            // ....}
            // };
            //
            // Example of code in pBuffer (the for loop is inserted by a forprops snippet):
            //
            // var object = { one: 1, two: 2 };
            // for (var property in object) {
            // ....if (object.hasOwnProperty(property)) {
            // ........
            // ....}
            // };
            //
            // Result examples:
            //
            // (1) If indent size = 2:
            //
            // var object = { one: 1, two: 2 };
            // for (var property in object) {
            // ..if (object.hasOwnProperty(property)) {
            // ....
            // ..}
            // };
            //
            // (2) If indent size = 2 and OpenBracesOnNewLineForControl = true:
            //
            // var object = { one: 1, two: 2 };
            // for (var property in object)
            // {
            // ..if (object.hasOwnProperty(property))
            // ..{
            // ....
            // ..}
            // };

            // Algorithm: The idea is to use Formatting.Formatter to format the inserted content.
            // However, Formatting.Formatter does not format lines that only contain spaces. For
            // example, here is how Formatting.Formatter formats the code above:
            //
            // var object = { one: 1, two: 2 };
            // for (var property in object) {
            // ..if (object.hasOwnProperty(property)) {
            // ........
            // ..}
            // };
            //
            // An additional step will be included to ensure such lines are formatted correctly.

            int baseIndentationLevel = GetViewIndentationLevelAtLine(ts[0].iStartLine);

            for (int lineNumber = ts[0].iStartLine; lineNumber <= ts[0].iEndLine; ++lineNumber) {
                string lineContent = _textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber).GetText();

                string indentationString = GetTemplateIndentationString(lineContent);
                if (indentationString != lineContent) { // This means line contains some characters other than spaces
                    continue;
                }

                int newIndentationLevel = baseIndentationLevel + GetTemplateIndentationLevel(indentationString);

                string newIndentation = _textView.Options.IsConvertTabsToSpacesEnabled()
                    ? new string(' ', newIndentationLevel * _textView.Options.GetIndentSize())
                    : new string('\t', newIndentationLevel);

                using (var edit = _textView.TextBuffer.CreateEdit()) {
                    int indendationPos;
                    pBuffer.GetPositionOfLineIndex(lineNumber, 0, out indendationPos);
                    Span bufferIndentationSpan = new Span(indendationPos, indentationString.Length);
                    edit.Replace(bufferIndentationSpan, newIndentation);
                    edit.Apply();
                }
            }

            // Now that we have handled empty lines, use Formatter to format the inserted content.
            int startPos = 0, endPos = 0;
            pBuffer.GetPositionOfLineIndex(ts[0].iStartLine, ts[0].iStartIndex, out startPos);
            pBuffer.GetPositionOfLineIndex(ts[0].iEndLine, ts[0].iEndIndex, out endPos);
            Formatting.FormattingOptions options = EditFilter.CreateFormattingOptions(_textView.Options, _textView.TextBuffer.CurrentSnapshot);
            string text = _textView.TextBuffer.CurrentSnapshot.GetText();
            EditFilter.ApplyEdits(_textView.TextBuffer, Formatting.Formatter.GetEditsForRange(text, startPos, endPos, options));

            return VSConstants.S_OK;
        }

        private static string GetTemplateIndentationString(string lineContent) {
            for (int i = 0; i < lineContent.Length; ++i) {
                if (lineContent[i] != ' ' && lineContent[i] != '\t') {
                    return lineContent.Substring(0, i);
                }
            }
            return lineContent;
        }

        private static int GetTemplateIndentationLevel(string indentationString) {
            if (indentationString.Length == 0) {
                return 0;
            }
            if (indentationString[0] == ' ') {
                return indentationString.Length / 4; // All node.js snippets use 4 space indentation.
            }
            // In case some snippets use tab style indentation
            return indentationString.Length;
        }

        private int GetViewIndentationLevelAtLine(int line) {
            string lineContent = _textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(line).GetText();
            if (_textView.Options.IsConvertTabsToSpacesEnabled()) {
                return CountLeadingCharacters(' ', lineContent) / _textView.Options.GetIndentSize();
            }
            return CountLeadingCharacters('\t', lineContent);
        }

        private static int CountLeadingCharacters(char character, string lineContent) {
            for (int i = 0; i < lineContent.Length; ++i) {
                if (lineContent[i] != character) {
                    return i;
                }
            }
            return lineContent.Length;
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