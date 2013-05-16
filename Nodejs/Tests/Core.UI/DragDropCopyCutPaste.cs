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
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.UI;
using Keyboard = TestUtilities.UI.Keyboard;
using Mouse = TestUtilities.UI.Mouse;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class DragDropCopyCutPaste {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            TestData.Deploy();
        }

        [TestCleanup]
        public void MyTestCleanup() {
            for (int i = 0; i < 20; i++) {
                try {
                    VsIdeTestHostContext.Dte.Solution.Close(false);
                    break;
                } catch {
                    VsIdeTestHostContext.Dte.Documents.CloseAll(EnvDTE.vsSaveChanges.vsSaveChangesNo);
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// Cut item, paste into folder, paste into top-level, 2nd paste shouldn’t do anything
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MultiPaste() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\MultiPaste.sln");

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var server = window.WaitForItem("Solution 'MultiPaste' (1 project)", "HelloWorld", "server.js");
            var server2 = window.WaitForItem("Solution 'MultiPaste' (1 project)", "HelloWorld", "server2.js");
            
            var point = server.GetClickablePoint();
            Mouse.MoveTo(point);
            Mouse.Click(MouseButton.Left);

            Keyboard.Press(Key.LeftShift);
            try {
                point = server2.GetClickablePoint();
                Mouse.MoveTo(point);
                Mouse.Click(MouseButton.Left);
            } finally {
                Keyboard.Release(Key.LeftShift);
            }
            
            Keyboard.ControlC();

            // https://pytools.codeplex.com/workitem/1144
            var folder = window.WaitForItem("Solution 'MultiPaste' (1 project)", "HelloWorld", "SubFolder");
            AutomationWrapper.Select(folder);
            Keyboard.ControlV();

            // paste once, multiple items should be pasted
            Assert.IsNotNull(window.WaitForItem("Solution 'MultiPaste' (1 project)", "HelloWorld", "SubFolder", "server.js"));
            Assert.IsNotNull(window.WaitForItem("Solution 'MultiPaste' (1 project)", "HelloWorld", "SubFolder", "server2.js"));

            AutomationWrapper.Select(folder);
            Keyboard.ControlV();

            // paste again, we should get the replace prompts...

            var dialog = new OverwriteFileDialog(app.WaitForDialog());
            dialog.Cancel();

            // https://pytools.codeplex.com/workitem/1154
            // and we shouldn't get a second dialog after cancelling...
            app.WaitForDialogDismissed();
        }

        /// <summary>
        /// Cut item, paste into folder, paste into top-level, 2nd paste shouldn’t do anything
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CutPastePasteItem() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var project = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste");
            var folder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "PasteFolder");
            var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutPastePasteItem.js");
            AutomationWrapper.Select(file);

            Keyboard.ControlX();

            AutomationWrapper.Select(folder);
            Keyboard.ControlV();
            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "PasteFolder", "CutPastePasteItem.js");

            AutomationWrapper.Select(project);
            Keyboard.ControlV();

            System.Threading.Thread.Sleep(1000);

            AssertFileDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutPastePasteItem.js");
        }

        /// <summary>
        /// Cut item, rename it, paste into top-level, check error message
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CutRenamePaste() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var project = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste");
            var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutRenamePaste", "CutRenamePaste.js");
            
            AutomationWrapper.Select(file);
            Keyboard.ControlX();

            AutomationWrapper.Select(file);
            Keyboard.Type(Key.F2);
            Keyboard.Type("CutRenamePasteNewName");
            Keyboard.Type(Key.Enter);

            System.Threading.Thread.Sleep(1000);
            AutomationWrapper.Select(project);
            Keyboard.ControlV();

            VisualStudioApp.CheckMessageBox("The source URL 'CutRenamePaste.js' could not be found.");
        }

        /// <summary>
        /// Cut item, rename it, paste into top-level, check error message
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CutDeletePaste() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var project = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste");
            var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutDeletePaste", "CutDeletePaste.js");

            AutomationWrapper.Select(file);
            Keyboard.ControlX();

            File.Delete(@"TestData\NodejsProjectData\DragDropCopyCutPaste\CutDeletePaste\CutDeletePaste.js");

            AutomationWrapper.Select(project);
            Keyboard.ControlV();

            VisualStudioApp.CheckMessageBox("The item 'CutDeletePaste.js' does not exist in the project directory. It may have been moved, renamed or deleted.");

            Assert.IsNotNull(window.FindItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutDeletePaste", "CutDeletePaste.js"));
        }

        /// <summary>
        /// Adds a new folder which fits exactly w/ no space left in the path name
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CopyFileToFolderTooLong() {
            var project = UITests.OpenLongFileNameProject(24);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
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

            var folderNode = window.WaitForItem("Solution 'LongFileNames' (1 project)", "LFN", "01234567891");
            Assert.IsNotNull(folderNode);

            var serverNode = window.WaitForItem("Solution 'LongFileNames' (1 project)", "LFN", "server.js");
            AutomationWrapper.Select(serverNode);
            Keyboard.ControlC();
            Keyboard.ControlV();

            var serverCopy = window.WaitForItem("Solution 'LongFileNames' (1 project)", "LFN", "server - Copy.js");
            Assert.IsNotNull(serverCopy);

            AutomationWrapper.Select(serverCopy);
            Keyboard.ControlC();

            AutomationWrapper.Select(folderNode);
            Keyboard.ControlV();

            VisualStudioApp.CheckMessageBox("The filename is too long.");
        }

        /// <summary>
        /// Adds a new folder which fits exactly w/ no space left in the path name
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CutFileToFolderTooLong() {
            var project = UITests.OpenLongFileNameProject(24);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
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

            var folderNode = window.WaitForItem("Solution 'LongFileNames' (1 project)", "LFN", "01234567891");
            Assert.IsNotNull(folderNode);

            var serverNode = window.FindItem("Solution 'LongFileNames' (1 project)", "LFN", "server.js");
            AutomationWrapper.Select(serverNode);
            Keyboard.ControlC();
            Keyboard.ControlV();

            var serverCopy = window.WaitForItem("Solution 'LongFileNames' (1 project)", "LFN", "server - Copy.js");
            Assert.IsNotNull(serverCopy);

            AutomationWrapper.Select(serverCopy);
            Keyboard.ControlX();

            AutomationWrapper.Select(folderNode);
            Keyboard.ControlV();

            VisualStudioApp.CheckMessageBox("The filename is too long.");
        }

        /// <summary>
        /// Cut folder, rename it, paste into top-level, check error message
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CutRenamePasteFolder() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var project = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste");
            var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutRenamePaste", "CutRenamePasteFolder");
            AutomationWrapper.Select(file);
            Keyboard.ControlX();

            Keyboard.Type(Key.F2);
            Keyboard.Type("CutRenamePasteFolderNewName");
            Keyboard.Type(Key.Enter);
            System.Threading.Thread.Sleep(1000);

            AutomationWrapper.Select(project);
            Keyboard.ControlV();

            VisualStudioApp.CheckMessageBox("The source URL 'CutRenamePasteFolder' could not be found.");
        }

        /// <summary>
        /// Copy a file node, drag and drop a different file, paste the node, should succeed
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CopiedBeforeDragPastedAfterDrop() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var project = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste");
            Assert.AreNotEqual(null, project);
            var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CopiedBeforeDragPastedAfterDrop.js");
            Assert.AreNotEqual(null, file);
            var draggedFile = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DragAndDroppedDuringCopy.js");
            Assert.AreNotEqual(null, draggedFile);
            var dragFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DragDuringCopyDestination");
            Assert.AreNotEqual(null, dragFolder);

            AutomationWrapper.Select(file);
            Keyboard.ControlC();

            AutomationWrapper.Select(draggedFile);
            
            Mouse.MoveTo(draggedFile.GetClickablePoint());
            Mouse.Down(MouseButton.Left);
            Mouse.MoveTo(dragFolder.GetClickablePoint());
            Mouse.Up(MouseButton.Left);

            var folder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "PasteFolder");
            AutomationWrapper.Select(folder);
            Keyboard.ControlV();
            
            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "PasteFolder", "CopiedBeforeDragPastedAfterDrop.js");
            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CopiedBeforeDragPastedAfterDrop.js");
        }

        /// <summary>
        /// Copy a file node from Node project, drag and drop node from other project, should get copy, not move
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void DragToAnotherProject() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var draggedFile = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "!Source", "DraggedToOtherProject.js");
            var destProject = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1");
            AutomationWrapper.Select(draggedFile);

            var point = draggedFile.GetClickablePoint();
            Mouse.MoveTo(point);
            Mouse.Down(MouseButton.Left);

            Mouse.MoveTo(destProject.GetClickablePoint());
            Mouse.Up(MouseButton.Left);

            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1", "DraggedToOtherProject.js");
            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "!Source", "DraggedToOtherProject.js");
        }

        /// <summary>
        /// Cut folder, paste onto itself, should report an error that the destination is the same as the source
        ///     Cannot move 'X'. The destination folder is the same as the source folder.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CutFolderPasteOnSelf() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var cutFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutFolderPasteOnSelf");
            AutomationWrapper.Select(cutFolder);

            Keyboard.ControlX();
            Keyboard.ControlV();
            VisualStudioApp.CheckMessageBox("Cannot move 'CutFolderPasteOnSelf'. The destination folder is the same as the source folder.");

            AssertFolderExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutFolderPasteOnSelf");
            AssertFolderDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutFolderPasteOnSelf - Copy");
        }

        /// <summary>
        /// Drag and drop a folder onto itself, nothing should happen
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void DragFolderOntoSelf() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var draggedFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DragFolderOntoSelf");
            AutomationWrapper.Select(draggedFolder);

            var point = draggedFolder.GetClickablePoint();
            Mouse.MoveTo(point);
            Mouse.Down(MouseButton.Left);
            Mouse.MoveTo(new Point(point.X + 1, point.Y + 1));

            Mouse.Up(MouseButton.Left);
                        
            AssertFolderExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DragFolderOntoSelf");
            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DragFolderOntoSelf", "File.js");
            AssertFolderDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DragFolderOntoSelf - Copy");
            AssertFileDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DragFolderOntoSelf", "File - Copy.js");
        }

        /// <summary>
        /// Drag and drop a folder onto itself, nothing should happen
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void DragFolderOntoChild() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var draggedFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "ParentFolder");
            var childFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "ParentFolder", "ChildFolder");
            AutomationWrapper.Select(draggedFolder);

            var point = draggedFolder.GetClickablePoint();
            Mouse.MoveTo(point);
            Mouse.Down(MouseButton.Left);
            Mouse.MoveTo(childFolder.GetClickablePoint());

            Mouse.Up(MouseButton.Left);

            VisualStudioApp.CheckMessageBox("Cannot move 'ParentFolder'. The destination folder is a subfolder of the source folder.");
            app.WaitForDialogDismissed();

            draggedFolder = window.FindItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "ParentFolder");
            Assert.IsNotNull(draggedFolder);
            childFolder = window.FindItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "ParentFolder", "ChildFolder");
            Assert.IsNotNull(childFolder);
            var parentInChildFolder = window.FindItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "ParentFolder", "ChildFolder", "ParentFolder");
            Assert.IsNull(parentInChildFolder);
        }

        /// <summary>
        /// Move a file to a location where a file with the name now already exists.  We should get an overwrite
        /// dialog, and after answering yes to overwrite the file should be moved.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CutFileReplace() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "MoveDupFilename", "Foo", "JavaScript1.js");
            Assert.AreNotEqual(null, file);
            var dest = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "MoveDupFilename");
            Assert.AreNotEqual(null, dest);

            AutomationWrapper.Select(file);

            Keyboard.ControlX();
            AutomationWrapper.Select(dest);

            Keyboard.ControlV();

            var dialog = new OverwriteFileDialog(app.WaitForDialog());
            dialog.Yes();

            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "MoveDupFilename", "JavaScript1.js");
            AssertFileDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "MoveDupFilename", "Foo", "JavaScript1.js");
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CutFolderAndFile() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var folder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutFolderAndFile", "CutFolder");
            var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutFolderAndFile", "CutFolder", "CutFolderAndFile.js");
            var dest = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste");

            Mouse.MoveTo(folder.GetClickablePoint());
            Mouse.Click(MouseButton.Left);
            try {
                Keyboard.Press(Key.LeftShift);
                Mouse.MoveTo(file.GetClickablePoint());
                Mouse.Click(MouseButton.Left);
            } finally {
                Keyboard.Release(Key.LeftShift);
            }

            Keyboard.ControlX();
            AutomationWrapper.Select(dest);
            Keyboard.ControlV();

            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutFolder", "CutFolderAndFile.js");
            AssertFileDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutFolderAndFile", "CutFolder");
        }

        /// <summary>
        /// Drag and drop a folder onto itself, nothing should happen
        ///     Cannot move 'CutFilePasteSameLocation.js'. The destination folder is the same as the source folder.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CutFilePasteSameLocation() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var project = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste");
            var cutFile = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutFilePasteSameLocation.js");
            AutomationWrapper.Select(cutFile);

            Keyboard.ControlX();
            AutomationWrapper.Select(project);

            Keyboard.ControlV();
            VisualStudioApp.CheckMessageBox("Cannot move 'CutFilePasteSameLocation.js'. The destination folder is the same as the source folder.");

            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutFilePasteSameLocation.js");
            AssertFileDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CutFilePasteSameLocation - Copy.js");
        }

        /// <summary>
        /// Drag and drop a folder onto itself, nothing should happen
        ///     Cannot move 'DragFolderAndFileToSameFolder'. The destination folder is the same as the source folder.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void DragFolderAndFileOntoSelf() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var folder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DragFolderAndFileOntoSelf");
            var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DragFolderAndFileOntoSelf", "File.js");

            Mouse.MoveTo(folder.GetClickablePoint());
            Mouse.Click(MouseButton.Left);
            try {
                Keyboard.Press(Key.LeftShift);
                Mouse.MoveTo(file.GetClickablePoint());
                Mouse.Click(MouseButton.Left);
            } finally {
                Keyboard.Release(Key.LeftShift);
            }

            Mouse.MoveTo(file.GetClickablePoint());
            Mouse.Down(MouseButton.Left);
            Mouse.MoveTo(folder.GetClickablePoint());
            Mouse.Up(MouseButton.Left);

            VisualStudioApp.CheckMessageBox("Cannot move 'DragFolderAndFileOntoSelf'. The destination folder is the same as the source folder.");
        }

        /// <summary>
        /// Add folder from another project, folder contains items on disk which are not in the project, only items in the project should be added.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CopyFolderFromAnotherHierarchy() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var folder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1", "CopiedFolderWithItemsNotInProject");
            var project = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste");

            AutomationWrapper.Select(folder);
            Keyboard.ControlC();

            AutomationWrapper.Select(project);
            Keyboard.ControlV();

            window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CopiedFolderWithItemsNotInProject", "Class.cs");

            AssertFolderExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CopiedFolderWithItemsNotInProject");
            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CopiedFolderWithItemsNotInProject", "Class.cs");
            AssertFileDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CopiedFolderWithItemsNotInProject", "Text.txt");
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CopyDeletePaste() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CopyDeletePaste", "CopyDeletePaste.js");
            var project = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste");

            AutomationWrapper.Select(file);
            Keyboard.ControlC();

            AutomationWrapper.Select(file);
            Keyboard.Type(Key.Delete);
            app.WaitForDialog();

            Keyboard.Type("\r");

            AutomationWrapper.Select(project);
            Keyboard.ControlV();

            VisualStudioApp.CheckMessageBox("The source URL 'CopyDeletePaste.js' could not be found.");
        }

        /// <summary>
        /// Drag file from another hierarchy into folder in our hierarchy, item should be added
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CrossHierarchyFileDragAndDrop() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var folder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1", "CrossHierarchyFileDragAndDrop.cs");
            var destFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DropFolder");

            Mouse.MoveTo(folder.GetClickablePoint());
            Mouse.Down(MouseButton.Left);
            Mouse.MoveTo(destFolder.GetClickablePoint());
            Mouse.Up(MouseButton.Left);

            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DropFolder", "CrossHierarchyFileDragAndDrop.cs");
        }
        
        /// <summary>
        /// Drag file from another hierarchy into folder in our hierarchy, item should be added
        ///     Cannot move the folder 'DuplicateFolderName'. A folder with that name already exists in the destination directory.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void DuplicateFolderName() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var folder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DuplicateFolderName");
            var destFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DuplicateFolderNameTarget");

            AutomationWrapper.Select(folder);
            Keyboard.ControlX();

            AutomationWrapper.Select(destFolder);
            Keyboard.ControlV();

            VisualStudioApp.CheckMessageBox("Cannot move the folder 'DuplicateFolderName'. A folder with that name already exists in the destination directory.");

            // try again with drag and drop, which defaults to move
            Mouse.MoveTo(folder.GetClickablePoint());
            Mouse.Down(MouseButton.Left);
            Mouse.MoveTo(destFolder.GetClickablePoint());
            Mouse.Up(MouseButton.Left);

            VisualStudioApp.CheckMessageBox("Cannot move the folder 'DuplicateFolderName'. A folder with that name already exists in the destination directory.");
        }

        /// <summary>
        /// Drag file from another hierarchy into folder in our hierarchy, item should be added
        ///     Cannot move the folder 'DuplicateFolderName'. A folder with that name already exists in the destination directory.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CopyDuplicateFolderName() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var folder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CopyDuplicateFolderName");
            var destFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CopyDuplicateFolderNameTarget");

            AutomationWrapper.Select(folder);
            Keyboard.ControlC();

            AutomationWrapper.Select(destFolder);
            Keyboard.ControlV();

            var dialog = new OverwriteFileDialog(app.WaitForDialog());
            Assert.IsTrue(dialog.Text.Contains("This folder already contains a folder called 'CopyDuplicateFolderName'"), "wrong text in overwrite dialog");
            dialog.No();

            AssertFileDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CopyDuplicateFolderNameTarget", "CopyDuplicateFolderName", "JavaScript1.js");
        }

        /// <summary>
        /// Cut item from one project, paste into another project, item should be removed from original project
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CrossHierarchyCut() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1", "CrossHierarchyCut.cs");
            var destFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste");

            AutomationWrapper.Select(file);
            Keyboard.ControlX();

            AutomationWrapper.Select(destFolder);
            Keyboard.ControlV();

            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CrossHierarchyCut.cs");
            AssertFileDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1", "CrossHierarchyCut.cs");
        }

        /// <summary>
        /// Cut an item from our project, paste into another project, item should be removed from our project
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ReverseCrossHierarchyCut() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CrossHierarchyCut.js");
            var destFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1");

            AutomationWrapper.Select(file);
            Keyboard.ControlX();

            AutomationWrapper.Select(destFolder);
            Keyboard.ControlV();

            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1", "CrossHierarchyCut.js");
            AssertFileDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CrossHierarchyCut.js");
        }

        /// <summary>
        /// Drag item from our project to other project, copy
        /// Drag item from other project to our project, still copy back
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void DoubleCrossHierarchyMove() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "!Source", "DoubleCrossHierarchy.js");
            var destFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1");

            AutomationWrapper.Select(file);
            Mouse.MoveTo(file.GetClickablePoint());
            Mouse.Down(MouseButton.Left);
            Mouse.MoveTo(destFolder.GetClickablePoint());
            Mouse.Up(MouseButton.Left);

            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1", "DoubleCrossHierarchy.js");
            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "!Source", "DoubleCrossHierarchy.js");

            file = window.FindItem("Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1", "DoubleCrossHierarchy.cs");
            destFolder = window.FindItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste");

            Mouse.MoveTo(file.GetClickablePoint());
            Mouse.Down(MouseButton.Left);
            Mouse.MoveTo(destFolder.GetClickablePoint());
            Mouse.Up(MouseButton.Left);

            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DoubleCrossHierarchy.cs");
            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1", "DoubleCrossHierarchy.cs");
        }

        /// <summary>
        /// Drag item from another project, drag same item again, prompt to overwrite, say yes, only one item should be in the hierarchy
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void DragTwiceAndOverwrite() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            for (int i = 0; i < 2; i++) {
                var file = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "ConsoleApplication1", "DragTwiceAndOverwrite.cs");
                var destFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste");

                Mouse.MoveTo(file.GetClickablePoint());
                Mouse.Down(MouseButton.Left);
                Mouse.MoveTo(destFolder.GetClickablePoint());
                Mouse.Up(MouseButton.Left);
            }

            var dialog = new OverwriteFileDialog(app.WaitForDialog());
            Assert.IsTrue(dialog.Text.Contains("A file with the name 'DragTwiceAndOverwrite.cs' already exists."), "wrong text");
            dialog.Yes();

            AssertFileExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DragTwiceAndOverwrite.cs");
            AssertFileDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "DragTwiceAndOverwrite - Copy.cs");
        }

        /// <summary>
        /// Drag item from another project, drag same item again, prompt to overwrite, say yes, only one item should be in the hierarchy
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CopyFolderMissingItem() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var folder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CopyFolderMissingItem");
            var destFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "PasteFolder");

            AutomationWrapper.Select(folder);
            Keyboard.ControlC();
            AutomationWrapper.Select(destFolder);
            Keyboard.ControlV();

            // make sure no dialogs pop up
            VisualStudioApp.CheckMessageBox("The item 'JavaScript1.js' does not exist in the project directory. It may have been moved, renamed or deleted.");

            AssertFolderExists(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "CopyFolderMissingItem");
            AssertFolderDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "PasteFolder", "CopyFolderMissingItem");
            AssertFileDoesntExist(window, "Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "PasteFolder", "JavaScript1.js");
        }

        /// <summary>
        /// Copy missing file
        /// 
        /// https://pytools.codeplex.com/workitem/1141
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CopyPasteMissingFile() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var folder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "MissingFile.js");
            var destFolder = window.WaitForItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "PasteFolder");

            AutomationWrapper.Select(folder);
            Keyboard.ControlC();
            AutomationWrapper.Select(destFolder);
            Keyboard.ControlV();

            VisualStudioApp.CheckMessageBox("The item 'MissingFile.js' does not exist in the project directory. It may have been moved, renamed or deleted.");
        }

        /// <summary>
        /// Copy missing file
        /// 
        /// https://nodejstools.codeplex.com/workitem/241
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveFolderExistingFile() {
            BasicProjectTests.OpenProject(@"TestData\NodejsProjectData\DragDropCopyCutPaste.sln", expectedProjects: 2);

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenSolutionExplorer();
            var window = app.SolutionExplorerTreeView;

            var folder = window.FindItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "FolderCollision");
            var destFolder = window.FindItem("Solution 'DragDropCopyCutPaste' (2 projects)", "DragDropCopyCutPaste", "PasteFolder");

            AutomationWrapper.Select(folder);
            Keyboard.ControlX();
            AutomationWrapper.Select(destFolder);
            Keyboard.ControlV();

            VisualStudioApp.CheckMessageBox("Unable to add 'FolderCollision'. A file with that name already exists.");
        }

        private static void AssertFileExists(SolutionExplorerTree window, params string[] path) {
            Assert.IsNotNull(window.WaitForItem(path), "Item not found in solution explorer" + String.Join("\\", path));

            var basePath = Path.Combine("TestData", "NodejsProjectData");
            for (int i = 1; i < path.Length; i++) {
                basePath = Path.Combine(basePath, path[i]);
            }
            Assert.IsTrue(File.Exists(basePath), "File doesn't exist: " + basePath);
        }

        private static void AssertFileDoesntExist(SolutionExplorerTree window, params string[] path) {
            Assert.IsNull(window.FindItem(path), "Item exists in solution explorer: " + String.Join("\\", path));

            var basePath = Path.Combine("TestData", "NodejsProjectData");
            for (int i = 1; i < path.Length; i++) {
                basePath = Path.Combine(basePath, path[i]);
            }
            Assert.IsFalse(File.Exists(basePath), "File exists: " + basePath);
        }

        private static void AssertFolderExists(SolutionExplorerTree window, params string[] path) {
            Assert.IsNotNull(window.WaitForItem(path), "Item not found in solution explorer" + String.Join("\\", path));

            var basePath = Path.Combine("TestData", "NodejsProjectData");
            for (int i = 1; i < path.Length; i++) {
                basePath = Path.Combine(basePath, path[i]);
            }
            Assert.IsTrue(Directory.Exists(basePath), "File doesn't exist: " + basePath);
        }

        private static void AssertFolderDoesntExist(SolutionExplorerTree window, params string[] path) {
            Assert.IsNull(window.FindItem(path), "Item exists in solution explorer: " + String.Join("\\", path));

            var basePath = Path.Combine("TestData", "NodejsProjectData");
            for (int i = 1; i < path.Length; i++) {
                basePath = Path.Combine(basePath, path[i]);
            }
            Assert.IsFalse(Directory.Exists(basePath), "File exists: " + basePath);
        }
    }
}
