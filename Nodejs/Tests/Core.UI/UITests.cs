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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;
using TestUtilities.UI;
using Keyboard = TestUtilities.UI.Keyboard;
using Mouse = TestUtilities.UI.Mouse;
using Path = System.IO.Path;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class UITests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

#if FALSE // Deferred projects currently aren't enabled
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void DeferredSaveWithDot() {
            // http://pytools.codeplex.com/workitem/623
            // enable deferred saving on projects

            using (var app = new VisualStudioApp()) {
                var props = app.Dte.get_Properties("Environment", "ProjectsAndSolution");
                var prevValue = props.Item("SaveNewProjects").Value;
                props.Item("SaveNewProjects").Value = false;
                app.OnDispose(() => props.Item("SaveNewProjects").Value = prevValue);

                // now run the test
                var newProjDialog = app.FileNewProject();

                newProjDialog.FocusLanguageNode("JavaScript");

                var consoleApp = newProjDialog.ProjectTypes.FindItem("Blank Node.js Application");
                consoleApp.Select();
                newProjDialog.ProjectName = "Fob.Baz";
                newProjDialog.ClickOK();

                // wait for new solution to load...
                for (int i = 0; i < 100 && app.Dte.Solution.Projects.Count == 0; i++) {
                    System.Threading.Thread.Sleep(1000);
                }

                TestUtils.DteExecuteCommandOnThreadPool("File.SaveAll");

                var saveProjDialog = new SaveProjectDialog(app.WaitForDialog());
                saveProjDialog.Save();

                app.WaitForDialogDismissed();

                var fullname = app.Dte.Solution.FullName;
                app.Dte.Solution.Close(false);

                Directory.Delete(Path.GetDirectoryName(fullname), true);
            }
        }
