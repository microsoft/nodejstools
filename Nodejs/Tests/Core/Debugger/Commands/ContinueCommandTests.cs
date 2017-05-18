// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace NodejsTests.Debugger.Commands
{
    [TestClass]
    public class ContinueCommandTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateContinueCommand()
        {
            // Arrange
            const int commandId = 3;
            const SteppingKind stepping = SteppingKind.Out;

            // Act
            var continueCommand = new ContinueCommand(commandId, stepping);

            // Assert
            Assert.AreEqual(commandId, continueCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"continue\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"stepaction\":\"{1}\",\"stepcount\":1}}}}",
                    commandId, stepping.ToString().ToLower(CultureInfo.InvariantCulture)),
                continueCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateContinueCommandWithOptionalParameters()
        {
            // Arrange
            const int commandId = 3;
            const SteppingKind stepping = SteppingKind.Out;
            const int stepCount = 3;

            // Act
            var continueCommand = new ContinueCommand(commandId, stepping, stepCount);

            // Assert
            Assert.AreEqual(commandId, continueCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"continue\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"stepaction\":\"{1}\",\"stepcount\":{2}}}}}",
                    commandId, stepping.ToString().ToLower(CultureInfo.InvariantCulture), stepCount),
                continueCommand.ToString());
        }
    }
}

