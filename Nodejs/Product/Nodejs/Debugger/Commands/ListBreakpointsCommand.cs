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
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands {
    internal sealed class ListBreakpointsCommand : DebuggerCommand {
        public ListBreakpointsCommand(int id) : base(id, "listbreakpoints") {
        }

        public Dictionary<int, int> Breakpoints { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            JToken body = response["body"];

            JArray breakpoints = (JArray)body["breakpoints"] ?? new JArray();
            Breakpoints = new Dictionary<int, int>(breakpoints.Count);

            foreach (JToken breakpoint in breakpoints) {
                var breakpointId = (int)breakpoint["number"];
                var hitCount = (int)breakpoint["hit_count"];

                Breakpoints.Add(breakpointId, hitCount);
            }
        }
    }
}