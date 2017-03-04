// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Commands
{
    [TestClass]
    public class SetExceptionBreakCommandTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetExceptionBreakCommand()
        {
            // Arrange
            const int commandId = 3;
            const bool uncaught = true;
            const bool enabled = true;

            // Act
            var setExceptionBreakCommand = new SetExceptionBreakCommand(commandId, uncaught, enabled);

            // Assert
            Assert.AreEqual(commandId, setExceptionBreakCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setexceptionbreak\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"type\":\"uncaught\",\"enabled\":{1}}}}}",
                    commandId, enabled.ToString().ToLower()),
                setExceptionBreakCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetExceptionBreakCommandForAll()
        {
            // Arrange
            const int commandId = 3;
            const bool uncaught = false;
            const bool enabled = false;

            // Act
            var setExceptionBreakCommand = new SetExceptionBreakCommand(commandId, uncaught, enabled);

            // Assert
            Assert.AreEqual(commandId, setExceptionBreakCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setexceptionbreak\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"type\":\"all\",\"enabled\":{1}}}}}",
                    commandId, enabled.ToString().ToLower()),
                setExceptionBreakCommand.ToString());
        }
    }
}

