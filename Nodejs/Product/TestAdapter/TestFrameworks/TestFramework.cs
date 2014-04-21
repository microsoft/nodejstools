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
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks {
    class TestFramework {
        private string _scriptFolder;
        private string _discoverEntryScriptFile;

        private const string UNITTEST_FILE_TOKEN = "#TEST-FILE#";
        private const string TESTCASE_LIST_FILE_TOKEN = "#TEST-LIST#";
        //private string _discoverScript =
        //    @"var runner = require(process.argv[3]);" +
        //    @"runner.find_tests('" + UNITTEST_FILE_TOKEN + @"'," + @"'" + TESTCASE_LIST_FILE_TOKEN + @"');";
           
        public TestFramework(string scriptFolder) {
            _scriptFolder = scriptFolder;
            Name = Path.GetFileNameWithoutExtension(scriptFolder);
            _discoverEntryScriptFile = Path.Combine(Path.GetDirectoryName(scriptFolder), "discover.js");
        }
        public string Name { get; private set; }
        public string DiscoverTests(string testFile,
            string nodeExe,
            IMessageLogger logger, 
            string workingDirectory) {

            string tests = string.Empty;
            string discoverResultFile = Path.GetTempFileName();
            try {
                EvaluateJavaScript(nodeExe, testFile, discoverResultFile, logger, workingDirectory);
                for (int i = 0; i < 4; i++) {
                    try {
                        tests = File.ReadAllText(discoverResultFile);
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
            return tests;
        }

        private string WrapWithQuot(string path) {
            if (!path.StartsWith("\"") && !path.StartsWith("\'")) {
                path = "\"" + path + "\"";
            }
            return path;
        }

        private string EvaluateJavaScript(string nodeExePath, string testFile, string discoverResultFile, IMessageLogger logger, string workingDirectory) {
            workingDirectory = workingDirectory.TrimEnd(new char['\\']);
            string arguments = WrapWithQuot(_discoverEntryScriptFile)
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
