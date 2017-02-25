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
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Events
{
    internal sealed class BreakpointEvent : IDebuggerEvent
    {
        public BreakpointEvent(JObject message)
        {
            this.Running = false;
            this.Line = (int)message["body"]["sourceLine"];
            this.Column = (int)message["body"]["sourceColumn"];

            var scriptId = (int)message["body"]["script"]["id"];
            var fileName = (string)message["body"]["script"]["name"];

            this.Module = new NodeModule(scriptId, fileName);

            var breakpoints = message["body"]["breakpoints"];
            this.Breakpoints = breakpoints != null
                ? breakpoints.Values<int>().ToList()
                : new List<int>();
        }

        public List<int> Breakpoints { get; private set; }
        public NodeModule Module { get; private set; }
        public int Line { get; private set; }
        public int Column { get; set; }
        public bool Running { get; private set; }
    }
}