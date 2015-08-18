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
            Compile("indented", "if (true) {\r\n   \r\n}")
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
                "while",
                "while (true) {\r\n   $body$\r\n};\r\n",
                new Declaration("false", "while (false) {\r\n   $body$\r\n};\r\n")
            ),
            new Snippet(
                "if",
                "if (true) {\r\n   $body$\r\n}\r\n",
                new Declaration("false", "if (false) {\r\n   $body$\r\n}\r\n")
            ),
            new Snippet(
                "iife",
                "(function (undefined) {\r\n   $body$\r\n})();\r\n",
                new Declaration("name","(function (name) {\r\n   $body$\r\n})();\r\n")
            ),
            new Snippet(
                "for",
                "for (var i = 0; i < length; i++) {\r\n   $body$\r\n};\r\n",
                new Declaration("counter", "for (var counter = 0; counter < length; counter++) {\r\n   $body$\r\n};\r\n")
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
        public void TestSelectedIndented() {
            using (var solution = BasicProject.Generate().ToVs()) {
                var server = solution.OpenItem("SnippetsTest", "indented.js");
                server.MoveCaret(2, 5);
                server.Invoke(() => server.TextView.Caret.EnsureVisible());
                server.SetFocus();

                Keyboard.Type("if\t");
                server.WaitForText("if (true) {\r\n   if (true) {\r\n      \r\n   }\r\n   \r\n}");
                Keyboard.Type("\r");
                Keyboard.Type("testing");
                server.WaitForText("if (true) {\r\n    if (true) {\r\n      testing\r\n   }\r\n   \r\n}");

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

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestSelected() {
            var snippet = new Snippet(
                "if",
                "if (true) {\r\n   $body$\r\n}\r\n",
                new Declaration("false", "if (false) {\r\n   $body$\r\n}\r\n")
            );
            using (var solution = BasicProject.Generate().ToVs()) {
                var app = TestOneTabSnippet(solution, snippet);

                Keyboard.Type("testing");
                app.WaitForText("if (false) {\r\n   testing\r\n}\r\n");

                solution.CloseActiveWindow(vsSaveChanges.vsSaveChangesNo);
            }

        }

        private static IEditor TestOneSurroundWithSnippet(IVisualStudioInstance solution, Snippet snippet, string category, string body = "nonempty", string file = "nonempty.js") {
            Console.WriteLine("Testing: {0}", snippet.Shortcut);
            var server = solution.OpenItem("SnippetsTest", file);
            server.Select(1, 1, server.Text.Length);
            server.Invoke(() => server.TextView.Caret.EnsureVisible());
            server.SetFocus();

            solution.ExecuteCommand("Edit.SurroundWith");
            return VerifySnippet(snippet, body, server);
        }

        private static IEditor TestOneInsertSnippet(IVisualStudioInstance solution, Snippet snippet, string category, string body = "nonempty", string file ="nonempty.js") {
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

            return VerifySnippet(snippet, "", server);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestSurroundWithMultiline() {
            using (var solution = BasicProject.Generate().ToVs()) {
                foreach (var snippet in BasicSnippets) {
                    TestOneSurroundWithSnippet(
                        solution,
                        snippet,
                        "Nodejs",
                        "one\r\n   two\r\n   three",
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
                Keyboard.Type("\t");
                server.WaitForText(decl.Expected.Replace("$body$", body));
                Keyboard.Type("\t");
            }
            Keyboard.Type("\r");
            return server;
        }

    }

}
