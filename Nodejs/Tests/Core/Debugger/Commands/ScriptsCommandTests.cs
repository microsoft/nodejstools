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
    public class ScriptsCommandTests {
        [TestMethod]
        public void CreateScriptsCommand() {
            // Arrange
            const int commandId = 3;
            const bool includeSource = true;

            // Act
            var scriptsCommand = new ScriptsCommand(commandId, includeSource);

            // Assert
            Assert.AreEqual(commandId, scriptsCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"scripts\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"includeSource\":{1}}}}}",
                    commandId, includeSource.ToString().ToLower()),
                scriptsCommand.ToString());
        }

        [TestMethod]
        public void CreateScriptsCommandWithOptionalParameters() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 5;
            const bool includeSource = false;

            // Act
            var scriptsCommand = new ScriptsCommand(commandId, includeSource, moduleId);

            // Assert
            Assert.AreEqual(commandId, scriptsCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"scripts\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"includeSource\":{1},\"ids\":[{2}]}}}}",
                    commandId, includeSource.ToString().ToLower(), moduleId),
                scriptsCommand.ToString());
        }

        [TestMethod]
        public void ProcessScriptsResponse() {
            // Arrange
            const int commandId = 3;
            const bool includeSource = true;
            var scriptsCommand = new ScriptsCommand(commandId, includeSource);

            // Act
            scriptsCommand.ProcessResponse(SerializationTestData.GetScriptsResponse());

            // Assert
            Assert.AreEqual(commandId, scriptsCommand.Id);
            Assert.IsNotNull(scriptsCommand.Modules);
            Assert.AreEqual(17, scriptsCommand.Modules.Count);
            NodeModule module = scriptsCommand.Modules[0];
            Assert.AreEqual("node.js", module.Name);
            Assert.AreEqual(17, module.ModuleId);
        }
    }
}