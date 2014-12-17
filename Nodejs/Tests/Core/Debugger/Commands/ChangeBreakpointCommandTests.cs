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

namespace NodejsTests.Debugger.Commands {
    [TestClass]
    public class ChangeBreakpointCommandTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateChangeBreakpointCommand() {
            // Arrange
            const int commandId = 3;
            const int breakpointId = 5;

            // Act
            var changeBreakpointCommand = new ChangeBreakpointCommand(commandId, breakpointId);

            // Assert
            Assert.AreEqual(commandId, changeBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"changebreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"breakpoint\":{1}}}}}",
                    commandId, breakpointId),
                changeBreakpointCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateChangeBreakpointCommandWithOptionalParameters() {
            // Arrange
            const int commandId = 3;
            const int breakpointId = 5;
            const bool enabled = true;
            const string condition = "value > 5";
            const int ignoreCount = 2;

            // Act
            var changeBreakpointCommand = new ChangeBreakpointCommand(commandId, breakpointId, enabled, condition, ignoreCount);

            // Assert
            Assert.AreEqual(commandId, changeBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"changebreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"breakpoint\":{1},\"enabled\":{2},\"condition\":\"{3}\",\"ignoreCount\":{4}}}}}",
                    commandId, breakpointId, enabled.ToString().ToLower(), condition, ignoreCount),
                changeBreakpointCommand.ToString());
        }
    }
}