//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace NodejsTests.Debugger.Serialization {
    [TestClass]
    public class PrototypeVariableTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreatePrototypeVariable() {
            // Arrange
            var parent = new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null);
            JObject json = SerializationTestData.GetLookupJsonPrototype();
            Dictionary<int, JToken> references = SerializationTestData.GetLookupJsonReferences();

            // Act
            var result = new NodePrototypeVariable(parent, json, references);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NodePropertyAttributes.DontEnum, result.Attributes);
            Assert.AreEqual(NodeVariableType.Object, result.Class);
            Assert.AreEqual(4, result.Id);
            Assert.AreEqual(NodeVariableType.Prototype, result.Name);
            Assert.AreEqual(parent, result.Parent);
            Assert.IsNull(result.StackFrame);
            Assert.AreEqual("#<Object>", result.Text);
            Assert.AreEqual(NodePropertyType.Normal, result.Type);
            Assert.AreEqual("object", result.TypeName);
            Assert.IsNull(result.Value);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateLookupVariableWithNullParent() {
            // Arrange
            JObject json = SerializationTestData.GetLookupJsonPrototype();
            Dictionary<int, JToken> references = SerializationTestData.GetLookupJsonReferences();

            // Act
            var result = new NodePrototypeVariable(null, json, references);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NodePropertyAttributes.DontEnum, result.Attributes);
            Assert.AreEqual(NodeVariableType.Object, result.Class);
            Assert.AreEqual(4, result.Id);
            Assert.AreEqual(NodeVariableType.Prototype, result.Name);
            Assert.IsNull(result.Parent);
            Assert.IsNull(result.StackFrame);
            Assert.AreEqual("#<Object>", result.Text);
            Assert.AreEqual(NodePropertyType.Normal, result.Type);
            Assert.AreEqual("object", result.TypeName);
            Assert.IsNull(result.Value);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateLookupVariableWithNullJsonValue() {
            // Arrange
            var parent = new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null);
            Exception exception = null;
            Dictionary<int, JToken> references = SerializationTestData.GetLookupJsonReferences();
            NodePrototypeVariable result = null;

            // Act
            try {
                result = new NodePrototypeVariable(parent, null, references);
            } catch (Exception e) {
                exception = e;
            }

            // Assert
            Assert.IsNull(result);
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateLookupVariableWithNullJsonReferences() {
            // Arrange
            var parent = new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null);
            JObject json = SerializationTestData.GetLookupJsonPrototype();
            Exception exception = null;
            NodePrototypeVariable result = null;

            // Act
            try {
                result = new NodePrototypeVariable(parent, json, null);
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