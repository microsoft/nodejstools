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

using System.Text.RegularExpressions;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Classifier;
using Microsoft.NodejsTools.Debugger.DataTips;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.NodejsTools.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TextManager.Interop;
using TestUtilities.Mocks;

namespace NodejsTests.Debugger.FileNameMapping {
    [TestClass]
    public class DataTipTests {
        private void DataTipTest(string input, string selectionRegex, string expectedDataTip) {
            var buffer = new MockTextBuffer(input, @"C:\fob.js", "Node.js");
            var view = new MockTextView(buffer);

            var classifierProvider = new NodejsClassifierProvider(new MockContentTypeRegistryService(NodejsConstants.Nodejs));
            classifierProvider._classificationRegistry = new MockClassificationTypeRegistryService();
            classifierProvider.GetClassifier(buffer);

            var analyzer = new VsProjectAnalyzer(AnalysisLevel.NodeLsHigh, true);
            buffer.AddProperty(typeof(VsProjectAnalyzer), analyzer);
            analyzer.AddBuffer(buffer);
            analyzer.WaitForCompleteAnalysis();

            var m = Regex.Match(input, selectionRegex);
            Assert.IsTrue(m.Success);

            var startPos = m.Index;
            var startLine = buffer.CurrentSnapshot.GetLineFromPosition(startPos);
            var endPos = m.Index + m.Length;
            var endLine = buffer.CurrentSnapshot.GetLineFromPosition(endPos);
            var selectionSpan = new TextSpan {
                iStartLine = startLine.LineNumber,
                iStartIndex = startPos - startLine.Start.Position,
                iEndLine = endLine.LineNumber,
                iEndIndex = endPos - endLine.Start.Position
            };

            var dataTipSpan = DataTipTextViewFilter.GetDataTipSpan(view, selectionSpan);
            if (expectedDataTip == null) {
                Assert.IsNull(dataTipSpan);
                return;
            }

            Assert.IsNotNull(dataTipSpan);
            var actualSpan = dataTipSpan.Value;

            startPos = input.IndexOf(expectedDataTip);
            Assert.AreNotEqual(-1, startPos);
            startLine = buffer.CurrentSnapshot.GetLineFromPosition(startPos);
            endPos = startPos + expectedDataTip.Length;
            endLine = buffer.CurrentSnapshot.GetLineFromPosition(endPos);
            var expectedSpan = new TextSpan {
                iStartLine = startLine.LineNumber,
                iStartIndex = startPos - startLine.Start.Position,
                iEndLine = endLine.LineNumber,
                iEndIndex = endPos - endLine.Start.Position
            };

            // TextSpan doesn't override ToString, so test output is unusable in case of failure when comparing
            // two spans directly - use an anonymous type instead to produce pretty output.
            Assert.AreEqual(
                new { expectedSpan.iStartLine, expectedSpan.iStartIndex, expectedSpan.iEndLine, expectedSpan.iEndIndex },
                new { actualSpan.iStartLine, actualSpan.iStartIndex, actualSpan.iEndLine, actualSpan.iEndIndex });
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void DataTipStandAloneVariable() {
            const string code = "start; middle; end";

            DataTipTest(code, @"(?=start)", "start");
            DataTipTest(code, @"(?=end)", "end");

            DataTipTest(code, @"(?=middle)", "middle");
            DataTipTest(code, @"(?<=mid)(?=dle)", "middle");
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void DataTipSelection() {
            const string code = "abc";

            DataTipTest(code, @"a(?=bc)", "abc");
            DataTipTest(code, @"ab(?=c)", "abc");
            DataTipTest(code, @"abc", "abc");
            DataTipTest(code, @"(?<=a)b(?=c)", "abc");
            DataTipTest(code, @"(?<=ab)c", "abc");
            DataTipTest(code, @"(?<=a)bc", "abc");
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void DataTipPropertyAccess() {
            const string code = "a.b[1-'2'].c";

            DataTipTest(code, @"(?=a)", "a");
            DataTipTest(code, @"(?=b)", "a.b");
            DataTipTest(code, @"(?=])", "a.b[1-'2']");
            DataTipTest(code, @"(?=c)", "a.b[1-'2'].c");
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void DataTipParens() {
            const string code = "((a-b)*(c-d))";

            DataTipTest(code, @"(?<=b)", "(a-b)");
            DataTipTest(code, @"(?<=d)", "(c-d)");
            DataTipTest(code, @"(?<=d\))", code);
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void DataTipNoSideEffects() {
            const string code = "(1 - f(x).y - 2).z";

            DataTipTest(code, @"(?=f)", "f");
            DataTipTest(code, @"(?=x)", "x");

            DataTipTest(code, @"(?<=x)", null);
            DataTipTest(code, @"(?=y)", null);
            DataTipTest(code, @"(?<=2)", null);
            DataTipTest(code, @"(?=z)", null);
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void DataTipSingleLineComment() {
            const string code = "/*a*/b/*c*/.d";

            DataTipTest(code, @"(?=a)", null);
            DataTipTest(code, @"(?=b)", "b");
            DataTipTest(code, @"(?=c)", "b/*c*/");
            DataTipTest(code, @"(?=d)", "b/*c*/.d");
        }

        [TestMethod, Priority(0), TestCategory("Debugging"), TestCategory("Ignore")]
        public void DataTipMultiLineComment() {
            const string code = "//a\r\nb//c\r\n.d";

            DataTipTest(code, @"(?=a)", null);
            DataTipTest(code, @"(?=b)", "b");
            DataTipTest(code, @"(?=c)", "b//c");
            DataTipTest(code, @"(?=d)", "b//c\r\n.d");
        }
    }
}

