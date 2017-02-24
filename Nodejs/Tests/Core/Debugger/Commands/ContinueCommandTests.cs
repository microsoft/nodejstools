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
using System.Globalization;

namespace NodejsTests.Debugger.Commands
{
    [TestClass]
    public class ContinueCommandTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateContinueCommand()
        {
            // Arrange
            const int commandId = 3;
            const SteppingKind stepping = SteppingKind.Out;

            // Act
            var continueCommand = new ContinueCommand(commandId, stepping);

            // Assert
            Assert.AreEqual(commandId, continueCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"continue\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"stepaction\":\"{1}\",\"stepcount\":1}}}}",
                    commandId, stepping.ToString().ToLower(CultureInfo.InvariantCulture)),
                continueCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateContinueCommandWithOptionalParameters()
        {
            // Arrange
            const int commandId = 3;
            const SteppingKind stepping = SteppingKind.Out;
            const int stepCount = 3;

            // Act
            var continueCommand = new ContinueCommand(commandId, stepping, stepCount);

            // Assert
            Assert.AreEqual(commandId, continueCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"continue\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"stepaction\":\"{1}\",\"stepcount\":{2}}}}}",
                    commandId, stepping.ToString().ToLower(CultureInfo.InvariantCulture), stepCount),
                continueCommand.ToString());
        }
    }
}