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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Debugger {
    class NodeBreakpoint {
        private readonly NodeDebugger _process;
        private readonly string _fileName, _requestedFileName;
        private int _lineNo;
        private readonly int _requestedLineNo;
        private readonly Dictionary<int, NodeBreakpointBinding> _bindings = new Dictionary<int, NodeBreakpointBinding>();
        private readonly bool _enabled;
        private readonly BreakOn _breakOn;
        private readonly string _condition;

        public NodeBreakpoint(
            NodeDebugger process,
            string fileName,
            string requestedFileName,
            int lineNo,
            int requestedLineNo,
            bool enabled,
            BreakOn breakOn,
            string condition
        ) {
            _process = process;
            _fileName = fileName;
            _requestedFileName = requestedFileName;
            _lineNo = lineNo;
            _requestedLineNo = requestedLineNo;
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
        public Task<NodeBreakpointBinding> BindAsync() {
            return _process.BindBreakpointAsync(this);
        }

        /// <summary>
        /// The filename where the breakpoint is set.  If source maps are in use then this
        /// is the actual JavaScript file.
        /// </summary>
        public string FileName {
            get {
                return _fileName;
            }
        }

        /// <summary>
        /// The file name where the breakpoint was requested to be set.  If source maps are in use this can be
        /// different than FileName.
        /// </summary>
        public string RequestedFileName {
            get {
                return _requestedFileName;
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

        public int RequestedLineNo {
            get {
                return _requestedLineNo;
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
