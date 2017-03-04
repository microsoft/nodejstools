// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Commands
{
    [TestClass]
    public class ClearBreakpointCommandTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateClearBreakpointCommand()
        {
            // Arrange
            const int commandId = 3;
            const int breakpointId = 5;

            // Act
            var clearBreakpointCommand = new ClearBreakpointCommand(commandId, breakpointId);

            // Assert
            Assert.AreEqual(commandId, clearBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"clearbreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"breakpoint\":{1}}}}}",
                    commandId, breakpointId),
                clearBreakpointCommand.ToString());
        }
    }
}

