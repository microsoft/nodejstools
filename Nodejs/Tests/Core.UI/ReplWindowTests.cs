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

using System.Collections.Generic;
using System.Threading;
using Microsoft.NodejsTools.Repl;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using TestUtilities;
using TestUtilities.UI;
using TestUtilities.UI.Nodejs;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class ReplWindowTests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestErrorNewLine() {
            var window = Prepare();
            Keyboard.Type("abc\r");
            window.WaitForText("> abc", "ReferenceError: abc is not defined", "> ");
            Keyboard.Type("42\r");
            window.WaitForText("> abc", "ReferenceError: abc is not defined", "> 42", "42", "> ");
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestColorOutput() {
            var window = Prepare();
            Keyboard.Type("[1,2,3]\r");
            window.WaitForText("> [1,2,3]", "[ 1, 2, 3 ]", "> ");

            IList<ClassificationSpan> spans = null;
            window.Invoke(() => {
                var snapshot = window.TextView.TextBuffer.CurrentSnapshot;
                spans = window.Classifier.GetClassificationSpans(new SnapshotSpan(snapshot, 0, snapshot.Length));
            });
            
            Classification.Verify(spans,
                new Classification("operator", 2, 3, "["),
                new Classification("number", 3, 4, "1"),
                new Classification("operator", 4, 5, ","),
                new Classification("number", 5, 6, "2"),
                new Classification("operator", 6, 7, ","),
                new Classification("number", 7, 8, "3"),
                new Classification("operator", 8, 9, "]"),
                new Classification("Node.js Interactive - Black", 11, 13, "[ "),
                new Classification("Node.js Interactive - Blue", 13, 14, "1"),
                new Classification("Node.js Interactive - Black", 14, 16, ", "),
                new Classification("Node.js Interactive - Blue", 16, 17, "2"),
                new Classification("Node.js Interactive - Black", 17, 19, ", "),
                new Classification("Node.js Interactive - Blue", 19, 20, "3"),
                new Classification("Node.js Interactive - Black", 20, 24, " ]\r\n")
            );
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ReplStdErr() {
            var window = Prepare();
            window.ReplWindow.Evaluator.ExecuteText("setTimeout(function () { not_a_fn(); }, 0);\r").Wait();

            bool receivedError = false;
            for (int retries = 10; retries >= 0; --retries) {
                // Text from stderr takes longer to appear than the console.log()
                // message, so we need to wait for it.

                IList<ClassificationSpan> spans = null;
                window.Invoke(() => {
                    var snapshot = window.TextView.TextBuffer.CurrentSnapshot;
                    spans = window.Classifier.GetClassificationSpans(new SnapshotSpan(snapshot, 0, snapshot.Length));
                });

                if (spans.Count > 0 && "Node.js Interactive - Red" == spans[spans.Count - 1].ClassificationType.Classification) {
                    receivedError = true;
                    break;
                }
                Thread.Sleep(500);
            }            
            Assert.IsTrue(receivedError, "Did not get text from stderr");
            //Flush the screen once the test is done
            //This way output isn't left which could lead to confusion
            window.ReplWindow.ClearScreen();
        }


        /// <summary>
        /// Opens the interactive window, clears the screen.
        /// </summary>
        internal InteractiveWindow Prepare(bool reopenOnly = false) {
            using (var app = new NodejsVisualStudioApp(VsIdeTestHostContext.Dte)) {
                app.SuppressCloseAllOnDispose();

                const string interpreterDescription = "Node.js Interactive Window";
                VsIdeTestHostContext.Dte.ExecuteCommand("View.Node.jsInteractiveWindow");
                var interactive = app.GetInteractiveWindow(interpreterDescription);
                if (interactive == null) {
                    Assert.Inconclusive("Need " + interpreterDescription);
                }
                interactive.WaitForIdleState();
                app.Element.SetFocus();
                interactive.Element.SetFocus();

                if (!reopenOnly) {
                    interactive.ClearScreen();
                    interactive.ReplWindow.ClearHistory();
                    interactive.WaitForReadyState();

                    interactive.Reset();
                    interactive.ClearScreen();
                    var task = interactive.ReplWindow.Evaluator.ExecuteText("console.log('READY')");
                    Assert.IsTrue(task.Wait(10000), "ReplWindow did not initialize in time");
                    Assert.AreEqual(ExecutionResult.Success, task.Result);
                    interactive.WaitForTextEnd("READY", "undefined", "> ");

                    interactive.ClearScreen();
                    interactive.ReplWindow.ClearHistory();
                }
                interactive.WaitForReadyState();
                return interactive;
            }
        }
    }

}
