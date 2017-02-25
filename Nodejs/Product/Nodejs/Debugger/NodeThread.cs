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

namespace Microsoft.NodejsTools.Debugger
{
    internal sealed class NodeThread
    {
        private readonly int _identity;
        private readonly NodeDebugger _process;
        private readonly bool _isWorkerThread;

        internal NodeThread(NodeDebugger process, int identity, bool isWorkerThread)
        {
            this._process = process;
            this._identity = identity;
            this._isWorkerThread = isWorkerThread;
            this.Name = "main thread";
        }

        public void StepInto()
        {
            this._process.SendStepInto(this._identity);
        }

        public void StepOver()
        {
            this._process.SendStepOver(this._identity);
        }

        public void StepOut()
        {
            this._process.SendStepOut(this._identity);
        }

        public void Resume()
        {
            this._process.SendResumeThread(this._identity);
        }

        public bool IsWorkerThread
        {
            get
            {
                return this._isWorkerThread;
            }
        }

        internal void ClearSteppingState()
        {
            this._process.SendClearStepping(this._identity);
        }

        public IList<NodeStackFrame> Frames { get; set; }

        public int CallstackDepth
        {
            get
            {
                return this.Frames != null ? this.Frames.Count : 0;
            }
        }

        public NodeStackFrame TopStackFrame
        {
            get
            {
                return this.Frames != null && this.Frames.Count > 0 ? this.Frames[0] : null;
            }
        }

        public NodeDebugger Process
        {
            get
            {
                return this._process;
            }
        }

        public string Name { get; set; }

        internal int Id
        {
            get
            {
                return this._identity;
            }
        }
    }
}
