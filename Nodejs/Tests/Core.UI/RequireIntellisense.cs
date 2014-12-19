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
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudioTools;
using TestUtilities;
using TestUtilities.SharedProject;
using TestUtilities.UI;
using Keyboard = TestUtilities.UI.Keyboard;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class RequireIntellisense : NodejsProjectTest {
        public static ProjectDefinition BasicProject = RequireProject(
            Compile("server", ""),
            Compile("myapp"),

            Folder("node_modules"),
            Folder("node_modules\\Foo"),
            Compile("node_modules\\quox", ""),

            Content("node_modules\\Foo\\package.json", ""),

            Folder("SomeFolder"),
            Compile("SomeFolder\\baz", "")
        );

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void InSubFolder() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "SomeFolder", "baz.js");
                Keyboard.Type("require(");

                using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                    Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                    // we pick up built-ins, folders w/ package.json, and peers
                    AssertUtil.ContainsAtLeast(
                        completionSession.Session.GetDisplayTexts(),
                        "http",
                        "Foo",
                        "quox.js"
                    );

                    AssertUtil.DoesntContain(completionSession.Session.GetDisplayTexts(), "./SomeFolder/baz.js");
                    AssertUtil.DoesntContain(completionSession.Session.GetDisplayTexts(), "./myapp.js");

                    AssertUtil.ContainsAtLeast(
                        completionSession.Session.GetInsertionTexts(),
                        "'http'",
                        "'Foo'",
                        "'quox.js'"
                    );

                    Keyboard.Type("quo\t)");

                    server.WaitForText("require('quox.js')");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void BasicRequireCompletions() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = BasicProject.Generate().ToVs()) {
                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("require(");

                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        // we pick up built-ins, folders w/ package.json, and peers
                        AssertUtil.ContainsAtLeast(
                            completionSession.Session.GetDisplayTexts(),
                            "http",
                            "Foo",
                            "./myapp.js",
                            "./SomeFolder/baz.js",
                            "quox.js"
                        );

                        // we don't show our own file
                        AssertUtil.DoesntContain(completionSession.Session.GetDisplayTexts(), "./server.js");

                        AssertUtil.ContainsAtLeast(
                            completionSession.Session.GetInsertionTexts(),
                            "'http'",
                            "'Foo'",
                            "'./myapp.js'",
                            "'./SomeFolder/baz.js'",
                            "'quox.js'"
                        );

                        Keyboard.Type("htt");
                        server.WaitForText("require(htt");

                        // we should be filtered down
                        AssertUtil.ContainsExactly(
                            completionSession.Session.GetDisplayTexts(),
                            "http",
                            "https"
                        );

                        // this should trigger completion
                        Keyboard.Type(")");
                        server.WaitForText("require('http')");
                    }

                    Keyboard.Backspace(8);
                    server.WaitForText("require");

                    Keyboard.Type("(");
                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        // this should dismiss the session and not complete anything
                        Keyboard.Type("'".ToString());
                        server.WaitForText("require('");

                        Assert.IsTrue(completionSession.Session.IsDismissed);
                    }

                    Keyboard.Backspace(2);
                    server.WaitForText("require");

                    Keyboard.Type("(");
                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        // this should dismiss the session and not complete anything
                        Keyboard.Type("\"".ToString());
                        server.WaitForText("require(\"");

                        Assert.IsTrue(completionSession.Session.IsDismissed);
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void BasicRequireCompletionsQuotes() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = BasicProject.Generate().ToVs()) {
                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("require('");
                    
                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        // we pick up built-ins, folders w/ package.json, and peers
                        AssertUtil.ContainsAtLeast(
                            completionSession.Session.GetDisplayTexts(),
                            "http",
                            "Foo",
                            "./myapp.js",
                            "./SomeFolder/baz.js",
                            "quox.js"
                        );

                        // we don't show our own file
                        AssertUtil.DoesntContain(completionSession.Session.GetDisplayTexts(), "./server.js");

                        AssertUtil.ContainsAtLeast(
                            completionSession.Session.GetInsertionTexts(),
                            "http'",
                            "Foo'",
                            "./myapp.js'",
                            "./SomeFolder/baz.js'",
                            "quox.js'"
                        );

                        Keyboard.Type("htt");
                        server.WaitForText("require('htt");

                        // we should be filtered down
                        AssertUtil.ContainsExactly(
                            completionSession.Session.GetDisplayTexts(),
                            "http",
                            "https"
                        );

                        // this should trigger completion
                        Keyboard.Type("')");
                        server.WaitForText("require('http')");
                    }

                    Keyboard.Backspace(7);
                    server.WaitForText("require(");

                    Keyboard.Type("'./");
                    server.WaitForText("require('./");
                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        AssertUtil.ContainsExactly(
                            completionSession.Session.GetInsertionTexts(),
                            "./myapp.js'",
                            "./SomeFolder/baz.js'"
                        );
                    }

                    Keyboard.Backspace(3);
                    server.WaitForText("require(");

                    Keyboard.Type("'");
                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        // this should dismiss the session and not complete anything
                        Keyboard.Type(")".ToString());
                        server.WaitForText("require(')");

                        Assert.IsTrue(completionSession.Session.IsDismissed);
                    }

                    Keyboard.Backspace(2);
                    server.WaitForText("require(");

                    Keyboard.Type("'");
                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        // this should dismiss the session and not complete anything
                        Keyboard.Type("\"".ToString());
                        server.WaitForText("require('\"");

                        Assert.IsTrue(completionSession.Session.IsDismissed);
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void BasicRequireCompletionsDoubleQuotes() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = BasicProject.Generate().ToVs()) {
                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("require(\"");

                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        // we pick up built-ins, folders w/ package.json, and peers
                        AssertUtil.ContainsAtLeast(
                            completionSession.Session.GetDisplayTexts(),
                            "http",
                            "Foo",
                            "./myapp.js",
                            "./SomeFolder/baz.js",
                            "quox.js"
                        );

                        // we don't show our own file
                        AssertUtil.DoesntContain(completionSession.Session.GetDisplayTexts(), "./server.js");

                        AssertUtil.ContainsAtLeast(
                            completionSession.Session.GetInsertionTexts(),
                            "http\"",
                            "Foo\"",
                            "./myapp.js\"",
                            "./SomeFolder/baz.js\"",
                            "quox.js\""
                        );

                        Keyboard.Type("htt");
                        server.WaitForText("require(\"htt");

                        // we should be filtered down
                        AssertUtil.ContainsExactly(
                            completionSession.Session.GetDisplayTexts(),
                            "http",
                            "https"
                        );

                        Keyboard.Type("\")");

                        server.WaitForText("require(\"http\")");
                    }

                    Keyboard.Backspace(7);
                    server.WaitForText("require(");

                    Keyboard.Type("'./");
                    server.WaitForText("require('./");
                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        AssertUtil.ContainsExactly(
                            completionSession.Session.GetInsertionTexts(),
                            "./myapp.js'",
                            "./SomeFolder/baz.js'"
                        );
                    }

                    Keyboard.Backspace(3);
                    server.WaitForText("require(");

                    Keyboard.Type("\"");
                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        // this should dismiss the session and not complete anything
                        Keyboard.Type(")".ToString());
                        server.WaitForText("require(\")");

                        Assert.IsTrue(completionSession.Session.IsDismissed);
                    }

                    Keyboard.Backspace(2);
                    server.WaitForText("require(");

                    Keyboard.Type("\"");
                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        // this should dismiss the session and not complete anything
                        Keyboard.Type("'".ToString());
                        server.WaitForText("require(\"'");

                        Assert.IsTrue(completionSession.Session.IsDismissed);
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RequireKeyboardCompletionBraceCompletionOff() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = BasicProject.Generate().ToVs()) {
                
                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("require(ad");
                    WaitForCompletionSessionAndCommit(server);

                    server.WaitForText("require('addons'");

                    Keyboard.Backspace(20);

                    Keyboard.Type("require('ad");
                    WaitForCompletionSessionAndCommit(server);

                    server.WaitForText("require('addons");

                    Keyboard.Backspace(20);

                    Keyboard.Type("require(\"ad");
                    WaitForCompletionSessionAndCommit(server);

                    server.WaitForText("require(\"addons");

                    Keyboard.Backspace(20);

                    TypeRequireStatementAndSelectModuleName(false, server);
                    Keyboard.Type("ad");
                    WaitForCompletionSessionAndCommit(server);

                    server.WaitForText("require('addons')");

                    Keyboard.PressAndRelease(Key.Right);
                    Keyboard.PressAndRelease(Key.Right);
                    Keyboard.Backspace(20);

                    TypeRequireStatementAndSelectModuleName(true, server);
                    Keyboard.Type("ad");
                    WaitForCompletionSessionAndCommit(server);

                    server.WaitForText("require(\"addons\")");
                }
            }
        }

        private static void TypeRequireStatementAndSelectModuleName(bool doubleQuotes, EditorWindow window) {
            Keyboard.Type(string.Format("require({0}ab{0})", doubleQuotes ? "\"" : "'"));
            window.MoveCaret(1, 10);
            window.Select(1, 10, 2);
            Keyboard.Type(Keyboard.CtrlSpace.ToString());
        }

        private static void WaitForCompletionSessionAndCommit(EditorWindow server) {
            using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                UIThread.Invoke(() => { completionSession.Session.Commit(); });
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RequireKeyboardCompletionBraceCompletionOn() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", true)) {
                using (var solution = BasicProject.Generate().ToVs()) {

                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("require(ad");
                    WaitForCompletionSessionAndCommit(server);

                    server.WaitForText("require('addons')");

                    Keyboard.Type(")");
                    server.WaitForText("require('addons')");

                    Keyboard.Backspace(20);

                    Keyboard.Type("require('ad");
                    WaitForCompletionSessionAndCommit(server);

                    server.WaitForText("require('addons')");

                    Keyboard.Type("')");
                    server.WaitForText("require('addons')");

                    Keyboard.Backspace(20);

                    Keyboard.Type("require(\"ad");
                    WaitForCompletionSessionAndCommit(server);

                    server.WaitForText("require(\"addons\")");

                    Keyboard.Type("\")");
                    server.WaitForText("require(\"addons\")");

                    Keyboard.Backspace(20);

                    TypeRequireStatementAndSelectModuleName(false, server);
                    Keyboard.Type("ad");
                    WaitForCompletionSessionAndCommit(server);

                    server.WaitForText("require('addons')");

                    Keyboard.PressAndRelease(Key.Right);
                    Keyboard.PressAndRelease(Key.Right);
                    Keyboard.Backspace(20);

                    TypeRequireStatementAndSelectModuleName(true, server);
                    Keyboard.Type("ad");
                    WaitForCompletionSessionAndCommit(server);

                    server.WaitForText("require(\"addons\")");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RequireBuiltinModules() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("require(");

                using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                    Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                    // we pick up built-ins, folders w/ package.json, and peers
                    AssertUtil.ContainsAtLeast(
                        completionSession.Session.GetDisplayTexts(),
                        "http",
                        "timers",
                        "module",
                        "addons",
                        "util",
                        "tls",
                        "path",
                        "fs",
                        "https",
                        "url",
                        "assert",
                        "child_process",
                        "zlib",
                        "os",
                        "cluster",
                        "tty",
                        "vm"
                    );
                }
            }
        }


        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void CloseParenCommits() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("require(");

                using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                    Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                    Keyboard.Type("ht)");

                    server.WaitForText("require('http')");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void UserModule() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = BasicProject.Generate().ToVs()) {
                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("require(");

                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        Keyboard.Type("./mya\t)");

                        server.WaitForText("require('./myapp.js')");
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void UserModuleInFolder() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = BasicProject.Generate().ToVs()) {
                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("require(");

                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        Keyboard.Type("./Some\t)");

                        server.WaitForText("require('./SomeFolder/baz.js')");
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void CloseQuoteDoesntCommit() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = BasicProject.Generate().ToVs()) {
                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("require('");

                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        Keyboard.Type("ht')");

                        server.WaitForText("require('ht')");
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RequireAfterOperator() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = BasicProject.Generate().ToVs()) {
                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("+require(");

                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        Keyboard.Type("ht\t)");

                        server.WaitForText("+require('http')");
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RequireAfterOpenParen() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = BasicProject.Generate().ToVs()) {
                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("(require(");

                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        Keyboard.Type("ht\t)");

                        server.WaitForText("(require('http')");
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RequireAfterComma() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = BasicProject.Generate().ToVs()) {
                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("f(a, require(");

                    using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                        Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                        Keyboard.Type("ht\t)");

                        server.WaitForText("f(a, require('http')");
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RequireAfterAssignment() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("var http = require(");

                using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                    Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                    Keyboard.Type("ht\t)");

                    server.WaitForText("var http = require('http')");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RequireAfterReturn() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("return require(");

                using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                    Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                    Keyboard.Type("ht\t)");

                    server.WaitForText("return require('http')");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RequireAfterSemiColon() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("f(x);\rrequire(");

                using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                    Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);

                    Keyboard.Type("ht\t)");

                    server.WaitForText("f(x);\r\nrequire('http')");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RequireAfterKeywordNoCompletions() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {

                using (var solution = BasicProject.Generate().ToVs()) {
                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("var require(");
                    server.WaitForText("var require(");

                    server.AssertNoIntellisenseSession();
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RequireAfterDotNoCompletions() {
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = BasicProject.Generate().ToVs()) {
                    var server = solution.OpenItem("Require", "server.js");
                    Keyboard.Type("x.require(");
                    server.WaitForText("x.require(");

                    server.AssertNoIntellisenseSession();
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void RequireAfterContinuedMultiLineStringNoCompletions() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("'foo\\\rrequire(");
                server.WaitForText("'foo\\\r\nrequire(");

                server.AssertNoIntellisenseSession();
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/354
        /// 
        /// Make sure adding a module externally gets picked up
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void AddModuleExternally() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");

                File.WriteAllText(
                    Path.Combine(solution.Directory, "Require", "node_modules", "blah.js"),
                    "exports = function(a,b,c) { }"
                );

                System.Threading.Thread.Sleep(3000);

                Keyboard.Type("require(");

                using (var completionSession = server.WaitForSession<ICompletionSession>()) {
                    Assert.AreEqual(1, completionSession.Session.CompletionSets.Count);
                    AssertUtil.ContainsAtLeast(
                        completionSession.Session.GetDisplayTexts(),
                        "blah.js"
                    );
                }
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/494
        /// 
        /// We should include submodules (like quox) and not just the top-level
        /// module when index.js is present.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void SubmodulesFiles() {
            var project = Project("RequireSubmodules",
                Compile("server", ""),
                Folder("mymod"),
                Compile("mymod\\index", ""),
                Compile("mymod\\quox", "")
            );
            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = project.Generate().ToVs()) {
                    var server = solution.OpenItem("RequireSubmodules", "server.js");

                    server.MoveCaret(1, 1);

                    Keyboard.Type("require('./mymod/q\t)");
                    server.WaitForText("require('./mymod/quox.js')");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void SubmodulesFiles2() {
            var project = Project("RequireSubmodules",
                Compile("server", ""),
                Folder("mymod"),
                Compile("mymod\\quox", "")
            );

            using (new OptionHolder("TextEditor", "Node.js", "BraceCompletion", false)) {
                using (var solution = project.Generate().ToVs()) {
                    var server = solution.OpenItem("RequireSubmodules", "server.js");

                    server.MoveCaret(1, 1);

                    Keyboard.Type("require('./mymod/q\t)");
                    server.WaitForText("require('./mymod/quox.js')");
                }
            }
        }

        private static ProjectDefinition RequireProject(params ProjectContentGenerator[] items) {
            return new ProjectDefinition("Require", NodejsProject, items);
        }
    }
}
