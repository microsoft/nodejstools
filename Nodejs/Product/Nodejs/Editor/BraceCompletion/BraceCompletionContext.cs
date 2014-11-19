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
#if DEV12_OR_LATER
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.NodejsTools.Formatting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace Microsoft.NodejsTools.Editor.BraceCompletion {
    [Export(typeof(IBraceCompletionContext))]
    internal class BraceCompletionContext : IBraceCompletionContext {
        public bool AllowOverType(IBraceCompletionSession session) {
            return true;
        }

        public void Finish(IBraceCompletionSession session) { }

        public void Start(IBraceCompletionSession session) { }

        public void OnReturn(IBraceCompletionSession session) {
            if (NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnEnter) {
                // reshape code from
                // {
                // |}
                // 
                // to
                // {
                //     |
                // }
                // where | indicates caret position.

                var closingPointPosition = session.ClosingPoint.GetPosition(session.SubjectBuffer.CurrentSnapshot);

                Debug.Assert(
                    condition: closingPointPosition > 0,
                    message: "The closing point position should always be greater than zero",
                    detailMessage: "The closing point position should always be greater than zero, " +
                                   "since there is also an opening point for this brace completion session");

                // Insert an extra newline and indent the closing brace manually.           
                session.SubjectBuffer.Insert(
                    closingPointPosition - 1,
                    session.TextView.Options.GetNewLineCharacter());

                // Format before setting the caret.
                Format(session);

                // After editing, set caret to the correct position.
                SetCaretPosition(session);
            }
        }

        private void Format(IBraceCompletionSession session) {
            var buffer = session.SubjectBuffer;
            EditFilter.ApplyEdits(
                buffer,
                Formatter.GetEditsAfterEnter(
                    buffer.CurrentSnapshot.GetText(),
                    session.OpeningPoint.GetPosition(buffer.CurrentSnapshot),
                    session.ClosingPoint.GetPosition(buffer.CurrentSnapshot),
                    EditFilter.CreateFormattingOptions(session.TextView.Options)
                )
            );
        }

        private static void SetCaretPosition(IBraceCompletionSession session) {
            // Find next line from brace.
            var snapshot = session.SubjectBuffer.CurrentSnapshot;
            var openCurlyLine = session.OpeningPoint.GetPoint(snapshot).GetContainingLine();
            var nextLineNumber = openCurlyLine.LineNumber + 1;

            bool nextLineExists = nextLineNumber < snapshot.LineCount;
            Debug.Assert(nextLineExists, "There are no lines after this brace completion's opening brace, no place to seek caret to.");
            if (!nextLineExists) {
                // Don't move the caret as we have somehow ended up without a line following our opening brace.
                return;
            }

            // Get indent for this line.
            ITextSnapshotLine nextLine = snapshot.GetLineFromLineNumber(nextLineNumber);
            var indentation = GetIndentationLevelForLine(session, nextLine);
            if (indentation > 0) {
                // before deleting, make sure this line is only whitespace.
                bool lineIsWhitepace = string.IsNullOrWhiteSpace(nextLine.GetText());
                Debug.Assert(lineIsWhitepace, "The line after the brace should be empty.");
                if (lineIsWhitepace) {
                    session.SubjectBuffer.Delete(nextLine.Extent);
                    MoveCaretTo(session.TextView, nextLine.End, indentation);
                }
            } else {
                MoveCaretTo(session.TextView, nextLine.End);
            }
        }

        private static int GetIndentationLevelForLine(IBraceCompletionSession session, ITextSnapshotLine line) {
            ISmartIndent smartIndenter;
            if (session.TextView.Properties.TryGetProperty<ISmartIndent>(typeof(SmartIndent), out smartIndenter)) {
                int? preferableIndentation = smartIndenter.GetDesiredIndentation(line);
                if (preferableIndentation.HasValue) {
                    return preferableIndentation.Value;
                }
            }

            return 0;
        }

        private static void MoveCaretTo(ITextView textView, SnapshotPoint point, int virtualSpaces = 0) {
            var pointOnViewBuffer =
                textView.BufferGraph.MapUpToBuffer(
                    point,
                    trackingMode: PointTrackingMode.Negative,
                    affinity: PositionAffinity.Successor,
                    targetBuffer: textView.TextBuffer);

            if (!pointOnViewBuffer.HasValue) {
                Debug.Fail(@"Point in view buffer should always be mappable from brace completion buffer?");
                return;
            }

            if (virtualSpaces <= 0) {
                textView.Caret.MoveTo(pointOnViewBuffer.Value);
            } else {
                var virtualPointOnView = new VirtualSnapshotPoint(pointOnViewBuffer.Value, virtualSpaces);
                textView.Caret.MoveTo(virtualPointOnView);
            }

            textView.Caret.EnsureVisible();
        }
    }
}
#endif