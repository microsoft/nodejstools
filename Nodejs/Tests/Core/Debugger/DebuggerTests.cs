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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;

namespace NodejsTests.Debugger {
    [TestClass]
    public class DebuggerTests : BaseDebuggerTests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        #region Enum Children Tests

        // NYI Disabled
        //[TestMethod, Priority(0)]
        public void EnumChildrenTest() {
            const int lastLine = 40;

            ChildTest(EnumChildrenTestName, lastLine, "s", new ChildInfo[] { new ChildInfo("[0]", "frozenset([2, 3, 4])") });
            ChildTest(EnumChildrenTestName, lastLine, "c2inst", new ChildInfo("abc", "42", "0x2a"), new ChildInfo("bar", "100", "0x64"), new ChildInfo("self", "myrepr", "myhex"));
            ChildTest(EnumChildrenTestName, lastLine, "c3inst", new ChildInfo("_contents", "[1, 2]"), new ChildInfo("abc", "42", "0x2a"), new ChildInfo("[0]", "1"), new ChildInfo("[1]", "2"));
            ChildTest(EnumChildrenTestName, lastLine, "l", new ChildInfo("[0]", "1"), new ChildInfo("[1]", "2"));
            ChildTest(EnumChildrenTestName, lastLine, "d1", new ChildInfo("[42]", "100", "0x64"));
            ChildTest(EnumChildrenTestName, lastLine, "d2", new ChildInfo("['abc']", "'foo'"));
            ChildTest(EnumChildrenTestName, lastLine, "i", null);
            ChildTest(EnumChildrenTestName, lastLine, "u1", null);
        }

