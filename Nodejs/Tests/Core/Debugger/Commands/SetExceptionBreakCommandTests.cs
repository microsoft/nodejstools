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

using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.Commands
{
    [TestClass]
    public class SetExceptionBreakCommandTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetExceptionBreakCommand()
        {
            // Arrange
            const int commandId = 3;
            const bool uncaught = true;
            const bool enabled = true;

            // Act
            var setExceptionBreakCommand = new SetExceptionBreakCommand(commandId, uncaught, enabled);

            // Assert
            Assert.AreEqual(commandId, setExceptionBreakCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setexceptionbreak\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"type\":\"uncaught\",\"enabled\":{1}}}}}",
                    commandId, enabled.ToString().ToLower()),
                setExceptionBreakCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetExceptionBreakCommandForAll()
        {
            // Arrange
            const int commandId = 3;
            const bool uncaught = false;
            const bool enabled = false;

            // Act
            var setExceptionBreakCommand = new SetExceptionBreakCommand(commandId, uncaught, enabled);

            // Assert
            Assert.AreEqual(commandId, setExceptionBreakCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setexceptionbreak\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"type\":\"all\",\"enabled\":{1}}}}}",
                    commandId, enabled.ToString().ToLower()),
                setExceptionBreakCommand.ToString());
        }
    }
}