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

using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Commands {
    [TestClass]
    public class ChangeLiveCommandTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateChangeLiveCommand() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 5;
            const string fileName = "fileName.js";
            const string source = "source";
            string wrappedSource = string.Format("{0}{1}{2}", NodeConstants.ScriptWrapBegin, source, NodeConstants.ScriptWrapEnd.Replace("\n", @"\n"));
            var module = new NodeModule(moduleId, fileName) { Source = source };

            // Act
            var changeLiveCommand = new ChangeLiveCommand(commandId, module);

            // Assert
            Assert.AreEqual(commandId, changeLiveCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"changelive\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"script_id\":{1},\"new_source\":\"{2}\",\"preview_only\":false}}}}",
                    commandId, moduleId, wrappedSource),
                changeLiveCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void ProcessChangeLiveResponse() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 5;
            const string fileName = "fileName.js";
            var module = new NodeModule(moduleId, fileName);
            var changeLiveCommand = new ChangeLiveCommand(commandId, module);

            // Act
            changeLiveCommand.ProcessResponse(SerializationTestData.GetChangeLiveResponse());

            // Assert
            Assert.AreEqual(commandId, changeLiveCommand.Id);
            Assert.IsTrue(changeLiveCommand.Updated);
            Assert.IsTrue(changeLiveCommand.StackModified);
        }
    }
}