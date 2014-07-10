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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.NodejsTools.TestFrameworks;

namespace TestAdapterTests {

    [TestClass]
    public class FrameworkDiscoverTest {
        [TestMethod, Priority(0)]
        public void InitializeAllFrameworks() {
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
        public void DefaultFramework_HasCorrectFolderInformation() {
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
