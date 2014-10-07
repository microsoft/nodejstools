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
        private static readonly FormattingOptions Options = new FormattingOptions() {
            SpaceAfterComma = true,
            SpaceAfterKeywordsInControlFlowStatements = true,
            SpaceAfterSemiColonInFor = true,
            SpaceBeforeAndAfterBinaryOperator = true,
            SpaceAfterFunctionInAnonymousFunctions = false,
            SpacesPerIndent = 4,
            NewLine = "\n"
        };

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            ExtractResource("ruleFormattingTests.json");
        }

        private static void ExtractResource(string file) {
            //if (!File.Exists(file)) 
            {
                using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("NodejsTests." + file))) {
                    File.WriteAllText(
                        file,
                        reader.ReadToEnd()
                    );
                }
            }
        }

        [TestMethod, Priority(0)]
        public void TypeScriptPortedTests() {
            dynamic testCases = _serializer.DeserializeObject("[" + File.ReadAllText("ruleFormattingTests.json") + "]");
            
            var options = new FormattingOptions();
            int testCaseId = 0;
            foreach (var testCase in testCases) {
                string filename = "fob" + testCaseId++ + ".js";
                Console.WriteLine(testCase["input"]);
                var buffer = new MockTextBuffer(testCase["input"], "test.py", "Node.js");
                if (!ApplyEdits(filename, testCase, buffer)) {
                    continue;
                }

                Assert.AreEqual(
                    ((string)testCase["expected"]).Replace("\r\n", "\n").Replace("\n", "\r\n"),
                    buffer.CurrentSnapshot.GetText().Replace("\r\n", "\n").Replace("\n", "\r\n")
                );
            }
        }

        private static bool ApplyEdits(string filename, dynamic testCase, MockTextBuffer buffer) {
            foreach (var operation in testCase["operations"]) {
                Edit[] edits = null;
                switch ((string)operation["operation"]) {
                    case "Document":
                        edits = Formatter.GetEditsForDocument(
                            buffer.CurrentSnapshot.GetText(),
                            Options
                        );
                        break;
                    case "CloseBrace":
                        edits = Formatter.GetEditsAfterKeystroke(
                            buffer.CurrentSnapshot.GetText(),
                            (int)operation["point"]["position"],
                           '}',
                            Options
                        );
                        break;
                    case "Enter":
                        var line = buffer.CurrentSnapshot.GetLineFromPosition((int)operation["point"]["position"]);
                        if(line.LineNumber > 0) {
                            edits = Formatter.GetEditsAfterEnter(
                                buffer.CurrentSnapshot.GetText(),
                                buffer.CurrentSnapshot.GetLineFromLineNumber(line.LineNumber - 1).Start,
                                line.End,
                                Options
                            );
                        }
                        break;
                    case "Semicolon":
                        edits = Formatter.GetEditsAfterKeystroke(
                            buffer.CurrentSnapshot.GetText(),
                            (int)operation["point"]["position"],
                           ';',
                            Options
                        );
                        break;
                    case "Paste":
                        edits = Formatter.GetEditsForRange(
                            buffer.CurrentSnapshot.GetText(),
                            (int)operation["span"]["start"],
                            (int)operation["span"]["start"] + (int)operation["span"]["length"],
                            Options
                        );
                        break;
                    case "Selection":
                        edits = Formatter.GetEditsForRange(
                            buffer.CurrentSnapshot.GetText(),
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

        private static void FormatDocumentTest(string input, string expected, FormattingOptions options = null) {
            string pathToFile = "fob.py";
            var buffer = new MockTextBuffer(input, pathToFile, "Node.js");
            var edits = Formatter.GetEditsForDocument(
                buffer.CurrentSnapshot.GetText(),
                options ?? new FormattingOptions()
            );

            EditFilter.ApplyEdits(buffer, edits);
            Assert.AreEqual(expected, buffer.CurrentSnapshot.GetText());
        }

        private static void FormatSelectionTest(string input, string expected, int start, int end, FormattingOptions options = null) {
            string pathToFile = "fob.py";
            var buffer = new MockTextBuffer(input, pathToFile, "Node.js");
            var edits = Formatter.GetEditsForRange(
                buffer.CurrentSnapshot.GetText(),
                start,
                end,
                options ?? new FormattingOptions()
            );
            EditFilter.ApplyEdits(buffer, edits);
            Assert.AreEqual(expected, buffer.CurrentSnapshot.GetText());
        }
    }
}
