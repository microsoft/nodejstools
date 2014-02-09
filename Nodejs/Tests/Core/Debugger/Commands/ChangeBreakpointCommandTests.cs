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

using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Commands {
    [TestClass]
    public class ChangeBreakpointCommandTests {
        [TestMethod]
        public void CreateChangeBreakpointCommand() {
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

        [TestMethod]
        public void CreateChangeBreakpointCommandWithOptionalParameters() {
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
                    commandId, breakpointId, enabled.ToString().ToLower(), condition, ignoreCount),
                changeBreakpointCommand.ToString());
        }
    }
}