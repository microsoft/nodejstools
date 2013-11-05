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

namespace NodejsTests.Debugger.Serialization {
    [TestClass]
    public class BacktraceVariableTests {
        [TestMethod, Priority(0)]
        public void CreateBacktraceVariable() {
            // Arrange
            JsonValue json = SerializationTestData.GetBacktraceJsonObject();
            var stackFrame = new NodeStackFrame(null, null, null, 0, 0, 0, 0);

            // Act
            var result = new NodeBacktraceVariable(stackFrame, json);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NodePropertyAttributes.ReadOnly, result.Attributes);
            Assert.IsNull(result.Class);
            Assert.AreEqual(21, result.Id);
            Assert.AreEqual("v_boolean", result.Name);
            Assert.IsNull(result.Parent);
            Assert.AreEqual(stackFrame, result.StackFrame);
            Assert.IsNull(result.Text);
            Assert.AreEqual(NodePropertyType.Normal, result.Type);
            Assert.AreEqual("boolean", result.TypeName);
            Assert.AreEqual("False", result.Value);
        }

        [TestMethod, Priority(0)]
        public void CreateBacktraceVariableWithNullName() {
            // Arrange
            JsonValue json = SerializationTestData.GetBacktraceJsonObjectWithNullName();
            var stackFrame = new NodeStackFrame(null, null, null, 0, 0, 0, 0);

            // Act
            var result = new NodeBacktraceVariable(stackFrame, json);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NodePropertyAttributes.ReadOnly, result.Attributes);
            Assert.IsNull(result.Class);
            Assert.AreEqual(21, result.Id);
            Assert.AreEqual(NodeVariableType.AnonymousVariable, result.Name);
            Assert.IsNull(result.Parent);
            Assert.AreEqual(stackFrame, result.StackFrame);
            Assert.IsNull(result.Text);
            Assert.AreEqual(NodePropertyType.Normal, result.Type);
            Assert.AreEqual("boolean", result.TypeName);
        }

        [TestMethod, Priority(0)]
        public void CreateBacktraceVariableWithNullStackFrame() {
            // Arrange
            JsonValue json = SerializationTestData.GetBacktraceJsonObject();
            Exception exception = null;
            NodeBacktraceVariable result = null;

            // Act
            try {
                result = new NodeBacktraceVariable(null, json);
            }
            catch (Exception e) {
                exception = e;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        }

        [TestMethod, Priority(0)]
        public void CreateBacktraceVariableWithNullJson() {
            // Arrange
            var stackFrame = new NodeStackFrame(null, null, null, 0, 0, 0, 0);
            Exception exception = null;
            NodeBacktraceVariable result = null;

            // Act
            try {
                result = new NodeBacktraceVariable(stackFrame, null);
            }
            catch (Exception e) {
                exception = e;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        }
    }
}