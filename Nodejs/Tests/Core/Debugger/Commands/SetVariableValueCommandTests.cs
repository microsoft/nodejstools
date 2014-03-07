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

using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace NodejsTests.Debugger.Commands {
    [TestClass]
    public class SetVariableValueCommandTests {
        [TestMethod]
        public void CreateSetVariableValueCommand() {
            // Arrange
            const int commandId = 3;
            const int frameId = 1;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            var stackframe = new NodeStackFrame(null, null, null, 0, 0, 0, 0, frameId);
            const string variableName = "port";
            const int handle = 40;

            // Act
            var setVariableValueCommand = new SetVariableValueCommand(commandId, resultFactoryMock.Object, stackframe, variableName, handle);

            // Assert
            Assert.AreEqual(commandId, setVariableValueCommand.Id);
            Assert.AreEqual(
                string.Format("{{\"command\":\"setVariableValue\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"name\":\"{1}\",\"newValue\":{{\"handle\":{2}}},\"scope\":{{\"frameNumber\":{3},\"number\":0}}}}}}",
                    commandId, variableName, handle, frameId),
                setVariableValueCommand.ToString());
        }

        [TestMethod]
        public void ProcessSetVariableValueResponse() {
            // Arrange
            const int commandId = 3;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            resultFactoryMock.Setup(factory => factory.Create(It.IsAny<INodeVariable>()))
                .Returns(() => new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null));
            var stackframe = new NodeStackFrame(null, null, null, 0, 0, 0, 0, 0);
            const string variableName = "port";
            const int handle = 40;
            var setVariableValueCommand = new SetVariableValueCommand(commandId, resultFactoryMock.Object, stackframe, variableName, handle);

            // Act
            setVariableValueCommand.ProcessResponse(SerializationTestData.GetSetVariableValueResponse());

            // Assert
            Assert.AreEqual(commandId, setVariableValueCommand.Id);
            Assert.IsNotNull(setVariableValueCommand.Result);
            resultFactoryMock.Verify(factory => factory.Create(It.IsAny<INodeVariable>()), Times.Once);
        }
    }
}