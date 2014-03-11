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
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace NodejsTests.Debugger.Events {
    [TestClass]
    public class CompileScriptEventTests {
        [TestMethod]
        public void CreateCompileScriptEvent() {
            // Arrange
            JObject message = JObject.Parse(Resources.NodeCompileScriptResponse);

            // Act
            var compileScriptEvent = new CompileScriptEvent(message);

            // Assert
            Assert.IsNotNull(compileScriptEvent.Module);
            Assert.AreEqual(34, compileScriptEvent.Module.Id);
            Assert.AreEqual("http.js", compileScriptEvent.Module.Name);
            Assert.AreEqual(true, compileScriptEvent.Running);
        }

        [TestMethod]
        public void CreateCompileScriptEventWithNullScriptName()
        {
            // Arrange
            JObject message = JObject.Parse(Resources.NodeCompileScriptResponseWithNullScriptName);

            // Act
            var compileScriptEvent = new CompileScriptEvent(message);

            // Assert
            Assert.IsNotNull(compileScriptEvent.Module);
            Assert.AreEqual(172, compileScriptEvent.Module.Id);
            Assert.AreEqual(NodeVariableType.UnknownModule, compileScriptEvent.Module.Name);
            Assert.AreEqual(true, compileScriptEvent.Running);
        }
    }
}