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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.UI;
using TestUtilities.SharedProject;
using Microsoft.VisualStudio.Language.Intellisense;

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

        [TestCleanup]
        public void MyTestCleanup() {
            for (int i = 0; i < 20; i++) {
                try {
                    VsIdeTestHostContext.Dte.Solution.Close(false);
                    break;
                } catch {
                    VsIdeTestHostContext.Dte.Documents.CloseAll(EnvDTE.vsSaveChanges.vsSaveChangesNo);
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void InSubFolder() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "SomeFolder", "baz.js");
                Keyboard.Type("require(");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                // we pick up built-ins, folders w/ package.json, and peers
                AssertUtil.ContainsAtLeast(
                    completionSession.GetDisplayTexts(),
                    "http",
                    "Foo",
                    "quox.js"
                );

                AssertUtil.DoesntContain(completionSession.GetDisplayTexts(), "./SomeFolder/baz.js");
                AssertUtil.DoesntContain(completionSession.GetDisplayTexts(), "./myapp.js");

                AssertUtil.ContainsAtLeast(
                    completionSession.GetInsertionTexts(),
                    "'http'",
                    "'Foo'",
                    "'quox.js'"
                );

                Keyboard.Type("quo\t)");

                server.WaitForText("require('quox.js')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void BasicRequireCompletions() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("require(");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                // we pick up built-ins, folders w/ package.json, and peers
                AssertUtil.ContainsAtLeast(
                    completionSession.GetDisplayTexts(),
                    "http",
                    "Foo",
                    "./myapp.js",
                    "./SomeFolder/baz.js",
                    "quox.js"
                );

                // we don't show our own file
                AssertUtil.DoesntContain(completionSession.GetDisplayTexts(), "./server.js");

                AssertUtil.ContainsAtLeast(
                    completionSession.GetInsertionTexts(),
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
                    completionSession.GetDisplayTexts(),
                    "http",
                    "https"
                );

                Keyboard.Type("\t)");

                server.WaitForText("require('http')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void BasicRequireCompletionsQuotes() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("require('");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                // we pick up built-ins, folders w/ package.json, and peers
                AssertUtil.ContainsAtLeast(
                    completionSession.GetDisplayTexts(),
                    "http",
                    "Foo",
                    "./myapp.js",
                    "./SomeFolder/baz.js",
                    "quox.js"
                );

                // we don't show our own file
                AssertUtil.DoesntContain(completionSession.GetDisplayTexts(), "./server.js");

                AssertUtil.ContainsAtLeast(
                    completionSession.GetInsertionTexts(),
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
                    completionSession.GetDisplayTexts(),
                    "http",
                    "https"
                );

                Keyboard.Type("\t)");

                server.WaitForText("require('http')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void BasicRequireCompletionsDoubleQuotes() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("require(\"");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                // we pick up built-ins, folders w/ package.json, and peers
                AssertUtil.ContainsAtLeast(
                    completionSession.GetDisplayTexts(),
                    "http",
                    "Foo",
                    "./myapp.js",
                    "./SomeFolder/baz.js",
                    "quox.js"
                );

                // we don't show our own file
                AssertUtil.DoesntContain(completionSession.GetDisplayTexts(), "./server.js");

                AssertUtil.ContainsAtLeast(
                    completionSession.GetInsertionTexts(),
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
                    completionSession.GetDisplayTexts(),
                    "http",
                    "https"
                );

                Keyboard.Type("\t)");

                server.WaitForText("require(\"http\")");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void RequireBuiltinModules() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("require(");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                // we pick up built-ins, folders w/ package.json, and peers
                AssertUtil.ContainsAtLeast(
                    completionSession.GetDisplayTexts(),
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


        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CloseParenCommits() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("require(");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                Keyboard.Type("ht)");

                server.WaitForText("require('http')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void UserModule() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("require(");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                Keyboard.Type("./mya\t)");

                server.WaitForText("require('./myapp.js')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void UserModuleInFolder() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("require(");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                Keyboard.Type("./Some\t)");

                server.WaitForText("require('./SomeFolder/baz.js')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void CloseQuoteDoesntCommit() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("require('");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                Keyboard.Type("ht')");

                server.WaitForText("require('ht')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void RequireAfterOperator() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("+require(");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                Keyboard.Type("ht\t)");

                server.WaitForText("+require('http')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void RequireAfterOpenParen() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("(require(");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                Keyboard.Type("ht\t)");

                server.WaitForText("(require('http')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void RequireAfterComma() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("f(a, require(");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                Keyboard.Type("ht\t)");

                server.WaitForText("f(a, require('http')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void RequireAfterAssignment() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("var http = require(");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                Keyboard.Type("ht\t)");

                server.WaitForText("var http = require('http')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void RequireAfterReturn() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("return require(");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                Keyboard.Type("ht\t)");

                server.WaitForText("return require('http')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void RequireAfterSemiColon() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("f(x);\rrequire(");

                var completionSession = server.WaitForSession<ICompletionSession>();
                Assert.AreEqual(1, completionSession.CompletionSets.Count);

                Keyboard.Type("ht\t)");

                server.WaitForText("f(x);\r\nrequire('http')");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void RequireAfterKeywordNoCompletions() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("var require(");
                server.WaitForText("var require(");

                server.AssertNoIntellisenseSession();
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void RequireAfterDotNoCompletions() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("x.require(");
                server.WaitForText("x.require(");

                server.AssertNoIntellisenseSession();
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void RequireAfterContinuedMultiLineStringNoCompletions() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "server.js");
                Keyboard.Type("'foo\\\rrequire(");
                server.WaitForText("'foo\\\r\nrequire(");

                server.AssertNoIntellisenseSession();
            }
        }

        private static ProjectDefinition RequireProject(params ProjectContentGenerator[] items) {
            return new ProjectDefinition("Require", NodejsProject, items);
        }
    }
}
