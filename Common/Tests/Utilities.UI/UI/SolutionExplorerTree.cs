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
using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI {
    public class SolutionExplorerTree : TreeView {
        public SolutionExplorerTree(AutomationElement element)
            : base(element) {
        }

        public void AssertFileExists(string projectLocation, params string[] path) {
            Assert.IsNotNull(WaitForItem(path), "Item not found in solution explorer" + String.Join("\\", path));

            var basePath = projectLocation;
            for (int i = 1; i < path.Length; i++) {
                basePath = Path.Combine(basePath, path[i]);
            }
            Assert.IsTrue(File.Exists(basePath), "File doesn't exist: " + basePath);
        }

        public void AssertFileDoesntExist(string projectLocation, params string[] path) {
            Assert.IsNull(FindItem(path), "Item exists in solution explorer: " + String.Join("\\", path));

            var basePath = projectLocation;
            for (int i = 1; i < path.Length; i++) {
                basePath = Path.Combine(basePath, path[i]);
            }
            Assert.IsFalse(File.Exists(basePath), "File exists: " + basePath);
        }

        public void AssertFolderExists(string projectLocation, params string[] path) {
            Assert.IsNotNull(WaitForItem(path), "Item not found in solution explorer" + String.Join("\\", path));

            var basePath = projectLocation;
            for (int i = 1; i < path.Length; i++) {
                basePath = Path.Combine(basePath, path[i]);
            }
            Assert.IsTrue(Directory.Exists(basePath), "File doesn't exist: " + basePath);
        }

        public void AssertFolderDoesntExist(string projectLocation, params string[] path) {
            Assert.IsNull(WaitForItemRemoved(path), "Item exists in solution explorer: " + String.Join("\\", path));

            var basePath = projectLocation;
            for (int i = 1; i < path.Length; i++) {
                basePath = Path.Combine(basePath, path[i]);
            }
            Assert.IsFalse(Directory.Exists(basePath), "File exists: " + basePath);
        }

    }
}
