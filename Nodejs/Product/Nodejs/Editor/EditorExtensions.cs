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
using System.Diagnostics;
using System.Text;
using Microsoft.NodejsTools.Classifier;
using Microsoft.NodejsTools.Jade;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace Microsoft.NodejsTools.Editor.Core {
    internal static class EditorExtensions {
        /// <summary>
        /// Find if the current insertion point is a comment and, if it is, insert a * on the following 
        /// line at the correct indentation.
        /// </summary>
        public static void FormatMultilineComment(this ITextView textView, SnapshotPoint commentStartingPoint, SnapshotPoint insertionPoint) {
            // Since this was marked as a comment span (before calling this method), this method does not 
            // handle being called from insertion points not in/following a multiline comment.
            bool commentStartingPointPrechecks =
                commentStartingPoint.Snapshot != null &&
                commentStartingPoint.Position + 2 <= commentStartingPoint.Snapshot.Length &&
                commentStartingPoint.Snapshot.GetText(commentStartingPoint.Position, 2) == "/*";

            bool insertionPointPrechecks =
                insertionPoint.Snapshot != null &&
                insertionPoint.Position > commentStartingPoint.Position;

            var snapshot = commentStartingPoint.Snapshot;
            Debug.Assert(commentStartingPointPrechecks,
                "Comment Starting Point should be set to the beginning of a multiline comment.");

            Debug.Assert(insertionPointPrechecks,
                "Insertion Point must be set to a position after the Comment Starting Point");

            // If for any reason any of the prechecks fail, just don't format the comment.  A no-op seems better than
            // an odd formatting result...
            if (!commentStartingPointPrechecks || !insertionPointPrechecks) {
                return;
            }

            // Figure out the amount of whitespace preceeding the * on the line.  Take spacing style with this to match.
            int startOfFirstCommentLine = snapshot.GetLineFromPosition(commentStartingPoint).Start;
            string beforeAsterisk = snapshot.GetText(startOfFirstCommentLine, commentStartingPoint - startOfFirstCommentLine);

            // If ConvertTabsToSpaces is enabled, do that and make a string of all spaces of the same length.
            // Otherwise, walk the string and replace all non-whitespace characters with a space and use that string.
            if (textView.Options.IsConvertTabsToSpacesEnabled()) {
                var tabSize = textView.Options.GetTabSize();
                beforeAsterisk = String.Format("{0} *", TextHelper.ConvertTabsToSpaces(beforeAsterisk, tabSize, true));
            } else {
                // Create a string builder and make it 2 spaces bigger for the asterisk at the end.
                var sb = new StringBuilder(beforeAsterisk.Length + 2);

                // If the user wants to keep tabs, then we need to keep them in here.  Convert non-whitespace characters.
                for (int i = 0; i < beforeAsterisk.Length; i++) {
                    if (!char.IsWhiteSpace(beforeAsterisk[i])) {
                        sb.Append(" ");
                    } else {
                        sb.Append(beforeAsterisk[i]);
                    }
                }

                sb.Append(" *");
                beforeAsterisk = sb.ToString();
            }

            // Calculate the amount of space following the *.  1 if there is only whitespace, indent otherwise.
            string afterAsterisk = " "; // by default we want a single space
            if (insertionPoint.Position >= 1) // only do if we aren't the first line.
            {
                string previousLineText = snapshot.GetLineFromPosition(insertionPoint.Position - 1).GetText();

                // Replace tabs in previous line string if we have that option set so later calculations are correct.
                if (textView.Options.IsConvertTabsToSpacesEnabled()) {
                    var tabSize = textView.Options.GetTabSize();
                    previousLineText = TextHelper.ConvertTabsToSpaces(previousLineText, tabSize);
                }

                string trimmedPreviousLine = previousLineText.Trim();
                if (trimmedPreviousLine.StartsWith("*") && trimmedPreviousLine.Length != 1) {
                    // if it started with the *, take the whitespace between that and the next non-whitespace character.
                    // trim the whitespace off the front so we know how much of the string was whitespace after the asterisk
                    var whitespaceStringLength = trimmedPreviousLine.Length - trimmedPreviousLine.Substring(1).TrimStart().Length - 1;
                    afterAsterisk = trimmedPreviousLine.Substring(1, whitespaceStringLength);
                }
            }

            // Find the position to insert the new line.
            int commentInsertionPoint = insertionPoint;

            // Insert the whitespace * string and set the caret to the correct position
            using (var edit = snapshot.TextBuffer.CreateEdit()) {
                edit.Insert(commentInsertionPoint, beforeAsterisk + afterAsterisk);
                edit.Apply();
            }
            var pt = textView.BufferGraph.MapUpToBuffer(
                new SnapshotPoint(snapshot.TextBuffer.CurrentSnapshot, commentInsertionPoint + beforeAsterisk.Length + afterAsterisk.Length),
                PointTrackingMode.Positive,
                PositionAffinity.Successor,
                textView.TextSnapshot.TextBuffer
            );
            if (pt != null) {
                // Set the cursor position immediatelly following our edit to guarantee placement.
                textView.Caret.MoveTo(pt.Value);
            }
        }

        internal static bool IsMultilineComment(this SnapshotPoint insertionPoint, out SnapshotSpan commentSpan) {
            var buffer = insertionPoint.Snapshot.TextBuffer;
            var classifier = buffer.GetNodejsClassifier();

            // Find if this is a comment and then find the starting position of the comment if so.
            var classificationSpans = classifier.GetClassificationSpans(
                new SnapshotSpan(insertionPoint.Snapshot, new Span(insertionPoint.Position - 1, 1)));

            foreach (var span in classificationSpans) {
                // If the span is classified as a comment, doesn't start with a single line comment delimiter, 
                // And our insertion point is within the span this is a multiline comment.  We have to check that we are 
                // within the span since a terminated comment with the cursor after the closure will show as a comment
                // even though we are outside.
                if (span.ClassificationType.IsOfType("comment") &&
                    !span.Span.GetText().StartsWith("//") &&
                    span.Span.End.Position >= insertionPoint.Position) {
                    commentSpan = span.Span;
                    return true;
                }
            }

            commentSpan = new SnapshotSpan();
            return false;
        }

        internal static bool IsNodeJsContent(this ITextSnapshot buffer) {
            return buffer.ContentType.IsOfType(NodejsConstants.Nodejs);
        }

        internal static bool IsNodeJsContent(this ITextBuffer buffer) {
            return buffer.ContentType.IsOfType(NodejsConstants.Nodejs);
        }
    }
}
