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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class ProjectPropertiesTests : NodejsProjectTest {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void DirtyProperties() {
            using (var solution = Project("DirtyProperties").Generate().ToVs()) {
                var proj = solution.FindItem("DirtyProperties");
                AutomationWrapper.Select(proj);
                solution.App.Dte.ExecuteCommand("ClassViewContextMenus.ClassViewMultiselectProjectreferencesItems.Properties");
                var window = solution.App.Dte.Windows.Item("DirtyProperties");
                Assert.AreEqual(window.Caption, "DirtyProperties");

                solution.Project.Properties.Item("NodejsPort").Value = 3000;
                Assert.AreEqual(false, solution.Project.Saved);
            }
        }
    }
}
