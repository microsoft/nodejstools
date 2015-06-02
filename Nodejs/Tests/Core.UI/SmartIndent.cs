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
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools.VSTestHost;
using TestUtilities;
using TestUtilities.SharedProject;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class SmartIndent : NodejsProjectTest {
        public static ProjectDefinition BasicProject = new ProjectDefinition(
            "AutoIndent",
            NodejsProject,
            Compile("server", ""),
            Compile("Bug384", @"Foo.prototype.Bar = function (callback) {

    var result = null;
    for (var i = 0; i < 10; i++) {
        if (i == 5) {
            console.log(i);
            break;
        }        
    }

}"),
          Compile("Bug1198", @"if (true)
    if (true)
        if (true)
            return;
")
        );

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void SmartIndentBug1198() {
            using (new DefaultSmartIndentOptionHolder()) {
                using (var solution = BasicProject.Generate().ToVs()) {
                    var doc = solution.OpenItem("AutoIndent", "Bug1198.js");
                    doc.Invoke(() => doc.TextView.Caret.MoveTo(doc.TextView.TextViewLines[4]));
                    Keyboard.Type(System.Windows.Input.Key.End);
                    Keyboard.Type(" ");
                    Assert.AreEqual(
                        9,
                        doc.TextView.Caret.Position.BufferPosition.GetContainingLine().Length
                    );
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void SmartIndentBug384() {
            using (new DefaultSmartIndentOptionHolder()) {
                using (var solution = BasicProject.Generate().ToVs()) {
                    var doc = solution.OpenItem("AutoIndent", "Bug384.js");
                    doc.Invoke(() => doc.TextView.Caret.MoveTo(doc.TextView.TextViewLines[9]));
                    Keyboard.Type(System.Windows.Input.Key.End);
                    Keyboard.Type(" ");
                    Assert.AreEqual(
                        5,
                        doc.TextView.Caret.Position.BufferPosition.GetContainingLine().Length
                    );
                }
            }

        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void SmartIndentBug3() {
            using (new DefaultSmartIndentOptionHolder(insertTabs: true)) {
                var testCases = new[] {
                    new {
                    Typed = @"var promise = new Promise(function () {
function a() {
if (b)
b = c;
100".Replace("\r\n", "\r"),
                    Expected =@"var promise = new Promise(function () {
\tfunction a() {
\t\tif (b)
\t\t\tb = c;
\t\t100
\t}
})".Replace(@"\t", "\t") }
                };

                VerifyAutoIndentTestCases(testCases);
            }
        }

        private class AutoIndentTestCase {
            public string Typed { get; set; }
            public string Expected { get; set; } 
        }

        private void VerifyAutoIndentTestCases(dynamic[] testCases)
        {
            using (var solution = BasicProject.Generate().ToVs()) {
                foreach (var testCase in testCases) {
                    Console.WriteLine("Typing  : {0}", testCase.Typed);
                    Console.WriteLine("Expected: {0}", testCase.Expected);
                    AutoIndentTest(
                        solution,
                        testCase.Typed,
                        testCase.Expected
                    );
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void SmartIndentBasic() {
            using (new DefaultSmartIndentOptionHolder(braceCompletion:false)) {
                var testCases = new[] {
                    // https://nodejstools.codeplex.com/workitem/1176
                    new {
                        Typed = "if (true)\rwhile(true)\rfalse;\r100",
                        Expected = @"if (true)
    while (true)
        false;
100"
                    },
                    new {
                        Typed = "if (true)\rif (true)\rfalse;\relse",
                        Expected = @"if (true)
    if (true)
        false;
    else"
                    },
                    // no {
                    new { 
                        Typed = "if (true)\r42\relse \r100\r200",
                        Expected = @"if (true)
    42
else
    100
200"              
                    },
                    new { 
                        Typed = "if (true)\r42\relse if (true)\r100\r200",
                        Expected = @"if (true)
    42
else if (true)
    100
200"              
                    },
                    new { 
                        Typed = "for(var i = 0; i<100; i++)\r42\r100",
                        Expected = @"for(var i = 0; i<100; i++)
    42
100"              
                    },
                    new { 
                        Typed = "while(true)\r42\r100",
                        Expected = @"while (true)
    42
100"              
                    },
                    new { 
                        Typed = "do \r42\rwhile(false)\r100",
                        Expected = @"do 
    42
while(false)
100"              
                    },
                    // grouping
                    new { 
                        Typed = "x = [1,\r2,\r3\r]",
                        Expected = @"x = [1,
    2,
    3
    ]"              
                    },
                    // nested function, dedent keyword
                    new { 
                        Typed = "function a() {\rfunction b() {\rreturn\r}\r\b}",
                        Expected = @"function a() {
    function b() {
        return
    }
}"
                    },
                    // nested function, dedent keyword
                    new { 
                        Typed = "function a() {\rfunction b() {\rfoo \r\b}\r\b}",
                        Expected = @"function a() {
    function b() {
        foo
    }
}"
                    },
                    // basic indentation
                    new {
                        Typed = "if (true) {\rconsole.log('hi');\r\b}",
                        Expected = @"if (true) {
    console.log('hi');
}"
                    },
                    // enter in multiline string
                    new {
                        Typed = "if (true) {\r\"foo\\\rbar\"\r\b}",
                        Expected = @"if (true) {
    ""foo\
bar""
}"
                    },
                    // enter in multiline comment
                    new {
                        Typed = "if (true) {\r/*foo\rbar*/\r\b}",
                        Expected = @"if (true) {
    /*foo
     * bar*/
}"
                    },
                    // auto dedent after return
                    new {
                        Typed = "if (true) {\rreturn \r}",
                        Expected = @"if (true) {
    return
}"
                    },
                    // auto dedent after return;
                    new {
                        Typed = "if (true) {\rreturn; \r}",
                        Expected = @"if (true) {
    return;
}"
                    },
                    // auto dedent after return;
                    new {
                        Typed = "if (true) {\rreturn;; \r}",
                        Expected = @"if (true) {
    return;;
}"
                    },
                    // auto dedent normal statement
                    new {
                        Typed = "if (true) {\rf(x)\r\b}",
                        Expected = @"if (true) {
    f(x)
}"
                    },
                    // auto dedent function call w/o params
                    new {
                        Typed = "if (true) {\rfoo()\r\b}",
                        Expected = @"if (true) {
    foo()
}"
                    },
                    // auto dedent normal statement ending in semicolon
                    new {
                        Typed = "if (true) {\rf(x);\r\b}",
                        Expected = @"if (true) {
    f(x);
}"
                    },
                };

                VerifyAutoIndentTestCases(testCases);
            }
        }


        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void BraceCompletion_BasicTests() {
            using (new DefaultSmartIndentOptionHolder()) {
                    var testCases = new[] {

                        // auto indent after return during brace completion
                        // https://nodejstools.codeplex.com/workitem/1466
                        new {
                            Typed = "function myfunc(name){\rm();",
                            Expected = "function myfunc(name){\r\n    m();\r\n}"
                        },
                        // https://nodejstools.codeplex.com/workitem/1560
                        new {
                            Typed = "var a = function (test) {\rreturn {\rtest",
                            Expected = "var a = function (test) {\r\n    return {\r\n        test\r\n    }\r\n}"
                        },
                        new {
                            Typed = "var a = function (test) {\rreturn [\rtest",
                            Expected = "var a = function (test) {\r\n    return [\r\n        test\r\n    ]\r\n}"
                        },
                        new {
                            Typed = "var a = function (test) {\rreturn (\rtest",
                            Expected = "var a = function (test) {\r\n    return (\r\n        test\r\n    )\r\n}"
                        },
                    };

                    VerifyAutoIndentTestCases(testCases);
            }
        }

        private static void AutoIndentTest(IVisualStudioInstance solution, string typedText, string expectedText) {
            var doc = solution.OpenItem("AutoIndent", "server.js");
            doc.MoveCaret(1, 1);
            doc.Invoke(() => doc.TextView.Caret.EnsureVisible());
            doc.SetFocus();

            Keyboard.Type(typedText);

            string actual = null;
            for (int i = 0; i < 100; i++) {
                actual = doc.TextView.TextBuffer.CurrentSnapshot.GetText();

                if (expectedText == actual) {
                    break;
                }
                System.Threading.Thread.Sleep(100);
            }
            Assert.AreEqual(expectedText, actual);

            VSTestContext.DTE.ActiveWindow.Close(vsSaveChanges.vsSaveChangesNo);
        }


        private class DefaultSmartIndentOptionHolder : IDisposable {
            List<OptionHolder> _options = new List<OptionHolder>();
            public DefaultSmartIndentOptionHolder(
                bool braceCompletion = true, 
                bool insertTabs = false,
                int indentSize = 4,
                int tabSize = 4) {
                    _options.AddRange(new[] {
#if DEV12_OR_LATER
                    new OptionHolder("TextEditor", "Node.js", "BraceCompletion", braceCompletion),
#endif
                    new OptionHolder("TextEditor", "Node.js", "InsertTabs", insertTabs),
                    new OptionHolder("TextEditor", "Node.js", "IndentSize", indentSize),
                    new OptionHolder("TextEditor", "Node.js", "TabSize", tabSize)
                });
            }

            public void Dispose() {
                _options.ForEach(option => option.Dispose());
            }
        }
    }

}
