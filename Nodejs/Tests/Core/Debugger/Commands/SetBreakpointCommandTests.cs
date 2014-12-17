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

using System;
using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace NodejsTests.Debugger.Commands {
    [TestClass]
    public class SetBreakpointCommandTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetBreakpointCommand() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 5;
            const int line = 2;
            const int column = 0;
            const string fileName = "module.js";
            var module = new NodeModule(moduleId, fileName);
            var breakOn = new BreakOn(BreakOnKind.Equal, 2);
            var position = new FilePosition(fileName, line, column);
            var breakpoint = new NodeBreakpoint(null, position, true, breakOn, null);

            // Act
            var setBreakpointCommand = new SetBreakpointCommand(commandId, module, breakpoint, false, false);

            // Assert
            Assert.AreEqual(commandId, setBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setbreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"line\":{1},\"column\":{2},\"type\":\"scriptId\",\"target\":{3},\"ignoreCount\":1}}}}",
                    commandId, line, column, module.Id),
                setBreakpointCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetBreakpointCommandOnFirstLine() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 5;
            const int line = 0;
            const int column = 0;
            const string fileName = "c:\\module.js";
            var module = new NodeModule(moduleId, fileName);
            var breakOn = new BreakOn(BreakOnKind.Equal, 2);
            var position = new FilePosition(fileName, line, column);
            var breakpoint = new NodeBreakpoint(null, position, true, breakOn, null);

            // Act
            var setBreakpointCommand = new SetBreakpointCommand(commandId, module, breakpoint, false, false);

            // Assert
            Assert.AreEqual(commandId, setBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setbreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"line\":{1},\"column\":{2},\"type\":\"scriptId\",\"target\":{3},\"ignoreCount\":1}}}}",
                    commandId, line, column + NodeConstants.ScriptWrapBegin.Length, module.Id),
                setBreakpointCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetBreakpointCommandOnRemoteFile() {
            // Arrange
            const int commandId = 3;
            const int line = 2;
            const int column = 0;
            const string fileName = @"module.js";
            var breakOn = new BreakOn(BreakOnKind.Equal, 2);
            var position = new FilePosition(fileName, line, column);
            var breakpoint = new NodeBreakpoint(null, position, true, breakOn, null);

            // Act
            var setBreakpointCommand = new SetBreakpointCommand(commandId, null, breakpoint, false, true);

            // Assert
            Assert.AreEqual(commandId, setBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setbreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"line\":{1},\"column\":{2},\"type\":\"scriptRegExp\",\"target\":\"^[Mm][Oo][Dd][Uu][Ll][Ee]\\\\.[Jj][Ss]$\",\"ignoreCount\":1}}}}",
                    commandId, line, column),
                setBreakpointCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetBreakpointCommandOnLocalFile() {
            // Arrange
            const int commandId = 3;
            const int line = 2;
            const int column = 0;
            const string fileName = @"c:\module.js";
            var breakOn = new BreakOn(BreakOnKind.Equal, 2);
            var position = new FilePosition(fileName, line, column);
            var breakpoint = new NodeBreakpoint(null, position, true, breakOn, null);

            // Act
            var setBreakpointCommand = new SetBreakpointCommand(commandId, null, breakpoint, false, false);

            // Assert
            Assert.AreEqual(commandId, setBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setbreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"line\":{1},\"column\":{2},\"type\":\"script\",\"target\":\"{3}\",\"ignoreCount\":1}}}}",
                    commandId, line, column, fileName.Replace(@"\", @"\\")),
                setBreakpointCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetBreakpointCommandWithoutPredicate() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 5;
            const int line = 2;
            const int column = 0;
            const string fileName = "module.js";
            var module = new NodeModule(moduleId, fileName);
            var breakOn = new BreakOn(BreakOnKind.Equal, 2);
            var position = new FilePosition(fileName, line, column);
            var breakpoint = new NodeBreakpoint(null, position, true, breakOn, null);

            // Act
            var setBreakpointCommand = new SetBreakpointCommand(commandId, module, breakpoint, true, false);

            // Assert
            Assert.AreEqual(commandId, setBreakpointCommand.Id);
            Assert.AreEqual(
                string.Format(
                    "{{\"command\":\"setbreakpoint\",\"seq\":{0},\"type\":\"request\",\"arguments\":{{\"line\":{1},\"column\":{2},\"type\":\"scriptId\",\"target\":{3}}}}}",
                    commandId, line, column, moduleId),
                setBreakpointCommand.ToString());
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void ProcessSetBreakpointResponse() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 33;
            const int line = 2;
            const int column = 0;
            const string fileName = "module.js";
            var module = new NodeModule(moduleId, fileName);
            var breakOn = new BreakOn(BreakOnKind.Equal, 2);
            var position = new FilePosition(fileName, line, column);
            var breakpoint = new NodeBreakpoint(null, position, true, breakOn, null);
            var setBreakpointCommand = new SetBreakpointCommand(commandId, module, breakpoint, false, false);
            JObject breakpointResponse = SerializationTestData.GetSetBreakpointResponse();

            // Act
            setBreakpointCommand.ProcessResponse(breakpointResponse);

            // Assert
            Assert.AreEqual(2, setBreakpointCommand.BreakpointId);
            Assert.AreEqual(0, setBreakpointCommand.Column);
            Assert.AreEqual(0, setBreakpointCommand.Line);
            Assert.AreEqual(false, setBreakpointCommand.Running);
            Assert.AreEqual(33, setBreakpointCommand.ScriptId);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void CreateSetBreakpointCommandWithNullBreakpoint() {
            // Arrange
            const int commandId = 3;
            const int moduleId = 5;
            const string fileName = "module.js";
            var module = new NodeModule(moduleId, fileName);
            SetBreakpointCommand setBreakpointCommand = null;
            Exception exception = null;

            // Act
            try {
                setBreakpointCommand = new SetBreakpointCommand(commandId, module, null, false, false);
            } catch (Exception e) {
                exception = e;
            }

            // Assert
            Assert.IsNull(setBreakpointCommand);
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof (ArgumentNullException));
        }
    }
}