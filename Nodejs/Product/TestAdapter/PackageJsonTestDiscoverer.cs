// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.NodejsTools.TestFrameworks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Microsoft.NodejsTools.TestAdapter
{
    [FileExtension(".json")]
    [DefaultExecutorUri(NodejsConstants.PackageJsonExecutorUriString)]
    public sealed class PackageJsonTestDiscoverer : ITestDiscoverer
    {
        private FrameworkDiscoverer frameworkDiscoverer;

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            AssemblyResolver.SetupHandler();

            var settings = new UnitTestSettings(discoveryContext.RunSettings);

            this.frameworkDiscoverer = this.frameworkDiscoverer ?? new FrameworkDiscoverer(settings.TestFrameworksLocation);

            foreach (var source in sources)
            {
                // we're only interested in package.json files here.
                if (PackageJsonFactory.IsPackageJsonFile(source))
                {
                    this.DiscoverTestFiles(source, logger, discoverySink);
                }
            }
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

            var testFx = default(TestFramework);

            foreach (var dep in packageJson.AllDependencies)
            {
                testFx = this.frameworkDiscoverer.GetFramework(dep.Name);
                if (testFx != null)
                {
                    break;
                }
            }
            testFx = testFx ?? this.frameworkDiscoverer.GetFramework(TestFrameworkDirectories.ExportRunnerFrameworkName);

            var nodeExePath = Nodejs.GetPathToNodeExecutableFromEnvironment();
            var worker = new TestDiscovererWorker(packageJsonPath, NodejsConstants.PackageJsonExecutorUri, nodeExePath);
            worker.DiscoverTests(testFolderPath, testFx, logger, discoverySink);
        }
    }
}
