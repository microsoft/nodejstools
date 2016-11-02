//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using MSBuild = Microsoft.Build.Evaluation;
using System.Globalization;

namespace Microsoft.NodejsTools.TestAdapter {
    [ExtensionUri(TestExecutor.ExecutorUriString)]
    class TestExecutor : ITestExecutor {
        public const string ExecutorUriString = "executor://NodejsTestExecutor/v1";
        public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);
        //get from NodeRemoteDebugPortSupplier::PortSupplierId
        private static readonly Guid NodejsRemoteDebugPortSupplierUnsecuredId = new Guid("{9E16F805-5EFC-4CE5-8B67-9AE9B643EF80}");

        private readonly ManualResetEvent _cancelRequested = new ManualResetEvent(false);

        private ProcessOutput _nodeProcess;
        private object _syncObject = new object();

        public void Cancel() {
            //let us just kill the node process there, rather do it late, because VS engine process 
            //could exit right after this call and our node process will be left running.
            KillNodeProcess();
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
            using (var app = VisualStudioApp.FromEnvironmentVariable(NodejsConstants.NodeToolsProcessIdEnvironmentVariable)) {
                // .njsproj file path -> project settings
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

        private void KillNodeProcess() {
            lock (_syncObject) {
                if (_nodeProcess != null) {
                    _nodeProcess.Kill();
                }
            }
        }
        private static int GetFreePort() {
            return Enumerable.Range(new Random().Next(49152, 65536), 60000).Except(
                from connection in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                select connection.LocalEndPoint.Port
            ).First();
        }

        private IEnumerable<string> GetInterpreterArgs(TestCase test, string workingDir, string projectRootDir) {
            TestFrameworks.NodejsTestInfo testInfo = new TestFrameworks.NodejsTestInfo(test.FullyQualifiedName);
            TestFrameworks.FrameworkDiscover discover = new TestFrameworks.FrameworkDiscover();
            return discover.Get(testInfo.TestFramework).ArgumentsToRunTests(testInfo.TestName, testInfo.ModulePath, workingDir, projectRootDir);
        }

        private static IEnumerable<string> GetDebugArgs(NodejsProjectSettings settings, out int port) {
            port = GetFreePort();

            return new[] {
                "--debug-brk=" + port.ToString()
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

            NodejsTestInfo testInfo = new NodejsTestInfo(test.FullyQualifiedName);
            List<string> args = new List<string>();
            int port = 0;
            if (runContext.IsBeingDebugged && app != null) {
                app.GetDTE().Debugger.DetachAll();
                args.AddRange(GetDebugArgs(settings, out port));
            }

            var workingDir = Path.GetDirectoryName(CommonUtils.GetAbsoluteFilePath(settings.WorkingDir, testInfo.ModulePath));
            args.AddRange(GetInterpreterArgs(test, workingDir, settings.ProjectRootDir));

            //Debug.Fail("attach debugger");
            if (!File.Exists(settings.NodeExePath)) {
                frameworkHandle.SendMessage(TestMessageLevel.Error, "Interpreter path does not exist: " + settings.NodeExePath);
                return;
            }
            lock (_syncObject) {
                _nodeProcess = ProcessOutput.Run(
                                        settings.NodeExePath,
                                        args,
                                        workingDir,
                                        null,
                                        false,
                                        null,
                                        false);

#if DEBUG
                frameworkHandle.SendMessage(TestMessageLevel.Informational, "cd " + workingDir);
                frameworkHandle.SendMessage(TestMessageLevel.Informational, _nodeProcess.Arguments);
#endif

                _nodeProcess.Wait(TimeSpan.FromMilliseconds(500));
                if (runContext.IsBeingDebugged && app != null) {
                    try {
                        //the '#ping=0' is a special flag to tell VS node debugger not to connect to the port,
                        //because a connection carries the consequence of setting off --debug-brk, and breakpoints will be missed.
                        string qualifierUri = string.Format(CultureInfo.InvariantCulture, "tcp://localhost:{0}#ping=0", port);
                        while (!app.AttachToProcess(_nodeProcess, NodejsRemoteDebugPortSupplierUnsecuredId, qualifierUri)) {
                            if (_nodeProcess.Wait(TimeSpan.FromMilliseconds(500))) {
                                break;
                            }
                        }
#if DEBUG
                    } catch (COMException ex) {
                        frameworkHandle.SendMessage(TestMessageLevel.Error, "Error occurred connecting to debuggee.");
                        frameworkHandle.SendMessage(TestMessageLevel.Error, ex.ToString());
                        KillNodeProcess();
                    }
#else
                    } catch (COMException) {
                        frameworkHandle.SendMessage(TestMessageLevel.Error, "Error occurred connecting to debuggee.");
                        KillNodeProcess();
                    }
#endif
                }
            }

            WaitHandle.WaitAll(new WaitHandle[] { _nodeProcess.WaitHandle });

            bool runCancelled = _cancelRequested.WaitOne(0);
            RecordEnd(frameworkHandle, test, testResult,
                string.Join(Environment.NewLine, _nodeProcess.StandardOutputLines),
                string.Join(Environment.NewLine, _nodeProcess.StandardErrorLines),
                (!runCancelled && _nodeProcess.ExitCode == 0) ? TestOutcome.Passed : TestOutcome.Failed);
            _nodeProcess.Dispose();
        }

        private NodejsProjectSettings LoadProjectSettings(string projectFile) {
            var env = new Dictionary<string, string>();
#if DEV15
            var root = Environment.GetEnvironmentVariable(NodejsConstants.NodeToolsVsInstallRootEnvironmentVariable);
            if (!string.IsNullOrEmpty(root)) {
                env["VsInstallRoot"] = root;
                env["MSBuildExtensionsPath32"] = Path.Combine(root, "MSBuild");
            }
#endif
            var buildEngine = new MSBuild.ProjectCollection(env);
            var proj = buildEngine.LoadProject(projectFile);

            var projectRootDir = Path.GetFullPath(Path.Combine(proj.DirectoryPath, proj.GetPropertyValue(CommonConstants.ProjectHome) ?? "."));

            return new NodejsProjectSettings() {
                ProjectRootDir = projectRootDir,

                WorkingDir = Path.GetFullPath(Path.Combine(projectRootDir, proj.GetPropertyValue(CommonConstants.WorkingDirectory) ?? ".")),

                NodeExePath = Nodejs.GetAbsoluteNodeExePath(
                    projectRootDir,
                    proj.GetPropertyValue(NodeProjectProperty.NodeExePath))
            };
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
            public string ProjectRootDir { get; set; }
        }
    }
}
