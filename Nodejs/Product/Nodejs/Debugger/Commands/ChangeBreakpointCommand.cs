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

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class ChangeBreakpointCommand : DebuggerCommand
    {
        private readonly Dictionary<string, object> _arguments;

        public ChangeBreakpointCommand(int id, int breakpointId, bool? enabled = null, string condition = null, int? ignoreCount = null)
            : base(id, "changebreakpoint")
        {
            _arguments = new Dictionary<string, object> { { "breakpoint", breakpointId } };

            if (enabled != null)
            {
                _arguments["enabled"] = enabled.Value;
            }

            if (condition != null)
            {
                _arguments["condition"] = condition;
            }

            if (ignoreCount != null)
            {
                _arguments["ignoreCount"] = ignoreCount.Value;
            }
        }

        protected override IDictionary<string, object> Arguments
        {
            get { return _arguments; }
        }
    }
}