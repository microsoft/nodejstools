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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// Implements <see cref="IClassifier"/> and provides classification using generic tokens
    /// </summary>
    class TokenBasedClassifier<TTokenType, TTokenClass> : IClassifier where TTokenClass : IToken<TTokenType> {
#pragma warning disable 67
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
#pragma warning restore 67

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tokenizer", Justification = "Standard name of code that produces tokens")]
        public ITokenizer<TTokenClass> Tokenizer { get; private set; }
        public TextRangeCollection<TTokenClass> Tokens { get; protected set; }

        protected ITextBuffer TextBuffer { get; set; }
        protected bool LineBasedClassification { get; set; }

        IClassificationContextNameProvider<TTokenClass> _classificationNameProvider;

        private int _lastValidPosition = 0;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "tokenizer", Justification = "Standard name of code that produces tokens")]
        public TokenBasedClassifier(ITextBuffer textBuffer,
                                    ITokenizer<TTokenClass> tokenizer,
                                    IClassificationContextNameProvider<TTokenClass> classificationNameProvider) {
            _classificationNameProvider = classificationNameProvider;

            Tokenizer = tokenizer;

            TextBuffer = textBuffer;
            TextBuffer.Changed += OnTextChanged;

            Tokens = new TextRangeCollection<TTokenClass>();
        }

        /// <summary>
        /// Override this in specific language to remove extra tokens that the new text may depend on
        /// </summary>
        /// <param name="position"></param>
        /// <param name="tokens"></param>
        protected virtual void RemoveSensitiveTokens(int position, TextRangeCollection<TTokenClass> tokens) {
        }

        protected virtual void OnTextChanged(object sender, TextContentChangedEventArgs e) {
            int start, oldLength, newLength;
            TextUtility.CombineChanges(e, out start, out oldLength, out newLength);

            // check if change is still within current snapshot. the problem is that
            // change could have been calculated against projected buffer and then
            // host (HTML editor) could have dropped projections effectively
            // shortening buffer to nothing.

            var snapshot = TextBuffer.CurrentSnapshot;
            if (start > snapshot.Length || start + newLength > snapshot.Length) {
                start = 0;
                newLength = snapshot.Length;
            }

            OnTextChanged(start, oldLength, newLength);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldLength", Justification = "It may be used in derived class and/or unit tests")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newLength", Justification = "It may be used in derived class and/or unit tests")]
        protected virtual void OnTextChanged(int start, int oldLength, int newLength) {
            // Invalidate items starting from start of the change and onward

            // Expand range to take into accound token that might be just touching
            // changed area. For example, in PHP / is punctuation token and adding *
            // to it should remove / so tokenizer can recreate comment token.
            // However / is technically outside of the changed area and hence may end up
            // lingering on.

            int initialIndex = -1;
            int changeStart = start;

            var touchingTokens = Tokens.GetItemsContainingInclusiveEnd(start);

            if (touchingTokens != null && touchingTokens.Count > 0) {
                initialIndex = touchingTokens.Min();
                start = Tokens[initialIndex].Start;
            }

            // nothing is touching but we still might have tokens right after us
            if (initialIndex < 0) {
                initialIndex = Tokens.GetFirstItemAfterPosition(start);
            }

            if (initialIndex == 0) {
                start = Tokens[0].Start;
            } else {
                while (initialIndex > 0) {
                    if (Tokens[initialIndex - 1].End == start) {
                        start = Tokens[initialIndex - 1].Start;
                        initialIndex--;
                    } else {
                        break;
                    }
                }
            }

            _lastValidPosition = Math.Min(_lastValidPosition, start);
            if (Tokens.Count > 0)
                Tokens.RemoveInRange(TextRange.FromBounds(_lastValidPosition, Tokens[Tokens.Count - 1].End), true);

            // In line-based tokenizers like SaSS or Jade we need to start at the beginning 
            // of the line i.e. at 'anchor' position that is canculated depending on particular
            // language syntax.

            _lastValidPosition = GetAnchorPosition(_lastValidPosition);

            RemoveSensitiveTokens(_lastValidPosition, Tokens);
            VerifyTokensSorted();

            _lastValidPosition = Tokens.Count > 0 ? Math.Min(_lastValidPosition, Tokens[Tokens.Count - 1].End) : 0;

            if (ClassificationChanged != null) {
                var snapshot = TextBuffer.CurrentSnapshot;

                ClassificationChanged(this, new ClassificationChangedEventArgs(
                        new SnapshotSpan(snapshot,
                            Span.FromBounds(_lastValidPosition, snapshot.Length)))
                            );
            }
        }

        public virtual IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span) {
            List<ClassificationSpan> classifications = new List<ClassificationSpan>();
            ITextSnapshot textSnapshot = TextBuffer.CurrentSnapshot;

            if (span.Length <= 2) {
                string ws = textSnapshot.GetText(span);
                if (String.IsNullOrWhiteSpace(ws))
                    return classifications;
            }

            // Token collection at this point contains valid tokens at least to a point
            // of the most recent change. We can reuse existing tokens but may also need
            // to tokenize to get tokens for the recently changed range.
            if (span.End > _lastValidPosition) {
                // Span is beyond the last position we know about. We need to tokenize new area.
                // tokenize from end of the last good token. If last token intersected last change
                // it would have been removed from the collection by now.

                int tokenizeFrom = Tokens.Count > 0 ? Tokens[Tokens.Count - 1].End : new SnapshotPoint(textSnapshot, 0);
                var tokenizeAnchor = GetAnchorPosition(tokenizeFrom);

                if (tokenizeAnchor < tokenizeFrom) {
                    Tokens.RemoveInRange(TextRange.FromBounds(tokenizeAnchor, span.End));
                    RemoveSensitiveTokens(tokenizeAnchor, Tokens);

                    tokenizeFrom = tokenizeAnchor;
                    VerifyTokensSorted();
                }

                var newTokens = Tokenizer.Tokenize(new TextProvider(TextBuffer.CurrentSnapshot), tokenizeFrom, span.End - tokenizeFrom);
                if (newTokens.Count > 0) {
                    Tokens.Add(newTokens);
                    _lastValidPosition = newTokens[newTokens.Count - 1].End;
                }
            }

            var tokensInSpan = Tokens.ItemsInRange(TextRange.FromBounds(span.Start, span.End));

            foreach (var token in tokensInSpan) {
                var compositeToken = token as ICompositeToken<TTokenClass>;

                if (compositeToken != null) {
                    foreach (var internalToken in compositeToken.TokenList) {
                        AddClassificationFromToken(classifications, textSnapshot, internalToken);
                    }
                } else {
                    AddClassificationFromToken(classifications, textSnapshot, token);
                }
            }

            return classifications;
        }

        protected virtual int GetAnchorPosition(int position) {
            if (LineBasedClassification) {
                var line = TextBuffer.CurrentSnapshot.GetLineFromPosition(position);
                position = Math.Min(position, line.Start);
            }

            return position;
        }

        private void AddClassificationFromToken(List<ClassificationSpan> classifications, ITextSnapshot textSnapshot, TTokenClass token) {
            // We don't necessarily map each token to a classification
            var ct = _classificationNameProvider.GetClassificationType(token);
            if (ct != null) {
                Span tokenSpan = new Span(token.Start, token.Length);
                ClassificationSpan cs = new ClassificationSpan(new SnapshotSpan(textSnapshot, tokenSpan), ct);
                classifications.Add(cs);
            }
        }

        private void VerifyTokensSorted() {
#if _DEBUG
            // Verify that tokens are sorted
            for (int i = 0; i < Tokens.Count - 1; i++)
            {
                if (Tokens[i].End > Tokens[i + 1].Start)
                {
                    Debug.Assert(false, "TokenBasedClassifier: tokens are not sorted!");
                    break;
                }
            }
#endif
        }
    }
}
