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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.NodejsTools.SourceMapping;

namespace Microsoft.NodejsTools.Debugger
{
    internal sealed class NodeBreakpoint
    {
        private readonly Dictionary<int, NodeBreakpointBinding> _bindings = new Dictionary<int, NodeBreakpointBinding>();
        private readonly BreakOn _breakOn;
        private readonly string _condition;
        private readonly bool _enabled;
        private readonly NodeDebugger _process;
        private readonly FilePosition _target;
        private bool _deleted;

        public NodeBreakpoint(NodeDebugger process, FilePosition target, bool enabled, BreakOn breakOn, string condition)
        {
            this._process = process;
            this._target = target;
            this._enabled = enabled;
            this._breakOn = breakOn;
            this._condition = condition;
        }

        public NodeDebugger Process
        {
            get { return this._process; }
        }

        /// <summary>
        /// The file name, line and column where the breakpoint was requested to be set.
        /// If source maps are in use this can be different than Position.
        /// </summary>
        public FilePosition Target
        {
            get { return this._target; }
        }

        /// <summary>
        /// Gets the position in the target JavaScript file using the provided SourceMapper.
        /// 
        /// This translates the breakpoint from the location where the user set it (possibly
        /// a TypeScript file) into the location where it lives in JavaScript code.
        /// </summary>
        public FilePosition GetPosition(SourceMapper mapper)
        {
            // Checks whether source map is available
            string javaScriptFileName;
            int javaScriptLine;
            int javaScriptColumn;

            if (mapper != null &&
                mapper.MapToJavaScript(this.Target.FileName, this.Target.Line, this.Target.Column, out javaScriptFileName, out javaScriptLine, out javaScriptColumn))
            {
                return new FilePosition(javaScriptFileName, javaScriptLine, javaScriptColumn);
            }

            return this.Target;
        }

        public bool Enabled
        {
            get { return this._enabled; }
        }

        public bool Deleted
        {
            get { return this._deleted; }
            set { this._deleted = value; }
        }

        public BreakOn BreakOn
        {
            get { return this._breakOn; }
        }

        public string Condition
        {
            get { return this._condition; }
        }

        public bool HasPredicate
        {
            get { return (!string.IsNullOrEmpty(this._condition) || NodeBreakpointBinding.GetEngineIgnoreCount(this._breakOn, 0) > 0); }
        }

        /// <summary>
        /// Requests the remote process enable the break point.  An event will be raised on the process
        /// when the break point is received.
        /// </summary>
        public Task<NodeBreakpointBinding> BindAsync()
        {
            return this._process.BindBreakpointAsync(this);
        }

        internal NodeBreakpointBinding CreateBinding(FilePosition target, FilePosition position, int breakpointId, int? scriptId, bool fullyBound)
        {
            var binding = new NodeBreakpointBinding(this, target, position, breakpointId, scriptId, fullyBound);
            this._bindings[breakpointId] = binding;
            return binding;
        }

        internal void RemoveBinding(NodeBreakpointBinding binding)
        {
            this._bindings.Remove(binding.BreakpointId);
        }

        internal IEnumerable<NodeBreakpointBinding> GetBindings()
        {
            return this._bindings.Values.ToArray();
        }
    }
}