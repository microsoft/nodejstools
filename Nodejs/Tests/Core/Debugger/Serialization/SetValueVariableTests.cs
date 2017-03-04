// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace NodejsTests.Debugger.Serialization
{
    [TestClass]
    public class SetValueVariableTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetVariableValue()
        {
            // Arrange
            JObject json = SerializationTestData.GetSetVariableValueResponse();
            const int frameId = 3;
            var stackFrame = new NodeStackFrame(frameId);
            const string name = "name";

            // Act
            var result = new NodeSetValueVariable(stackFrame, name, json);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NodePropertyAttributes.None, result.Attributes);
            Assert.IsNull(result.Class);
            Assert.AreEqual(44, result.Id);
            Assert.AreEqual(name, result.Name);
            Assert.IsNull(result.Parent);
            Assert.AreEqual(stackFrame, result.StackFrame);
            Assert.AreEqual("55", result.Text);
            Assert.AreEqual(NodePropertyType.Normal, result.Type);
            Assert.AreEqual("number", result.TypeName);
            Assert.AreEqual("55", result.Value);
        }
    }
}

