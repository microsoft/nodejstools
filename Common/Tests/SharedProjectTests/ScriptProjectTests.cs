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

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.SharedProject;
using TestUtilities.UI;

namespace Microsoft.VisualStudioTools.SharedProjectTests {
    /// <summary>
    /// Test cases which are applicable to projects designed for scripting languages.
    /// </summary>
    [TestClass]
    public class ScriptProjectTests : SharedProjectTest {
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RunWithoutStartupFile() {
            foreach (var projectType in ProjectTypes) {
                var testDef = new ProjectDefinition("RunWithoutStartupFile", projectType);

                using (var solution = testDef.Generate().ToVs()) {
                    solution.App.OpenDialogWithDteExecuteCommand("Debug.Start");
                    VisualStudioApp.CheckMessageBox(
                        "No startup file is defined for the startup project."
                    );

                    solution.App.OpenDialogWithDteExecuteCommand("Debug.StartWithoutDebugging");
                    VisualStudioApp.CheckMessageBox(
                        "No startup file is defined for the startup project."
                    );
                }
            }
        }

        /// <summary>
        /// Renaming the folder containing the startup script should update the startup script
        /// https://nodejstools.codeplex.com/workitem/476
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RenameStartupFileFolder() {
            foreach (var projectType in ProjectTypes) {
                var testDef = new ProjectDefinition(
                    "RenameStartupFileFolder", 
                    projectType,
                    Folder("Folder"),
                    Compile("Folder\\server"),
                    Property("StartupFile", "Folder\\server" + projectType.CodeExtension)
                );

                using (var solution = testDef.Generate().ToVs()) {
                    var folder = solution.Project.ProjectItems.Item("Folder");
                    folder.Name = "FolderNew";

                    string startupFile = (string)solution.Project.Properties.Item("StartupFile").Value;
                    Assert.IsTrue(
                        startupFile.EndsWith(projectType.Code("FolderNew\\server")),
                        "Expected FolderNew in path, got {0}",
                        startupFile
                    );
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RenameStartupFile() {
            foreach (var projectType in ProjectTypes) {
                var testDef = new ProjectDefinition(
                    "RenameStartupFileFolder",
                    projectType,
                    Folder("Folder"),
                    Compile("Folder\\server"),
                    Property("StartupFile", "Folder\\server" + projectType.CodeExtension)
                );

                using (var solution = testDef.Generate().ToVs()) {
                    var file = solution.Project.ProjectItems.Item("Folder").ProjectItems.Item("server" + projectType.CodeExtension);
                    file.Name = "server2" + projectType.CodeExtension;

                    Assert.AreEqual(
                        "server2" + projectType.CodeExtension,
                        Path.GetFileName(
                            (string)solution.Project.Properties.Item("StartupFile").Value
                        )
                    );
                }
            }
        }
    }
}