        // NYI Disabled
        //[TestMethod, Priority(0)]
        public void EnumChildrenTestPrevFrame() {
            const int breakLine = 2;

            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "s", 1, new ChildInfo[] { new ChildInfo("[0]", "frozenset([2, 3, 4])") });
            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "c2inst", 1, new ChildInfo("abc", "42", "0x2a"), new ChildInfo("bar", "100", "0x64"), new ChildInfo("self", "myrepr", "myhex"));
            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "l", 1, new ChildInfo("[0]", "1"), new ChildInfo("[1]", "2"));
            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "d1", 1, new ChildInfo("[42]", "100", "0x64"));
            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "d2", 1, new ChildInfo("['abc']", "'foo'"));
            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "i", 1, null);
            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "u1", 1, null);
        }

        // NYI Disabled
        //[TestMethod, Priority(0)]
        public void GeneratorChildrenTest() {
            ChildTest("GeneratorTest.py", 6, "a", 0,
                new ChildInfo("gi_code"),
                new ChildInfo("gi_frame"),
                new ChildInfo("gi_running")
            );
        }

        public virtual string EnumChildrenTestName {
            get {
                return "EnumChildTest.js";
            }
        }

        private void ChildTest(string filename, int lineNo, string text, params ChildInfo[] children) {
            ChildTest(filename, lineNo, text, 0, children);
        }

        private void ChildTest(string filename, int lineNo, string text, int frame, params ChildInfo[] children) {
            NodeThread thread = null;
            var process =
                DebugProcess(
                    filename,
                    onLoadComplete: (newproc, newthread) => {
                        AddBreakPoint(newproc, filename, lineNo, 0);
                        thread = newthread;
                    }
                );

            AutoResetEvent brkHit = new AutoResetEvent(false);
            process.BreakpointHit += (sender, args) => {
                brkHit.Set();
            };

            process.Start();

            AssertWaited(brkHit);

            var frames = thread.Frames;

            NodeEvaluationResult evalRes = frames[frame].ExecuteTextAsync(text).Result;
            Assert.IsTrue(evalRes != null, "didn't get evaluation result");

            if (children == null) {
                Assert.IsTrue(!evalRes.Type.HasFlag(NodeExpressionType.Expandable));
                Assert.IsTrue(evalRes.GetChildrenAsync().Result == null);
            } else {
                Assert.IsTrue(evalRes.Type.HasFlag(NodeExpressionType.Expandable));
                var childrenReceived = new List<NodeEvaluationResult>(evalRes.GetChildrenAsync().Result);

                Assert.AreEqual(children.Length, childrenReceived.Count, String.Format("received incorrect number of children: {0} expected, received {1}", children.Length, childrenReceived.Count));
                for (int i = 0; i < children.Length; i++) {
                    var curChild = children[i];
                    bool foundChild = false;
                    for (int j = 0; j < childrenReceived.Count; j++) {
                        var curReceived = childrenReceived[j];
                        if (ChildrenMatch(curChild, curReceived)) {
                            foundChild = true;

                            if (children[i].ChildText.StartsWith("[")) {
                                Assert.AreEqual(childrenReceived[j].Expression, text + children[i].ChildText);
                            } else {
                                Assert.AreEqual(childrenReceived[j].Expression, text + "." + children[i].ChildText);
                            }

                            Assert.AreEqual(childrenReceived[j].Frame, frames[frame]);
                            childrenReceived.RemoveAt(j);
                            break;
                        }
                    }
                    Assert.IsTrue(foundChild, "failed to find " + children[i].ChildText + " found " + String.Join(", ", childrenReceived.Select(x => x.Expression)));
                }
                Assert.IsTrue(childrenReceived.Count == 0, "there's still some children left over which we didn't find");
            }

            process.Resume();

            process.WaitForExit();
        }

        private bool ChildrenMatch(ChildInfo curChild, NodeEvaluationResult curReceived) {
            return curReceived.StringValue == curChild.ChildText && 
                (curReceived.StringValue == curChild.Repr || curChild.Repr == null);
        }

        class ChildInfo {
            public readonly string ChildText;
            public readonly string Repr;
            public readonly string HexRepr;

            public ChildInfo(string key, string value = null, string hexRepr = null) {
                ChildText = key;
                Repr = value;
                HexRepr = hexRepr;
            }
        }

        #endregion

        #region BreakAll Tests

        [TestMethod, Priority(0)]
        public void TestBreakAll() {
            // Load process (running)
            NodeThread thread = null;
            using (var process = DebugProcess(
                "BreakAllTest.js",
                onLoadComplete: (newproc, newthread) => {
                    thread = newthread;
                }
            )) {
                // BreakAll
                Thread.Sleep(500);
                AutoResetEvent breakComplete = new AutoResetEvent(false);
                process.AsyncBreakComplete += (sender, args) => {
                    Assert.AreEqual(thread, args.Thread);
                    breakComplete.Set();
                };
                process.BreakAllAsync().Wait();
                AssertWaited(breakComplete);
                breakComplete.Reset();

                process.Terminate();
                AssertNotSet(breakComplete);
            }
        }

        #endregion

        #region Eval Tests

        [TestMethod, Priority(0)]
        public void EvalTests() {
            TestDebuggerSteps(
                "LocalsTest4.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 1),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 8),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 1),
                    new TestStep(validation: (process, thread) => {
                            ExecTest(thread, expression: "x + 1", expectedType: "Number", expectedValue: "43", expectedFrame: "f");
                        }
                    ),
                    new TestStep(validation: (process, thread) => {
                            ExecTest(thread, frameIndex: 1, expression: "baz - 1", expectedType: "Number", expectedValue: "41", expectedFrame: "g");
                        }
                    ),
                    new TestStep(validation: (process, thread) => {
                            ExecTest(thread, expression: "not_defined", expectedException: "ReferenceError: not_defined is not defined", expectedFrame: "f");
                        }
                    ),
                    new TestStep(validation: (process, thread) => {
                            ExecTest(thread, expression: "bad_expression)", expectedException: "SyntaxError: Unexpected token )", expectedFrame: "f");
                        }
                    ),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        #endregion

        #region Locals Tests

        [TestMethod, Priority(0)]
        public void LocalsTest() {
            LocalsTest(
                "LocalsTest.js",
                2,
                expectedLocals: new string[] { "x" }
            );

            LocalsTest(
                "LocalsTest2.js",
                1,
                expectedParams: new string[] { "x" }
            );

            LocalsTest(
                "LocalsTest3.js",
                2,
                expectedParams: new string[] { "x" },
                expectedLocals: new string[] { "y" }
            );
        }

        /// <summary>
        /// http://nodejstools.codeplex.com/workitem/13
        /// </summary>
        [TestMethod, Priority(0)]
        public void SpecialNumberLocalsTest() {
            LocalsTest(
                "SpecialNumberLocalsTest.js",
                6,
                expectedLocals: new string[] { "nan", "negInf", "nul", "posInf" },
                expectedValues: new string[] { "NaN", "-Infinity", "null", "Infinity" },
                expectedHexValues: new string[] { "NaN", "-Infinity", null, "Infinity" }
            );
        }

        [TestMethod, Priority(0)]
        public void GlobalsTest() {
            LocalsTest(
                "GlobalsTest.js",
                3,
                expectedParams: new string[] { "exports", "require", "module", "__filename", "__dirname" },
                expectedLocals: new[] { "y", "x" });
        }

        #endregion

        #region Stepping Tests

        [TestMethod, Priority(0)]
        public void StepTest() {
            // Bug 509: http://pytools.codeplex.com/workitem/509
            TestDebuggerSteps(
                "SteppingTestBug509.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 2),
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 0), // step into triangular_number
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 0), // step over triangular_number
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 0), // step into triangular_number
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 0), // step into triangular_number
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // UNDONE Enable once just my code supported
            //// Bug 508: http://pytools.codeplex.com/workitem/508
            //TestDebuggerSteps(
            //    "SteppingTestBug508.js",
            //    new[] {
            //        new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 1),
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 2), // step print (should step over)
            //        new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
            //    }
            //);


            // Bug 507: http://pytools.codeplex.com/workitem/507
            TestDebuggerSteps(
                "SteppingTestBug507",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 7),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 11),    // step over Z.prototype.foo = function () { ... }
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 12),    // step over p = Z()
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 8),     // step into print add_two_numbers(p.foo, 3)
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 12),     // step out return 7
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 1),     // step into add_two_numbers(p.foo, 3)
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // Bug 503: http://pytools.codeplex.com/workitem/503
            TestDebuggerSteps(
                "SteppingTestBug503.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 6),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 14),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 17),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 6),   // continue from def x1(y):
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 2),          // step out after hitting breakpoint at return y
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 2),          // step out z += 1
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 2),          // step out z += 1
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 2),          // step out z += 1
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 2),          // step out z += 1
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 18),         // step out z += 1

                    new TestStep(action: TestAction.StepOut, expectedBreakpointHit: 14),         // step out after stepping out to x2(5)
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 11),         // step out after hitting breakpoint at return y
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 11),         // step out return z + 3
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 11),         // step out return z + 3
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 11),         // step out return z + 3
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 11),         // step out return z + 3
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 19),         // step out return z + 3
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // UNDONE Enable once just my code supported
            //TestDebuggerSteps(
            //    "SteppingTest7.js",
            //    new[] {
            //        new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 16),
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 2),     // step into f() call
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 3),     // step into print 'abc'
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 4),     // step into print 'def'
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 5),     // step into print 'baz'
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 17),    // step into end }
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 8),     // step into g()
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 9),     // step into dict assign
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 10),    // step into print 'hello'
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 18),    // step into end }
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 13),    // step into h()
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 14),    // step into print 'h'
            //        new TestStep(action: TestAction.StepInto, expectedStepComplete: 19),    // step into end }
            //        new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
            //    }
            //);

            TestDebuggerSteps(
                "SteppingTest6.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 1), // step over print 'hello world'
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 2), // step over a = [1, 2, 3]
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 3),  // step over print a
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            TestDebuggerSteps(
                "SteppingTest5.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 8),
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 4), // step into f()
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 9), // step out of f() on line "g()"
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            TestDebuggerSteps(
                "SteppingTest4.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 1), // step into f()
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 2), // step over for i in (1,3):
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 1), // step over for print i
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 2), // step over for i in (1,3):
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 1), // step over for print i
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 2), // step over for i in (1,3):
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 1), // step over for print i
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 4), // step over for i in (1,3):
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            TestDebuggerSteps(
                "SteppingTest3.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 5),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 6), // step over f()
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            TestDebuggerSteps(
                "SteppingTest3.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 5),
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 1), // step into f()
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 6),  // step out of f()
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            TestDebuggerSteps(
                "SteppingTest2.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 4),
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 1), // step into f()
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 2), // step over print 'hi'
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            TestDebuggerSteps(
                "SteppingTest2.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 4),
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 1), // step into f()
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 2), // step over print 'hi'
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 5), // step over end }
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            TestDebuggerSteps(
                "SteppingTest.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 1), // step over print "hello"
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 2), // step over print "goodbye"
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        #endregion

        #region Startup Tests

        // F5 startup
        [TestMethod, Priority(0)]
        public void Startup_NoBreakOnEntryPoint() {
            TestDebuggerSteps(
                "BreakpointTest.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread),
                }
            );
        }

        // F10/F11 startup
        [TestMethod, Priority(0)]
        public void Startup_BreakOnEntryPoint() {
            TestDebuggerSteps(
                "BreakpointTest.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        // F5/F10/F11 startup
        [TestMethod, Priority(0)]
        public void Startup_BreakOnEntryPointBreakPoint() {
            TestDebuggerSteps(
                "BreakpointTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 0),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        // F5 startup
        [TestMethod, Priority(0)]
        public void Startup_NoBreakOnEntryPointTracePoint() {
            TestDebuggerSteps(
                "BreakpointTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 0),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 0),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread),
                }
            );
        }

        // F10/F11 startup
        [TestMethod, Priority(0)]
        public void Startup_BreakOnEntryPointTracePoint() {
            TestDebuggerSteps(
                "BreakpointTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 0),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 0),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        // F5 startup
        [TestMethod, Priority(0)]
        public void Startup_NoBreakOnEntryPointBreakOn() {
            TestDebuggerSteps(
                "BreakpointTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 0, breakOn: new BreakOn(BreakOnKind.Equal, 2)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread),
                }
            );
        }

        // F5 startup
        [TestMethod, Priority(0)]
        public void Startup_BreakOnEntryPointBreakOn() {
            TestDebuggerSteps(
                "BreakpointTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 0, breakOn: new BreakOn(BreakOnKind.Equal, 2)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        #endregion

        #region Running Tests

        // Covered by Startup_NoBreakOnEntryPoint()
        //[TestMethod, Priority(0)]
        //public void Running_Simple() { }

        [TestMethod, Priority(0)]
        public void Running_AccrossBreakPoint() {
            TestDebuggerSteps(
                "BreakpointTest3.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 1),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 4),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 1),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void Running_AccrossTracePoint() {
            TestDebuggerSteps(
                "BreakpointTest3.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 1),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 4),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 1),
                    new TestStep(action: TestAction.ResumeThread),
                }
            );
        }

        #endregion

        #region Stepping Tests

        [TestMethod, Priority(0)]
        public void Stepping_Basic() {
            TestDebuggerSteps(
                "SteppingBasic.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 9),

                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 10),    // Step over
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 11),    // Step over
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 1),     // Step into
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 2),     // Step over
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 13),    // Step over (out)

                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 14),    // Step into (over)
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 5),     // Step into
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 6),     // Step into (over)
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 1),     // Step into
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 2),     // Step into (over)
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 7),     // Step into (out)
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 16),    // Step into (out)

                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 5),     // Step into
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 6),     // Step into (over)
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 1),     // Step into
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 7),      // Step out
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 18),     // Step out

                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void Stepping_AccrossBreakPoints() {
            TestDebuggerSteps(
                "SteppingAccrossBreakPoints.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 1),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 5),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 6),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 7),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 10),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 12),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 13),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 15),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 16),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 18),

                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 9),

                    new TestStep(action: TestAction.StepOver, expectedBreakpointHit: 10),   // Step over to breakpoint
                    new TestStep(action: TestAction.StepOver, expectedBreakpointHit: 5),    // Step over (into) breakpoint
                    new TestStep(action: TestAction.StepOver, expectedBreakpointHit: 6),    // Step over to breakpoint
                    new TestStep(action: TestAction.StepOver, expectedBreakpointHit: 1),    // Step over to (into) nested breakpoint
                    new TestStep(action: TestAction.StepOver, expectedBreakpointHit: 2),    // Step over to breakpoint
                    new TestStep(action: TestAction.StepOver, expectedBreakpointHit: 7),    // Step over (out) to breakpoint
                    new TestStep(action: TestAction.StepOver, expectedBreakpointHit: 12),   // Step over (out) to breakpoint

                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 13),   // Step into (over to) breakpoint
                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 5),    // Step into breakpoint
                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 6),    // Step into (over to) breakpoint
                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 1),    // Step into breakpoint
                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 2),    // Step into (over to) breakpoint
                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 7),    // Step into (out to) breakpoint
                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 15),   // Step into (out to) breakpoint

                    new TestStep(action: TestAction.StepOut, expectedBreakpointHit: 16),    // Step out (over) to breakpoint
                    new TestStep(action: TestAction.StepOut, expectedBreakpointHit: 5),     // Step out to (into) breakpoint
                    new TestStep(action: TestAction.StepOut, expectedBreakpointHit: 6),     // Step out (over) to breakpoint
                    new TestStep(action: TestAction.StepOut, expectedBreakpointHit: 1),     // Step out to (into) breakpoint
                    new TestStep(action: TestAction.StepOut, expectedBreakpointHit: 2),     // Step out (over) to breakpoint
                    new TestStep(action: TestAction.StepOut, expectedBreakpointHit: 7),     // Step out to breakpoint
                    new TestStep(action: TestAction.StepOut, expectedBreakpointHit: 18),    // Step out to breakpoint

                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void Stepping_AccrossTracePoints() {
            TestDebuggerSteps(
                "SteppingAccrossTracePoints.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 1),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 5),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 6),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 7),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 10),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 12),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 13),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 15),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 17),

                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 9),

                    new TestStep(action: TestAction.StepOver, expectedBreakpointHit: 10),       // Step over to tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 10),    // Step over complete
                    new TestStep(action: TestAction.StepOver, expectedBreakpointHit: 1),        // Step over (accross) tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 2),    // Step over (accross) tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 12),   // Step over to tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 12),    // Step over complete

                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 13),       // Step into (over to) tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 13),    // Step into complete
                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 5),        // Step into tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 5),     // Step into complete
                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 6),        // Step into (over to) tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 6),     // Step into (over) complete
                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 1),        // Step into tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 1),     // Step into complete
                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 2),        // Step into (over to) tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 2),     // Step into (over) complete
                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 7),        // Step into (out to) tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 7),     // Step into (out) complete
                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 15),       // Step into (out to) tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 15),    // Step into (out) complete

                    new TestStep(action: TestAction.StepInto, expectedBreakpointHit: 5),        // Step into tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 5),     // Step into complete
                    new TestStep(action: TestAction.StepOut, expectedBreakpointHit: 6),         // Step out (accross) tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 1),    // Step out (accross) tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 2),    // Step out (accross) tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 7),    // Step out (accross) tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 17),   // Step out (accross) tracepoint
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 17),    // Step out complete

                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void Stepping_AcrossCaughtExceptions() {
            TestDebuggerSteps(
                "SteppingAcrossCaughtExceptions.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 11),

                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 17),

                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 11),
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 2),
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 12),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 13),
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 18),

                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                },
                defaultExceptionTreatment: ExceptionHitTreatment.BreakNever,
                exceptionTreatments: CollectExceptionTreatments("Error", ExceptionHitTreatment.BreakNever)
            );
        }
        [TestMethod, Priority(0)]
        public void DebuggingDownloaded() {
            TestDebuggerSteps(
                "DebuggingDownloaded.js",
                new[] {
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 204, expectedBreakFile: "node.js", builtin: true),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 205, expectedBreakFile: "node.js", builtin: true),
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 0),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpointFile: "console.js", targetBreakpoint: 52, builtin: true),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 52, expectedBreakFile: "console.js", builtin: true, validation:
                        async (process, thread) => {
                            var module = thread.Frames[0].Module;

                            // User data
                            var obj = new object();
                            module.Document = obj;
                            Assert.AreEqual(obj, module.Document);
                            module.Document = null;
                            Assert.AreEqual(null, module.Document);

                            // Download builtin
                            Assert.IsTrue(module.BuiltIn);
                            var scriptText = await process.GetScriptTextAsync(module.ModuleId);
                            Assert.IsTrue(scriptText.Contains("function Console("));

                            // Download non-builtin
                            module = thread.Frames[2].Module;
                            Assert.IsFalse(module.BuiltIn);
                            scriptText = await process.GetScriptTextAsync(module.ModuleId);
                            StreamReader streamReader = new StreamReader(module.FileName);
                            var fileText = streamReader.ReadToEnd();
                            streamReader.Close();
                            Assert.IsTrue(scriptText.Contains(fileText));
                        }
                    ),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }


        [TestMethod, Priority(0)]
        public void Breaking_InFunctionPassedFewerThanTakenParms() {
            TestDebuggerSteps(
                "FunctionPassedFewerThanTakenParms.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 1),

                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 4),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 1),

                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void Stepping_IntoFunctionPassedFewerThanTakenParms() {
            TestDebuggerSteps(
                "FunctionPassedFewerThanTakenParms.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 4),

                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 1),

                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        #endregion

        #region Breakpoint Tests

        [TestMethod, Priority(0)]
        public void CannonicalHelloWorldTest() {
            AutoResetEvent textRead = new AutoResetEvent(false);
            TestDebuggerSteps(
                "HelloWorld.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 3),
                    new TestStep(action: TestAction.ResumeProcess),
                    new TestStep(validation: (process, thread) => {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(stateinfo => {
                            var req = (HttpWebRequest)WebRequest.Create("http://localhost:1337/");
                            var resp = (HttpWebResponse)req.GetResponse();
                            var stream = resp.GetResponseStream();
                            var reader = new StreamReader(stream);
                            var text = reader.ReadToEnd();
                            Assert.AreEqual("Hello World\n", text);
                            textRead.Set();
                        }));
                    }),
                    new TestStep(action: TestAction.Wait, expectedBreakpointHit: 3),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 4),
                    new TestStep(action: TestAction.ResumeProcess),
                    new TestStep(validation: (process, thread) => {
                        AssertWaited(textRead);
                    }),
                    new TestStep(action: TestAction.KillProcess),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void BreakOnFixedUpBreakpoint() {
            AutoResetEvent textRead = new AutoResetEvent(false);
            TestDebuggerSteps(
                "HelloWorldWithClosure.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 6, expectFailure: true),
                    new TestStep(action: TestAction.ResumeProcess),
                    new TestStep(validation: (process, thread) => {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(stateinfo => {
                            var req = (HttpWebRequest)WebRequest.Create("http://localhost:1337/");
                            var resp = (HttpWebResponse)req.GetResponse();
                            var stream = resp.GetResponseStream();
                            var reader = new StreamReader(stream);
                            var text = reader.ReadToEnd();
                            Assert.AreEqual("Hello World\n", text);
                            textRead.Set();
                        }));
                    }),
                    new TestStep(action: TestAction.Wait, expectedBreakpointHit: 6),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 7),
                    new TestStep(action: TestAction.ResumeProcess),
                    new TestStep(validation: (process, thread) => {
                        AssertWaited(textRead);
                    }),
                    new TestStep(action: TestAction.KillProcess),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void SetBreakpointWhileRunning() {
            TestDebuggerSteps(
                "RunForever.js",
                new[] {
                    new TestStep(action: TestAction.ResumeProcess),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 1),
                    new TestStep(action: TestAction.Wait, expectedBreakpointHit: 1),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 1),
                    new TestStep(action: TestAction.KillProcess),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void TestBreakpoints() {
            TestDebuggerSteps(
                "BreakpointTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 0),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void TestBreakpoints2() {
            TestDebuggerSteps(
                "BreakpointTest2.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void TestBreakpoints3() {
            TestDebuggerSteps(
                "BreakpointTest3.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 1),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 4),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 1),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointsConditionals() {
            TestDebuggerSteps(
                "BreakpointTest2.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, condition: "i == 1"),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                "BreakpointTest2.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, condition: "i < 3"),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                "BreakpointTest2.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, condition: "i > 3"),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointEnable() {
            TestDebuggerSteps(
                "BreakpointTest2.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, enabled: false),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 6, enabled: false),    // Should never hit
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.UpdateBreakpoint, targetBreakpoint: 2, enabled: true),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2),
                    new TestStep(action: TestAction.UpdateBreakpoint, targetBreakpoint: 2, enabled: false),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointRemove() {
            TestDebuggerSteps(
                "BreakpointTest2.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2),
                    new TestStep(action: TestAction.RemoveBreakpoint, targetBreakpoint: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        //[TestMethod, Priority(0)]
        //public void TestBreakpointFailed() {
        //    var process =
        //        DebugProcess(
        //            DebuggerTestPath + "BreakpointTest.js",
        //            resumeOnProcessLoad: false);

        //    AutoResetEvent breakpointBindSuccess = new AutoResetEvent(false);
        //    AutoResetEvent breakpointBindFailure = new AutoResetEvent(false);

        //    AddBreakPoint(
        //        process,
        //        "BreakpointTest.js",
        //        1000,
        //        successHandler: () => {
        //            breakpointBindSuccess.Set();
        //        },
        //        failureHandler: () => {
        //            breakpointBindFailure.Set();
        //        }
        //    );
        //    AssertWaited(breakpointBindFailure);
        //    AssertNotSet(breakpointBindSuccess);

        //    process.Resume();
        //    process.WaitForExit();
        //}

        [TestMethod, Priority(0)]
        public void TestBreakpointsBreakOn() {
            // BreakOnKind.Always
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 1),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 3),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 4),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 5),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 7),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 8),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 9),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 10),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // BreakOnKind.Equal
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.Equal, 1)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 1),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.Equal, 10)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 10),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.Equal, 11)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // BreakOnKind.GreaterThanOrEqual
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.GreaterThanOrEqual, 1)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 1),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 3),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 4),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 5),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 7),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 8),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 9),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 10),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.GreaterThanOrEqual, 10)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 10),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.GreaterThanOrEqual, 11)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // BreakOnKind.Mod
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.Mod, 1)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 1),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 3),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 4),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 5),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 7),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 8),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 9),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 10),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.Mod, 5)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 5),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 10),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.Mod, 10)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 10),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.Mod, 11)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // Updates
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.Always, 0)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 1),
                    new TestStep(action: TestAction.UpdateBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.Equal, 3)),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 3),
                    new TestStep(action: TestAction.UpdateBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.GreaterThanOrEqual, 5)),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 5),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 6),
                    new TestStep(action: TestAction.UpdateBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.Mod, 3)),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 9),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // Invalid BreakOn
            bool exceptionHit = false;
            try {
                new BreakOn(BreakOnKind.Equal, 0);
            }
            catch (ArgumentException e) {
                Assert.AreEqual("Invalid BreakOn count", e.Message);
                exceptionHit = true;
            }
            Assert.IsTrue(exceptionHit);
            exceptionHit = false;
            try {
                new BreakOn(BreakOnKind.GreaterThanOrEqual, 0);
            }
            catch (ArgumentException e) {
                Assert.AreEqual("Invalid BreakOn count", e.Message);
                exceptionHit = true;
            }
            Assert.IsTrue(exceptionHit);
            exceptionHit = false;
            try {
                new BreakOn(BreakOnKind.Mod, 0);
            }
            catch (ArgumentException e) {
                Assert.AreEqual("Invalid BreakOn count", e.Message);
                exceptionHit = true;
            }
            Assert.IsTrue(exceptionHit);
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointsHitCount() {
            TestDebuggerSteps(
                "BreakpointBreakOn.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, breakOn: new BreakOn(BreakOnKind.Mod, 3)),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 3),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 1, targetBreakpoint: 2, expectedHitCount: 3),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 2, targetBreakpoint: 2, expectedHitCount: 4),
                    new TestStep(action: TestAction.UpdateBreakpoint, targetBreakpoint: 2, hitCount: 0),
                    new TestStep(action: TestAction.None, targetBreakpoint: 2, expectedHitCount: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 3),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 2, targetBreakpoint: 2, expectedHitCount: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointInvalidLineFixup() {
            TestDebuggerSteps(
                "FixupBreakpointOnComment.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 0, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 3, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 6, expectFailure: true),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 1),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 1),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 4),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 9),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                "FixupBreakpointOnBlankLine.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 0, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 3, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 6, expectFailure: true),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 2),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 5),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 10),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                "FixupBreakpointOnFunction.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 1),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 5, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 10, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 15),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 19, expectFailure: true),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 24),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 1),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 12),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 15),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 21),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                "RequiresScriptsWithBreakpointFixup.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpointFile: "FixupBreakpointOnComment.js", targetBreakpoint: 0, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpointFile: "FixupBreakpointOnComment.js", targetBreakpoint: 3, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpointFile: "FixupBreakpointOnComment.js", targetBreakpoint: 6, expectFailure: true),

                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpointFile: "FixupBreakpointOnBlankLine.js", targetBreakpoint: 0, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpointFile: "FixupBreakpointOnBlankLine.js", targetBreakpoint: 3, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpointFile: "FixupBreakpointOnBlankLine.js", targetBreakpoint: 6, expectFailure: true),

                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpointFile: "FixupBreakpointOnFunction.js", targetBreakpoint: 1, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpointFile: "FixupBreakpointOnFunction.js", targetBreakpoint: 5, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpointFile: "FixupBreakpointOnFunction.js", targetBreakpoint: 10, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpointFile: "FixupBreakpointOnFunction.js", targetBreakpoint: 15, expectFailure: true),
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpointFile: "FixupBreakpointOnFunction.js", targetBreakpoint: 19, expectFailure: true),

                    new TestStep(action: TestAction.ResumeProcess, expectedBreakFile: "FixupBreakpointOnComment.js", expectedBreakpointHit: 1),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakFile: "FixupBreakpointOnComment.js", expectedBreakpointHit: 4),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakFile: "FixupBreakpointOnComment.js", expectedBreakpointHit: 9),

                    new TestStep(action: TestAction.ResumeProcess, expectedBreakFile: "FixupBreakpointOnBlankLine.js", expectedBreakpointHit: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakFile: "FixupBreakpointOnBlankLine.js", expectedBreakpointHit: 5),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakFile: "FixupBreakpointOnBlankLine.js", expectedBreakpointHit: 10),

                    new TestStep(action: TestAction.ResumeProcess, expectedBreakFile: "FixupBreakpointOnFunction.js", expectedBreakpointHit: 1),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakFile: "FixupBreakpointOnFunction.js", expectedBreakpointHit: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakFile: "FixupBreakpointOnFunction.js", expectedBreakpointHit: 12),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakFile: "FixupBreakpointOnFunction.js", expectedBreakpointHit: 15),
                    new TestStep(action: TestAction.ResumeProcess, expectedBreakFile: "FixupBreakpointOnFunction.js", expectedBreakpointHit: 21),

                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        private void TestBreakpointPredicatedEntrypoint(string filename, int targetBreakpoint, int expectedHit) {
            var expectFailure = (expectedHit != targetBreakpoint);
            // No predicates
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // No predicates, tracepoint
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // True hit count predicate
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, breakOn: new BreakOn(BreakOnKind.GreaterThanOrEqual, 1), expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // True hit count predicate, tracepoint
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, breakOn: new BreakOn(BreakOnKind.GreaterThanOrEqual, 1), expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // False hit count predicate
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, breakOn: new BreakOn(BreakOnKind.GreaterThanOrEqual, 2), expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // True condition predicate
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, condition: "0 == 0", expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // True condition predicate, tracepoint
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, condition: "0 == 0", expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // False condition predicate
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, condition: "0 != 0", expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // True hit count and condition predicate
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, breakOn: new BreakOn(BreakOnKind.GreaterThanOrEqual, 1), condition: "0 == 0", expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // True hit count and condition predicate, tracepoint
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, breakOn: new BreakOn(BreakOnKind.GreaterThanOrEqual, 1), condition: "0 == 0", expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // False hit count and condition predicate
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, breakOn: new BreakOn(BreakOnKind.GreaterThanOrEqual, 2), condition: "0 != 0", expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            // Mixed hit count and condition predicate
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, breakOn: new BreakOn(BreakOnKind.GreaterThanOrEqual, 1), condition: "0 != 0", expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: targetBreakpoint, breakOn: new BreakOn(BreakOnKind.GreaterThanOrEqual, 2), condition: "0 == 0", expectFailure: expectFailure),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: expectedHit),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointPredicatedEntrypointNoFixup() {
            TestBreakpointPredicatedEntrypoint("BreakpointTest.js", targetBreakpoint: 0, expectedHit: 0);
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointPredicatedEntrypointBlankLineFixup() {
            TestBreakpointPredicatedEntrypoint("FixupBreakpointOnBlankLine.js", targetBreakpoint: 0, expectedHit: 2);
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointPredicatedEntrypointCommentFixup() {
            TestBreakpointPredicatedEntrypoint("FixupBreakpointOnComment.js", targetBreakpoint: 0, expectedHit: 1);
        }
       
        #endregion

        #region Exception Tests

        [TestMethod, Priority(0)]
        public void TestExceptions() {
            // Well-known, handled
            // Implicit break always
            TestExceptions(
                DebuggerTestPath + @"WellKnownHandledException.js",
                ExceptionHitTreatment.BreakAlways,
                null,
                0,
                new ExceptionInfo("Error", "Error: Error description", 3)
            );

            // Explicit break always
            TestExceptions(
                DebuggerTestPath + @"WellKnownHandledException.js",
                ExceptionHitTreatment.BreakNever,
                CollectExceptionTreatments("Error", ExceptionHitTreatment.BreakAlways),
                0,
                new ExceptionInfo("Error", "Error: Error description", 3)
            );

            // Explicit break always (both)
            TestExceptions(
                DebuggerTestPath + @"WellKnownHandledException.js",
                ExceptionHitTreatment.BreakAlways,
                CollectExceptionTreatments("Error", ExceptionHitTreatment.BreakAlways),
                0,
                new ExceptionInfo("Error", "Error: Error description", 3)
            );

            // UNDONE test break on unhandled once supported
            //// Implicit break on unhandled
            //TestExceptions(
            //    DebuggerTestPath + @"WellKnownHandledException.js",
            //    ExceptionHitTreatment.BreakOnUnhandled,
            //    null,
            //    0
            //);

            //// Explicit break on unhandled
            //TestExceptions(
            //    DebuggerTestPath + @"WellKnownHandledException.js",
            //    ExceptionHitTreatment.BreakAlways,
            //    GetExceptionTreatments("Error", ExceptionHitTreatment.BreakOnUnhandled),
            //    0
            //);

            //// Explicit break on unhandled (both)
            //TestExceptions(
            //    DebuggerTestPath + @"WellKnownHandledException.js",
            //    ExceptionHitTreatment.BreakOnUnhandled,
            //    GetExceptionTreatments("Error", ExceptionHitTreatment.BreakOnUnhandled),
            //    0
            //);

            // Well-known, unhandled
            // Implicit break always
            TestExceptions(
                DebuggerTestPath + @"WellKnownUnhandledException.js",
                ExceptionHitTreatment.BreakAlways,
                null,
                8,
                new ExceptionInfo("Error", "Error: Error description", 2)
            );

            // Explicit break always
            TestExceptions(
                DebuggerTestPath + @"WellKnownUnhandledException.js",
                ExceptionHitTreatment.BreakNever,
                CollectExceptionTreatments("Error", ExceptionHitTreatment.BreakAlways),
                8,
                new ExceptionInfo("Error", "Error: Error description", 2)
            );

            // Explicit break always (both)
            TestExceptions(
                DebuggerTestPath + @"WellKnownUnhandledException.js",
                ExceptionHitTreatment.BreakAlways,
                CollectExceptionTreatments("Error", ExceptionHitTreatment.BreakAlways),
                8,
                new ExceptionInfo("Error", "Error: Error description", 2)
            );

            // Implicit break on unhandled
            TestExceptions(
                DebuggerTestPath + @"WellKnownUnhandledException.js",
                ExceptionHitTreatment.BreakOnUnhandled,
                null,
                8,
                new ExceptionInfo("Error", "Error: Error description", 2)
            );

            // Explicit break on unhandled
            TestExceptions(
                DebuggerTestPath + @"WellKnownUnhandledException.js",
                ExceptionHitTreatment.BreakNever,
                CollectExceptionTreatments("Error", ExceptionHitTreatment.BreakOnUnhandled),
                8,
                new ExceptionInfo("Error", "Error: Error description", 2)
            );

            // Explicit break on unhandled (both)
            TestExceptions(
                DebuggerTestPath + @"WellKnownUnhandledException.js",
                ExceptionHitTreatment.BreakOnUnhandled,
                CollectExceptionTreatments("Error", ExceptionHitTreatment.BreakOnUnhandled),
                8,
                new ExceptionInfo("Error", "Error: Error description", 2)
            );
        }

        [TestMethod, Priority(0)]
        public void TestExceptionsTypes() {
            TestExceptions(
                DebuggerTestPath + @"ExceptionTypes.js",
                ExceptionHitTreatment.BreakAlways,
                null,
                0,
                new ExceptionInfo("Error", "Error: msg", 3),
                new ExceptionInfo("ReferenceError", "ReferenceError: UndefinedVariable is not defined", 9),
                new ExceptionInfo("RangeError", "RangeError: Invalid array length", 15),
                new ExceptionInfo("TypeError", "TypeError: Object UserStringValue has no method 'UndefinedFunction'", 21),
                new ExceptionInfo("URIError", "URIError: URI malformed"),
                new ExceptionInfo("SyntaxError", "SyntaxError: Invalid regular expression: missing /"),
                new ExceptionInfo("EvalError", "EvalError: msg", 39),
                new ExceptionInfo("UserDefinedError", "UserDefinedError: msg", 53),
                new ExceptionInfo("UserDefinedRangeError", "UserDefinedRangeError: msg", 65),
                new ExceptionInfo("UserDefinedType", "[object Object]", 73),
                new ExceptionInfo(NodeVariableType.Number, "1", 82),
                new ExceptionInfo(NodeVariableType.String, "exception_string", 88),
                new ExceptionInfo(NodeVariableType.Boolean, "false", 94)
            );
        }

        [TestMethod, Priority(0)]
        public void TestComplexExceptions() {
            TestExceptions(
                DebuggerTestPath + @"ComplexExceptions.js",
                ExceptionHitTreatment.BreakAlways,
                null,
                0,
                new ExceptionInfo("UserDefinedClass", "[object Object]", 5),
                new ExceptionInfo("TypeError", "TypeError: TypeError description", 13),
                new ExceptionInfo("ReferenceError", "ReferenceError: ReferenceError description", 16),
                new ExceptionInfo("TypeError", "TypeError: TypeError description", 24),
                new ExceptionInfo("ReferenceError", "ReferenceError: ReferenceError description", 27),
                new ExceptionInfo("Error", "Error: Error description", 34)
            );
        }

        /// <summary>
        /// http://nodejstools.codeplex.com/workitem/63
        /// 
        /// Exceptions shouldnt't be reported while calling Require, for alpha this means we don't break on ENOENT
        /// by default.
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestRequireExceptions() {
            TestExceptions(
                DebuggerTestPath + @"RequireExceptions.js",
                null,
                null,
                0
            );
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/379
        /// 
        /// Test handling of exceptions in evaluated code
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestExceptionInEvaluatedCode() {
            TestDebuggerSteps(
                "ExceptionInEvaluatedCode.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 1),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 2),
                    new TestStep(action: TestAction.StepOver, expectedExceptionRaised: new ExceptionInfo("SyntaxError", "SyntaxError: Unexpected token )", 0)),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 4),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 6),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        #endregion

        #region Deep Callstack Tests

        [TestMethod, Priority(0)]
        public void Stepping_AccrossDeepThrow() {
            TestDebuggerSteps(
                "ThrowsWithDeepCallstack.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 11),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 13),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                },
                defaultExceptionTreatment: ExceptionHitTreatment.BreakAlways,
                exceptionTreatments: CollectExceptionTreatments(NodeVariableType.String, ExceptionHitTreatment.BreakNever)
            );
        }

        [TestMethod, Priority(0)]
        public void Stepping_AccrossDeepTracePoint() {
            TestDebuggerSteps(
                "DeepCallstack.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 3),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 15),
                    new TestStep(action: TestAction.StepOver, expectedBreakpointHit: 3),
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 17),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void Stepping_AccrossDeepFixedUpTracePoint() {
            TestDebuggerSteps(
                "DeepCallstack.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 2, expectFailure: true),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 15),
                    new TestStep(action: TestAction.StepOver, expectedBreakpointHit: 3),
                    new TestStep(action: TestAction.ResumeThread, expectedStepComplete: 17),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        #endregion

        #region Module Load Tests

        [TestMethod, Priority(0)]
        public void TestModuleLoad() {
            TestModuleLoad(
                "NoRequires.js",
                "NoRequires.js"
            );

            TestModuleLoad(
                "HasRequires.js",
                "HasRequires.js",
                "IsRequired.js"
            );
        }

        private void TestModuleLoad(string filename, params string[] expectedModulesLoaded) {
            List<string> receivedFilenames = new List<string>();
            TestDebuggerSteps(
                filename,
                new[] {
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0)
                },
                onProcessCreated: newProcess => {
                    newProcess.ModuleLoaded += (sender, args) => {
                        receivedFilenames.Add(args.Module.FileName);
                    };
                }
            );

            Assert.IsTrue(receivedFilenames.Count >= expectedModulesLoaded.Length);
            var set = new HashSet<string>();
            foreach (var received in receivedFilenames) {
                set.Add(Path.GetFileName(received));
            }

            foreach (var file in expectedModulesLoaded) {
                Assert.IsTrue(set.Contains(file));
            }
        }

        private object DebugProcess(string filename, object onProcessCreated) {
            throw new NotImplementedException();
        }

        #endregion

        #region Exit Code Tests

        [TestMethod, Priority(0)]
        public void TestExitNormal() {
            TestDebuggerSteps(
                "ExitNormal.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0)]
        public void TestExitException() {
            TestDebuggerSteps(
                "ExitException.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExceptionRaised: new ExceptionInfo("Error", "Error: msg", 1)),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 8),
                }
            );

        }

        [TestMethod, Priority(0)]
        public void TestExitExplicit() {
            TestDebuggerSteps(
                "ExitExplicit.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 42),
                }
            );
        }

        #endregion

        #region Argument Tests

        [TestMethod, Priority(0)]
        public void TestInterpreterArguments() {
            TestDebuggerSteps(
                "PassedArgs.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExceptionRaised: new ExceptionInfo("Error", "Error: Invalid args", 3)),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 8),
                }
            );
            TestDebuggerSteps(
                "PassedArgs.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                },
                interpreterOptions: "42"
            );
        }

        #endregion

        [TestMethod, Priority(0)]
        public void TestDuplicateFileName() {
            TestDebuggerSteps(
                "DuppedFilename.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, 
                        targetBreakpoint: 2, 
                        targetBreakpointFile: "Directory\\DuppedFilename.js",
                        expectFailure:true),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread, 
                        expectedHitCount: 1, 
                        targetBreakpoint: 2, 
                        targetBreakpointFile: "Directory\\DuppedFilename.js", 
                        expectedBreakFunction: "f",
                        expectedBreakpointHit: 2),
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 4),               
                }
            );
        }

        #region Attach Tests

        [TestMethod, Priority(0)]
        public void LocalAttach() {
            var filename = "RunForever.js";

            using (var sysProcess = StartNodeProcess(filename)) {

                for (var i = 0; i < 3; ++i) {
                    var process = AttachToNodeProcess(id: sysProcess.Id);
                    var thread = process.GetThreads().First();
                    TestDebuggerSteps(
                        process,
                        thread,
                        filename,
                        new[] {
                            new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 1),
                            new TestStep(action: TestAction.Wait, expectedBreakpointHit: 1),
                            new TestStep(action: TestAction.ResumeProcess, expectedBreakpointHit: 1),
                            new TestStep(action: TestAction.Detach),
                        }
                    );
                }

                sysProcess.Kill();
            }
        }



