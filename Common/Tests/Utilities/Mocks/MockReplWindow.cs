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
using System.Text;
using System.Threading.Tasks;
#if NTVS_FEATURE_INTERACTIVEWINDOW
using Microsoft.NodejsTools.Repl;
#else
using Microsoft.VisualStudio.Repl;
#endif
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace TestUtilities.Mocks {
    public class MockReplWindow : IReplWindow {
        private readonly StringBuilder _output = new StringBuilder();
        private readonly StringBuilder _error = new StringBuilder();
        private readonly IReplEvaluator _eval;
        private readonly MockTextView _view;
        private readonly string _contentType;

        public MockReplWindow(IReplEvaluator eval, string contentType = "Python") {            
            _eval = eval;
            _contentType = contentType;
            _view = new MockTextView(new MockTextBuffer(String.Empty, "text"));
            _eval.Initialize(this);
        }

        public bool ShowAnsiCodes { get; set; }

        public Task<ExecutionResult> Execute(string text) {
            _view.BufferGraph.AddBuffer(
                new MockTextBuffer(text, contentType: _contentType)
            );
            return _eval.ExecuteText(text);
        }

        public string Output {
            get {
                return _output.ToString();
            }
        }

        public string Error {
            get {
                return _error.ToString();
            }
        }

        #region IReplWindow Members

        public IWpfTextView TextView {
            get { return _view; }
        }

        public ITextBuffer CurrentLanguageBuffer {
            get { return _view.TextBuffer; }
        }

        public IReplEvaluator Evaluator {
            get { return _eval; }
        }

        public string Title {
            get { return "Mock Repl Window";  }
        }

        public void ClearScreen() {
            _output.Clear();
            _error.Clear();
        }

        public void ClearHistory() {
            throw new NotImplementedException();
        }

        public void Focus() {
            throw new NotImplementedException();
        }

        public void Cancel() {            
            throw new NotImplementedException();
        }

        public void InsertCode(string text) {
            throw new NotImplementedException();
        }

        public void Submit(IEnumerable<string> inputs) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<ExecutionResult> Reset() {
            return _eval.Reset();            
        }

        public void AbortCommand() {
            _eval.AbortCommand();
        }

        public Task<ExecutionResult> ExecuteCommand(string command) {
            var tcs = new TaskCompletionSource<ExecutionResult>();
            tcs.SetException(new NotImplementedException());
            return tcs.Task;
        }

        public void WriteLine(string text) {
            if (!ShowAnsiCodes && text.IndexOf('\x1b') != -1) {
                AppendEscapedText(text);
            } else {
                _output.AppendLine(text);
            }
        }

        private void AppendEscapedText(string text) {
            // http://en.wikipedia.org/wiki/ANSI_escape_code
            // process any ansi color sequences...

            int escape = text.IndexOf('\x1b');
            int start = 0;
            do {
                if (escape != start) {
                    // add unescaped text
                    _output.Append(text.Substring(start, escape - start));
                }

                // process the escape sequence                
                if (escape < text.Length - 1 && text[escape + 1] == '[') {
                    // We have the Control Sequence Introducer (CSI) - ESC [

                    int? value = 0;
                    for (int i = escape + 2; i < text.Length; i++) { // skip esc + [
                        if (text[i] >= '0' && text[i] <= '9') {
                            // continue parsing the integer...
                            if (value == null) {
                                value = 0;
                            }
                            value = 10 * value.Value + (text[i] - '0');
                        } else if (text[i] == ';') {
                            if (value != null) {
                                value = null;
                            } else {
                                // CSI ; - invalid or CSI ### ;;, both invalid
                                break;
                            }
                        } else if (text[i] == 'm') {
                            // parsed a valid code
                            start = i + 1;
                        } else {
                            // unknown char, invalid escape
                            break;
                        }
                    }

                    escape = text.IndexOf('\x1b', escape + 1);
                }// else not an escape sequence, process as text

            } while (escape != -1);
            if (start != text.Length - 1) {
                _output.Append(text.Substring(start));
            }
        }

        public void WriteOutput(object value) {
            _output.Append(value.ToString());
        }

        public void WriteError(object value) {
            _error.Append(value);
        }

        public string ReadStandardInput() {
            throw new NotImplementedException();
        }

        public void SetOptionValue(ReplOptions option, object value) {
        }

        public object GetOptionValue(ReplOptions option) {
            return null;
        }

        public event Action ReadyForInput {
            add { }
            remove { }
        }

        #endregion
    }
}
