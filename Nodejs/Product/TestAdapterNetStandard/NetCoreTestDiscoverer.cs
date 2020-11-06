// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.NodejsTools.TestFrameworks;
using Microsoft.NodejsTools.TypeScript;
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
            // The issue we encountered is that the msbuild implementation was not able to 
            // locate the SDK targets/props files. See: https://github.com/Microsoft/msbuild/issues/3434
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
                    var projectHome = Path.GetDirectoryName(projectFile);

                    // Prioritize configuration files over manually setup tests files.
                    var testItems = this.GetConfigItems(projectHome);
                    if (!testItems.Any())
                    {
                        var testFramework = propertyGroup.Descendants(NodeProjectProperty.TestFramework).FirstOrDefault()?.Value;
                        var testRoot = propertyGroup.Descendants(NodeProjectProperty.TestRoot).FirstOrDefault()?.Value;
                        var outDir = propertyGroup.Descendants(NodeProjectProperty.TypeScriptOutDir).FirstOrDefault()?.Value ?? "";

                        if (string.IsNullOrEmpty(testRoot) || string.IsNullOrEmpty(testFramework))
                        {
                            logger.SendMessage(TestMessageLevel.Warning, $"No TestRoot or TestFramework specified for '{Path.GetFileName(projectFile)}'.");
                            continue;
                        }

                        var testFolder = Path.Combine(projectHome, testRoot);

                        if (!Directory.Exists(testFolder))
                        {
                            logger.SendMessage(TestMessageLevel.Warning, $"Test folder path '{testFolder}' doesn't exist.");
                            continue;
                        }

                        testItems = this.GetTestItems(testFolder, testFramework, outDir);
                    }

                    if (testItems.Any())
                    {
                        var nodeExePath = Nodejs.GetAbsoluteNodeExePath(projectHome, propertyGroup.Descendants(NodeProjectProperty.NodeExePath).FirstOrDefault()?.Value);
                        if (string.IsNullOrEmpty(nodeExePath))
                        {
                            // if nothing specified in the project fallback to environment
                            nodeExePath = Nodejs.GetPathToNodeExecutableFromEnvironment();
                        }

                        this.DiscoverTests(testItems, frameworkDiscoverer, discoverySink, logger, nodeExePath, projectFile);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.SendMessage(TestMessageLevel.Error, ex.Message);
                throw;
            }
        }

        private void DiscoverTests(Dictionary<string, HashSet<string>> testItems, FrameworkDiscoverer frameworkDiscoverer, ITestCaseDiscoverySink discoverySink, IMessageLogger logger, string nodeExePath, string projectFullPath)
        {
            foreach (var testFx in testItems.Keys)
            {
                var testFramework = frameworkDiscoverer.GetFramework(testFx);
                if (testFramework == null)
                {
                    logger.SendMessage(TestMessageLevel.Warning, $"Ignoring unsupported test framework '{testFx}'.");
                    continue;
                }

                var fileList = testItems[testFx];

                var discoverWorker = new TestDiscovererWorker(projectFullPath, NodejsConstants.ExecutorUri, nodeExePath);
                discoverWorker.DiscoverTests(fileList, testFramework, logger, discoverySink);
            }
        }

        private Dictionary<string, HashSet<string>> GetConfigItems(string projectRoot)
        {
            var files = Directory.EnumerateFiles(projectRoot, "angular.json", SearchOption.AllDirectories)
                .Where(x => !x.Contains("\\node_modules\\"));
            if (files.Any())
            {
                return new Dictionary<string, HashSet<string>>()
                {
                    {
                        TestFrameworkDirectories.AngularFrameworkName,
                        new HashSet<string>(files)
                    }
                };
            }

            return default;
        }

        private Dictionary<string, HashSet<string>> GetTestItems(string projectRoot, string testFramework, string outDir)
        {
            // If we find ts or tsx files, get the JS file and return.
            var files = Directory.EnumerateFiles(projectRoot, "*.ts?", SearchOption.AllDirectories)
                .Where(x => !x.Contains("\\node_modules\\"));
            if (files.Any())
            {
                var testFilePaths = files
                    .Where(x => TypeScriptHelpers.IsTypeScriptFile(x))
                    .Select(x => TypeScriptHelpers.GetTypeScriptBackedJavaScriptFile(x, projectRoot, outDir));

                return new Dictionary<string, HashSet<string>>()
                {
                    {
                        testFramework,
                        new HashSet<string>(testFilePaths)
                    }
                };
            }

            files = Directory.EnumerateFiles(projectRoot, "*.js", SearchOption.AllDirectories)
                .Where(x => !x.Contains("\\node_modules\\"));
            if (files.Any())
            {
                return new Dictionary<string, HashSet<string>>()
                {
                    {
                        testFramework,
                        new HashSet<string>(files)
                    }
                };
            }

            return default;
        }
    }
}
