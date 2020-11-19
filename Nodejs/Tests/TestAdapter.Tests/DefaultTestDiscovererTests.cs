// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.NodejsTools.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using static TestAdapter.Tests.TestProjectFactory;

namespace TestAdapter.Tests
{
    [TestClass]
    public class DefaultTestDiscovererTests
    {
        private void AssureNodeModules(string path)
        {
            if (Directory.Exists(Path.Combine(path, "node_modules")))
            {
                return;
            }

            var processStartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = "/C npm install",
                WorkingDirectory = path
            };

            var process = Process.Start(processStartInfo);
            process.WaitForExit(10 * 60 * 1000); // 10 minutes
        }

        private bool PathEquals(string path1, string path2)
        {
            return (path1 == null && path2 == null)
                || (path1 != null && path2 != null && string.Equals(Path.GetFullPath(path1), Path.GetFullPath(path2), StringComparison.OrdinalIgnoreCase));
        }

        private void AssertTestCases(List<TestCase> expected, List<TestCase> actual)
        {
            if (expected.Count != actual.Count)
            {
                Assert.Fail($"Expected and actual does not have the same amount of items. Expected.Count = {expected.Count}, Actual.Count = {actual.Count}");
            }

            var expectedCopy = new List<TestCase>(expected);
            foreach (var testCase in actual)
            {
                var found = expectedCopy.Find(x =>
                    x.FullyQualifiedName == testCase.FullyQualifiedName
                    && x.DisplayName == testCase.DisplayName
                    && x.LineNumber == testCase.LineNumber
                    && x.ExecutorUri == testCase.ExecutorUri
                    && this.PathEquals(x.Source, testCase.Source)
                    && this.PathEquals(x.CodeFilePath, testCase.CodeFilePath)
                    && string.Equals(x.GetPropertyValue<string>(JavaScriptTestCaseProperties.TestFramework, default), testCase.GetPropertyValue<string>(JavaScriptTestCaseProperties.TestFramework, default), StringComparison.OrdinalIgnoreCase) // For some reason, mocha test framework is all lowercase.
                    && this.PathEquals(x.GetPropertyValue<string>(JavaScriptTestCaseProperties.WorkingDir, default), testCase.GetPropertyValue<string>(JavaScriptTestCaseProperties.WorkingDir, default))
                    && this.PathEquals(x.GetPropertyValue<string>(JavaScriptTestCaseProperties.ProjectRootDir, default), testCase.GetPropertyValue<string>(JavaScriptTestCaseProperties.ProjectRootDir, default))
                    && this.PathEquals(x.GetPropertyValue<string>(JavaScriptTestCaseProperties.TestFile, default), testCase.GetPropertyValue<string>(JavaScriptTestCaseProperties.TestFile, default))
                    && this.PathEquals(x.GetPropertyValue<string>(JavaScriptTestCaseProperties.ConfigDirPath, default), testCase.GetPropertyValue<string>(JavaScriptTestCaseProperties.ConfigDirPath, default)));

                if (found == null)
                {
                    Assert.Fail($"Expected does not have item: {JsonConvert.SerializeObject(testCase)}");
                }

                expectedCopy.Remove(found);
            }
        }

        private void AssertProject(ProjectName projectName, int expectedTests)
        {
            // Arrange
            var testProjectFactory = new TestProjectFactory(projectName);

            var actual = new List<TestCase>();
            var expected = testProjectFactory.GetTestCases();

            var discoverContext = new Mock<IDiscoveryContext>();
            var messageLogger = new Mock<IMessageLogger>();
            var testCaseDiscoverySink = new Mock<ITestCaseDiscoverySink>();

            testCaseDiscoverySink.Setup(x => x.SendTestCase(It.IsAny<TestCase>()))
                .Callback<TestCase>(x => actual.Add(x));

            var testSource = testProjectFactory.GetProjectFilePath();
            var sources = new List<string>() { testSource };

            AssureNodeModules(testProjectFactory.GetProjectDirPath());

            var testDiscoverer = new DefaultTestDiscoverer();

            // Act
            testDiscoverer.DiscoverTests(sources, discoverContext.Object, messageLogger.Object, testCaseDiscoverySink.Object);

            // Assert
            testCaseDiscoverySink.Verify(x => x.SendTestCase(It.IsAny<TestCase>()), Times.Exactly(expectedTests));
            this.AssertTestCases(expected, actual);
        }

        [TestInitialize]
        public void InitializeTests()
        {
            // Setup the variable due to LoadProjects looks for a specific path of Microsoft.NodejsToolsV2.targets.
            // TODO: Probably we could remove this dependency by configuring the BuildOutput path, but I haven't found how to do it.
            Environment.SetEnvironmentVariable("VSINSTALLDIR", @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Preview");
        }

        [TestMethod]
        public void DiscoversTests_ConfiguredPerFile()
        {
            this.AssertProject(ProjectName.NodeAppWithTestsConfiguredPerFile, 10);
        }

        [TestMethod]
        public void DiscoversTests_Node_ConfiguredOnProject()
        {
            this.AssertProject(ProjectName.NodeAppWithTestsConfiguredOnProject, 2);
        }

        [TestMethod]
        public void DiscoversTests_NodeAppWithAngularTests()
        {
            this.AssertProject(ProjectName.NodeAppWithAngularTests, 5);
        }
    }
}
