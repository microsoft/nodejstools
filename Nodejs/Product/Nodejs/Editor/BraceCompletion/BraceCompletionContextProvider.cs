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
using System.ComponentModel.Composition;
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
            if (openingPoint.Position <= 0) {
                // If we are at the start of the buffer, there is no reason to do a completion.
                return false;
            }

            var classifier = openingPoint.Snapshot.TextBuffer.GetNodejsClassifier();
            var classificationSpans = classifier.GetClassificationSpans(new SnapshotSpan(openingPoint - 1, 1));

            foreach (ClassificationSpan span in classificationSpans) {
                if (span.ClassificationType.IsOfType("comment")) {
                    return false;
                } else if (span.ClassificationType.IsOfType("literal")) {
                    return false;
                }
            }

            // If we haven't stopped this, go ahead and start the completion session.
            return true;
        }
    }
}
#endif