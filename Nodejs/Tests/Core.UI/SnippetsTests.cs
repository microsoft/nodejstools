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
using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.SharedProject;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class SnippetsTests:NodejsProjectTest {

        private static ProjectDefinition BasicProject = Project(
            "SnippetsTest",
            Compile("server", ""),
            Compile("multiline", "one\r\ntwo\r\nthree"),
            Compile("nonempty", "nonempty"),
            Compile("indented", "if (False){\r\n    }")
        );

        class Snippet {
            public readonly string Shortcut;
            public readonly string Expected;
            public readonly Declaration[] Declarations;

            public Snippet(string shortcut, string expected, params Declaration[] declarations) {
                Shortcut = shortcut;
                Expected = expected;
                Declarations = declarations;
            }
        }

        class Declaration {
            public readonly string Replacement;
            public readonly string Expected;

            public Declaration(string replacement, string expected) {
                Replacement = replacement;
                Expected = expected;
            }
        }

        private static readonly Snippet[] BasicSnippets = new Snippet[] {
            new Snippet(
                "console.log",
                "console.log($body$)",
                new Declaration("body","console.log(body)")
            ),
            new Snippet(
                "while",
                "while (True){\r\n    $body$\r\n}",
                new Declaration("False", "while (False){\r\n    $body$\r\n}")
            ),
            new Snippet(
                "if",
                "if (True){\r\n   $body$\r\n}",
                new Declaration("False", "while (False){\r\n  $body$\r\n}")

            ),
            new Snippet(
                "iife",
                "(function undefined {\r\n  $body$\r\n})();",
                new Declaration("name","(function name {\r\n    $body$\r\n})();")
            ),
            new Snippet(
                "for",
                "for (var i = 0; i < length; i++) {\r\n   $body$\r\n}",
                new Declaration ("length","for (var i = 0; i < 42; i++) {\r\n   $body$\r\n}")
            )
        };

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestInsertSnippet() {
            using (var solution = BasicProject.Generate().ToVs()) {
                    
                foreach (var snippet in BasicSnippets) {
                    TestOneInsertSnippet(solution, snippet, "Nodejs");

                    solution.CloseActiveWindow(vsSaveChanges.vsSaveChangesNo);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestInsertSnippetIndented() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var app = solution.OpenItem("SnippetsTest", "indented.js");
                app.MoveCaret(2, 5);
                app.Invoke(() => app.TextView.Caret.EnsureVisible());
                app.SetFocus();

                Keyboard.Type("while\t");
                app.WaitForText("if (False){\r\n    while (True){\r\n    body\r\n}}");
                Keyboard.Type("\r");
                Keyboard.Type("replacedBody");
                app.WaitForText("if (False){\r\n    while (True){\r\n    replacedBody\r\n}}");

                solution.CloseActiveWindow(vsSaveChanges.vsSaveChangesNo);
            }

        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestSurroundWithSnippet() {
            using (var solution = BasicProject.Generate().ToVs()) {
                foreach (var snippet in BasicSnippets) {
                    TestOneSurroundWithSnippet(solution, snippet, "Nodejs");

                    solution.CloseActiveWindow(vsSaveChanges.vsSaveChangesNo);
                }
            }
        }

        private static IEditor TestOneSurroundWithSnippet(IVisualStudioInstance solution, Snippet snippet, string category, string body = "body", string file = "server.js") {
            Console.WriteLine("Testing: {0}", snippet.Shortcut);
            var server = solution.OpenItem("SnippetsTest", file);
            server.Select(1, 1, server.Text.Length);
            server.Invoke(() => server.TextView.Caret.EnsureVisible());
            server.SetFocus();

            solution.ExecuteCommand("Edit.SurroundWith");
            return VerifySnippet(snippet, body, server);
        }

        private static IEditor TestOneInsertSnippet(IVisualStudioInstance solution, Snippet snippet, string category, string body = "body", string file = "server.js") {
            Console.WriteLine("Testing: {0}", snippet.Shortcut);
            var server = solution.OpenItem("SnippetsTest", file);
            server.Select(1, 1, server.Text.Length);
            server.Invoke(() => server.TextView.Caret.EnsureVisible());
            server.SetFocus();

            solution.ExecuteCommand("Edit.InsertSnippet");
            Keyboard.Type(category + "\t");

            return VerifySnippet(snippet, body, server);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestBasicSnippetsTab() {
            using (var solution = BasicProject.Generate().ToVs()) {
                foreach (var snippet in BasicSnippets) {
                    TestOneTabSnippet(solution, snippet);

                    solution.CloseActiveWindow(vsSaveChanges.vsSaveChangesNo);
                }
            }
        }

        private static IEditor TestOneTabSnippet(IVisualStudioInstance solution, Snippet snippet) {
            Console.WriteLine("Testing: {0}", snippet.Shortcut);
            var server = solution.OpenItem("SnippetsTest", "server.js");
            server.MoveCaret(1, 1);
            server.Invoke(() => server.TextView.Caret.EnsureVisible());
            server.SetFocus();

            return VerifySnippet(snippet, "body", server);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestSurroundWithMultiline() {
            using (var solution = BasicProject.Generate().ToVs()) {
                foreach (var snippet in BasicSnippets) {
                    TestOneSurroundWithSnippet(
                        solution,
                        snippet,
                        "one\r\n    two\r\n    three",
                        "multiline.js"
                    );

                    solution.CloseActiveWindow(vsSaveChanges.vsSaveChangesNo);
                }
            }
        }

        private static IEditor VerifySnippet(Snippet snippet, string body, IEditor server) {
            Keyboard.Type(snippet.Shortcut + "\t");

            server.WaitForText(snippet.Expected.Replace("$body$", body));

            foreach (var decl in snippet.Declarations) {
                Console.WriteLine("Declaration: {0}", decl.Replacement);
                Keyboard.Type(decl.Replacement);
                server.WaitForText(decl.Expected.Replace("$body$", body));
                Keyboard.Type("\t");
            }
            Keyboard.Type("\r");
            return server;
        }

    }

}
