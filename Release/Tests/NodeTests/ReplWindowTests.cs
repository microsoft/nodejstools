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
using System.IO;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities.Mocks;

namespace NodeTests {
    [TestClass]
    public class ReplWindowTests {
        [TestMethod, Priority(0)]
        public void TestNumber() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("42");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual(window.Output, "42");
            }
        }

        [TestMethod, Priority(0)]
        public void TestRequire() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("require('http').constructor");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("[Function: Object]", window.Output);
            }
        }

        [TestMethod, Priority(0)]
        public void TestFunctionDefinition() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("function f() { }");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("undefined", window.Output);
                window.ClearScreen();

                res = eval.ExecuteText("f");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("[Function: f]", window.Output);
            }
        }

        [TestMethod, Priority(0)]
        public void TestConsoleLog() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("console.log('hi')");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("hi\r\nundefined", window.Output);
            }
        }

        [TestMethod, Priority(0)]
        public void TestConsoleWarn() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("console.warn('hi')");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("hi\r\n", window.Error);
            }
        }

        [TestMethod, Priority(0)]
        public void TestConsoleError() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("console.error('hi')");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("hi\r\n", window.Error);
            }
        }

        [TestMethod, Priority(0)]
        public void TestConsoleDir() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("console.dir({'abc': {'foo': [1,2,3,4,5,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40]}})");
                var expected = @"{ abc: 
   { foo: 
      [ 1,
        2,
        3,
        4,
        5,
        7,
        8,
        9,
        10,
        11,
        12,
        13,
        14,
        15,
        16,
        17,
        18,
        19,
        20,
        21,
        22,
        23,
        24,
        25,
        26,
        27,
        28,
        29,
        30,
        31,
        32,
        33,
        34,
        35,
        36,
        37,
        38,
        39,
        40 ] } }
undefined";
                Assert.IsTrue(res.Wait(10000));
                var received = window.Output;
                AreEqual(expected, received);
            }
        }

        private static void AreEqual(string expected, string received) {
            for (int i = 0; i < expected.Length && i < received.Length; i++) {
                Assert.AreEqual(expected[i], received[i], String.Format("Mismatch at {0}: expected {1} got {2} in <{3}>", i, expected[i], received[i], received));
            }
            Assert.AreEqual(expected.Length, received.Length, "strings differ by length");
        }

        // 
        [TestMethod, Priority(0)]
        public void LargeOutput() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("var x = 'abc'; for(i = 0; i<12; i++) { x += x; }; x");
                string expected = "abc";
                for (int i = 0; i < 12; i++) {
                    expected += expected;
                }

                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("'" + expected + "'", window.Output);
            }
        }

        [TestMethod, Priority(0)]
        public void TestException() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("throw 'an error';");

                Assert.IsTrue(res.Wait(10000));

                Assert.AreEqual("an error", window.Error);
            }
        }

        [TestMethod, Priority(0)]
        public void TestProcessExit() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("process.exit(0);");

                Assert.IsTrue(res.Wait(10000));

                Assert.AreEqual("The process has exited", window.Error);
                window.ClearScreen();

                res = eval.ExecuteText("42");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("Current interactive window is disconnected - please reset the process.\r\n", window.Error);
            }
        }

        [TestMethod, Priority(0)]
        public void TestReset() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);

                var res = eval.ExecuteText("1");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("1", window.Output);
                res = window.Reset();
                Assert.IsTrue(res.Wait(10000));

                Assert.AreEqual("The process has exited", window.Error);
                window.ClearScreen();

                //Check to ensure the REPL continues to work after Reset
                res = eval.ExecuteText("var a = 1");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("undefined", window.Output);
                res = eval.ExecuteText("a");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("undefined1", window.Output);                
            }
        }

        [TestMethod, Priority(0)]
        public void TestSaveNoFile() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("function f() { }");

                Assert.IsTrue(res.Wait(10000));

                res = eval.ExecuteText("function g() { }");
                Assert.IsTrue(res.Wait(10000));

                new SaveReplCommand().Execute(window, "").Wait(10000);

                Assert.IsTrue(window.Error.Contains("save requires a filename"));
            }
        }

        [TestMethod, Priority(0)]
        public void TestSaveBadFile() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("function f() { }");

                Assert.IsTrue(res.Wait(10000));

                res = eval.ExecuteText("function g() { }");
                Assert.IsTrue(res.Wait(10000));

                new SaveReplCommand().Execute(window, "<foo>").Wait(10000);

                Assert.IsTrue(window.Error.Contains("Invalid filename: <foo>"));
            }
        }

        [TestMethod, Priority(0)]
        public void TestSave() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval, NodeConstants.JavaScript);
                var res = window.Execute("function f() { }");

                Assert.IsTrue(res.Wait(10000));

                res = window.Execute("function g() { }");
                Assert.IsTrue(res.Wait(10000));

                var path = Path.GetTempFileName();
                File.Delete(path);
                new SaveReplCommand().Execute(window, path).Wait(10000);

                Assert.IsTrue(File.Exists(path));
                var saved = File.ReadAllText(path);

                Assert.IsTrue(saved.IndexOf("function f") != -1);
                Assert.IsTrue(saved.IndexOf("function g") != -1);

                Assert.IsTrue(window.Output.Contains("Session saved to:"));
            }
        }

        [TestMethod, Priority(0)]
        public void TestBadSave() {
            using (var eval = new NodeReplEvaluator()) {
                var window = new MockReplWindow(eval);
                var res = eval.ExecuteText("function f() { }");

                Assert.IsTrue(res.Wait(10000));

                res = eval.ExecuteText("function g() { }");
                Assert.IsTrue(res.Wait(10000));

                new SaveReplCommand().Execute(window, "C:\\Some\\Directory\\That\\Does\\Not\\Exist\\foo.js").Wait(10000);

                Assert.IsTrue(window.Error.Contains("Failed to save: "));
            }
        }

        [TestMethod, Priority(0)]
        public void ReplEvaluatorProvider() {
            var provider = new NodeReplEvaluatorProvider();
            Assert.AreEqual(null, provider.GetEvaluator("Unknown"));
            Assert.AreNotEqual(null, provider.GetEvaluator("{E4AC36B7-EDC5-4AD2-B758-B5416D520705}"));
        }

        [TestMethod, Priority(0)]
        public void TestPartialInputs() {
            using (var eval = new NodeReplEvaluator()) {
                Assert.AreEqual(eval.CanExecuteText(@"function f() {"), false);
                Assert.AreEqual(eval.CanExecuteText(@"function f() {}"), true);
                Assert.AreEqual(eval.CanExecuteText(@"var net = require(""net""),"), false);
                Assert.AreEqual(eval.CanExecuteText(@"var net = require(""net""),
      repl = require(""repl"");"), true);
            }
        }

    }
}
