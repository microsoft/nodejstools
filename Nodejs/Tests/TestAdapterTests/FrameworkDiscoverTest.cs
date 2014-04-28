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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;

namespace TestAdapterTests {

    [TestClass]
    public class FrameworkDiscoverTest {
        [TestMethod]
        public void InitializeAllFrameworks() {
            //Arrange and Act
            FrameworkDiscover discover = new FrameworkDiscover("c:\\Dummy");

            //Assert
            TestFramework defaultOne = discover.Get("Default");
            Assert.IsNotNull(defaultOne);
            TestFramework mocha = discover.Get("moCHA");//searching on name is case insensitive
            Assert.IsNotNull(mocha);
            TestFramework nonSenseOne = discover.Get("NonSense");
            Assert.IsNull(nonSenseOne);
        }

        [TestMethod]
        public void DefaultFramework_HasCorrectFolderInformation() {
            //Arrange
            string testName = "dummyUT";
            string testFile = "dummyTestFile.js";
            string vsixInstallFolder = "c:\\dummyFolder";
            string workingFolder = "c:\\DummyNodejsProject";
            string frameworkName = "Default";
            FrameworkDiscover discover = new FrameworkDiscover(vsixInstallFolder);

            //Act
            TestFramework defaultOne = discover.Get(frameworkName);
            string[] args = defaultOne.ArgumentsToRunTests(testName, testFile, workingFolder);

            //Assert
            Assert.AreEqual("\"" + vsixInstallFolder + "\\TestFrameworks\\run_tests.js" + "\"", args[0]);
            Assert.AreEqual(frameworkName, args[1]);
            Assert.AreEqual("\"" + testName + "\"", args[2]);
            Assert.AreEqual("\"" + testFile + "\"", args[3]);
            Assert.AreEqual("\"" + workingFolder + "\"", args[4]);
        }
    }
}
