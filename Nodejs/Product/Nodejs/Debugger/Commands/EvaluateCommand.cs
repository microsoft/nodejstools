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
using Microsoft.NodejsTools.Debugger.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class EvaluateCommand : DebuggerCommand
    {
        private readonly Dictionary<string, object> _arguments;
        private readonly string _expression;
        private readonly IEvaluationResultFactory _resultFactory;
        private readonly NodeStackFrame _stackFrame;

        public EvaluateCommand(int id, IEvaluationResultFactory resultFactory, string expression, NodeStackFrame stackFrame = null)
            : base(id, "evaluate")
        {
            this._resultFactory = resultFactory;
            this._expression = expression;
            this._stackFrame = stackFrame;

            this._arguments = new Dictionary<string, object> {
                { "expression", _expression },
                { "frame", _stackFrame != null ? _stackFrame.FrameId : 0 },
                { "global", false },
                { "disable_break", true },
                { "maxStringLength", -1 }
            };
        }

        public EvaluateCommand(int id, IEvaluationResultFactory resultFactory, int variableId, NodeStackFrame stackFrame = null)
            : base(id, "evaluate")
        {
            this._resultFactory = resultFactory;
            this._expression = "variable";
            this._stackFrame = stackFrame;

            this._arguments = new Dictionary<string, object> {
                { "expression", _expression + ".toString()" },
                { "frame", _stackFrame != null ? _stackFrame.FrameId : 0 },
                { "global", false },
                { "disable_break", true },
                { "additional_context", new[] { new { name = _expression, handle = variableId } } },
                { "maxStringLength", -1 }
            };
        }

        protected override IDictionary<string, object> Arguments
        {
            get { return this._arguments; }
        }

        public NodeEvaluationResult Result { get; private set; }

        public override void ProcessResponse(JObject response)
        {
            base.ProcessResponse(response);

            var variableProvider = new NodeEvaluationVariable(this._stackFrame, this._expression, response["body"]);
            this.Result = this._resultFactory.Create(variableProvider);
        }
    }
}