// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Serialization;

namespace NodejsTests.Mocks
{
    internal class MockNodeVariable : INodeVariable
    {
        public int Id { get; set; }
        public NodeEvaluationResult Parent { get; set; }
        public NodeStackFrame StackFrame { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Value { get; set; }
        public string Class { get; set; }
        public string Text { get; set; }
        public NodePropertyAttributes Attributes { get; set; }
        public NodePropertyType Type { get; set; }
    }
}