//        /// <summary>
//        /// threading module imports thread.start_new_thread, verifies that we patch threading's method
//        /// in addition to patching the thread method so that breakpoints on threads created after
//        /// attach via the threading module can be hit.
//        /// </summary>
//        [TestMethod, Priority(0)]
//        public void AttachThreadingStartNewThread() {
//            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach
//                // http://pytools.codeplex.com/workitem/638
//                // http://pytools.codeplex.com/discussions/285741#post724014
//                var psi = new ProcessStartInfo(NodePath, "\"" + TestData.GetPath(@"TestData\DebuggerProject\ThreadingStartNewThread.py") + "\"");
//                psi.WorkingDirectory = TestData.GetPath(@"TestData\DebuggerProject");
//                Process p = Process.Start(psi);
//                System.Threading.Thread.Sleep(1000);

//                AutoResetEvent attached = new AutoResetEvent(false);
//                AutoResetEvent breakpointHit = new AutoResetEvent(false);

//                NodeDebugger proc;
//                ConnErrorMessages errReason;
//                if ((errReason = NodeDebugger.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
//                    Assert.Fail("Failed to attach {0}", errReason);
//                }

//                proc.ProcessLoaded += (sender, args) => {
//                    attached.Set();
//                    var bp = proc.AddBreakPoint("ThreadingStartNewThread.py", 9);
//                    bp.Add();

