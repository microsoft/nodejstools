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
using System.Linq;
using Microsoft.NodejsTools.Classifier;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.NodejsTools.Editor.ShowBraces {
    /// <summary>
    /// Provides highlighting of matching braces in a text view.
    /// </summary>
    internal class BraceMatchingTagger : ITagger<TextMarkerTag> {
        private readonly ITextView _view;
        private readonly ITextBuffer _sourceBuffer;
        private SnapshotPoint? _currentChar;
        private static readonly TextMarkerTag _tag = new TextMarkerTag("Brace Matching (Rectangle)");

        /// <summary>
        ///  Brace pairs that are matched on.
        /// </summary>
        private static readonly Dictionary<char, char> _braceList = new Dictionary<char, char>(){
                {'{', '}'},
                {'[', ']'},
                {'(', ')'}
            };

        internal BraceMatchingTagger(ITextView view, ITextBuffer sourceBuffer) {
            this._view = view;
            this._sourceBuffer = sourceBuffer;
            this._currentChar = null;

            this._view.Caret.PositionChanged += CaretPositionChanged;
            this._view.LayoutChanged += ViewLayoutChanged;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        internal void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e) {
            if (e.NewSnapshot != e.OldSnapshot) {
                UpdateAtCaretPosition(_view.Caret.Position);
            }
        }

        internal void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
            UpdateAtCaretPosition(e.NewPosition);
        }

        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
            // Don't do anything if there is no content in the buffer or
            // the current SnapshotPoint is not initialized or at the end of the buffer .
            if (spans.Count == 0 || !_currentChar.HasValue || _currentChar.Value.Position > _currentChar.Value.Snapshot.Length) {
                yield break;
            }

            // If the requested snapshot isn't the same as the one the brace is on, translate our spans to the expected snapshot.
            SnapshotPoint currentChar = _currentChar.Value;
            if (spans[0].Snapshot != currentChar.Snapshot) {
                currentChar = currentChar.TranslateTo(spans[0].Snapshot, PointTrackingMode.Positive);
            }

            // Get the current char and the previous char.
            SnapshotPoint lastChar = currentChar == 0 ? currentChar : currentChar - 1;
            SnapshotSpan pairSpan = new SnapshotSpan();
            char currentText;
            char lastText;
            char closeChar;
            if (currentChar.Position < currentChar.Snapshot.Length - 1 &&
                _braceList.TryGetValue(currentText = currentChar.GetChar(), out closeChar)) {
                // The value is an opening brace, which is the *next/current* character, we will find the closing character.
                if (BraceMatchingTagger.FindMatchingCloseChar(currentChar, currentText, closeChar, _view.TextViewLines.Count, out pairSpan) == true) {
                    yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(currentChar, 1), _tag);
                    yield return new TagSpan<TextMarkerTag>(pairSpan, _tag);
                }
            } else if (currentChar.Snapshot.Length > 1 &&
                lastChar.Position < currentChar.Snapshot.Length &&
                _braceList.ContainsValue(lastText = lastChar.GetChar())) {
                // The value is a closing brace, which is the *previous* character, we will find the opening character.
                var open = from n in _braceList
                           where n.Value.Equals(lastText)
                           select n.Key;
                if (BraceMatchingTagger.FindMatchingOpenChar(lastChar, open.Single(), lastText, _view.TextViewLines.Count, out pairSpan) == true) {
                    yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(lastChar, 1), _tag);
                    yield return new TagSpan<TextMarkerTag>(pairSpan, _tag);
                }
            }
        }

        internal void UpdateAtCaretPosition(CaretPosition caretPosition) {
            _currentChar = caretPosition.Point.GetPoint(_sourceBuffer, caretPosition.Affinity);

            if (!_currentChar.HasValue) {
                return;
            }

            var tempEvent = TagsChanged;
            if (tempEvent != null) {
                tempEvent(this, new SnapshotSpanEventArgs(new SnapshotSpan(_sourceBuffer.CurrentSnapshot, 0,
                    _sourceBuffer.CurrentSnapshot.Length)));
            }
        }

        private static bool FindMatchingCloseChar(SnapshotPoint startPoint, char open, char close, int maxLines, out SnapshotSpan pairSpan) {
            int closuresEncountered = 0;
            int openingsEncountered = 1;

            for (int i = startPoint.Position + 1; i < startPoint.Snapshot.Length; i++) {
                GetOpeningAndClosureCount(new SnapshotPoint(startPoint.Snapshot, i), open, close, ref closuresEncountered, ref openingsEncountered);

                if (openingsEncountered == closuresEncountered) {
                    pairSpan = new SnapshotSpan(startPoint.Snapshot, i, 1);
                    return true;
                }
            }

            pairSpan = new SnapshotSpan();
            return false;
        }

        private static bool FindMatchingOpenChar(SnapshotPoint startPoint, char open, char close, int maxLines, out SnapshotSpan pairSpan) {
            int closuresEncountered = 1;
            int openingsEncountered = 0;

            for (int i = startPoint.Position - 1; i >= 0; i--) {
                GetOpeningAndClosureCount(new SnapshotPoint(startPoint.Snapshot, i), open, close, ref closuresEncountered, ref openingsEncountered);

                if (openingsEncountered == closuresEncountered) {
                    pairSpan = new SnapshotSpan(startPoint.Snapshot, i, 1);
                    return true;
                }
            }

            pairSpan = new SnapshotSpan();
            return false;
        }

        private static void GetOpeningAndClosureCount(SnapshotPoint snapshotPoint, char openBrace, char closeBrace, ref int closeBracesEncountered, ref int openBracesEncountered) {
            var classifier = snapshotPoint.Snapshot.TextBuffer.GetNodejsClassifier();

            // If the classifier is null, rather than throw, we should just return without doing anything.
            if (classifier != null) {
                var classifications = classifier.GetClassificationSpans(new SnapshotSpan(snapshotPoint, 1));
                foreach (var classification in classifications) {
                    if (classification.ClassificationType.IsOfType(NodejsPredefinedClassificationTypeNames.Grouping) && classification.Span.Length == 1) {
                        if (classification.Span.GetText() == openBrace.ToString()) {
                            openBracesEncountered++;
                        } else if (classification.Span.GetText() == closeBrace.ToString()) {
                            closeBracesEncountered++;
                        }
                    }
                }
            }
        }
    }
}