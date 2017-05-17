// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.NodejsTools;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Mocks;
using TestUtilities.Nodejs;

namespace NodejsTests
{
    [TestClass]
    public class ProjectUpgradeTests
    {
        [ClassInitialize]
        public static void DoDeployment(TestContext context)
        {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        [TestMethod, TestCategory("Core"), Priority(0), TestCategory("Ignore")]
        public void UpgradeEnvironmentVariables()
        {
            var factory = new BaseNodeProjectFactory(null);
            var sp = new MockServiceProvider();
            sp.Services[typeof(SVsQueryEditQuerySave).GUID] = null;
            sp.Services[typeof(SVsActivityLog).GUID] = new MockActivityLog();
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

