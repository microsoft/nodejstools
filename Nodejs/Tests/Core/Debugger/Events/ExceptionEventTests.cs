// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace NodejsTests.Debugger.Events
{
    [TestClass]
    public class ExceptionEventTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateExceptionEvent()
        {
            // Arrange
            JObject message = JObject.Parse(Resources.NodeExceptionResponse);

            // Act
            var exceptionEvent = new ExceptionEvent(message);

            // Assert
            Assert.AreEqual(0, exceptionEvent.ExceptionId);
            Assert.AreEqual(3, exceptionEvent.Line);
            Assert.AreEqual(6, exceptionEvent.Column);
            Assert.AreEqual("Error", exceptionEvent.ExceptionName);
            Assert.IsNull(exceptionEvent.ErrorNumber);
            Assert.AreEqual("Error: Mission failed!", exceptionEvent.Description);
            Assert.AreEqual(false, exceptionEvent.Uncaught);
            Assert.IsNotNull(exceptionEvent.Module);
            Assert.AreEqual("server.js", exceptionEvent.Module.Name);
            Assert.AreEqual(false, exceptionEvent.Running);
        }
    }
}

