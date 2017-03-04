// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace NodejsTests.Debugger.Commands
{
    [TestClass]
    public class SetVariableValueCommandTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetVariableValueCommand()
        {
            // Arrange
            const int commandId = 3;
            const int frameId = 1;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            var stackFrame = new NodeStackFrame(frameId);
            const string variableName = "port";
            const int handle = 40;

            // Act
            var setVariableValueCommand = new SetVariableValueCommand(commandId, resultFactoryMock.Object, stackFrame, variableName, handle);

            // Assert
            Assert.AreEqual(commandId, setVariableValueCommand.Id);
            Assert.AreEqual(
                string.Format("{{\"command\":\"setVariableValue\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"name\":\"{1}\",\"newValue\":{{\"handle\":{2}}},\"scope\":{{\"frameNumber\":{3},\"number\":0}}}}}}",
                    commandId, variableName, handle, frameId),
                setVariableValueCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void ProcessSetVariableValueResponse()
        {
            // Arrange
            const int commandId = 3;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            resultFactoryMock.Setup(factory => factory.Create(It.IsAny<INodeVariable>()))
                .Returns(() => new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null));
            var stackFrame = new NodeStackFrame(0);
            const string variableName = "port";
            const int handle = 40;
            var setVariableValueCommand = new SetVariableValueCommand(commandId, resultFactoryMock.Object, stackFrame, variableName, handle);

            // Act
            setVariableValueCommand.ProcessResponse(SerializationTestData.GetSetVariableValueResponse());

            // Assert
            Assert.AreEqual(commandId, setVariableValueCommand.Id);
            Assert.IsNotNull(setVariableValueCommand.Result);
            resultFactoryMock.Verify(factory => factory.Create(It.IsAny<INodeVariable>()), Times.Once);
        }
    }
}

