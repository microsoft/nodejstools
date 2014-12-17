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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands {
    abstract class DebuggerCommand {
        private readonly string _commandName;

        protected DebuggerCommand(int id, string commandName) {
            Id = id;
            _commandName = commandName;
        }

        /// <summary>
        /// Gets a command arguments.
        /// </summary>
        protected virtual IDictionary<string, object> Arguments {
            get { return null; }
        }

        /// <summary>
        /// Gets a command identifier.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets a value indicating whether command execution in progress.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// Parses response message.
        /// </summary>
        /// <param name="response">Message.</param>
        /// <returns>Indicates whether command execution succeeded.</returns>
        public virtual void ProcessResponse(JObject response) {
            Running = (bool?)response["running"] ?? false;

            if (!(bool)response["success"]) {
                var message = (string)response["message"];
                throw new DebuggerCommandException(message);
            }
        }

        /// <summary>
        /// Serializes a command.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return JsonConvert.SerializeObject(
                new {
                    command = _commandName,
                    seq = Id,
                    type = "request",
                    arguments = Arguments
                });
        }
    }
}