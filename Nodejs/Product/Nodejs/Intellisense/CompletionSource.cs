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
using System.Collections.Generic;
using System.Linq;
using Microsoft.NodejsTools.Classifier;
using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.NodejsTools.Intellisense {
    sealed partial class CompletionSource : ICompletionSource {
        public const string NodejsRequireCompletionSetMoniker = "Node.js require";

        private readonly ITextBuffer _textBuffer;
        private readonly NodejsClassifier _classifier;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGlyphService _glyphService;

        private static string[] _allowRequireTokens = new[] { "!", "!=", "!==", "%", "%=", "&", "&&", "&=", "(", ")", 
            "*", "*=", "+", "++", "+=", ",", "-", "--", "-=",  "..", "...", "/", "/=", ":", ";", "<", "<<", "<<=", 
            "<=", "=", "==", "===", ">", ">=", ">>", ">>=", ">>>", ">>>=", "?", "[", "^", "^=", "{", "|", "|=", "||", 
            "}", "~", "in", "case", "new", "return", "throw", "typeof"
        };

        private static string[] _keywords = new[] {
            "break", "case", "catch", "class", "const", "continue", "default", "delete", "do", "else", "eval", "extends", 
            "false", "field", "final", "finally", "for", "function", "if", "import", "in", "instanceof", "new", "null", 
            "package", "private", "protected", "public", "return", "super", "switch", "this", "throw", "true", "try", 
            "typeof", "var", "while", "with",
            "abstract", "debugger", "enum", "export", "goto", "implements", "native", "static", "synchronized", "throws",
            "transient", "volatile"
        };

        public CompletionSource(ITextBuffer textBuffer, NodejsClassifier classifier, IServiceProvider serviceProvider, IGlyphService glyphService) {
            _textBuffer = textBuffer;
            _classifier = classifier;
            _serviceProvider = serviceProvider;
            _glyphService = glyphService;
        }

        #region ICompletionSource Members

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets) {
            var buffer = _textBuffer;
            var snapshot = buffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(buffer).GetPoint(snapshot);

            // Disable completions if user is editing a special command (e.g. ".cls") in the REPL.
            if (snapshot.TextBuffer.Properties.ContainsProperty(typeof(IReplEvaluator)) && snapshot.Length != 0 && snapshot[0] == '.') {
                return;
            }

            if (ShouldTriggerRequireIntellisense(triggerPoint, _classifier, true, true)) {
                AugmentCompletionSessionForRequire(triggerPoint, session, completionSets);
                return;
            }

            var textBuffer = _textBuffer;
            var span = GetApplicableSpan(session, textBuffer);
            var provider = VsProjectAnalyzer.GetCompletions(
                _textBuffer.CurrentSnapshot,
                span,
                session.GetTriggerPoint(buffer)
            );

            var completions = provider.GetCompletions(_glyphService);
            if (completions != null && completions.Completions.Count > 0) {
                completionSets.Add(completions);
            }
        }
        private void AugmentCompletionSessionForRequire(SnapshotPoint triggerPoint, ICompletionSession session, IList<CompletionSet> completionSets) {
            var classifications = EnumerateClassificationsInReverse(_classifier, triggerPoint);
            bool quote = false;
            int length = 0;

            // check which one of these we're doing:
            // require(         inserting 'module' at trigger point
            // require('        inserting module' at trigger point
            // requre('ht')     ctrl space at ht, inserting http' at trigger point - 2
            // requre('addo')   ctrl space at add, inserting addons' at trigger point - 3

            // Therefore we have no quotes or quotes.  In no quotes we insert both
            // leading and trailing quotes.  In quotes we leave the leading quote in
            // place and replace any other quotes value that was already there.

            if (classifications.MoveNext()) {
                var curText = classifications.Current.Span.GetText();
                if (curText.StartsWith("'") || curText.StartsWith("\"")) {
                    // we're in the quotes case, figure out the existing string,
                    // and use that at the applicable span.
                    var fullSpan = _classifier.GetClassificationSpans(
                        new SnapshotSpan(
                            classifications.Current.Span.Start,
                            classifications.Current.Span.End.GetContainingLine().End
                        )
                    ).First();

                    quote = true;
                    triggerPoint -= (curText.Length - 1);
                    length = fullSpan.Span.Length - 1;
                }
                // else it's require(
            }

            var buffer = _textBuffer;
            var textBuffer = _textBuffer;
            var span = GetApplicableSpan(session, textBuffer);
            var provider = VsProjectAnalyzer.GetRequireCompletions(
                _textBuffer.CurrentSnapshot,
                span,
                session.GetTriggerPoint(buffer),
                quote
            );

            var completions = provider.GetCompletions(_glyphService);
            if (completions != null && completions.Completions.Count > 0) {
                completionSets.Add(completions);
            }
        }

        /// <summary>
        /// Returns the span to use for the provided intellisense session.
        /// </summary>
        /// <returns>A tracking span. The span may be of length zero if there
        /// is no suitable token at the trigger point.</returns>
        internal static ITrackingSpan GetApplicableSpan(IIntellisenseSession session, ITextBuffer buffer) {
            var snapshot = buffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(buffer);

            var span = GetApplicableSpan(snapshot, triggerPoint);
            if (span != null) {
                return span;
            }
            return snapshot.CreateTrackingSpan(triggerPoint.GetPosition(snapshot), 0, SpanTrackingMode.EdgeInclusive);
        }

        /// <summary>
        /// Returns the applicable span at the provided position.
        /// </summary>
        /// <returns>A tracking span, or null if there is no token at the
        /// provided position.</returns>
        internal static ITrackingSpan GetApplicableSpan(ITextSnapshot snapshot, ITrackingPoint point) {
            return GetApplicableSpan(snapshot, point.GetPosition(snapshot));
        }

        /// <summary>
        /// Returns the applicable span at the provided position.
        /// </summary>
        /// <returns>A tracking span, or null if there is no token at the
        /// provided position.</returns>
        internal static ITrackingSpan GetApplicableSpan(ITextSnapshot snapshot, int position) {
            var classifier = snapshot.TextBuffer.GetNodejsClassifier();
            var line = snapshot.GetLineFromPosition(position);
            if (classifier == null || line == null) {
                return null;
            }

            var spanLength = position - line.Start.Position;
            // Increase position by one to include 'fob' in: "abc.|fob"
            if (spanLength < line.Length) {
                spanLength += 1;
            }

            var classifications = classifier.GetClassificationSpans(new SnapshotSpan(line.Start, spanLength));
            // Handle "|"
            if (classifications == null || classifications.Count == 0) {
                return null;
            }

            var lastToken = classifications[classifications.Count - 1];
            // Handle "fob |"
            if (lastToken == null || position > lastToken.Span.End) {
                return null;
            }

            if (position > lastToken.Span.Start) {
                if (lastToken.CanComplete()) {
                    // Handle "fo|o"
                    return snapshot.CreateTrackingSpan(lastToken.Span, SpanTrackingMode.EdgeInclusive);
                } else {
                    // Handle "<|="
                    return null;
                }
            }

            var secondLastToken = classifications.Count >= 2 ? classifications[classifications.Count - 2] : null;
            if (lastToken.Span.Start == position && lastToken.CanComplete() &&
                (secondLastToken == null ||             // Handle "|fob"
                 position > secondLastToken.Span.End || // Handle "if |fob"
                 !secondLastToken.CanComplete())) {     // Handle "abc.|fob"
                return snapshot.CreateTrackingSpan(lastToken.Span, SpanTrackingMode.EdgeInclusive);
            }

            // Handle "abc|."
            // ("ab|c." would have been treated as "ab|c")
            if (secondLastToken != null && secondLastToken.Span.End == position && secondLastToken.CanComplete()) {
                return snapshot.CreateTrackingSpan(secondLastToken.Span, SpanTrackingMode.EdgeInclusive);
            }

            return null;
        }

        /// <summary>
        /// Checks if we are at a require statement where we can offer completions.
        /// 
        /// The bool flags are used to control when we are checking if we should provide
        /// the completions before updating the buffer the characters the user just typed.
        /// </summary>
        /// <param name="triggerPoint">The point where the completion session is being triggered</param>
        /// <param name="classifier">A classifier for getting the tokens</param>
        /// <param name="eatOpenParen">True if the open paren has been inserted and we should expect it</param>
        /// <param name="allowQuote">True if we will parse the require(' or require(" forms.</param>
        /// <returns></returns>
        internal static bool ShouldTriggerRequireIntellisense(SnapshotPoint triggerPoint, IClassifier classifier, bool eatOpenParen, bool allowQuote = false) {
            var classifications = EnumerateClassificationsInReverse(classifier, triggerPoint);
            bool atRequire = false;

            if (allowQuote && classifications.MoveNext()) {
                var curText = classifications.Current.Span.GetText();
                if (!curText.StartsWith("'") && !curText.StartsWith("\"")) {
                    // no leading quote, reset back to original classifications.
                    classifications = EnumerateClassificationsInReverse(classifier, triggerPoint);
                }
            }

            if ((!eatOpenParen || EatToken(classifications, "(")) && EatToken(classifications, "require")) {
                // start of a file or previous token to require is followed by an expression
                if (!classifications.MoveNext()) {
                    // require at beginning of the file
                    atRequire = true;
                } else {
                    var tokenText = classifications.Current.Span.GetText();

                    atRequire =
                        classifications.Current.Span.Start.GetContainingLine().LineNumber != triggerPoint.GetContainingLine().LineNumber ||
                        tokenText.EndsWith(";") || // f(x); has ); displayed as a single token
                        _allowRequireTokens.Contains(tokenText) || // require after a token which starts an expression
                        (tokenText.All(IsIdentifierChar) && !_keywords.Contains(tokenText));    // require after anything else that isn't a statement like keyword 
                    //      (including identifiers which are on the previous line)
                }
            }

            return atRequire;
        }

        internal static bool IsIdentifierChar(char ch) {
            return ch == '_' || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || ch == '$';
        }

        private static bool EatToken(IEnumerator<ClassificationSpan> classifications, string tokenText) {
            return classifications.MoveNext() && classifications.Current.Span.GetText() == tokenText;
        }

        /// <summary>
        /// Enumerates all of the classifications in reverse starting at start to the beginning of the file.
        /// </summary>
        private static IEnumerator<ClassificationSpan> EnumerateClassificationsInReverse(IClassifier classifier, SnapshotPoint start) {
            var curLine = start.GetContainingLine();
            var spanEnd = start;

            for (; ; ) {
                var classifications = classifier.GetClassificationSpans(new SnapshotSpan(curLine.Start, spanEnd));
                for (int i = classifications.Count - 1; i >= 0; i--) {
                    yield return classifications[i];
                }

                if (curLine.LineNumber == 0) {
                    break;
                }

                curLine = start.Snapshot.GetLineFromLineNumber(curLine.LineNumber - 1);
                spanEnd = curLine.End;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
        }

        #endregion
    }
}
