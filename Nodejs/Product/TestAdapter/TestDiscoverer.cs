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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;

using MSBuild = Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.TestAdapter {
    [FileExtension(".njsproj")]
    [DefaultExecutorUri(TestExecutor.ExecutorUriString)]
    class TestDiscoverer : ITestDiscoverer {
        public TestDiscoverer() {
        }

        /// <summary>
        /// ITestDiscover, Given a list of test sources this method pulls out the test cases
        /// </summary>
        /// <param name="sources">List of test sources passed from client (Client can be VS or command line)</param>
        /// <param name="discoveryContext">Context and runSettings for current run.  Discoverer pulls out the tests based on current context</param>
        /// <param name="logger">Used to relay messages to registered loggers</param>
        /// <param name="discoverySink">Callback used to notify client upon discovery of test cases</param>
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink) {
            ValidateArg.NotNull(sources, "sources");
            ValidateArg.NotNull(discoverySink, "discoverySink");
            ValidateArg.NotNull(logger, "logger");

            using (var buildEngine = new MSBuild.ProjectCollection()) {
                try {
                    // Load all the test containers passed in (.pyproj msbuild files)
                    foreach (string source in sources) {
                        buildEngine.LoadProject(source);
                    }

                    foreach (var proj in buildEngine.LoadedProjects) {

                        var projectHome = Path.GetFullPath(Path.Combine(proj.DirectoryPath, "."));

                        NameValueCollection testItems = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
                        // Provide all files to the test analyzer
                        foreach (var item in proj.Items.Where(item => item.ItemType == "Compile" || item.ItemType == "TypeScriptCompile")) {

                            //Check to see if this is a TestCase
                            string value = item.GetMetadataValue("TestFramework");
                            if (!TestContainerDiscoverer.IsValidTestFramework(value)) {
                                continue;
                            }
                            string fileAbsolutePath = CommonUtils.GetAbsoluteFilePath(projectHome, item.EvaluatedInclude);

                            if (Path.GetExtension(fileAbsolutePath).Equals(NodejsConstants.TypeScriptExtension, StringComparison.OrdinalIgnoreCase)) {
                                //We're dealing with TypeScript, switch to the underlying js file
                                fileAbsolutePath = fileAbsolutePath.Substring(0, fileAbsolutePath.Length - 3) + ".js";
                            } else if (!Path.GetExtension(fileAbsolutePath).Equals(".js", StringComparison.OrdinalIgnoreCase)) {
                                continue;
                            }

                            testItems.Add(value, fileAbsolutePath);
                        }

                        //Debug.Fail("Before Discover");
                        DiscoverTests(testItems, proj, discoverySink, logger);
                    }
                } finally {
                    // Disposing buildEngine does not clear the document cache in
                    // VS 2013, so manually unload all projects before disposing.
                    buildEngine.UnloadAllProjects();
                }
            }
        }

        private void DiscoverTests(NameValueCollection testItems, MSBuild.Project proj, ITestCaseDiscoverySink discoverySink, IMessageLogger logger) {
            List<TestFrameworks.NodejsTestInfo> result = new List<TestFrameworks.NodejsTestInfo>();
            var projectHome = Path.GetFullPath(Path.Combine(proj.DirectoryPath, "."));
            var projSource = ((MSBuild.Project)proj).FullPath;

            var nodeExePath = proj.GetPropertyValue(NodejsConstants.NodeExePath);
            if (string.IsNullOrEmpty(nodeExePath)) {
                nodeExePath = NodejsTools.Nodejs.NodeExePath;
            }

            if (!File.Exists(nodeExePath)) {
                logger.SendMessage(TestMessageLevel.Error, String.Format("Node.exe was not found.  Please install Node.js before running tests."));
                return;
            }
            //TODO
            // string files = string.Join(";", testItems.GetValues(null));
            //logger.SendMessage(TestMessageLevel.Informational, String.Format("Processing {0}", files));
            int testCount = 0;
            foreach (string testFx in testItems.Keys) {
                TestFrameworks.TestFramework testFramework = GetTestFrameworkObject(testFx);
                if (testFramework == null) {
                    logger.SendMessage(TestMessageLevel.Warning, String.Format("Ignoring unsupported test framework {0}", testFx));
                    continue;
                }

                List<TestFrameworks.NodejsTestInfo> discoveredTestCases = testFramework.FindTests(testItems.GetValues(testFx), nodeExePath, logger, projectHome);
                testCount += discoveredTestCases.Count;
                foreach (TestFrameworks.NodejsTestInfo discoveredTest in discoveredTestCases) {
                    string qualifiedName = discoveredTest.FullyQualifiedName;
                    logger.SendMessage(TestMessageLevel.Informational, String.Format("Creating TestCase:{0}", qualifiedName));
                    discoverySink.SendTestCase(new TestCase(qualifiedName, TestExecutor.ExecutorUri, projSource) {
                        CodeFilePath = discoveredTest.ModulePath,
                        LineNumber = 0,
                        DisplayName = discoveredTest.TestName
                    });
                }
            }
            if (testCount == 0) {
                logger.SendMessage(TestMessageLevel.Warning, String.Format("Discovered 0 testcases."));
            }
            //logger.SendMessage(TestMessageLevel.Informational, String.Format("Processing Finished {0}", files));
        }

        private TestFrameworks.TestFramework GetTestFrameworkObject(string testFramework) {
            TestFrameworks.FrameworkDiscover discover = new TestFrameworks.FrameworkDiscover();
            return discover.Get(testFramework);
        }

        internal static MSBuild.Project LoadProject(MSBuild.ProjectCollection buildEngine, string fullProjectPath) {
            var buildProject = buildEngine.GetLoadedProjects(fullProjectPath).FirstOrDefault();

            if (buildProject != null) {
                buildEngine.UnloadProject(buildProject);
            }
            return buildEngine.LoadProject(fullProjectPath);
        }
    }
}