#endif

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void AbsolutePaths() {
            var proj = File.ReadAllText(TestData.GetPath(@"TestData\NodejsProjectData\AbsolutePath\AbsolutePath.njsproj"));
            proj = proj.Replace("[ABSPATH]", TestData.GetPath(@"TestData\NodejsProjectData\AbsolutePath"));
            File.WriteAllText(TestData.GetPath(@"TestData\NodejsProjectData\AbsolutePath\AbsolutePath.njsproj"), proj);


            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\AbsolutePath.sln");
                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var programPy = window.WaitForItem("Solution 'AbsolutePath' (1 project)", "AbsolutePath", "server.js");
                Assert.AreNotEqual(null, programPy);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void MoveStartupFile() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\MoveStartupFile.sln");
                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var server = window.WaitForItem("Solution 'MoveStartupFile' (1 project)", "HelloWorld", "server.js");
                var folder = window.WaitForItem("Solution 'MoveStartupFile' (1 project)", "HelloWorld", "TestDir");

                AutomationWrapper.Select(server);
                Keyboard.ControlX();

                AutomationWrapper.Select(folder);
                Keyboard.ControlV();

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'MoveStartupFile' (1 project)", "HelloWorld", "TestDir", "server.js"));

                Assert.IsTrue(((string)project.Properties.Item("StartupFile").Value).EndsWith("TestDir\\server.js"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void CopyPasteFile() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var programPy = window.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld", "server.js");

                AutomationWrapper.Select(programPy);

                Keyboard.ControlC();
                Keyboard.ControlV();

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld", "server - Copy.js"));

                AutomationWrapper.Select(programPy);
                Keyboard.ControlC();
                Keyboard.ControlV();

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld", "server - Copy (2).js"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void DeleteFile() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\DeleteFile.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var programPy = window.WaitForItem("Solution 'DeleteFile' (1 project)", "HelloWorld", "server.js");
                Assert.IsTrue(File.Exists(@"TestData\NodejsProjectData\DeleteFile\server.js"));
                AutomationWrapper.Select(programPy);

                Keyboard.Type(Key.Delete);
                app.WaitForDialog();
                VisualStudioApp.CheckMessageBox(MessageBoxButton.Ok, "will be deleted permanently");
                app.WaitForDialogDismissed();

                window.WaitForItemRemoved("Solution 'DeleteFile' (1 project)", "HelloWorld", "server.js");

                Assert.IsFalse(File.Exists(@"TestData\NodejsProjectData\DeleteFile\server.js"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void AddNewFolder() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var projectNode = window.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld");
                AutomationWrapper.Select(projectNode);

                var startingDirs = new HashSet<string>(Directory.GetDirectories(@"TestData\NodejsProjectData\HelloWorld"), StringComparer.OrdinalIgnoreCase);
                Keyboard.PressAndRelease(Key.F10, Key.LeftCtrl, Key.LeftShift);
                Keyboard.PressAndRelease(Key.D);
                Keyboard.PressAndRelease(Key.Right);
                Keyboard.PressAndRelease(Key.D);
                Keyboard.Type("MyNewFolder");
                var curDirs = new HashSet<string>(Directory.GetDirectories(@"TestData\NodejsProjectData\HelloWorld"), StringComparer.OrdinalIgnoreCase);
                Assert.IsTrue(curDirs.IsSubsetOf(startingDirs) && startingDirs.IsSubsetOf(curDirs), "new directory created" + String.Join(", ", curDirs) + " vs. " + String.Join(", ", startingDirs));

                Keyboard.PressAndRelease(Key.Enter);

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld", "MyNewFolder"));

                Assert.IsTrue(Directory.Exists(@"TestData\NodejsProjectData\HelloWorld\MyNewFolder"));
            }
        }

        /// <summary>
        /// Adds a new folder which fits exactly w/ no space left in the path name
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void AddNewFolderLongPathBoundary() {
            using (var app = new VisualStudioApp()) {
                var project = OpenLongFileNameProject(app, 24);

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var projectNode = window.WaitForItem("Solution 'LongFileNames' (1 project)", "LFN");
                AutomationWrapper.Select(projectNode);

                Keyboard.PressAndRelease(Key.F10, Key.LeftCtrl, Key.LeftShift);
                Keyboard.PressAndRelease(Key.D);
                Keyboard.PressAndRelease(Key.Right);
                Keyboard.PressAndRelease(Key.D);
                Keyboard.Type("01234567891");
                Keyboard.PressAndRelease(Key.Enter);

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'LongFileNames' (1 project)", "LFN", "01234567891"));

                var projectLoc = Path.GetDirectoryName(project.FullName);
                var checkedPath = Path.Combine(projectLoc, "LongFileNames", "01234567891");

                Assert.IsTrue(Directory.Exists(checkedPath), checkedPath + " does not exist");
            }
        }

        /// <summary>
        /// Adds a new folder with a path that's too long, typing a new path.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void AddNewFolderLongPathTooLong() {
            using (var app = new VisualStudioApp()) {
                var project = OpenLongFileNameProject(app, 24);

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var projectNode = window.WaitForItem("Solution 'LongFileNames' (1 project)", "LFN");
                AutomationWrapper.Select(projectNode);

                Keyboard.PressAndRelease(Key.F10, Key.LeftCtrl, Key.LeftShift);
                Keyboard.PressAndRelease(Key.D);
                Keyboard.PressAndRelease(Key.Right);
                Keyboard.PressAndRelease(Key.D);
                Keyboard.Type("012345678912");
                Keyboard.PressAndRelease(Key.Enter);

                VisualStudioApp.CheckMessageBox("The filename or extension is too long.");
            }
        }

        /// <summary>
        /// Adds a folder with a path that's too long using the default provided folder name.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void AddNewFolderLongPathTooLongCancelEdit() {
            using (var app = new VisualStudioApp()) {
                var project = OpenLongFileNameProject(app, 21);

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var projectNode = window.WaitForItem("Solution 'LongFileNames' (1 project)", "LFN");
                AutomationWrapper.Select(projectNode);

                Keyboard.PressAndRelease(Key.F10, Key.LeftCtrl, Key.LeftShift);
                Keyboard.PressAndRelease(Key.D);
                Keyboard.PressAndRelease(Key.Right);
                Keyboard.PressAndRelease(Key.D);
                Keyboard.PressAndRelease(Key.Escape);

                VisualStudioApp.CheckMessageBox("The filename or extension is too long.");
            }
        }

        /// <summary>
        /// Adds a folder with a path that's too long using the default provided folder name.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void AddNewItemLongPathBoundary() {
            using (var app = new VisualStudioApp()) {
                var project = OpenLongFileNameProject(app, 12);

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var projectNode = window.WaitForItem("Solution 'LongFileNames' (1 project)", "LFN");
                AutomationWrapper.Select(projectNode);

                using (var newItem = NewItemDialog.FromDte(app)) {
                    newItem.FileName = "NewJSFil.js";
                    newItem.OK();
                }

                Assert.IsNotNull(window.WaitForItem("Solution 'LongFileNames' (1 project)", "LFN", "NewJSFil.js"));
                Assert.IsTrue(File.Exists(Path.Combine(Path.GetDirectoryName(project.FullName), "LongFileNames", "NewJSFil.js")));
            }
        }

        /// <summary>
        /// Adds a folder with a path that's too long using the default provided folder name.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void AddNewItemLongPathTooLong() {
            using (var app = new VisualStudioApp()) {
                var project = OpenLongFileNameProject(app, 12);

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var projectNode = window.WaitForItem("Solution 'LongFileNames' (1 project)", "LFN");
                AutomationWrapper.Select(projectNode);

                using (var newItem = NewItemDialog.FromDte(app)) {
                    newItem.FileName = "NewJSFile.js";
                    newItem.OK();
                }

                VisualStudioApp.CheckMessageBox("The filename or extension is too long.");
            }
        }

        /// <summary>
        /// Adds a folder with a path that's too long using the default provided folder name.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void DeleteLockedFolder() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\DeleteLockedFolder.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                var folderNode = window.WaitForItem("Solution 'DeleteLockedFolder' (1 project)", "DeleteLockedFolder", "Folder");
                AutomationWrapper.Select(folderNode);

                var psi = new ProcessStartInfo(
                    Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "system32", "cmd.exe"));
                psi.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, @"TestData\NodejsProjectData\DeleteLockedFolder\Folder");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                using (var process = System.Diagnostics.Process.Start(psi)) {
                    try {
                        //Ensure the other process started and has time to lock the file
                        System.Threading.Thread.Sleep(1000);
                        Keyboard.Type(Key.Delete);
                        app.WaitForDialog();
                        Keyboard.Type(Key.Enter);
                        System.Threading.Thread.Sleep(500);

                        VisualStudioApp.CheckMessageBox("The process cannot access the file 'Folder' because it is being used by another process.");
                    } finally {
                        process.Kill();
                    }
                }

                Assert.IsNotNull(window.FindItem("Solution 'DeleteLockedFolder' (1 project)", "DeleteLockedFolder", "Folder"));
            }
        }

        internal static Project OpenLongFileNameProject(VisualStudioApp app, int spaceRemaining = 30) {
            string testDir = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString());
            int targetPathLength = 260 - spaceRemaining - "\\LongFileNames\\".Length;
            testDir = testDir + new string('X', targetPathLength - testDir.Length);
            Console.WriteLine("Creating long file name project ({0}) at: {1}", testDir.Length, testDir);

            Directory.CreateDirectory(testDir);
            File.Copy(@"TestData\NodejsProjectData\LongFileNames.sln", Path.Combine(testDir, "LongFileNames.sln"));
            File.Copy(@"TestData\NodejsProjectData\LFN.njsproj", Path.Combine(testDir, "LFN.njsproj"));

            CopyDirectory(@"TestData\NodejsProjectData\LongFileNames", Path.Combine(testDir, "LongFileNames"));

            return app.OpenProject(Path.Combine(testDir, "LongFileNames.sln"));
        }

        private static void CopyDirectory(string source, string dest) {
            Directory.CreateDirectory(dest);

            foreach (var file in Directory.GetFiles(source)) {
                var target = Path.Combine(dest, Path.GetFileName(file));
                Console.WriteLine("Copying {0} to {1}", file, target);
                File.Copy(file, target);
            }

            foreach (var dir in Directory.GetDirectories(source)) {
                Console.WriteLine("Copying dir {0} to {1}", dir, Path.Combine(dest, dir));
                CopyDirectory(dir, Path.Combine(dest, dir));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void AddNewFolderNested() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var projectNode = window.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld");
                AutomationWrapper.Select(projectNode);

                Keyboard.PressAndRelease(Key.F10, Key.LeftCtrl, Key.LeftShift);
                Keyboard.PressAndRelease(Key.D);
                Keyboard.PressAndRelease(Key.Right);
                Keyboard.PressAndRelease(Key.D);
                Keyboard.Type("FolderX");
                Keyboard.PressAndRelease(Key.Enter);

                var folderNode = window.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld", "FolderX");

                Assert.AreNotEqual(null, folderNode, "failed to find folder X");

                AutomationWrapper.Select(folderNode);

                Keyboard.PressAndRelease(Key.F10, Key.LeftCtrl, Key.LeftShift);
                Keyboard.PressAndRelease(Key.D);
                Keyboard.PressAndRelease(Key.Right);
                Keyboard.PressAndRelease(Key.D);
                Keyboard.Type("FolderY");
                Keyboard.PressAndRelease(Key.Enter);

                var innerFolderNode = window.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld", "FolderX", "FolderY");

                Assert.AreNotEqual(null, innerFolderNode, "failed to find folder Y");

                AutomationWrapper.Select(innerFolderNode);

                var newItem = project.ProjectItems.Item("FolderX").Collection.Item("FolderY").Collection.AddFromFile(
                    TestData.GetPath(@"TestData\DebuggerProject\BreakpointBreakOn.js")
                );

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld", "FolderX", "FolderY", "BreakpointBreakOn.js"), "failed to find added file");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RenameProjectToExisting() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\RenameProjectTestUI.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var projectNode = window.WaitForItem("Solution 'RenameProjectTestUI' (1 project)", "HelloWorld");

                // rename once, cancel renaming to existing file....
                AutomationWrapper.Select(projectNode);
                Keyboard.PressAndRelease(Key.F2);
                System.Threading.Thread.Sleep(100);

                Keyboard.Type("HelloWorldExisting");
                Keyboard.PressAndRelease(Key.Enter);

                IntPtr dialog = app.WaitForDialog();

                VisualStudioApp.CheckMessageBox("HelloWorldExisting.njsproj", "overwrite");

                // rename again, don't cancel...
                AutomationWrapper.Select(projectNode);
                Keyboard.PressAndRelease(Key.F2);
                System.Threading.Thread.Sleep(100);

                Keyboard.Type("HelloWorldExisting");
                Keyboard.PressAndRelease(Key.Enter);

                dialog = app.WaitForDialog();

                VisualStudioApp.CheckMessageBox(MessageBoxButton.Yes, "HelloWorldExisting.njsproj", "overwrite");

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'RenameProjectTestUI' (1 project)", "HelloWorldExisting"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RenameItemsTest() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\RenameItemsTestUI.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                // find server.js, send copy & paste, verify copy of file is there
                var projectNode = window.WaitForItem("Solution 'RenameItemsTestUI' (1 project)", "HelloWorld", "server.js");

                // rename once, cancel renaming to existing file....
                AutomationWrapper.Select(projectNode);
                Keyboard.PressAndRelease(Key.F2);
                System.Threading.Thread.Sleep(100);

                Keyboard.Type("NewName.txt");
                Keyboard.Type(Key.Delete);  // delete extension left at end
                Keyboard.Type(Key.Delete);
                Keyboard.Type(Key.Delete);
                System.Threading.Thread.Sleep(100);
                Keyboard.PressAndRelease(Key.Enter);

                IntPtr dialog = app.WaitForDialog();

                VisualStudioApp.CheckMessageBox(MessageBoxButton.Cancel, "file name extension");

                // rename again, don't cancel...
                AutomationWrapper.Select(projectNode);
                Keyboard.PressAndRelease(Key.F2);
                System.Threading.Thread.Sleep(100);

                Keyboard.Type("NewName.txt");
                Keyboard.Type(Key.Delete);  // delete extension left at end
                Keyboard.Type(Key.Delete);
                Keyboard.Type(Key.Delete);
                System.Threading.Thread.Sleep(100);
                Keyboard.PressAndRelease(Key.Enter);

                dialog = app.WaitForDialog();

                VisualStudioApp.CheckMessageBox(MessageBoxButton.Yes, "file name extension");

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'RenameItemsTestUI' (1 project)", "HelloWorld", "NewName.txt"));

                var subJs = window.WaitForItem("Solution 'RenameItemsTestUI' (1 project)", "HelloWorld", "Sub1", "Sub2", "Foo.js");
                Assert.IsNotNull(subJs);

                var sub1 = window.FindItem("Solution 'RenameItemsTestUI' (1 project)", "HelloWorld", "Sub1");
                AutomationWrapper.Select(sub1);
                Keyboard.PressAndRelease(Key.F2);
                System.Threading.Thread.Sleep(100);

                Keyboard.Type("FolderName");
                Keyboard.PressAndRelease(Key.Enter);

                for (int i = 0; i < 20; i++) {
                    try {
                        if (project.GetIsFolderExpanded("FolderName")) {
                            break;
                        }
                    } catch (ArgumentException) {
                    }
                    System.Threading.Thread.Sleep(100);
                }

                Assert.IsTrue(project.GetIsFolderExpanded("FolderName"));
                Assert.AreNotEqual(null, window.WaitForItem("Solution 'RenameItemsTestUI' (1 project)", "HelloWorld", "FolderName", "Sub2", "Foo.js"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void CrossProjectCopy() {
            using (var app = new VisualStudioApp()) {
                app.OpenProject(@"TestData\NodejsProjectData\HelloWorld2.sln", expectedProjects: 2);

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                var folderNode = window.WaitForItem("Solution 'HelloWorld2' (2 projects)", "HelloWorld2", "TestFolder3");
                AutomationWrapper.Select(folderNode);

                Keyboard.ControlC();

                var projectNode = window.FindItem("Solution 'HelloWorld2' (2 projects)", "HelloWorld");

                AutomationWrapper.Select(projectNode);
                Keyboard.ControlV();

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'HelloWorld2' (2 projects)", "HelloWorld", "TestFolder3"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void CrossProjectCutPaste() {
            using (var app = new VisualStudioApp()) {
                app.OpenProject(@"TestData\NodejsProjectData\HelloWorld2.sln", expectedProjects: 2);

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                var folderNode = window.WaitForItem("Solution 'HelloWorld2' (2 projects)", "HelloWorld2", "TestFolder2");
                AutomationWrapper.Select(folderNode);

                Keyboard.ControlX();

                var projectNode = window.FindItem("Solution 'HelloWorld2' (2 projects)", "HelloWorld");

                AutomationWrapper.Select(projectNode);
                Keyboard.ControlV();

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'HelloWorld2' (2 projects)", "HelloWorld", "TestFolder2"));
                Assert.AreEqual(null, window.WaitForItemRemoved("Solution 'HelloWorld2' (2 projects)", "HelloWorld2", "TestFolder2"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void CutPaste() {
            using (var app = new VisualStudioApp()) {
                app.OpenProject(@"TestData\NodejsProjectData\HelloWorld2.sln", expectedProjects: 2);

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                var subItem = window.WaitForItem("Solution 'HelloWorld2' (2 projects)", "HelloWorld2", "TestFolder", "SubItem.js");
                AutomationWrapper.Select(subItem);

                Keyboard.ControlX();

                var projectNode = window.FindItem("Solution 'HelloWorld2' (2 projects)", "HelloWorld2");

                AutomationWrapper.Select(projectNode);
                Keyboard.ControlV();

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'HelloWorld2' (2 projects)", "HelloWorld2", "SubItem.js"));
                Assert.AreEqual(null, window.WaitForItemRemoved("Solution 'HelloWorld2' (2 projects)", "HelloWorld2", "TestFolder", "SubItem.js"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void CopyFolderOnToSelf() {
            using (var app = new VisualStudioApp()) {
                app.OpenProject(@"TestData\NodejsProjectData\HelloWorld2.sln", expectedProjects: 2);

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                var folder = window.WaitForItem("Solution 'HelloWorld2' (2 projects)", "HelloWorld2", "TestFolder");
                AutomationWrapper.Select(folder);

                Keyboard.ControlC();
                AutomationWrapper.Select(folder);
                Keyboard.ControlV();

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'HelloWorld2' (2 projects)", "HelloWorld2", "TestFolder - Copy"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void DragDropTest() {
            using (var app = new VisualStudioApp()) {
                app.OpenProject(@"TestData\NodejsProjectData\DragDropTest.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                var folder = window.WaitForItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "TestFolder", "SubItem.js");
                var point = folder.GetClickablePoint();
                Mouse.MoveTo(point);
                Mouse.Down(MouseButton.Left);

                var project = window.FindItem("Solution 'DragDropTest' (1 project)", "DragDropTest");
                point = project.GetClickablePoint();
                Mouse.MoveTo(point);
                Mouse.Up(MouseButton.Left);

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "SubItem.js"));
            }
        }

        /// <summary>
        /// Drag a file onto another file in the same directory, nothing should happen
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void DragDropFileToFileTest() {
            using (var app = new VisualStudioApp()) {
                app.OpenProject(@"TestData\NodejsProjectData\DragDropTest.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                var folder = window.WaitForItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "TestFolder", "SubItem2.js");
                var point = folder.GetClickablePoint();
                Mouse.MoveTo(point);
                Mouse.Down(MouseButton.Left);

                var project = window.FindItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "TestFolder", "SubItem3.js");
                point = project.GetClickablePoint();
                Mouse.MoveTo(point);
                Mouse.Up(MouseButton.Left);

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "TestFolder", "SubItem2.js"));
                Assert.AreNotEqual(null, window.WaitForItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "TestFolder", "SubItem3.js"));
            }
        }

        /// <summary>
        /// Drag a file onto it's containing folder, nothing should happen
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void DragDropFileToContainingFolderTest() {
            using (var app = new VisualStudioApp()) {
                app.OpenProject(@"TestData\NodejsProjectData\DragDropTest.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                var folder = window.WaitForItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "TestFolder", "SubItem2.js");
                var point = folder.GetClickablePoint();
                Mouse.MoveTo(point);
                Mouse.Down(MouseButton.Left);

                var project = window.FindItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "TestFolder");
                point = project.GetClickablePoint();
                Mouse.MoveTo(point);
                Mouse.Up(MouseButton.Left);

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "TestFolder", "SubItem2.js"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void DragLeaveTest() {
            using (var app = new VisualStudioApp()) {
                app.OpenProject(@"TestData\NodejsProjectData\DragDropTest.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                var item = window.WaitForItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "TestFolder2", "SubItem.js");
                var project = window.WaitForItem("Solution 'DragDropTest' (1 project)", "DragDropTest");

                // click on SubItem.js
                var point = item.GetClickablePoint();
                Mouse.MoveTo(point);
                Mouse.Down(MouseButton.Left);

                // move to project and hover
                var projectPoint = project.GetClickablePoint();
                Mouse.MoveTo(projectPoint);

                // move back and release
                Mouse.MoveTo(point);
                Mouse.Up(MouseButton.Left);

                Assert.AreNotEqual(null, window.FindItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "TestFolder2", "SubItem.js"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void DragLeaveFolderTest() {
            using (var app = new VisualStudioApp()) {
                app.OpenProject(@"TestData\NodejsProjectData\DragDropTest.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                var folder = window.WaitForItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "TestFolder2", "SubFolder");
                var project = window.WaitForItem("Solution 'DragDropTest' (1 project)", "DragDropTest");

                // click on SubItem.js
                var point = folder.GetClickablePoint();
                Mouse.MoveTo(point);
                Mouse.Down(MouseButton.Left);

                // move to project and hover
                var projectPoint = project.GetClickablePoint();
                Mouse.MoveTo(projectPoint);

                // move back and release
                Mouse.MoveTo(point);
                Mouse.Up(MouseButton.Left);

                Assert.AreNotEqual(null, window.FindItem("Solution 'DragDropTest' (1 project)", "DragDropTest", "TestFolder2", "SubFolder"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void MultiSelectCopyAndPaste() {
            using (var app = new VisualStudioApp()) {
                app.OpenProject(@"TestData\NodejsProjectData\MultiSelectCopyAndPaste.sln");

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;

                var folderNode = window.WaitForItem("Solution 'MultiSelectCopyAndPaste' (1 project)", "MultiSelectCopyAndPaste", "server.js");
                Mouse.MoveTo(folderNode.GetClickablePoint());
                Mouse.Click();

                Keyboard.Press(Key.LeftShift);
                Keyboard.PressAndRelease(Key.Down);
                Keyboard.PressAndRelease(Key.Down);
                Keyboard.Release(Key.LeftShift);
                Keyboard.ControlC();

                var projectNode = window.WaitForItem("Solution 'MultiSelectCopyAndPaste' (1 project)", "MultiSelectCopyAndPaste");

                AutomationWrapper.Select(projectNode);
                Keyboard.ControlV();

                Assert.AreNotEqual(null, window.WaitForItem("Solution 'MultiSelectCopyAndPaste' (1 project)", "MultiSelectCopyAndPaste", "server - Copy.js"));
                Assert.AreNotEqual(null, window.WaitForItem("Solution 'MultiSelectCopyAndPaste' (1 project)", "MultiSelectCopyAndPaste", "server2 - Copy.js"));
                Assert.AreNotEqual(null, window.WaitForItem("Solution 'MultiSelectCopyAndPaste' (1 project)", "MultiSelectCopyAndPaste", "server3 - Copy.js"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TransferItem() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\HelloWorld.sln");

                string filename, basename;
                int i = 0;
                do {
                    i++;
                    basename = "test" + i + " .js";
                    filename = Path.Combine(Path.GetTempPath(), basename);
                } while (System.IO.File.Exists(filename));

                System.IO.File.WriteAllText(filename, "function f() { }");

                var fileWindow = app.Dte.ItemOperations.OpenFile(filename);

                using (var dlg = ChooseLocationDialog.FromDte(app)) {
                    dlg.SelectProject("HelloWorld");
                    dlg.OK();
                }

                app.OpenSolutionExplorer();
                var window = app.SolutionExplorerTreeView;
                Assert.AreNotEqual(null, window.WaitForItem("Solution 'HelloWorld' (1 project)", "HelloWorld", basename));

                Assert.AreEqual(fileWindow.Caption, basename);

                System.IO.File.Delete(filename);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void SaveAs() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodejsProjectData\SaveAsUI.sln");

                app.OpenSolutionExplorer();
                var solutionTree = app.SolutionExplorerTreeView;

                // open and edit the file
                var folderNode = solutionTree.WaitForItem("Solution 'SaveAsUI' (1 project)", "HelloWorld", "server.js");
                folderNode.SetFocus();
                Keyboard.PressAndRelease(Key.Enter);

                var item = project.ProjectItems.Item("server.js");
                var window = item.Open();
                window.Activate();

                var selection = ((TextSelection)window.Selection);
                selection.SelectAll();
                selection.Delete();

                // save under a new file name
                var saveDialog = app.SaveAs();
                string oldName = saveDialog.FileName;
                saveDialog.FileName = "Program2.js";
                saveDialog.Save();

                Assert.AreNotEqual(null, solutionTree.WaitForItem("Solution 'SaveAsUI' (1 project)", "HelloWorld", "Program2.js"));
            }
        }
    }
}
