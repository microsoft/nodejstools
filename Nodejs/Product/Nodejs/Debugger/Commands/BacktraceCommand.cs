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
using Microsoft.NodejsTools.Debugger.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class BacktraceCommand : DebuggerCommand
    {
        private readonly Dictionary<string, object> _arguments;
        private readonly IEvaluationResultFactory _resultFactory;
        private readonly bool _depthOnly;
        private readonly NodeModule _unknownModule = new NodeModule(-1, NodeVariableType.UnknownModule);

        public BacktraceCommand(int id, IEvaluationResultFactory resultFactory, int fromFrame, int toFrame, bool depthOnly = false)
            : base(id, "backtrace")
        {
            _resultFactory = resultFactory;
            _depthOnly = depthOnly;

            _arguments = new Dictionary<string, object> {
                { "fromFrame", fromFrame },
                { "toFrame", toFrame },
                { "inlineRefs", true }
            };
        }

        protected override IDictionary<string, object> Arguments
        {
            get { return _arguments; }
        }

        public int CallstackDepth { get; private set; }
        public List<NodeStackFrame> StackFrames { get; private set; }
        public Dictionary<int, NodeModule> Modules { get; private set; }

        public override void ProcessResponse(JObject response)
        {
            base.ProcessResponse(response);

            JToken body = response["body"];
            CallstackDepth = (int)body["totalFrames"];

            // Collect frames only if required
            if (_depthOnly)
            {
                Modules = new Dictionary<int, NodeModule>();
                StackFrames = new List<NodeStackFrame>();
                return;
            }

            // Extract scripts (if not provided)
            Modules = GetModules((JArray)response["refs"]);

            // Extract frames
            JArray frames = (JArray)body["frames"] ?? new JArray();
            StackFrames = new List<NodeStackFrame>(frames.Count);

            foreach (JToken frame in frames)
            {
                // Create stack frame
                string functionName = GetFunctionName(frame);
                var moduleId = (int?)frame["func"]["scriptId"];

                NodeModule module;
                if (!moduleId.HasValue || !Modules.TryGetValue(moduleId.Value, out module))
                {
                    module = _unknownModule;
                }

                int line = (int?)frame["line"] ?? 0;
                int column = (int?)frame["column"] ?? 0;
                int frameId = (int?)frame["index"] ?? 0;

                var stackFrame = new NodeStackFrame(frameId)
                {
                    Column = column,
                    FunctionName = functionName,
                    Line = line,
                    Module = module
                };

                // Locals
                JArray variables = (JArray)frame["locals"] ?? new JArray();
                stackFrame.Locals = GetVariables(stackFrame, variables);

                // Arguments
                variables = (JArray)frame["arguments"] ?? new JArray();
                stackFrame.Parameters = GetVariables(stackFrame, variables);

                StackFrames.Add(stackFrame);
            }
        }

        private List<NodeEvaluationResult> GetVariables(NodeStackFrame stackFrame, IEnumerable<JToken> variables)
        {
            return variables.Select(t => new NodeBacktraceVariable(stackFrame, t))
                .Select(variableProvider => _resultFactory.Create(variableProvider)).ToList();
        }

        private static string GetFunctionName(JToken frame)
        {
            JToken func = frame["func"];
            var framename = (string)func["name"];
            if (string.IsNullOrEmpty(framename))
            {
                framename = (string)func["inferredName"];
            }
            if (string.IsNullOrEmpty(framename))
            {
                framename = NodeVariableType.AnonymousFunction;
            }
            return framename;
        }

        private static Dictionary<int, NodeModule> GetModules(JArray references)
        {
            var scripts = new Dictionary<int, NodeModule>(references.Count);
            foreach (JToken reference in references)
            {
                var scriptId = (int)reference["id"];
                var fileName = (string)reference["name"];

                scripts.Add(scriptId, new NodeModule(scriptId, fileName));
            }
            return scripts;
        }
    }
}