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
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.NodejsTools;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.NodejsTools.TestAdapter {
    [ExtensionUri(TestExecutor.ExecutorUriString)]
    class TestExecutor : ITestExecutor {
        public const string ExecutorUriString = "executor://NodejsTestExecutor/v1";
        public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);
        private static readonly Guid NodejsRemoteDebugPortSupplierUnsecuredId = new Guid("{2AF68ED9-6A8A-4210-9B4D-92DDEBAB8CCC}");
        //private static readonly string TestLauncherPath = "";//NodejsToolsInstallPath.GetFile("visualstudio_py_testlauncher.py");
                
        private readonly ManualResetEvent _cancelRequested = new ManualResetEvent(false);

        public void Cancel() {
            _cancelRequested.Set();
        }

        /// <summary>
        /// This is the equivallent of "RunAll" functionality
        /// </summary>
        /// <param name="sources">Refers to the list of test sources passed to the test adapter from the client.  (Client could be VS or command line)</param>
        /// <param name="runContext">Defines the settings related to the current run</param>
        /// <param name="frameworkHandle">Handle to framework.  Used for recording results</param>
        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle) {
            ValidateArg.NotNull(sources, "sources");
            ValidateArg.NotNull(runContext, "runContext");
            ValidateArg.NotNull(frameworkHandle, "frameworkHandle");

            _cancelRequested.Reset();

            var receiver = new TestReceiver();
            var discoverer = new TestDiscoverer();
            discoverer.DiscoverTests(sources, null, frameworkHandle, receiver);

            if (_cancelRequested.WaitOne(0)) {
                return;
            }

            RunTestCases(receiver.Tests, runContext, frameworkHandle);
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle) {
            ValidateArg.NotNull(tests, "tests");
            ValidateArg.NotNull(runContext, "runContext");
            ValidateArg.NotNull(frameworkHandle, "frameworkHandle");

            _cancelRequested.Reset();

            RunTestCases(tests, runContext, frameworkHandle);
        }

        private void RunTestCases(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle) {
            // May be null, but this is handled by RunTestCase if it matters.
            // No VS instance just means no debugging, but everything else is
            // okay.
            using (var app = VisualStudioApp.FromCommandLineArgs(Environment.GetCommandLineArgs())) {
                // .pyproj file path -> project settings
                var sourceToSettings = new Dictionary<string, NodejsProjectSettings>();

                foreach (var test in tests) {
                    if (_cancelRequested.WaitOne(0)) {
                        break;
                    }

                    try {
                        RunTestCase(app, frameworkHandle, runContext, test, sourceToSettings);
                    } catch (Exception ex) {
                        frameworkHandle.SendMessage(TestMessageLevel.Error, ex.ToString());
                    }
                }
            }
        }

        private static int GetFreePort() {
            return Enumerable.Range(new Random().Next(49152, 65536), 60000).Except(
                from connection in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                select connection.LocalEndPoint.Port
            ).First();
        }

        private static string GetWorkingDirectory(TestCase test, NodejsProjectSettings settings) {
            string testFile;
            string testClass;
            string testMethod;
            TestDiscoverer.ParseFullyQualifiedTestName(test.FullyQualifiedName, out testFile, out testClass, out testMethod);

            return Path.GetDirectoryName(CommonUtils.GetAbsoluteFilePath(settings.WorkingDir, testFile));
        }

        private static IEnumerable<string> GetInterpreterArgs(TestCase test) {
            string testFile;
            string testClass;
            string testMethod;
            TestDiscoverer.ParseFullyQualifiedTestName(test.FullyQualifiedName, out testFile, out testClass, out testMethod);

            var moduleName = Path.GetFileNameWithoutExtension(testFile);

            return new[] { 
                "-e",                
                String.Format("var testCase = require('{0}'); testCase['{1}']();", testFile.Replace("\\","\\\\"), testMethod)
            };
        }

        private static IEnumerable<string> GetDebugArgs(NodejsProjectSettings settings, out string secret, out int port) {
            var secretBuffer = new byte[24];
            RandomNumberGenerator.Create().GetNonZeroBytes(secretBuffer);
            secret = Convert.ToBase64String(secretBuffer);

            port = GetFreePort();

            return new[] {
                "--debug",
                "-p", port.ToString()
            };
        }

        private void RunTestCase(VisualStudioApp app, IFrameworkHandle frameworkHandle, IRunContext runContext, TestCase test, Dictionary<string, NodejsProjectSettings> sourceToSettings) {
            var testResult = new TestResult(test);
            frameworkHandle.RecordStart(test);
            testResult.StartTime = DateTimeOffset.Now;

            NodejsProjectSettings settings;
            if (!sourceToSettings.TryGetValue(test.Source, out settings)) {
                sourceToSettings[test.Source] = settings = LoadProjectSettings(test.Source);
            }
            if (settings == null) {
                frameworkHandle.SendMessage(
                    TestMessageLevel.Error,
                    "Unable to determine interpreter to use for " + test.Source);
                RecordEnd(
                    frameworkHandle,
                    test,
                    testResult,
                    null,
                    "Unable to determine interpreter to use for " + test.Source,
                    TestOutcome.Failed);
                return;
            }

            var workingDir = GetWorkingDirectory(test, settings);
            var args = GetInterpreterArgs(test);
            var searchPath = settings.SearchPath;

            if (!CommonUtils.IsSameDirectory(workingDir, settings.WorkingDir)) {
                if (string.IsNullOrEmpty(searchPath)) {
                    searchPath = settings.WorkingDir;
                } else {
                    searchPath = settings.WorkingDir + ";" + searchPath;
                }
            }

            string secret = null;
            int port = 0;
            if (runContext.IsBeingDebugged && app != null) {
                app.DTE.Debugger.DetachAll();
                args = args.Concat(GetDebugArgs(settings, out secret, out port));
            }

            if (!File.Exists(settings.NodeExePath)) {
                frameworkHandle.SendMessage(TestMessageLevel.Error, "Interpreter path does not exist: " + settings.NodeExePath);
                return;
            }
            using (var proc = ProcessOutput.Run(
                                    settings.NodeExePath,
                                    args,
                                    workingDir,
                                    null,
                                    false,
                                    null)) {
                bool killed = false;

#if DEBUG
                frameworkHandle.SendMessage(TestMessageLevel.Informational, "cd " + workingDir);
                frameworkHandle.SendMessage(TestMessageLevel.Informational, proc.Arguments);
#endif

                proc.Wait(TimeSpan.FromMilliseconds(500));
                if (runContext.IsBeingDebugged && app != null) {
                    try {
                        while (!app.AttachToProcess(proc, NodejsRemoteDebugPortSupplierUnsecuredId, secret, port)) {
                            if (proc.Wait(TimeSpan.FromMilliseconds(500))) {
                                break;
                            }
                        }
#if DEBUG
                    } catch (COMException ex) {
                        frameworkHandle.SendMessage(TestMessageLevel.Error, "Error occurred connecting to debuggee.");
                        frameworkHandle.SendMessage(TestMessageLevel.Error, ex.ToString());
                        proc.Kill();
                        killed = true;
                    }
#else
                    } catch (COMException) {
                        frameworkHandle.SendMessage(TestMessageLevel.Error, "Error occurred connecting to debuggee.");
                        proc.Kill();
                        killed = true;
                    }
#endif
                }

                if (!killed && WaitHandle.WaitAny(new WaitHandle[] { _cancelRequested, proc.WaitHandle }) == 0) {
                    proc.Kill();
                    killed = true;
                } else {
                    RecordEnd(frameworkHandle, test, testResult,
                        string.Join(Environment.NewLine, proc.StandardOutputLines),
                        string.Join(Environment.NewLine, proc.StandardErrorLines),
                        (proc.ExitCode == 0 && !killed) ? TestOutcome.Passed : TestOutcome.Failed);
                }
            }
        }

        private NodejsProjectSettings LoadProjectSettings(string projectFile) {
            var buildEngine = new MSBuild.ProjectCollection();
            var proj = buildEngine.LoadProject(projectFile);
            
            var projectHome = Path.GetFullPath(Path.Combine(proj.DirectoryPath, proj.GetPropertyValue(CommonConstants.ProjectHome) ?? "."));

            var projSettings = new NodejsProjectSettings();
                        
            projSettings.WorkingDir = Path.GetFullPath(Path.Combine(projectHome, proj.GetPropertyValue(CommonConstants.WorkingDirectory) ?? "."));
            
            projSettings.NodeExePath = proj.GetPropertyValue(NodejsConstants.NodeExePath);
            if (string.IsNullOrEmpty(projSettings.NodeExePath)) {
                projSettings.NodeExePath = NodejsTools.Nodejs.NodeExePath;
            }

            return projSettings;
        }

        private static void RecordEnd(IFrameworkHandle frameworkHandle, TestCase test, TestResult result, string stdout, string stderr, TestOutcome outcome) {
            result.EndTime = DateTimeOffset.Now;
            result.Duration = result.EndTime - result.StartTime;
            result.Outcome = outcome;
            result.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, stdout));
            result.Messages.Add(new TestResultMessage(TestResultMessage.StandardErrorCategory, stderr));
            result.Messages.Add(new TestResultMessage(TestResultMessage.AdditionalInfoCategory, stderr));

            frameworkHandle.RecordResult(result);
            frameworkHandle.RecordEnd(test, outcome);
        }

        class DataReceiver {
            public readonly StringBuilder Data = new StringBuilder();

            public void DataReceived(object sender, DataReceivedEventArgs e) {
                if (e.Data != null) {
                    Data.AppendLine(e.Data);
                }
            }
        }

        class TestReceiver : ITestCaseDiscoverySink {
            public List<TestCase> Tests { get; private set; }
            
            public TestReceiver() {
                Tests = new List<TestCase>();
            }
            
            public void SendTestCase(TestCase discoveredTest) {
                Tests.Add(discoveredTest);
            }
        }

        class NodejsProjectSettings {
            public NodejsProjectSettings() {
                NodeExePath = String.Empty; 
                SearchPath = String.Empty;
                WorkingDir = String.Empty;                
            }

            public string NodeExePath { get; set; }
            public string SearchPath { get; set; }
            public string WorkingDir { get; set; }
        }
    }
}
