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