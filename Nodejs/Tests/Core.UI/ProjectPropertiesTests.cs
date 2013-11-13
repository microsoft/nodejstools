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

using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class ProjectPropertiesTests : NodejsProjectTest {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void DirtyProperties() {
            using (var solution = Project("DirtyProperties").Generate().ToVs()) {
                var proj = solution.FindItem("DirtyProperties");
                AutomationWrapper.Select(proj);
                solution.App.Dte.ExecuteCommand("ClassViewContextMenus.ClassViewMultiselectProjectreferencesItems.Properties");
                var window = VsIdeTestHostContext.Dte.Windows.Item("DirtyProperties");
                Assert.AreEqual(window.Caption, "DirtyProperties");

                solution.Project.Properties.Item("NodejsPort").Value = 3000;
                Assert.AreEqual(false, solution.Project.Saved);
            }
        }
    }
}
