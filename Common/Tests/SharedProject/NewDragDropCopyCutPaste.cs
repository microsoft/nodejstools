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

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Automation;
using System.Windows.Input;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.SharedProject;
using TestUtilities.UI;
using Keyboard = TestUtilities.UI.Keyboard;
using Mouse = TestUtilities.UI.Mouse;

namespace Microsoft.VisualStudioTools.SharedProjectTests {
    [TestClass]
    public class NewDragDropCopyCutPaste : SharedProjectTest {
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveExcludedFolderKeyboard() {
            MoveExcludedFolder(MoveByKeyboard);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveExcludedFolderMouse() {
            MoveExcludedFolder(MoveByMouse);
        }

        private void MoveExcludedFolder(MoveDelegate mover) {
            foreach (var projectType in ProjectTypes) {
                var testDef = new ProjectDefinition("MoveExcludedFolder", 
                    projectType, 
                    PropertyGroup(
                        Property("ProjectView", "ShowAllFiles")
                    ), 
                    ItemGroup(
                        Folder("Foo", isExcluded: true),
                        Folder("Foo\\Bar", isExcluded: true),
                        Folder("Baz", isExcluded: true)
                    )
                );

                using (var solution = testDef.Generate()) {
                    var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
                    app.OpenProject(solution.Filename);

                    var window = app.OpenSolutionExplorer();

                    SelectSolutionNode(window, "Solution 'MoveExcludedFolder' (1 project)");

                    mover(
                        window.FindItem("Solution 'MoveExcludedFolder' (1 project)", "MoveExcludedFolder", "Baz"),
                        window.FindItem("Solution 'MoveExcludedFolder' (1 project)", "MoveExcludedFolder", "Foo")
                    );

                    window.AssertFolderDoesntExist(Path.GetDirectoryName(solution.Filename), "Solution 'MoveExcludedFolder' (1 project)", "MoveExcludedFolder", "Foo");
                    window.AssertFolderExists(Path.GetDirectoryName(solution.Filename), "Solution 'MoveExcludedFolder' (1 project)", "MoveExcludedFolder", "Baz", "Foo");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveExcludedItemToFolderKeyboard() {
            MoveExcludedItemToFolder(MoveByKeyboard);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveExcludedItemToFolderMouse() {
            MoveExcludedItemToFolder(MoveByMouse);
        }

        private void MoveExcludedItemToFolder(MoveDelegate mover) {
            foreach (var projectType in ProjectTypes) {
                var testDef = new ProjectDefinition("MoveExcludedItemToFolder", 
                    projectType, 
                    PropertyGroup(
                        Property("ProjectView", "ShowAllFiles")
                    ), 
                    ItemGroup(
                        Folder("Folder"),
                        Compile("codefile", isExcluded: true)
                    )
                );

                using (var solution = testDef.Generate()) {
                    var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
                    app.OpenProject(solution.Filename);

                    var window = app.OpenSolutionExplorer();

                    SelectSolutionNode(window, "Solution 'MoveExcludedItemToFolder' (1 project)");

                    mover(
                        window.FindItem("Solution 'MoveExcludedItemToFolder' (1 project)", "MoveExcludedItemToFolder", "Folder"),
                        window.FindItem("Solution 'MoveExcludedItemToFolder' (1 project)", "MoveExcludedItemToFolder", "codefile" + projectType.CodeExtension)
                    );

                    window.AssertFileDoesntExist(Path.GetDirectoryName(solution.Filename), "Solution 'MoveExcludedItemToFolder' (1 project)", "MoveExcludedItemToFolder", "codefile" + projectType.CodeExtension);
                    window.AssertFileExists(Path.GetDirectoryName(solution.Filename), "Solution 'MoveExcludedItemToFolder' (1 project)", "MoveExcludedItemToFolder", "Folder", "codefile" + projectType.CodeExtension);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveDuplicateFileNameSkipMoveKeyboard() {
            MoveDuplicateFileNameSkipMove(MoveByKeyboard);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveDuplicateFileNameSkipMoveMouse() {
            MoveDuplicateFileNameSkipMove(MoveByMouse);
        }

        /// <summary>
        /// Move item within the project from one location to where it already exists, skipping the move.
        /// </summary>
        private void MoveDuplicateFileNameSkipMove(MoveDelegate mover) {
            foreach (var projectType in ProjectTypes) {
                var testDef = new ProjectDefinition("MoveDuplicateFileName",
                    projectType,
                    ItemGroup(
                        Folder("Folder"),
                        Content("textfile.txt", "root"),
                        Content("Folder\\textfile.txt", "Folder")
                    )
                );

                using (var solution = testDef.Generate()) {
                    var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
                    app.OpenProject(solution.Filename);

                    var window = app.OpenSolutionExplorer();

                    SelectSolutionNode(window, "Solution 'MoveDuplicateFileName' (1 project)");

                    mover(
                        window.FindItem("Solution 'MoveDuplicateFileName' (1 project)", "MoveDuplicateFileName", "Folder"),
                        window.FindItem("Solution 'MoveDuplicateFileName' (1 project)", "MoveDuplicateFileName", "textfile.txt")
                    );

                    var dialog = new OverwriteFileDialog(app.WaitForDialog());
                    dialog.No();

                    app.WaitForDialogDismissed();

                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "root", "Solution 'MoveDuplicateFileName' (1 project)", "MoveDuplicateFileName", "textfile.txt");
                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "Folder", "Solution 'MoveDuplicateFileName' (1 project)", "MoveDuplicateFileName", "Folder", "textfile.txt");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveDuplicateFileNamesSkipOneKeyboard() {
            MoveDuplicateFileNamesSkipOne(MoveByKeyboard);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveDuplicateFileNamesSkipOneMouse() {
            MoveDuplicateFileNamesSkipOne(MoveByMouse);
        }

        /// <summary>
        /// Cut 2 items, paste where they exist, skip pasting the 1st one but paste the 2nd.
        /// 
        /// The 1st item shouldn't be removed from the parent hierarchy, the 2nd should, and only the 2nd item should be overwritten.
        /// </summary>
        private void MoveDuplicateFileNamesSkipOne(MoveDelegate mover) {
            foreach (var projectType in ProjectTypes) {
                var testDef = new ProjectDefinition("MoveDuplicateFileName",
                    projectType,
                    ItemGroup(
                        Folder("Folder"),
                        Content("textfile1.txt", "root1"),
                        Content("textfile2.txt", "root2"),
                        Content("Folder\\textfile1.txt", "Folder1"),
                        Content("Folder\\textfile2.txt", "Folder2")
                    )
                );

                using (var solution = testDef.Generate()) {
                    var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
                    app.OpenProject(solution.Filename);

                    var window = app.OpenSolutionExplorer();

                    SelectSolutionNode(window, "Solution 'MoveDuplicateFileName' (1 project)");
                    mover(
                        window.FindItem("Solution 'MoveDuplicateFileName' (1 project)", "MoveDuplicateFileName", "Folder"),
                        window.FindItem("Solution 'MoveDuplicateFileName' (1 project)", "MoveDuplicateFileName", "textfile1.txt"),
                        window.FindItem("Solution 'MoveDuplicateFileName' (1 project)", "MoveDuplicateFileName", "textfile2.txt")
                    );

                    var dialog = new OverwriteFileDialog(app.WaitForDialog());
                    dialog.No();

                    System.Threading.Thread.Sleep(1000);

                    dialog = new OverwriteFileDialog(app.WaitForDialog());
                    dialog.Yes();

                    app.WaitForDialogDismissed();

                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "root1", "Solution 'MoveDuplicateFileName' (1 project)", "MoveDuplicateFileName", "textfile1.txt");
                    window.AssertFileDoesntExist(Path.GetDirectoryName(solution.Filename), "Solution 'MoveDuplicateFileName' (1 project)", "MoveDuplicateFileName", "textfile2.txt");
                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "Folder1", "Solution 'MoveDuplicateFileName' (1 project)", "MoveDuplicateFileName", "Folder", "textfile1.txt");
                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "root2", "Solution 'MoveDuplicateFileName' (1 project)", "MoveDuplicateFileName", "Folder", "textfile2.txt");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveDuplicateFileNamesCrossProjectSkipOneKeyboard() {
            MoveDuplicateFileNamesCrossProjectSkipOne(MoveByKeyboard);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveDuplicateFileNamesCrossProjectSkipOneMouse() {
            MoveDuplicateFileNamesCrossProjectSkipOne(MoveByMouse);
        }

        /// <summary>
        /// Cut 2 items, paste where they exist, skip pasting the 1st one but paste the 2nd.
        /// 
        /// The 1st item shouldn't be removed from the parent hierarchy, the 2nd should, and only the 2nd item should be overwritten.
        /// </summary>
        private void MoveDuplicateFileNamesCrossProjectSkipOne(MoveDelegate mover) {
            foreach (var projectType in ProjectTypes) {
                var projectDefs = new[] {
                    new ProjectDefinition("MoveDuplicateFileName",
                        projectType,
                        ItemGroup(
                            Content("textfile1.txt", "textfile1 - lang"),
                            Content("textfile2.txt", "textfile2 - lang")
                        )
                    ),
                    new ProjectDefinition("MoveDuplicateFileName2",
                        projectType,
                        ItemGroup(
                            Folder("Folder"),
                            Content("textfile1.txt", "textfile1 - 2"),
                            Content("textfile2.txt", "textfile2 - 2")
                        )
                    )
                };

                using (var solution = SolutionFile.Generate("MoveDuplicateFileName", projectDefs)) {
                    var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
                    app.OpenProject(solution.Filename, expectedProjects: 2);

                    var window = app.OpenSolutionExplorer();

                    SelectSolutionNode(window, "Solution 'MoveDuplicateFileName' (2 projects)");

                    var item1 = window.FindItem("Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName", "textfile1.txt");
                    var item2 = window.FindItem("Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName", "textfile2.txt");
                    mover(
                        window.FindItem("Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName2"),
                        item1,
                        item2
                    );

                    var dialog = new OverwriteFileDialog(app.WaitForDialog());
                    dialog.No();

                    System.Threading.Thread.Sleep(1000);

                    dialog = new OverwriteFileDialog(app.WaitForDialog());
                    dialog.Yes();

                    app.WaitForDialogDismissed();

                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "textfile1 - lang", "Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName", "textfile1.txt");
                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "textfile2 - lang", "Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName", "textfile2.txt");
                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "textfile1 - 2", "Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName2", "textfile1.txt");
                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "textfile2 - lang", "Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName2", "textfile2.txt");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveDuplicateFileNameCrossProjectSkipMoveKeyboard() {
            MoveDuplicateFileNameCrossProjectSkipMove(MoveByKeyboard);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveDuplicateFileNameCrossProjectSkipMoveMouse() {
            MoveDuplicateFileNameCrossProjectSkipMove(MoveByMouse);
        }

        /// <summary>
        /// Move item to where an item by that name exists across 2 projects of the same type.
        /// 
        /// https://pytools.codeplex.com/workitem/1967
        /// </summary>
        private void MoveDuplicateFileNameCrossProjectSkipMove(MoveDelegate mover) {
            foreach (var projectType in ProjectTypes) {
                var projectDefs = new[] {
                    new ProjectDefinition("MoveDuplicateFileName1",
                        projectType,
                        ItemGroup(
                            Content("textfile.txt", "MoveDuplicateFileName1")
                        )
                    ),
                    new ProjectDefinition("MoveDuplicateFileName2",
                        projectType,
                        ItemGroup(
                            Folder("Folder"),
                            Content("textfile.txt", "MoveDuplicateFileName2")
                        )
                    )
                };

                using (var solution = SolutionFile.Generate("MoveDuplicateFileName", projectDefs)) {
                    var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
                    app.OpenProject(solution.Filename, expectedProjects: 2);

                    var window = app.OpenSolutionExplorer();

                    SelectSolutionNode(window, "Solution 'MoveDuplicateFileName' (2 projects)");

                    mover(
                        window.FindItem("Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName2"),
                        window.FindItem("Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName1", "textfile.txt")
                    );

                    var dialog = new OverwriteFileDialog(app.WaitForDialog());
                    dialog.No();

                    app.WaitForDialogDismissed();

                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "MoveDuplicateFileName1", "Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName1", "textfile.txt");
                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "MoveDuplicateFileName2", "Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName2", "textfile.txt");
                }

            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveDuplicateFileNameCrossProjectCSharpSkipMoveKeyboard() {
            MoveDuplicateFileNameCrossProjectCSharpSkipMove(MoveByKeyboard);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveDuplicateFileNameCrossProjectCSharpSkipMoveMouse() {
            MoveDuplicateFileNameCrossProjectCSharpSkipMove(MoveByMouse);
        }

        /// <summary>
        /// Move item to where item exists across project types.
        /// </summary>
        private void MoveDuplicateFileNameCrossProjectCSharpSkipMove(MoveDelegate mover) {
            foreach (var projectType in ProjectTypes) {
                var projectDefs = new[] {
                    new ProjectDefinition("MoveDuplicateFileName1",
                        projectType,
                        ItemGroup(
                            Content("textfile.txt", "MoveDuplicateFileName1")
                        )
                    ),
                    new ProjectDefinition("MoveDuplicateFileNameCS",
                        ProjectType.CSharp,
                        ItemGroup(
                            Folder("Folder"),
                            Content("textfile.txt", "MoveDuplicateFileNameCS")
                        )
                    )
                };

                using (var solution = SolutionFile.Generate("MoveDuplicateFileName", projectDefs)) {
                    var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
                    app.OpenProject(solution.Filename, expectedProjects: 2);

                    
                    var window = app.OpenSolutionExplorer();

                    SelectSolutionNode(window, "Solution 'MoveDuplicateFileName' (2 projects)");

                    mover(
                        window.FindItem("Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileNameCS"),
                        window.FindItem("Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName1", "textfile.txt")
                    );

                    // say no to replacing in the C# project system
                    app.WaitForDialog();
                    Keyboard.Type(Key.N);

                    app.WaitForDialogDismissed();

                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "MoveDuplicateFileName1", "Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileName1", "textfile.txt");
                    window.AssertFileExistsWithContent(Path.GetDirectoryName(solution.Filename), "MoveDuplicateFileNameCS", "Solution 'MoveDuplicateFileName' (2 projects)", "MoveDuplicateFileNameCS", "textfile.txt");
                }

            }
        }

        /// <summary>
        /// Selects the solution node using the mouse.
        /// 
        /// This is used to reset the state of the mouse before a test as some
        /// tests can cause the mouse to be left in an odd state - the mouse up
        /// event is delivered to solution explorer, but selecting items later
        /// doesn't work because the mouse is left in an odd state.  If you
        /// make this method a nop and try and run all of the tests you'll
        /// see the bad behavior.
        /// </summary>
        private static void SelectSolutionNode(SolutionExplorerTree window, string name) {
            Mouse.MoveTo(window.WaitForItem(name).GetClickablePoint());
            Mouse.Click(MouseButton.Left);
        }

        /// <summary>
        /// Moves one or more items in solution explorer to the destination using the mouse.
        /// </summary>
        private static void MoveByMouse(AutomationElement destination, params AutomationElement[] source) {
            AutomationWrapper.Select(source.First());
            for (int i = 1; i < source.Length; i++) {
                AutomationWrapper.AddToSelection(source[i]);
            }

            var app = new VisualStudioApp(VsIdeTestHostContext.Dte);
            Mouse.MoveTo(source.First().GetClickablePoint());
            Mouse.Down(MouseButton.Left);

            try {
                Keyboard.Press(Key.LeftShift);
                Mouse.MoveTo(destination.GetClickablePoint());
                Mouse.Up(MouseButton.Left);
            } finally {
                Keyboard.Release(Key.LeftShift);
            }
        }

        /// <summary>
        /// Moves one or more items in solution explorer using the keyboard to cut and paste.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        private static void MoveByKeyboard(AutomationElement destination, params AutomationElement[] source) {
            AutomationWrapper.Select(source.First());
            for (int i = 1; i < source.Length; i++) {
                AutomationWrapper.AddToSelection(source[i]);
            }
            
            Keyboard.ControlX();

            AutomationWrapper.Select(destination);
            Keyboard.ControlV();
        }

        private delegate void MoveDelegate(AutomationElement destination, params AutomationElement[] source);
    }
}
