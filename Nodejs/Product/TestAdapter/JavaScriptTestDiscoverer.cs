// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.NodejsTools.SourceMapping;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.NodejsTools.TestAdapter
{
#if NETSTANDARD2_0
    [FileExtension(".dll")]
#else
    [FileExtension(".njsproj"), FileExtension(".csproj"), FileExtension(".vbproj")]
#endif
    [DefaultExecutorUri(NodejsConstants.ExecutorUriString)]
    public partial class JavaScriptTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
#if !NETSTANDARD2_0
            AssemblyResolver.SetupHandler();
            this.DiscoverTestsCore(sources, discoveryContext, logger, discoverySink);
#else
            this.DiscoverTestsCoreNetStandard(sources, logger, discoverySink);
#endif
        }

#if NETSTANDARD2_0
        private void DiscoverTestsCoreNetStandard(IEnumerable<string> sources, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            ValidateArg.NotNull(sources, nameof(sources));
            ValidateArg.NotNull(discoverySink, nameof(discoverySink));
            ValidateArg.NotNull(logger, nameof(logger));

            var projects = new List<(string projectFilePath, IEnumerable<XElement> propertyGroup)>();

            // There's an issue when loading the project using the .NET Core msbuild bits,
            // so we load the xml, and extract the properties we care about.
            // Downside is we only have the raw contents of the XmlElements, i.e. we don't
            // expand any variables.
            try
            {
                foreach (var source in sources)
                {
                    var cleanPath = source.Trim('\'', '"');
                    var project = XDocument.Load(cleanPath);

                    // structure looks like Project/PropertyGroup/JsTestRoot and Project/PropertyGroup/JsTestFramework
                    var propertyGroup = project.Descendants("Project").Descendants("PropertyGroup");

                    projects.Add((cleanPath, propertyGroup));
                }

                foreach (var (projectFile, propertyGroup) in projects)
                {
                    var testFramework = propertyGroup.Descendants(NodeProjectProperty.TestFramework).FirstOrDefault()?.Value;
                    var testRoot = propertyGroup.Descendants(NodeProjectProperty.TestRoot).FirstOrDefault()?.Value;
                    var outDir = propertyGroup.Descendants(NodeProjectProperty.TypeScriptOutDir).FirstOrDefault()?.Value ?? "";

                    if (string.IsNullOrEmpty(testRoot) || string.IsNullOrEmpty(testFramework))
                    {
                        logger.SendMessage(TestMessageLevel.Warning, $"No TestRoot or TestFramework specified for '{Path.GetFileName(projectFile)}'.");
                        continue;
                    }

                    var projectHome = Path.GetDirectoryName(projectFile);
                    var testItems = new Dictionary<string, HashSet<TestFileEntry>>(StringComparer.OrdinalIgnoreCase);
                    var testFolder = Path.Combine(projectHome, testRoot);

                    if (!Directory.Exists(testFolder))
                    {
                        logger.SendMessage(TestMessageLevel.Warning, $"Test folder path '{testFolder}' doesn't exist.");
                        continue;
                    }

                    // grab all files, we try for .ts files first, and only parse the .js files if we don't find any
                    foreach (var file in Directory.EnumerateFiles(testFolder, "*.ts", SearchOption.AllDirectories))
                    {
                        ProcessFiles(file);
                    }

                    if (!testItems.Any())
                    {
                        foreach (var file in Directory.EnumerateFiles(Path.Combine(projectHome, testRoot), "*.ts", SearchOption.AllDirectories))
                        {
                            ProcessFiles(file);
                        }
                    }

                    if (testItems.Any())
                    {
                        var nodeExePath = Nodejs.GetAbsoluteNodeExePath(projectHome, propertyGroup.Descendants(NodeProjectProperty.NodeExePath).FirstOrDefault()?.Value);
                        this.DiscoverTests(testItems, discoverySink, logger, nodeExePath, projectHome, projectFile);
                    }

                    void ProcessFiles(string fileAbsolutePath)
                    {
                        var typeScriptTest = TypeScript.TypeScriptHelpers.IsTypeScriptFile(fileAbsolutePath);
                        if (typeScriptTest)
                        {
                            fileAbsolutePath = TypeScript.TypeScriptHelpers.GetTypeScriptBackedJavaScriptFile(projectHome, outDir, fileAbsolutePath);
                        }
                        else if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(fileAbsolutePath), ".js"))
                        {
                            return;
                        }

                        if (!testItems.TryGetValue(testFramework, out var fileList))
                        {
                            fileList = new HashSet<TestFileEntry>(TestFileEntryComparer.Instance);
                            testItems.Add(testFramework, fileList);
                        }
                        fileList.Add(new TestFileEntry(fileAbsolutePath, typeScriptTest));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.SendMessage(TestMessageLevel.Error, ex.Message);
                throw;
            }
        }
