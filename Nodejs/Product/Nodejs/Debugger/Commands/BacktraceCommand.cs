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
    sealed class BacktraceCommand : DebuggerCommand {
        private readonly Dictionary<string, object> _arguments;
        private readonly NodeDebugger _debugger;
        private readonly IEvaluationResultFactory _resultFactory;
        private readonly NodeModule _unknownModule = new NodeModule(-1, NodeVariableType.UnknownModule, NodeVariableType.UnknownModule);

        public BacktraceCommand(
            int id,
            IEvaluationResultFactory resultFactory,
            int fromFrame,
            int toFrame,
            NodeDebugger debugger = null) : base(id, "backtrace") {
            _resultFactory = resultFactory;
            _debugger = debugger;

            _arguments = new Dictionary<string, object> {
                { "fromFrame", fromFrame },
                { "toFrame", toFrame },
                { "inlineRefs", true }
            };
        }

        protected override IDictionary<string, object> Arguments {
            get { return _arguments; }
        }

        public int CallstackDepth { get; private set; }
        public List<NodeStackFrame> StackFrames { get; private set; }
        public Dictionary<int, NodeModule> Modules { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            JToken body = response["body"];
            CallstackDepth = (int)body["totalFrames"];

            // Should not collect stack frames without debugger
            if (_debugger == null) {
                return;
            }

            // Extract scripts (if not provided)
            Dictionary<int, NodeModule> modules = GetScripts((JArray)response["refs"], _debugger);

            // Extract frames
            var frames = (JArray)body["frames"];
            if (frames == null) {
                return;
            }

            var results = new List<NodeStackFrame>(frames.Count);
            foreach (JToken frame in frames) {
                // Create stack frame
                string name = GetFrameName(frame);
                var moduleId = (int)frame["func"]["scriptId"];

                NodeModule module;
                if (!modules.TryGetValue(moduleId, out module)) {
                    module = _unknownModule;
                }

                int line = (int)frame["line"] + 1;
                var stackFrameId = (int)frame["index"];
                var stackFrame = new NodeStackFrame(_debugger, module, name, line, line, line, stackFrameId);

                // Locals
                var variables = (JArray)frame["locals"];
                List<NodeEvaluationResult> locals = GetVariables(stackFrame, variables);

                // Arguments
                variables = (JArray)frame["arguments"];
                List<NodeEvaluationResult> parameters = GetVariables(stackFrame, variables);

                stackFrame.Locals = locals;
                stackFrame.Parameters = parameters;

                results.Add(stackFrame);
            }

            Modules = modules;
            StackFrames = results;
        }

        private List<NodeEvaluationResult> GetVariables(NodeStackFrame stackFrame, IEnumerable<JToken> variables) {
            return variables.Select(t => new NodeBacktraceVariable(stackFrame, t))
                .Select(variableProvider => _resultFactory.Create(variableProvider)).ToList();
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

        private static Dictionary<int, NodeModule> GetScripts(JArray references, NodeDebugger debugger) {
            var scripts = new Dictionary<int, NodeModule>(references.Count);
            foreach (JToken reference in references) {
                var scriptId = (int)reference["id"];
                var javaScriptFilename = (string)reference["name"];
                string fileName = debugger.GetModuleFileName(javaScriptFilename);

                scripts.Add(scriptId, new NodeModule(scriptId, fileName, javaScriptFilename));
            }
            return scripts;
        }
    }
}