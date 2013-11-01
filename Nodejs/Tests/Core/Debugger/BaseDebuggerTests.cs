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
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace NodejsTests.Debugger {
    internal enum NodeVersion {
        NodeVersion_Unknown,
    };

    public class BaseDebuggerTests {
        internal virtual string DebuggerTestPath {
            get {
                return TestUtilities.TestData.GetPath(@"TestData\DebuggerProject\");
            }
        }

        internal static NodeBreakpoint AddBreakPoint(
            NodeDebugger newproc,
            string fileName,
            int line,
            bool enabled = true,
            BreakOn breakOn = new BreakOn(),
            string condition = "",
            Action<bool> successHandler = null,
            Action failureHandler = null
        ) {
            NodeBreakpoint breakPoint = newproc.AddBreakPoint(fileName, line, enabled, breakOn, condition);
            breakPoint.Add(successHandler, failureHandler);
            return breakPoint;
        }

        internal NodeDebugger DebugProcess(
            string filename,
            Action<NodeDebugger> onProcessCreated = null,
            Action<NodeDebugger, NodeThread> onLoadComplete = null,
            string interpreterOptions = null,
            NodeDebugOptions debugOptions = NodeDebugOptions.None,
            string cwd = null,
            string arguments = "",
            bool resumeOnProcessLoad = true
        ) {
            if (!Path.IsPathRooted(filename)) {
                filename = DebuggerTestPath + filename;
            }
            string fullPath = Path.GetFullPath(filename);
            string dir = cwd ?? Path.GetFullPath(Path.GetDirectoryName(filename));
            if (!String.IsNullOrEmpty(arguments)) {
                arguments = "\"" + fullPath + "\" " + arguments;
            } else {
                arguments = "\"" + fullPath + "\"";
            }

            // Load process
            AutoResetEvent processLoaded = new AutoResetEvent(false);
            Assert.IsNotNull(Nodejs.NodeExePath, "Node isn't installed");
            NodeDebugger process =
                new NodeDebugger(
                    Nodejs.NodeExePath,
                    arguments,
                    dir,
                    null,
                    interpreterOptions,
                    debugOptions,
                    null,
                    createNodeWindow: false);
            if (onProcessCreated != null) {
                onProcessCreated(process);
            }
            process.ProcessLoaded += (sender, args) => {
                // Invoke onLoadComplete delegate, if requested
                if (onLoadComplete != null) {
                    onLoadComplete(process, args.Thread);
                }
                processLoaded.Set();
            };
            process.Start();
            AssertWaited(processLoaded);

            // Resume, if requested
            if (resumeOnProcessLoad) {
                process.Resume();
            }

            return process;
        }

        internal Process StartNodeProcess(
            string filename,
            string interpreterOptions = null,
            string cwd = null,
            string arguments = "",
            bool startBrokenAtEntryPoint = false
        ) {
            if (!Path.IsPathRooted(filename)) {
                filename = DebuggerTestPath + filename;
            }
            string fullPath = Path.GetFullPath(filename);
            string dir = cwd ?? Path.GetFullPath(Path.GetDirectoryName(filename));
            arguments = 
                (startBrokenAtEntryPoint ? "--debug-brk" : "--debug") +
                " \"" + fullPath + "\"" +
                (String.IsNullOrEmpty(arguments) ? "" : " " + arguments);

            Assert.IsNotNull(Nodejs.NodeExePath, "Node isn't installed");
            var psi = new ProcessStartInfo(Nodejs.NodeExePath, arguments);
            psi.WorkingDirectory = dir;
            var process = new Process();
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;
            process.Start();

            return process;
        }

        internal NodeDebugger AttachToNodeProcess(
            Action<NodeDebugger> onProcessCreated = null,
            Action<NodeDebugger, NodeThread> onLoadComplete = null,
            string hostName = "localhost",
            ushort portNumber = 5858,
            int id = 0,
            bool resumeOnProcessLoad = false){
            // Load process
            AutoResetEvent processLoaded = new AutoResetEvent(false);
            var process = new NodeDebugger(hostName, portNumber, id);
            if (onProcessCreated != null) {
                onProcessCreated(process);
            }
            process.ProcessLoaded += (sender, args) => {
                // Invoke onLoadComplete delegate, if requested
                if (onLoadComplete != null) {
                    onLoadComplete(process, args.Thread);
                }
                processLoaded.Set();
            };
            process.StartListening();
            AssertWaited(processLoaded);

            // Resume, if requested
            if (resumeOnProcessLoad) {
                process.Resume();
            }

            return process;
        }

        internal virtual NodeVersion Version {
            get {
                return NodeVersion.NodeVersion_Unknown;
            }
        }

        internal void LocalsTest(
            string filename,
            int breakpoint,
            int frameIndex = 0,
            string[] expectedParams = null,
            string[] expectedLocals = null
        ) {
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: breakpoint),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: breakpoint),
                    new TestStep(validation: (process, thread) => {
                        var frame = thread.Frames[frameIndex];
                        AssertUtil.ContainsExactly(
                            new HashSet<string>(expectedParams ?? new string[] { }),
                            frame.Parameters.Select(x => x.Expression)
                        );
                        AssertUtil.ContainsExactly(
                            new HashSet<string>(expectedLocals ?? new string[] { }),
                            frame.Locals.Select(x => x.Expression)
                        );
                    }),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        internal void ExecTest(
            NodeThread thread,
            int frameIndex = 0,
            string expression = null,
            string expectedType = null,
            string expectedValue = null,
            string expectedException = null,
            string expectedFrame = null
        ) {
            AutoResetEvent textExecuted = new AutoResetEvent(false);
            var frame = thread.Frames[frameIndex];
            NodeEvaluationResult evaluationResult = null;
            frame.ExecuteText(
                expression,
                (result) => {
                    evaluationResult = result;
                    textExecuted.Set();
                }
            );
            AssertWaited(textExecuted);
            if (expectedType != null) {
                Assert.AreEqual(expectedType, evaluationResult.TypeName);
            }
            if (expectedValue != null) {
                Assert.AreEqual(expectedValue, evaluationResult.StringValue);
            }
            if (expectedException != null) {
                Assert.AreEqual(expectedException, evaluationResult.ExceptionText);
            }
            if (expectedFrame != null) {
                Assert.AreEqual(expectedFrame, frame.FunctionName);
            }
        }

        internal class ExceptionHandlerInfo {
            public readonly int FirstLine;
            public readonly int LastLine;
            public readonly HashSet<string> Expressions;

            public ExceptionHandlerInfo(int firstLine, int lastLine, params string[] expressions) {
                FirstLine = firstLine;
                LastLine = lastLine;
                Expressions = new HashSet<string>(expressions);
            }
        }

        const ExceptionHitTreatment _breakNever = ExceptionHitTreatment.BreakNever;
        const ExceptionHitTreatment _breakAlways = ExceptionHitTreatment.BreakAlways;
        const ExceptionHitTreatment _breakOnUnhandled = ExceptionHitTreatment.BreakOnUnhandled;

        internal class ExceptionInfo {
            public readonly string TypeName;
            public readonly string Description;
            public readonly int? LineNo;

            public ExceptionInfo(string typeName, string description, int? lineNo = null) {
                TypeName = typeName;
                Description = description;
                LineNo = lineNo;
            }
        }

        internal ICollection<KeyValuePair<string, ExceptionHitTreatment>> CollectExceptionTreatments(string exceptionName = null, ExceptionHitTreatment exceptionTreatment = ExceptionHitTreatment.BreakAlways) {
            return
                string.IsNullOrEmpty(exceptionName) ?
                new KeyValuePair<string, ExceptionHitTreatment>[0] :
                new KeyValuePair<string, ExceptionHitTreatment>[] { new KeyValuePair<string, ExceptionHitTreatment>(exceptionName, exceptionTreatment) };
        }

        internal void TestExceptions(
            string filename,
            ExceptionHitTreatment? defaultExceptionTreatment,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments,
            int expectedExitCode,
            params ExceptionInfo[] exceptions
        ) {
            var steps = new List<TestStep>();
            foreach (var exception in exceptions) {
                steps.Add(new TestStep(action: TestAction.ResumeProcess, expectedExceptionRaised: exception));
            }
            steps.Add(new TestStep(action: TestAction.ResumeProcess, expectedExitCode: expectedExitCode));
            TestDebuggerSteps(
                filename,
                steps,
                defaultExceptionTreatment: defaultExceptionTreatment,
                exceptionTreatments: exceptionTreatments
                );
        }

        internal enum TestAction {
            None = 0,
            Wait,
            ResumeThread,
            ResumeProcess,
            StepOver,
            StepInto,
            StepOut,
            AddBreakpoint,
            RemoveBreakpoint,
            UpdateBreakpoint,
            KillProcess,
            Detach,
        }

        internal struct TestStep {
            public TestAction _action;
            public int? _expectedEntryPointHit;
            public int? _expectedBreakpointHit;
            public int? _expectedStepComplete;
            public ExceptionInfo _expectedExceptionRaised;
            public int? _targetBreakpoint;
            public uint? _expectedHitCount;
            public uint? _hitCount;
            public bool? _enabled;
            public BreakOn? _breakOn;
            public string _condition;
            public Action<NodeDebugger, NodeThread> _validation;
            public int? _expectedExitCode;

            internal TestStep(
                TestAction action = TestAction.None,
                int? expectedEntryPointHit = null,
                int? expectedBreakpointHit = null,
                int? expectedStepComplete = null,
                ExceptionInfo expectedExceptionRaised = null,
                int? targetBreakpoint = null,
                uint? expectedHitCount = null,
                uint? hitCount = null,
                bool? enabled = null,
                BreakOn? breakOn = null,
                string condition = null,
                Action<NodeDebugger, NodeThread> validation = null,
                int? expectedExitCode = null
            ) {
                _action = action;
                _expectedEntryPointHit = expectedEntryPointHit;
                _expectedBreakpointHit = expectedBreakpointHit;
                _expectedStepComplete = expectedStepComplete;
                _expectedExceptionRaised = expectedExceptionRaised;
                _targetBreakpoint = targetBreakpoint;
                _expectedHitCount = expectedHitCount;
                _hitCount = hitCount;
                _enabled = enabled;
                _breakOn = breakOn;
                _condition = condition;
                _validation = validation;
                _expectedExitCode = expectedExitCode;
            }
        }

        internal void TestDebuggerSteps(
            string filename,
            IEnumerable<TestStep> steps,
            string interpreterOptions = null,
            Action<NodeDebugger> onProcessCreated = null,
            ExceptionHitTreatment? defaultExceptionTreatment = null,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments = null
        ) {
            if (!Path.IsPathRooted(filename)) {
                filename = DebuggerTestPath + filename;
            }

            NodeThread thread = null;
            using (var process = DebugProcess(
                filename,
                onProcessCreated: onProcessCreated,
                onLoadComplete: (newproc, newthread) => {
                    thread = newthread;
                },
                interpreterOptions: interpreterOptions,
                resumeOnProcessLoad: false
            )) {
                TestDebuggerSteps(
                    process,
                    thread,
                    filename,
                    steps,
                    defaultExceptionTreatment,
                    exceptionTreatments,
                    waitForExit: true);
            }
        }

        internal void TestDebuggerSteps(
            NodeDebugger process,
            NodeThread thread,
            string filename,
            IEnumerable<TestStep> steps,
            ExceptionHitTreatment? defaultExceptionTreatment = null,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments = null,
            bool waitForExit = false
        ) {
            if (!Path.IsPathRooted(filename)) {
                filename = DebuggerTestPath + filename;
            }

            if (defaultExceptionTreatment != null || exceptionTreatments != null) {
                process.SetExceptionTreatment(defaultExceptionTreatment, exceptionTreatments);
            }

            Dictionary<int, NodeBreakpoint> breakpoints = new Dictionary<int, NodeBreakpoint>();
            AutoResetEvent breakpointBindSuccess = new AutoResetEvent(false);
            AutoResetEvent breakpointBindFailure = new AutoResetEvent(false);

            AutoResetEvent entryPointHit = new AutoResetEvent(false);
            process.EntryPointHit += (sender, e) => {
                Assert.AreEqual(thread, e.Thread);
                entryPointHit.Set();
            };

            AutoResetEvent breakpointHit = new AutoResetEvent(false);
            process.BreakpointHit += (sender, e) => {
                Assert.AreEqual(thread, e.Thread);
                Assert.AreEqual(thread.Frames.First().LineNo, e.Breakpoint.LineNo);
                breakpointHit.Set();
            };

            AutoResetEvent stepComplete = new AutoResetEvent(false);
            process.StepComplete += (sender, e) => {
                Assert.AreEqual(thread, e.Thread);
                stepComplete.Set();
            };

            AutoResetEvent exceptionRaised = new AutoResetEvent(false);
            NodeException exception = null;
            process.ExceptionRaised += (sender, e) => {
                Assert.AreEqual(thread, e.Thread);
                exception = e.Exception;
                exceptionRaised.Set();
            };

            AutoResetEvent processExited = new AutoResetEvent(false);
            int exitCode = 0;
            process.ProcessExited += (sender, e) => {
                exitCode = e.ExitCode;
                processExited.Set();
            };

            foreach (var step in steps) {
                Assert.IsFalse(
                    ((step._expectedEntryPointHit != null ? 1 : 0) +
                     (step._expectedBreakpointHit != null ? 1 : 0) +
                     (step._expectedStepComplete != null ? 1 : 0) +
                     (step._expectedExceptionRaised != null ? 1 : 0)) > 1);
                NodeBreakpoint breakpoint;
                bool wait = false;
                switch (step._action) {
                    case TestAction.None:
                        break;
                    case TestAction.Wait:
                        wait = true;
                        break;
                    case TestAction.ResumeThread:
                        thread.Resume();
                        wait = true;
                        break;
                    case TestAction.ResumeProcess:
                        process.Resume();
                        wait = true;
                        break;
                    case TestAction.StepOver:
                        thread.StepOver();
                        wait = true;
                        break;
                    case TestAction.StepInto:
                        thread.StepInto();
                        wait = true;
                        break;
                    case TestAction.StepOut:
                        thread.StepOut();
                        wait = true;
                        break;
                    case TestAction.AddBreakpoint:
                        int breakpointLine = step._targetBreakpoint.Value;
                        Assert.IsFalse(breakpoints.TryGetValue(breakpointLine, out breakpoint));
                        breakpoints[breakpointLine] =
                            AddBreakPoint(
                                process,
                                filename,
                                breakpointLine,
                                step._enabled ?? true,
                                step._breakOn ?? new BreakOn(),
                                step._condition,
                                successHandler: (fixedUpLocation) => {
                                    breakpointBindSuccess.Set();
                                }
                            );
                        AssertWaited(breakpointBindSuccess);
                        AssertNotSet(breakpointBindFailure);
                        breakpointBindSuccess.Reset();
                        break;
                    case TestAction.RemoveBreakpoint:
                        breakpointLine = step._targetBreakpoint.Value;
                        breakpoints[breakpointLine].Remove();
                        breakpoints.Remove(breakpointLine);
                        break;
                    case TestAction.UpdateBreakpoint:
                        breakpoint = breakpoints[step._targetBreakpoint.Value];
                        if (step._hitCount != null) {
                            Assert.IsTrue(breakpoint.SetHitCount(step._hitCount.Value));
                        }
                        if (step._enabled != null) {
                            Assert.IsTrue(breakpoint.SetEnabled(step._enabled.Value));
                        }
                        if (step._breakOn != null) {
                            Assert.IsTrue(breakpoint.SetBreakOn(step._breakOn.Value));
                        }
                        if (step._condition != null) {
                            Assert.IsTrue(breakpoint.SetCondition(step._condition));
                        }
                        break;
                    case TestAction.KillProcess:
                        process.Terminate();
                        break;
                    case TestAction.Detach:
                        process.Detach();
                        break;
                }

                if (wait) {
                    if (step._expectedEntryPointHit != null) {
                        AssertWaited(entryPointHit);
                        AssertNotSet(breakpointHit);
                        AssertNotSet(stepComplete);
                        AssertNotSet(exceptionRaised);
                        Assert.IsNull(exception);
                        entryPointHit.Reset();
                    } else if (step._expectedBreakpointHit != null) {
                        AssertWaited(breakpointHit);
                        AssertNotSet(entryPointHit);
                        AssertNotSet(stepComplete);
                        AssertNotSet(exceptionRaised);
                        Assert.IsNull(exception);
                        breakpointHit.Reset();
                    } else if (step._expectedStepComplete != null) {
                        AssertWaited(stepComplete);
                        AssertNotSet(entryPointHit);
                        AssertNotSet(breakpointHit);
                        AssertNotSet(exceptionRaised);
                        Assert.IsNull(exception);
                        stepComplete.Reset();
                    } else if (step._expectedExceptionRaised != null) {
                        AssertWaited(exceptionRaised);
                        AssertNotSet(entryPointHit);
                        AssertNotSet(breakpointHit);
                        AssertNotSet(stepComplete);
                        exceptionRaised.Reset();
                    } else {
                        AssertNotSet(entryPointHit);
                        AssertNotSet(breakpointHit);
                        AssertNotSet(stepComplete);
                        AssertNotSet(exceptionRaised);
                        Assert.IsNull(exception);
                    }
                }

                if (step._expectedEntryPointHit != null) {
                    Assert.AreEqual(step._expectedEntryPointHit.Value, thread.Frames.First().LineNo);
                }
                else if (step._expectedBreakpointHit != null) {
                    Assert.AreEqual(step._expectedBreakpointHit.Value, thread.Frames.First().LineNo);
                }
                else if (step._expectedStepComplete != null) {
                    Assert.AreEqual(step._expectedStepComplete.Value, thread.Frames.First().LineNo);
                }
                else if (step._expectedExceptionRaised != null) {
                    Assert.AreEqual(step._expectedExceptionRaised.TypeName, exception.TypeName);
                    Assert.AreEqual(step._expectedExceptionRaised.Description, exception.Description);
                    if (step._expectedExceptionRaised.LineNo != null) {
                        Assert.AreEqual(step._expectedExceptionRaised.LineNo.Value, thread.Frames[0].LineNo);
                    }
                    exception = null;
                }

                if (step._expectedHitCount != null) {
                    breakpoint = breakpoints[step._targetBreakpoint.Value];
                    Assert.AreEqual(step._expectedHitCount.Value, breakpoint.GetHitCount());
                }

                if (step._validation != null) {
                    step._validation(process, thread);
                }

                if (step._expectedExitCode != null) {
                    AssertWaited(processExited);
                    Assert.AreEqual(step._expectedExitCode.Value, exitCode);
                }
            }

            if (waitForExit) {
                process.WaitForExit(10000);
            }

            AssertNotSet(entryPointHit);
            AssertNotSet(breakpointHit);
            AssertNotSet(stepComplete);
            AssertNotSet(exceptionRaised);
            Assert.IsNull(exception);
        }

        internal static void AssertWaited(EventWaitHandle eventObj) {
            if (!eventObj.WaitOne(10000)) {
                Assert.Fail("Failed to wait on event");
            }
        }

        internal static void AssertNotSet(EventWaitHandle eventObj) {
            if (eventObj.WaitOne(10)) {
                Assert.Fail("Unexpected set EventWaitHandle");
            }
        }

    }
}
