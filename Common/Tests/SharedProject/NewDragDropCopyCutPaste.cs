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
using System.IO;
using System.Windows.Input;
using EnvDTE;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.SharedProject;
using TestUtilities.UI;
using Mouse = TestUtilities.UI.Mouse;

namespace Microsoft.VisualStudioTools.SharedProjectTests {
    [TestClass]
    public class NewDragDropCopyCutPaste : SharedProjectTest {
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveExcludedFolder() {
            foreach (var projectKind in ProjectKinds) {
                var testDef = new ProjectDefinition("MoveExcludedFolder", 
                    projectKind, 
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

                    app.OpenSolutionExplorer();
                    var window = app.SolutionExplorerTreeView;

                    var folder = window.FindItem("Solution 'MoveExcludedFolder' (1 project)", "MoveExcludedFolder", "Foo");
                    var point = folder.GetClickablePoint();
                    Mouse.MoveTo(point);
                    Mouse.Down(MouseButton.Left);

                    var destFolder = window.FindItem("Solution 'MoveExcludedFolder' (1 project)", "MoveExcludedFolder", "Baz");
                    Mouse.MoveTo(destFolder.GetClickablePoint());
                    Mouse.Up(MouseButton.Left);

                    window.AssertFolderDoesntExist(Path.GetDirectoryName(solution.Filename), "Solution 'MoveExcludedFolder' (1 project)", "MoveExcludedFolder", "Foo");
                    window.AssertFolderExists(Path.GetDirectoryName(solution.Filename), "Solution 'MoveExcludedFolder' (1 project)", "MoveExcludedFolder", "Baz", "Foo");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MoveExcludedItemToFolder() {
            foreach (var projectKind in ProjectKinds) {
                var testDef = new ProjectDefinition("MoveExcludedItemToFolder", 
                    projectKind, 
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

                    app.OpenSolutionExplorer();
                    var window = app.SolutionExplorerTreeView;

                    var folder = window.FindItem("Solution 'MoveExcludedItemToFolder' (1 project)", "MoveExcludedItemToFolder", "codefile" + projectKind.CodeExtension);
                    var point = folder.GetClickablePoint();
                    Mouse.MoveTo(point);
                    Mouse.Down(MouseButton.Left);

                    var destFolder = window.FindItem("Solution 'MoveExcludedItemToFolder' (1 project)", "MoveExcludedItemToFolder", "Folder");
                    Mouse.MoveTo(destFolder.GetClickablePoint());
                    Mouse.Up(MouseButton.Left);

                    window.AssertFileDoesntExist(Path.GetDirectoryName(solution.Filename), "Solution 'MoveExcludedItemToFolder' (1 project)", "MoveExcludedItemToFolder", "codefile" + projectKind.CodeExtension);
                    window.AssertFileExists(Path.GetDirectoryName(solution.Filename), "Solution 'MoveExcludedItemToFolder' (1 project)", "MoveExcludedItemToFolder", "Folder", "codefile" + projectKind.CodeExtension);
                }
            }
        }
    }
}
