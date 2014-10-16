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
using System.Linq;
using System.Windows;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using TestUtilities;
using TestUtilities.UI;
using Key = System.Windows.Input.Key;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public sealed class BasicIntellisense : NodejsProjectTest {
        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/347
        /// 
        /// Make sure Ctrl-Space works
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CtrlSpace() {
            var project = Project("CtrlSpace",
                Compile("server", "var http = require('http');\r\nhttp.createS")
            );

            using (var solution = project.Generate().ToVs()) {
                var server = solution.OpenItem("CtrlSpace", "server.js");

                server.MoveCaret(2, 13);

                VsIdeTestHostContext.Dte.ExecuteCommand("Edit.CompleteWord");

                server.WaitForText("var http = require('http');\r\nhttp.createServer");
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/560
        /// 
        /// Make sure we get intellisense for process.stdin
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ProcessStdinIntellisense() {
            var project = Project("ProcessStdIn",
                Compile("server", "var x = process.stdin;\r\n")
            );

            using (var solution = project.Generate().ToVs()) {
                var server = solution.OpenItem("ProcessStdIn", "server.js");

                server.MoveCaret(2, 1);
                Keyboard.Type("x.");
                System.Threading.Thread.Sleep(3000);
                Keyboard.Type("addLis\t");

                server.WaitForText("var x = process.stdin;\r\nx.addListener");
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/552
        /// 
        /// Make sure reference path intellisense works
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ReferenceIntellisense() {
            var project = Project("ReferenceIntellisense",
                Compile("server", "/// <reference path=\"app.js\" />\r\nvar x = abc;\r\n"),
                Compile("app", "abc = 42;")
            );

            using (var solution = project.Generate().ToVs()) {
                var server = solution.OpenItem("ReferenceIntellisense", "server.js");

                server.MoveCaret(3, 1);

                Keyboard.Type("x.");
                System.Threading.Thread.Sleep(3000);
                Keyboard.Type("toF\t");

                server.WaitForText("/// <reference path=\"app.js\" />\r\nvar x = abc;\r\nx.toFixed");
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/497
        /// 
        /// Intellisense to a module in node_modules which defines package.json
        /// which points at a folder should successfully pick up the intellisense.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void NodeModulesPackageJsonFolder() {
            var project = Project("ReferenceIntellisense",
                Compile("server", "require('mymod')"),
                Folder("node_modules"),
                Folder("node_modules\\mymod"),
                Folder("node_modules\\mymod\\lib"),
                Compile("node_modules\\mymod\\lib\\index", "exports.x = 42;"),
                Content("node_modules\\mymod\\package.json", "{main: './lib/index.js', name: 'mymod'}")
            );

            using (var solution = project.Generate().ToVs()) {
                var server = solution.OpenItem("ReferenceIntellisense", "server.js");

                server.MoveCaret(1, 17);

                Keyboard.Type(".x.");
                System.Threading.Thread.Sleep(3000);
                Keyboard.Type("toF\t");
                server.WaitForText("require('mymod').x.toFixed");
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1203
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void SignaturesTest() {
            var project = Project("SignaturesTest",
                Compile("server", "function f(a, b, c) { }\r\n\r\n")
            );

            using (var solution = project.Generate().ToVs()) {
                var server = solution.OpenItem("SignaturesTest", "server.js");

                server.MoveCaret(3, 1);

                Keyboard.Type("f(");
                
                using (var sh = server.WaitForSession<ISignatureHelpSession>()) {
                    var session = sh.Session;
                    Assert.AreEqual("a", session.SelectedSignature.CurrentParameter.Name);
                }

                Keyboard.Backspace();
                Keyboard.Backspace();

                Keyboard.Type("new f(");

                using (var sh = server.WaitForSession<ISignatureHelpSession>()) {
                    var session = sh.Session;
                    Assert.AreEqual("a", session.SelectedSignature.CurrentParameter.Name);
                }

            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1201
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void IntellisenseAfterNewLine() {
            var project = Project("NewLineTest",
                Compile("server", "'blah'\r\n")
            );

            using (var solution = project.Generate().ToVs()) {
                var server = solution.OpenItem("NewLineTest", "server.js");

                server.MoveCaret(2, 1);

                Keyboard.Type(".");

                using (var sh = server.WaitForSession<ICompletionSession>()) {
                    var session = sh.Session;
                    AssertUtil.ContainsAtLeast(session.CompletionSets[0].Completions.Select(x => x.InsertionText), "big");
                }
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1201
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void IntellisenseAfterNewLine2() {
            var project = Project("NewLineTest",
                Compile("server", "var x = 'abc';\r\n// foo\r\nx\r\nvar abc=42\r\nx")
            );

            using (var solution = project.Generate().ToVs()) {
                var server = solution.OpenItem("NewLineTest", "server.js");

                server.MoveCaret(3, 2);

                Keyboard.Type(".");

                using (var sh = server.WaitForSession<ICompletionSession>()) {
                    var session = sh.Session;
                    AssertUtil.ContainsAtLeast(session.CompletionSets[0].Completions.Select(x => x.InsertionText), "big");
                }

                server.MoveCaret(5, 2);

                Keyboard.Type(".");

                using (var sh = server.WaitForSession<ICompletionSession>()) {
                    var session = sh.Session;
                    AssertUtil.ContainsAtLeast(session.CompletionSets[0].Completions.Select(x => x.InsertionText), "big");
                }
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1144
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void IntellisenseAfterSingleLineComment() {
            var project = Project("IntellisenseAfterSingleCommentTest",
                Compile("server", "'blah'//blah\r\n")
            );

            using (var solution = project.Generate().ToVs()) {
                var server = solution.OpenItem("IntellisenseAfterSingleCommentTest", "server.js");

                server.MoveCaret(2, 1);
                Keyboard.Type(".");

                using (var sh = server.WaitForSession<ICompletionSession>()) {
                    var session = sh.Session;
                    AssertUtil.ContainsAtLeast(session.CompletionSets[0].Completions.Select(x => x.InsertionText), "big");
                }
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/1144
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void IntellisenseAfterMultiLineComment() {
            var project = Project("IntellisenseAfterMultiCommentTest",
                Compile("server", "'blah'/*blah*/")
            );

            using (var solution = project.Generate().ToVs()) {
                var server = solution.OpenItem("IntellisenseAfterMultiCommentTest", "server.js");

                server.MoveCaret(1, 15);
                Keyboard.Type(".");

                using (var sh = server.WaitForSession<ICompletionSession>()) {
                    var session = sh.Session;
                    AssertUtil.ContainsAtLeast(session.CompletionSets[0].Completions.Select(x => x.InsertionText), "big");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void JSDocTest() {
            var project = Project("JSDocTest",
                Compile("app", @"
/** Documentation for f.
  * 
  * Just a paragraph. It shouldn't show up anywhere.
  *
  * @param  a   Documentation for a.
  * 
  * Another paragraph that won't show up anywhere. This one has a {@link}.
  *
  * @arg    b   Documentation for b.
  *             It spans multiple lines.
  * @param  c   Documentation for c. It has a {@link} in it.
  * @arg   [d]  Documentation for d. It is an optional parameter.
  * @argument [e=123]
  * Documentation for e. It has a default value.
  *
  * @see Not a parameter!
  */
function f(a, b, c, d, e) {}

/** Documentation for g. */
var g = function() {}

var h;
/** Documentation for h. */
h = function() {}
                ")
            );

            using (var solution = project.Generate().ToVs()) {
                var editor = solution.OpenItem("JSDocTest", "app.js");

                // Go to end of file
                Keyboard.PressAndRelease(Key.End, Key.LeftCtrl);

                // function f()
                Keyboard.Type("f(");
                using (var sh = editor.WaitForSession<ISignatureHelpSession>()) {
                    var session = sh.Session;
                    Assert.AreEqual("Documentation for f.\r\n...", session.SelectedSignature.Documentation);
                }
                Keyboard.Backspace();
                Keyboard.Backspace();

                // var g = function()
                Keyboard.Type("g(");
                using (var sh = editor.WaitForSession<ISignatureHelpSession>()) {
                    var session = sh.Session;
                    Assert.AreEqual("Documentation for g.", session.SelectedSignature.Documentation);
                }
                Keyboard.Backspace();
                Keyboard.Backspace();

                // h = function()
                Keyboard.Type("h(");
                using (var sh = editor.WaitForSession<ISignatureHelpSession>()) {
                    var session = sh.Session;
                    Assert.AreEqual("Documentation for h.", session.SelectedSignature.Documentation);
                }
                Keyboard.Backspace();
                Keyboard.Backspace();

                // @param parsing
                Keyboard.Type("f(");
                using (var sh = editor.WaitForSession<ISignatureHelpSession>()) {
                    var session = sh.Session;
                    Assert.AreEqual("f(a, b, c, d?, e? = 123)", session.SelectedSignature.Content);

                    Keyboard.Type("a");
                    Assert.AreEqual("a", session.SelectedSignature.CurrentParameter.Name);
                    Assert.AreEqual("Documentation for a.", session.SelectedSignature.CurrentParameter.Documentation);

                    Keyboard.Type(", b");
                    Assert.AreEqual("b", session.SelectedSignature.CurrentParameter.Name);
                    Assert.AreEqual("Documentation for b. It spans multiple lines.", session.SelectedSignature.CurrentParameter.Documentation);

                    Keyboard.Type(", c");
                    Assert.AreEqual("c", session.SelectedSignature.CurrentParameter.Name);
                    Assert.AreEqual("Documentation for c. It has a {@link} in it.", session.SelectedSignature.CurrentParameter.Documentation);

                    Keyboard.Type(", d");
                    Assert.AreEqual("d", session.SelectedSignature.CurrentParameter.Name);
                    Assert.AreEqual("Documentation for d. It is an optional parameter.", session.SelectedSignature.CurrentParameter.Documentation);

                    Keyboard.Type(", e");
                    Assert.AreEqual("e", session.SelectedSignature.CurrentParameter.Name);
                    Assert.AreEqual("Documentation for e. It has a default value.", session.SelectedSignature.CurrentParameter.Documentation);
                }
                Keyboard.Backspace();
                Keyboard.Backspace();

            }
        }

    }
}
