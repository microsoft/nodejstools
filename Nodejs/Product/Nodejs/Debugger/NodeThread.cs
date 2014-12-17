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

namespace Microsoft.NodejsTools.Debugger {
    class NodeThread {
        private readonly int _identity;
        private readonly NodeDebugger _process;
        private readonly bool _isWorkerThread;
        private string _name;
        private IList<NodeStackFrame> _frames;

        internal NodeThread(NodeDebugger process, int identity, bool isWorkerThread) {
            _process = process;
            _identity = identity;
            _isWorkerThread = isWorkerThread;
            _name = "main thread";
        }

        public void StepInto() {
            _process.SendStepInto(_identity);
        }

        public void StepOver() {
            _process.SendStepOver(_identity);
        }

        public void StepOut() {
            _process.SendStepOut(_identity);
        }

        public void Resume() {
            _process.SendResumeThread(_identity);
        }

        public bool IsWorkerThread {
            get {
                return _isWorkerThread;
            }
        }

        internal void ClearSteppingState() {
            _process.SendClearStepping(_identity);
        }

        public IList<NodeStackFrame> Frames {
            get {
                return _frames;
            }
            set {
                _frames = value;
            }
        }

        public int CallstackDepth {
            get {
                return _frames != null ? _frames.Count : 0;
            }
        }

        public NodeStackFrame TopStackFrame {
            get {
                return _frames != null && _frames.Count > 0 ? _frames[0] : null;
            }
        }

        public NodeDebugger Process {
            get {
                return _process;
            }
        }

        public string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }

        internal int Id {
            get {
                return _identity;
            }
        }
    }
}