//                    bp = proc.AddBreakPoint("ThreadingStartNewThread.py", 5);
//                    bp.Add();

//                    proc.Resume();
//                };
//                NodeThread mainThread = null;
//                NodeThread bpThread = null;
//                bool wrongLine = false;
//                proc.BreakpointHit += (sender, args) => {
//                    if (args.Breakpoint.LineNo == 9) {
//                        // stop running the infinite loop
//                        Debug.WriteLine(String.Format("First BP hit {0}", args.Thread.Id));
//                        args.Thread.Frames[0].ExecuteTextAsync("x = False", (x) => {});
//                        mainThread = args.Thread;
//                    } else if (args.Breakpoint.LineNo == 5) {
//                        // we hit the breakpoint on the new thread
//                        Debug.WriteLine(String.Format("Second BP hit {0}", args.Thread.Id));
//                        breakpointHit.Set();
//                        bpThread = args.Thread;
//                    } else {
//                        Debug.WriteLine(String.Format("Hit breakpoint on wrong line number: {0}", args.Breakpoint.LineNo));
//                        wrongLine = true;
//                        attached.Set();
//                        breakpointHit.Set();
//                    }
//                    proc.Resume();
//                };
//                proc.StartListening();

//                Assert.IsTrue(attached.WaitOne(10000));
//                Assert.IsTrue(breakpointHit.WaitOne(10000));
//                Assert.IsFalse(wrongLine);

