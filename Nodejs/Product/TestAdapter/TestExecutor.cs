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
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.TestAdapter {

    class ResultObject {
        public ResultObject() {
            title = String.Empty;
            passed = false;
            stdout = String.Empty;
            stderr = String.Empty;
            time = 0;
        }
        public string title { get; set; }
        public bool passed { get; set; }
        public string stdout { get; set; }
        public string stderr { get; set; }
        public int time { get; set; }
    }

    [ExtensionUri(TestExecutor.ExecutorUriString)]
    class TestExecutor : ITestExecutor {
        public const string ExecutorUriString = "executor://NodejsTestExecutor/v1";
        public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);
        //get from NodeRemoteDebugPortSupplier::PortSupplierId
        private static readonly Guid NodejsRemoteDebugPortSupplierUnsecuredId = new Guid("{9E16F805-5EFC-4CE5-8B67-9AE9B643EF80}");

        private readonly ManualResetEvent _cancelRequested = new ManualResetEvent(false);

        private static readonly char[] _needToBeQuoted = new[] { ' ', '"' };
        private ProcessStartInfo _psi;
        private Process _nodeProcess;
        private object _syncObject = new object();

        public void Cancel() {
            //let us just kill the node process there, rather do it late, because VS engine process 
            //could exit right after this call and our node process will be left running.
            KillNodeProcess();
            _cancelRequested.Set();
        }

        /// <summary>
        /// This is the equivalent of "RunAll" functionality
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
            // May be null, but this is handled by RunTestCase if it matters.
            // No VS instance just means no debugging, but everything else is
            // okay.
            using (var app = VisualStudioApp.FromEnvironmentVariable(NodejsConstants.NodeToolsProcessIdEnvironmentVariable)) {
                // .njsproj file path -> project settings
                var projectToTests = new Dictionary<string, List<TestCase>>();
                var sourceToSettings = new Dictionary<string, NodejsProjectSettings>();
                NodejsProjectSettings settings = null;

                // put tests into dictionary where key is their project working directory
                foreach (var test in receiver.Tests) {
                    if (!sourceToSettings.TryGetValue(test.Source, out settings)) {
                        sourceToSettings[test.Source] = settings = LoadProjectSettings(test.Source);
                    }
                    if (!projectToTests.ContainsKey(settings.WorkingDir)) {
                        projectToTests[settings.WorkingDir] = new List<TestCase>();
                    }
                    projectToTests[settings.WorkingDir].Add(test);
                }

                // where key is the workingDir and value is a list of tests
                foreach (KeyValuePair<string, List<TestCase>> entry in projectToTests) {
                    List<string> args = new List<string>();
                    TestCase firstTest = entry.Value.ElementAt(0);
                    int port = 0;
                    if (runContext.IsBeingDebugged && app != null) {
                        app.GetDTE().Debugger.DetachAll();
                        args.AddRange(GetDebugArgs(settings, out port));
                    }

                    args.AddRange(GetInterpreterArgs(firstTest, entry.Key, settings.ProjectRootDir));

                    // launch node process
                    LaunchNodeProcess(settings.WorkingDir, settings.NodeExePath, args);
                    // Run all test cases in a given project
                    RunTestCases(entry.Value, runContext, frameworkHandle, settings);
                    // dispose node process
                    _nodeProcess.Dispose();
                }
            }
        }

        /// <summary>
        /// This is the equivalent of "Run Selected Tests" functionality.
        /// </summary>
        /// <param name="tests">The list of TestCases selected to run</param>
        /// <param name="runContext">Defines the settings related to the current run</param>
        /// <param name="frameworkHandle">Handle to framework.  Used for recording results</param>
        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle) {
            ValidateArg.NotNull(tests, "tests");
            ValidateArg.NotNull(runContext, "runContext");
            ValidateArg.NotNull(frameworkHandle, "frameworkHandle");
            _cancelRequested.Reset();

            TestCase firstTest = tests.First();
            NodejsProjectSettings settings = LoadProjectSettings(firstTest.Source);
            List<string> args = new List<string>();
            args.AddRange(GetInterpreterArgs(firstTest, settings.WorkingDir, settings.ProjectRootDir));

            LaunchNodeProcess(settings.WorkingDir, settings.NodeExePath, args);
            // Run all test cases selected
            RunTestCases(tests, runContext, frameworkHandle, settings);
            _nodeProcess.Dispose();
        }

        private void RunTestCases(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle, NodejsProjectSettings settings) {
            // May be null, but this is handled by RunTestCase if it matters.
            // No VS instance just means no debugging, but everything else is
            // okay.
            using (var app = VisualStudioApp.FromEnvironmentVariable(NodejsConstants.NodeToolsProcessIdEnvironmentVariable)) {
                int port = 0;
                // .njsproj file path -> project settings
                var sourceToSettings = new Dictionary<string, NodejsProjectSettings>();
                TestCaseObject testObject;
                List<TestCaseObject> testObjects = new List<TestCaseObject>();
                foreach (var test in tests) {
                    if (_cancelRequested.WaitOne(0)) {
                        break;
                    }
                    frameworkHandle.RecordStart(test);

                    if (settings == null) {
                        frameworkHandle.SendMessage(
                            TestMessageLevel.Error,
                            "Unable to determine interpreter to use for " + test.Source);
                        frameworkHandle.RecordEnd(test, TestOutcome.Failed);
                    }

                    NodejsTestInfo testInfo = new NodejsTestInfo(test.FullyQualifiedName);
                    List<string> args = new List<string>();
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
                    testObject = new TestCaseObject(args[1], args[2], args[3], args[4], args[5]);
                    testObjects.Add(testObject);
                }

                if (!_nodeProcess.HasExited) {
                    // send testcases to run_tests.js
                    _nodeProcess.StandardInput.WriteLine(JsonConvert.SerializeObject(testObjects));
                    _nodeProcess.StandardInput.Close();
                    _nodeProcess.WaitForExit();
                }

                if (runContext.IsBeingDebugged && app != null) {
                    try {
                        //the '#ping=0' is a special flag to tell VS node debugger not to connect to the port,
                        //because a connection carries the consequence of setting off --debug-brk, and breakpoints will be missed.
                        string qualifierUri = string.Format("tcp://localhost:{0}#ping=0", port);
                        //while (!app.AttachToProcess(_nodeProcess, NodejsRemoteDebugPortSupplierUnsecuredId, qualifierUri)) {
                        //    if (_nodeProcess.Wait(TimeSpan.FromMilliseconds(500))) {
                        //        break;
                        //    }
                        //}
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
            var results = GetTestResultFromProcess(_nodeProcess.StandardOutput);

            bool runCancelled = _cancelRequested.WaitOne(0);

            if (results != null) {
                RecordEnd(frameworkHandle, tests, results);
            }
            else {
                frameworkHandle.SendMessage(TestMessageLevel.Error, "Failed to obtain results");
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

        private List<ResultObject> ParseTestResult(string line) {
            List<ResultObject> jsonResults = null;
            try {
                jsonResults = JsonConvert.DeserializeObject<List<ResultObject>>(line);
            } catch (Exception) { }
            return jsonResults;
        }

        private List<ResultObject> GetTestResultFromProcess(StreamReader sr) {
            List<ResultObject> results = null;
            while (sr.Peek() >= 0) {
                results = ParseTestResult(sr.ReadLine());
                if (results == null) {
                    continue;
                }
                break;
            }
            return results;
        }

        private void LaunchNodeProcess(string workingDir, string nodeExePath, List<string> args) {
            _psi = new ProcessStartInfo("cmd.exe") {
                Arguments = string.Format(@"/S /C pushd {0} & {1} {2}",
                ProcessOutput.QuoteSingleArgument(workingDir),
                ProcessOutput.QuoteSingleArgument(nodeExePath),
                ProcessOutput.GetArguments(args, true)),
                CreateNoWindow = true,
                UseShellExecute = false
            };
            _psi.RedirectStandardInput = true;
            _psi.RedirectStandardOutput = true;
            _nodeProcess = Process.Start(_psi);
        }

        private NodejsProjectSettings LoadProjectSettings(string projectFile) {
            var buildEngine = new MSBuild.ProjectCollection();
            var proj = buildEngine.LoadProject(projectFile);

            var projectRootDir = Path.GetFullPath(Path.Combine(proj.DirectoryPath, proj.GetPropertyValue(CommonConstants.ProjectHome) ?? "."));

            NodejsProjectSettings projSettings = new NodejsProjectSettings();

            projSettings.ProjectRootDir = projectRootDir;

            projSettings.WorkingDir = Path.GetFullPath(Path.Combine(projectRootDir, proj.GetPropertyValue(CommonConstants.WorkingDirectory) ?? "."));

            projSettings.NodeExePath =
                Nodejs.GetAbsoluteNodeExePath(
                    projectRootDir,
                    proj.GetPropertyValue(NodejsConstants.NodeExePath));

            return projSettings;
        }

        private static void RecordEnd(IFrameworkHandle frameworkHandle, IEnumerable<TestCase> tests, List<ResultObject> results) {
            if (tests.Count() == results.Count()) {
                TestResult result;
                foreach(var res in results) {
                    var test = tests.Where(n => n.DisplayName == res.title);
                    if(test.Count() == 1) {
                        //frameworkHandle.RecordStart(test.First());
                        result = new TestResult(test.First());
                        result.Outcome = res.passed ? TestOutcome.Passed : TestOutcome.Failed;
                        result.Duration = new TimeSpan(0, 0, 0, 0, res.time);
                        result.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, res.stdout));
                        result.Messages.Add(new TestResultMessage(TestResultMessage.StandardErrorCategory, res.stderr));
                        result.Messages.Add(new TestResultMessage(TestResultMessage.AdditionalInfoCategory, res.stderr));
                        frameworkHandle.RecordResult(result);
                        frameworkHandle.RecordEnd(test.First(), result.Outcome);
                    }
                }
            }
        }
    }
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

class TestCaseObject {
    public TestCaseObject() {
        framework = String.Empty;
        testName = String.Empty;
        testFile = String.Empty;
        workingFolder = String.Empty;
        projectFolder = String.Empty;
    }

    public TestCaseObject(string framework, string testName, string testFile, string workingFolder, string projectFolder) {
        this.framework = framework;
        this.testName = testName;
        this.testFile = testFile;
        this.workingFolder = workingFolder;
        this.projectFolder = projectFolder;
    }
    public string framework { get; set; }
    public string testName { get; set; }
    public string testFile { get; set; }
    public string workingFolder { get; set; }
    public string projectFolder { get; set; }

}