// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    internal sealed class NodeEvaluationVariable : INodeVariable
    {
        public NodeEvaluationVariable(NodeStackFrame stackFrame, string name, JToken message)
        {
            Utilities.ArgumentNotNull("name", name);
            Utilities.ArgumentNotNull("message", message);

            this.Id = (int)message["handle"];
            this.Parent = null;
            this.StackFrame = stackFrame;
            this.Name = name;
            this.TypeName = (string)message["type"];
            this.Value = (string)message["value"];
            this.Class = (string)message["className"];
            this.Text = (string)message["text"];
            this.Attributes = NodePropertyAttributes.None;
            this.Type = NodePropertyType.Normal;
        }

        public int Id { get; }
        public NodeEvaluationResult Parent { get; }
        public string Name { get; }
        public string TypeName { get; }
        public string Value { get; }
        public string Class { get; }
        public string Text { get; }
        public NodePropertyAttributes Attributes { get; }
        public NodePropertyType Type { get; }
        public NodeStackFrame StackFrame { get; }
    }
}
