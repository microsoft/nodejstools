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
    public class EvaluationVariableTests {
        [TestMethod]
        public void CreateEvaluationVariable() {
            // Arrange
            JObject json = SerializationTestData.GetEvaluationJsonObject();
            var stackFrame = new NodeStackFrame(null, null, null, 0, 0, 0, 0, 0);
            const string name = "name";

            // Act
            var result = new NodeEvaluationVariable(stackFrame, name, json);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NodePropertyAttributes.None, result.Attributes);
            Assert.IsNull(result.Class);
            Assert.AreEqual(16, result.Id);
            Assert.AreEqual(name, result.Name);
            Assert.IsNull(result.Parent);
            Assert.AreEqual(stackFrame, result.StackFrame);
            Assert.AreEqual("<tag>Value</tag>", result.Text);
            Assert.AreEqual(NodePropertyType.Normal, result.Type);
            Assert.AreEqual("string", result.TypeName);
            Assert.AreEqual("<tag>Value</tag>", result.Value);
        }

        [TestMethod]
        public void CreateBacktraceVariableWithNullStackFrame() {
            // Arrange
            JObject json = SerializationTestData.GetBacktraceJsonObject();
            Exception exception = null;
            NodeEvaluationVariable result = null;
            const string name = "name";

            // Act
            try {
                result = new NodeEvaluationVariable(null, name, json);
            } catch (Exception e) {
                exception = e;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        }

        [TestMethod]
        public void CreateBacktraceVariableWithNullName() {
            // Arrange
            JObject json = SerializationTestData.GetBacktraceJsonObject();
            Exception exception = null;
            NodeEvaluationVariable result = null;
            const string name = "name";

            // Act
            try {
                result = new NodeEvaluationVariable(null, name, json);
            } catch (Exception e) {
                exception = e;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        }

        [TestMethod]
        public void CreateBacktraceVariableWithNullJson() {
            // Arrange
            var stackFrame = new NodeStackFrame(null, null, null, 0, 0, 0, 0, 0);
            Exception exception = null;
            NodeEvaluationVariable result = null;
            const string name = "name";

            // Act
            try {
                result = new NodeEvaluationVariable(stackFrame, name, null);
            } catch (Exception e) {
                exception = e;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        }
    }
}