//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System.Collections.Generic;
using Microsoft.NodejsTools.Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger {
    [TestClass]
    public class ExceptionHandlerTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void SetNewDefaultExceptionHitTreatment() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();

            // Act
            bool updated = exceptionHandler.SetDefaultExceptionHitTreatment(ExceptionHitTreatment.BreakOnUnhandled);

            // Assert
            Assert.IsTrue(updated);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void SetSameDefaultExceptionHitTreatment() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();

            // Act
            bool updated = exceptionHandler.SetDefaultExceptionHitTreatment(ExceptionHitTreatment.BreakNever);

            // Assert
            Assert.IsFalse(updated);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void GetExceptionHitTreatmentForKnownError() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();

            // Act
            ExceptionHitTreatment result = exceptionHandler.GetExceptionHitTreatment("Error");

            // Assert
            Assert.AreEqual(ExceptionHitTreatment.BreakNever, result);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void GetExceptionHitTreatmentForUnknownError() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();

            // Act
            ExceptionHitTreatment result = exceptionHandler.GetExceptionHitTreatment("Error(MY)");

            // Assert
            Assert.AreEqual(ExceptionHitTreatment.BreakNever, result);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void GetExceptionHitTreatmentForUnknownErrorAfterChangingDefaults() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();
            const ExceptionHitTreatment newDefault = ExceptionHitTreatment.BreakAlways;

            // Act
            exceptionHandler.SetDefaultExceptionHitTreatment(newDefault);
            ExceptionHitTreatment result = exceptionHandler.GetExceptionHitTreatment("Error(MY)");

            // Assert
            Assert.AreEqual(newDefault, result);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void SetSameExceptionTreatments() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();
            const string exceptionName = "Error";
            const ExceptionHitTreatment newValue = ExceptionHitTreatment.BreakNever;
            ExceptionHitTreatment initial = exceptionHandler.GetExceptionHitTreatment(exceptionName);

            // Act
            bool updated = exceptionHandler.SetExceptionTreatments(new Dictionary<string, ExceptionHitTreatment> {
                { exceptionName, newValue }
            });
            ExceptionHitTreatment changed = exceptionHandler.GetExceptionHitTreatment(exceptionName);

            // Assert
            Assert.AreEqual(ExceptionHitTreatment.BreakNever, initial);
            Assert.IsFalse(updated);
            Assert.AreEqual(initial, changed);
            Assert.AreEqual(newValue, changed);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
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
            Assert.AreEqual(ExceptionHitTreatment.BreakNever, initial);
            Assert.IsTrue(updated);
            Assert.AreEqual(newValue, changed);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void ClearExceptionTreatments() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();
            const string exceptionName = "SyntaxError";
            exceptionHandler.SetExceptionTreatments(new Dictionary<string, ExceptionHitTreatment> {
                { exceptionName, ExceptionHitTreatment.BreakAlways }
            });
            ExceptionHitTreatment initial = exceptionHandler.GetExceptionHitTreatment(exceptionName);

            // Act
            bool updated = exceptionHandler.ClearExceptionTreatments(new Dictionary<string, ExceptionHitTreatment> {
                { exceptionName, ExceptionHitTreatment.BreakOnUnhandled }
            });
            ExceptionHitTreatment changed = exceptionHandler.GetExceptionHitTreatment(exceptionName);

            // Assert
            Assert.AreEqual(ExceptionHitTreatment.BreakAlways, initial);
            Assert.IsTrue(updated);
            Assert.AreEqual(ExceptionHitTreatment.BreakNever, changed);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void ResetExceptionTreatments() {
            // Arrange
            var exceptionHandler = new ExceptionHandler();
            exceptionHandler.SetExceptionTreatments(new Dictionary<string, ExceptionHitTreatment> {
                { "Node.js Exceptions", ExceptionHitTreatment.BreakAlways }
            });

            // Act
            bool updated = exceptionHandler.ResetExceptionTreatments();

            // Assert
            Assert.IsTrue(updated);
        }
    }
}