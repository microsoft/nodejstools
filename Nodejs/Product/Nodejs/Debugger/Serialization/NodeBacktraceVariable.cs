// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    internal sealed class NodeBacktraceVariable : INodeVariable
    {
        public NodeBacktraceVariable(NodeStackFrame stackFrame, JToken parameter)
        {
            Utilities.ArgumentNotNull(nameof(stackFrame), stackFrame);
            Utilities.ArgumentNotNull(nameof(parameter), parameter);

            var value = parameter["value"];
            this.Id = (int)value["ref"];
            this.Parent = null;
            this.StackFrame = stackFrame;
            this.Name = (string)parameter["name"] ?? NodeVariableType.AnonymousVariable;
            this.TypeName = (string)value["type"];
            this.Value = (string)value["value"];
            this.Class = (string)value["className"];
            try
            {
                this.Text = (string)value["text"];
            }
            catch (ArgumentException)
            {
                this.Text = string.Empty;
            }
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
