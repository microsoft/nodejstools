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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Mocks;
using System.Web.Script.Serialization;

namespace NodejsTests {
    [TestClass]
    public sealed class FormattingTests {
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private static readonly FormatCodeOptions Options = new FormatCodeOptions() {
            InsertSpaceAfterCommaDelimiter = true,
            InsertSpaceAfterKeywordsInControlFlowStatements = true,
            InsertSpaceAfterSemicolonInForStatements = true,
            InsertSpaceBeforeAndAfterBinaryOperators = true,
            InsertSpaceAfterFunctionKeywordForAnonymousFunctions = false,
            TabSize = 4, IndentSize = 4,
            ConvertTabsToSpaces = true,
            NewLineCharacter = "\n"
        };

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            ExtractResource("typescriptServices.js");
            ExtractResource("ruleFormattingTests.json");
        }

        private static void ExtractResource(string file) {
            if (!File.Exists(file)) {
                File.WriteAllText(
                    file,
                    new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("NodejsTests." + file)).ReadToEnd()
                );
            }
        }

        [TestMethod, Priority(0)]
        public void TypeScriptPortedTests() {
            dynamic testCases = _serializer.DeserializeObject("[" + File.ReadAllText("ruleFormattingTests.json") + "]");

            var inst = JavaScriptFormattingService.Instance;
            var options = new FormatCodeOptions();
            int testCaseId = 0;
            foreach (var testCase in testCases) {
                string filename = "fob" + testCaseId++ + ".js";

                var buffer = new MockTextBuffer(testCase["input"], filename, "Node.js");
                inst.AddDocument(filename, buffer);

                if (!ApplyEdits(filename, inst, testCase, buffer)) {
                    continue;
                }

                Assert.AreEqual(
                    ((string)testCase["expected"]).Replace("\r\n", "\n").Replace("\n", "\r\n"),
                    buffer.CurrentSnapshot.GetText().Replace("\r\n", "\n").Replace("\n", "\r\n")
                );
            }
        }

        private static bool ApplyEdits(string filename, JavaScriptFormattingService inst, dynamic testCase, MockTextBuffer buffer) {
            foreach (var operation in testCase["operations"]) {
                TextEdit[] edits = null;
                switch ((string)operation["operation"]) {
                    case "Document":
                        edits = inst.GetFormattingEditsForDocument(
                            filename,
                            0,
                            buffer.CurrentSnapshot.Length,
                            Options
                        );
                        break;
                    case "CloseBrace":
                        edits = inst.GetFormattingEditsAfterKeystroke(
                            filename,
                            (int)operation["point"]["position"],
                            "}",
                            Options
                        );
                        break;
                    case "Enter":
                        edits = inst.GetFormattingEditsAfterKeystroke(
                            filename,
                            (int)operation["point"]["position"],
                            "\r\n",
                            Options
                        );
                        break;
                    case "Semicolon":
                        edits = inst.GetFormattingEditsAfterKeystroke(
                            filename,
                            (int)operation["point"]["position"],
                            ";",
                            Options
                        );
                        break;
                    case "Paste":
                        edits = inst.GetFormattingEditsOnPaste(
                            filename,
                            (int)operation["span"]["start"],
                            (int)operation["span"]["start"] + (int)operation["span"]["length"],
                            Options
                        );
                        break;
                    case "Selection":
                        edits = inst.GetFormattingEditsForRange(
                            filename,
                            (int)operation["span"]["start"],
                            (int)operation["span"]["start"] + (int)operation["span"]["length"],
                            Options
                        );
                        break;
                    default:
                        Console.WriteLine("Skipping unsupported test case " + operation["operation"]);
                        return false;
                }

                EditFilter.ApplyEdits(buffer, edits);
            }
            return true;
        }

        private static void FormatDocumentTest(string input, string expected, FormatCodeOptions options = null) {
            var inst = JavaScriptFormattingService.Instance;
            var buffer = new MockTextBuffer(input, "fob.py", "Node.js");
            inst.AddDocument("fob.py", buffer);

            var edits = inst.GetFormattingEditsForDocument(
                "fob.py",
                0,
                buffer.CurrentSnapshot.Length,
                options ?? new FormatCodeOptions()
            );

            EditFilter.ApplyEdits(buffer, edits);
            Assert.AreEqual(expected, buffer.CurrentSnapshot.GetText());
        }

        private static void FormatSelectionTest(string input, string expected, int start, int end, FormatCodeOptions options = null) {
            var inst = JavaScriptFormattingService.Instance;
            var buffer = new MockTextBuffer(input, "fob.py", "Node.js");
            inst.AddDocument("fob.py", buffer);

            var edits = inst.GetFormattingEditsForRange(
                "fob.py",
                start,
                end,
                options ?? new FormatCodeOptions()
            );
            EditFilter.ApplyEdits(buffer, edits);
            Assert.AreEqual(expected, buffer.CurrentSnapshot.GetText());
        }
    }
}
