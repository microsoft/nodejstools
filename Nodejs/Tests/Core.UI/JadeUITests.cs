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
using EnvDTE;
using Microsoft.NodejsTools;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools;
using TestUtilities;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class JadeUITests : NodejsProjectTest {
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void InsertTabs() {
            using (new OptionHolder("TextEditor", "Jade", "InsertTabs", true)) {
                using (var solution = Project("TabsSpaces", Content("quox.jade", "ul\r\n    li A\r\n    li B")).Generate().ToVs()) {
                    var jadeFile = solution.OpenItem("TabsSpaces", "quox.jade");
                    jadeFile.MoveCaret(1, 1);
                    Keyboard.Type("\t");
                    Assert.AreEqual(jadeFile.Text, "\tul\r\n    li A\r\n    li B");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void InsertSpaces() {
            using (new OptionHolder("TextEditor", "Jade", "InsertTabs", false)) {
                using (var solution = Project("TabsSpaces", Content("quox.jade", "ul\r\n    li A\r\n    li B")).Generate().ToVs()) {
                    var jadeFile = solution.OpenItem("TabsSpaces", "quox.jade");
                    jadeFile.MoveCaret(1, 1);
                    Keyboard.Type("\t");
                    Assert.AreEqual(jadeFile.Text, "    ul\r\n    li A\r\n    li B");
                }
            }
        }
    }
}
