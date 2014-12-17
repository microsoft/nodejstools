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

using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Classifier;
using Microsoft.NodejsTools.Editor.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using TestUtilities;
using TestUtilities.Mocks;

namespace NodejsTests {
    [TestClass]
    public class MultilineCommentTests {

        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
            AssertListener.Initialize();
        }

        [TestMethod, Priority(0)]
        public void MultilineComment() {
            TestMultilineCommenting(
                "/*<enter>",
                "/*\r\n" +
                " * ");
        }

        [TestMethod, Priority(0)]
        public void MultilineCommentCurrentLine() {
            TestMultilineCommenting(
                "console.log('Hello'); /*<enter>\r\n",
                "console.log('Hello'); /*\r\n" +
                "                       * \r\n");
        }

        [TestMethod, Priority(0)]
        public void MultilineCommentBlankLine() {
            TestMultilineCommenting(
                "            /*<enter>\r\n",

                "            /*\r\n" +
                "             * \r\n");
        }

        [TestMethod, Priority(0)]
        public void MultilineCommentOddIndenting() {
            TestMultilineCommenting(
                "            /*\r\n" +
                "                        *   some words and blank space     <enter>",

                "            /*\r\n" +
                "                        *   some words and blank space     \r\n" +
                "             *   ");
        }

        [TestMethod, Priority(0)]
        public void MultilineCommentTabRemovalAndOddIndenting() {
            TestMultilineCommenting(
                "   \t         /*\r\n" +
                "     *    \tsome words and blank space     <enter>",

                "   \t         /*\r\n" +
                "     *    \tsome words and blank space     \r\n" +
                "              *      ");
        }

        [TestMethod, Priority(0)]
        public void MultilineCommentAdjacentCommentsIndent() {
            TestMultilineCommenting(
                "   /*\r\n" +
                "    * \r\n" +
                "    */\r\n" +
                "        /*\r\n" +
                "          * <enter>\r\n" +
                "            */"
,
                "   /*\r\n" +
                "    * \r\n" +
                "    */\r\n" +
                "        /*\r\n" +
                "          * \r\n" +
                "         * \r\n" +
                "            */");

        }

        [TestMethod, Priority(0)]
        public void MultilineCommentEmbeddedCommentsFormatting() {
            TestMultilineCommenting(
                "                /*\r\n" +
                "                 * \r\n" +
                "                 * \r\n" +
                "                 * /*\r\n" +
                "                 *               /*\r\n" +
                "                 *               <enter>          /*\r\n" +
                "                 *                          /*\r\n" +
                "                 *                          */\r\n",

                "                /*\r\n" +
                "                 * \r\n" +
                "                 * \r\n" +
                "                 * /*\r\n" +
                "                 *               /*\r\n" +
                "                 *               \r\n" +
                "                 *           /*\r\n" +
                "                 *                          /*\r\n" +
                "                 *                          */\r\n");
        }

        [TestMethod, Priority(0)]
        public void MultilineCommentSingleLineCommentsUnaffected() {
            TestMultilineCommenting(
                "   \t//         /*<enter>",
                "   \t//         /*\r\n",
                containsMultilineComment: false);

        }

        private void TestMultilineCommenting(string startingText, string expectedEndingText, bool containsMultilineComment = true) {
            const string insertionPointString = "<enter>";

            // Find insertion point and fake enter key by putting \r\n before the insertion point.
            startingText = startingText.Replace(insertionPointString, string.Format("\r\n{0}", insertionPointString));

            // Take input text and find index of the enter.
            int insertionPosition = startingText.IndexOf(insertionPointString);
            startingText = startingText.Remove(insertionPosition, insertionPointString.Length);

            // create the view and request a multiline comment format
            var view = new MockTextView(
                new MockTextBuffer(startingText, "C:\\app.js", NodejsConstants.Nodejs));
            var insertionPoint = new SnapshotPoint(view.TextSnapshot, insertionPosition);

            // Setup mock registry service and classification provider for the IsMultilineComment method.
            var classifierProvider = new NodejsClassifierProvider(new MockContentTypeRegistryService());
            classifierProvider._classificationRegistry = new MockClassificationTypeRegistryService();
            var classifier = classifierProvider.GetClassifier(view.TextBuffer);

            SnapshotSpan commentSpan;
            if (insertionPoint.IsMultilineComment(out commentSpan)) {
                view.FormatMultilineComment(commentSpan.Start, insertionPoint);
            } else if (containsMultilineComment) {
                Assert.Fail("This was not seen as a comment.  Something went wrong");
            }

            Assert.AreEqual(expectedEndingText, view.TextBuffer.CurrentSnapshot.GetText());
        }
    }
}