//                Assert.AreNotEqual(mainThread, bpThread);
//                proc.Detach();

//                p.Kill();
//            }
//        }


//        [TestMethod, Priority(0)]
//        public void AttachReattach() {
//            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach
//                Process p = Process.Start(NodePath, "\"" + TestData.GetPath(@"TestData\DebuggerProject\InfiniteRun.py") + "\"");
//                System.Threading.Thread.Sleep(1000);

//                AutoResetEvent attached = new AutoResetEvent(false);
//                AutoResetEvent detached = new AutoResetEvent(false);
//                for (int i = 0; i < 10; i++) {
//                    Console.WriteLine(i);

//                    NodeDebugger proc;
//                    ConnErrorMessages errReason;
//                    if ((errReason = NodeDebugger.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
//                        Assert.Fail("Failed to attach {0}", errReason);
//                    }

//                    proc.ProcessLoaded += (sender, args) => {
//                        attached.Set();
//                    };
//                    proc.ProcessExited += (sender, args) => {
//                        detached.Set();
//                    };
//                    proc.StartListening();

//                    Assert.IsTrue(attached.WaitOne(10000));
//                    proc.Detach();
//                    Assert.IsTrue(detached.WaitOne(10000));
//                }

//                p.Kill();
//            }
//        }

//        /// <summary>
//        /// When we do the attach one thread is blocked in native code.  We attach, resume execution, and that
//        /// thread should eventually wake up.  
//        /// 
//        /// The bug was two issues, when doing a resume all:
//        ///		1) we don't clear the stepping if it's STEPPING_ATTACH_BREAK
//        ///		2) We don't clear the stepping if we haven't yet blocked the thread
//        ///		
//        /// Because the thread is blocked in native code, and we don't clear the stepping, when the user
//        /// hits resume the thread will eventually return back to Python code, and then we'll block it
//        /// because we haven't cleared the stepping bit.
//        /// </summary>
//        [TestMethod, Priority(0)]
//        public void AttachMultithreadedSleeper() {
//            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach
//                // http://pytools.codeplex.com/discussions/285741 1/12/2012 6:20 PM
//                Process p = Process.Start(NodePath, "\"" + TestData.GetPath(@"TestData\DebuggerProject\AttachMultithreadedSleeper.py") + "\"");
//                System.Threading.Thread.Sleep(1000);

