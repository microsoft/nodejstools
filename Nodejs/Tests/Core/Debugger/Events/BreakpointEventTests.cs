// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace NodejsTests.Debugger.Events
{
    [TestClass]
    public class BreakpointEventTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateBreakpointEvent()
        {
            // Arrange
            JObject message = JObject.Parse(Resources.NodeBreakpointResponse);

            // Act
            var breakpointEvent = new BreakpointEvent(message);

            // Assert
            Assert.IsNotNull(breakpointEvent.Breakpoints);
            Assert.AreEqual(1, breakpointEvent.Breakpoints.Count);
            Assert.AreEqual(2, breakpointEvent.Breakpoints[0]);
            Assert.AreEqual(1, breakpointEvent.Line);
            Assert.AreEqual(0, breakpointEvent.Column);
            Assert.IsNotNull(breakpointEvent.Module);
            Assert.AreEqual("server.js", breakpointEvent.Module.Name);
            Assert.AreEqual(false, breakpointEvent.Running);
        }
    }
}

