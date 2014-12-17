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

namespace Microsoft.NodejsTools.Debugger {
    class BreakpointHitEventArgs : EventArgs {
        private readonly NodeBreakpointBinding _breakpointBinding;
        private readonly NodeThread _thread;

        public BreakpointHitEventArgs(NodeBreakpointBinding breakpoint, NodeThread thread) {
            _breakpointBinding = breakpoint;
            _thread = thread;
        }

        public NodeBreakpointBinding BreakpointBinding {
            get {
                return _breakpointBinding;
            }
        }

        public NodeThread Thread {
            get {
                return _thread;
            }
        }
    }
}
