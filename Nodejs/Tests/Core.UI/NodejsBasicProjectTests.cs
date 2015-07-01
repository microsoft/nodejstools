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
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using EnvDTE;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.VSTestHost;
using TestUtilities;
using TestUtilities.UI;
using TestUtilities.Nodejs;
using Key = System.Windows.Input.Key;
using MouseButton = System.Windows.Input.MouseButton;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class NodejsBasicProjectTests : NodejsProjectTest {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void AddNewTypeScriptItem() {
            using (var solution = Project("AddNewTypeScriptItem", Compile("server")).Generate().ToVs()) {
                var project = solution.WaitForItem("AddNewTypeScriptItem", "server.js");
                AutomationWrapper.Select(project);

                using (var newItem = solution.AddNewItem()) {
                    newItem.FileName = "NewTSFile.ts";
                    newItem.OK();
                }

                using (AutoResetEvent buildDone = new AutoResetEvent(false)) {
                    VSTestContext.DTE.Events.BuildEvents.OnBuildDone += (sender, args) => {
                        buildDone.Set();
                    };

                    solution.ExecuteCommand("Build.BuildSolution");
                    solution.WaitForOutputWindowText("Build", "tsc.exe");
                    Assert.IsTrue(buildDone.WaitOne(10000), "failed to wait for build)");
                }
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1195
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestExcludedErrors() {
            var project = Project("TestExcludedErrors",
                Compile("server", "function f(a, b, c) { }\r\n\r\n"),
                Compile("excluded", "aa bb", isExcluded: true)
            );

            using (var solution = project.Generate().ToVs()) {
                List<IVsTaskItem> allItems = solution.WaitForErrorListItems(0);
                Assert.AreEqual(0, allItems.Count);

                var excluded = solution.WaitForItem("TestExcludedErrors", "excluded.js");
                AutomationWrapper.Select(excluded);
                solution.ExecuteCommand("Project.IncludeInProject");

                allItems = solution.WaitForErrorListItems(1);
                Assert.AreEqual(1, allItems.Count);

                excluded = solution.WaitForItem("TestExcludedErrors", "excluded.js");
                AutomationWrapper.Select(excluded);
                solution.ExecuteCommand("Project.ExcludeFromProject");

                allItems = solution.WaitForErrorListItems(0);
                Assert.AreEqual(0, allItems.Count);
            }
        }


        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestDebuggerPort() {
            var filename = Path.Combine(TestData.GetTempPath(), Path.GetRandomFileName());
            Console.WriteLine("Temp file is: {0}", filename);
            var code = String.Format(@"
require('fs').writeFileSync('{0}', process.debugPort);
while(true) {{
}}", filename.Replace("\\", "\\\\"));

            var project = Project("DebuggerPort",
                Compile("server", code),
                Property(NodejsConstants.DebuggerPort, "1234"),
                Property(CommonConstants.StartupFile, "server.js")
            );

            using (var solution = project.Generate().ToVs()) {
                solution.ExecuteCommand("Debug.Start");
                solution.WaitForMode(dbgDebugMode.dbgRunMode);

                for (int i = 0; i < 10 && !File.Exists(filename); i++) {
                    System.Threading.Thread.Sleep(1000);
                }
                Assert.IsTrue(File.Exists(filename), "debugger port not written out");
                solution.ExecuteCommand("Debug.StopDebugging");

                Assert.AreEqual(
                    File.ReadAllText(filename),
                    "1234"
                );
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestEnvironmentVariables() {
            var filename = Path.Combine(TestData.GetTempPath(), Path.GetRandomFileName());
            Console.WriteLine("Temp file is: {0}", filename);
            var code = String.Format(@"
require('fs').writeFileSync('{0}', process.env.fob + process.env.bar + process.env.baz);
while(true) {{
}}", filename.Replace("\\", "\\\\"));

            var project = Project("EnvironmentVariables",
                Compile("server", code),
                Property(NodejsConstants.Environment, "fob=1\nbar=2;3\r\nbaz=4"),
                Property(CommonConstants.StartupFile, "server.js")
            );

            using (var solution = project.Generate().ToVs()) {
                solution.ExecuteCommand("Debug.Start");
                solution.WaitForMode(dbgDebugMode.dbgRunMode);

                for (int i = 0; i < 10 && !File.Exists(filename); i++) {
                    System.Threading.Thread.Sleep(1000);
                }
                Assert.IsTrue(File.Exists(filename), "environment variables not written out");
                solution.ExecuteCommand("Debug.StopDebugging");

                Assert.AreEqual(
                    File.ReadAllText(filename),
                    "12;34"
                );
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestEnvironmentVariablesNoDebugging() {
            var filename = Path.Combine(TestData.GetTempPath(), Path.GetRandomFileName());
            Console.WriteLine("Temp file is: {0}", filename);
            var code = String.Format(@"
require('fs').writeFileSync('{0}', process.env.fob + process.env.bar + process.env.baz);
", filename.Replace("\\", "\\\\"));

            var project = Project("EnvironmentVariables",
                Compile("server", code),
                Property(NodejsConstants.Environment, "fob=1\nbar=2;3\r\nbaz=4"),
                Property(CommonConstants.StartupFile, "server.js")
            );

            using (var solution = project.Generate().ToVs()) {
                solution.ExecuteCommand("Debug.StartWithoutDebugging");

                for (int i = 0; i < 10 && !File.Exists(filename); i++) {
                    System.Threading.Thread.Sleep(1000);
                }
                Assert.IsTrue(File.Exists(filename), "environment variables not written out");

                Assert.AreEqual(
                    File.ReadAllText(filename),
                    "12;34"
                );
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestProjectProperties() {
            var filename = Path.Combine(TestData.GetTempPath(), Path.GetRandomFileName());

            var project = Project("ProjectProperties",
                Compile("server"),
                Property(NodejsConstants.Environment, "fob=1\r\nbar=2;3\nbaz=4"),
                Property(NodejsConstants.DebuggerPort, "1234"),
                Property(CommonConstants.StartupFile, "server.js")
            );

            using (var solution = project.Generate().ToVs()) {
                var projectNode = solution.WaitForItem("ProjectProperties");
                AutomationWrapper.Select(projectNode);

                solution.ExecuteCommand("ClassViewContextMenus.ClassViewMultiselectProjectReferencesItems.Properties");
                AutomationElement doc = null;
                for (int i = 0; i < 10; i++) {
                    doc = ((VisualStudioInstance)solution).App.GetDocumentTab("ProjectProperties");
                    if (doc != null) {
                        break;
                    }
                    System.Threading.Thread.Sleep(1000);
                }
                Assert.IsNotNull(doc, "Failed to find project properties tab");

                var debuggerPort =
                    new TextBox(
                        new AutomationWrapper(doc).FindByAutomationId("_debuggerPort")
                    );
                var envVars = new TextBox(
                    new AutomationWrapper(doc).FindByAutomationId("_envVars")
                );

                Assert.AreEqual(debuggerPort.Value, "1234");
                Assert.AreEqual(envVars.Value, "fob=1\r\nbar=2;3\r\nbaz=4");

                debuggerPort.Value = "2468";

                // Multi-line text box does not support setting value via automation.
                envVars.SetFocus();
                Keyboard.ControlA();
                Keyboard.Backspace();
                Keyboard.Type("fob=0\nbar=0;0\nbaz=0");

                solution.ExecuteCommand("File.SaveAll");

                var projFile = File.ReadAllText(solution.GetProject("ProjectProperties").FullName);
                Assert.AreNotEqual(-1, projFile.IndexOf("<DebuggerPort>2468</DebuggerPort>"));
                Assert.AreNotEqual(-1, projFile.IndexOf("<Environment>fob=0\r\nbar=0;0\r\nbaz=0</Environment>"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestClientServerIntelliSenseModes() {
            string
                solutionLabel = "Solution 'ClientServerCode' (1 project)",
                projectLabel = "ClientServerCode",
                nodeDirectoryLabel = NodejsFolderNode.AppendLabel("NodeDirectory", FolderContentType.Node),
                nodeSubDirectoryLabel = NodejsFolderNode.AppendLabel("NodeSubDirectory", FolderContentType.Node),
                browserDirectoryLabel = NodejsFolderNode.AppendLabel("BrowserDirectory", FolderContentType.Browser),
                emptyBrowserSubDirectoryLabel = "BrowserSubDirectory",
                browserSubDirectoryLabel = NodejsFolderNode.AppendLabel("BrowserSubDirectory", FolderContentType.Browser),
                mixedDirectoryLabel = NodejsFolderNode.AppendLabel("MixedDirectory", FolderContentType.Mixed),
                mixedDirectoryBrowserDirectoryLabel = NodejsFolderNode.AppendLabel("BrowserDirectory", FolderContentType.Browser),
                mixedDirectoryNodeDirectoryLabel = NodejsFolderNode.AppendLabel("NodeDirectory", FolderContentType.Node),
                browserCodeLabel = "browserCode.js",
                mixedDirectoryRenamedLabel = NodejsFolderNode.AppendLabel("MixedDirectoryRenamed", FolderContentType.Mixed);

            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\ClientServerCode\ClientServerCode.sln");

                // Wait until project is loaded
                var solutionExplorer = app.OpenSolutionExplorer();

                solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    "app.js");

                var nodejsProject = app.GetProject("ClientServerCode").GetNodejsProject();

                var projectNode = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel);

                var browserDirectory = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    browserDirectoryLabel
                    );
                Assert.IsNotNull(
                    browserDirectory,
                    "Browser directories should be labeled as such. Could not find " + browserDirectoryLabel);

                var browserSubDirectory = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    browserDirectoryLabel,
                    emptyBrowserSubDirectoryLabel
                );
                Assert.IsNotNull(
                    browserSubDirectory,
                    "Project initialization: could not find " + emptyBrowserSubDirectoryLabel);

                var nodeDirectory = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    nodeDirectoryLabel
                );
                Assert.IsNotNull(
                    nodeDirectory,
                    "Node directories should be labeled as such. Could not find " + nodeDirectoryLabel);

                var nodeSubDirectory = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    nodeDirectoryLabel,
                    nodeSubDirectoryLabel
                );
                Assert.IsNotNull(
                    nodeSubDirectory,
                    "Project initialization: could not find " + nodeSubDirectoryLabel);

                projectNode.Select();
                using (var newItem = NewItemDialog.FromDte(app)) {
                    newItem.FileName = "newItem.js";
                    newItem.OK();
                }

                Assert.AreEqual(
                    "Compile",
                    nodejsProject.GetItemType("newItem.js"),
                    "Top level files should be set to item type 'Compile'");

                Keyboard.Type("process.");
                Keyboard.Type(Keyboard.CtrlSpace.ToString());

                using (var session = app.GetDocument(Path.Combine(nodejsProject.ProjectHome, @"newItem.js")).WaitForSession<ICompletionSession>()) {
                    var completions = session.Session.CompletionSets.First().Completions.Select(x => x.InsertionText);
                    Assert.IsTrue(
                        completions.Contains("env"),
                        "New documents of the node type should open with default VS editor"
                        );
                }

                browserSubDirectory.Select();
                using (var newBrowserItem = NewItemDialog.FromDte(app)) {
                    newBrowserItem.FileName = "newBrowserItem.js";
                    newBrowserItem.OK();
                }

                Keyboard.Type("document.");
                System.Threading.Thread.Sleep(2000);
                Keyboard.Type(Keyboard.CtrlSpace.ToString());

                using (var session = app.GetDocument(Path.Combine(nodejsProject.ProjectHome, @"BrowserDirectory\browserSubDirectory\newBrowserItem.js")).WaitForSession<ICompletionSession>()) {
                    var completions = session.Session.CompletionSets.First().Completions.Select(x => x.InsertionText);
                    Assert.IsTrue(
                        completions.Contains("body"),
                        "New documents of the browser type should open with default VS editor"
                        );
                }

                browserSubDirectory = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    browserDirectoryLabel,
                    browserSubDirectoryLabel
                );
                Assert.IsNotNull(
                    browserSubDirectory,
                    "Folder label was not updated to " + browserSubDirectoryLabel);

                var newBrowserItemFile = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    browserDirectoryLabel,
                    browserSubDirectoryLabel,
                    "newBrowserItem.js"
                );
                Assert.AreEqual(
                    "Content",
                    nodejsProject.GetItemType(@"BrowserDirectory\BrowserSubDirectory\newBrowserItem.js"),
                    "Adding a javascript file to a 'browser' directory should set the item type as Content.");

                var mixedDirectory = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    mixedDirectoryLabel
                );
                Assert.IsNotNull(
                    mixedDirectory,
                    "Folder with mixed browser/node content should be specified as such. Could not find: " + mixedDirectoryLabel);

                nodeDirectory.Select();
                using (var newTypeScriptItem = NewItemDialog.FromDte(app)) {
                    newTypeScriptItem.FileName = "newTypeScriptItem.ts";
                    newTypeScriptItem.OK();
                }

                Assert.AreEqual(
                    "TypeScriptCompile",
                    nodejsProject.GetItemType(@"NodeDirectory\newTypeScriptItem.ts"),
                    "Non-javascript files should retain their content type.");

                var newBrowserItemNode = nodejsProject.FindNodeByFullPath(
                    Path.Combine(nodejsProject.ProjectHome, @"BrowserDirectory\BrowserSubDirectory\newBrowserItem.js")
                );

                newBrowserItemNode.ExcludeFromProject();
                var excludedBrowserItem = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    browserDirectoryLabel,
                    emptyBrowserSubDirectoryLabel
                );
                Assert.IsNotNull(
                    emptyBrowserSubDirectoryLabel,
                    "Label should be removed when there are no included javascript files the directory. Could not find " + emptyBrowserSubDirectoryLabel);

                (newBrowserItemNode as NodejsFileNode).IncludeInProject(false);
                var includedBrowserItem = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    browserDirectoryLabel,
                    browserSubDirectoryLabel
                );
                Assert.IsNotNull(
                    includedBrowserItem,
                    "Label should be added when a javascript file is included in the directory. Could not find " + browserSubDirectoryLabel);

                var mixedDirectoryNode = app.GetProject("ClientServerCode").GetNodejsProject().FindNodeByFullPath(
                    Path.Combine(nodejsProject.ProjectHome, @"MixedDirectory\"));

                System.Threading.Thread.Sleep(2000);
                mixedDirectory.Select();
                Keyboard.PressAndRelease(Key.F2);
                Keyboard.Type("MixedDirectoryRenamed");
                Keyboard.PressAndRelease(Key.Enter);

                mixedDirectoryNode.ExpandItem(EXPANDFLAGS.EXPF_ExpandFolderRecursively);
                var renamedMixedDirectory = solutionExplorer.WaitForItem(
                   solutionLabel,
                   projectLabel,
                   mixedDirectoryRenamedLabel,
                   mixedDirectoryBrowserDirectoryLabel,
                   browserCodeLabel
               );

                Assert.IsNotNull(
                    renamedMixedDirectory,
                    "Renaming mixed directory failed: could not find " + browserCodeLabel);

                newBrowserItemNode.ItemNode.ItemTypeName = "Compile";

                var nodeBrowserSubDirectory = solutionExplorer.WaitForItem(
                      solutionLabel,
                      projectLabel,
                      NodejsFolderNode.AppendLabel("BrowserDirectory", FolderContentType.Mixed),
                      NodejsFolderNode.AppendLabel("BrowserSubDirectory", FolderContentType.Node)
                );
                Assert.IsNotNull(
                    nodeBrowserSubDirectory,
                    "Changing the item type should change the directory label. Could not find " +
                    NodejsFolderNode.AppendLabel("BrowserSubDirectory", FolderContentType.Node)
                    );

                var nodeDirectoryNode = app.GetProject("ClientServerCode").GetNodejsProject().FindNodeByFullPath(
                    Path.Combine(app.GetProject("ClientServerCode").GetNodejsProject().ProjectHome,
                    @"NodeDirectory\"));
                (nodeDirectoryNode as NodejsFolderNode).SetItemTypeRecursively(VSLangProj.prjBuildAction.prjBuildActionContent);

                Assert.AreEqual(
                    "Content",
                    nodejsProject.GetItemType(@"NodeDirectory\NodeSubDirectory\nodeCode.js"),
                    "nodeCode.js file should be marked as content after recursively setting directory contents as Content."
                );
                Assert.AreEqual(
                    "TypeScriptCompile",
                    nodejsProject.GetItemType(@"NodeDirectory\newTypeScriptItem.ts"),
                    "Only javascript file item types should change when marking item types recursively."
                );

                var fromPoint = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    mixedDirectoryRenamedLabel,
                    mixedDirectoryNodeDirectoryLabel
                ).GetClickablePoint();

                var toPoint = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    mixedDirectoryRenamedLabel,
                    mixedDirectoryBrowserDirectoryLabel
                ).GetClickablePoint();

                Mouse.MoveTo(fromPoint);
                Mouse.Down(MouseButton.Left);

                Mouse.MoveTo(toPoint);
                Mouse.Up(MouseButton.Left);

                var draggedDirectory = solutionExplorer.WaitForItem(
                    solutionLabel,
                    projectLabel,
                    mixedDirectoryRenamedLabel,
                    NodejsFolderNode.AppendLabel("BrowserDirectory", FolderContentType.Mixed),
                    NodejsFolderNode.AppendLabel("NodeDirectory", FolderContentType.Node)
                );
                Assert.IsNotNull(
                    draggedDirectory,
                    "Labels not properly updated after dragging and dropping directory."
                );
            }
        }
    }
}
