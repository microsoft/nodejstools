// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks
{
    public sealed class TestFramework
    {
        private readonly string vsixScriptFolder;
        private readonly string findTestsScriptFile;
        private readonly string runTestsScriptFile;

        public TestFramework(string vsixScriptFolder)
        {
            this.vsixScriptFolder = vsixScriptFolder;
            this.Name = Path.GetFileNameWithoutExtension(vsixScriptFolder);
            this.findTestsScriptFile = Path.Combine(Path.GetDirectoryName(vsixScriptFolder), "find_tests.js");
            this.runTestsScriptFile = Path.Combine(Path.GetDirectoryName(vsixScriptFolder), "run_tests.js");
        }

        public string Name { get; }

        public IEnumerable<NodejsTestInfo> FindTests(IEnumerable<string> testFiles,
            string nodeExe,
            IMessageLogger logger,
            string projectRoot)
        {
            var testInfo = string.Empty;
            var discoverResultFile = Path.GetTempFileName();
            try
            {
                var stdout = EvaluateJavaScript(nodeExe, string.Join(";", testFiles), discoverResultFile, logger, projectRoot);
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    var stdoutLines = stdout.Split(new[] { Environment.NewLine },
                        StringSplitOptions.RemoveEmptyEntries).Where(s => s.StartsWith("NTVS_ERROR:")).Select(s => s.Trim().Remove(0, 11));

                    if (stdoutLines != null && stdoutLines.Count() > 0)
                    {
                        foreach (var s in stdoutLines)
                        {
                            logger.SendMessage(TestMessageLevel.Error, s);
                        }
                        //There was an error during detection, return an empty set
                        return Enumerable.Empty<NodejsTestInfo>();
                    }
                }

                for (var i = 0; i < 4; i++)
                {
                    try
                    {
                        testInfo = File.ReadAllText(discoverResultFile);
                        break;
                    }
                    catch (IOException)
                    {
                        //We took an error processing the file.  Wait a few and try again
                        Thread.Sleep(500);
                    }
                }
            }
            finally
            {
#if !DEBUG
                try
                {
                    File.Delete(discoverResultFile);
                }
                catch (Exception)
                {
                    //Unable to delete for some reason
                    //We leave the file behind in this case, its in TEMP so eventually OS will clean up
                }
#endif
            }

            var testCases = new List<NodejsTestInfo>();
            var discoveredTests = JsonConvert.DeserializeObject<List<DiscoveredTest>>(testInfo);
            if (discoveredTests != null)
            {
                foreach (var discoveredTest in discoveredTests)
                {
                    var line = discoveredTest.Line + 1;
                    var column = discoveredTest.Column + 1;
                    var test = new NodejsTestInfo(discoveredTest.File, discoveredTest.Test, this.Name, line, column, projectRoot);
                    testCases.Add(test);
                }
            }
            return testCases;
        }

        public ArgumentsToRunTests GetArgumentsToRunTests(string testName, string testFile, string workingDirectory, string projectRootDir)
        {
            workingDirectory = workingDirectory.TrimEnd('\\');
            projectRootDir = projectRootDir.TrimEnd('\\');
            return new ArgumentsToRunTests(
                this.runTestsScriptFile,
                this.Name,
                testName,
                testFile,
                workingDirectory,
                projectRootDir);
        }

        private static string WrapWithQuotes(string path)
        {
            if (!path.StartsWith("\"", StringComparison.Ordinal) && !path.StartsWith("\'", StringComparison.Ordinal))
            {
                path = "\"" + path + "\"";
            }
            return path;
        }

        /// <summary>
        /// Wrap name of the test in the quotes, to be passed to the command line.
        /// </summary>
        /// <param name="testName">Name of the test to excape for command line usage.</param>
        /// <returns>Name of the test, escaped according to the command line rules.</returns>
        private static string WrapTestNameWithQuotes(string testName)
        {
            return "\"" + testName.Replace("\"", "\\\"") + "\"";
        }

        private string EvaluateJavaScript(string nodeExePath, string testFile, string discoverResultFile, IMessageLogger logger, string workingDirectory)
        {
            workingDirectory = workingDirectory.TrimEnd(new char['\\']);
#if DEBUG
            var arguments = $"{WrapWithQuotes(this.findTestsScriptFile)} {this.Name} {WrapWithQuotes(testFile)} {WrapWithQuotes(discoverResultFile)} {WrapWithQuotes(workingDirectory)}";
            logger.SendMessage(TestMessageLevel.Informational, "Arguments: " + arguments);
#endif

            var stdout = string.Empty;
            try
            {
                var process = ProcessOutput.Run(nodeExePath, new[] { this.findTestsScriptFile, this.Name, testFile, discoverResultFile, workingDirectory }, workingDirectory, env: null, visible: false, redirector: new DiscoveryRedirector(logger));

                process.Wait();
            }
            catch (FileNotFoundException e)
            {
                logger.SendMessage(TestMessageLevel.Error, $"Error starting node.exe.{Environment.NewLine}{e}");
            }

            return stdout;
        }

        public static bool IsValidTestFramework(string testFramework)
        {
            return !string.IsNullOrWhiteSpace(testFramework);
        }

        private sealed class DiscoveredTest
        {
            // fields are set using serializer
#pragma warning disable CS0649
            public string Test;
            public string Suite;
            public string File;
            public int Line;
            public int Column;
#pragma warning restore CS0649
        }

        private sealed class DiscoveryRedirector : Redirector
        {
            private const string NTVS_Error = "NTVS_ERROR:";

            private readonly IMessageLogger logger;

            public DiscoveryRedirector(IMessageLogger logger)
            {
                this.logger = logger;
            }

            public override void WriteErrorLine(string line)
            {
                if (line.StartsWith(NTVS_Error))
                {
                    this.logger.SendMessage(TestMessageLevel.Error, line.Substring(NTVS_Error.Length).TrimStart());
                }
                else
                {
                    this.logger.SendMessage(TestMessageLevel.Error, line);
                }
            }

            public override void WriteLine(string line)
            {
                this.logger.SendMessage(TestMessageLevel.Informational, line);
            }
        }

        public sealed class ArgumentsToRunTests
        {
            public ArgumentsToRunTests(string runTestsScriptFile, string testFramework, string testName, string testFile, string workingDirectory, string projectRootDir)
            {
                this.RunTestsScriptFile = WrapWithQuotes(runTestsScriptFile);
                this.TestFramework = testFramework;
                this.TestName = WrapTestNameWithQuotes(testName);
                this.TestFile = WrapWithQuotes(testFile);
                this.WorkingDirectory = WrapWithQuotes(workingDirectory);
                this.ProjectRootDir = WrapWithQuotes(projectRootDir);
            }

            public readonly string RunTestsScriptFile;
            public readonly string TestFramework;
            public readonly string TestName;
            public readonly string TestFile;
            public readonly string WorkingDirectory;
            public readonly string ProjectRootDir;
        }
    }
}
