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

namespace Microsoft.NodejsTools.Debugger.Commands {
    sealed class ClearBreakpointCommand : DebuggerCommand {
        private readonly Dictionary<string, object> _arguments;

        public ClearBreakpointCommand(int id, int breakpointId) : base(id, "clearbreakpoint") {
            _arguments = new Dictionary<string, object> {
                { "breakpoint", breakpointId }
            };
        }

        protected override IDictionary<string, object> Arguments {
            get { return _arguments; }
        }
    }
}