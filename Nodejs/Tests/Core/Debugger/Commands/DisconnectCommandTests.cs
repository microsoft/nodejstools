// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Commands
{
    [TestClass]
    public class DisconnectCommandTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateDisconnectCommand()
        {
            // Arrange
            const int commandId = 3;

            // Act
            var disconnectCommand = new DisconnectCommand(commandId);

            // Assert
            Assert.AreEqual(commandId, disconnectCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"disconnect\",\"seq\":{0},\"type\":\"request\",\"arguments\":null}}",
                    commandId),
                disconnectCommand.ToString());
        }
    }
}

