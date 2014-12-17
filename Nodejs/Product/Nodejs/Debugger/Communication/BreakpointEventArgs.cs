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
using Microsoft.NodejsTools.Debugger.Events;

namespace Microsoft.NodejsTools.Debugger.Communication {
    sealed class BreakpointEventArgs : EventArgs {
        public BreakpointEventArgs(BreakpointEvent breakpointEvent) {
            BreakpointEvent = breakpointEvent;
        }

        public BreakpointEvent BreakpointEvent { get; private set; }
    }
}