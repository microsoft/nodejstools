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
using Microsoft.NodeTools.Repl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities.Mocks;

namespace NodeTests {
    [TestClass]
    public class ReplWindowTests {
        [TestMethod, Priority(0)]
        public void TestNumber() {
            var eval = new NodeReplEvaluator();
            var window = new MockReplWindow(eval);
            eval.Initialize(window);
            var res = eval.ExecuteText("42");
            Assert.IsTrue(res.Wait(10000));
            Assert.AreEqual(window.Output, "42");
        }

        [TestMethod, Priority(0)]
        public void TestRequire() {
            var eval = new NodeReplEvaluator();
            var window = new MockReplWindow(eval);
            eval.Initialize(window);
            var res = eval.ExecuteText("require('http').constructor");
            Assert.IsTrue(res.Wait(10000));
            Assert.AreEqual("[Function: Object]", window.Output);
        }

        [TestMethod, Priority(0)]
        public void TestFunctionDefinition() {
            var eval = new NodeReplEvaluator();
            var window = new MockReplWindow(eval);
            eval.Initialize(window);
            var res = eval.ExecuteText("function f() { }");
            Assert.IsTrue(res.Wait(10000));
            Assert.AreEqual("undefined", window.Output);
            window.ClearScreen();

            res = eval.ExecuteText("f");
            Assert.IsTrue(res.Wait(10000));
            Assert.AreEqual("[Function: f]", window.Output);
        }

        [TestMethod, Priority(0)]
        public void TestConsoleLog() {
            var eval = new NodeReplEvaluator();
            var window = new MockReplWindow(eval);
            eval.Initialize(window);
            var res = eval.ExecuteText("console.log('hi')");
            Assert.IsTrue(res.Wait(10000));
            Assert.AreEqual("hi\r\nundefined", window.Output);
        }

        [TestMethod, Priority(0)]
        public void TestConsoleWarn() {
            var eval = new NodeReplEvaluator();
            var window = new MockReplWindow(eval);
            eval.Initialize(window);
            var res = eval.ExecuteText("console.warn('hi')");
            Assert.IsTrue(res.Wait(10000));
            Assert.AreEqual("hi\r\n", window.Error);
        }

        [TestMethod, Priority(0)]
        public void TestConsoleError() {
            var eval = new NodeReplEvaluator();
            var window = new MockReplWindow(eval);
            eval.Initialize(window);
            var res = eval.ExecuteText("console.error('hi')");
            Assert.IsTrue(res.Wait(10000));
            Assert.AreEqual("hi\r\n", window.Error);
        }

        [TestMethod, Priority(0)]
        public void TestConsoleDir() {
            var eval = new NodeReplEvaluator();
            var window = new MockReplWindow(eval);
            eval.Initialize(window);
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

        private static void AreEqual(string expected, string received) {
            for (int i = 0; i < expected.Length && i < received.Length; i++) {
                Assert.AreEqual(expected[i], received[i], String.Format("Mismatch at {0}: expected {1} got {2} in <{3}>", i, expected[i], received[i], received));
            }
            Assert.AreEqual(expected.Length, received.Length, "strings differ by length");
        }

        // 
        [TestMethod, Priority(0)]
        public void LargeOutput() {
            var eval = new NodeReplEvaluator();
            var window = new MockReplWindow(eval);
            eval.Initialize(window);
            var res = eval.ExecuteText("var x = 'abc'; for(i = 0; i<12; i++) { x += x; }; x");
            string expected = "abc";
            for (int i = 0; i < 12; i++) {
                expected += expected;
            }

            Assert.IsTrue(res.Wait(10000));
            Assert.AreEqual("'" + expected + "'", window.Output);
        }

        [TestMethod, Priority(0)]
        public void TestException() {
            var eval = new NodeReplEvaluator();
            var window = new MockReplWindow(eval);
            eval.Initialize(window);
            var res = eval.ExecuteText("throw 'an error';");            

            Assert.IsTrue(res.Wait(10000));

            Assert.AreEqual("an error", window.Error);
        }

        [TestMethod, Priority(0)]
        public void TestProcessExit() {
            var eval = new NodeReplEvaluator();
            var window = new MockReplWindow(eval);
            eval.Initialize(window);
            var res = eval.ExecuteText("process.exit(0);");

            Assert.IsTrue(res.Wait(10000));

            Assert.AreEqual("The process has exited", window.Error);
            window.ClearScreen();

            res = eval.ExecuteText("42");
            Assert.IsTrue(res.Wait(10000));
            Assert.AreEqual("Current interactive window is disconnected - please reset the process.\r\n", window.Error);
        }

        [TestMethod, Priority(0)]
        public void ReplEvaluatorProvider() {
            var provider = new NodeReplEvaluatorProvider();
            Assert.AreEqual(null, provider.GetEvaluator("Unknown"));
            Assert.AreNotEqual(null, provider.GetEvaluator("{E4AC36B7-EDC5-4AD2-B758-B5416D520705}"));
        }
    }
}
