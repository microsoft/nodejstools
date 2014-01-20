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

using System.Windows.Automation;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class NodejsBasicProjectTests : NodejsProjectTest {
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void AddNewTypeScriptItem() {
            using (var solution = Project("AddNewTypeScriptItem", Compile("server")).Generate().ToVs()) {
                var project = solution.WaitForItem("AddNewTypeScriptItem", "server.js");
                AutomationWrapper.Select(project);

                var dialog = solution.App.OpenDialogWithDteExecuteCommand("Project.AddNewItem");
                var newItem = new NewItemDialog(AutomationElement.FromHandle(dialog));
                newItem.FileName = "NewTSFile.ts";
                newItem.ClickOK();

                VsIdeTestHostContext.Dte.ExecuteCommand("Build.BuildSolution");
                solution.App.WaitForOutputWindowText("Build", "tsc.exe");
            }
        }
    }
}
