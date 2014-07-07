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
using System.Diagnostics;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.NodejsTools.Editor.Core {
    internal static class EditorExtensions {
        public static bool CommentOrUncommentBlock(ITextView view, bool comment) {
            SnapshotPoint start, end;
            SnapshotPoint? mappedStart, mappedEnd;

            // select the region to comment.
            // if the selected area is non-empty, let's comment the group of lines, 
            // otherwise just comment the current line
            if (view.Selection.IsActive && !view.Selection.IsEmpty) {
                // comment every line in the selection
                start = view.Selection.Start.Position;
                end = view.Selection.End.Position;
                mappedStart = MapPoint(view, start);

                var endLine = end.GetContainingLine();

                // If we grabbed the last line by just the start, don't comment it as it isn't actually selected.
                if (endLine.Start == end) {
                    end = end.Snapshot.GetLineFromLineNumber(endLine.LineNumber - 1).End;
                }

                mappedEnd = MapPoint(view, end);
            } else {
                // comment the current line
                start = end = view.Caret.Position.BufferPosition;
                mappedStart = mappedEnd = MapPoint(view, start);
            }

            // Now that we have selected the region to comment, let's do the work.
            if (mappedStart != null && mappedEnd != null &&
                mappedStart.Value <= mappedEnd.Value) {
                if (comment) {
                    CommentRegion(view, mappedStart.Value, mappedEnd.Value);
                } else {
                    UncommentRegion(view, mappedStart.Value, mappedEnd.Value);
                }

                // After commenting, update the selection to the complete commented area.
                if (IsNodeJsContent(view.TextBuffer)) {
                    UpdateSelection(view, start, end);
                }
                return true;
            }

            return false;
        }

        internal static bool IsNodeJsContent(ITextSnapshot buffer) {
            return buffer.ContentType.IsOfType(NodejsConstants.Nodejs);
        }

        internal static bool IsNodeJsContent(ITextBuffer buffer) {
            return buffer.ContentType.IsOfType(NodejsConstants.Nodejs);
        }

        /// <summary>
        /// Adds comment characters (//) to the start of each line.  If there is a selection the comment is applied
        /// to each selected line.  Otherwise the comment is applied to the current line.
        /// </summary>
        private static void CommentRegion(ITextView view, SnapshotPoint start, SnapshotPoint end) {
            Debug.Assert(start.Snapshot == end.Snapshot);
            var snapshot = start.Snapshot;

            using (var edit = snapshot.TextBuffer.CreateEdit()) {
                int minColumn = Int32.MaxValue;

                // First pass, determine the position to place the comment.
                // This is done as we want all of the comment lines to line up at the end.
                // We also should ignore whitelines in this calculation.
                for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++) {
                    var curLine = snapshot.GetLineFromLineNumber(i);
                    var text = curLine.GetText();

                    int firstNonWhitespace = IndexOfNonWhitespaceCharacter(text);
                    if (firstNonWhitespace >= 0 && firstNonWhitespace < minColumn) {
                        minColumn = firstNonWhitespace;
                    }
                }

                // Second pass, place the comment.
                for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++) {
                    var curLine = snapshot.GetLineFromLineNumber(i);
                    if (String.IsNullOrWhiteSpace(curLine.GetText())) {
                        continue;
                    }

                    Debug.Assert(curLine.Length >= minColumn);

                    edit.Insert(curLine.Start.Position + minColumn, "//");
                }

                edit.Apply();
            }
        }

        /// <summary>
        /// Removes a comment character (//) from the start of each line.  If there is a selection the character is
        /// removed from each selected line.  Otherwise the character is removed from the current line.  Uncommented
        /// lines are ignored.
        /// </summary>
        private static void UncommentRegion(ITextView view, SnapshotPoint start, SnapshotPoint end) {
            Debug.Assert(start.Snapshot == end.Snapshot);
            var snapshot = start.Snapshot;

            using (var edit = snapshot.TextBuffer.CreateEdit()) {
                for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++) {
                    var curLine = snapshot.GetLineFromLineNumber(i);

                    DeleteCommentChars(edit, curLine);
                }

                edit.Apply();
            }
        }

        private static SnapshotPoint? MapPoint(ITextView view, SnapshotPoint point) {
            return view.BufferGraph.MapDownToFirstMatch(
               point,
               PointTrackingMode.Positive,
               IsNodeJsContent,
               PositionAffinity.Successor
            );
        }

        private static void UpdateSelection(ITextView view, SnapshotPoint start, SnapshotPoint end) {
            // Select the full region that is commented, do not select if in projection buffer 
            // (the selection might span non-language buffer regions)
            view.Selection.Select(
                new SnapshotSpan(
                    start.GetContainingLine().Start.TranslateTo(view.TextBuffer.CurrentSnapshot, PointTrackingMode.Negative),
                    end.GetContainingLine().End.TranslateTo(view.TextBuffer.CurrentSnapshot, PointTrackingMode.Positive)
                ),
                false
            );
        }

        private static void DeleteCommentChars(ITextEdit edit, ITextSnapshotLine curLine) {
            var text = curLine.GetText();
            for (int j = 0; j < text.Length; j++) {
                if (!Char.IsWhiteSpace(text[j])) {
                    if (text.Substring(j, 2) == "//") {
                        edit.Delete(curLine.Start.Position + j, 2);
                    }
                    break;
                }
            }
        }

        private static int IndexOfNonWhitespaceCharacter(string text) {
            for (int j = 0; j < text.Length; j++) {
                if (!Char.IsWhiteSpace(text[j])) {
                    return j;
                }
            }
            return -1;
        }
    }
}
