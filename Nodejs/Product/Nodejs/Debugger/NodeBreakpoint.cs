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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.NodejsTools.SourceMapping;
using Microsoft.VisualStudio.Debugger.Symbols;
using TypeScriptSourceMapReader;

namespace Microsoft.NodejsTools.Debugger {
    sealed class NodeBreakpoint {
        private readonly Dictionary<int, NodeBreakpointBinding> _bindings = new Dictionary<int, NodeBreakpointBinding>();
        private readonly BreakOn _breakOn;
        private readonly string _condition;
        private readonly bool _enabled;
        private readonly NodeDebugger _process;
        private readonly FilePosition _target;
        private bool _deleted;

        public NodeBreakpoint(NodeDebugger process, FilePosition target, bool enabled, BreakOn breakOn, string condition) {
            _process = process;
            _target = target;
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
        /// Gets the position in the target JavaScript file using the provided SourceMapper.
        /// 
        /// This translates the breakpoint from the location where the user set it (possibly
        /// a TypeScript file) into the location where it lives in JavaScript code.
        /// </summary>
        public FilePosition GetPosition(string javaScriptFileName) {
            
            // Checks whether source map is available
            string sourceMapFilename = FindSourceMapFile(javaScriptFileName);
            
            if (!string.IsNullOrEmpty(sourceMapFilename)) {

                SourceMapReader sourceMapReader = new SourceMapReader();
                DecodedSourceMap decodedSourceMap = sourceMapReader.LoadSourceMap(javaScriptFileName, sourceMapFilename);

                // The NodeJS debugger is 1-based index while the SourceMapReader is 0-based index
                DkmTextSpan jsSpan = decodedSourceMap.MapTsSourcePosition(Target.FileName, new DkmTextSpan(Target.Line + 1, Target.Line + 1, Target.Column + 1, Target.Column + 1));
                int javaScriptLine = jsSpan.StartLine - 1;
                int javaScriptColumn = jsSpan.StartColumn - 1;
                if (javaScriptLine >= 0 && javaScriptColumn >= 0) {
                    return new FilePosition(javaScriptFileName,  javaScriptLine, javaScriptColumn);
                }
            }           

             return Target;
        }

        private string FindSourceMapFile(string jsFileName) {
            string sourceMapFilename = null;

            if (File.Exists(jsFileName)) {
                int markerStart;
                string[] contents = File.ReadAllLines(jsFileName);
                const string marker = "# sourceMappingURL=";                
                string markerLine = contents.Reverse().FirstOrDefault(x => x.IndexOf(marker, StringComparison.Ordinal) != -1);
                if (markerLine != null && (markerStart = markerLine.IndexOf(marker, StringComparison.Ordinal)) != -1) {
                    sourceMapFilename = markerLine.Substring(markerStart + marker.Length).Trim();

                    try {
                        if (!File.Exists(sourceMapFilename)) {
                            sourceMapFilename = Path.Combine(Path.GetDirectoryName(jsFileName) ?? string.Empty, Path.GetFileName(sourceMapFilename));
                        }
                    } catch (ArgumentException) {
                    } catch (PathTooLongException) {
                    }
                }
            }
            return sourceMapFilename;
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