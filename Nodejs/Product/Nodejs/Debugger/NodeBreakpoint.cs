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
    sealed class NodeBreakpoint {
        private readonly Dictionary<int, NodeBreakpointBinding> _bindings = new Dictionary<int, NodeBreakpointBinding>();
        private readonly BreakOn _breakOn;
        private readonly string _condition;
        private readonly bool _enabled;
        private readonly FilePosition _position;
        private readonly NodeDebugger _process;
        private readonly FilePosition _target;
        private bool _deleted;

        public NodeBreakpoint(NodeDebugger process, FilePosition position, bool enabled, BreakOn breakOn, string condition)
            : this(process, position, position, enabled, breakOn, condition) {
        }

        public NodeBreakpoint(NodeDebugger process, FilePosition target, FilePosition position, bool enabled, BreakOn breakOn, string condition) {
            _process = process;
            _target = target;
            _position = position;
            _enabled = enabled;
            _breakOn = breakOn;
            _condition = condition;
        }

        public NodeDebugger Process {
            get { return _process; }
        }

        /// <summary>
        /// The file name, line and column where the breakpoint was requested to be set.
        /// If source maps are in use this can be different than Position.
        /// </summary>
        public FilePosition Target {
            get { return _target; }
        }

        /// <summary>
        /// The filename, line and column where the breakpoint is set. If source maps are in use
        /// then this is position in the actual JavaScript file.
        /// </summary>
        public FilePosition Position {
            get { return _position; }
        }

        public bool Enabled {
            get { return _enabled; }
        }

        public bool Deleted {
            get { return _deleted; }
            set { _deleted = value;  }
        }

        public BreakOn BreakOn {
            get { return _breakOn; }
        }

        public string Condition {
            get { return _condition; }
        }

        public bool HasPredicate {
            get { return (!string.IsNullOrEmpty(_condition) || NodeBreakpointBinding.GetEngineIgnoreCount(_breakOn, 0) > 0); }
        }

        /// <summary>
        /// Requests the remote process enable the break point.  An event will be raised on the process
        /// when the break point is received.
        /// </summary>
        public Task<NodeBreakpointBinding> BindAsync() {
            return _process.BindBreakpointAsync(this);
        }

        internal NodeBreakpointBinding CreateBinding(FilePosition target, FilePosition position, int breakpointId, int? scriptId, bool fullyBound) {
            var binding = new NodeBreakpointBinding(this, target, position, breakpointId, scriptId, fullyBound);
            _bindings[breakpointId] = binding;
            return binding;
        }

        internal void RemoveBinding(NodeBreakpointBinding binding) {
            _bindings.Remove(binding.BreakpointId);
        }

        internal IEnumerable<NodeBreakpointBinding> GetBindings() {
            return _bindings.Values.ToArray();
        }
    }
}