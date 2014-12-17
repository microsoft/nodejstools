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

using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace NodejsTests.Debugger.Commands {
    [TestClass]
    public class BacktraceCommandTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateBacktraceCommand() {
            // Arrange
            const int commandId = 3;
            const int fromFrame = 0;
            const int toFrame = 7;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();

            // Act
            var backtraceCommand = new BacktraceCommand(commandId, resultFactoryMock.Object, fromFrame, toFrame);

            // Assert
            Assert.AreEqual(commandId, backtraceCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"backtrace\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"fromFrame\":{1},\"toFrame\":{2},\"inlineRefs\":true}}}}",
                    commandId, fromFrame, toFrame),
                backtraceCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void ProcessBacktraceForCallstackDepth() {
            // Arrange
            const int commandId = 3;
            const int fromFrame = 0;
            const int toFrame = 7;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            resultFactoryMock.Setup(factory => factory.Create(It.IsAny<INodeVariable>()))
                .Returns(() => new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null));
            var backtraceCommand = new BacktraceCommand(commandId, resultFactoryMock.Object, fromFrame, toFrame, true);
            JObject backtraceMessage = SerializationTestData.GetBacktraceResponse();

            // Act
            backtraceCommand.ProcessResponse(backtraceMessage);

            // Assert
            Assert.AreEqual(7, backtraceCommand.CallstackDepth);
            Assert.IsNotNull(backtraceCommand.Modules);
            Assert.AreEqual(0, backtraceCommand.Modules.Count);
            Assert.IsNotNull(backtraceCommand.StackFrames);
            Assert.AreEqual(0, backtraceCommand.StackFrames.Count);
            resultFactoryMock.Verify(factory => factory.Create(It.IsAny<INodeVariable>()), Times.Never);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void ProcessBacktraceForStackFrames() {
            // Arrange
            const int commandId = 3;
            const int fromFrame = 0;
            const int toFrame = 7;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            resultFactoryMock.Setup(factory => factory.Create(It.IsAny<INodeVariable>()))
                .Returns(() => new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null));
            var backtraceCommand = new BacktraceCommand(commandId, resultFactoryMock.Object, fromFrame, toFrame);
            JObject backtraceMessage = SerializationTestData.GetBacktraceResponse();

            // Act
            backtraceCommand.ProcessResponse(backtraceMessage);

            // Assert
            Assert.AreEqual(7, backtraceCommand.StackFrames.Count);
            NodeStackFrame firstFrame = backtraceCommand.StackFrames[0];
            Assert.AreEqual(NodeVariableType.AnonymousFunction, firstFrame.FunctionName);
            Assert.AreEqual(@"C:\Users\Tester\documents\visual studio 2012\Projects\NodejsApp1\NodejsApp1\server.js", firstFrame.FileName);
            Assert.AreEqual(22, firstFrame.Line);
            Assert.AreEqual(0, firstFrame.Column);
            Assert.AreEqual(15, firstFrame.Locals.Count);
            Assert.AreEqual(5, firstFrame.Parameters.Count);
            Assert.IsNotNull(backtraceCommand.Modules);
            Assert.AreEqual(3, backtraceCommand.Modules.Count);
            resultFactoryMock.Verify(factory => factory.Create(It.IsAny<INodeVariable>()), Times.Exactly(51));
        }
    }
}