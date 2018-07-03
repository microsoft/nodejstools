// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.TestAdapter
{
    [FileExtension(".json")]
    [DefaultExecutorUri(NodejsConstants.PackageJsonExecutorUriString)]
    public sealed class PackageJsonTestDiscoverer : IJavaScriptTestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            AssemblyResolver.SetupHandler();
            foreach (var source in sources)
            {
                // we're only interested in package.json files here.
                if (PackageJsonFactory.IsPackageJsonFile(source))
                {
                    this.DiscoverTestFiles(source, logger, discoverySink);
                }
            }
        }

        public void DiscoverTests(UnitTestSettings unitTestSettings, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {

        }

        private void DiscoverTestFiles(string packageJsonPath, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            logger.SendMessage(TestMessageLevel.Informational, $"Parsing '{packageJsonPath}'.");

            var packageJson = PackageJsonFactory.Create(packageJsonPath);
            if (string.IsNullOrEmpty(packageJson.TestRoot))
            {
                logger.SendMessage(TestMessageLevel.Informational, "No vsTestOptions|testRoot specified.");
                return;
            }

            var workingDir = Path.GetDirectoryName(packageJsonPath);
            var testFolderPath = Path.Combine(workingDir, packageJson.TestRoot);

            if (!Directory.Exists(testFolderPath))
            {
                logger.SendMessage(TestMessageLevel.Error, $"Testroot '{packageJson.TestRoot}' doesn't exist.");
                return;
            }

            TestFramework testFx = null;

            foreach (var dep in packageJson.AllDependencies)
            {
                testFx = FrameworkDiscoverer.Instance.Get(dep.Name);
                if (testFx != null)
                {
                    break;
                }
            }
            testFx = testFx ?? FrameworkDiscoverer.Instance.Get("ExportRunner");

            this.DiscoverTestFiles(testFolderPath, testFx, logger, discoverySink);
        }

        private void DiscoverTestFiles(string testFolderPath, TestFramework testFx, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            var workingDir = ".";
            var packageJsonPath = ".\\package.json";

            var nodeExePath = Nodejs.GetPathToNodeExecutableFromEnvironment();

            if (!File.Exists(nodeExePath))
            {
                logger.SendMessage(TestMessageLevel.Error, "Node.exe was not found. Please install Node.js before running tests.");
                return;
            }

            var fileList = Directory.GetFiles(testFolderPath, "*.js", SearchOption.AllDirectories);
            var files = string.Join(";", fileList);
            logger.SendMessage(TestMessageLevel.Informational, $"Processing: {files}");

            var discoveredTestCases = testFx.FindTests(fileList, nodeExePath, logger, projectRoot: workingDir);
            if (!discoveredTestCases.Any())
            {
                logger.SendMessage(TestMessageLevel.Warning, "Discovered 0 testcases.");
                return;
            }

            foreach (var discoveredTest in discoveredTestCases)
            {
                var qualifiedName = discoveredTest.FullyQualifiedName;
                const string indent = "  ";
                logger.SendMessage(TestMessageLevel.Informational, $"{indent}Creating TestCase:{qualifiedName}");
                //figure out the test source info such as line number
                var filePath = CommonUtils.GetAbsoluteFilePath(workingDir, discoveredTest.TestFile);

                var testcase = new TestCase(qualifiedName, NodejsConstants.PackageJsonExecutorUri, packageJsonPath)
                {
                    CodeFilePath = filePath,
                    LineNumber = discoveredTest.SourceLine,
                    DisplayName = discoveredTest.TestName
                };

                testcase.SetPropertyValue(JavaScriptTestCaseProperties.TestFramework, testFx.Name);
                testcase.SetPropertyValue(JavaScriptTestCaseProperties.WorkingDir, workingDir);
                testcase.SetPropertyValue(JavaScriptTestCaseProperties.ProjectRootDir, workingDir);
                testcase.SetPropertyValue(JavaScriptTestCaseProperties.NodeExePath, nodeExePath);
                testcase.SetPropertyValue(JavaScriptTestCaseProperties.TestFile, filePath);

                discoverySink.SendTestCase(testcase);
            }

            logger.SendMessage(TestMessageLevel.Informational, $"Processing finished for framework '{testFx.Name}'.");
        }
    }
}
