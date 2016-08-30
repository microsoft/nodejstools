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
using System.Threading;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Debugger.DebugEngine {
    // This class represents a succesfully parsed expression to the debugger. 
    // It is returned as a result of a successful call to IDebugExpressionContext2.ParseText
    // It allows the debugger to obtain the values of an expression in the debuggee. 
    class UncalculatedAD7Expression : IDebugExpression2 {
        private readonly string _expression;
        private readonly AD7StackFrame _frame;
        private CancellationTokenSource _tokenSource;

        public UncalculatedAD7Expression(AD7StackFrame frame, string expression) {
            _frame = frame;
            _expression = expression;
        }

        #region IDebugExpression2 Members

        // This method cancels asynchronous expression evaluation as started by a call to the IDebugExpression2::EvaluateAsync method.
        int IDebugExpression2.Abort() {
            if (_tokenSource == null) {
                return VSConstants.E_FAIL;
            }

            _tokenSource.Cancel();

            return VSConstants.S_OK;
        }

        // This method evaluates the expression asynchronously.
        // This method should return immediately after it has started the expression evaluation. 
        // When the expression is successfully evaluated, an IDebugExpressionEvaluationCompleteEvent2 
        // must be sent to the IDebugEventCallback2 event callback
        int IDebugExpression2.EvaluateAsync(enum_EVALFLAGS dwFlags, IDebugEventCallback2 pExprCallback) {
            _tokenSource = new CancellationTokenSource();

            _frame.StackFrame.ExecuteTextAsync(_expression, _tokenSource.Token)
                .ContinueWith(p => {
                    try {
                        IDebugProperty2 property;
                        if (p.Exception != null && p.Exception.InnerException != null) {
                            property = new AD7EvalErrorProperty(p.Exception.InnerException.Message);
                        } else if (p.IsCanceled) {
                            property = new AD7EvalErrorProperty("Evaluation canceled");
                        } else if (p.IsFaulted || p.Result == null) {
                            property = new AD7EvalErrorProperty("Error");
                        } else {
                            property = new AD7Property(_frame, p.Result);
                        }

                        _tokenSource.Token.ThrowIfCancellationRequested();
                        _frame.Engine.Send(
                            new AD7ExpressionEvaluationCompleteEvent(this, property),
                            AD7ExpressionEvaluationCompleteEvent.IID,
                            _frame.Engine,
                            _frame.Thread);
                    } finally {
                        _tokenSource.Dispose();
                        _tokenSource = null;
                    }
                }, _tokenSource.Token);

            return VSConstants.S_OK;
        }

        // This method evaluates the expression synchronously.
        int IDebugExpression2.EvaluateSync(enum_EVALFLAGS dwFlags, uint dwTimeout, IDebugEventCallback2 pExprCallback, out IDebugProperty2 ppResult) {
            TimeSpan timeout = TimeSpan.FromMilliseconds(dwTimeout);
            var tokenSource = new CancellationTokenSource(timeout);
            ppResult = null;

            NodeEvaluationResult result;
            try {
                result = _frame.StackFrame.ExecuteTextAsync(_expression, tokenSource.Token).WaitAsync(timeout, tokenSource.Token).WaitAndUnwrapExceptions();
            } catch (DebuggerCommandException ex) {
                ppResult = new AD7EvalErrorProperty(ex.Message);
                return VSConstants.S_OK;
            } catch (OperationCanceledException) {
                return DebuggerConstants.E_EVALUATE_TIMEOUT;
            }

            if (result == null) {
                return VSConstants.E_FAIL;
            }

            ppResult = new AD7Property(_frame, result);
            return VSConstants.S_OK;
        }

        #endregion
    }
}