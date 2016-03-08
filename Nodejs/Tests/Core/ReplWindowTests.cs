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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Mocks;

namespace NodejsTests {
    [TestClass]
    public class ReplWindowTests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            if (!File.Exists("visualstudio_nodejs_repl.js")) {
                using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("NodejsTests.visualstudio_nodejs_repl.js"))) {
                    File.WriteAllText(
                        "visualstudio_nodejs_repl.js",
                        reader.ReadToEnd()
                    );
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestNumber() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("42");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual(window.Output, "42");
            }
        }

        private static NodejsReplEvaluator ProjectlessEvaluator() {
            return new NodejsReplEvaluator(TestNodejsReplSite.Instance);
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestRequire() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("require('http').constructor");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("[Function: Object]", window.Output);
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestFunctionDefinition() {
            var whitespaces = new[] { "", "\r\n", "   ", "\r\n    " };
            using (var eval = ProjectlessEvaluator()) {
                foreach (var whitespace in whitespaces) {
                    Console.WriteLine("Whitespace: {0}", whitespace);
                    var window = new MockReplWindow(eval);
                    window.ClearScreen();
                    var res = eval.ExecuteText(whitespace + "function f() { }");
                    Assert.IsTrue(res.Wait(10000));
                    Assert.AreEqual("undefined", window.Output);
                    window.ClearScreen();

                    res = eval.ExecuteText("f");
                    Assert.IsTrue(res.Wait(10000));
                    Assert.AreEqual("[Function: f]", window.Output);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestConsoleLog() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("console.log('hi')");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("hi\r\nundefined", window.Output);
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestConsoleWarn() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("console.warn('hi')");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("hi\r\n", window.Error);
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestConsoleError() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("console.error('hi')");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("hi\r\n", window.Error);
            }
        }

<<<<<<< HEAD
        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore"), TestCategory("Ignore")]
=======
        [Ignore]
        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
>>>>>>> 59fe154edefc5a62b312756298a95d54b2afe8da
        public void TestConsoleDir() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
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
        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void LargeOutput() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("var x = 'abc'; for(i = 0; i<12; i++) { x += x; }; x");
                string expected = "abc";
                for (int i = 0; i < 12; i++) {
                    expected += expected;
                }

                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("'" + expected + "'", window.Output);
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestException() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("throw 'an error';");

                Assert.IsTrue(res.Wait(10000));

                Assert.AreEqual("an error", window.Error);
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestExceptionNull() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("throw null;");

                Assert.IsTrue(res.Wait(10000));

                Assert.AreEqual("undefined", window.Output);
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestExceptionUndefined() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("throw undefined;");

                Assert.IsTrue(res.Wait(10000));

                Assert.AreEqual("undefined", window.Output);
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestProcessExit() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("process.exit(0);");

                Assert.IsTrue(res.Wait(10000));

                Assert.AreEqual("The process has exited" + Environment.NewLine, window.Error);
                window.ClearScreen();

                res = eval.ExecuteText("42");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("Current interactive window is disconnected - please reset the process.\r\n", window.Error);
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestReset() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();

                var res = eval.ExecuteText("1");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("1", window.Output);
                res = window.Reset();
                Assert.IsTrue(res.Wait(10000));

                Assert.AreEqual("The process has exited" + Environment.NewLine, window.Error);
                window.ClearScreen();
                Assert.AreEqual("", window.Output);
                Assert.AreEqual("", window.Error);

                //Check to ensure the REPL continues to work after Reset
                res = eval.ExecuteText("var a = 1");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("undefined", window.Output);
                res = eval.ExecuteText("a");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("undefined1", window.Output);
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestSaveNoFile() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("function f() { }");

                Assert.IsTrue(res.Wait(10000));

                res = eval.ExecuteText("function g() { }");
                Assert.IsTrue(res.Wait(10000));

                new SaveReplCommand().Execute(window, "").Wait(10000);

                Assert.IsTrue(window.Error.Contains("save requires a filename"));
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestSaveBadFile() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("function f() { }");

                Assert.IsTrue(res.Wait(10000));

                res = eval.ExecuteText("function g() { }");
                Assert.IsTrue(res.Wait(10000));

                new SaveReplCommand().Execute(window, "<foo>").Wait(10000);

                Assert.IsTrue(window.Error.Contains("Invalid filename: <foo>"));
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestSave() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval, NodejsConstants.JavaScript);
                window.ClearScreen();
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

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestBadSave() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("function f() { }");

                Assert.IsTrue(res.Wait(10000));

                res = eval.ExecuteText("function g() { }");
                Assert.IsTrue(res.Wait(10000));

                new SaveReplCommand().Execute(window, "C:\\Some\\Directory\\That\\Does\\Not\\Exist\\foo.js").Wait(10000);

                Assert.IsTrue(window.Error.Contains("Failed to save: "));
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void ReplEvaluatorProvider() {
            var provider = new NodejsReplEvaluatorProvider();
            Assert.AreEqual(null, provider.GetEvaluator("Unknown"));
            Assert.AreNotEqual(null, provider.GetEvaluator("{E4AC36B7-EDC5-4AD2-B758-B5416D520705}"));
        }

        private static string[] _partialInputs = {  "function f(",
                                                    "function f() {",
                                                    "x = {foo:",
                                                    "{\r\nfoo:42",
                                                    "function () {",
                                                    "for(var i = 0; i<10; i++) {",
                                                    "for(var i = 0; i<10; i++) {\r\nconsole.log('hi');",
                                                    "while(true) {",
                                                    "while(true) {\r\nbreak;",
                                                    "do {",
                                                    "do {\r\nbreak;",
                                                    "if(true) {",
                                                    "if(true) {\r\nconsole.log('hi');",
                                                    "if(true) {\r\nconsole.log('hi');\r\n}else{",
                                                    "if(true) {\r\nconsole.log('hi');\r\n}else{\r\nconsole.log('bye');",
                                                    "switch(\"abc\") {",
                                                    "switch(\"abc\") {\r\ncase \"foo\":",
                                                    "switch(\"abc\") {\r\ncase \"foo\":\r\nbreak;",
                                                    "switch(\"abc\") {\r\ncase \"foo\":\r\nbreak;\r\ncase \"abc\":",
                                                    "switch(\"abc\") {\r\ncase \"foo\":\r\nbreak;\r\ncase \"abc\":console.log('hi');",
                                                    "switch(\"abc\") {\r\ncase \"foo\":\r\nbreak;\r\ncase \"abc\":console.log('hi');\r\nbreak;",
                                                    "[1,",
                                                    "[1,\r\n2,",
                                                    "var net = require('net'),"
                                                   };
        private static string[] _completeInputs = { "try {\r\nconsole.log('hi')\r\n} catch {\r\n}",
                                                    "try {\r\nconsole.log('hi')\r\n} catch(a) {\r\n}",
                                                    "function f(\r\na) {\r\n}\r\n\r\n};",
                                                    "x = {foo}",
                                                    "x = {foo:42}",
                                                    "{x:42}",
                                                    "{\r\nfoo:42\r\n}",
                                                    "function () {\r\nconsole.log('hi');\r\n}",
                                                    "for(var i = 0; i<10; i++) {\r\nconsole.log('hi');\r\n}",
                                                    "while(true) {\r\nbreak;\r\n}",
                                                    "do {\r\nbreak;\r\n}while(true);",
                                                    "if(true) {\r\nconsole.log('hi');\r\n}",
                                                    "if(true) {\r\nconsole.log('hi');\r\n}else{\r\nconsole.log('bye');\r\n}",
                                                    "switch('abc') {\r\ncase 'foo':\r\nbreak;\r\ncase 'abc':\r\nconsole.log('hi');\r\nbreak;\r\n}",
                                                    "[1,\r\n2,\r\n3]",
                                                    "var net = require('net'),\r\n      repl = require('repl');",
                                                  };

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestPartialInputs() {
            using (var eval = ProjectlessEvaluator()) {
                foreach (var partialInput in _partialInputs) {
                    Assert.AreEqual(false, eval.CanExecuteText(partialInput), "Partial input successfully parsed: " + partialInput);
                }
                foreach (var completeInput in _completeInputs) {
                    Assert.AreEqual(true, eval.CanExecuteText(completeInput), "Complete input failed to parse: " + completeInput);
                }
            }
        }

<<<<<<< HEAD
        [TestMethod, Priority(0), TestCategory("Ignore")]
=======
        [Ignore]
        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
<<<<<<< HEAD
>>>>>>> e9bec53... Add AppVeyorIgnoreCategory to repl window tests and disable one that fails locally
=======
>>>>>>> 59fe154edefc5a62b312756298a95d54b2afe8da
        public void TestVarI() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();

                var res = eval.ExecuteText("i");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("ReferenceError: i is not defined", window.Error);
                Assert.AreEqual("", window.Output);
                res = eval.ExecuteText("var i = 987654;");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("undefined", window.Output);
                res = eval.ExecuteText("i");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("undefined987654", window.Output);
            }
        }

        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestObjectLiteral() {
            using (var eval = ProjectlessEvaluator()) {
                var window = new MockReplWindow(eval);
                window.ClearScreen();
                var res = eval.ExecuteText("{x:42}");
                Assert.IsTrue(res.Wait(10000));
                Assert.AreEqual("{ x: 42 }", window.Output);
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/279
        /// </summary>
        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void TestRequireInProject() {
            string testDir;
            do {
                testDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (Directory.Exists(testDir));
            Directory.CreateDirectory(testDir);
            var moduleDir = Path.Combine(testDir, "node_modules");
            Directory.CreateDirectory(moduleDir);
            File.WriteAllText(Path.Combine(moduleDir, "foo.js"), "exports.foo = function(a, b, c) { }");
            File.WriteAllText(Path.Combine(testDir, "bar.js"), "exports.bar = function(a, b, c) { }");

            try {
                using (var eval = new NodejsReplEvaluator(new TestNodejsReplSite(null, testDir))) {
                    var window = new MockReplWindow(eval);
                    window.ClearScreen();
                    var res = eval.ExecuteText("require('foo.js');");
                    Assert.IsTrue(res.Wait(10000));
                    Assert.AreEqual(window.Output, "{ foo: [Function] }");
                    window.ClearScreen();

                    res = eval.ExecuteText("require('./bar.js');");
                    Assert.IsTrue(res.Wait(10000));
                    Assert.AreEqual(window.Output, "{ bar: [Function] }");
                }
            } finally {
                try {
                    Directory.Delete(testDir, true);
                } catch (IOException) {
                }
            }
        }

        // https://nodejstools.codeplex.com/workitem/1575
        [Ignore]
        [TestMethod, Priority(0), Timeout(180000)]
        public async Task TestNpmReplCommandProcessExitSucceeds() {
            var npmPath = Nodejs.GetPathToNodeExecutableFromEnvironment("npm.cmd");
            using (var eval = ProjectlessEvaluator()) {
                var mockWindow = new MockReplWindow(eval) {
                    ShowAnsiCodes = true
                };
                mockWindow.ClearScreen();
                var redirector = new NpmReplCommand.NpmReplRedirector(mockWindow);

                for (int j = 0; j < 200; j++) {
                    await NpmReplCommand.ExecuteNpmCommandAsync(
                        redirector,
                        npmPath,
                        null,
                        new[] {"config", "get", "registry"},
                        null);
                }
            }
        }

<<<<<<< HEAD
        [TestMethod, Priority(0), TestCategory("Ignore")]
=======
        [Ignore]
        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
<<<<<<< HEAD
>>>>>>> e9bec53... Add AppVeyorIgnoreCategory to repl window tests and disable one that fails locally
=======
>>>>>>> 59fe154edefc5a62b312756298a95d54b2afe8da
        public void TestNpmReplRedirector() {
            using (var eval = ProjectlessEvaluator()) {
                var mockWindow = new MockReplWindow(eval) {
                    ShowAnsiCodes = true
                };
                mockWindow.ClearScreen();
                var redirector = new NpmReplCommand.NpmReplRedirector(mockWindow);

                redirector.WriteLine("npm The sky is at a stable equilibrium");
                var expectedInfoLine =
                    NpmReplCommand.NpmReplRedirector.NormalAnsiColor + "npm The sky is at a stable equilibrium" +
                    Environment.NewLine;

                Assert.AreEqual(expectedInfoLine, mockWindow.Output);
                Assert.IsFalse(redirector.HasErrors);
                mockWindow.ClearScreen();

                redirector.WriteLine("npm WARN The sky is at an unstable equilibrium!");
                var expectedWarnLine =
                    NpmReplCommand.NpmReplRedirector.WarnAnsiColor + "npm WARN" +
                    NpmReplCommand.NpmReplRedirector.NormalAnsiColor + " The sky is at an unstable equilibrium!" +
                    Environment.NewLine;

                Assert.AreEqual(expectedWarnLine, mockWindow.Output);
                Assert.IsFalse(redirector.HasErrors);
                mockWindow.ClearScreen();

                redirector.WriteLine("npm ERR! The sky is falling!");
                var expectedErrorLine =
                    NpmReplCommand.NpmReplRedirector.ErrorAnsiColor + "npm ERR!" +
                    NpmReplCommand.NpmReplRedirector.NormalAnsiColor + " The sky is falling!" +
                    Environment.NewLine;
                Assert.AreEqual(expectedErrorLine, mockWindow.Output);
                Assert.IsTrue(redirector.HasErrors);
                mockWindow.ClearScreen();

                var decodedInfoLine = "├── parseurl@1.0.1";
                string encodedText = Console.OutputEncoding.GetString(Encoding.UTF8.GetBytes(decodedInfoLine));
                redirector.WriteLine(encodedText);
                var expectedDecodedInfoLine = NpmReplCommand.NpmReplRedirector.NormalAnsiColor + decodedInfoLine
                    + Environment.NewLine;

                Assert.AreEqual(expectedDecodedInfoLine, mockWindow.Output);
                Assert.IsTrue(redirector.HasErrors, "Errors should remain until end");
            }
        }
    }
}
