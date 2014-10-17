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
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Project.ImportWizard;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Mocks;
using TestUtilities.Nodejs;

namespace NodejsTests {
    [TestClass]
    public class ProjectUpgradeTests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        [TestMethod, TestCategory("Core"), Priority(0)]
        public void UpgradeEnvironmentVariables() {
            var factory = new BaseNodeProjectFactory(null);
            var sp = new MockServiceProvider();
            sp.Services["SVsQueryEditQuerySave"] = null;
            sp.Services["SVsActivityLog"] = new MockActivityLog();
            factory.Site = sp;

            var upgrade = (IVsProjectUpgradeViaFactory)factory;

            // Use a copy of the project so we don't interfere with other
            // tests using them.
            var origProject = TestData.GetPath(Path.Combine("TestData", "ProjectUpgrade", "EnvironmentVariables.njsproj"));
            var tempProject = Path.Combine(TestData.GetTempPath("ProjectUpgrade"), "EnvironmentVariables.njsproj");
            File.Copy(origProject, tempProject);

            int actual;
            Guid factoryGuid;
            uint flags;
            var hr = upgrade.UpgradeProject_CheckOnly(
                tempProject,
                null,
                out actual,
                out factoryGuid,
                out flags
            );

            Assert.AreEqual(0, hr);
            Assert.AreEqual(1, actual);
            Assert.AreEqual(typeof(BaseNodeProjectFactory).GUID, factoryGuid);

            string newLocation;
            hr = upgrade.UpgradeProject(
                tempProject,
                0u,
                null,
                out newLocation,
                null,
                out actual,
                out factoryGuid);

            Assert.AreEqual(0, hr);
            Assert.AreEqual(1, actual);
            Assert.AreEqual(typeof(BaseNodeProjectFactory).GUID, factoryGuid);

            Assert.IsTrue(File.ReadAllText(tempProject).Contains("<Environment>fob=1\r\nbar=2</Environment>"));
            Assert.IsFalse(File.ReadAllText(tempProject).Contains("<EnvironmentVariables>"));
        }
    }
}
