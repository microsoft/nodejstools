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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.NodejsTools.TestAdapter {
    [FileExtension(".njsproj")]
    [DefaultExecutorUri(TestExecutor.ExecutorUriString)]
    class TestDiscoverer : ITestDiscoverer {

        public TestDiscoverer() { }

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
                        var projSource = ((MSBuild.Project)proj).FullPath;

                        var nodeExePath = proj.GetPropertyValue(NodejsConstants.NodeExePath);
                        if (string.IsNullOrEmpty(nodeExePath)) {
                            nodeExePath = NodejsTools.Nodejs.NodeExePath;
                        }

                        if (!File.Exists(nodeExePath)) {
                            logger.SendMessage(TestMessageLevel.Error, String.Format("Node.exe was not found.  Please install Node.js before running tests."));
                            continue;
                        }

                        // Provide all files to the test analyzer
                        foreach (var item in proj.Items.Where(item => item.ItemType == "Compile" || item.ItemType == "TypeScriptCompile")) {

                            //Check to see if this is a TestCase
                            string value = item.GetMetadataValue("TestFramework");
                            if (!TestContainerDiscoverer.IsValidTestFramework(value)) {
                                continue;
                            }

                            string fileAbsolutePath = CommonUtils.GetAbsoluteFilePath(projectHome, item.EvaluatedInclude);
                            string testFileAbsolutePath = fileAbsolutePath;

                            if (Path.GetExtension(fileAbsolutePath).Equals(".ts", StringComparison.OrdinalIgnoreCase)) {
                                //We're dealing with TypeScript
                                //Switch to the underlying js file
                                fileAbsolutePath = fileAbsolutePath.Substring(0, fileAbsolutePath.Length - 3) + ".js";
                            } else if (!Path.GetExtension(fileAbsolutePath).Equals(".js", StringComparison.OrdinalIgnoreCase)) {
                                continue;
                            }

                            logger.SendMessage(TestMessageLevel.Informational, String.Format("Processing {0}", fileAbsolutePath));

                            string testCases = String.Empty;
                            string tempFile = Path.GetTempFileName();

                            try {
                                EvaluateJavaScript(nodeExePath, String.Format("var fs = require('fs'); var stream = fs.createWriteStream('{0}'); var testCase = require('{1}'); for(var x in testCase) {{ stream.write(x + '\\r\\n'); }} stream.end();", tempFile.Replace("\\", "\\\\"), fileAbsolutePath.Replace("\\", "\\\\")), logger);
                                for (int i = 0; i < 4; i++) {
                                    try {
                                        testCases = File.ReadAllText(tempFile);
                                        break;
                                    } catch (IOException) {
                                        //We took an error processing the file.  Wait a few and try again
                                        Thread.Sleep(500);
                                    }
                                }
                            } finally {
                                try {
                                    File.Delete(tempFile);
                                } catch (Exception) { //
                                    //Unable to delete for some reason
                                    //  We leave the file behind in this case, its in TEMP so eventually OS will clean up
                                }
                            }


                            if (String.IsNullOrEmpty(testCases)) {
                                logger.SendMessage(TestMessageLevel.Warning, String.Format("Discovered 0 testcases in: {0}", fileAbsolutePath));
                            } else {
                                foreach (var testFunction in testCases.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)) {
                                    //TestCase Qualified name format
                                    //Path::ModuleName::TestName
                                    string testName = MakeFullyQualifiedTestName(fileAbsolutePath, Path.GetFileNameWithoutExtension(fileAbsolutePath), testFunction);

                                    logger.SendMessage(TestMessageLevel.Informational, String.Format("Creating TestCase:{0}", testName));
                                    var testCase = new TestCase(testName, TestExecutor.ExecutorUri, projSource) {
                                        CodeFilePath = testFileAbsolutePath,
                                        LineNumber = 0,
                                        DisplayName = testFunction
                                    };

                                    discoverySink.SendTestCase(testCase);
                                }
                            }
                            logger.SendMessage(TestMessageLevel.Informational, String.Format("Processing Finished {0}", fileAbsolutePath));
                        }
                    }
                } finally {
                    // Disposing buildEngine does not clear the document cache in
                    // VS 2013, so manually unload all projects before disposing.
                    buildEngine.UnloadAllProjects();
                }
            }
        }

        internal static string MakeFullyQualifiedTestName(string modulePath, string className, string methodName) {
            return modulePath + "::" + className + "::" + methodName;
        }

        internal static void ParseFullyQualifiedTestName(string fullyQualifiedName, out string modulePath, out string className, out string methodName) {
            string[] parts = fullyQualifiedName.Split(new string[] { "::" }, StringSplitOptions.None);
            Debug.Assert(parts.Length == 3);
            modulePath = parts[0];
            className = parts[1];
            methodName = parts[2];
        }

        internal static MSBuild.Project LoadProject(MSBuild.ProjectCollection buildEngine, string fullProjectPath) {
            var buildProject = buildEngine.GetLoadedProjects(fullProjectPath).FirstOrDefault();

            if (buildProject != null) {
                buildEngine.UnloadProject(buildProject);
            }
            return buildEngine.LoadProject(fullProjectPath);
        }

        private string EvaluateJavaScript(string nodeExePath, string code, IMessageLogger logger) {
#if DEBUG
            logger.SendMessage(TestMessageLevel.Informational, String.Format("  Code {0}", code));
#endif
            string arguments = "-e \"" + code + "\"";

            var processStartInfo = new ProcessStartInfo(nodeExePath, arguments);
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;

            string stdout = "";
            try {
                using (var process = Process.Start(processStartInfo)) {

                    process.EnableRaisingEvents = true;
                    process.OutputDataReceived += (sender, args) => {
                        stdout += args.Data + Environment.NewLine;

                    };
                    process.ErrorDataReceived += (sender, args) => {
                        stdout += args.Data + Environment.NewLine;

                    };
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();

                    process.WaitForExit();
#if DEBUG
                    logger.SendMessage(TestMessageLevel.Informational, String.Format("  Process exited: {0}", process.ExitCode));
#endif
                }
#if DEBUG
                logger.SendMessage(TestMessageLevel.Informational, String.Format("  StdOut:{0}", stdout));
#endif
            } catch (FileNotFoundException e) {
                logger.SendMessage(TestMessageLevel.Error, String.Format("Error starting node.exe.\n {0}", e));
            }

            return stdout;
        }
    }
}
