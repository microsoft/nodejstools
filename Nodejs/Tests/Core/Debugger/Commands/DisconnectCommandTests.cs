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
    public class DisconnectCommandTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateDisconnectCommand() {
            // Arrange
            const int commandId = 3;

            // Act
            var disconnectCommand = new DisconnectCommand(commandId);

            // Assert
            Assert.AreEqual(commandId, disconnectCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"disconnect\",\"seq\":{0},\"type\":\"request\",\"arguments\":null}}",
                    commandId),
                disconnectCommand.ToString());
        }
    }
}