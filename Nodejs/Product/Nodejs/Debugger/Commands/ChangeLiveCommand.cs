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
    sealed class ChangeLiveCommand : DebuggerCommand {
        private readonly Dictionary<string, object> _arguments;

        public ChangeLiveCommand(int id, NodeModule module) : base(id, "changelive") {
            // Wrap script contents as following https://github.com/joyent/node/blob/v0.10.26-release/src/node.js#L880
            string source = string.Format("{0}{1}{2}",
                NodeConstants.ScriptWrapBegin,
                module.Source,
                NodeConstants.ScriptWrapEnd);

            _arguments = new Dictionary<string, object> {
                { "script_id", module.Id },
                { "new_source", source },
                { "preview_only", false },
            };
        }

        protected override IDictionary<string, object> Arguments {
            get { return _arguments; }
        }

        public bool Updated { get; private set; }
        public bool StackModified { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            JToken result = response["body"]["result"];
            Updated = (bool)result["updated"];
            StackModified = (bool)result["stack_modified"];
        }
    }
}