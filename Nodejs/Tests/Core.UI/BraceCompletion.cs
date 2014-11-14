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
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.SharedProject;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class BraceCompletion : NodejsProjectTest {
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

        /// <summary>
        /// Test the different brace completions ({, [, (, ', ").
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void BraceCompletionsBasic() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "SomeFolder", "baz.js");

                // Test '('
                Keyboard.Type("require(");
                server.WaitForText("require()");

                // Test '"'
                Keyboard.Type("\"");
                server.WaitForText("require(\"\")");

                // Test step over
                Keyboard.Type("\")");
                server.WaitForText("require(\"\")");

                // Complete this line and attempt '{'
                Keyboard.Type(";\nfunction a(b) {");
                server.WaitForText("require(\"\");\r\nfunction a(b) {}");

                // Verify both nested completions and '[' as well as '''
                Keyboard.Type("\nvar arr = ['elem\\'ent1', 'element2");
                server.WaitForText("require(\"\");\r\nfunction a(b) {\r\nvar arr = ['elem\\'ent1', 'element2']}");

                Keyboard.Type("'];");
                server.WaitForText("require(\"\");\r\nfunction a(b) {\r\n    var arr = ['elem\\'ent1', 'element2'];}");

                // Verify typeover
                Keyboard.Type("\n}");
                server.WaitForText("require(\"\");\r\nfunction a(b) {\r\n    var arr = ['elem\\'ent1', 'element2'];\r\n}");
            }
        }

        /// <summary>
        /// Verify that within comments and literals we do not attempt brace completion.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void BraceCompletionDoesNotOccurInCommentsOrLiterals() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "SomeFolder", "baz.js");

                // Verify that a comment gets no completions brace causes nothing if it has no start.
                Keyboard.Type("// '");
                server.WaitForText("// '");

                // Verify that inside strings no extra completions happen
                Keyboard.Type("\nconsole.log(\"'");
                server.WaitForText("// '\r\nconsole.log(\"'\")");

                Keyboard.Type("\", '{");
                server.WaitForText("// '\r\nconsole.log(\"'\", '{')");
            }
        }

        /// <summary>
        /// Verify that within a literal with an escaped quote we don't start a whole second completion session.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void BraceCompletionTerminatesOnEscapedQuoteInLiteralAndDoesntStartAgain() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("Require", "SomeFolder", "baz.js");

                // Verify that when adding an escaped double quote to a literal that we don't try to start
                // a new brace completion on close of the string literal.
                Keyboard.Type("var a = \"some\\\"text");
                server.WaitForText("var a = \"some\\\"text");

                Keyboard.Type("\"");
                server.WaitForText("var a = \"some\\\"text\"");
            }
        }

        private static ProjectDefinition RequireProject(params ProjectContentGenerator[] items) {
            return new ProjectDefinition("Require", NodejsProject, items);
        }
    }
}
