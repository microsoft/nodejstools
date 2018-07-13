// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Implements <see cref="IClassifier"/> and provides classification using generic tokens
    /// </summary>
    internal class TokenBasedClassifier<TTokenType, TTokenClass> : IClassifier where TTokenClass : IToken<TTokenType>
    {
#pragma warning disable 67
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
#pragma warning restore 67

        public ITokenizer<TTokenClass> Tokenizer { get; }
        public TextRangeCollection<TTokenClass> Tokens { get; protected set; }

        protected ITextBuffer TextBuffer { get; }
        protected bool LineBasedClassification { get; set; }

        private IClassificationContextNameProvider<TTokenClass> _classificationNameProvider;

        private int _lastValidPosition = 0;

        public TokenBasedClassifier(ITextBuffer textBuffer,
                                    ITokenizer<TTokenClass> tokenizer,
                                    IClassificationContextNameProvider<TTokenClass> classificationNameProvider)
        {
            this._classificationNameProvider = classificationNameProvider;

            this.Tokenizer = tokenizer;

            this.TextBuffer = textBuffer;
            this.TextBuffer.Changed += this.OnTextChanged;

            this.Tokens = new TextRangeCollection<TTokenClass>();
        }

        /// <summary>
        /// Override this in specific language to remove extra tokens that the new text may depend on
        /// </summary>
        /// <param name="position"></param>
        /// <param name="tokens"></param>
        protected virtual void RemoveSensitiveTokens(int position, TextRangeCollection<TTokenClass> tokens)
        {
        }

        protected virtual void OnTextChanged(object sender, TextContentChangedEventArgs e)
        {
            TextUtility.CombineChanges(e, out var start, out var oldLength, out var newLength);

            // check if change is still within current snapshot. the problem is that
            // change could have been calculated against projected buffer and then
            // host (HTML editor) could have dropped projections effectively
            // shortening buffer to nothing.

            var snapshot = this.TextBuffer.CurrentSnapshot;
            if (start > snapshot.Length || start + newLength > snapshot.Length)
            {
                start = 0;
                newLength = snapshot.Length;
            }

            OnTextChanged(start, oldLength, newLength);
        }

        protected virtual void OnTextChanged(int start, int oldLength, int newLength)
        {
            // Invalidate items starting from start of the change and onward

            // Expand range to take into accound token that might be just touching
            // changed area. For example, in PHP / is punctuation token and adding *
            // to it should remove / so tokenizer can recreate comment token.
            // However / is technically outside of the changed area and hence may end up
            // lingering on.

            var initialIndex = -1;
            var changeStart = start;

            var touchingTokens = this.Tokens.GetItemsContainingInclusiveEnd(start);

            if (touchingTokens != null && touchingTokens.Count > 0)
            {
                initialIndex = touchingTokens.Min();
                start = this.Tokens[initialIndex].Start;
            }

            // nothing is touching but we still might have tokens right after us
            if (initialIndex < 0)
            {
                initialIndex = this.Tokens.GetFirstItemAfterPosition(start);
            }

            if (initialIndex == 0)
            {
                start = this.Tokens[0].Start;
            }
            else
            {
                while (initialIndex > 0)
                {
                    if (this.Tokens[initialIndex - 1].End == start)
                    {
                        start = this.Tokens[initialIndex - 1].Start;
                        initialIndex--;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            this._lastValidPosition = Math.Min(this._lastValidPosition, start);
            if (this.Tokens.Count > 0)
            {
                this.Tokens.RemoveInRange(TextRange.FromBounds(this._lastValidPosition, this.Tokens[this.Tokens.Count - 1].End), true);
            }

            // In line-based tokenizers like SaSS or Jade we need to start at the beginning 
            // of the line i.e. at 'anchor' position that is canculated depending on particular
            // language syntax.

            this._lastValidPosition = GetAnchorPosition(this._lastValidPosition);

            RemoveSensitiveTokens(this._lastValidPosition, this.Tokens);
            VerifyTokensSorted();

            this._lastValidPosition = this.Tokens.Count > 0 ? Math.Min(this._lastValidPosition, this.Tokens[this.Tokens.Count - 1].End) : 0;

            if (ClassificationChanged != null)
            {
                var snapshot = this.TextBuffer.CurrentSnapshot;

                ClassificationChanged(this, new ClassificationChangedEventArgs(
                        new SnapshotSpan(snapshot,
                            Span.FromBounds(this._lastValidPosition, snapshot.Length)))
                            );
            }
        }

        public virtual IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var classifications = new List<ClassificationSpan>();
            var textSnapshot = this.TextBuffer.CurrentSnapshot;

            if (span.Length <= 2)
            {
                var ws = textSnapshot.GetText(span);
                if (string.IsNullOrWhiteSpace(ws))
                {
                    return classifications;
                }
            }

            // Token collection at this point contains valid tokens at least to a point
            // of the most recent change. We can reuse existing tokens but may also need
            // to tokenize to get tokens for the recently changed range.
            if (span.End > this._lastValidPosition)
            {
                // Span is beyond the last position we know about. We need to tokenize new area.
                // tokenize from end of the last good token. If last token intersected last change
                // it would have been removed from the collection by now.

                var tokenizeFrom = this.Tokens.Count > 0 ? this.Tokens[this.Tokens.Count - 1].End : new SnapshotPoint(textSnapshot, 0);
                var tokenizeAnchor = GetAnchorPosition(tokenizeFrom);

                if (tokenizeAnchor < tokenizeFrom)
                {
                    this.Tokens.RemoveInRange(TextRange.FromBounds(tokenizeAnchor, span.End));
                    RemoveSensitiveTokens(tokenizeAnchor, this.Tokens);

                    tokenizeFrom = tokenizeAnchor;
                    VerifyTokensSorted();
                }

                var newTokens = this.Tokenizer.Tokenize(new TextProvider(this.TextBuffer.CurrentSnapshot), tokenizeFrom, span.End - tokenizeFrom);
                if (newTokens.Count > 0)
                {
                    this.Tokens.Add(newTokens);
                    this._lastValidPosition = newTokens[newTokens.Count - 1].End;
                }
            }

            var tokensInSpan = this.Tokens.ItemsInRange(TextRange.FromBounds(span.Start, span.End));

            foreach (var token in tokensInSpan)
            {

                if (token is ICompositeToken<TTokenClass> compositeToken)
                {
                    foreach (var internalToken in compositeToken.TokenList)
                    {
                        AddClassificationFromToken(classifications, textSnapshot, internalToken);
                    }
                }
                else
                {
                    AddClassificationFromToken(classifications, textSnapshot, token);
                }
            }

            return classifications;
        }

        protected virtual int GetAnchorPosition(int position)
        {
            if (this.LineBasedClassification)
            {
                var line = this.TextBuffer.CurrentSnapshot.GetLineFromPosition(position);
                position = Math.Min(position, line.Start);
            }

            return position;
        }

        private void AddClassificationFromToken(List<ClassificationSpan> classifications, ITextSnapshot textSnapshot, TTokenClass token)
        {
            // We don't necessarily map each token to a classification
            var ct = this._classificationNameProvider.GetClassificationType(token);
            if (ct != null)
            {
                var tokenSpan = new Span(token.Start, token.Length);
                var cs = new ClassificationSpan(new SnapshotSpan(textSnapshot, tokenSpan), ct);
                classifications.Add(cs);
            }
        }

        private void VerifyTokensSorted()
        {
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
