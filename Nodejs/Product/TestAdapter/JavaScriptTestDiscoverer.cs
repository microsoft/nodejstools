// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.NodejsTools.SourceMapping;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.NodejsTools.TestAdapter
{
    [FileExtension(".njsproj"), FileExtension(".csproj"), FileExtension(".vbproj")]
    [DefaultExecutorUri(NodejsConstants.ExecutorUriString)]
    public partial class JavaScriptTestDiscoverer : ITestDiscoverer
    {
        internal static AssemblyResolver AssemblyResolver { get; set; }

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            if (JavaScriptTestDiscoverer.AssemblyResolver == null)
            {
                JavaScriptTestDiscoverer.AssemblyResolver = new AssemblyResolver();
            }
            this.DiscoverTestsCore(sources, discoveryContext, logger, discoverySink);
        }

        /// <summary>
        /// ITestDiscover, Given a list of test sources this method pulls out the test cases
        /// </summary>
        /// <param name="sources">List of test sources passed from client (Client can be VS or command line)</param>
        /// <param name="discoveryContext">Context and runSettings for current run.  Discoverer pulls out the tests based on current context</param>
        /// <param name="logger">Used to relay messages to registered loggers</param>
        /// <param name="discoverySink">Callback used to notify client upon discovery of test cases</param>
        private void DiscoverTestsCore(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            ValidateArg.NotNull(sources, nameof(sources));
            ValidateArg.NotNull(discoverySink, nameof(discoverySink));
            ValidateArg.NotNull(logger, nameof(logger));

            var env = new Dictionary<string, string>();
            var root = Environment.GetEnvironmentVariable("VSINSTALLDIR");

#if DEBUG
            logger.SendMessage(TestMessageLevel.Informational, $"VSINSTALLDIR: {root}");
#endif

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

                        var testItems = new Dictionary<string, HashSet<TestFileEntry>>(StringComparer.OrdinalIgnoreCase);

                        var testRoot = proj.GetProperty(NodeProjectProperty.TestRoot)?.EvaluatedValue;
                        var testFramework = proj.GetProperty(NodeProjectProperty.TestFramework)?.EvaluatedValue;

                        if (!string.IsNullOrEmpty(testRoot) && string.IsNullOrEmpty(testFramework))
                        {
                            logger.SendMessage(TestMessageLevel.Warning, $"TestRoot specified for '{Path.GetFileName(proj.FullPath)}' but no TestFramework.");
                        }

                        // Provide all files to the test analyzer
                        foreach (var item in proj.Items.Where(item => item.ItemType != "None"))
                        {
                            string testFrameworkName;
                            string fileAbsolutePath;
                            if (!string.IsNullOrEmpty(testRoot))
                            {
                                testFrameworkName = testFramework;
                                var testRootPath = Path.GetFullPath(Path.Combine(proj.DirectoryPath, testRoot));
                                fileAbsolutePath = CommonUtils.GetAbsoluteFilePath(projectHome, item.EvaluatedInclude);
                                if (!fileAbsolutePath.StartsWith(testRootPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                //Check to see if this is a TestCase
                                testFrameworkName = item.GetMetadataValue("TestFramework");
                                if (!TestFrameworks.TestFramework.IsValidTestFramework(testFrameworkName))
                                {
                                    continue;
                                }
                                fileAbsolutePath = CommonUtils.GetAbsoluteFilePath(projectHome, item.EvaluatedInclude);
                            }

                            var typeScriptTest = TypeScript.TypeScriptHelpers.IsTypeScriptFile(fileAbsolutePath);
                            if (typeScriptTest)
                            {
                                fileAbsolutePath = TypeScript.TypeScriptHelpers.GetTypeScriptBackedJavaScriptFile(proj, fileAbsolutePath);
                            }
                            else if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(fileAbsolutePath), ".js"))
                            {
                                continue;
                            }

                            if (!testItems.TryGetValue(testFrameworkName, out var fileList))
                            {
                                fileList = new HashSet<TestFileEntry>(TestFileEntryComparer.Instance);
                                testItems.Add(testFrameworkName, fileList);
                            }
                            fileList.Add(new TestFileEntry(fileAbsolutePath, typeScriptTest));
                        }

                        this.DiscoverTests(testItems, proj, discoverySink, logger);
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

        private void DiscoverTests(Dictionary<string, HashSet<TestFileEntry>> testItems, MSBuild.Project proj, ITestCaseDiscoverySink discoverySink, IMessageLogger logger)
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
                logger.SendMessage(TestMessageLevel.Error, "Node.exe was not found. Please install Node.js before running tests.");
                return;
            }

            var testCount = 0;
            foreach (var testFx in testItems.Keys)
            {
                var testFramework = FrameworkDiscover.Intance.Get(testFx);
                if (testFramework == null)
                {
                    logger.SendMessage(TestMessageLevel.Warning, $"Ignoring unsupported test framework '{testFx}'.");
                    continue;
                }

                var fileList = testItems[testFx];
                var files = string.Join(";", fileList.Select(p => p.File));
                logger.SendMessage(TestMessageLevel.Informational, string.Format(CultureInfo.CurrentCulture, "Processing: {0}", files));

                var discoveredTestCases = testFramework.FindTests(fileList.Select(p => p.File), nodeExePath, logger, projectRoot: projectHome);
                testCount += discoveredTestCases.Count;
                foreach (var discoveredTest in discoveredTestCases)
                {
                    var qualifiedName = discoveredTest.FullyQualifiedName;
                    const string indent = "  ";
                    logger.SendMessage(TestMessageLevel.Informational, $"{indent}Creating TestCase:{qualifiedName}");
                    //figure out the test source info such as line number
                    var filePath = discoveredTest.ModulePath;
                    var entry = fileList.First(p => StringComparer.OrdinalIgnoreCase.Equals(p.File, filePath));
                    FunctionInformation fi = null;
                    if (entry.IsTypeScriptTest)
                    {
                        fi = SourceMapper.MaybeMap(new FunctionInformation(string.Empty,
                                                                           discoveredTest.TestName,
                                                                           discoveredTest.SourceLine,
                                                                           entry.File));
                    }

                    var testcase = new TestCase(qualifiedName, NodejsConstants.ExecutorUri, projSource)
                    {
                        CodeFilePath = fi?.Filename ?? filePath,
                        LineNumber = fi?.LineNumber ?? discoveredTest.SourceLine,
                        DisplayName = discoveredTest.TestName
                    };

                    testcase.SetPropertyValue(JavaScriptTestCaseProperties.TestFramework, testFx);
                    testcase.SetPropertyValue(JavaScriptTestCaseProperties.NodeExePath, nodeExePath);
                    testcase.SetPropertyValue(JavaScriptTestCaseProperties.ProjectRootDir, projectHome);
                    testcase.SetPropertyValue(JavaScriptTestCaseProperties.WorkingDir, projectHome);

                    discoverySink.SendTestCase(testcase);
                }
                logger.SendMessage(TestMessageLevel.Informational, string.Format(CultureInfo.CurrentCulture, "Processing finished for framework '{0}'.", testFx));
            }
            if (testCount == 0)
            {
                logger.SendMessage(TestMessageLevel.Warning, string.Format(CultureInfo.CurrentCulture, "Discovered 0 testcases."));
            }
        }
    }
}
