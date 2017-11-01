// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    internal sealed class NodePrototypeVariable : INodeVariable
    {
        public NodePrototypeVariable(NodeEvaluationResult parent, JToken prototype, Dictionary<int, JToken> references)
        {
            Utilities.ArgumentNotNull("prototype", prototype);
            Utilities.ArgumentNotNull("references", references);

            this.Id = (int)prototype["ref"];
            if (!references.TryGetValue(this.Id, out var reference))
            {
                reference = prototype;
            }
            this.Parent = parent;
            this.StackFrame = parent != null ? parent.Frame : null;
            this.Name = NodeVariableType.Prototype;
            this.TypeName = (string)reference["type"];
            this.Value = (string)reference["value"];
            this.Class = (string)reference["className"];
            this.Text = (string)reference["text"];
            this.Attributes = NodePropertyAttributes.DontEnum;
            this.Type = NodePropertyType.Normal;
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
