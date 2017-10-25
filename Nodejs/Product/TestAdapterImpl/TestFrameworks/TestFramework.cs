// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks
{
    internal class TestFramework
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

        public List<NodejsTestInfo> FindTests(IEnumerable<string> testFiles,
            string nodeExe,
            IMessageLogger logger,
            string workingDirectory)
        {
            var testInfo = string.Empty;
            var discoverResultFile = Path.GetTempFileName();
            try
            {
                var stdout = EvaluateJavaScript(nodeExe, string.Join(";", testFiles), discoverResultFile, logger, workingDirectory);
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
                        return new List<NodejsTestInfo>();
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
                try
                {
                    File.Delete(discoverResultFile);
                }
                catch (Exception)
                {
                    //Unable to delete for some reason
                    //We leave the file behind in this case, its in TEMP so eventually OS will clean up
                }
            }

            var testCases = new List<NodejsTestInfo>();
            var discoveredTests = (List<DiscoveredTest>)JsonConvert.DeserializeObject(testInfo, typeof(List<DiscoveredTest>));
            if (discoveredTests != null)
            {
                foreach (var discoveredTest in discoveredTests)
                {
                    var line = discoveredTest.Line + 1;
                    var column = discoveredTest.Column + 1;
                    var test = new NodejsTestInfo(discoveredTest.File, discoveredTest.Test, this.Name, line, column);
                    testCases.Add(test);
                }
            }
            return testCases;
        }

        public string[] ArgumentsToRunTests(string testName, string testFile, string workingDirectory, string projectRootDir)
        {
            workingDirectory = workingDirectory.TrimEnd('\\');
            projectRootDir = projectRootDir.TrimEnd('\\');
            return new[] {
                WrapWithQuotes(this.runTestsScriptFile),
                this.Name,
                WrapTestNameWithQuotes(testName),
                WrapWithQuotes(testFile),
                WrapWithQuotes(workingDirectory),
                WrapWithQuotes(projectRootDir)
            };
        }

        private string WrapWithQuotes(string path)
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
        private string WrapTestNameWithQuotes(string testName)
        {
            return "\"" + testName.Replace("\"", "\\\"") + "\"";
        }

        private string EvaluateJavaScript(string nodeExePath, string testFile, string discoverResultFile, IMessageLogger logger, string workingDirectory)
        {
            workingDirectory = workingDirectory.TrimEnd(new char['\\']);
            var arguments = WrapWithQuotes(this.findTestsScriptFile)
                + " " + this.Name +
                " " + WrapWithQuotes(testFile) +
                " " + WrapWithQuotes(discoverResultFile) +
                " " + WrapWithQuotes(workingDirectory);

            var processStartInfo = new ProcessStartInfo(nodeExePath, arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var stdout = string.Empty;
            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    process.EnableRaisingEvents = true;
                    process.OutputDataReceived += (sender, args) =>
                    {
                        stdout += args.Data + Environment.NewLine;
                    };
                    process.ErrorDataReceived += (sender, args) =>
                    {
                        stdout += args.Data + Environment.NewLine;
                    };
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();

                    process.WaitForExit();
#if DEBUG
                    logger.SendMessage(TestMessageLevel.Informational, string.Format(CultureInfo.InvariantCulture, "  Process exited: {0}", process.ExitCode));
#endif
                }
#if DEBUG
                logger.SendMessage(TestMessageLevel.Informational, string.Format(CultureInfo.InvariantCulture, "  StdOut:{0}", stdout));
#endif
            }
            catch (FileNotFoundException e)
            {
                logger.SendMessage(TestMessageLevel.Error, string.Format(CultureInfo.InvariantCulture, "Error starting node.exe.\n {0}", e));
            }

            return stdout;
        }

        private class DiscoveredTest
        {
            public string Test { get; set; }
            public string Suite { get; set; }
            public string File { get; set; }
            public int Line { get; set; }
            public int Column { get; set; }
        }
    }
}
