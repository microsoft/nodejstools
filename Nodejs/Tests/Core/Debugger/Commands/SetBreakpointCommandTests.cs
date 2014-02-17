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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Commands {
    [TestClass]
    public class SetBreakpointCommandTests {
        [TestMethod]
        public void CreateSetBreakpointCommand() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 5;
            const int line = 2;
            const string fileName = "module.js";
            var module = new NodeModule(moduleId, fileName, fileName);
            var breakOn = new BreakOn(BreakOnKind.Equal, 2);
            var breakpoint = new NodeBreakpoint(null, null, null, line, 0, true, breakOn, null);

            // Act
            var setBreakpointCommand = new SetBreakpointCommand(commandId, module, breakpoint);

            // Assert
            Assert.AreEqual(commandId, setBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setbreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"line\":{1},\"column\":{2},\"type\":\"scriptId\",\"target\":{3},\"ignoreCount\":1}}}}",
                    commandId, line - 1, 0, module.ModuleId),
                setBreakpointCommand.ToString());
        }

        [TestMethod]
        public void CreateSetBreakpointCommandOnFirstLine() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 5;
            const int line = 1;
            const string fileName = "module.js";
            var module = new NodeModule(moduleId, fileName, fileName);
            var breakOn = new BreakOn(BreakOnKind.Equal, 2);
            var breakpoint = new NodeBreakpoint(null, null, null, line, 0, true, breakOn, null);

            // Act
            var setBreakpointCommand = new SetBreakpointCommand(commandId, module, breakpoint);

            // Assert
            Assert.AreEqual(commandId, setBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setbreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"line\":{1},\"column\":{2},\"type\":\"scriptId\",\"target\":{3},\"ignoreCount\":1}}}}",
                    commandId, line - 1, 1, module.ModuleId),
                setBreakpointCommand.ToString());
        }

        [TestMethod]
        public void CreateSetBreakpointCommandOnFile() {
            // Arrange
            const int commandId = 3;
            const int line = 2;
            const string fileName = "module.js";
            var breakOn = new BreakOn(BreakOnKind.Equal, 2);
            var breakpoint = new NodeBreakpoint(null, fileName, null, line, 0, true, breakOn, null);

            // Act
            var setBreakpointCommand = new SetBreakpointCommand(commandId, null, breakpoint);

            // Assert
            Assert.AreEqual(commandId, setBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setbreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"line\":{1},\"column\":{2},\"type\":\"scriptRegExp\",\"target\":\"^[Mm][Oo][Dd][Uu][Ll][Ee]\\\\.[Jj][Ss]$\",\"ignoreCount\":1}}}}",
                    commandId, line - 1, 0),
                setBreakpointCommand.ToString());
        }

        [TestMethod]
        public void CreateSetBreakpointCommandWithoutPredicate() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 5;
            const int line = 2;
            const string fileName = "module.js";
            var module = new NodeModule(moduleId, fileName, fileName);
            var breakOn = new BreakOn(BreakOnKind.Equal, 2);
            var breakpoint = new NodeBreakpoint(null, null, null, line, 0, true, breakOn, null);

            // Act
            var setBreakpointCommand = new SetBreakpointCommand(commandId, module, breakpoint, true);

            // Assert
            Assert.AreEqual(commandId, setBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setbreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"line\":{1},\"column\":{2},\"type\":\"scriptId\",\"target\":{3}}}}}",
                    commandId, line - 1, 0, moduleId),
                setBreakpointCommand.ToString());
        }
    }
}