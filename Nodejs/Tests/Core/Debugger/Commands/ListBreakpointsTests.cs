// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Commands
{
    [TestClass]
    public class ListBreakpointsTests
    {
        [TestMethod, Priority(0)]
        public void CreateListBreakpointsCommand()
        {
            // Arrange
            const int commandId = 3;

            // Act
            var listBreakpointsCommand = new ListBreakpointsCommand(commandId);

            // Assert
            Assert.AreEqual(commandId, listBreakpointsCommand.Id);
            Assert.AreEqual(
                string.Format("{{\"command\":\"listbreakpoints\",\"seq\":{0},\"type\":\"request\",\"arguments\":null}}", commandId),
                listBreakpointsCommand.ToString());
        }
    }
}

