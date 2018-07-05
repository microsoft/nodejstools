// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Microsoft.NodejsTools.TestAdapter
{
    [FileExtension(".dll")]
    [DefaultExecutorUri(NodejsConstants.ExecutorUriString)]
    public sealed class NetCoreTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            // we can ignore the sources argument since this will be a collection of assemblies.
            ValidateArg.NotNull(discoverySink, nameof(discoverySink));
            ValidateArg.NotNull(logger, nameof(logger));

            // extract the project file from the discovery context.
            var unitTestSettings = new UnitTestSettings(discoveryContext.RunSettings);

            if (string.IsNullOrEmpty(unitTestSettings.TestSource))
            {
                // no need to log since the test executor will report 'no tests'
                return;
            }

            if (string.IsNullOrEmpty(unitTestSettings.TestFrameworksLocation))
            {
                logger.SendMessage(TestMessageLevel.Error, "Failed to locate the test frameworks.");
                return;
            }

            sources = new[] { unitTestSettings.TestSource };
            var frameworkDiscoverer = new FrameworkDiscoverer(unitTestSettings.TestFrameworksLocation);

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

                    // we only support project files, e.g. csproj, vbproj, etc.
                    if (!cleanPath.EndsWith("proj", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

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
                    var testItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
                        foreach (var file in Directory.EnumerateFiles(testFolder, "*.js", SearchOption.AllDirectories))
                        {
                            ProcessFiles(file);
                        }
                    }

                    if (testItems.Any())
                    {

                        var testFx = frameworkDiscoverer.Get(testFramework);
                        if (testFx == null)
                        {
                            logger.SendMessage(TestMessageLevel.Warning, $"Ignoring unsupported test framework '{testFramework}'.");
                            return;
                        }

                        var nodeExePath = Nodejs.GetAbsoluteNodeExePath(projectHome, propertyGroup.Descendants(NodeProjectProperty.NodeExePath).FirstOrDefault()?.Value);
                        this.DiscoverTests(testItems, testFx, discoverySink, logger, nodeExePath, projectFile);
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

                        testItems.Add(fileAbsolutePath);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.SendMessage(TestMessageLevel.Error, ex.Message);
                throw;
            }
        }

        private void DiscoverTests(HashSet<string> fileList, TestFramework testFramework, ITestCaseDiscoverySink discoverySink, IMessageLogger logger, string nodeExePath, string projectFullPath)
        {
            if (!File.Exists(nodeExePath))
            {
                logger.SendMessage(TestMessageLevel.Error, "Node.exe was not found. Please install Node.js before running tests.");
                return;
            }

            var discoverWorker = new TestDiscovererWorker(projectFullPath, NodejsConstants.ExecutorUri, nodeExePath);
            discoverWorker.DiscoverTests(fileList, testFramework, logger, discoverySink);
        }
    }
}
