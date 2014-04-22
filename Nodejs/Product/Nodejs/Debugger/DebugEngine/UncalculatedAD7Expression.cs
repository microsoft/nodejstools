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
using System.Threading;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

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
        //
        // This is primarily used for the immediate window which this engine does not currently support.
        int IDebugExpression2.EvaluateAsync(enum_EVALFLAGS dwFlags, IDebugEventCallback2 pExprCallback) {
            _tokenSource = new CancellationTokenSource();

            _frame.StackFrame.ExecuteTextAsync(_expression, _tokenSource.Token)
                .ContinueWith(p => {
                    try {
                        if (p.IsFaulted || p.IsCanceled) {
                            return;
                        }

                        _tokenSource.Token.ThrowIfCancellationRequested();

                        _frame.Engine.Send(
                            new AD7ExpressionEvaluationCompleteEvent(this, new AD7Property(_frame, p.Result)),
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
                result = _frame.StackFrame.ExecuteTextAsync(_expression, tokenSource.Token)
                    .WaitAsync(timeout, tokenSource.Token).Result;
                ppResult = new AD7Property(_frame, result);
            } catch (AggregateException ae) {
                Exception baseException = ae.GetBaseException();
                if (baseException is DebuggerCommandException) {
                    return VSConstants.E_FAIL;
                }

                throw;
            }

            ppResult = new AD7Property(_frame, result);

            return VSConstants.S_OK;
        }

        #endregion
    }
}