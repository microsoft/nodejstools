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
#if DEV12_OR_LATER
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.NodejsTools.Classifier;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Editor.BraceCompletion {
    [Export(typeof(IBraceCompletionContextProvider))]
    [BracePair(BraceKind.CurlyBrackets.Open, BraceKind.CurlyBrackets.Close)]
    [BracePair(BraceKind.SquareBrackets.Open, BraceKind.SquareBrackets.Close)]
    [BracePair(BraceKind.Parentheses.Open, BraceKind.Parentheses.Close)]
    [BracePair(BraceKind.SingleQuotes.Open, BraceKind.SingleQuotes.Close)]
    [BracePair(BraceKind.DoubleQuotes.Open, BraceKind.DoubleQuotes.Close)]
    [ContentType(NodejsConstants.Nodejs)]
    internal sealed class BraceCompletionContextProvider : IBraceCompletionContextProvider {
        public bool TryCreateContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionContext context) {
            // if we are in a comment or string literal we cannot begin a completion session.
            if (IsValidBraceCompletionContext(openingPoint)) {
                context = new BraceCompletionContext();
                return true;
            } else {
                context = null;
                return false;
            }
        }

        private bool IsValidBraceCompletionContext(SnapshotPoint openingPoint) {
            Debug.Assert(openingPoint.Position >= 0, "SnapshotPoint.Position should always be zero or positive.");

            if (openingPoint.Position > 0) {
                var classifier = openingPoint.Snapshot.TextBuffer.GetNodejsClassifier();
                var classificationSpans = classifier.GetClassificationSpans(new SnapshotSpan(openingPoint - 1, 1));

                foreach (ClassificationSpan span in classificationSpans) {
                    if (span.ClassificationType.IsOfType("comment")) {
                        return false;
                    } else if (span.ClassificationType.IsOfType("literal")) {
                        return false;
                    }
                }
            }
            
            // If we haven't stopped this, go ahead and start the completion session.
            // Either we were at position 0 (a safe place to be placing a brace completion)
            // or we were in a classification that is safe for brace completion.
            return true;
        }
    }
}
#endif