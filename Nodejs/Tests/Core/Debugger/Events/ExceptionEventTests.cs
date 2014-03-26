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
    public class ExceptionEventTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateExceptionEvent() {
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