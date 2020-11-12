// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.NodejsTools.TestAdapter
{
    // We require to put a non-existent file extension to avoid duplicate discovery executions.
    [FileExtension("NTVS_NonExistentFileExtension")]
    public class ProjectTestDiscoverer : ITestDiscoverer
    {
        public virtual string TestDiscovererName => nameof(ProjectTestDiscoverer);

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            AssemblyResolver.SetupHandler();
            this.DiscoverTestsCore(sources, logger, discoverySink);
        }

        /// <summary>
        /// ITestDiscover, Given a list of test sources this method pulls out the test cases
        /// </summary>
        /// <param name="sources">List of test sources passed from client (Client can be VS or command line)</param>
        /// <param name="logger">Used to relay messages to registered loggers</param>
        /// <param name="discoverySink">Callback used to notify client upon discovery of test cases</param>
        private void DiscoverTestsCore(IEnumerable<string> sources, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
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

                    FrameworkDiscoverer frameworkDiscoverer = null;
                    foreach (var proj in buildEngine.LoadedProjects)
                    {
                        var projectHome = Path.GetFullPath(Path.Combine(proj.DirectoryPath, "."));

                        var testItems = TestFrameworkFactory.GetTestItems(projectHome, proj);

                        if (testItems.Any())
                        {
                            frameworkDiscoverer = frameworkDiscoverer ?? new FrameworkDiscoverer();

                            var nodeExePath = Nodejs.GetAbsoluteNodeExePath(projectHome, proj.GetPropertyValue(NodeProjectProperty.NodeExePath));
                            if (string.IsNullOrEmpty(nodeExePath))
                            {
                                // if nothing specified in the project fallback to environment
                                nodeExePath = Nodejs.GetPathToNodeExecutableFromEnvironment();
                            }

                            this.DiscoverTests(testItems, frameworkDiscoverer, discoverySink, logger, nodeExePath, proj.FullPath);
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

        private void DiscoverTests(Dictionary<string, HashSet<string>> testItems, FrameworkDiscoverer frameworkDiscoverer, ITestCaseDiscoverySink discoverySink, IMessageLogger logger, string nodeExePath, string projectFullPath)
        {
            var discoverWorker = new TestDiscovererWorker(projectFullPath, nodeExePath);

            foreach (var testFx in testItems.Keys)
            {
                var testFramework = frameworkDiscoverer.GetFramework(testFx);
                if (testFramework == null)
                {
                    logger.SendMessage(TestMessageLevel.Warning, $"Ignoring unsupported test framework '{testFx}'.");
                    continue;
                }

                var fileList = testItems[testFx];

                discoverWorker.DiscoverTests(fileList, testFramework, logger, discoverySink, this.TestDiscovererName);
            }
        }
    }
}
