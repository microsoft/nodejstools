// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.TestAdapter
{
    internal sealed partial class TestExecutorWorker
    {
        private static readonly Version Node8Version = new Version(8, 0);

        //get from NodeRemoteDebugPortSupplier::PortSupplierId
        private static readonly Guid NodejsRemoteDebugPortSupplierUnsecuredId = new Guid("{9E16F805-5EFC-4CE5-8B67-9AE9B643EF80}");

        private readonly ManualResetEvent cancelRequested = new ManualResetEvent(false);
        private readonly ManualResetEvent testsCompleted = new ManualResetEvent(false);

        private readonly IFrameworkHandle frameworkHandle;
        private readonly IRunContext runContext;
        private List<TestCase> currentTests;
        private ProcessOutput nodeProcess;
        private TestResult currentResult;
        private ResultObject currentResultObject;

        public TestExecutorWorker(IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            this.frameworkHandle = frameworkHandle;
            this.runContext = runContext;
        }

        public void Cancel()
        {
            //let us just kill the node process there, rather do it late, because VS engine process 
            //could exit right after this call and our node process will be left running.
            KillNodeProcess();
            this.cancelRequested.Set();
        }

        /// <summary>
        /// This is the equivalent of "RunAll" functionality
        /// </summary>
        /// <param name="sources">Refers to the list of test sources passed to the test adapter from the client.  (Client could be VS or command line)</param>
        /// <param name="runContext">Defines the settings related to the current run</param>
        /// <param name="frameworkHandle">Handle to framework.  Used for recording results</param>
        public void RunTests(IEnumerable<string> sources, ITestDiscoverer discoverer)
        {
            ValidateArg.NotNull(sources, nameof(sources));
            ValidateArg.NotNull(discoverer, nameof(discoverer));

            // Cancel any running tests before starting a new batch
            this.Cancel();

            var receiver = new TestReceiver();
            discoverer.DiscoverTests(sources, /*discoveryContext*/null, this.frameworkHandle, receiver);

            if (this.cancelRequested.WaitOne(0))
            {
                return;
            }

            this.RunTests(receiver.Tests);
        }

        /// <summary>
        /// This is the equivalent of "Run Selected Tests" functionality.
        /// </summary>
        /// <param name="tests">The list of TestCases selected to run</param>
        /// <param name="runContext">Defines the settings related to the current run</param>
        /// <param name="frameworkHandle">Handle to framework.  Used for recording results</param>
        public void RunTests(IEnumerable<TestCase> tests)
        {
            ValidateArg.NotNull(tests, nameof(tests));

            // Cancel any running tests before starting a new batch
            this.Cancel();

            // .ts file path -> project settings
            var fileToTests = new Dictionary<string, List<TestCase>>();

            // put tests into dictionary where key is their source file
            foreach (var test in tests)
            {
                if (!fileToTests.ContainsKey(test.CodeFilePath))
                {
                    fileToTests[test.CodeFilePath] = new List<TestCase>();
                }
                fileToTests[test.CodeFilePath].Add(test);
            }

            // where key is the file and value is a list of tests
            foreach (var testcaseList in fileToTests.Values)
            {
                this.currentTests = testcaseList;

                // Run all test cases in a given file
                RunTestCases(testcaseList);
            }
        }

        private void RunTestCases(IEnumerable<TestCase> tests)
        {
            // May be null, but this is handled by RunTestCase if it matters.
            // No VS instance just means no debugging, but everything else is
            // okay.
            if (!tests.Any())
            {
                return;
            }

            var startedFromVs = this.HasVisualStudioProcessId(out var vsProcessId);

            var nodeArgs = new List<string>();

            var testObjects = new List<TestCaseObject>();

            // All tests being run are for the same test file, so just use the first test listed to get the working dir
            var firstTest = tests.First();
            var testFramework = firstTest.GetPropertyValue(JavaScriptTestCaseProperties.TestFramework, defaultValue: "ExportRunner");
            var workingDir = firstTest.GetPropertyValue(JavaScriptTestCaseProperties.WorkingDir, defaultValue: Path.GetDirectoryName(firstTest.CodeFilePath));
            var nodeExePath = firstTest.GetPropertyValue<string>(JavaScriptTestCaseProperties.NodeExePath, defaultValue: null);
            var projectRootDir = firstTest.GetPropertyValue(JavaScriptTestCaseProperties.ProjectRootDir, defaultValue: Path.GetDirectoryName(firstTest.CodeFilePath));

            if (string.IsNullOrEmpty(nodeExePath) || !File.Exists(nodeExePath))
            {
                this.frameworkHandle.SendMessage(TestMessageLevel.Error, "Interpreter path does not exist: " + nodeExePath);
                return;
            }

            var nodeVersion = Nodejs.GetNodeVersion(nodeExePath);

            // We can only log telemetry when we're running in VS.
            // Since the required assemblies are not on disk if we're not running in VS, we have to reference them in a separate method
            // this way the .NET framework only tries to load the assemblies when we actually need them.
            if (startedFromVs)
            {
                this.LogTelemetry(tests.Count(), nodeVersion, this.runContext.IsBeingDebugged, testFramework);
            }

            foreach (var test in tests)
            {
                if (this.cancelRequested.WaitOne(0))
                {
                    break;
                }

                var args = GetInterpreterArgs(test, workingDir, projectRootDir);

                // Fetch the run_tests argument for starting node.exe if not specified yet
                if (nodeArgs.Count == 0)
                {
                    nodeArgs.Add(args.RunTestsScriptFile);
                }

                testObjects.Add(new TestCaseObject(framework: args.TestFramework, testName: args.TestName, testFile: args.TestFile, workingFolder: args.WorkingDirectory, projectFolder: args.ProjectRootDir));
            }

            var port = 0;
            if (this.runContext.IsBeingDebugged && startedFromVs)
            {
                this.DetachDebugger(vsProcessId);
                // Ensure that --debug-brk or --inspect-brk is the first argument
                nodeArgs.Insert(0, GetDebugArgs(nodeVersion, out port));
            }

            // make sure the tests completed is not signalled.
            this.testsCompleted.Reset();

            this.nodeProcess = ProcessOutput.Run(
                nodeExePath,
                nodeArgs,
                workingDir,
                env: null,
                visible: false,
                redirector: new TestExecutionRedirector(this.ProcessTestRunnerEmit),
                quoteArgs: false);

            if (this.runContext.IsBeingDebugged && startedFromVs)
            {
                this.AttachDebugger(vsProcessId, port, nodeVersion);
            }

            var serializedObjects = JsonConvert.SerializeObject(testObjects);

            // Send the process the list of tests to run and wait for it to complete
            this.nodeProcess.WriteInputLine(serializedObjects);

            // for node 8 the process doesn't automatically exit when debugging, so always detach
            WaitHandle.WaitAny(new[] { this.nodeProcess.WaitHandle, this.testsCompleted });
            if (this.runContext.IsBeingDebugged && startedFromVs)
            {
                this.DetachDebugger(vsProcessId);
            }

            // Automatically fail tests that haven't been run by this point (failures in before() hooks)
            foreach (var notRunTest in this.currentTests)
            {
                var result = new TestResult(notRunTest)
                {
                    Outcome = TestOutcome.Failed
                };

                if (this.currentResultObject != null)
                {
                    result.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, this.currentResultObject.stdout));
                    result.Messages.Add(new TestResultMessage(TestResultMessage.StandardErrorCategory, this.currentResultObject.stderr));
                }
                this.frameworkHandle.RecordResult(result);
                this.frameworkHandle.RecordEnd(notRunTest, TestOutcome.Failed);
            }
        }

        private void ProcessTestRunnerEmit(string line)
        {
            try
            {
                var testEvent = JsonConvert.DeserializeObject<TestEvent>(line);
                // Extract test from list of tests
                var tests = this.currentTests.Where(n => n.DisplayName == testEvent.title);
                if (tests.Any())
                {
                    switch (testEvent.type)
                    {
                        case "test start":
                            {
                                this.currentResult = new TestResult(tests.First())
                                {
                                    StartTime = DateTimeOffset.Now
                                };
                                this.frameworkHandle.RecordStart(tests.First());
                            }
                            break;
                        case "result":
                            {
                                RecordEnd(tests.First(), testEvent.result);
                            }
                            break;
                        case "pending":
                            {
                                this.currentResult = new TestResult(tests.First());
                                RecordEnd(tests.First(), testEvent.result);
                            }
                            break;
                    }
                }
                else if (testEvent.type == "suite end")
                {
                    this.currentResultObject = testEvent.result;
                    this.testsCompleted.Set();
                }
            }
            catch (JsonReaderException)
            {
                // Often lines emitted while running tests are not test results, and thus will fail to parse above
            }

            void RecordEnd(TestCase test, ResultObject resultObject)
            {
                var standardOutputLines = resultObject.stdout.Split('\n');
                var standardErrorLines = resultObject.stderr.Split('\n');

                if (resultObject.pending == true)
                {
                    this.currentResult.Outcome = TestOutcome.Skipped;
                }
                else
                {
                    this.currentResult.EndTime = DateTimeOffset.Now;
                    this.currentResult.Duration = this.currentResult.EndTime - this.currentResult.StartTime;
                    this.currentResult.Outcome = resultObject.passed ? TestOutcome.Passed : TestOutcome.Failed;
                }

                var errorMessage = string.Join(Environment.NewLine, standardErrorLines);

                this.currentResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, string.Join(Environment.NewLine, standardOutputLines)));
                this.currentResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardErrorCategory, errorMessage));
                this.currentResult.Messages.Add(new TestResultMessage(TestResultMessage.AdditionalInfoCategory, errorMessage));
                this.frameworkHandle.RecordResult(this.currentResult);
                this.frameworkHandle.RecordEnd(test, this.currentResult.Outcome);
                this.currentTests.Remove(test);
            }
        }

        private bool HasVisualStudioProcessId(out int processId)
        {
            processId = 0;
            var pid = Environment.GetEnvironmentVariable(NodejsConstants.NodeToolsProcessIdEnvironmentVariable);
            return pid == null ? false : int.TryParse(pid, out processId);
        }

        private void DetachDebugger(int vsProcessId)
        {
            VisualStudioApp.DetachDebugger(vsProcessId);
        }

        private void LogTelemetry(int testCount, Version nodeVersion, bool isDebugging, string testFramework)
        {
            VisualStudioApp.LogTelemetry(testCount, nodeVersion, isDebugging, testFramework);
        }

        private void AttachDebugger(int vsProcessId, int port, Version nodeVersion)
        {
            try
            {
                if (nodeVersion >= Node8Version)
                {
                    VisualStudioApp.AttachToProcessNode2DebugAdapter(vsProcessId, port);
                }
                else
                {
                    //the '#ping=0' is a special flag to tell VS node debugger not to connect to the port,
                    //because a connection carries the consequence of setting off --debug-brk, and breakpoints will be missed.
                    var qualifierUri = string.Format("tcp://localhost:{0}#ping=0", port);
                    while (!VisualStudioApp.AttachToProcess(this.nodeProcess.Process, vsProcessId, NodejsRemoteDebugPortSupplierUnsecuredId, qualifierUri))
                    {
                        if (this.nodeProcess.Wait(TimeSpan.FromMilliseconds(500)))
                        {
                            break;
                        }
                    }
                }
#if DEBUG
            }
            catch (COMException ex)
            {
                this.frameworkHandle.SendMessage(TestMessageLevel.Error, "Error occurred connecting to debuggee.");
                this.frameworkHandle.SendMessage(TestMessageLevel.Error, ex.ToString());
                KillNodeProcess();
            }
#else
            }
            catch (COMException)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Error, "Error occurred connecting to debuggee.");
                KillNodeProcess();
            }
