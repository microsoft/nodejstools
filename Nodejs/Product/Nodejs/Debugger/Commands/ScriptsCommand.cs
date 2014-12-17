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
    sealed class ScriptsCommand : DebuggerCommand {
        private readonly Dictionary<string, object> _arguments;

        public ScriptsCommand(int id, bool includeSource = false, int? moduleId = null) : base(id, "scripts") {
            _arguments = new Dictionary<string, object> {
                { "includeSource", includeSource }
            };

            if (moduleId != null) {
                _arguments["ids"] = new object[] { moduleId };
            }
        }

        protected override IDictionary<string, object> Arguments {
            get { return _arguments; }
        }

        public List<NodeModule> Modules { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            JArray body = (JArray)response["body"] ?? new JArray();
            Modules = new List<NodeModule>(body.Count);

            foreach (JToken module in body) {
                var fileName = (string)module["name"];
                if (fileName == null) {
                    continue;
                }

                var id = (int)module["id"];
                var source = (string)module["source"];
                if (!string.IsNullOrEmpty(source) &&
                    source.StartsWith(NodeConstants.ScriptWrapBegin) &&
                    source.EndsWith(NodeConstants.ScriptWrapEnd)) {
                    source = source.Substring(
                        NodeConstants.ScriptWrapBegin.Length,
                        source.Length - NodeConstants.ScriptWrapBegin.Length - NodeConstants.ScriptWrapEnd.Length);
                }

                Modules.Add(new NodeModule(id, fileName) { Source = source });
            }
        }
    }
}