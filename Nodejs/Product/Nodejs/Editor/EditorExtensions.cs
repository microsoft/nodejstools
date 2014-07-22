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
using System.Text;
using Microsoft.NodejsTools.Classifier;
using Microsoft.NodejsTools.Jade;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace Microsoft.NodejsTools.Editor.Core {
	internal static class EditorExtensions {
		public static bool CommentOrUncommentBlock(this ITextView view, bool comment) {
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
				if (view.TextBuffer.IsNodeJsContent()) {
					UpdateSelection(view, start, end);
				}
				return true;
			}

			return false;
		}

		/// <summary>
		/// Find if the current insertion point is a comment and, if it is, insert a * on the following 
		/// line at the correct indentation.
		/// </summary>
		public static void FormatMultilineComment(this ITextView textView, SnapshotPoint commentStartingPoint, SnapshotPoint insertionPoint) {
			// Since this was marked as a comment span (before calling this method), this method does not 
			// handle being called from insertion points not in/following a multiline comment.
			bool commentStartingPointPrechecks = 
				commentStartingPoint.Snapshot != null &&
				commentStartingPoint.Position + 2 <= commentStartingPoint.Snapshot.Length  &&
				commentStartingPoint.Snapshot.GetText(commentStartingPoint.Position, 2) == "/*";

			bool insertionPointPrechecks = 
				insertionPoint.Snapshot != null &&
				insertionPoint.Position > commentStartingPoint.Position;

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
			int startOfFirstCommentLine = textView.TextSnapshot.GetLineFromPosition(commentStartingPoint).Start;
			string beforeAsterisk = textView.TextSnapshot.GetText(startOfFirstCommentLine, commentStartingPoint - startOfFirstCommentLine);

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
				string previousLineText = textView.TextSnapshot.GetLineFromPosition(insertionPoint.Position - 1).GetText();

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
			using (var edit = insertionPoint.Snapshot.TextBuffer.CreateEdit()) {
				edit.Insert(commentInsertionPoint, beforeAsterisk + afterAsterisk);
				edit.Apply();
			}

			// Set the cursor position immediatelly following our edit to guarantee placement.
			textView.Caret.MoveTo(new SnapshotPoint(textView.TextSnapshot, commentInsertionPoint + beforeAsterisk.Length + afterAsterisk.Length));
		}

		internal static bool IsMultilineComment(this SnapshotPoint insertionPoint, out SnapshotSpan commentSpan) {
			var buffer = insertionPoint.Snapshot.TextBuffer;
			var classifier = buffer.GetNodejsClassifier();

			// Find if this is a comment and then find the starting position of the comment if so.
			var classificationSpans = buffer.GetNodejsClassifier().GetClassificationSpans(
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
