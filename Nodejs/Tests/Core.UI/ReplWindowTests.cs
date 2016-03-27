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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudio.Language.Intellisense;
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

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void TestErrorNewLine() {
            Test((app, window) => {
                Keyboard.Type("abc\r");
                window.WaitForText("> abc", "ReferenceError: abc is not defined", "> ");
                Keyboard.Type("42\r");
                window.WaitForText("> abc", "ReferenceError: abc is not defined", "> 42", "42", "> ");
            });
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void TestColorOutput() {
            Test((app, window) => {
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
            });
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void TestStdErrIsRed() {
            Test((app, window) => {
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
            });
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void TestCompletion() {
            Test((app, window) => {
                Keyboard.Type("''.");
                using (var session = window.WaitForSession<ICompletionSession>()) {
                    Assert.IsTrue(session.Session.CompletionSets.First().Completions.Any(x => x.InsertionText == "length"));
                }
            });
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void TestNoSpecialCommandCompletion() {
            Test((app, window) => {
                Keyboard.Type(".");
                window.AssertNoIntellisenseSession();
            });
        }

        /// <summary>
        /// Opens the interactive window, clears the screen.
        /// </summary>
        internal void Test(Action<NodejsVisualStudioApp, InteractiveWindow> body) {
            using (var app = new NodejsVisualStudioApp()) {
                app.SuppressCloseAllOnDispose();

                const string interpreterDescription = "Node.js Interactive Window";
                app.Dte.ExecuteCommand("View.Node.jsInteractiveWindow");
                var interactive = app.GetInteractiveWindow(interpreterDescription);
                if (interactive == null) {
                    Assert.Inconclusive("Need " + interpreterDescription);
                }
                interactive.WaitForIdleState();
                app.Element.SetFocus();
                interactive.Element.SetFocus();

                interactive.ClearInput();

                bool isReady = false;
                for (int retries = 10; retries > 0; --retries) {
                    interactive.Reset();
                    try {
                        var task = interactive.ReplWindow.Evaluator.ExecuteText("console.log('READY')");
                        Assert.IsTrue(task.Wait(10000), "ReplWindow did not initialize in time");
                        if (!task.Result.IsSuccessful) {
                            continue;
                        }
                    } catch (TaskCanceledException) {
                        continue;
                    }

                    interactive.WaitForTextEnd("READY", "undefined", "> ");
                    isReady = true;
                    break;
                }
                Assert.IsTrue(isReady, "ReplWindow did not initialize");

                interactive.ClearScreen();
                interactive.ReplWindow.ClearHistory();

                body(app, interactive);

                interactive.ClearScreen();
                interactive.ReplWindow.ClearHistory();
            }
        }
    }

}
