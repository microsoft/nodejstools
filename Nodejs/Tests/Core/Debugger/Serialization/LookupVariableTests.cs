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
using System.Collections.Generic;
using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Serialization {
    [TestClass]
    public class LookupVariableTests {
        [TestMethod, Priority(0)]
        public void CreateLookupVariable() {
            // Arrange
            var parent = new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null);
            JsonValue json = SerializationTestData.GetLookupJsonProperty();
            Dictionary<int, JsonValue> references = SerializationTestData.GetLookupJsonReferences();

            // Act
            var result = new NodeLookupVariable(parent, json, references);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NodePropertyAttributes.None, result.Attributes);
            Assert.IsNull(result.Class);
            Assert.AreEqual(54, result.Id);
            Assert.AreEqual("first", result.Name);
            Assert.AreEqual(parent, result.Parent);
            Assert.IsNull(result.StackFrame);
            Assert.AreEqual("1", result.Text);
            Assert.AreEqual(NodePropertyType.Field, result.Type);
            Assert.AreEqual("number", result.TypeName);
            Assert.AreEqual("1", result.Value);
        }

        [TestMethod, Priority(0)]
        public void CreateLookupVariableWithNullParent() {
            // Arrange
            Exception exception = null;
            JsonValue json = SerializationTestData.GetLookupJsonProperty();
            Dictionary<int, JsonValue> references = SerializationTestData.GetLookupJsonReferences();
            NodeLookupVariable result = null;

            // Act
            try {
                result = new NodeLookupVariable(null, json, references);
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
        public void CreateLookupVariableWithNullJsonValue() {
            // Arrange
            var parent = new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null);
            Exception exception = null;
            Dictionary<int, JsonValue> references = SerializationTestData.GetLookupJsonReferences();
            NodeLookupVariable result = null;

            // Act
            try {
                result = new NodeLookupVariable(parent, null, references);
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
        public void CreateLookupVariableWithNullJsonReferences() {
            // Arrange
            var parent = new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null);
            JsonValue json = SerializationTestData.GetLookupJsonProperty();
            Exception exception = null;
            NodeLookupVariable result = null;

            // Act
            try {
                result = new NodeLookupVariable(parent, json, null);
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