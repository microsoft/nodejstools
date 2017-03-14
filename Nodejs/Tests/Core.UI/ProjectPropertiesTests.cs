// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools.VSTestHost;
using TestUtilities;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI
{
    [TestClass]
    public class ProjectPropertiesTests : NodejsProjectTest
    {
        [ClassInitialize]
        public static void DoDeployment(TestContext context)
        {
            AssertListener.Initialize();
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void DirtyProperties()
        {
            using (var solution = Project("DirtyProperties").Generate().ToVs())
            {
                var proj = solution.FindItem("DirtyProperties");
                AutomationWrapper.Select(proj);
                solution.ExecuteCommand("ClassViewContextMenus.ClassViewMultiselectProjectreferencesItems.Properties");
                var window = VSTestContext.DTE.Windows.Item("DirtyProperties");
                Assert.AreEqual(window.Caption, "DirtyProperties");

                solution.GetProject("DirtyProperties").Properties.Item("NodejsPort").Value = 3000;
                Assert.AreEqual(false, solution.GetProject("DirtyProperties").Saved);
            }
        }
    }
}

