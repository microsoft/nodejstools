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

using System;
using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace NodejsTests.Debugger.Serialization {
    [TestClass]
    public class SetValueVariableTests {
        [TestMethod]
        public void CreateSetVariableValue() {
            // Arrange
            JObject json = SerializationTestData.GetSetVariableValueResponse();
            const int frameId = 3;
            var stackFrame = new NodeStackFrame(null, null, null, 0, 0, 0, frameId);
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