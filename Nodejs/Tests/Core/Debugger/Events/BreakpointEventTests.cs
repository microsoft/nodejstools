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

using Microsoft.NodejsTools.Debugger.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace NodejsTests.Debugger.Events {
    [TestClass]
    public class BreakpointEventTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateBreakpointEvent() {
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