// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    internal sealed class NodeSetValueVariable : INodeVariable
    {
        public NodeSetValueVariable(NodeStackFrame stackFrame, string name, JToken message)
        {
            this.Id = (int)message["body"]["newValue"]["handle"];
            this.StackFrame = stackFrame;
            this.Parent = null;
            this.Name = name;
            this.TypeName = (string)message["body"]["newValue"]["type"];
            this.Value = (string)message["body"]["newValue"]["value"];
            this.Class = (string)message["body"]["newValue"]["className"];
            this.Text = (string)message["body"]["newValue"]["text"];
            this.Attributes = NodePropertyAttributes.None;
            this.Type = NodePropertyType.Normal;
        }

        public int Id { get; }
        public NodeEvaluationResult Parent { get; }
        public NodeStackFrame StackFrame { get; }
        public string Name { get; }
        public string TypeName { get; }
        public string Value { get; }
        public string Class { get; }
        public string Text { get; }
        public NodePropertyAttributes Attributes { get; }
        public NodePropertyType Type { get; }
    }
}
