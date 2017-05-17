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
            Utilities.ArgumentNotNull("property", property);
            Utilities.ArgumentNotNull("references", references);

            this.Id = (int)property["ref"];
            JToken reference;
            if (!references.TryGetValue(this.Id, out reference))
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

