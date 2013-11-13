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
using System.Windows.Forms;
using EnvDTE;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

}")
        );

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void SmartIndentBug384() {
            using (var solution = BasicProject.Generate().ToVs()) {                
                var doc = solution.OpenItem("AutoIndent", "Bug384.js");
                doc.Invoke(() => doc.TextView.Caret.MoveTo(doc.TextView.TextViewLines[9]));
                Keyboard.Type(System.Windows.Input.Key.End);
                Assert.AreEqual(
                    4,
                    doc.TextView.Caret.Position.VirtualBufferPosition.VirtualSpaces
                );
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void SmartIndentBasic() {
#if DEV12_OR_LATER
            var props = VsIdeTestHostContext.Dte.get_Properties("TextEditor", "Node.js");
            bool? oldValue = null;
            try {
                oldValue = (bool)props.Item("BraceCompletion").Value;
                props.Item("BraceCompletion").Value = false;
#endif
                var testCases = new[] {
                // no {
                new { 
                    Typed = "if (true)\r42\relse\r100\r200",
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
                    Expected = @"while(true)
    42
100"              
                },
                new { 
                    Typed = "do\r42\rwhile(false)\r100",
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
}"              },
                // nested function, dedent keyword
                new { 
                    Typed = "function a() {\rfunction b() {\rfoo\r\b}\r\b}",
                    Expected = @"function a() {
    function b() {
        foo
    }
}"              },
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
bar*/
}"
                },
                // auto dedent after return
                new {
                    Typed = "if (true) {\rreturn\r}",
                    Expected = @"if (true) {
    return
}"
                },
                // auto dedent after return;
                new {
                    Typed = "if (true) {\rreturn;\r}",
                    Expected = @"if (true) {
    return;
}"
                },
                // auto dedent after return;
                new {
                    Typed = "if (true) {\rreturn;;\r}",
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
#if DEV12_OR_LATER
            } finally {
                if (oldValue != null) {
                    props.Item("BraceCompletion").Value = oldValue;
                }
            }
#endif
        }

        private static void AutoIndentTest(VisualStudioSolution solution, string typedText, string expectedText) {
            var doc = solution.OpenItem("AutoIndent", "server.js");

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

            solution.App.Dte.ActiveWindow.Close(vsSaveChanges.vsSaveChangesNo);
        }
    }
}