//                AutoResetEvent attached = new AutoResetEvent(false);

//                NodeDebugger proc;
//                ConnErrorMessages errReason;
//                if ((errReason = NodeDebugger.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
//                    Assert.Fail("Failed to attach {0}", errReason);
//                }

//                proc.ProcessLoaded += (sender, args) => {
//                    attached.Set();
//                };
//                proc.StartListening();

//                Assert.IsTrue(attached.WaitOne(10000));
//                proc.Resume();
//                Debug.WriteLine("Waiting for exit");
//                Assert.IsTrue(proc.WaitForExit(20000));
//            }
//        }

//        /// <summary>
//        /// Python 3.2 changes the rules about when we can call Py_InitThreads.
//        /// 
//        /// http://pytools.codeplex.com/workitem/834
//        /// </summary>
//        [TestMethod, Priority(0)]
//        public void AttachSingleThreadedSleeper() {
//            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach
//                // http://pytools.codeplex.com/discussions/285741 1/12/2012 6:20 PM
//                Process p = Process.Start(NodePath, "\"" + TestData.GetPath(@"TestData\DebuggerProject\AttachSingleThreadedSleeper.py") + "\"");
//                System.Threading.Thread.Sleep(1000);

//                AutoResetEvent attached = new AutoResetEvent(false);

//                NodeDebugger proc;
//                ConnErrorMessages errReason;
//                if ((errReason = NodeDebugger.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
//                    Assert.Fail("Failed to attach {0}", errReason);
//                }

//                proc.ProcessLoaded += (sender, args) => {
//                    attached.Set();
//                };
//                proc.StartListening();

//                Assert.IsTrue(attached.WaitOne(10000));
//                proc.Resume();
//                Debug.WriteLine("Waiting for exit");
//                proc.Terminate();
//            }
//        }

//        [TestMethod, Priority(0)]
//        public void AttachReattachThreadingInited() {
//            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython shouldn't support attach
//                Process p = Process.Start(NodePath, "\"" + TestData.GetPath(@"TestData\DebuggerProject\InfiniteRunThreadingInited.py") + "\"");
//                System.Threading.Thread.Sleep(1000);

//                AutoResetEvent attached = new AutoResetEvent(false);
//                AutoResetEvent detached = new AutoResetEvent(false);
//                for (int i = 0; i < 10; i++) {
//                    Console.WriteLine(i);

//                    NodeDebugger proc;
//                    ConnErrorMessages errReason;
//                    if ((errReason = NodeDebugger.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
//                        Assert.Fail("Failed to attach {0}", errReason);
//                    }

//                    proc.ProcessLoaded += (sender, args) => {
//                        attached.Set();
//                    };
//                    proc.ProcessExited += (sender, args) => {
//                        detached.Set();
//                    };
//                    proc.StartListening();

//                    Assert.IsTrue(attached.WaitOne(10000));
//                    proc.Detach();
//                    Assert.IsTrue(detached.WaitOne(10000));
//                }

//                p.Kill();
//            }
//        }

//        [TestMethod, Priority(0)]
//        public void AttachReattachInfiniteThreads() {
//            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython shouldn't support attach
//                Process p = Process.Start(NodePath, "\"" + TestData.GetPath(@"TestData\DebuggerProject\InfiniteThreads.py") + "\"");
//                System.Threading.Thread.Sleep(1000);

//                AutoResetEvent attached = new AutoResetEvent(false);
//                AutoResetEvent detached = new AutoResetEvent(false);
//                for (int i = 0; i < 10; i++) {
//                    Console.WriteLine(i);

//                    NodeDebugger proc;
//                    ConnErrorMessages errReason;
//                    if ((errReason = NodeDebugger.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
//                        Assert.Fail("Failed to attach {0}", errReason);
//                    }

//                    proc.ProcessLoaded += (sender, args) => {
//                        attached.Set();
//                    };
//                    proc.ProcessExited += (sender, args) => {
//                        detached.Set();
//                    };
//                    proc.StartListening();

//                    Assert.IsTrue(attached.WaitOne(20000));
//                    proc.Detach();
//                    Assert.IsTrue(detached.WaitOne(20000));

//                }

//                p.Kill();
//            }
//        }

//        [TestMethod, Priority(0)]
//        public void AttachTimeout() {
//            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach

//                string cast = "(PyCodeObject*)";
//                if (Version.Version >= NodeLanguageVersion.V32) {
//                    // 3.2 changed the API here...
//                    cast = "";
//                }

//                var hostCode = @"#include <python.h>
//#include <windows.h>
//#include <stdio.h>
//
//int main(int argc, char* argv[]) {
//    Py_Initialize();
//    auto event = OpenEventA(EVENT_ALL_ACCESS, FALSE, argv[1]);
//    if(!event) {
//        printf(""Failed to open event\r\n"");
//    }
//    printf(""Waiting for event\r\n"");
//    if(WaitForSingleObject(event, INFINITE)) {
//        printf(""Wait failed\r\n"");
//    }
//
//    auto loc = PyDict_New ();
//    auto glb = PyDict_New ();
//
//    auto src = " + cast + @"Py_CompileString (""while 1:\n    pass"", ""<stdin>"", Py_file_input);
//
//    if(src == nullptr) {
//        printf(""Failed to compile code\r\n"");
//    }
//    printf(""Executing\r\n"");
//    PyEval_EvalCode(src, glb, loc);
//}";
//                AttachTest(hostCode);
//            }
//        }

