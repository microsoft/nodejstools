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
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks {
    class TestFramework {
        private readonly string _vsixScriptFolder;
        private readonly string _findTestsScriptFile;
        private readonly string _runTestsScriptFile;

        public TestFramework(string vsixScriptFolder) {
            _vsixScriptFolder = vsixScriptFolder;
            Name = Path.GetFileNameWithoutExtension(vsixScriptFolder);
            _findTestsScriptFile = Path.Combine(Path.GetDirectoryName(vsixScriptFolder), "find_tests.js");
            _runTestsScriptFile = Path.Combine(Path.GetDirectoryName(vsixScriptFolder), "run_tests.js");
        }

        public string Name { get; private set; }
        public List<NodejsTestInfo> FindTests(string testFile,
            string nodeExe,
            IMessageLogger logger, 
            string workingDirectory) {

            string testNames = string.Empty;
            string discoverResultFile = Path.GetTempFileName();
            try {
                EvaluateJavaScript(nodeExe, testFile, discoverResultFile, logger, workingDirectory);
                for (int i = 0; i < 4; i++) {
                    try {
                        testNames = File.ReadAllText(discoverResultFile);
                        break;
                    } catch (IOException) {
                        //We took an error processing the file.  Wait a few and try again
                        Thread.Sleep(500);
                    }
                }
            } finally {
                try {
                    File.Delete(discoverResultFile);
                } catch (Exception) { //
                    //Unable to delete for some reason
                    //We leave the file behind in this case, its in TEMP so eventually OS will clean up
                }
            }

            List<NodejsTestInfo> testCases = new List<NodejsTestInfo>();
            foreach (var testName in testNames.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)) {
                NodejsTestInfo test = new NodejsTestInfo(testFile, testName, Name);
                testCases.Add(test);
            }
            return testCases;
        }

        public string[] ArgumentsToRunTests(string testName, string testFile, string workingDirectory) {
            workingDirectory = workingDirectory.TrimEnd(new char['\\']);
            return new string[] {
                WrapWithQuot(_runTestsScriptFile),
                Name,
                WrapWithQuot(testName),
                WrapWithQuot(testFile),
                WrapWithQuot(workingDirectory)
            };
        }

        private string WrapWithQuot(string path) {
            if (!path.StartsWith("\"") && !path.StartsWith("\'")) {
                path = "\"" + path + "\"";
            }
            return path;
        }

        private string EvaluateJavaScript(string nodeExePath, string testFile, string discoverResultFile, IMessageLogger logger, string workingDirectory) {
            workingDirectory = workingDirectory.TrimEnd(new char['\\']);
            string arguments = WrapWithQuot(_findTestsScriptFile)
                + " " + Name +
                " " + WrapWithQuot(testFile) +
                " " + WrapWithQuot(discoverResultFile) +
                " " + WrapWithQuot(workingDirectory);

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
