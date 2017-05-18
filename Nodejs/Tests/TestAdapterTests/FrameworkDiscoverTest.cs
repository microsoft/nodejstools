// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.NodejsTools.TestFrameworks;

namespace TestAdapterTests
{
    [TestClass]
    public class FrameworkDiscoverTest
    {
        [TestMethod, Priority(0)]
        public void InitializeAllFrameworks()
        {
            //Arrange and Act
            string[] frameworkDirectories = new string[] {
                "c:\\nodejstools\\" + TestFrameworkDirectories.ExportRunnerFramework,
                "c:\\nodejstools\\" + "mocha"
             };
            FrameworkDiscover discover = new FrameworkDiscover(frameworkDirectories);

            //Assert
            TestFramework defaultOne = discover.Get(TestFrameworkDirectories.ExportRunnerFramework);
            Assert.IsNotNull(defaultOne);
            TestFramework mocha = discover.Get("moCHA");//searching on name is case insensitive
            Assert.IsNotNull(mocha);
            TestFramework nonSenseOne = discover.Get("NonSense");
            Assert.IsNull(nonSenseOne);
        }

        [TestMethod, Priority(0)]
        public void DefaultFramework_HasCorrectFolderInformation()
        {
            //Arrange
            string testName = "dummyUT";
            string testFile = "dummyTestFile.js";
            string vsixInstallFolder = "c:\\dummyFolder";
            string workingFolder = "c:\\DummyNodejsProject";
            string framework = TestFrameworkDirectories.ExportRunnerFramework;
            string testFrameworkDirectory = vsixInstallFolder + "\\" + framework;
            FrameworkDiscover discover = new FrameworkDiscover(new string[] { testFrameworkDirectory });

            //Act
            TestFramework defaultOne = discover.Get(TestFrameworkDirectories.ExportRunnerFramework);
            string[] args = defaultOne.ArgumentsToRunTests(testName, testFile, workingFolder, workingFolder);

            //Assert
            Assert.AreEqual("\"" + vsixInstallFolder + "\\run_tests.js" + "\"", args[0]);
            Assert.AreEqual(framework, args[1]);
            Assert.AreEqual("\"" + testName + "\"", args[2]);
            Assert.AreEqual("\"" + testFile + "\"", args[3]);
            Assert.AreEqual("\"" + workingFolder + "\"", args[4]);
        }
    }
}

