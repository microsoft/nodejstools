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
        private readonly ReplOutputClassifierProvider provider;
        internal static readonly object ColorKey = new object();
        private readonly ITextBuffer buffer;

        public ReplOutputClassifier(ReplOutputClassifierProvider provider, ITextBuffer buffer)
        {
            this.provider = provider;
            this.buffer = buffer;
            this.buffer.Changed += (_, e) =>
            {
                if (e.After.Length == 0)
                {
                    this.buffer.Properties[ColorKey] = new List<ColoredSpan>();
                }
            };
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
        {
            add { }
            remove { }
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            if (!this.buffer.Properties.TryGetProperty(ColorKey, out List<ColoredSpan> coloredSpans))
            {
                return new ClassificationSpan[0];
            }

            var classifications = new List<ClassificationSpan>();

            var startIndex = coloredSpans.BinarySearch(new ColoredSpan(span, InteractiveWindowColor.White), SpanStartComparer.Instance);
            if (startIndex < 0)
            {
                startIndex = ~startIndex - 1;
                if (startIndex < 0) {
                    startIndex = 0;
                }
            }

            var spanEnd = span.End.Position;
            for (var i = startIndex; i < coloredSpans.Count && coloredSpans[i].Span.Start < spanEnd; i++)
            {
                if (coloredSpans[i].Color.HasValue && this.provider.TryGetValue(coloredSpans[i].Color.Value, out var type))
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
            public static SpanStartComparer Instance = new SpanStartComparer();

            public int Compare(ColoredSpan x, ColoredSpan y)
            {
                return x.Span.Start - y.Span.Start;
            }
        }
    }

    internal sealed class ColoredSpan
    {
        public readonly Span Span;
        public readonly InteractiveWindowColor? Color;

        public ColoredSpan(Span span, InteractiveWindowColor? color)
        {
            this.Span = span;
            this.Color = color;
        }
    }
}
