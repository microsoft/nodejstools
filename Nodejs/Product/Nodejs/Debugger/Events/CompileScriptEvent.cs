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

using Microsoft.NodejsTools.Debugger.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Events
{
    internal sealed class CompileScriptEvent : IDebuggerEvent
    {
        public CompileScriptEvent(JObject message)
        {
            this.Running = (bool)message["running"];

            var scriptId = (int)message["body"]["script"]["id"];
            string fileName = (string)message["body"]["script"]["name"] ?? NodeVariableType.UnknownModule;

            this.Module = new NodeModule(scriptId, fileName);
        }

        public NodeModule Module { get; private set; }
        public bool Running { get; private set; }
    }
}