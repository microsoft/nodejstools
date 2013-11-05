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

using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Debugger.Serialization {
    class NodeBacktraceVariable : INodeVariable {
        public NodeBacktraceVariable(NodeStackFrame stackFrame, JsonValue parameter) {
            Utilities.ArgumentNotNull("stackFrame", stackFrame);
            Utilities.ArgumentNotNull("parameter", parameter);

            JsonValue value = parameter["value"];
            Id = value.GetValue<int>("ref");
            Parent = null;
            StackFrame = stackFrame;
            Name = parameter.GetValue<string>("name") ?? NodeVariableType.AnonymousVariable;
            TypeName = value.GetValue<string>("type");
            Value = value.GetValue<string>("value");
            Class = value.GetValue<string>("className");
            Text = value.GetValue<string>("text");
            Attributes = NodePropertyAttributes.ReadOnly;
            Type = NodePropertyType.Normal;
        }

        public int Id { get; private set; }
        public NodeEvaluationResult Parent { get; private set; }
        public string Name { get; private set; }
        public string TypeName { get; private set; }
        public string Value { get; private set; }
        public string Class { get; private set; }
        public string Text { get; private set; }
        public NodePropertyAttributes Attributes { get; private set; }
        public NodePropertyType Type { get; private set; }
        public NodeStackFrame StackFrame { get; private set; }
    }
}