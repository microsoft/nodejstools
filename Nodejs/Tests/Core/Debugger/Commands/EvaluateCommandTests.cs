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
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace NodejsTests.Debugger.Commands {
    [TestClass]
    public class EvaluateCommandTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateEvaluateCommand() {
            // Arrange
            const int commandId = 3;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            const string expression = "expression";

            // Act
            var evaluateCommand = new EvaluateCommand(commandId, resultFactoryMock.Object, expression);

            // Assert
            Assert.AreEqual(commandId, evaluateCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"evaluate\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"expression\":\"{1}\",\"frame\":0,\"global\":false,\"disable_break\":true,\"maxStringLength\":-1}}}}",
                    commandId, expression),
                evaluateCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateEvaluateCommandWithVariableId() {
            // Arrange
            const int commandId = 3;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            const int variableId = 2;

            // Act
            var evaluateCommand = new EvaluateCommand(commandId, resultFactoryMock.Object, variableId);

            // Assert
            Assert.AreEqual(commandId, evaluateCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"evaluate\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"expression\":\"variable.toString()\",\"frame\":0,\"global\":false,\"disable_break\":true,\"additional_context\":[{{\"name\":\"variable\",\"handle\":{1}}}],\"maxStringLength\":-1}}}}",
                    commandId, variableId),
                evaluateCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void ProcessEvaluateResponse() {
            // Arrange
            const int commandId = 3;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            resultFactoryMock.Setup(factory => factory.Create(It.IsAny<INodeVariable>()))
                .Returns(() => new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null));
            const string expression = "expression";
            var stackFrame = new NodeStackFrame(0);
            var evaluateCommand = new EvaluateCommand(commandId, resultFactoryMock.Object, expression, stackFrame);

            // Act
            evaluateCommand.ProcessResponse(SerializationTestData.GetEvaluateResponse());

            // Assert
            Assert.AreEqual(commandId, evaluateCommand.Id);
            Assert.IsNotNull(evaluateCommand.Result);
            resultFactoryMock.Verify(factory => factory.Create(It.IsAny<INodeVariable>()), Times.Once);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void ProcessEvaluateResponseWithReferenceError() {
            // Arrange
            const int commandId = 3;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            resultFactoryMock.Setup(factory => factory.Create(It.IsAny<INodeVariable>()));
            const string expression = "hello";
            var stackFrame = new NodeStackFrame(0);
            var evaluateCommand = new EvaluateCommand(commandId, resultFactoryMock.Object, expression, stackFrame);
            Exception exception = null;

            // Act
            try {
                evaluateCommand.ProcessResponse(SerializationTestData.GetEvaluateResponseWithReferenceError());
            } catch (Exception e) {
                exception = e;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof (DebuggerCommandException));
            Assert.AreEqual("ReferenceError: hello is not defined", exception.Message);
            Assert.IsNull(evaluateCommand.Result);
            resultFactoryMock.Verify(factory => factory.Create(It.IsAny<INodeVariable>()), Times.Never);
        }
    }
}