#endif
        }

        private static int GetFreePort()
        {
            return Enumerable.Range(new Random().Next(49152, 65536), 60000).Except(
                from connection in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                select connection.LocalEndPoint.Port
            ).First();
        }

        private static TestFramework.ArgumentsToRunTests GetInterpreterArgs(TestCase test, string workingDir, string projectRootDir)
        {
            var testFile = test.GetPropertyValue(JavaScriptTestCaseProperties.TestFile, defaultValue: test.CodeFilePath);
            var testFramework = test.GetPropertyValue<string>(JavaScriptTestCaseProperties.TestFramework, defaultValue: null);
            return FrameworkDiscoverer.Instance.Get(testFramework).GetArgumentsToRunTests(test.DisplayName, testFile, workingDir, projectRootDir);
        }

        private static string GetDebugArgs(Version nodeVersion, out int port)
        {
            port = GetFreePort();

            return nodeVersion >= Node8Version ? $"--inspect-brk={port}" : $"--debug-brk={port}";
        }

        private void KillNodeProcess()
        {
            this.nodeProcess?.Kill();
        }

        internal sealed class TestExecutionRedirector : Redirector
        {
            private readonly Action<string> writer;

            public TestExecutionRedirector(Action<string> onWriteLine)
            {
                this.writer = onWriteLine;
            }

            public override void WriteErrorLine(string line) => this.writer(line);

            public override void WriteLine(string line) => this.writer(line);

            public override bool CloseStandardInput() => false;
        }
    }
}
