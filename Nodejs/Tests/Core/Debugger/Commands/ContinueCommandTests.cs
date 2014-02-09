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
    public class ContinueCommandTests {
        [TestMethod]
        public void CreateContinueCommand() {
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
                    commandId, stepping.ToString().ToLower()),
                continueCommand.ToString());
        }

        [TestMethod]
        public void CreateContinueCommandWithOptionalParameters() {
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
                    commandId, stepping.ToString().ToLower(), stepCount),
                continueCommand.ToString());
        }
    }
}