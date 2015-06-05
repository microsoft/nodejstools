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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools;
using TestUtilities;
using TestUtilities.Nodejs;
using TestUtilities.SharedProject;
using TestUtilities.UI;
using MessageBoxButton = TestUtilities.MessageBoxButton;
using ST = System.Threading;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class BasicProjectTests : SharedProjectTest {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void LoadNodejsProject() {
            using (var app = new VisualStudioApp()) {
                app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                Assert.IsTrue(app.Dte.Solution.IsOpen, "The solution is not open");
                Assert.IsTrue(app.Dte.Solution.Projects.Count == 1, String.Format("Loading project resulted in wrong number of loaded projects, expected 1, received {0}", app.Dte.Solution.Projects.Count));

                var iter = app.Dte.Solution.Projects.GetEnumerator();
                iter.MoveNext();
                Project project = (Project)iter.Current;
                Assert.AreEqual("HelloWorld.njsproj", Path.GetFileName(project.FileName), "Wrong project file name");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void SaveProjectAs() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                AssertError<ArgumentNullException>(() => project.SaveAs(null));
                project.SaveAs(TestData.GetPath(@"TestData\NodejsProjectData\TempFile.njsproj"));
                project.Save("");   // empty string means just save

                // try too long of a file
                try {
                    project.SaveAs("TempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFile.njsproj");
                    Assert.Fail("was able to save with long file name");
                } catch (InvalidOperationException e) {
                    Assert.IsTrue(e.ToString().Contains("exceeds the maximum number of"));
                }

                // save to a new location
                bool couldSaveToC = false;
                try {
                    project.SaveAs("C:\\TempFile.njsproj");
                    couldSaveToC = true;
                } catch (UnauthorizedAccessException e) {
                    // Saving to a new location is now permitted, but this location will not succeed.
                    Assert.IsTrue(e.ToString().Contains("Access to the path 'C:\\TempFile.njsproj' is denied."));
                } //catch (InvalidOperationException e) {
                //    Assert.IsTrue(e.ToString().Contains("The project file can only be saved into the project location"));
                //}

                project.SaveAs(TestData.GetPath(@"TestData\NodejsProjectData\TempFile.njsproj"));
                project.Save("");   // empty string means just save
                project.Delete();
                if (couldSaveToC) {
                    Assert.Inconclusive("could save to C:\\");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RenameProjectTest() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\RenameProjectTest.sln");

                // try it another way...
                project.Properties.Item("FileName").Value = "HelloWorld2.njsproj";
                Assert.AreEqual(project.Name, "HelloWorld2");

                // and yet another way...
                project.Name = "HelloWorld3";
                Assert.AreEqual(project.Name, "HelloWorld3");

                project.Name = "HelloWorld3";

                // invalid renames
                AssertError<InvalidOperationException>(() => project.Name = "");
                AssertError<InvalidOperationException>(() => project.Name = null);
                AssertError<InvalidOperationException>(() => project.Name = "TempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFile");
                AssertError<InvalidOperationException>(() => project.Name = "             ");
                AssertError<InvalidOperationException>(() => project.Name = "...............");
                var oldName = project.Name;
                project.Name = ".foo";
                Assert.AreEqual(project.Name, ".foo");
                project.Name = oldName;

                string projPath = TestData.GetPath(@"TestData\NodejsProjectData\RenameProjectTest\HelloWorld3.njsproj");
                string movePath = TestData.GetPath(@"TestData\NodejsProjectData\RenameProjectTest\HelloWorld_moved.njsproj");
                try {
                    File.Move(projPath, movePath);
                    AssertError<InvalidOperationException>(() => project.Name = "HelloWorld4");
                } finally {
                    File.Move(movePath, projPath);
                }

                try {
                    File.Copy(projPath, movePath);
                    AssertError<InvalidOperationException>(() => project.Name = "HelloWorld_moved");
                } finally {
                    File.Delete(movePath);
                }
            }

        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void ProjectAddFolder() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                var folder = project.ProjectItems.AddFolder("Test\\Folder\\Name");
                var folder2 = project.ProjectItems.AddFolder("Test\\Folder\\Name2");

                // try again when it already exists
                AssertError<ArgumentException>(() => project.ProjectItems.AddFolder("Test"));

                Assert.AreEqual("Name", folder.Properties.Item("FileName").Value);
                Assert.AreEqual("Name", folder.Properties.Item("FolderName").Value);

                Assert.AreEqual(TestData.GetPath(@"TestData\NodejsProjectData\HelloWorld\Test\Folder\Name\"), folder.Properties.Item("FullPath").Value);

                Assert.IsTrue(Directory.Exists(TestData.GetPath(@"TestData\NodejsProjectData\HelloWorld\Test\Folder\Name")));

                folder2.Properties.Item("FolderName").Value = "Name3";
                Assert.AreEqual("Name3", folder2.Name);
                folder2.Properties.Item("FileName").Value = "Name4";
                Assert.AreEqual("Name4", folder2.Name);

                AssertNotImplemented(() => folder.Open(""));
                AssertNotImplemented(() => folder.SaveAs(""));
                AssertNotImplemented(() => folder.Save());
                AssertNotImplemented(() => { var tmp = folder.IsOpen; });
                Assert.AreEqual(0, folder.Collection.Count);
                Assert.AreEqual(true, folder.Saved);

                Assert.AreEqual("{6bb5f8ef-4483-11d3-8bcf-00c04f8ec28c}", folder.Kind);

                folder.ExpandView();

                folder.Delete();
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void ProjectAddFolderThroughUI() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\AddFolderExists.sln");

                var solutionExplorer = app.SolutionExplorerTreeView;

                var solutionNode = solutionExplorer.FindItem("Solution 'AddFolderExists' (1 project)");
                var projectNode = solutionExplorer.FindItem("Solution 'AddFolderExists' (1 project)", "AddFolderExists");

                ProjectNewFolderWithName(app, solutionNode, projectNode, "A");

                var folderA = project.ProjectItems.Item("A");
                var folderANode = solutionExplorer.WaitForItem("Solution 'AddFolderExists' (1 project)", "AddFolderExists", "A");
                Assert.IsNotNull(folderANode);

                Assert.AreEqual(TestData.GetPath("TestData\\NodejsProjectData\\AddFolderExists\\A\\"), folderA.Properties.Item("FullPath").Value);
                Assert.IsTrue(Directory.Exists(TestData.GetPath("TestData\\NodejsProjectData\\AddFolderExists\\A\\")));

                ProjectNewFolderWithName(app, solutionNode, folderANode, "B");

                var folderB = folderA.ProjectItems.Item("B");
                var folderBNode = solutionExplorer.WaitForItem("Solution 'AddFolderExists' (1 project)", "AddFolderExists", "A", "B");
                Assert.IsNotNull(folderBNode);

                Assert.AreEqual(TestData.GetPath("TestData\\NodejsProjectData\\AddFolderExists\\A\\B\\"), folderB.Properties.Item("FullPath").Value);
                Assert.IsTrue(Directory.Exists(TestData.GetPath("TestData\\NodejsProjectData\\AddFolderExists\\A\\B\\")));

                ProjectNewFolderWithName(app, solutionNode, folderBNode, "C");

                var folderC = folderB.ProjectItems.Item("C");
                var folderCNode = solutionExplorer.WaitForItem("Solution 'AddFolderExists' (1 project)", "AddFolderExists", "A", "B", "C");
                Assert.IsNotNull(folderCNode);

                // 817 & 836: Nested subfolders
                // Setting the wrong VirtualNodeName in FolderNode.FinishFolderAdd caused C's fullpath to be ...\AddFolderExists\B\C\
                // instead of ...\AddFolderExists\A\B\C\.
                Assert.AreEqual(TestData.GetPath("TestData\\NodejsProjectData\\AddFolderExists\\A\\B\\C\\"), folderC.Properties.Item("FullPath").Value);
                Assert.IsTrue(Directory.Exists(TestData.GetPath("TestData\\NodejsProjectData\\AddFolderExists\\A\\B\\C\\")));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestAddExistingFolder() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\AddExistingFolder.sln");

                var solutionExplorer = app.SolutionExplorerTreeView;

                var projectNode = solutionExplorer.WaitForItem("Solution 'AddExistingFolder' (1 project)", "AddExistingFolder");
                AutomationWrapper.Select(projectNode);

                using (var dialog = SelectFolderDialog.AddExistingFolder(app)) {
                    Assert.AreEqual(dialog.Address.ToLower(), Path.GetFullPath(@"TestData\NodejsProjectData\AddExistingFolder").ToLower());

                    dialog.FolderName = Path.GetFullPath(@"TestData\NodejsProjectData\AddExistingFolder\TestFolder");
                    dialog.SelectFolder();
                }

                Assert.AreNotEqual(solutionExplorer.WaitForItem("Solution 'AddExistingFolder' (1 project)", "AddExistingFolder", "TestFolder"), null);
                Assert.AreNotEqual(solutionExplorer.WaitForItem("Solution 'AddExistingFolder' (1 project)", "AddExistingFolder", "TestFolder", "TestFile.txt"), null);

                var subFolderNode = solutionExplorer.WaitForItem("Solution 'AddExistingFolder' (1 project)", "AddExistingFolder", "SubFolder");
                AutomationWrapper.Select(subFolderNode);

                using (var dialog = SelectFolderDialog.AddExistingFolder(app)) {
                    Assert.AreEqual(dialog.Address.ToLower(), Path.GetFullPath(@"TestData\NodejsProjectData\AddExistingFolder\SubFolder").ToLower());
                    dialog.FolderName = Path.GetFullPath(@"TestData\NodejsProjectData\AddExistingFolder\SubFolder\TestFolder2");
                    dialog.SelectFolder();
                }

                Assert.AreNotEqual(solutionExplorer.WaitForItem("Solution 'AddExistingFolder' (1 project)", "AddExistingFolder", "SubFolder", "TestFolder2"), null);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestAddExistingFolderProject() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\AddExistingFolder.sln");

                var solutionExplorer = app.SolutionExplorerTreeView;

                var projectNode = solutionExplorer.WaitForItem("Solution 'AddExistingFolder' (1 project)", "AddExistingFolder");
                AutomationWrapper.Select(projectNode);

                using (var dialog = SelectFolderDialog.AddExistingFolder(app)) {
                    Assert.AreEqual(dialog.Address.ToLower(), Path.GetFullPath(@"TestData\NodejsProjectData\AddExistingFolder").ToLower());

                    dialog.FolderName = Path.GetFullPath(@"TestData\NodejsProjectData\AddExistingFolder");
                    dialog.SelectFolder();
                }

                VisualStudioApp.CheckMessageBox("Cannot add folder 'AddExistingFolder' as a child or decedent of self.");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestAddExistingFolderDebugging() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\AddExistingFolder.sln");
                var window = project.ProjectItems.Item("server.js").Open();
                window.Activate();

                var docWindow = app.GetDocument(window.Document.FullName);

                var solutionExplorer = app.SolutionExplorerTreeView;
                app.ExecuteCommand("Debug.Start");
                app.WaitForMode(dbgDebugMode.dbgRunMode);

                app.OpenSolutionExplorer();
                var projectNode = solutionExplorer.WaitForItem("Solution 'AddExistingFolder' (1 project)", "AddExistingFolder");
                AutomationWrapper.Select(projectNode);

                try {
                    app.Dte.ExecuteCommand("ProjectandSolutionContextMenus.Project.Add.Existingfolder");

                    // try and dismiss the dialog if we successfully executed
                    try {
                        var dialog = app.WaitForDialog();
                        Keyboard.Type(System.Windows.Input.Key.Escape);
                    } finally {
                        Assert.Fail("Was able to add an existing folder");
                    }
                } catch (COMException) {
                }
                app.ExecuteCommand("Debug.StopDebugging");
                app.WaitForMode(dbgDebugMode.dbgDesignMode);

                projectNode = solutionExplorer.WaitForItem("Solution 'AddExistingFolder' (1 project)", "AddExistingFolder");
                AutomationWrapper.Select(projectNode);

                using (var addDialog = SelectFolderDialog.AddExistingFolder(app)) {
                    Assert.AreEqual(addDialog.Address.ToLower(), Path.GetFullPath(@"TestData\NodejsProjectData\AddExistingFolder").ToLower());

                    addDialog.FolderName = Path.GetFullPath(@"TestData\NodejsProjectData\AddExistingFolder\TestFolder");
                    addDialog.SelectFolder();
                }

                Assert.AreNotEqual(solutionExplorer.WaitForItem("Solution 'AddExistingFolder' (1 project)", "AddExistingFolder", "TestFolder"), null);
            }
        }

        /// <summary>
        /// 1) Right click on project and choose add\new folder
        /// 2) Commit the default name (NewFolder*) by hitting enter
        /// 3) F2
        /// 4) Change name
        /// 5) Enter to commit
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void ProjectAddAndRenameFolder() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                var solutionExplorer = app.SolutionExplorerTreeView;

                var folder = project.ProjectItems.AddFolder("AddAndRenameFolder");
                var subfolderNode = solutionExplorer.FindItem("Solution 'HelloWorld' (1 project)", "HelloWorld", "AddAndRenameFolder");

                // rename it
                AutomationWrapper.Select(subfolderNode);
                Keyboard.Type(System.Windows.Input.Key.F2);
                Keyboard.Type("AddAndRenameFolderNewName");
                Keyboard.Type(System.Windows.Input.Key.Enter);

                subfolderNode = solutionExplorer.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld", "AddAndRenameFolderNewName");
                Assert.IsTrue(Directory.Exists(@"TestData\NodejsProjectData\HelloWorld\AddAndRenameFolderNewName"));
            }
        }

        /// <summary>
        /// 1) Create  a new folder (under project)
        /// 2) Create a nested new folder (under folder created in 1)
        /// 3) Rename nested folder
        /// 4) Drag and drop nested folder onto project
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void ProjectAddAndMoveRenamedFolder() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");
                var solutionExplorer = app.SolutionExplorerTreeView;

                var folder = project.ProjectItems.AddFolder("AddAndMoveRenamedFolder\\AddAndMoveRenamedSubFolder");
                var subfolderNode = solutionExplorer.FindItem("Solution 'HelloWorld' (1 project)", "HelloWorld", "AddAndMoveRenamedFolder", "AddAndMoveRenamedSubFolder");

                // rename it
                AutomationWrapper.Select(subfolderNode);
                Keyboard.Type(System.Windows.Input.Key.F2);
                Keyboard.Type("AddAndMoveRenamedNewName");
                Keyboard.Type(System.Windows.Input.Key.Enter);

                subfolderNode = solutionExplorer.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld", "AddAndMoveRenamedFolder", "AddAndMoveRenamedNewName");

                Assert.IsTrue(Directory.Exists(@"TestData\NodejsProjectData\HelloWorld\AddAndMoveRenamedFolder\AddAndMoveRenamedNewName"), "AddAndMoveRenamedFolder\\AddAndMoveRenamedNewName doesn't exist");

                AutomationWrapper.Select(subfolderNode);
                Keyboard.ControlX();

                var projNode = solutionExplorer.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld");
                AutomationWrapper.Select(projNode);

                Keyboard.ControlV();

                var movedNode = solutionExplorer.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld", "AddAndMoveRenamedNewName");

                Assert.IsTrue(Directory.Exists(@"TestData\NodejsProjectData\HelloWorld\AddAndMoveRenamedNewName"), "HelloWorld\\AddAndMoveRenamedNewName doesn't exist");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void ProjectBuild() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                app.Dte.Solution.SolutionBuild.Build(true);
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/823
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void ProjectBuildWithProjFileSeparateFromSources() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld3.sln");

                app.Dte.Solution.SolutionBuild.Build(true);
                Assert.AreEqual(0, app.Dte.Solution.SolutionBuild.LastBuildInfo, "Expected no build failures");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void ProjectRenameAndDeleteItem() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\RenameItemsTest.sln");

                app.Dte.Documents.CloseAll(vsSaveChanges.vsSaveChangesNo);

                // invalid renames
                AssertError<InvalidOperationException>(() => project.ProjectItems.Item("ProgramX.js").Name = "");
                AssertError<InvalidOperationException>(() => project.ProjectItems.Item("ProgramX.js").Name = "TempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFileTempFile");
                AssertError<InvalidOperationException>(() => project.ProjectItems.Item("ProgramX.js").Name = "              ");
                AssertError<InvalidOperationException>(() => project.ProjectItems.Item("ProgramX.js").Name = "..............");
                project.ProjectItems.Item("ProgramX.js").Name = ".foo";
                project.ProjectItems.Item(".foo").Name = "ProgramX.js";
                AssertError<InvalidOperationException>(() => project.ProjectItems.Item("ProgramX.js").Name = "ProgramY.js");
                project.ProjectItems.Item("ProgramX.js").Name = "PrOgRaMX.js";
                project.ProjectItems.Item("ProgramX.js").Name = "ProgramX.js";

                project.ProjectItems.Item("ProgramX.js").Name = "Program2.js";

                bool foundProg2 = false;
                foreach (ProjectItem item in project.ProjectItems) {
                    Debug.Assert(item.Name != "ProgramX.js");
                    if (item.Name == "Program2.js") {
                        foundProg2 = true;
                    }
                }
                Assert.IsTrue(foundProg2);

                // rename using a different method...
                project.ProjectItems.Item("ProgramY.js").Properties.Item("FileName").Value = "Program3.js";
                bool foundProg3 = false;
                foreach (ProjectItem item in project.ProjectItems) {
                    Debug.Assert(item.Name != "ProgramY.js");
                    if (item.Name == "Program3.js") {
                        foundProg3 = true;
                    }
                }

                project.ProjectItems.Item("Program3.js").Remove();

                Assert.IsTrue(foundProg3);

                Assert.AreEqual(0, project.ProjectItems.Item("ProgramZ.js").ProjectItems.Count);
                AssertError<ArgumentNullException>(() => project.ProjectItems.Item("ProgramZ.js").SaveAs(null));
                // try Save As, this won't rename it in the project.
                project.ProjectItems.Item("ProgramZ.js").SaveAs("Program4.js");

                bool foundProgZ = false;
                foreach (ProjectItem item in project.ProjectItems) {
                    Debug.Assert(item.Name
                        != "Program4.js");
                    if (item.Name == "ProgramZ.js") {
                        foundProgZ = true;
                    }
                }
                Assert.IsTrue(foundProgZ);

                File.WriteAllText("TemplateItem2.js", "");
                var newItem = project.ProjectItems.AddFromFile(Path.GetFullPath("TemplateItem2.js"));
                newItem.Open();

                // save w/o filename, w/ filename that matches, and w/ wrong filename
                newItem.Save();
                newItem.Save("TemplateItem2.js");
                AssertError<InvalidOperationException>(() => newItem.Save("WrongFilename.js"));

                // rename something in a folder...
                project.ProjectItems.Item("SubFolder").ProjectItems.Item("SubItem.js").Name = "NewSubItem.js";

                project.ProjectItems.Item("ProgramDelete.js").Delete();
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestAutomationProperties() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                int propCount = 0;
                foreach (Property prop in project.Properties) {
                    object intIndexValue;
                    try {
                        intIndexValue = project.Properties.Item(propCount + 1).Value;
                    } catch (Exception e) {
                        intIndexValue = e.Message;
                    }
                    object nameIndexValue;
                    try {
                        nameIndexValue = project.Properties.Item(prop.Name).Value;
                    } catch (Exception e) {
                        nameIndexValue = e.Message;
                    }

                    object indexedValue;
                    try {
                        indexedValue = project.Properties.Item(prop.Name).get_IndexedValue(null);
                    } catch (Exception e) {
                        indexedValue = e.Message;
                    }

                    Assert.AreEqual(intIndexValue, nameIndexValue);
                    Assert.AreEqual(intIndexValue, intIndexValue);
                    Assert.AreEqual(app.Dte, project.Properties.Item(propCount + 1).DTE);
                    Assert.AreEqual(0, project.Properties.Item(propCount + 1).NumIndices);
                    Assert.AreNotEqual(null, project.Properties.Item(propCount + 1).Parent);
                    Assert.AreEqual(null, project.Properties.Item(propCount + 1).Application);
                    Assert.AreNotEqual(null, project.Properties.Item(propCount + 1).Collection);
                    propCount++;
                }

                Assert.AreEqual(propCount, project.Properties.Count);

                Assert.AreEqual(project.Properties.DTE, app.Dte);

                Assert.AreEqual(project.Properties.Item("StartWebBrowser").Value.GetType(), typeof(bool));
                Assert.IsTrue(project.Properties.Item("NodejsPort").Value == null || project.Properties.Item("NodejsPort").Value.GetType() == typeof(int));

                Assert.AreEqual(project.Properties.Item("LaunchUrl").Value, null);
                Assert.AreEqual(project.Properties.Item("ScriptArguments").Value, null);
                Assert.AreEqual(project.Properties.Item("NodeExeArguments").Value, null);
                Assert.AreEqual(project.Properties.Item("NodeExePath").Value.GetType(), typeof(string));

                project.Properties.Item("StartWebBrowser").Value = true;
                Assert.AreEqual(project.Properties.Item("StartWebBrowser").Value, true);
                project.Properties.Item("StartWebBrowser").Value = false;
                Assert.AreEqual(project.Properties.Item("StartWebBrowser").Value, false);

                project.Properties.Item("NodejsPort").Value = 10000;
                Assert.AreEqual(project.Properties.Item("NodejsPort").Value, 10000);
                project.Properties.Item("NodejsPort").Value = 5000;
                Assert.AreEqual(project.Properties.Item("NodejsPort").Value, 5000);

                foreach (var value in new[] { "LaunchUrl", "ScriptArguments", "NodeExeArguments", "NodeExePath" }) {
                    string tmpValue = Guid.NewGuid().ToString();

                    project.Properties.Item(value).Value = tmpValue;
                    Assert.AreEqual(project.Properties.Item(value).Value, tmpValue);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestAutomationProject() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                Assert.AreEqual("{9092aa53-fb77-4645-b42d-1ccca6bd08bd}", project.Kind);
                // we don't yet expose a VSProject interface here, if we did we'd need tests for it, but it doesn't support
                // any functionality we care about/implement yet.
                Assert.AreNotEqual(null, project.Object);

                Assert.AreEqual(true, project.Saved);
                project.Saved = false;
                Assert.AreEqual(false, project.Saved);
                project.Saved = true;

                Assert.AreEqual(null, project.Globals);
                Assert.AreEqual("{04726c27-8125-471a-bac0-2301d273db5e}", project.ExtenderCATID);
                var extNames = project.ExtenderNames;
                Assert.AreEqual(typeof(string[]), extNames.GetType());
                Assert.AreEqual(2, ((string[])extNames).Length);
                Assert.AreEqual(null, project.ParentProjectItem);
                Assert.AreEqual(null, project.CodeModel);
                AssertError<ArgumentNullException>(() => project.get_Extender(null));
                AssertError<COMException>(() => project.get_Extender("DoesNotExist"));
                Assert.AreEqual(null, project.Collection);

                foreach (ProjectItem item in project.ProjectItems) {
                    Assert.AreEqual(item.Name, project.ProjectItems.Item(1).Name);
                    break;
                }

                Assert.AreEqual(app.Dte, project.ProjectItems.DTE);
                Assert.AreEqual(project, project.ProjectItems.Parent);
                Assert.AreEqual(null, project.ProjectItems.Kind);

                AssertError<ArgumentException>(() => project.ProjectItems.Item(-1));
                AssertError<ArgumentException>(() => project.ProjectItems.Item(0));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestProjectItemAutomation() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                var item = project.ProjectItems.Item("server.js");
                Assert.AreEqual(null, item.ExtenderNames);
                Assert.AreEqual(null, item.ExtenderCATID);
                Assert.AreEqual(null, item.SubProject);
                Assert.AreEqual("{6bb5f8ee-4483-11d3-8bcf-00c04f8ec28c}", item.Kind);
                Assert.AreEqual(null, item.ConfigurationManager);
                Assert.AreNotEqual(null, item.Collection.Item("server.js"));
                AssertError<ArgumentOutOfRangeException>(() => item.get_FileNames(-1));
                AssertNotImplemented(() => item.Saved = false);

                AssertError<ArgumentException>(() => item.get_IsOpen("ThisIsNotTheGuidYoureLookingFor"));
                AssertError<ArgumentException>(() => item.Open("ThisIsNotTheGuidYoureLookingFor"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestRelativePaths() {
            // link to outside file should show up as top-level item
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\RelativePaths.sln");

                var item = project.ProjectItems.Item("server.js");
                Assert.IsNotNull(item);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void ProjectConfiguration() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                project.ConfigurationManager.AddConfigurationRow("NewConfig", "Debug", true);
                project.ConfigurationManager.AddConfigurationRow("NewConfig2", "UnknownConfig", true);

                AssertError<ArgumentException>(() => project.ConfigurationManager.DeleteConfigurationRow(null));
                project.ConfigurationManager.DeleteConfigurationRow("NewConfig");
                project.ConfigurationManager.DeleteConfigurationRow("NewConfig2");

                var debug = project.ConfigurationManager.Item("Debug", "Any CPU");
                Assert.AreEqual(debug.IsBuildable, true);

                Assert.AreEqual("Any CPU", ((object[])project.ConfigurationManager.PlatformNames)[0]);
                Assert.AreEqual("Any CPU", ((object[])project.ConfigurationManager.SupportedPlatforms)[0]);

                Assert.AreEqual(null, project.ConfigurationManager.ActiveConfiguration.Object);

                //var workingDir = project.ConfigurationManager.ActiveConfiguration.Properties.Item("WorkingDirectory");
                //Assert.AreEqual(".", workingDir);

                // not supported
                AssertError<COMException>(() => project.ConfigurationManager.AddPlatform("NewPlatform", "Any CPU", false));
                AssertError<COMException>(() => project.ConfigurationManager.DeletePlatform("NewPlatform"));
            }
        }

        /// <summary>
        /// Opens a project w/ a reference to a .NET assembly (not a project).  Makes sure we get completion against the assembly, changes the assembly, rebuilds, makes
        /// sure the completion info changes.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void AddFolderExists() {
            Directory.CreateDirectory(TestData.GetPath(@"TestData\NodejsProjectData\\AddFolderExists\\X"));
            Directory.CreateDirectory(TestData.GetPath(@"TestData\NodejsProjectData\\AddFolderExists\\Y"));

            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\AddFolderExists.sln");

                var solutionExplorer = app.SolutionExplorerTreeView;

                var solutionNode = solutionExplorer.FindItem("Solution 'AddFolderExists' (1 project)");

                var projectNode = solutionExplorer.FindItem("Solution 'AddFolderExists' (1 project)", "AddFolderExists");

                ProjectNewFolder(app, solutionNode, projectNode);

                System.Threading.Thread.Sleep(1000);
                Keyboard.Type("."); // bad filename
                Keyboard.Type(System.Windows.Input.Key.Enter);

#if DEV11_OR_LATER
                VisualStudioApp.CheckMessageBox(MessageBoxButton.Ok, "Directory names cannot contain any of the following characters");
#else
                VisualStudioApp.CheckMessageBox(MessageBoxButton.Ok, ". is an invalid filename");
#endif
                System.Threading.Thread.Sleep(1000);

                Keyboard.Type(".."); // another bad filename
                Keyboard.Type(System.Windows.Input.Key.Enter);

#if DEV11_OR_LATER
                VisualStudioApp.CheckMessageBox(MessageBoxButton.Ok, "Directory names cannot contain any of the following characters");
#else
                VisualStudioApp.CheckMessageBox(MessageBoxButton.Ok, ".. is an invalid filename");
#endif
                System.Threading.Thread.Sleep(1000);

                Keyboard.Type("Y"); // another bad filename
                Keyboard.Type(System.Windows.Input.Key.Enter);

                VisualStudioApp.CheckMessageBox(MessageBoxButton.Ok, "The folder Y already exists.");
                System.Threading.Thread.Sleep(1000);

                Keyboard.Type("X");
                Keyboard.Type(System.Windows.Input.Key.Enter);

                // item should be successfully added now.
                VisualStudioApp.CheckMessageBox(MessageBoxButton.Ok, "The folder X already exists.");

                Keyboard.Type("Z");
                Keyboard.Type(System.Windows.Input.Key.Enter);
                WaitForItem(project, "Z");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void AddFolderCopyAndPasteFile() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\AddFolderCopyAndPasteFile.sln");
                var solutionExplorer = app.SolutionExplorerTreeView;
                var solutionNode = solutionExplorer.FindItem("Solution 'AddFolderCopyAndPasteFile' (1 project)");

                var projectNode = solutionExplorer.FindItem("Solution 'AddFolderCopyAndPasteFile' (1 project)", "AddFolderCopyAndPasteFile");

                var serverNode = solutionExplorer.FindItem("Solution 'AddFolderCopyAndPasteFile' (1 project)", "AddFolderCopyAndPasteFile", "server.js");
                Mouse.MoveTo(serverNode.GetClickablePoint());
                Mouse.Click();
                Keyboard.ControlC();
                Keyboard.ControlV();

                // Make sure that copy/paste directly under the project node works:
                // http://pytools.codeplex.com/workitem/738
                Assert.IsNotNull(solutionExplorer.WaitForItem("Solution 'AddFolderCopyAndPasteFile' (1 project)", "AddFolderCopyAndPasteFile", "server - Copy.js"));

                ProjectNewFolder(app, solutionNode, projectNode);

                Keyboard.Type("Foo");
                Keyboard.Type(System.Windows.Input.Key.Return);

                WaitForItem(project, "Foo");

                Mouse.MoveTo(serverNode.GetClickablePoint());
                Mouse.Click();
                Keyboard.ControlC();

                var folderNode = solutionExplorer.FindItem("Solution 'AddFolderCopyAndPasteFile' (1 project)", "AddFolderCopyAndPasteFile", "Foo");
                Mouse.MoveTo(folderNode.GetClickablePoint());
                Mouse.Click();

                Keyboard.ControlV();

                Assert.IsNotNull(solutionExplorer.WaitForItem("Solution 'AddFolderCopyAndPasteFile' (1 project)", "AddFolderCopyAndPasteFile", "Foo", "server.js"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void CopyAndPasteFolder() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\CopyAndPasteFolder.sln");
                var solutionExplorer = app.SolutionExplorerTreeView;
                var solutionNode = solutionExplorer.FindItem("Solution 'CopyAndPasteFolder' (1 project)");

                var projectNode = solutionExplorer.FindItem("Solution 'CopyAndPasteFolder' (1 project)", "CopyAndPasteFolder");

                var folderNode = solutionExplorer.FindItem("Solution 'CopyAndPasteFolder' (1 project)", "CopyAndPasteFolder", "X");

                // paste to project node, make sure the files are there
                StringCollection paths = new StringCollection() {
                    Path.Combine(Directory.GetCurrentDirectory(), "TestData", "NodejsProjectData", "CopiedFiles")
                };

                ToSTA(() => Clipboard.SetFileDropList(paths));

                Mouse.MoveTo(projectNode.GetClickablePoint());
                Mouse.Click();
                Keyboard.ControlV();

                Assert.IsNotNull(solutionExplorer.WaitForItem("Solution 'CopyAndPasteFolder' (1 project)", "CopyAndPasteFolder", "CopiedFiles"));
                Assert.IsTrue(File.Exists(Path.Combine("TestData", "NodejsProjectData", "CopyAndPasteFolder", "CopiedFiles", "SomeFile.js")));
                Assert.IsTrue(File.Exists(Path.Combine("TestData", "NodejsProjectData", "CopyAndPasteFolder", "CopiedFiles", "Foo", "SomeOtherFile.js")));

                Mouse.MoveTo(folderNode.GetClickablePoint());
                Mouse.Click();

                // paste to folder node, make sure the files are there
                ToSTA(() => Clipboard.SetFileDropList(paths));
                Keyboard.ControlV();

                Assert.IsNotNull(solutionExplorer.WaitForItem("Solution 'CopyAndPasteFolder' (1 project)", "CopyAndPasteFolder", "X", "CopiedFiles"));
                Assert.IsTrue(File.Exists(Path.Combine("TestData", "NodejsProjectData", "CopyAndPasteFolder", "X", "CopiedFiles", "SomeFile.js")));
                Assert.IsTrue(File.Exists(Path.Combine("TestData", "NodejsProjectData", "CopyAndPasteFolder", "X", "CopiedFiles", "Foo", "SomeOtherFile.js")));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void CopyAndPasteEmptyFolder() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\CopyAndPasteFolder.sln");
                var solutionExplorer = app.SolutionExplorerTreeView;
                var solutionNode = solutionExplorer.FindItem("Solution 'CopyAndPasteFolder' (1 project)");

                var projectNode = solutionExplorer.FindItem("Solution 'CopyAndPasteFolder' (1 project)", "CopyAndPasteFolder");

                var folderNode = solutionExplorer.FindItem("Solution 'CopyAndPasteFolder' (1 project)", "CopyAndPasteFolder", "X");

                var emptyFolderName = "EmptyFolder" + Guid.NewGuid();
                Directory.CreateDirectory(emptyFolderName);
                // paste to project node, make sure the files are there
                StringCollection paths = new StringCollection() {
                    Path.Combine(Directory.GetCurrentDirectory(), emptyFolderName)
                };

                ToSTA(() => Clipboard.SetFileDropList(paths));

                Mouse.MoveTo(projectNode.GetClickablePoint());
                Mouse.Click();
                Keyboard.ControlV();

                Assert.IsNotNull(solutionExplorer.WaitForItem("Solution 'CopyAndPasteFolder' (1 project)", "CopyAndPasteFolder", emptyFolderName));
                Assert.IsTrue(Directory.Exists(Path.Combine("TestData", "NodejsProjectData", "CopyAndPasteFolder", emptyFolderName)));

                Mouse.MoveTo(folderNode.GetClickablePoint());
                Mouse.Click();

                // paste to folder node, make sure the files are there
                ToSTA(() => Clipboard.SetFileDropList(paths));
                Keyboard.ControlV();

                Assert.IsNotNull(solutionExplorer.WaitForItem("Solution 'CopyAndPasteFolder' (1 project)", "CopyAndPasteFolder", "X", emptyFolderName));
                Assert.IsTrue(Directory.Exists(Path.Combine("TestData", "NodejsProjectData", "CopyAndPasteFolder", "X", emptyFolderName)));
            }
        }

        private static void ToSTA(ST.ThreadStart code) {
            ST.Thread t = new ST.Thread(code);
            t.SetApartmentState(ST.ApartmentState.STA);
            t.Start();
            t.Join();
        }

        /// <summary>
        /// Verify we can copy a folder with multiple items in it.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void CopyFolderWithMultipleItems() {
            // http://mpfproj10.codeplex.com/workitem/11618
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\FolderMultipleItems.sln");
                var solutionExplorer = app.SolutionExplorerTreeView;
                var solutionNode = solutionExplorer.FindItem("Solution 'FolderMultipleItems' (1 project)");

                var projectNode = solutionExplorer.FindItem("Solution 'FolderMultipleItems' (1 project)", "FolderMultipleItems");

                var folderNode = solutionExplorer.FindItem("Solution 'FolderMultipleItems' (1 project)", "FolderMultipleItems", "A");

                Mouse.MoveTo(folderNode.GetClickablePoint());
                Mouse.Click();
                Keyboard.ControlC();

                Keyboard.ControlV();
                WaitForItem(project, "A - Copy");

                Assert.IsNotNull(solutionExplorer.FindItem("Solution 'FolderMultipleItems' (1 project)", "FolderMultipleItems", "A - Copy", "a.js"));
                Assert.IsNotNull(solutionExplorer.FindItem("Solution 'FolderMultipleItems' (1 project)", "FolderMultipleItems", "A - Copy", "b.js"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void LoadProjectWithDuplicateItems() {
            using (var app = new VisualStudioApp()) {
                var solution = app.OpenProject(@"TestData\NodejsProjectData\DuplicateItems.sln");

                var itemCount = new Dictionary<string, int>();

                CountNames(itemCount, solution.ProjectItems);

                CountIs(itemCount, "A", 1);
                CountIs(itemCount, "B", 1);
                CountIs(itemCount, "a.js", 1);
                CountIs(itemCount, "b.js", 1);
                CountIs(itemCount, "server.js", 2);
                CountIs(itemCount, "HelloWorld.njsproj", 1);
                CountIs(itemCount, "HelloWorld.js", 0);     // not included because the actual name is server.js
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void CopyFullPath() {
            foreach (var projectType in ProjectTypes) {
                var def = new ProjectDefinition(
                    "HelloWorld",
                    projectType,
                    Compile("server"),
                    Folder("IncFolder", isExcluded: false),
                    Folder("ExcFolder", isExcluded: true),
                    Compile("app", isExcluded: true),
                    Compile("missing", isMissing: true)
                );

                using (var solution = def.Generate().ToVs()) {
                    var projectDir = Path.GetDirectoryName(solution.GetProject("HelloWorld").FullName);

                    CheckCopyFullPath(solution,
                                      solution.WaitForItem("HelloWorld", "IncFolder"),
                                      projectDir + "\\IncFolder\\");
                    var excFolder = solution.WaitForItem("HelloWorld", "ExcFolder");
                    if (excFolder == null) {
                        solution.SelectProject(solution.GetProject("HelloWorld"));
                        solution.ExecuteCommand("Project.ShowAllFiles");
                        excFolder = solution.WaitForItem("HelloWorld", "ExcFolder");
                    }
                    CheckCopyFullPath(solution, excFolder, projectDir + "\\ExcFolder\\");
                    CheckCopyFullPath(solution,
                                      solution.WaitForItem("HelloWorld", "server" + def.ProjectType.CodeExtension),
                                      projectDir + "\\server" + def.ProjectType.CodeExtension);
                    CheckCopyFullPath(solution,
                                      solution.WaitForItem("HelloWorld", "app" + def.ProjectType.CodeExtension),
                                      projectDir + "\\app" + def.ProjectType.CodeExtension);
                    CheckCopyFullPath(solution,
                                      solution.WaitForItem("HelloWorld", "missing" + def.ProjectType.CodeExtension),
                                      projectDir + "\\missing" + def.ProjectType.CodeExtension);
                }
            }
        }

        private void CheckCopyFullPath(IVisualStudioInstance vs, ITreeNode element, string expected) {
            string clipboardText = "";
            Console.WriteLine("Checking CopyFullPath on:{0}", expected);
            AutomationWrapper.Select(element);
            vs.Dte.ExecuteCommand("File.CopyFullPath");

            var app = ((VisualStudioInstance)vs).App;
            app.ServiceProvider.GetUIThread().Invoke(() => clipboardText = System.Windows.Clipboard.GetText());

            Assert.AreEqual(expected, clipboardText);
        }

        private static void CountIs(Dictionary<string, int> count, string key, int expected) {
            int actual;
            if (!count.TryGetValue(key, out actual)) {
                actual = 0;
            }
            Assert.AreEqual(expected, actual, "count[" + key + "]");
        }

        private static void CountNames(Dictionary<string, int> count, ProjectItems items) {
            if (items == null) {
                return;
            }

            foreach (var item in items.OfType<ProjectItem>()) {
                if (!string.IsNullOrEmpty(item.Name)) {
                    int value;
                    if (!count.TryGetValue(item.Name, out value)) {
                        value = 0;
                    }
                    count[item.Name] = value + 1;
                }
                CountNames(count, item.ProjectItems);
            }
        }

        private static void ProjectNewFolder(VisualStudioApp app, System.Windows.Automation.AutomationElement solutionNode, System.Windows.Automation.AutomationElement projectNode) {
            // Project menu can take a little while to appear...
            for (int i = 0; i < 20; i++) {
                AutomationWrapper.Select(projectNode);
                projectNode.SetFocus();
                try {
                    app.Dte.ExecuteCommand("Project.NewFolder");
                    break;
                } catch (Exception e) {
                    Debug.WriteLine("New folder failed: {0}", e);
                }

                Debug.WriteLine("Back to solution explorer...");
                Mouse.MoveTo(solutionNode.GetClickablePoint());
                Mouse.Click();
                System.Threading.Thread.Sleep(250);
            }
        }

        private static void ProjectNewFolderWithName(VisualStudioApp app, System.Windows.Automation.AutomationElement solutionNode, System.Windows.Automation.AutomationElement projectNode, string name) {
            Mouse.MoveTo(projectNode.GetClickablePoint());
            Mouse.Click(System.Windows.Input.MouseButton.Right);

            Keyboard.Type("d");
            Keyboard.PressAndRelease(System.Windows.Input.Key.Right);
            Keyboard.Type("d");

            System.Threading.Thread.Sleep(250);

            Keyboard.Type(name);
            Keyboard.Type("\n");

            System.Threading.Thread.Sleep(500);
        }

        private static ProjectItem WaitForItem(Project project, string name) {
            bool found = false;
            ProjectItem item = null;
            for (int i = 0; i < 40; i++) {
                try {
                    item = project.ProjectItems.Item(name);
                    if (item != null) {
                        found = true;
                        break;
                    }
                } catch (ArgumentException) {
                }
                // wait for the edit to complete
                System.Threading.Thread.Sleep(250);
            }
            Assert.IsTrue(found);
            return item;
        }

        private static void AssertNotImplemented(Action action) {
            AssertError<NotImplementedException>(action);
        }

        private static void AssertError<T>(Action action) where T : Exception {
            try {
                action();
                Assert.Fail();
            } catch (T) {
            }
        }
    }
}
