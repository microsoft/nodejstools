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
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace NodejsTests.Debugger.Events {
    [TestClass]
    public class CompileScriptEventTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
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