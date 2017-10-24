// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.NodejsTools.SourceMapping;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.NodejsTools.TestAdapter
{
    public class TestDiscoverer
    {
        public TestDiscoverer()
        {
        }

        /// <summary>
        /// ITestDiscover, Given a list of test sources this method pulls out the test cases
        /// </summary>
        /// <param name="sources">List of test sources passed from client (Client can be VS or command line)</param>
        /// <param name="discoveryContext">Context and runSettings for current run.  Discoverer pulls out the tests based on current context</param>
        /// <param name="logger">Used to relay messages to registered loggers</param>
        /// <param name="discoverySink">Callback used to notify client upon discovery of test cases</param>
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            ValidateArg.NotNull(sources, "sources");
            ValidateArg.NotNull(discoverySink, "discoverySink");
            ValidateArg.NotNull(logger, "logger");

            var env = new Dictionary<string, string>();
            var root = Environment.GetEnvironmentVariable(NodejsConstants.NodeToolsVsInstallRootEnvironmentVariable);

            if (!string.IsNullOrEmpty(root))
            {
                env["VsInstallRoot"] = root;
                env["MSBuildExtensionsPath32"] = Path.Combine(root, "MSBuild");
            }

            using (var buildEngine = new MSBuild.ProjectCollection(env))
            {
                try
                {
                    // Load all the test containers passed in (.njsproj msbuild files)
                    foreach (var source in sources)
                    {
                        buildEngine.LoadProject(source);
                    }

                    foreach (var proj in buildEngine.LoadedProjects)
                    {
                        var projectHome = Path.GetFullPath(Path.Combine(proj.DirectoryPath, "."));

                        var testItems = new Dictionary<string, List<TestFileEntry>>(StringComparer.OrdinalIgnoreCase);
                        // Provide all files to the test analyzer
                        foreach (var item in proj.Items.Where(item => item.ItemType == "Compile" || item.ItemType == "TypeScriptCompile"))
                        {
                            //Check to see if this is a TestCase
                            var value = item.GetMetadataValue("TestFramework");
                            if (!TestContainerDiscoverer.IsValidTestFramework(value))
                            {
                                continue;
                            }
                            var fileAbsolutePath = CommonUtils.GetAbsoluteFilePath(projectHome, item.EvaluatedInclude);
                            var typeScriptTest = TypeScript.TypeScriptHelpers.IsTypeScriptFile(fileAbsolutePath);
                            if (typeScriptTest)
                            {
                                fileAbsolutePath = TypeScript.TypeScriptHelpers.GetTypeScriptBackedJavaScriptFile(proj, fileAbsolutePath);
                            }
                            else if (!Path.GetExtension(fileAbsolutePath).Equals(".js", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            if (!testItems.TryGetValue(value, out var fileList))
                            {
                                fileList = new List<TestFileEntry>();
                                testItems.Add(value, fileList);
                            }
                            fileList.Add(new TestFileEntry(fileAbsolutePath, typeScriptTest));
                        }

                        DiscoverTests(testItems, proj, discoverySink, logger);
                    }
                }
                catch (Exception ex)
                {
                    logger.SendMessage(TestMessageLevel.Error, ex.Message);
                    throw;
                }
                finally
                {
                    // Disposing buildEngine does not clear the document cache in
                    // VS 2013, so manually unload all projects before disposing.
                    buildEngine.UnloadAllProjects();
                }
            }
        }

        private void DiscoverTests(Dictionary<string, List<TestFileEntry>> testItems, MSBuild.Project proj, ITestCaseDiscoverySink discoverySink, IMessageLogger logger)
        {
            var result = new List<TestFrameworks.NodejsTestInfo>();
            var projectHome = Path.GetFullPath(Path.Combine(proj.DirectoryPath, "."));
            var projSource = proj.FullPath;

            var nodeExePath =
                Nodejs.GetAbsoluteNodeExePath(
                    projectHome,
                    proj.GetPropertyValue(NodeProjectProperty.NodeExePath));

            if (!File.Exists(nodeExePath))
            {
                logger.SendMessage(TestMessageLevel.Error, string.Format(CultureInfo.CurrentCulture, "Node.exe was not found.  Please install Node.js before running tests."));
                return;
            }

            var testCount = 0;
            foreach (var testFx in testItems.Keys)
            {
                var testFramework = GetTestFrameworkObject(testFx);
                if (testFramework == null)
                {
                    logger.SendMessage(TestMessageLevel.Warning, string.Format(CultureInfo.CurrentCulture, "Ignoring unsupported test framework {0}", testFx));
                    continue;
                }

                var fileList = testItems[testFx];
                var files = string.Join(";", fileList.Select(p => p.File));
                logger.SendMessage(TestMessageLevel.Informational, string.Format(CultureInfo.CurrentCulture, "Processing: {0}", files));

                var discoveredTestCases = testFramework.FindTests(fileList.Select(p => p.File), nodeExePath, logger, projectHome);
                testCount += discoveredTestCases.Count;
                foreach (var discoveredTest in discoveredTestCases)
                {
                    var qualifiedName = discoveredTest.FullyQualifiedName;
                    logger.SendMessage(TestMessageLevel.Informational, string.Format(CultureInfo.CurrentCulture, "  " /*indent*/ + "Creating TestCase:{0}", qualifiedName));
                    //figure out the test source info such as line number
                    var filePath = discoveredTest.ModulePath;
                    var entry = fileList.Find(p => p.File.Equals(filePath, StringComparison.OrdinalIgnoreCase));
                    FunctionInformation fi = null;
                    if (entry.IsTypeScriptTest)
                    {
                        fi = SourceMapper.MaybeMap(new FunctionInformation(string.Empty,
                                                                           discoveredTest.TestName,
                                                                           discoveredTest.SourceLine,
                                                                           entry.File));
                    }
                    discoverySink.SendTestCase(
                        new TestCase(qualifiedName, TestExecutor.ExecutorUri, projSource)
                        {
                            CodeFilePath = (fi != null) ? fi.Filename : filePath,
                            LineNumber = (fi != null && fi.LineNumber.HasValue) ? fi.LineNumber.Value : discoveredTest.SourceLine,
                            DisplayName = discoveredTest.TestName
                        });
                }
                logger.SendMessage(TestMessageLevel.Informational, string.Format(CultureInfo.CurrentCulture, "Processing finished for framework of {0}", testFx));
            }
            if (testCount == 0)
            {
                logger.SendMessage(TestMessageLevel.Warning, string.Format(CultureInfo.CurrentCulture, "Discovered 0 testcases."));
            }
        }

        private TestFrameworks.TestFramework GetTestFrameworkObject(string testFramework)
        {
            var discover = new TestFrameworks.FrameworkDiscover();
            return discover.Get(testFramework);
        }

        internal static MSBuild.Project LoadProject(MSBuild.ProjectCollection buildEngine, string fullProjectPath)
        {
            var buildProject = buildEngine.GetLoadedProjects(fullProjectPath).FirstOrDefault();

            if (buildProject != null)
            {
                buildEngine.UnloadProject(buildProject);
            }
            return buildEngine.LoadProject(fullProjectPath);
        }

        private class TestFileEntry
        {
            public string File { get; set; }
            public bool IsTypeScriptTest { get; set; }

            public TestFileEntry(string file, bool isTypeScriptTest)
            {
                this.File = file;
                this.IsTypeScriptTest = isTypeScriptTest;
            }
        }
    }
}
