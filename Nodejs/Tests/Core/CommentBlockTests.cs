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

using Microsoft.NodejsTools.Editor.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using TestUtilities.Mocks;

namespace NodejsTests {
    [TestClass]
    public class CommentBlockTests {
        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestCommentCurrentLine() {
            var view = new MockTextView(
                new MockTextBuffer(@"console.log('Hello');
console.log('Goodbye');"));

            view.Caret.MoveTo(view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(0).Start);

            view.CommentOrUncommentBlock(true);

            Assert.AreEqual(@"//console.log('Hello');
console.log('Goodbye');",
                view.TextBuffer.CurrentSnapshot.GetText());

            view.Caret.MoveTo(view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start);

            view.CommentOrUncommentBlock(true);

            Assert.AreEqual(@"//console.log('Hello');
//console.log('Goodbye');",
                 view.TextBuffer.CurrentSnapshot.GetText());
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestUnCommentCurrentLine() {
            var view = new MockTextView(
                new MockTextBuffer(@"//console.log('Hello');
//console.log('Goodbye');"));

            view.Caret.MoveTo(view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(0).Start);

            view.CommentOrUncommentBlock(false);

            Assert.AreEqual(@"console.log('Hello');
//console.log('Goodbye');",
                 view.TextBuffer.CurrentSnapshot.GetText());

            view.Caret.MoveTo(view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start);

            view.CommentOrUncommentBlock(false);

            Assert.AreEqual(@"console.log('Hello');
console.log('Goodbye');",
                view.TextBuffer.CurrentSnapshot.GetText());
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestComment() {
            var view = new MockTextView(
                new MockTextBuffer(@"console.log('Hello');
console.log('Goodbye');"));

            view.Selection.Select(
                new SnapshotSpan(view.TextBuffer.CurrentSnapshot, new Span(0, view.TextBuffer.CurrentSnapshot.Length)),
                false
            );

            view.CommentOrUncommentBlock(true);

            Assert.AreEqual(@"//console.log('Hello');
//console.log('Goodbye');",
                 view.TextBuffer.CurrentSnapshot.GetText());
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestCommentEmptyLine() {
            var view = new MockTextView(
                new MockTextBuffer(@"console.log('Hello');

console.log('Goodbye');"));

            view.Selection.Select(
                new SnapshotSpan(view.TextBuffer.CurrentSnapshot, new Span(0, view.TextBuffer.CurrentSnapshot.Length)),
                false
            );

            view.CommentOrUncommentBlock(true);

            Assert.AreEqual(@"//console.log('Hello');

//console.log('Goodbye');",
                 view.TextBuffer.CurrentSnapshot.GetText());
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestCommentWhiteSpaceLine() {
            var view = new MockTextView(
                new MockTextBuffer(@"console.log('Hello');
   
console.log('Goodbye');"));

            view.Selection.Select(
                new SnapshotSpan(view.TextBuffer.CurrentSnapshot, new Span(0, view.TextBuffer.CurrentSnapshot.Length)),
                false
            );

            view.CommentOrUncommentBlock(true);

            Assert.AreEqual(@"//console.log('Hello');
   
//console.log('Goodbye');",
                 view.TextBuffer.CurrentSnapshot.GetText());
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestCommentIndented() {
            var view = new MockTextView(
                new MockTextBuffer(@"function f(){
    console.log('Hello');
    console.log('Still here');
    console.log('Goodbye');
}"));

            view.Selection.Select(
                new SnapshotSpan(
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start,
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(2).End
                ),
                false
            );

            view.CommentOrUncommentBlock(true);

            Assert.AreEqual(@"function f(){
    //console.log('Hello');
    //console.log('Still here');
    console.log('Goodbye');
}",
                    view.TextBuffer.CurrentSnapshot.GetText());
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestCommentIndentedBlankLine() {
            var view = new MockTextView(
                new MockTextBuffer(@"function f(){
    console.log('Hello');

    console.log('Still here');
    console.log('Goodbye');
}"));

            view.Selection.Select(
                new SnapshotSpan(
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start,
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(3).End
                ),
                false
            );

            view.CommentOrUncommentBlock(true);

            Assert.AreEqual(@"function f(){
    //console.log('Hello');

    //console.log('Still here');
    console.log('Goodbye');
}",
                    view.TextBuffer.CurrentSnapshot.GetText());
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestCommentBlankLine() {
            var view = new MockTextView(
                new MockTextBuffer(@"console.log('hi');

console.log('bye');"));

            view.Caret.MoveTo(view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start);

            view.CommentOrUncommentBlock(true);

            Assert.AreEqual(@"console.log('hi');

console.log('bye');",
             view.TextBuffer.CurrentSnapshot.GetText());
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestCommentIndentedWhiteSpaceLine() {
            var view = new MockTextView(
                new MockTextBuffer(@"function f(){
    console.log('Hello');
  
    console.log('Still here');
    console.log('Goodbye');
}"));

            view.Selection.Select(
                new SnapshotSpan(
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start,
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(3).End
                ),
                false
            );

            view.CommentOrUncommentBlock(true);

            Assert.AreEqual(@"function f(){
    //console.log('Hello');
  
    //console.log('Still here');
    console.log('Goodbye');
}",
                    view.TextBuffer.CurrentSnapshot.GetText());
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestUnCommentIndented() {
            var view = new MockTextView(
                new MockTextBuffer(@"function f(){
    //console.log('Hello');
    //console.log('Still here');
    console.log('Goodbye');
}"));

            view.Selection.Select(
                new SnapshotSpan(
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start,
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(2).End
                ),
                false
            );

            view.CommentOrUncommentBlock(false);

            Assert.AreEqual(@"function f(){
    console.log('Hello');
    console.log('Still here');
    console.log('Goodbye');
}",
                    view.TextBuffer.CurrentSnapshot.GetText());
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestUnComment() {
            var view = new MockTextView(
                new MockTextBuffer(@"//console.log('Hello');
//console.log('Goodbye');"));

            view.Selection.Select(
                new SnapshotSpan(view.TextBuffer.CurrentSnapshot, new Span(0, view.TextBuffer.CurrentSnapshot.Length)),
                false
            );

            view.CommentOrUncommentBlock(false);

            Assert.AreEqual(@"console.log('Hello');
console.log('Goodbye');",
                view.TextBuffer.CurrentSnapshot.GetText());
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestCommentStartOfLastLine() {
            var view = new MockTextView(
                new MockTextBuffer(@"console.log('Hello');
console.log('Goodbye');"));

            view.Selection.Select(
                new SnapshotSpan(view.TextBuffer.CurrentSnapshot, new Span(0, view.TextBuffer.CurrentSnapshot.GetText().IndexOf("console.log('Goodbye');"))),
                false
            );

            view.CommentOrUncommentBlock(true);

            Assert.AreEqual(@"//console.log('Hello');
console.log('Goodbye');",
                view.TextBuffer.CurrentSnapshot.GetText());
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestCommentAfterCodeIsNotUncommented() {
            var view = new MockTextView(
                new MockTextBuffer(@"console.log('Hello');//comment that should stay a comment;
//console.log('Still here');//another comment that should stay a comment;
console.log('Goodbye');"));

            view.Selection.Select(
                new SnapshotSpan(view.TextBuffer.CurrentSnapshot, new Span(0, view.TextBuffer.CurrentSnapshot.GetText().IndexOf("console.log('Goodbye');"))),
                false
            );

            view.CommentOrUncommentBlock(false);

            Assert.AreEqual(@"console.log('Hello');//comment that should stay a comment;
console.log('Still here');//another comment that should stay a comment;
console.log('Goodbye');",
                view.TextBuffer.CurrentSnapshot.GetText());
        }
    }
}
