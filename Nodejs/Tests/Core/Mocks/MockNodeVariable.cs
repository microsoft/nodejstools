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

using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Serialization;

namespace NodejsTests.Mocks {
    class MockNodeVariable : INodeVariable {
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