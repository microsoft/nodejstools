// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger.Events;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace NodejsTests.Debugger.Events
{
    [TestClass]
    public class CompileScriptEventTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateCompileScriptEvent()
        {
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

        [TestMethod, Priority(0), TestCategory("Debugging")]
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

