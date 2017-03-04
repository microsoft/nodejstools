// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace NodejsTests.Debugger.Commands
{
    [TestClass]
    public class ChangeBreakpointCommandTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateChangeBreakpointCommand()
        {
            // Arrange
            const int commandId = 3;
            const int breakpointId = 5;

            // Act
            var changeBreakpointCommand = new ChangeBreakpointCommand(commandId, breakpointId);

            // Assert
            Assert.AreEqual(commandId, changeBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"changebreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"breakpoint\":{1}}}}}",
                    commandId, breakpointId),
                changeBreakpointCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateChangeBreakpointCommandWithOptionalParameters()
        {
            // Arrange
            const int commandId = 3;
            const int breakpointId = 5;
            const bool enabled = true;
            const string condition = "value > 5";
            const int ignoreCount = 2;

            // Act
            var changeBreakpointCommand = new ChangeBreakpointCommand(commandId, breakpointId, enabled, condition, ignoreCount);

            // Assert
            Assert.AreEqual(commandId, changeBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"changebreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"breakpoint\":{1},\"enabled\":{2},\"condition\":\"{3}\",\"ignoreCount\":{4}}}}}",
                    commandId, breakpointId, enabled.ToString().ToLower(CultureInfo.InvariantCulture), condition, ignoreCount),
                changeBreakpointCommand.ToString());
        }
    }
}