//        /// <summary>
//        /// Attempts to attach w/ code only running on new threads which are initialized using PyGILState_Ensure
//        /// </summary>
//        [TestMethod, Priority(0)]
//        public void AttachNewThread_PyGILState_Ensure() {
//            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach


//                File.WriteAllText("gilstate_attach.py", @"def test():
//    for i in range(10):
//        print(i)
//
//    return 0");

//                var hostCode = @"#include <Windows.h>
//#include <process.h>
//#undef _DEBUG
//#include <Python.h>
//
//PyObject *g_pFunc;
//
//void Thread(void*)
//{
//    printf(""Worker thread started %x\r\n"", GetCurrentThreadId());
//    while (true)
//    {
//        PyGILState_STATE state = PyGILState_Ensure();
//        PyObject *pValue;
//
//        pValue = PyObject_CallObject(g_pFunc, 0);
//        if (pValue != NULL) {
//            //printf(""Result of call: %ld\n"", PyInt_AsLong(pValue));
//            Py_DECREF(pValue);
//        } else {
//            PyErr_Print();
//            return;
//        }
//        PyGILState_Release(state);
//
//        Sleep(1000);
//    }
//}
//
//void main()
//{
//    PyObject *pName, *pModule;
//
//    Py_Initialize();
//    PyEval_InitThreads();
//    pName = CREATE_STRING(""gilstate_attach"");
//
//    pModule = PyImport_Import(pName);
//    Py_DECREF(pName);
//
//    if (pModule != NULL) {
//        g_pFunc = PyObject_GetAttrString(pModule, ""test"");
//
//        if (g_pFunc && PyCallable_Check(g_pFunc))
//        {
//            DWORD threadID;
//            threadID = _beginthread(&Thread, 1024*1024, 0);
//            threadID = _beginthread(&Thread, 1024*1024, 0);
//
//            PyEval_ReleaseLock();
//            while (true);
//        }
//        else
//        {
//            if (PyErr_Occurred())
//                PyErr_Print();
//        }
//        Py_XDECREF(g_pFunc);
//        Py_DECREF(pModule);
//    }
//    else
//    {
//        PyErr_Print();
//        return;
//    }
//    Py_Finalize();
//    return;
//}".Replace("CREATE_STRING", CreateString);
//                CompileCode(hostCode);

//                // start the test process w/ our handle
//                Process p = Process.Start("test.exe");

//                System.Threading.Thread.Sleep(1500);

//                AutoResetEvent attached = new AutoResetEvent(false);
//                AutoResetEvent bpHit = new AutoResetEvent(false);
//                NodeDebugger proc;
//                ConnErrorMessages errReason;
//                if ((errReason = NodeDebugger.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
//                    Assert.Fail("Failed to attach {0}", errReason);
//                } else {
//                    Console.WriteLine("Attached");
//                }

//                proc.ProcessLoaded += (sender, args) => {
//                    Console.WriteLine("Process loaded");
//                    attached.Set();
//                };
//                proc.StartListening();

//                Assert.IsTrue(attached.WaitOne(20000));

//                proc.BreakpointHit += (sender, args) => {
//                    Console.WriteLine("Breakpoint hit");
//                    bpHit.Set();
//                };

//                var bp = proc.AddBreakPoint("gilstate_attach.py", 3);
//                bp.Add();

//                Assert.IsTrue(bpHit.WaitOne(20000));
//                proc.Detach();

//                p.Kill();
//            }
//        }

//        /// <summary>
//        /// Attempts to attach w/ code only running on new threads which are initialized using PyThreadState_New
//        /// </summary>
//        [TestMethod, Priority(0)]
//        public void AttachNewThread_PyThreadState_New() {

//            if (GetType() != typeof(DebuggerTestsIpy) &&    // IronPython doesn't support attach
//                Version.Version <= NodeLanguageVersion.V31) {    // PyEval_AcquireLock deprecated in 3.2
//                File.WriteAllText("gilstate_attach.py", @"def test():
//    for i in range(10):
//        print(i)
//
//    return 0");

//                var hostCode = @"#include <Windows.h>
//#include <process.h>
//#undef _DEBUG
//#include <Python.h>
//
//PyObject *g_pFunc;
//
//void Thread(void*)
//{
//    printf(""Worker thread started %x\r\n"", GetCurrentThreadId());
//    while (true)
//    {
//        PyEval_AcquireLock();
//        PyInterpreterState* pMainInterpreterState = PyInterpreterState_Head();
//        auto pThisThreadState = PyThreadState_New(pMainInterpreterState);
//        PyThreadState_Swap(pThisThreadState);
//
//        PyObject *pValue;
//
//        pValue = PyObject_CallObject(g_pFunc, 0);
//        if (pValue != NULL) {
//            //printf(""Result of call: %ld\n"", PyInt_AsLong(pValue));
//            Py_DECREF(pValue);
//        } else {
//            PyErr_Print();
//            return;
//        }
//
//        PyThreadState_Swap(NULL);
//        PyThreadState_Clear(pThisThreadState);
//        PyThreadState_Delete(pThisThreadState);
//        PyEval_ReleaseLock();
//
//        Sleep(1000);
//    }
//}
//
//void main()
//{
//    PyObject *pName, *pModule;
//
//    Py_Initialize();
//    PyEval_InitThreads();
//    pName = CREATE_STRING(""gilstate_attach"");
//
//    pModule = PyImport_Import(pName);
//    Py_DECREF(pName);
//
//    if (pModule != NULL) {
//        g_pFunc = PyObject_GetAttrString(pModule, ""test"");
//
//        if (g_pFunc && PyCallable_Check(g_pFunc))
//        {
//            DWORD threadID;
//            threadID = _beginthread(&Thread, 1024*1024, 0);
//            threadID = _beginthread(&Thread, 1024*1024, 0);
//            PyEval_ReleaseLock();
//
//            while (true);
//        }
//        else
//        {
//            if (PyErr_Occurred())
//                PyErr_Print();
//        }
//        Py_XDECREF(g_pFunc);
//        Py_DECREF(pModule);
//    }
//    else
//    {
//        PyErr_Print();
//        return;
//    }
//    Py_Finalize();
//    return;
//}".Replace("CREATE_STRING", CreateString);
//                CompileCode(hostCode);

//                // start the test process w/ our handle
//                Process p = Process.Start("test.exe");

//                System.Threading.Thread.Sleep(1500);

//                AutoResetEvent attached = new AutoResetEvent(false);
//                AutoResetEvent bpHit = new AutoResetEvent(false);
//                NodeDebugger proc;
//                ConnErrorMessages errReason;
//                if ((errReason = NodeDebugger.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
//                    Assert.Fail("Failed to attach {0}", errReason);
//                } else {
//                    Console.WriteLine("Attached");
//                }

//                proc.ProcessLoaded += (sender, args) => {
//                    Console.WriteLine("Process loaded");
//                    attached.Set();
//                };
//                proc.StartListening();

//                Assert.IsTrue(attached.WaitOne(20000));

//                proc.BreakpointHit += (sender, args) => {
//                    Console.WriteLine("Breakpoint hit");
//                    bpHit.Set();
//                };

//                var bp = proc.AddBreakPoint("gilstate_attach.py", 3);
//                bp.Add();

//                Assert.IsTrue(bpHit.WaitOne(20000));
//                proc.Detach();

//                p.Kill();
//            }
//        }

//        public virtual string CreateString {
//            get {
//                return "PyString_FromString";
//            }
//        }

//        [TestMethod, Priority(0)]
//        public void AttachTimeoutThreadsInitialized() {
//            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach

//                string cast = "(PyCodeObject*)";
//                if (Version.Version >= NodeLanguageVersion.V32) {
//                    // 3.2 changed the API here...
//                    cast = "";
//                }


//                var hostCode = @"#include <python.h>
//#include <windows.h>
//
//int main(int argc, char* argv[]) {
//    Py_Initialize();
//    PyEval_InitThreads();
//
//    auto event = OpenEventA(EVENT_ALL_ACCESS, FALSE, argv[1]);
//    WaitForSingleObject(event, INFINITE);
//
//    auto loc = PyDict_New ();
//    auto glb = PyDict_New ();
//
//    auto src = " + cast + @"Py_CompileString (""while 1:\n    pass"", ""<stdin>"", Py_file_input);
//
//    if(src == nullptr) {
//        printf(""Failed to compile code\r\n"");
//    }
//    printf(""Executing\r\n"");
//    PyEval_EvalCode(src, glb, loc);
//}";
//                AttachTest(hostCode);

//            }
//        }

//        private void AttachTest(string hostCode) {
//            CompileCode(hostCode);

