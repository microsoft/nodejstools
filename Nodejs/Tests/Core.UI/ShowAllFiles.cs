/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using EnvDTE;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Project;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools.Project;
using TestUtilities;
using TestUtilities.Nodejs;
using TestUtilities.UI;
using Keyboard = TestUtilities.UI.Keyboard;
using Mouse = TestUtilities.UI.Mouse;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class ShowAllFiles {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            NodejsTestData.Deploy();
        }

        [TestCleanup]
        public void MyTestCleanup() {
            for (int i = 0; i < 100; i++) {
                try {
                    VsIdeTestHostContext.Dte.Solution.Close(false);
                    break;
                } catch {
                    VsIdeTestHostContext.Dte.Documents.CloseAll(EnvDTE.vsSaveChanges.vsSaveChangesNo);
                    System.Threading.Thread.Sleep(200);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllFilesToggle() {
            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFilesToggle.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var projectNode = window.WaitForItem("Solution 'ShowAllFilesToggle' (1 project)", "HelloWorld", "SubFolder", "server.js");
            AutomationWrapper.Select(projectNode);

            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // start showing all

            Assert.IsTrue(GetIsFolderExpanded(project, "SubFolder"));
        }

        internal static bool GetIsFolderExpanded(Project project, string folder) {
            return GetNodeState(project, folder, __VSHIERARCHYITEMSTATE.HIS_Expanded);
        }

        internal static bool GetIsItemBolded(Project project, string item) {
            return GetNodeState(project, item, __VSHIERARCHYITEMSTATE.HIS_Bold);
        }

        internal static bool GetNodeState(Project project, string item, __VSHIERARCHYITEMSTATE state) {
            var itemNode = (HierarchyNode)project.ProjectItems.Item(item).Properties.Item("Node").Value;
            var id = itemNode.ID;

            // make sure we're still expanded.
            var solutionWindow = UIHierarchyUtilities.GetUIHierarchyWindow(
                VsIdeTestHostContext.ServiceProvider,
                new Guid(ToolWindowGuids80.SolutionExplorer)
            );

            uint result;
            ErrorHandler.ThrowOnFailure(
                solutionWindow.GetItemState(
                    itemNode.ProjectMgr.GetOuterInterface<IVsUIHierarchy>(),
                    id,
                    (uint)state,
                    out result
                )
            );
            return (result & (uint)state) != 0;
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllFilesFilesAlwaysHidden() {
            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFiles.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var projectNode = window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld");

            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "ShowAllFiles.sln"));
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "ShowAllFiles.suo"));
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllFilesSymLinks() {
            using(System.Diagnostics.Process p = System.Diagnostics.Process.Start("cmd.exe",
                String.Format("/c mklink /J \"{0}\" \"{1}\"", 
                    TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesSymLink\SymFolder"), 
                    TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesSymLink\SubFolder")
                ))) {
                p.WaitForExit();
            }

            using(System.Diagnostics.Process p = System.Diagnostics.Process.Start("cmd.exe",
                String.Format("/c mklink /J \"{0}\" \"{1}\"",
                    TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesSymLink\SubFolder\Infinite"),
                    TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesSymLink\SubFolder")
                ))) {                
                p.WaitForExit();                
            }

            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFilesSymLink.sln");
            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesSymLink' (1 project)", "HelloWorld", "SymFolder"));

            // https://pytools.codeplex.com/workitem/1150 - infinite links, not displayed
            Assert.IsNull(window.FindItem("Solution 'ShowAllFilesSymLink' (1 project)", "HelloWorld", "SubFolder", "Infinite"));

            File.WriteAllText(
                TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesSymLink\SubFolder\Foo.txt"),
                "Hi!"
            );

            // https://pytools.codeplex.com/workitem/1152 - watching the sym link folder
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesSymLink' (1 project)", "HelloWorld", "SubFolder", "Foo.txt"));
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesSymLink' (1 project)", "HelloWorld", "SymFolder", "Foo.txt"));
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllFilesLinked() {
            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFilesLinked.sln");
            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var linkedNode = window.WaitForItem("Solution 'ShowAllFilesLinked' (1 project)", "HelloWorld", "File.js");
            AutomationWrapper.Select(linkedNode);
            Keyboard.ControlC();

            var subFolder = window.WaitForItem("Solution 'ShowAllFilesLinked' (1 project)", "HelloWorld", "SubFolder");
            AutomationWrapper.Select(subFolder);

            Keyboard.ControlV();
            VisualStudioApp.CheckMessageBox("Cannot copy linked files within the same project. You cannot have more than one link to the same file in a project.");

            linkedNode = window.WaitForItem("Solution 'ShowAllFilesLinked' (1 project)", "HelloWorld", "SubFolder", "LinkedFile.js");
            AutomationWrapper.Select(linkedNode);

            Keyboard.ControlX();

            var projectNode = window.WaitForItem("Solution 'ShowAllFilesLinked' (1 project)", "HelloWorld");
            AutomationWrapper.Select(projectNode);

            Keyboard.ControlV();
            project.Save();

            var text = File.ReadAllText(@"TestData\NodejsProjectData\ShowAllFilesLinked\HelloWorld.njsproj");
            Assert.IsTrue(text.IndexOf("<Link>LinkedFile.js</Link>") != -1);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllFilesIncludeExclude() {
            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFilesIncludeExclude.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var projectNode = window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld");

            var excludedFolder = project.ProjectItems.Item("ExcludeFolder1");
            var itemTxt = excludedFolder.ProjectItems.Item("Item.txt");
            var buildAction = itemTxt.Properties.Item("BuildAction");
            Assert.IsNotNull(buildAction);

            var notInProject = window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "NotInProject.js");
            AutomationWrapper.Select(notInProject);
            
            try {
                app.Dte.ExecuteCommand("Project.SetasNode.jsStartupFile");
                Assert.Fail("Successfully set startup file on excluded item");
            } catch (COMException) {
            }

            var folder = window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "ExcludeFolder1");
            AutomationWrapper.Select(folder);
            app.Dte.ExecuteCommand("Project.ExcludeFromProject");

            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // stop showing all

            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "ExcludeFolder1"));

            AutomationWrapper.Select(projectNode);
            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // start showing all again
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "ExcludeFolder1"));
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "ExcludeFolder1", "Item.txt"));

            // https://nodejstools.codeplex.com/workitem/250
            var linkedFile = window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "LinkedFile.js");
            AutomationWrapper.Select(linkedFile);
            app.Dte.ExecuteCommand("Project.ExcludeFromProject");
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "LinkedFile.js"));

            // https://pytools.codeplex.com/workitem/1153
            // Shouldn't have a BuildAction property on excluded items
            try {
                project.ProjectItems.Item("ExcludedFolder1").ProjectItems.Item("Item.txt").Properties.Item("BuildAction");
                Assert.Fail("Excluded item had BuildAction");
            } catch (ArgumentException) {
            }

            var file = window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "ExcludeFolder2", "Item.txt");
            AutomationWrapper.Select(file);
            app.Dte.ExecuteCommand("Project.ExcludeFromProject");

            AutomationWrapper.Select(projectNode);
            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // stop showing all
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "ExcludeFolder2", "Item.txt"));
            AutomationWrapper.Select(projectNode);
            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // start showing all

            var itemTxtNode = window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "ExcludeFolder2", "Item.txt");
            Assert.IsNotNull(itemTxtNode);

            AutomationWrapper.Select(itemTxtNode);

            // https://pytools.codeplex.com/workitem/1143
            try {
                VsIdeTestHostContext.Dte.ExecuteCommand("Project.AddNewItem");
                Assert.Fail("Added a new item on excluded node");
            } catch (COMException) {
            }

            var excludedFolderNode = window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "ExcludeFolder1");
            Assert.IsNotNull(excludedFolderNode);
            AutomationWrapper.Select(excludedFolderNode);
            try {
                VsIdeTestHostContext.Dte.ExecuteCommand("Project.NewFolder");
                Assert.Fail("Added a new folder on excluded node");
            } catch (COMException) {
            }

            // include
            folder = window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "IncludeFolder1");
            AutomationWrapper.Select(folder);
            app.Dte.ExecuteCommand("Project.IncludeInProject");
            AutomationWrapper.Select(projectNode);
            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // stop showing all

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "IncludeFolder1"));
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "IncludeFolder1", "Item.txt"));

            // https://nodejstools.codeplex.com/workitem/242
            // Rename Item.txt on disk
            File.Move(
                TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesIncludeExclude\IncludeFolder1\Item.txt"),
                TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesIncludeExclude\IncludeFolder1\ItemNew.txt")
            );

            var includedItem = window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "IncludeFolder1", "Item.txt");
            AutomationWrapper.Select(includedItem);
            app.Dte.ExecuteCommand("Project.ExcludeFromProject");

            // Rename it back
            File.Move(
                TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesIncludeExclude\IncludeFolder1\ItemNew.txt"),
                TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesIncludeExclude\IncludeFolder1\Item.txt")
            );

            AutomationWrapper.Select(projectNode);
            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // start showing all

            // item should be back
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "IncludeFolder1", "Item.txt"));

            folder = window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "IncludeFolder2", "Item.txt");
            AutomationWrapper.Select(folder);
            app.Dte.ExecuteCommand("Project.IncludeInProject");
            AutomationWrapper.Select(projectNode);
            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // stop showing all

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "IncludeFolder2"));
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "IncludeFolder2", "Item.txt"));
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "IncludeFolder2", "Item2.txt"));

            AutomationWrapper.Select(projectNode);
            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // start showing all

            // exclude an item which exists, but is not on disk, it should be removed
            var notOnDisk = window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "NotOnDisk.js");
            AutomationWrapper.Select(notOnDisk);
            app.Dte.ExecuteCommand("Project.ExcludeFromProject");
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "NotOnDisk.js"));

            var notOnDiskFolder = window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "NotOnDiskFolder");
            AutomationWrapper.Select(notOnDiskFolder);
            app.Dte.ExecuteCommand("Project.ExcludeFromProject");
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "NotOnDiskFolder"));

            // https://pytools.codeplex.com/workitem/1138
            var server = window.FindItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "server.js");

            AutomationWrapper.Select(server);
            Keyboard.ControlC();
            System.Threading.Thread.Sleep(1000);

            var includeFolder3 = window.FindItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "IncludeFolder3");
            AutomationWrapper.Select(includeFolder3);

            Keyboard.ControlV();
            System.Threading.Thread.Sleep(1000);

            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // stop showing all

            // folder should now be included
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "IncludeFolder3"));

            // https://nodejstools.codeplex.com/workitem/250
            // Excluding the startup item, and then including it again, it should be bold
            server = window.FindItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "server.js");
            AutomationWrapper.Select(server);
            app.Dte.ExecuteCommand("Project.ExcludeFromProject");

            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "server.js"));
            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // start showing all
            server = window.FindItem("Solution 'ShowAllFilesIncludeExclude' (1 project)", "HelloWorld", "server.js");
            AutomationWrapper.Select(server);

            app.Dte.ExecuteCommand("Project.IncludeInProject");
            System.Threading.Thread.Sleep(2000);

            Assert.IsTrue(GetIsItemBolded(project, "server.js"));
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void DumpProperties() {
            var project = BasicProjectTests.OpenProject(@"C:\Source\ConsoleApplication7\ConsoleApplication7.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;
            window.WaitForItem("Solution 'ConsoleApplication7' (1 project)", "ConsoleApplication7", "Program - Copy.cs");
            var item = project.ProjectItems.Item("Program - Copy.cs");
            foreach (var x in item.Properties) {
                Console.WriteLine(x);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllFilesChanges() {
            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFiles.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;
            
            var projectNode = window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld");
            AutomationWrapper.Select(projectNode);

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "NotInProject.js"));

            // everything should be there...
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "Folder"));
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "Folder", "File.js"));
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "Folder", "File.txt"));
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "Folder", "SubFolder"));
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "Folder", "SubFolder", "SubFile.txt"));

            // create some stuff, it should show up...
            File.WriteAllText(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\NewFile.txt"), "");
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "NewFile.txt"));

            File.WriteAllText(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\Folder\NewFile.txt"), "");
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "Folder", "NewFile.txt"));

            File.WriteAllText(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\Folder\SubFolder\NewFile.txt"), "");
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "Folder", "SubFolder", "NewFile.txt"));

            Directory.CreateDirectory(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\NewFolder"));
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "NewFolder"));

            Directory.CreateDirectory(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\NewFolder\SubFolder"));
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "NewFolder", "SubFolder"));

            File.WriteAllText(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\NewFolder\SubFolder\NewFile.txt"), "");
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "NewFolder", "SubFolder", "NewFile.txt"));

            // delete some stuff, it should go away
            File.Delete(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\Folder\File.txt"));
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "Folder", "File.txt"));

            File.Delete(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\NewFile.txt"));
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "NewFile.txt"));

            File.Delete(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\NewFolder\NewFile.txt"));
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "NewFolder", "NewFile.txt"));

            File.Delete(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\NewFolder\SubFolder\NewFile.txt"));
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "NewFolder", "SubFolder", "NewFile.txt"));

            Directory.Delete(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\NewFolder\SubFolder"));
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "NewFolder", "SubFolder"));

            Directory.Delete(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\NewFolder"));
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "NewFolder"));

            Directory.Move(
                TestData.GetPath(@"TestData\NodejsProjectData\MovedIntoShowAllFiles"),
                TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\MovedIntoShowAllFiles")
            );

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "MovedIntoShowAllFiles", "Text.txt"));

            // move it back
            Directory.Move(
                TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\MovedIntoShowAllFiles"),
                TestData.GetPath(@"TestData\NodejsProjectData\MovedIntoShowAllFiles")
            );

            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "MovedIntoShowAllFiles", "Text.txt"));

            // and move it back into the project one more time
            Directory.Move(
                TestData.GetPath(@"TestData\NodejsProjectData\MovedIntoShowAllFiles"),
                TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFiles\MovedIntoShowAllFiles")
            );

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFiles' (1 project)", "HelloWorld", "MovedIntoShowAllFiles", "Text.txt"));
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllFilesHiddenFiles() {
            var file = TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesHiddenFiles\NotInProject.js");
            File.SetAttributes(
                file,
                File.GetAttributes(file) | FileAttributes.Hidden
            );
            file = TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesHiddenFiles\Folder\File.txt");
            File.SetAttributes(
                file,
                File.GetAttributes(file) | FileAttributes.Hidden
            );
            file = TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesHiddenFiles\HiddenFolder");
            File.SetAttributes(
                file,
                File.GetAttributes(file) | FileAttributes.Hidden
            );
            
            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFilesHiddenFiles.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var projectNode = window.WaitForItem("Solution 'ShowAllFilesHiddenFiles' (1 project)", "HelloWorld");

            // hidden files/folders shouldn't be visible
            Assert.IsNull(window.FindItem("Solution 'ShowAllFilesHiddenFiles' (1 project)", "HelloWorld", "NotInProject.js"));
            Assert.IsNull(window.FindItem("Solution 'ShowAllFilesHiddenFiles' (1 project)", "HelloWorld", "Folder", "File.txt"));
            Assert.IsNull(window.FindItem("Solution 'ShowAllFilesHiddenFiles' (1 project)", "HelloWorld", "HiddenFolder"));

            // but if they change back, they should be
            file = TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesHiddenFiles\NotInProject.js");
            File.SetAttributes(
                file,
                File.GetAttributes(file)  & ~FileAttributes.Hidden
            );

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesHiddenFiles' (1 project)", "HelloWorld", "NotInProject.js"));
            file = TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesHiddenFiles\Folder\File.txt");
            File.SetAttributes(
                file,
                File.GetAttributes(file) & ~FileAttributes.Hidden
            );

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesHiddenFiles' (1 project)", "HelloWorld", "Folder", "File.txt"));
            file = TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesHiddenFiles\HiddenFolder");
            File.SetAttributes(
                file,
                File.GetAttributes(file) & ~FileAttributes.Hidden
            );
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesHiddenFiles' (1 project)", "HelloWorld", "HiddenFolder"));

            // changing non-hidden items to hidden should cause them to be removed
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesHiddenFiles' (1 project)", "HelloWorld", "NotInProject2.js"));
            file = TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesHiddenFiles\NotInProject2.js");
            File.SetAttributes(
                file,
                File.GetAttributes(file) | FileAttributes.Hidden
            );
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesHiddenFiles' (1 project)", "HelloWorld", "NotInProject2.js"));

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesHiddenFiles' (1 project)", "HelloWorld", "Folder"));
            file = TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesHiddenFiles\Folder");
            File.SetAttributes(
                file,
                File.GetAttributes(file) | FileAttributes.Hidden
            );
            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesHiddenFiles' (1 project)", "HelloWorld", "Folder"));
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllFilesOnPerUser() {
            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFilesOnPerUser.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;
            
            var projectNode = window.WaitForItem("Solution 'ShowAllFilesOnPerUser' (1 project)", "HelloWorld");
            AutomationWrapper.Select(projectNode);

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesOnPerUser' (1 project)", "HelloWorld", "NotInProject.js"));

            // change setting, UI should be updated
            app.Dte.ExecuteCommand("Project.ShowAllFiles");

            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesOnPerUser' (1 project)", "HelloWorld", "NotInProject.js"));

            // save setting, user project file should be updated
            project.Save();

            var projectText = File.ReadAllText(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesOnPerUser\HelloWorld.njsproj.user"));
            Assert.IsTrue(projectText.Contains("<ProjectView>ProjectFiles</ProjectView>"));
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllFilesOnPerProject() {
            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFilesOnPerProject.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var projectNode = window.WaitForItem("Solution 'ShowAllFilesOnPerProject' (1 project)", "HelloWorld");
            AutomationWrapper.Select(projectNode);

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesOnPerProject' (1 project)", "HelloWorld", "NotInProject.js"));

            // change setting, UI should be updated
            app.Dte.ExecuteCommand("Project.ShowAllFiles");

            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesOnPerProject' (1 project)", "HelloWorld", "NotInProject.js"));

            // save setting, project file should be updated
            project.Save();

            var projectText = File.ReadAllText(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesOnPerProject\HelloWorld.njsproj"));
            Assert.IsTrue(projectText.Contains("<ProjectView>ProjectFiles</ProjectView>"));
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllFilesOffPerUser() {
            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFilesOffPerUser.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var projectNode = window.WaitForItem("Solution 'ShowAllFilesOffPerUser' (1 project)", "HelloWorld");
            AutomationWrapper.Select(projectNode);

            Assert.IsNull(window.FindItem("Solution 'ShowAllFilesOffPerUser' (1 project)", "HelloWorld", "NotInProject.js"));

            // change setting, UI should be updated
            app.Dte.ExecuteCommand("Project.ShowAllFiles");

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesOffPerUser' (1 project)", "HelloWorld", "NotInProject.js"));

            // save setting, user project file should be updated
            project.Save();

            var projectText = File.ReadAllText(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesOffPerUser\HelloWorld.njsproj.user"));
            Assert.IsTrue(projectText.Contains("<ProjectView>ShowAllFiles</ProjectView>"));
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllFilesOffPerProject() {
            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFilesOffPerProject.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var projectNode = window.WaitForItem("Solution 'ShowAllFilesOffPerProject' (1 project)", "HelloWorld");
            AutomationWrapper.Select(projectNode);

            Assert.IsNull(window.FindItem("Solution 'ShowAllFilesOffPerProject' (1 project)", "HelloWorld", "NotInProject.js"));

            // change setting, UI should be updated
            app.Dte.ExecuteCommand("Project.ShowAllFiles");

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesOffPerProject' (1 project)", "HelloWorld", "NotInProject.js"));

            // save setting, project file should be updated
            project.Save();

            var projectText = File.ReadAllText(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesOffPerProject\HelloWorld.njsproj"));
            Assert.IsTrue(projectText.Contains("<ProjectView>ShowAllFiles</ProjectView>"));
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllFilesDefault() {
            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFilesDefault.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var projectNode = window.WaitForItem("Solution 'ShowAllFilesDefault' (1 project)", "HelloWorld");
            AutomationWrapper.Select(projectNode);

            
            Assert.IsNull(window.FindItem("Solution 'ShowAllFilesDefault' (1 project)", "HelloWorld", "NotInProject.js"));

            // change setting, UI should be updated
            app.Dte.ExecuteCommand("Project.ShowAllFiles");

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesDefault' (1 project)", "HelloWorld", "NotInProject.js"));

            // save setting, user project file should be updated
            project.Save();

            var projectText = File.ReadAllText(TestData.GetPath(@"TestData\NodejsProjectData\ShowAllFilesDefault\HelloWorld.njsproj.user"));
            Assert.IsTrue(projectText.Contains("<ProjectView>ShowAllFiles</ProjectView>"));
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/240
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ShowAllMoveNotInProject() {
            var project = BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\ShowAllFilesMoveNotInProject.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            window.WaitForItem("Solution 'ShowAllFilesMoveNotInProject' (1 project)", "HelloWorld");

            var file = window.WaitForItem("Solution 'ShowAllFilesMoveNotInProject' (1 project)", "HelloWorld", "NotInProject.js");
            AutomationWrapper.Select(file);
            Keyboard.ControlX();
            System.Threading.Thread.Sleep(1000);

            var folder = window.WaitForItem("Solution 'ShowAllFilesMoveNotInProject' (1 project)", "HelloWorld", "Folder");
            AutomationWrapper.Select(folder);
            Keyboard.ControlV();

            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesMoveNotInProject' (1 project)", "HelloWorld", "Folder", "NotInProject.js"));

            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // stop showing all

            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesMoveNotInProject' (1 project)", "HelloWorld", "Folder", "NotInProject.js"));

            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // start showing again

            var subFolder = window.WaitForItem("Solution 'ShowAllFilesMoveNotInProject' (1 project)", "HelloWorld", "Folder", "SubFolder");
            AutomationWrapper.Select(subFolder);

            Keyboard.ControlX();
            System.Threading.Thread.Sleep(1000);
            var projectNode = window.WaitForItem("Solution 'ShowAllFilesMoveNotInProject' (1 project)", "HelloWorld");
            AutomationWrapper.Select(projectNode);
            Keyboard.ControlV();
            Assert.IsNotNull(window.WaitForItem("Solution 'ShowAllFilesMoveNotInProject' (1 project)", "HelloWorld", "SubFolder"));

            app.Dte.ExecuteCommand("Project.ShowAllFiles"); // stop showing all

            Assert.IsNull(window.WaitForItemRemoved("Solution 'ShowAllFilesMoveNotInProject' (1 project)", "HelloWorld", "SubFolder"));
        }
    }
}