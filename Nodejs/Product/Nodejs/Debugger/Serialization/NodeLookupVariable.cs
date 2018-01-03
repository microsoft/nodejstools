// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    internal sealed class NodeLookupVariable : INodeVariable
    {
        public NodeLookupVariable(NodeEvaluationResult parent, JToken property, Dictionary<int, JToken> references)
        {
            Utilities.ArgumentNotNull(nameof(property), property);
            Utilities.ArgumentNotNull(nameof(references), references);

            this.Id = (int)property["ref"];
            if (!references.TryGetValue(this.Id, out var reference))
            {
                reference = property;
            }
            this.Parent = parent;
            this.StackFrame = parent != null ? parent.Frame : null;
            this.Name = (string)property["name"];
            this.TypeName = (string)reference["type"];
            this.Value = (string)reference["value"];
            this.Class = (string)reference["className"];
            this.Text = (string)reference["text"];
            this.Attributes = (NodePropertyAttributes)property.Value<int>("attributes");
            this.Type = (NodePropertyType)property.Value<int>("propertyType");
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
