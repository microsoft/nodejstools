// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.TestAdapter
{
    [ExtensionUri(NodejsConstants.PackageJsonExecutorUriString)]
    public sealed class PackageJsonTestExecutor : ITestExecutor
    {
        private List<TestCase> currentTests;
        private IFrameworkHandle frameworkHandle;
        private TestResult currentResult;
        private ResultObject currentResultObject;

        public void Cancel()
        {
            // We don't support cancellation
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            var receiver = new JavaScriptTestExecutor.TestReceiver();
            var discoverer = new PackageJsonTestDiscoverer();
            discoverer.DiscoverTests(sources, null, frameworkHandle, receiver);

            this.RunTests(receiver.Tests, runContext, frameworkHandle);
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (!tests.Any())
            {
                return;
            }

            this.frameworkHandle = frameworkHandle;
            this.currentTests = new List<TestCase>(tests);

            var nodeArgs = new List<string>();
            var testObjects = new List<TestCaseObject>();

            // All tests being run are for the same test file, so just use the first test listed to get the working dir
            var firstTest = tests.First();
            var testFramework = firstTest.GetPropertyValue(JavaScriptTestCaseProperties.TestFramework, defaultValue: "ExportRunner");
            var workingDir = firstTest.GetPropertyValue(JavaScriptTestCaseProperties.WorkingDir, defaultValue: Path.GetDirectoryName(firstTest.Source));
            var nodeExePath = firstTest.GetPropertyValue<string>(JavaScriptTestCaseProperties.NodeExePath, defaultValue: null);

            if (!File.Exists(nodeExePath))
            {
                frameworkHandle.SendMessage(TestMessageLevel.Error, $"Interpreter path does not exist: {nodeExePath}.");
                return;
            }

            foreach (var test in tests)
            {
                var args = GetInterpreterArgs(test, workingDir, workingDir);

                // Fetch the run_tests argument for starting node.exe if not specified yet
                if (nodeArgs.Count == 0)
                {
                    nodeArgs.Add(args.RunTestsScriptFile);
                }

                testObjects.Add(new TestCaseObject(framework: args.TestFramework, testName: args.TestName, testFile: args.TestFile, workingFolder: args.WorkingDirectory, projectFolder: args.ProjectRootDir));
            }

            var nodeProcess = ProcessOutput.Run(
                nodeExePath,
                nodeArgs,
                workingDir,
                env: null,
                visible: false,
                redirector: new JavaScriptTestExecutor.TestExecutionRedirector(this.ProcessTestRunnerEmit),
                quoteArgs: false);

            // Send the process the list of tests to run and wait for it to complete
            nodeProcess.WriteInputLine(JsonConvert.SerializeObject(testObjects));

            // for node 8 the process doesn't automatically exit when debugging, so always detach
            WaitHandle.WaitAny(new[] { nodeProcess.WaitHandle });

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
                frameworkHandle.RecordResult(result);
                frameworkHandle.RecordEnd(notRunTest, TestOutcome.Failed);
            }
        }

        private static TestFramework.ArgumentsToRunTests GetInterpreterArgs(TestCase test, string workingDir, string projectRootDir)
        {
            var testFile = test.GetPropertyValue(JavaScriptTestCaseProperties.TestFile, defaultValue: test.CodeFilePath);
            var testFramework = test.GetPropertyValue<string>(JavaScriptTestCaseProperties.TestFramework, defaultValue: null);
            return FrameworkDiscover.Intance.Get(testFramework).GetArgumentsToRunTests(test.DisplayName, testFile, workingDir, projectRootDir);
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
                                RecordEnd(this.frameworkHandle, tests.First(), this.currentResult, testEvent.result);
                            }
                            break;
                        case "pending":
                            {
                                this.currentResult = new TestResult(tests.First());
                                RecordEnd(this.frameworkHandle, tests.First(), this.currentResult, testEvent.result);
                            }
                            break;
                    }
                }
                else if (testEvent.type == "suite end")
                {
                    this.currentResultObject = testEvent.result;
                }
            }
            catch (JsonReaderException)
            {
                // Often lines emitted while running tests are not test results, and thus will fail to parse above
            }
        }

        private void RecordEnd(IFrameworkHandle frameworkHandle, TestCase test, TestResult result, ResultObject resultObject)
        {
            var standardOutputLines = resultObject.stdout.Split('\n');
            var standardErrorLines = resultObject.stderr.Split('\n');

            if (resultObject.pending == true)
            {
                result.Outcome = TestOutcome.Skipped;
            }
            else
            {
                result.EndTime = DateTimeOffset.Now;
                result.Duration = result.EndTime - result.StartTime;
                result.Outcome = resultObject.passed ? TestOutcome.Passed : TestOutcome.Failed;
            }

            result.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, string.Join(Environment.NewLine, standardOutputLines)));
            result.Messages.Add(new TestResultMessage(TestResultMessage.StandardErrorCategory, string.Join(Environment.NewLine, standardErrorLines)));
            result.Messages.Add(new TestResultMessage(TestResultMessage.AdditionalInfoCategory, string.Join(Environment.NewLine, standardErrorLines)));
            frameworkHandle.RecordResult(result);
            frameworkHandle.RecordEnd(test, result.Outcome);
            this.currentTests.Remove(test);
        }
    }
}
