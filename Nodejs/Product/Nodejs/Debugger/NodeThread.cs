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
    sealed class NodeThread {
        private readonly int _identity;
        private readonly NodeDebugger _process;
        private readonly bool _isWorkerThread;

        internal NodeThread(NodeDebugger process, int identity, bool isWorkerThread) {
            _process = process;
            _identity = identity;
            _isWorkerThread = isWorkerThread;
            Name = "main thread";
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

        public IList<NodeStackFrame> Frames { get; set; }

        public int CallstackDepth {
            get {
                return Frames != null ? Frames.Count : 0;
            }
        }

        public NodeStackFrame TopStackFrame {
            get {
                return Frames != null && Frames.Count > 0 ? Frames[0] : null;
            }
        }

        public NodeDebugger Process {
            get {
                return _process;
            }
        }

        public string Name { get; set; }

        internal int Id {
            get {
                return _identity;
            }
        }
    }
}
