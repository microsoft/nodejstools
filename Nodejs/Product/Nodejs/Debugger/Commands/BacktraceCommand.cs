/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Linq;
using Microsoft.NodejsTools.Debugger.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands {
    sealed class BacktraceCommand : DebuggerCommandBase {
        private readonly Dictionary<int, NodeModule> _modules;
        private readonly IEvaluationResultFactory _resultFactory;
        private readonly NodeThread _thread;
        private readonly NodeModule _unknownModule = new NodeModule(null, -1, "<unknown>");

        public BacktraceCommand(int id, IEvaluationResultFactory resultFactory,
            int fromFrame,
            int toFrame,
            NodeThread thread = null,
            Dictionary<int, NodeModule> modules = null) : base(id) {
            _resultFactory = resultFactory;
            _thread = thread;
            _modules = modules;

            CommandName = "backtrace";
            Arguments = new Dictionary<string, object> {
                { "fromFrame", fromFrame },
                { "toFrame", toFrame },
                { "inlineRefs", true }
            };

            StackFrames = new List<NodeStackFrame>();
        }

        public int CallstackDepth { get; private set; }
        public List<NodeStackFrame> StackFrames { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            JToken body = response["body"];
            CallstackDepth = (int)body["totalFrames"];

            // Should not collect stack frames without thread
            if (_thread == null) {
                return;
            }

            // Extract scripts (if not provided)
            Dictionary<int, NodeModule> modules = _modules ?? GetScripts((JArray)response["refs"]);

            // Extract frames
            var frames = (JArray)body["frames"];
            if (frames == null) {
                return;
            }

            for (int i = 0; i < frames.Count; i++) {
                JToken frame = frames[i];

                // Create stack frame
                string name = GetFrameName(frame);
                var moduleId = (int)frame["func"]["scriptId"];

                NodeModule module;
                if (!modules.TryGetValue(moduleId, out module)) {
                    module = _unknownModule;
                }

                int line = (int)frame["line"] + 1;
                var stackFrameId = (int)frame["index"];

                var stackFrame = new NodeStackFrame(_thread, module, name, line, line, line, stackFrameId);

                // Locals
                var variables = (JArray)frame["locals"];
                List<NodeEvaluationResult> locals = GetVariables(stackFrame, variables);

                // Arguments
                variables = (JArray)frame["arguments"];
                List<NodeEvaluationResult> parameters = GetVariables(stackFrame, variables);

                stackFrame.Locals = locals;
                stackFrame.Parameters = parameters;

                StackFrames.Add(stackFrame);
            }
        }

        private List<NodeEvaluationResult> GetVariables(NodeStackFrame stackFrame, JArray variables) {
            var results = new List<NodeEvaluationResult>(variables.Count);
            for (int i = 0; i < variables.Count; i++) {
                var variableProvider = new NodeBacktraceVariable(stackFrame, variables[i]);
                NodeEvaluationResult result = _resultFactory.Create(variableProvider);
                results.Add(result);
            }
            return results.ToList();
        }

        private static string GetFrameName(JToken frame) {
            JToken func = frame["func"];
            var framename = (string)func["name"];
            if (string.IsNullOrEmpty(framename)) {
                framename = (string)func["inferredName"];
            }
            if (string.IsNullOrEmpty(framename)) {
                framename = NodeVariableType.AnonymousFunction;
            }
            return framename;
        }

        private static Dictionary<int, NodeModule> GetScripts(JArray references) {
            var scripts = new Dictionary<int, NodeModule>(references.Count);
            for (int i = 0; i < references.Count; i++) {
                JToken reference = references[i];
                var scriptId = (int)reference["id"];
                var filename = (string)reference["name"];

                scripts.Add(scriptId, new NodeModule(null, scriptId, filename));
            }
            return scripts;
        }
    }
}