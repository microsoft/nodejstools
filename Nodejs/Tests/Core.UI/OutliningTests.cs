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
using System.Diagnostics;
using Microsoft.NodejsTools;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using TestUtilities;
using TestUtilities.Nodejs;
using TestUtilities.UI;

namespace NodejsTests {
    [TestClass]
    public class OutliningTests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
            if (NodejsPackage.Instance == null) {
                // The REPL is open on restart, but our package isn't loaded yet.  Force
                // it to load now.
                var shell = (IVsShell)NodejsPackage.GetGlobalService(typeof(SVsShell));
                Guid packageGuid = typeof(NodejsPackage).GUID;
                IVsPackage package;
                if (ErrorHandler.Failed(shell.LoadPackage(ref packageGuid, out package))) {
                    return;
                }
                Debug.Assert(NodejsPackage.Instance != null);
            }
        }

        [TestMethod, Priority(0)]
        [HostType("VSTestHost")]
        public void OutliningToplevelFunctionDefinitions() {
            OutlineTest(
                "toplevel.js",
                new ExpectedTag(25, 90, @" {
    // function object
    console.log('function object')
}"),
  new ExpectedTag(130, 163, @" {
    // function expression
}"));
        }

        [TestMethod, Priority(0)]
        [HostType("VSTestHost")]
        public void OutliningNestedFunctionDefinitions() {
            OutlineTest("nested.js",
                new ExpectedTag(25, 280, @" {
    console.log('Function object')
    var nestedFunctionExpression = function () {
        console.log('Nested function expression')
        function nestedFunctionObject() {
            console.log('Nested function object');
        }
    }
}"),

                new ExpectedTag(111, 277, @" {
        console.log('Nested function expression')
        function nestedFunctionObject() {
            console.log('Nested function object');
        }
    }"),
                new ExpectedTag(205, 270, @" {
            console.log('Nested function object');
        }"));
        }

        [TestMethod, Priority(0)]
        [HostType("VSTestHost")]
        public void OutliningBadInput() {
            // there should be no exceptions and no outlining when parsing a malformed for statement
            OutlineTest("broken.js");
        }

        private void OutlineTest(string filename, params ExpectedTag[] expected) {
            using (var app = new VisualStudioApp()) {
                var prevOption = NodejsPackage.Instance.AdvancedEditorOptionsPage.EnterOutliningOnOpen;
                try {
                    NodejsPackage.Instance.AdvancedEditorOptionsPage.EnterOutliningOnOpen = true;
                    
                    var project = app.OpenProject(@"TestData\Outlining\Outlining.sln");

                    var item = project.ProjectItems.Item(filename);
                    var window = item.Open();
                    window.Activate();

                    System.Threading.Thread.Sleep(2000);

                    var doc = app.GetDocument(item.Document.FullName);
                    var snapshot = doc.TextView.TextBuffer.CurrentSnapshot;
                    var tags = doc.GetTaggerAggregator<IOutliningRegionTag>(doc.TextView.TextBuffer).GetTags(new SnapshotSpan(snapshot, 0, snapshot.Length));

                    VerifyTags(doc.TextView.TextBuffer, tags, expected);
                }
                finally {
                    NodejsPackage.Instance.AdvancedEditorOptionsPage.EnterOutliningOnOpen = prevOption;
                }
            }
        }

        private void VerifyTags(ITextBuffer buffer, IEnumerable<IMappingTagSpan<IOutliningRegionTag>> tags, params ExpectedTag[] expected) {
            var ltags = new List<IMappingTagSpan<IOutliningRegionTag>>(tags);

            foreach (var tag in ltags) {
                int start = tag.Span.Start.GetInsertionPoint(x => x == buffer).Value.Position;
                int end = tag.Span.End.GetInsertionPoint(x => x == buffer).Value.Position;
                Console.WriteLine("new ExpectedTag({0}, {1}, \"{2}\"),",
                    start,
                    end,
                    Classification.FormatString(buffer.CurrentSnapshot.GetText(Span.FromBounds(start, end)))
                );
            }
            Assert.AreEqual(expected.Length, ltags.Count);

            for (int i = 0; i < ltags.Count; i++) {
                int start = ltags[i].Span.Start.GetInsertionPoint(x => x == buffer).Value.Position;
                int end = ltags[i].Span.End.GetInsertionPoint(x => x == buffer).Value.Position;
                Assert.AreEqual(expected[i].Start, start);
                Assert.AreEqual(expected[i].End, end);
                Assert.AreEqual(expected[i].Text, buffer.CurrentSnapshot.GetText(Span.FromBounds(start, end)));
                Assert.AreEqual(ltags[i].Tag.IsImplementation, true);
            }
        }

        private class ExpectedTag {
            public readonly int Start, End;
            public readonly string Text;

            public ExpectedTag(int start, int end, string text) {
                Start = start;
                End = end;
                Text = text;
            }
        }
    }
}
