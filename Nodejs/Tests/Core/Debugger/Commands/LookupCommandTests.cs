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
using Newtonsoft.Json;

namespace NodejsTests.Debugger.Commands {
    [TestClass]
    public class LookupCommandTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateLookupCommand() {
            // Arrange
            const int commandId = 3;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            var handles = new[] { 25 };

            // Act
            var lookupCommand = new LookupCommand(commandId, resultFactoryMock.Object, handles);

            // Assert
            Assert.AreEqual(commandId, lookupCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"lookup\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"handles\":{1},\"includeSource\":false}}}}",
                    commandId, JsonConvert.SerializeObject(handles)),
                lookupCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void ProcessLookupResponse() {
            // Arrange
            const int commandId = 3;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            resultFactoryMock.Setup(factory => factory.Create(It.IsAny<INodeVariable>()))
                .Returns(() => new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null));
            const int handle = 25;
            var handles = new[] { handle };
            var lookupCommand = new LookupCommand(commandId, resultFactoryMock.Object, handles);

            // Act
            lookupCommand.ProcessResponse(SerializationTestData.GetLookupResponse());

            // Assert
            Assert.AreEqual(commandId, lookupCommand.Id);
            Assert.IsNotNull(lookupCommand.Results);
            Assert.IsTrue(lookupCommand.Results.ContainsKey(handle));
            Assert.IsNotNull(lookupCommand.Results[handle]);
            resultFactoryMock.Verify(factory => factory.Create(It.IsAny<INodeVariable>()), Times.AtLeastOnce);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void ProcessLookupResponseWithPrimitiveObject() {
            // Arrange
            const int commandId = 3;
            var resultFactoryMock = new Mock<IEvaluationResultFactory>();
            resultFactoryMock.Setup(factory => factory.Create(It.IsAny<INodeVariable>()))
                .Returns(() => new NodeEvaluationResult(0, null, null, null, null, null, NodeExpressionType.None, null));
            const int handle = 9;
            var handles = new[] { handle };
            var lookupCommand = new LookupCommand(commandId, resultFactoryMock.Object, handles);

            // Act
            lookupCommand.ProcessResponse(SerializationTestData.GetLookupResponseWithPrimitiveObject());

            // Assert
            Assert.AreEqual(commandId, lookupCommand.Id);
            Assert.IsNotNull(lookupCommand.Results);
            Assert.IsTrue(lookupCommand.Results.ContainsKey(handle));
            Assert.IsNotNull(lookupCommand.Results[handle]);
            resultFactoryMock.Verify(factory => factory.Create(It.IsAny<INodeVariable>()), Times.Once);
        }
    }
}