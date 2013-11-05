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
using System.Linq;

namespace Microsoft.NodejsTools.Debugger {
    class NodeBreakpoint {
        private readonly NodeDebugger _process;
        private readonly string _fileName;
        private int _lineNo;
        private Dictionary<int, NodeBreakpointBinding> _bindings = new Dictionary<int, NodeBreakpointBinding>();
        private bool _enabled;
        private BreakOn _breakOn;
        private string _condition;

        public NodeBreakpoint(
            NodeDebugger process,
            string fileName,
            int lineNo,
            bool enabled,
            BreakOn breakOn,
            string condition
        ) {
            _process = process;
            _fileName = fileName;
            _lineNo = lineNo;
            _enabled = enabled;
            _breakOn = breakOn;
            _condition = condition;
        }

        public NodeDebugger Process {
            get {
                return _process;
            }
        }

        /// <summary>
        /// Requests the remote process enable the break point.  An event will be raised on the process
        /// when the break point is received.
        /// </summary>
        public void Bind(Action<NodeBreakpointBinding> successHandler = null, Action failureHandler = null) {
            _process.BindBreakpoint(this, successHandler, failureHandler);
        }

        public string FileName {
            get {
                return _fileName;
            }
        }

        public int LineNo {
            get {
                return _lineNo;
            }
            set {
                _lineNo = value;
            }
        }

        public bool Enabled {
            get {
                return _enabled;
            }
        }

        public BreakOn BreakOn {
            get {
                return _breakOn;
            }
        }

        public string Condition {
            get {
                return _condition;
            }
        }

        public bool HasPredicate {
            get {
                return (!string.IsNullOrEmpty(_condition) || NodeBreakpointBinding.GetEngineIgnoreCount(_breakOn, 0) > 0);

            }
        }

        internal NodeBreakpointBinding CreateBinding(int lineNo, int breakpointID, int? scriptID, bool fullyBound) {
            var binding = new NodeBreakpointBinding(this, lineNo, breakpointID, scriptID, fullyBound);
            _bindings[breakpointID] = binding;
            return binding;
        }

        internal void RemoveBinding(NodeBreakpointBinding binding) {
            _bindings.Remove(binding.BreakpointID);
        }

        internal IEnumerable<NodeBreakpointBinding> GetBindings() {
            return _bindings.Values.ToArray();
        }
    }
}
