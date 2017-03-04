// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;
using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Commands
{
    [TestClass]
    public class ScriptsCommandTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateScriptsCommand()
        {
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

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateScriptsCommandWithOptionalParameters()
        {
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
                    commandId, includeSource.ToString().ToLower(CultureInfo.InvariantCulture), moduleId),
                scriptsCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void ProcessScriptsResponse()
        {
            // Arrange
            const int commandId = 3;
            const bool includeSource = true;
            const string nodejs = "node.js";
            var scriptsCommand = new ScriptsCommand(commandId, includeSource);

            // Act
            scriptsCommand.ProcessResponse(SerializationTestData.GetScriptsResponse());

            // Assert
            Assert.AreEqual(commandId, scriptsCommand.Id);
            Assert.IsNotNull(scriptsCommand.Modules);
            Assert.AreEqual(17, scriptsCommand.Modules.Count);
            NodeModule module = scriptsCommand.Modules[0];
            Assert.AreEqual(nodejs, module.Name);
            Assert.AreEqual(nodejs, module.Source);
            Assert.AreEqual(nodejs, module.FileName);
            Assert.AreEqual(nodejs, module.JavaScriptFileName);
            Assert.AreEqual(17, module.Id);
        }
    }
}

