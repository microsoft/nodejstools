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
    }
}
