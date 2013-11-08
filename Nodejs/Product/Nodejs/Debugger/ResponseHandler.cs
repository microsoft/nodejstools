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
using System.Diagnostics;
using System.Threading;

namespace Microsoft.NodejsTools.Debugger {
    class ResponseHandler {
        private readonly AutoResetEvent _completedEvent;
        private readonly Action<Dictionary<string, object>> _failureHandler;
        private readonly Func<bool> _shortCircuitPredicate;
        private readonly Action<Dictionary<string, object>> _successHandler;
        private int? _timeout;

        public ResponseHandler(
            Action<Dictionary<string, object>> successHandler = null,
            Action<Dictionary<string, object>> failureHandler = null,
            int? timeout = null,
            Func<bool> shortCircuitPredicate = null
            ) {
            Debug.Assert(
                successHandler != null || failureHandler != null || timeout != null,
                "At least success handler, failure handler or timeout should be non-null");
            _successHandler = successHandler;
            _failureHandler = failureHandler;
            _timeout = timeout;
            _shortCircuitPredicate = shortCircuitPredicate;
            if (timeout.HasValue) {
                _completedEvent = new AutoResetEvent(false);
            }
        }

        public bool Wait() {
            // Handle asynchronous (no wait)
            if (_completedEvent == null) {
                Debug.Assert((_timeout == null), "No completedEvent implies no timeout");
                Debug.Assert((_shortCircuitPredicate == null), "No completedEvent implies no shortCircuitPredicate");
                return true;
            }
            Debug.Assert((_timeout != null) && _timeout > 0, "completedEvent implies timeout");

            // Handle synchronous without short circuiting
            int timeout = _timeout.Value;
            if (_shortCircuitPredicate == null) {
                return _completedEvent.WaitOne(timeout);
            }

            // Handle synchronous with short circuiting
            int interval = Math.Max(1, timeout/10);
            while (!_shortCircuitPredicate()) {
                if (_completedEvent.WaitOne(Math.Min(timeout, interval))) {
                    return true;
                }

                timeout -= interval;
                if (timeout <= 0) {
                    break;
                }
            }
            return false;
        }

        public void HandleResponse(Dictionary<string, object> json) {
            if ((bool) json["success"]) {
                if (_successHandler != null) {
                    _successHandler(json);
                }
            } else {
                if (_failureHandler != null) {
                    _failureHandler(json);
                }
            }

            if (_completedEvent != null) {
                _completedEvent.Set();
            }
        }
    }
}