//            // start the test process w/ our handle
//            var eventName = Guid.NewGuid().ToString();
//            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.AutoReset, eventName);
//            ProcessStartInfo psi = new ProcessStartInfo("test.exe", eventName);
//            psi.UseShellExecute = false;
//            psi.RedirectStandardError = psi.RedirectStandardOutput = true;
//            psi.CreateNoWindow = true;

//            Process p = Process.Start(psi);
//            var outRecv = new OutputReceiver();
//            p.OutputDataReceived += outRecv.OutputDataReceived;
//            p.ErrorDataReceived += outRecv.OutputDataReceived;
//            p.BeginErrorReadLine();
//            p.BeginOutputReadLine();

//            try {
//                // start the attach with the GIL held
//                AutoResetEvent attached = new AutoResetEvent(false);
//                NodeDebugger proc;
//                ConnErrorMessages errReason;
//                if ((errReason = NodeDebugger.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
//                    Assert.Fail("Failed to attach {0}", errReason);
//                }

//                bool isAttached = false;
//                proc.ProcessLoaded += (sender, args) => {
//                    attached.Set();
//                    isAttached = false;
//                };
//                proc.StartListening();

//                Assert.AreEqual(false, isAttached); // we shouldn't have attached yet, we should be blocked
//                handle.Set();   // let the code start running

//                Assert.IsTrue(attached.WaitOne(20000));
//                proc.Detach();

//                p.Kill();
//            } finally {
//                Debug.WriteLine(String.Format("Process output: {0}", outRecv.Output.ToString()));
//            }
//        }

//        private void CompileCode(string hostCode) {
//            File.WriteAllText("test.cpp", hostCode);

//            // compile our host code...
//            var startInfo = new ProcessStartInfo(
//                Path.Combine(GetVCInstallDir(), "bin", "cl.exe"),
//                String.Format("/I{0}\\Include test.cpp /link /libpath:{0}\\libs", Path.GetDirectoryName(NodePath))
//            );
            
//            startInfo.EnvironmentVariables["PATH"] = Environment.GetEnvironmentVariable("PATH") + ";" + GetVSIDEInstallDir();
//            startInfo.EnvironmentVariables["INCLUDE"] = Path.Combine(GetVCInstallDir(), "INCLUDE") + ";" + Path.Combine(GetWindowsSDKDir(), "Include");
//            startInfo.EnvironmentVariables["LIB"] = Path.Combine(GetVCInstallDir(), "LIB") + ";" + Path.Combine(GetWindowsSDKDir(), "Lib");
//            Console.WriteLine(startInfo.EnvironmentVariables["LIB"]);

//            startInfo.UseShellExecute = false;
//            startInfo.RedirectStandardError = true;
//            startInfo.RedirectStandardOutput = true;
//            startInfo.CreateNoWindow = true;
//            var compileProcess = Process.Start(startInfo);

//            var outputReceiver = new OutputReceiver();
//            compileProcess.OutputDataReceived += outputReceiver.OutputDataReceived; // for debugging if you change the code...
//            compileProcess.ErrorDataReceived += outputReceiver.OutputDataReceived;
//            compileProcess.BeginErrorReadLine();
//            compileProcess.BeginOutputReadLine();
//            compileProcess.WaitForExit();

//            Assert.AreEqual(0, compileProcess.ExitCode, 
//                "Incorrect exit code: " + compileProcess.ExitCode + Environment.NewLine +
//                outputReceiver.Output.ToString()
//            );
//        }

//        private static string GetVCInstallDir() {
//            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\VisualStudio\\" + VSUtility.Version + "\\Setup\\VC")) {
//                return key.GetValue("ProductDir").ToString();
//            }
//        }

//        private static string GetWindowsSDKDir() {
//            string[] sdkVersions = new[] { "v7.0A", "v8.0A", "v7.0" };
//            object regValue = null;
//            foreach (var sdkVersion in sdkVersions) {
//                regValue = Registry.GetValue(
//                    "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Microsoft SDKs\\Windows\\" + sdkVersion,
//                    "InstallationFolder",
//                    null);

//                if (regValue != null && Directory.Exists(Path.Combine(regValue.ToString(), "Include"))) {
//                    break;
//                }
//            }
            
//            if (regValue == null) {
//                Assert.IsTrue(Directory.Exists("C:\\Program Files\\Microsoft SDKs\\Windows\\v7.0\\Include"), "Windows SDK is not installed");
//                return "C:\\Program Files\\Microsoft SDKs\\Windows\\v7.0";
//            }

//            return regValue.ToString();
//        }

//        private static string GetVSIDEInstallDir() {
//            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\VisualStudio\\" + VSUtility.Version + "\\Setup\\VS")) {
//                return key.GetValue("EnvironmentDirectory").ToString();
//            }
//        }

        #endregion

        #region Output Tests

        // NYI Disabled
        //[TestMethod, Priority(0)]
        public void Test3xStdoutBuffer() {
            bool gotOutput = false;
            var process =
                DebugProcess(
                    @"StdoutBuffer3x.py",
                    onLoadComplete: (processObj, threadObj) => {
                        processObj.DebuggerOutput += (sender, args) => {
                            Assert.IsTrue(!gotOutput, "got output more than once");
                            gotOutput = true;
                            Assert.AreEqual("foo", args.Output);
                        };
                    },
                    debugOptions: NodeDebugOptions.RedirectOutput
                );

            process.Start();
            process.WaitForExit();

            Assert.IsTrue(gotOutput, "failed to get output");
        }

        // NYI Disabled
        //[TestMethod, Priority(0)]
        public void TestInputFunction() {
            // 845 Python 3.3 Bad argument type for the debugger output wrappers
            // A change to the Python 3.3 implementation of input() now requires
            // that `errors` be set to a valid value on stdout. This test
            // ensures that calls to `input` continue to work.

            var expectedOutput = "Provide A: foo\n";
            string actualOutput = string.Empty;

            var process =
                DebugProcess(
                    @"InputFunction.py",
                    onLoadComplete: (processObj, threadObj) => {
                        processObj.DebuggerOutput += (sender, args) => {
                            actualOutput += args.Output;
                        };
                    },
                    debugOptions: NodeDebugOptions.RedirectOutput
                ); //debugOptions: NodeDebugOptions.RedirectOutput | NodeDebugOptions.RedirectInput);

            process.Start();
            Thread.Sleep(1000);
            //process.SendStringToStdInput("foo\n");
            process.WaitForExit();

            Assert.AreEqual(expectedOutput, actualOutput);
        }

        #endregion

        #region TypeScript Tests

        [TestMethod, Priority(0)]
        public void TypeScript_Stepping_Basic() {
            TestDebuggerSteps(
                "TypeScriptTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, 
                        targetBreakpoint: 1,
                        targetBreakpointColumn: 43,
                        targetBreakpointFile: "TypeScriptTest.ts"),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread, 
                        expectedHitCount: 1, 
                        targetBreakpoint: 1,
                        targetBreakpointColumn: 43,
                        targetBreakpointFile: "TypeScriptTest.ts", 
                        expectedBreakFunction: "Greeter.constructor",
                        expectedBreakpointHit: 1),
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 8),
                }
            );

            TestDebuggerSteps(
                "TypeScriptTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, 
                        targetBreakpoint: 7, 
                        targetBreakpointFile: "TypeScriptTest.ts"),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread, 
                        expectedHitCount: 1, 
                        targetBreakpoint: 7, 
                        targetBreakpointFile: "TypeScriptTest.ts", 
                        expectedBreakFunction: "Greeter",
                        expectedBreakpointHit: 7),
                    new TestStep(action: TestAction.StepInto, expectedStepComplete: 1),
                }
            );

            TestDebuggerSteps(
                "TypeScriptTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, 
                        targetBreakpoint: 7, 
                        targetBreakpointFile: "TypeScriptTest.ts"),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread, 
                        expectedHitCount: 1, 
                        targetBreakpoint: 7, 
                        targetBreakpointFile: "TypeScriptTest.ts", 
                        expectedBreakFunction: "Greeter",
                        expectedBreakpointHit: 7),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 8),
                }
            );

            TestDebuggerSteps(
                "TypeScriptTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 3, targetBreakpointFile: "TypeScriptTest.ts"),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread, expectedHitCount: 1, targetBreakpoint: 3, targetBreakpointFile: "TypeScriptTest.ts", expectedBreakFunction: "Greeter.greet", expectedBreakpointHit: 3),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            TestDebuggerSteps(
                "TypeScriptTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, 
                        targetBreakpoint: 1, 
                        targetBreakpointFile: "TypeScriptTest.ts"),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread, 
                        expectedHitCount: 1, 
                        targetBreakpoint: 1, 
                        targetBreakpointFile: "TypeScriptTest.ts", 
                        expectedBreakFunction: "Greeter.constructor",
                        expectedBreakpointHit: 1),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            TestDebuggerSteps(
                "TypeScriptTest2.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, 
                        targetBreakpoint: 2, 
                        targetBreakpointFile: "TypeScriptTest2.ts"),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread, 
                        expectedHitCount: 1, 
                        targetBreakpoint: 2, 
                        targetBreakpointFile: "TypeScriptTest2.ts", 
                        expectedBreakFunction: "Greeter.constructor",
                        expectedBreakpointHit: 2),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }
        #endregion
    }
}
