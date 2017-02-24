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
    public class BreakpointEventTests
    {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateBreakpointEvent()
        {
            // Arrange
            JObject message = JObject.Parse(Resources.NodeBreakpointResponse);

            // Act
            var breakpointEvent = new BreakpointEvent(message);

            // Assert
            Assert.IsNotNull(breakpointEvent.Breakpoints);
            Assert.AreEqual(1, breakpointEvent.Breakpoints.Count);
            Assert.AreEqual(2, breakpointEvent.Breakpoints[0]);
            Assert.AreEqual(1, breakpointEvent.Line);
            Assert.AreEqual(0, breakpointEvent.Column);
            Assert.IsNotNull(breakpointEvent.Module);
            Assert.AreEqual("server.js", breakpointEvent.Module.Name);
            Assert.AreEqual(false, breakpointEvent.Running);
        }
    }
}