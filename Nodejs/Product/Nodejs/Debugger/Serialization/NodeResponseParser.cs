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

namespace Microsoft.NodejsTools.Debugger.Serialization {
    class NodeResponseParser : INodeResponseParser {
        private readonly INodeEvaluationResultFactory _evaluationResultFactory;

        public NodeResponseParser(INodeEvaluationResultFactory evaluationResultFactory) {
            _evaluationResultFactory = evaluationResultFactory;
        }

        public NodeStackFrame[] ProcessBacktrace(NodeDebugger debugger, JsonValue message) {
            // Extract scripts
            IList<JsonValue> refs = message.GetArray("refs");
            Dictionary<int, NodeModule> modules = GetScripts(refs);

            // Extract frames
            JsonValue body = message["body"];
            IList<JsonValue> frames = body.GetArray("frames");
            if (frames == null) {
                return new NodeStackFrame[] {};
            }

            var stackFrames = new List<NodeStackFrame>(frames.Count);

            for (int i = 0; i < frames.Count; i++) {
                JsonValue frame = frames[i];

                // Create stack frame
                string name = GetFrameName(frame);
                var moduleId = frame["func"].GetValue<int>("scriptId");
                NodeModule module = modules[moduleId];
                int line = frame.GetValue<int>("line") + 1;
                var stackFrameId = frame.GetValue<int>("index");
                NodeThread mainThread = debugger.GetThreads().FirstOrDefault(p => p.Id == debugger.MainThreadId);

                var stackFrame = new NodeStackFrame(mainThread, module, name, line, line, line, stackFrameId);

                // Locals
                IList<JsonValue> variables = frame.GetArray("locals");
                List<NodeEvaluationResult> locals = GetVariables(debugger, stackFrame, variables);

                // Arguments
                variables = frame.GetArray("arguments");
                List<NodeEvaluationResult> parameters = GetVariables(debugger, stackFrame, variables);

                stackFrame.Locals = locals;
                stackFrame.Parameters = parameters;

                stackFrames.Add(stackFrame);
            }

            return stackFrames.ToArray();
        }

        public NodeEvaluationResult[] ProcessLookup(NodeDebugger debugger, NodeEvaluationResult parent, JsonValue message) {
            // Retrieve references
            IList<JsonValue> refs = message.GetArray("refs");
            var references = new Dictionary<int, JsonValue>(refs.Count);
            for (int i = 0; i < refs.Count; i++) {
                JsonValue reference = refs[i];
                var id = reference.GetValue<int>("handle");
                references.Add(id, reference);
            }

            // Retrieve properties
            JsonValue body = message["body"];
            JsonValue objectData = body[parent.Handle.ToString()];
            var properties = new List<NodeEvaluationResult>();

            IList<JsonValue> props = objectData.GetArray("properties");
            if (props != null) {
                for (int i = 0; i < props.Count; i++) {
                    JsonValue property = props[i];
                    var variableProvider = new NodeLookupVariable(parent, property, references);
                    NodeEvaluationResult result = _evaluationResultFactory.Create(debugger, variableProvider);
                    properties.Add(result);
                }
            }

            // Try to get prototype
            JsonValue prototype = objectData["protoObject"];
            if (prototype != null) {
                var variableProvider = new NodePrototypeVariable(parent, prototype, references);
                NodeEvaluationResult result = _evaluationResultFactory.Create(debugger, variableProvider);
                properties.Add(result);
            }

            return properties.ToArray();
        }

        public NodeEvaluationResult ProcessEvaluate(NodeDebugger debugger, NodeStackFrame stackFrame, string expression, JsonValue message) {
            JsonValue body = message["body"];
            var variableProvider = new NodeEvaluationVariable(stackFrame, expression, body);
            return _evaluationResultFactory.Create(debugger, variableProvider);
        }

        private static string GetFrameName(JsonValue frame) {
            JsonValue func = frame["func"];
            var framename = func.GetValue<string>("name");
            if (string.IsNullOrEmpty(framename)) {
                framename = func.GetValue<string>("inferredName");
            }
            if (string.IsNullOrEmpty(framename)) {
                framename = NodeVariableType.AnonymousFunction;
            }
            return framename;
        }

        private static Dictionary<int, NodeModule> GetScripts(IList<JsonValue> references) {
            var scripts = new Dictionary<int, NodeModule>(references.Count);
            for (int i = 0; i < references.Count; i++) {
                JsonValue reference = references[i];
                var scriptId = reference.GetValue<int>("id");
                var filename = reference.GetValue<string>("name");

                scripts.Add(scriptId, new NodeModule(scriptId, filename));
            }
            return scripts;
        }

        private List<NodeEvaluationResult> GetVariables(NodeDebugger debugger, NodeStackFrame stackFrame, IList<JsonValue> variables) {
            var results = new List<NodeEvaluationResult>(variables.Count);
            for (int i = 0; i < variables.Count; i++) {
                var variableProvider = new NodeBacktraceVariable(stackFrame, variables[i]);
                NodeEvaluationResult result = _evaluationResultFactory.Create(debugger, variableProvider);
                results.Add(result);
            }
            return results.ToList();
        }
    }
}