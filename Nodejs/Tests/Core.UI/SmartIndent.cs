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
using EnvDTE;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.SharedProject;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class SmartIndent : NodejsProjectTest {
        public static ProjectDefinition BasicProject = new ProjectDefinition("AutoIndent", NodejsProject, Compile("server", ""));

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void SmartIndentBasic() {
            var testCases = new[] {
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
