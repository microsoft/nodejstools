// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Commands
{
    [TestClass]
    public class SuspendCommandTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSuspendCommand()
        {
            // Arrange
            const int commandId = 3;

            // Act
            var suspendCommand = new SuspendCommand(commandId);

            // Assert
            Assert.AreEqual(commandId, suspendCommand.Id);
            Assert.AreEqual(
                string.Format("{{\"command\":\"suspend\",\"seq\":{0},\"type\":\"request\",\"arguments\":null}}", commandId),
                suspendCommand.ToString());
        }
    }
}

