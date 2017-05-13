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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools;
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

            NodeEvaluationResult evalRes = frames[frame].ExecuteTextAsync(text).WaitAndUnwrapExceptions();
            Assert.IsTrue(evalRes != null, "didn't get evaluation result");

            if (children == null) {
                Assert.IsTrue(!evalRes.Type.HasFlag(NodeExpressionType.Expandable));
                Assert.IsTrue(evalRes.GetChildrenAsync().WaitAndUnwrapExceptions() == null);
            } else {
                Assert.IsTrue(evalRes.Type.HasFlag(NodeExpressionType.Expandable));
                var childrenReceived = new List<NodeEvaluationResult>(evalRes.GetChildrenAsync().WaitAndUnwrapExceptions());

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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore")]
        public void BreakAll() {
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
                process.BreakAllAsync().WaitAndUnwrapExceptions();
                AssertWaited(breakComplete);
                breakComplete.Reset();

                process.Terminate();
                AssertNotSet(breakComplete);
            }
        }

        #endregion

        #region Eval Tests

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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
        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void SpecialNumberLocalsTest() {
            LocalsTest(
                "SpecialNumberLocalsTest.js",
                6,
                expectedLocals: new string[] { "nan", "negInf", "nul", "posInf" },
                expectedValues: new string[] { "NaN", "-Infinity", "null", "Infinity" },
                expectedHexValues: new string[] { "NaN", "-Infinity", null, "Infinity" }
            );
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void GlobalsTest() {
            LocalsTest(
                "GlobalsTest.js",
                3,
                expectedParams: new string[] { "exports", "require", "module", "__filename", "__dirname" },
                expectedLocals: new[] { "y", "x" });
        }

        #endregion

        #region Stepping Tests

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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
        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore")]
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
        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore"), TestCategory("Ignore")]
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
        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore"), TestCategory("Ignore")]
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
        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore")]
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
        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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
        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore")]
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
        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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
                exceptionTreatments: CollectExceptionTreatments(ExceptionHitTreatment.BreakNever, "Error")
            );
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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
                            var scriptText = await process.GetScriptTextAsync(module.Id);
                            Assert.IsTrue(scriptText.Contains("function Console("));

                            // Download non-builtin
                            module = thread.Frames[2].Module;
                            Assert.IsFalse(module.BuiltIn);
                            scriptText = await process.GetScriptTextAsync(module.Id);
                            string fileText = File.ReadAllText(module.FileName);                            
                            Assert.IsTrue(scriptText.Contains(fileText));
                        }
                    ),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void Breakpoints() {
            TestDebuggerSteps(
                "BreakpointTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 0),
                    new TestStep(action: TestAction.ResumeThread, expectedBreakpointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void Breakpoints2() {
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void Breakpoints3() {
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void BreakpointsConditionals() {
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void BreakpointEnable() {
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void BreakpointRemove() {
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
        //public void BreakpointFailed() {
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void BreakpointsBreakOn() {
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
            } catch (ArgumentException e) {
                Assert.AreEqual("Invalid BreakOn count", e.Message);
                exceptionHit = true;
            }
            Assert.IsTrue(exceptionHit);
            exceptionHit = false;
            try {
                new BreakOn(BreakOnKind.GreaterThanOrEqual, 0);
            } catch (ArgumentException e) {
                Assert.AreEqual("Invalid BreakOn count", e.Message);
                exceptionHit = true;
            }
            Assert.IsTrue(exceptionHit);
            exceptionHit = false;
            try {
                new BreakOn(BreakOnKind.Mod, 0);
            } catch (ArgumentException e) {
                Assert.AreEqual("Invalid BreakOn count", e.Message);
                exceptionHit = true;
            }
            Assert.IsTrue(exceptionHit);
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void BreakpointsHitCount() {
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void BreakpointInvalidLineFixup() {
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void BreakpointPredicatedEntrypointNoFixup() {
            TestBreakpointPredicatedEntrypoint("BreakpointTest.js", targetBreakpoint: 0, expectedHit: 0);
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void BreakpointPredicatedEntrypointBlankLineFixup() {
            TestBreakpointPredicatedEntrypoint("FixupBreakpointOnBlankLine.js", targetBreakpoint: 0, expectedHit: 2);
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void BreakpointPredicatedEntrypointCommentFixup() {
            TestBreakpointPredicatedEntrypoint("FixupBreakpointOnComment.js", targetBreakpoint: 0, expectedHit: 1);
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore")]
        public void DuplicateFileName() {
            TestDebuggerSteps(
                "DuppedFilename.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint,
                        targetBreakpoint: 1,
                        targetBreakpointFile: "Directory\\DuppedFilename.js",
                        expectFailure: true),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread,
                        expectedHitCount: 1,
                        targetBreakpoint: 1,
                        targetBreakpointFile: "Directory\\DuppedFilename.js",
                        expectedBreakFunction: "f",
                        expectedBreakpointHit: 1),
                    new TestStep(action: TestAction.StepOut, expectedStepComplete: 4),
                });
        }

        #endregion

        #region Exception Tests

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void Exceptions() {
            // Well-known, handled
            // Implicit break always
            //TestExceptions(
            //    DebuggerTestPath + @"WellKnownHandledException.js",
            //    ExceptionHitTreatment.BreakAlways,
            //    null,
            //    0,
            //    new ExceptionInfo("Error", "Error: Error description", 3)
            //);

            // Explicit break always
            TestExceptions(
                DebuggerTestPath + @"WellKnownHandledException.js",
                ExceptionHitTreatment.BreakNever,
                CollectExceptionTreatments(ExceptionHitTreatment.BreakAlways, "Error"),
                0,
                new ExceptionInfo("Error", "Error: Error description", 3)
            );

            // Explicit break always (both)
            TestExceptions(
                DebuggerTestPath + @"WellKnownHandledException.js",
                ExceptionHitTreatment.BreakAlways,
                CollectExceptionTreatments(ExceptionHitTreatment.BreakAlways, "Error"),
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
            //TestExceptions(
            //    DebuggerTestPath + @"WellKnownUnhandledException.js",
            //    ExceptionHitTreatment.BreakAlways,
            //    null,
            //    8,
            //    new ExceptionInfo("Error", "Error: Error description", 2)
            //);

            // Explicit break always
            TestExceptions(
                DebuggerTestPath + @"WellKnownUnhandledException.js",
                ExceptionHitTreatment.BreakNever,
                CollectExceptionTreatments(ExceptionHitTreatment.BreakAlways, "Error"),
                8,
                new ExceptionInfo("Error", "Error: Error description", 2)
            );

            // Explicit break always (both)
            TestExceptions(
                DebuggerTestPath + @"WellKnownUnhandledException.js",
                ExceptionHitTreatment.BreakAlways,
                CollectExceptionTreatments(ExceptionHitTreatment.BreakAlways, "Error"),
                8,
                new ExceptionInfo("Error", "Error: Error description", 2)
            );

            // Implicit break on unhandled
            //TestExceptions(
            //    DebuggerTestPath + @"WellKnownUnhandledException.js",
            //    ExceptionHitTreatment.BreakOnUnhandled,
            //    null,
            //    8,
            //    new ExceptionInfo("Error", "Error: Error description", 2)
            //);

            // Explicit break on unhandled
            TestExceptions(
                DebuggerTestPath + @"WellKnownUnhandledException.js",
                ExceptionHitTreatment.BreakNever,
                CollectExceptionTreatments(ExceptionHitTreatment.BreakOnUnhandled, "Error"),
                8,
                new ExceptionInfo("Error", "Error: Error description", 2)
            );

            // Explicit break on unhandled (both)
            TestExceptions(
                DebuggerTestPath + @"WellKnownUnhandledException.js",
                ExceptionHitTreatment.BreakOnUnhandled,
                CollectExceptionTreatments(ExceptionHitTreatment.BreakOnUnhandled, "Error"),
                8,
                new ExceptionInfo("Error", "Error: Error description", 2)
            );
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void ExceptionsTypes() {
            TestExceptions(
                DebuggerTestPath + @"ExceptionTypes.js",
                ExceptionHitTreatment.BreakAlways,
                CollectExceptionTreatments(ExceptionHitTreatment.BreakAlways, "Error", "RangeError", "TypeError", "ReferenceError", "URIError", "SyntaxError", "EvalError", "UserDefinedError", "UserDefinedRangeError", "UserDefinedType"),
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void ComplexExceptions() {
            TestExceptions(
                DebuggerTestPath + @"ComplexExceptions.js",
                ExceptionHitTreatment.BreakAlways,
                CollectExceptionTreatments(ExceptionHitTreatment.BreakAlways, "Error", "UserDefinedClass", "TypeError", "ReferenceError"),
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
        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void RequireExceptions() {
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
        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void ExceptionInEvaluatedCode() {
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void Stepping_AccrossDeepThrow() {
            TestDebuggerSteps(
                "ThrowsWithDeepCallstack.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 11),
                    new TestStep(action: TestAction.StepOver, expectedStepComplete: 13),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                },
                defaultExceptionTreatment: ExceptionHitTreatment.BreakAlways,
                exceptionTreatments: CollectExceptionTreatments(ExceptionHitTreatment.BreakNever, NodeVariableType.String)
            );
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void ModuleLoad() {
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void ExitNormal() {
            TestDebuggerSteps(
                "ExitNormal.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void ExitException() {
            TestDebuggerSteps(
                "ExitException.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExceptionRaised: new ExceptionInfo("Error", "Error: msg", 1)),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 8),
                },
                exceptionTreatments: CollectExceptionTreatments(ExceptionHitTreatment.BreakAlways, "Error")
            );

        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore")]
        public void ExitExplicit() {
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

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void ScriptArguments() {
            TestDebuggerSteps(
                "PassedArgs.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExceptionRaised: new ExceptionInfo("Error", "Error: Invalid args", 3)),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 8),
                },
                exceptionTreatments: CollectExceptionTreatments(ExceptionHitTreatment.BreakAlways, "Error")
            );
            TestDebuggerSteps(
                "PassedArgs.js",
                new[] {
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                },
                scriptArguments: "42"
            );
        }

        #endregion

        #region Attach Tests

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore")]
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

        #endregion

        #region TypeScript Tests

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
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

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1515
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void TypeScript_Stepping_Basic_RedirectDir() {
            TestDebuggerSteps(
                "TypeScriptOut\\TypeScriptTest.js",
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
                "TypeScriptOut\\TypeScriptTest.js",
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
                "TypeScriptOut\\TypeScriptTest.js",
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
                "TypeScriptOut\\TypeScriptTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, targetBreakpoint: 3, targetBreakpointFile: "TypeScriptTest.ts"),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread, expectedHitCount: 1, targetBreakpoint: 3, targetBreakpointFile: "TypeScriptTest.ts", expectedBreakFunction: "Greeter.greet", expectedBreakpointHit: 3),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );

            TestDebuggerSteps(
                "TypeScriptOut\\TypeScriptTest.js",
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
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void TypeScript_Break_On_First_Line() {
            TestDebuggerSteps(
                "TypeScriptTest3.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, 
                        targetBreakpoint: 0,
                        targetBreakpointFile: "TypeScriptTest3.ts"),
                    new TestStep(action: TestAction.ResumeThread, 
                        expectedHitCount: 1, 
                        targetBreakpoint: 0,
                        targetBreakpointColumn: 0,
                        targetBreakpointFile: "TypeScriptTest3.ts", 
                        expectedBreakFunction: NodeVariableType.AnonymousFunction,
                        expectedBreakpointHit: 0),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void TypeScript_Inheiritance_BreakInClass() {
            TestDebuggerSteps(
                "TypeScriptInheritTest.js",
                new[] {
                    new TestStep(action: TestAction.AddBreakpoint, 
                        targetBreakpoint: 7,
                        targetBreakpointFile: "TypeScriptInheritApple.ts",
                        expectFailure: true),
                    new TestStep(action: TestAction.ResumeThread, expectedEntryPointHit: 0),
                    new TestStep(action: TestAction.ResumeThread, 
                        expectedHitCount: 1, 
                        targetBreakpoint: 7,
                        targetBreakpointColumn: 0,
                        targetBreakpointFile: "TypeScriptInheritApple.ts", 
                        expectedBreakFunction: "TypeScriptInheritApple.constructor",
                        expectedBreakpointHit: 7,
                        expectReBind: true),
                    new TestStep(action: TestAction.ResumeThread, 
                        expectedHitCount: 2, 
                        targetBreakpoint: 7,
                        targetBreakpointColumn: 0,
                        targetBreakpointFile: "TypeScriptInheritApple.ts", 
                        expectedBreakFunction: "TypeScriptInheritApple.constructor",
                        expectedBreakpointHit: 7),
                    new TestStep(action: TestAction.ResumeProcess, expectedExitCode: 0),
                }
            );
        }
        #endregion

        #region Helpers Tests

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("AppVeyorIgnore")]
        public async Task TaskWaitAsync() {
            // Successful task, no timeout.
            var task = Task.Run(() => {
                Thread.Sleep(500);
                return 42; });
            Assert.AreEqual(42, await task.WaitAsync(TimeSpan.FromMilliseconds(1000)));

            // Failed task, no timeout.
            var tex = new Exception();
            try {
                task = Task.Run(() => {
                    Thread.Sleep(500);
                    if ("".Length == 0) {
                        throw tex;
                    }
                    return 42;
                });
                await task.WaitAsync(TimeSpan.FromMilliseconds(1000));
                Assert.Fail("Exception expected");
            } catch (Exception cex) {
                Assert.AreSame(tex, cex);
            }

            // Timeout before task completes.
            task = Task.Run(() => {
                Thread.Sleep(500);
                return 42;
            });
            try {
                await task.WaitAsync(TimeSpan.FromMilliseconds(100));
                Assert.Fail("TaskCanceledException expected");
            } catch (TaskCanceledException) {
            }

            // Forced cancelation before task completes.
            task = Task.Run(() => {
                Thread.Sleep(500);
                return 42;
            });
            try {
                await task.WaitAsync(TimeSpan.FromMilliseconds(300), new CancellationTokenSource(100).Token);
                Assert.Fail("TaskCanceledException expected");
            } catch (TaskCanceledException) {
            }
        }

        #endregion
    }
}
