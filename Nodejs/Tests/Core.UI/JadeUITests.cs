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
using System.IO;
using System.Windows.Automation;
using EnvDTE;
using Microsoft.NodejsTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools;
using TestUtilities;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class JadeUITests : NodejsProjectTest {
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void InsertTabs() {
            using (new OptionHolder("TextEditor", "Jade", "InsertTabs", true)) {
                using (var solution = Project("TabsSpaces", Content("quox.jade", "ul\r\n    li A\r\n    li B")).Generate().ToVs()) {
                    var jadeFile = solution.OpenItem("TabsSpaces", "quox.jade");
                    jadeFile.MoveCaret(1, 1);
                    Keyboard.Type("\t");
                    Assert.AreEqual(jadeFile.Text, "\tul\r\n    li A\r\n    li B");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void InsertSpaces() {
            using (new OptionHolder("TextEditor", "Jade", "InsertTabs", false)) {
                using (var solution = Project("TabsSpaces", Content("quox.jade", "ul\r\n    li A\r\n    li B")).Generate().ToVs()) {
                    var jadeFile = solution.OpenItem("TabsSpaces", "quox.jade");
                    jadeFile.MoveCaret(1, 1);
                    Keyboard.Type("\t");
                    Assert.AreEqual(jadeFile.Text, "    ul\r\n    li A\r\n    li B");
                }
            }
        }
    }
}
