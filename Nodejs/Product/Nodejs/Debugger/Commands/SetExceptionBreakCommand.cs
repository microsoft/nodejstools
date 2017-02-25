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
    internal sealed class SetExceptionBreakCommand : DebuggerCommand
    {
        private readonly Dictionary<string, object> _arguments;

        public SetExceptionBreakCommand(int id, bool uncaughtExceptions, bool enabled) : base(id, "setexceptionbreak")
        {
            this._arguments = new Dictionary<string, object> {
                { "type", uncaughtExceptions ? "uncaught" : "all" },
                { "enabled", enabled }
            };
        }

        protected override IDictionary<string, object> Arguments
        {
            get { return this._arguments; }
        }
    }
}