#endif 

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
                        var cleanPath = source.Trim('\'', '"');
                        buildEngine.LoadProject(cleanPath);
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
                                if (!TestFramework.IsValidTestFramework(testFrameworkName))
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

                        if (testItems.Any())
                        {
                            this.DiscoverTests(testItems, proj, discoverySink, logger);
                        }
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
            var result = new List<NodejsTestInfo>();
            var projectHome = Path.GetFullPath(Path.Combine(proj.DirectoryPath, "."));
            var projSource = proj.FullPath;

            var nodeExePath =
                Nodejs.GetAbsoluteNodeExePath(
                    projectHome,
                    proj.GetPropertyValue(NodeProjectProperty.NodeExePath));

            this.DiscoverTests(testItems, discoverySink, logger, nodeExePath, projectHome, projSource);
        }

        private void DiscoverTests(Dictionary<string, HashSet<TestFileEntry>> testItems, ITestCaseDiscoverySink discoverySink, IMessageLogger logger, string nodeExePath, string projectHome, string projectFullPath)
        {
            if (!File.Exists(nodeExePath))
            {
                logger.SendMessage(TestMessageLevel.Error, "Node.exe was not found. Please install Node.js before running tests.");
                return;
            }

            var testCount = 0;
            foreach (var testFx in testItems.Keys)
            {
                var testFramework = FrameworkDiscoverer.Instance.Get(testFx);
                if (testFramework == null)
                {
                    logger.SendMessage(TestMessageLevel.Warning, $"Ignoring unsupported test framework '{testFx}'.");
                    continue;
                }

                var fileList = testItems[testFx];
                var files = string.Join(";", fileList.Select(p => p.FullPath));
                logger.SendMessage(TestMessageLevel.Informational, string.Format(CultureInfo.CurrentCulture, "Processing: {0}", files));

                var discoveredTestCases = testFramework.FindTests(fileList.Select(p => p.FullPath), nodeExePath, logger, projectRoot: projectHome);
                testCount += discoveredTestCases.Count();
                foreach (var discoveredTest in discoveredTestCases)
                {
                    var qualifiedName = discoveredTest.FullyQualifiedName;
                    const string indent = "  ";
                    logger.SendMessage(TestMessageLevel.Informational, $"{indent}Creating TestCase:{qualifiedName}");
                    //figure out the test source info such as line number
                    var filePath = discoveredTest.TestPath;
                    var entry = fileList.First(p => StringComparer.OrdinalIgnoreCase.Equals(p.FullPath, filePath));
                    FunctionInformation fi = null;
                    if (entry.IsTypeScriptTest)
                    {
                        fi = SourceMapper.MaybeMap(new FunctionInformation(string.Empty,
                                                                           discoveredTest.TestName,
                                                                           discoveredTest.SourceLine,
                                                                           entry.FullPath));
                    }

                    var testcase = new TestCase(qualifiedName, NodejsConstants.ExecutorUri, projectFullPath)
                    {
                        CodeFilePath = fi?.Filename ?? filePath,
                        LineNumber = fi?.LineNumber ?? discoveredTest.SourceLine,
                        DisplayName = discoveredTest.TestName
                    };

                    testcase.SetPropertyValue(JavaScriptTestCaseProperties.TestFramework, testFx);
                    testcase.SetPropertyValue(JavaScriptTestCaseProperties.NodeExePath, nodeExePath);
                    testcase.SetPropertyValue(JavaScriptTestCaseProperties.ProjectRootDir, projectHome);
                    testcase.SetPropertyValue(JavaScriptTestCaseProperties.WorkingDir, projectHome);
                    testcase.SetPropertyValue(JavaScriptTestCaseProperties.TestFile, filePath);

                    discoverySink.SendTestCase(testcase);
                }
                logger.SendMessage(TestMessageLevel.Informational, string.Format(CultureInfo.CurrentCulture, "Processing finished for framework '{0}'.", testFx));
            }
        }
    }
}
