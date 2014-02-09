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

using System.Collections.Generic;
using Microsoft.NodejsTools.Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger {
    [TestClass]
    public class ExceptionHandlerTests {
        [TestMethod]
        public void SetNewDefaultExceptionHitTreatment() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();

            // Act
            bool updated = exceptionHandler.SetDefaultExceptionHitTreatment(ExceptionHitTreatment.BreakOnUnhandled);

            // Assert
            Assert.IsTrue(updated);
        }

        [TestMethod]
        public void SetSameDefaultExceptionHitTreatment() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();

            // Act
            bool updated = exceptionHandler.SetDefaultExceptionHitTreatment(ExceptionHitTreatment.BreakAlways);

            // Assert
            Assert.IsFalse(updated);
        }

        [TestMethod]
        public void GetExceptionHitTreatmentForKnownError() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();

            // Act
            ExceptionHitTreatment result = exceptionHandler.GetExceptionHitTreatment("Error");

            // Assert
            Assert.AreEqual(ExceptionHitTreatment.BreakAlways, result);
        }

        [TestMethod]
        public void GetExceptionHitTreatmentForUnknownError() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();

            // Act
            ExceptionHitTreatment result = exceptionHandler.GetExceptionHitTreatment("Error(MY)");

            // Assert
            Assert.AreEqual(ExceptionHitTreatment.BreakAlways, result);
        }

        [TestMethod]
        public void GetExceptionHitTreatmentForUnknownErrorAfterChangingDefaults() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();
            const ExceptionHitTreatment newDefault = ExceptionHitTreatment.BreakOnUnhandled;

            // Act
            exceptionHandler.SetDefaultExceptionHitTreatment(newDefault);
            ExceptionHitTreatment result = exceptionHandler.GetExceptionHitTreatment("Error(MY)");

            // Assert
            Assert.AreEqual(newDefault, result);
        }

        [TestMethod]
        public void SetSameExceptionTreatments() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();
            const string exceptionName = "Error";
            const ExceptionHitTreatment newValue = ExceptionHitTreatment.BreakAlways;
            ExceptionHitTreatment initial = exceptionHandler.GetExceptionHitTreatment(exceptionName);

            // Act
            bool updated = exceptionHandler.SetExceptionTreatments(new Dictionary<string, ExceptionHitTreatment> {
                { exceptionName, newValue }
            });
            ExceptionHitTreatment changed = exceptionHandler.GetExceptionHitTreatment(exceptionName);

            // Assert
            Assert.AreEqual(ExceptionHitTreatment.BreakAlways, initial);
            Assert.IsFalse(updated);
            Assert.AreEqual(initial, changed);
            Assert.AreEqual(newValue, changed);
        }

        [TestMethod]
        public void SetNewExceptionTreatments() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();
            const string exceptionName = "Error";
            const ExceptionHitTreatment newValue = ExceptionHitTreatment.BreakOnUnhandled;
            ExceptionHitTreatment initial = exceptionHandler.GetExceptionHitTreatment(exceptionName);

            // Act
            bool updated = exceptionHandler.SetExceptionTreatments(new Dictionary<string, ExceptionHitTreatment> {
                { exceptionName, newValue }
            });
            ExceptionHitTreatment changed = exceptionHandler.GetExceptionHitTreatment(exceptionName);

            // Assert
            Assert.AreEqual(ExceptionHitTreatment.BreakAlways, initial);
            Assert.IsTrue(updated);
            Assert.AreEqual(newValue, changed);
        }

        [TestMethod]
        public void ClearExceptionTreatments() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();
            const string exceptionName = "SyntaxError";
            ExceptionHitTreatment initial = exceptionHandler.GetExceptionHitTreatment(exceptionName);

            // Act
            bool updated = exceptionHandler.ClearExceptionThreatments(new Dictionary<string, ExceptionHitTreatment> {
                { exceptionName, ExceptionHitTreatment.BreakOnUnhandled }
            });
            ExceptionHitTreatment changed = exceptionHandler.GetExceptionHitTreatment(exceptionName);

            // Assert
            Assert.AreEqual(ExceptionHitTreatment.BreakNever, initial);
            Assert.IsTrue(updated);
            Assert.AreEqual(ExceptionHitTreatment.BreakAlways, changed);
        }

        [TestMethod]
        public void ResetExceptionTreatments() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();

            // Act
            bool updated = exceptionHandler.ResetExceptionTreatments();

            // Assert
            Assert.IsTrue(updated);
        }
    }
}