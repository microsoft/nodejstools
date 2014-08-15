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

using System.Text.RegularExpressions;
using Microsoft.NodejsTools.Classifier;
using Microsoft.NodejsTools.Debugger.DataTips;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TextManager.Interop;
using TestUtilities.Mocks;

namespace NodejsTests.Debugger.FileNameMapping {
    [TestClass]
    public class DataTipTests {
        private void DataTipTest(string input, string selectionRegex, string expectedDataTip) {
            var buffer = new MockTextBuffer(input, @"C:\fob.js", "Node.js");
            var view = new MockTextView(buffer);

            var classifierProvider = new NodejsClassifierProvider(new MockContentTypeRegistryService());
            classifierProvider._classificationRegistry = new MockClassificationTypeRegistryService();
            classifierProvider.GetClassifier(buffer);

            var analyzer = new VsProjectAnalyzer();
            buffer.AddProperty(typeof(VsProjectAnalyzer), analyzer);
            analyzer.MonitorTextView(view, new[] { buffer });
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

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void DataTipStandAloneVariable() {
            const string code = "start; middle; end";

            DataTipTest(code, @"(?=start)", "start");
            DataTipTest(code, @"(?=end)", "end");

            DataTipTest(code, @"(?=middle)", "middle");
            DataTipTest(code, @"(?<=mid)(?=dle)", "middle");
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void DataTipSelection() {
            const string code = "abc";

            DataTipTest(code, @"a(?=bc)", "abc");
            DataTipTest(code, @"ab(?=c)", "abc");
            DataTipTest(code, @"abc", "abc");
            DataTipTest(code, @"(?<=a)b(?=c)", "abc");
            DataTipTest(code, @"(?<=ab)c", "abc");
            DataTipTest(code, @"(?<=a)bc", "abc");
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void DataTipPropertyAccess() {
            const string code = "a.b[1-'2'].c";

            DataTipTest(code, @"(?=a)", "a");
            DataTipTest(code, @"(?=b)", "a.b");
            DataTipTest(code, @"(?=])", "a.b[1-'2']");
            DataTipTest(code, @"(?=c)", "a.b[1-'2'].c");
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void DataTipParens() {
            const string code = "((a-b)*(c-d))";

            DataTipTest(code, @"(?<=b)", "(a-b)");
            DataTipTest(code, @"(?<=d)", "(c-d)");
            DataTipTest(code, @"(?<=d\))", code);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void DataTipNoSideEffects() {
            const string code = "(1 - f(x).y - 2).z";

            DataTipTest(code, @"(?=f)", "f");
            DataTipTest(code, @"(?=x)", "x");

            DataTipTest(code, @"(?<=x)", null);
            DataTipTest(code, @"(?=y)", null);
            DataTipTest(code, @"(?<=2)", null);
            DataTipTest(code, @"(?=z)", null);
        }
    }
}

