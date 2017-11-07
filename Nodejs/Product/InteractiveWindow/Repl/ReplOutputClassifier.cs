// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.NodejsTools.Repl
{
    /// <summary>
    /// Classifies regions for REPL error output spans.  These are always classified as errors.
    /// </summary>
    internal class ReplOutputClassifier : IClassifier
    {
        private readonly ReplOutputClassifierProvider _provider;
        internal static object ColorKey = new object();
        private readonly ITextBuffer _buffer;

        public ReplOutputClassifier(ReplOutputClassifierProvider provider, ITextBuffer buffer)
        {
            _provider = provider;
            _buffer = buffer;
        }

        #region IClassifier Members

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
        {
            add { }
            remove { }
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            if (!_buffer.Properties.TryGetProperty(ColorKey, out List<ColoredSpan> coloredSpans))
            {
                return new ClassificationSpan[0];
            }

            List<ClassificationSpan> classifications = new List<ClassificationSpan>();

            int startIndex = coloredSpans.BinarySearch(new ColoredSpan(span, InteractiveWindowColor.White), SpanStartComparer.Instance);
            if (startIndex < 0)
            {
                startIndex = ~startIndex - 1;
            }

            int spanEnd = span.End.Position;
            for (int i = startIndex; i < coloredSpans.Count && coloredSpans[i].Span.Start < spanEnd; i++)
            {
                if (_provider.TryGetValue(coloredSpans[i].Color, out var type))
                {
                    var overlap = span.Overlap(coloredSpans[i].Span);
                    if (overlap != null)
                    {
                        classifications.Add(new ClassificationSpan(overlap.Value, type));
                    }
                }
            }

            return classifications;
        }

        private sealed class SpanStartComparer : IComparer<ColoredSpan>
        {
            internal static SpanStartComparer Instance = new SpanStartComparer();

            public int Compare(ColoredSpan x, ColoredSpan y)
            {
                return x.Span.Start - y.Span.Start;
            }
        }

        #endregion
    }
}
