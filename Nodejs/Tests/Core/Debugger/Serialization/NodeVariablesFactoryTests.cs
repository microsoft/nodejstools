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
using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodejsTests.Mocks;

namespace NodejsTests.Debugger.Serialization
{
    [TestClass]
    public class NodeVariablesFactoryTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateNullEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 5,
                Parent = null,
                StackFrame = null,
                Name = "v_null",
                TypeName = "null",
                Value = "null",
                Class = null,
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.IsNull(result.HexValue);
            Assert.AreEqual(variable.Value.Length, result.StringLength);
            Assert.AreEqual(variable.Value, result.StringValue);
            Assert.AreEqual(NodeExpressionType.None, result.Type);
            Assert.AreEqual(NodeVariableType.Null, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateNumberEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 20,
                Parent = null,
                StackFrame = null,
                Name = "v_number",
                TypeName = "number",
                Value = "55",
                Class = null,
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.AreEqual(string.Format("0x{0:x}", int.Parse(variable.Value)), result.HexValue);
            Assert.AreEqual(variable.Value.Length, result.StringLength);
            Assert.AreEqual(variable.Value, result.StringValue);
            Assert.AreEqual(NodeExpressionType.None, result.Type);
            Assert.AreEqual(NodeVariableType.Number, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateBooleanEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 21,
                Parent = null,
                StackFrame = null,
                Name = "v_boolean",
                TypeName = "boolean",
                Value = "false",
                Class = null,
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.IsNull(result.HexValue);
            Assert.AreEqual(variable.Value.Length, result.StringLength);
            Assert.AreEqual(variable.Value, result.StringValue);
            Assert.AreEqual(NodeExpressionType.Boolean, result.Type);
            Assert.AreEqual(NodeVariableType.Boolean, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateRegexpEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 22,
                Parent = null,
                StackFrame = null,
                Name = "v_regexp",
                TypeName = "regexp",
                Value = "55",
                Class = null,
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.IsNull(result.HexValue);
            Assert.AreEqual(variable.Value.Length, result.StringLength);
            Assert.AreEqual(variable.Value, result.StringValue);
            Assert.AreEqual(NodeExpressionType.Expandable, result.Type);
            Assert.AreEqual(NodeVariableType.Regexp, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateFunctionWithoutTextEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 23,
                Parent = null,
                StackFrame = null,
                Name = "v_function",
                TypeName = "function",
                Value = "55",
                Class = null,
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.IsNull(result.HexValue);
            Assert.AreEqual(string.Format("{{{0}}}", NodeVariableType.Function), result.StringValue);
            Assert.AreEqual(NodeExpressionType.Function | NodeExpressionType.Expandable, result.Type);
            Assert.AreEqual(NodeVariableType.Function, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateFunctionWithTextEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 23,
                Parent = null,
                StackFrame = null,
                Name = "v_function",
                TypeName = "function",
                Value = "55",
                Class = null,
                Text = "function(){...}"
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.IsNull(result.HexValue);
            Assert.AreEqual(variable.Text, result.StringValue);
            Assert.AreEqual(NodeExpressionType.Function | NodeExpressionType.Expandable, result.Type);
            Assert.AreEqual(NodeVariableType.Function, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateStringEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 24,
                Parent = null,
                StackFrame = null,
                Name = "v_string",
                TypeName = "string",
                Value = "string",
                Class = null,
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.IsNull(result.HexValue);
            Assert.AreEqual(variable.Value.Length, result.StringLength);
            Assert.AreEqual(variable.Value, result.StringValue);
            Assert.AreEqual(result.Type, NodeExpressionType.String);
            Assert.AreEqual(NodeVariableType.String, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateObjectEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 25,
                Parent = null,
                StackFrame = null,
                Name = "v_object",
                TypeName = "object",
                Value = null,
                Class = "Object",
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.IsNull(result.HexValue);
            Assert.AreEqual("{...}", result.StringValue);
            Assert.AreEqual(result.Type, NodeExpressionType.Expandable);
            Assert.AreEqual(NodeVariableType.Object, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateDateEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 27,
                Parent = null,
                StackFrame = null,
                Name = "v_date",
                TypeName = "object",
                Value = null,
                Class = "Date",
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.IsNull(result.HexValue);
            Assert.AreEqual(string.Format("{{{0}}}", variable.Class), result.StringValue);
            Assert.AreEqual(result.Type, NodeExpressionType.Expandable);
            Assert.AreEqual(NodeVariableType.Object, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateArrayEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 19,
                Parent = null,
                StackFrame = null,
                Name = "v_array",
                TypeName = "object",
                Value = null,
                Class = "Array",
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.IsNull(result.HexValue);
            Assert.AreEqual(string.Format("{{{0}}}", variable.Class), result.StringValue);
            Assert.AreEqual(result.Type, NodeExpressionType.Expandable);
            Assert.AreEqual(NodeVariableType.Object, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateErrorEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 26,
                Parent = null,
                StackFrame = null,
                Name = "v_error",
                TypeName = "error",
                Value = "Error: dsfdsf",
                Class = null,
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.IsNull(result.HexValue);
            Assert.AreEqual(variable.Value.Substring(7), result.StringValue);
            Assert.AreEqual(result.Type, NodeExpressionType.Expandable);
            Assert.AreEqual(NodeVariableType.Error, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateUnknownEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 26,
                Parent = null,
                StackFrame = null,
                Name = "v_unknown",
                TypeName = "unknown",
                Value = "Unknown",
                Class = null,
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.IsNull(result.HexValue);
            Assert.AreEqual(variable.Value, result.StringValue);
            Assert.AreEqual(NodeExpressionType.None, result.Type);
            Assert.AreEqual(NodeVariableType.Unknown, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateUnknownEvaluationResultWithEmptyValue()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 26,
                Parent = null,
                StackFrame = null,
                Name = "v_unknown",
                TypeName = "unknown",
                Value = null,
                Class = null,
                Text = "Unknown"
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(variable.Name, result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.IsNull(result.HexValue);
            Assert.AreEqual(variable.Text, result.StringValue);
            Assert.AreEqual(NodeExpressionType.None, result.Type);
            Assert.AreEqual(NodeVariableType.Unknown, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateEvaluationResultFromNullVariable()
        {
            // Arrange
            Exception exception = null;
            var factory = new EvaluationResultFactory();

            // Act
            try
            {
                factory.Create(null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ArgumentNullException));
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateArrayElementEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 19,
                Parent = new NodeEvaluationResult(0, null, null, "Object", "v_array", "v_array", NodeExpressionType.Expandable, null),
                StackFrame = null,
                Name = "0",
                TypeName = "number",
                Value = "0",
                Class = "Number",
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Format("[{0}]", variable.Name), result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(string.Format(@"{0}[{1}]", variable.Parent.Expression, variable.Name), result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.AreEqual(string.Format("0x{0:x}", int.Parse(variable.Value)), result.HexValue);
            Assert.AreEqual(variable.Value, result.StringValue);
            Assert.AreEqual(result.Type, NodeExpressionType.None);
            Assert.AreEqual(NodeVariableType.Number, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateObjectElementEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 19,
                Parent = new NodeEvaluationResult(0, null, null, "Object", "v_object", "v_object", NodeExpressionType.Expandable, null),
                StackFrame = null,
                Name = "m_number",
                TypeName = "number",
                Value = "0",
                Class = "Number",
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(string.Format(@"{0}.{1}", variable.Parent.Expression, variable.Name), result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.AreEqual(string.Format("0x{0:x}", int.Parse(variable.Value)), result.HexValue);
            Assert.AreEqual(variable.Value, result.StringValue);
            Assert.AreEqual(result.Type, NodeExpressionType.None);
            Assert.AreEqual(NodeVariableType.Number, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateObjectElementWithInvalidIdentifierEvaluationResult()
        {
            // Arrange
            var variable = new MockNodeVariable
            {
                Id = 19,
                Parent = new NodeEvaluationResult(0, null, null, "Object", "v_object", "v_object", NodeExpressionType.Expandable, null),
                StackFrame = null,
                Name = "123name",
                TypeName = "number",
                Value = "0",
                Class = "Number",
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(string.Format(@"{0}[""{1}""]", variable.Parent.Expression, variable.Name), result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.AreEqual(string.Format("0x{0:x}", int.Parse(variable.Value)), result.HexValue);
            Assert.AreEqual(variable.Value, result.StringValue);
            Assert.AreEqual(result.Type, NodeExpressionType.None);
            Assert.AreEqual(NodeVariableType.Number, result.TypeName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateEvaluationResultForPrototypeChild()
        {
            // Arrange
            const string parentName = "parent";
            var variable = new MockNodeVariable
            {
                Id = 19,
                Parent = new NodeEvaluationResult(
                    0,
                    null,
                    null,
                    "Object",
                    NodeVariableType.Prototype,
                    parentName + "." + NodeVariableType.Prototype,
                    NodeExpressionType.Expandable,
                    null),
                StackFrame = null,
                Name = "name",
                TypeName = "number",
                Value = "0",
                Class = "Number",
                Text = null
            };
            var factory = new EvaluationResultFactory();

            // Act
            NodeEvaluationResult result = factory.Create(variable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(variable.Name, result.Expression);
            Assert.IsNull(result.Frame);
            Assert.AreEqual(string.Format(@"{0}.{1}", parentName, variable.Name), result.FullName);
            Assert.AreEqual(variable.Id, result.Handle);
            Assert.AreEqual(string.Format("0x{0:x}", int.Parse(variable.Value)), result.HexValue);
            Assert.AreEqual(variable.Value, result.StringValue);
            Assert.AreEqual(result.Type, NodeExpressionType.None);
            Assert.AreEqual(NodeVariableType.Number, result.TypeName);
        }
    }
}