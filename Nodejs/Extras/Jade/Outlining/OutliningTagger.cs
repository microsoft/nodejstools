// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.NodejsTools.Jade
{
    internal class OutliningTagger : ITagger<IOutliningRegionTag>
    {
        private OutlineRegionCollection _currentRegions;
        private OutlineRegionBuilder _regionBuilder;
        private ITextBuffer _textBuffer;

        public OutliningTagger(ITextBuffer textBuffer, OutlineRegionBuilder regionBuilder)
        {
            this._textBuffer = textBuffer;
            this._regionBuilder = regionBuilder;
            this._regionBuilder.RegionsChanged += this.OnRegionsChanged;
        }

        #region ITagger<IOutliningRegionTag>
        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || this._currentRegions == null || this._currentRegions.Count == 0)
            {
                yield break;
            }

            var snapshot = this._textBuffer.CurrentSnapshot;
            var entire = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End).TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);

            int startPosition = entire.Start.GetContainingLine().Start;
            int endPosition = entire.End.GetContainingLine().End;

            foreach (var region in this._currentRegions)
            {
                var end = Math.Min(region.End, snapshot.Length);

                if (region.Start <= endPosition && end >= startPosition)
                {
                    yield return new TagSpan<IOutliningRegionTag>(
                        new SnapshotSpan(snapshot, Span.FromBounds(region.Start, end)),
                        new OutliningRegionTag(false, false, region.DisplayText, region.HoverText));
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        #endregion

        private void OnRegionsChanged(object sender, OutlineRegionsChangedEventArgs e)
        {
            var snapshot = this._textBuffer.CurrentSnapshot;

            if (e.Regions.TextBufferVersion == this._textBuffer.CurrentSnapshot.Version.VersionNumber)
            {
                if (TagsChanged != null)
                {
                    var start = Math.Min(e.ChangedRange.Start, snapshot.Length);
                    var end = Math.Min(e.ChangedRange.End, snapshot.Length);

                    TagsChanged(this, new SnapshotSpanEventArgs(
                        new SnapshotSpan(snapshot, Span.FromBounds(start, end))));
                }

                this._currentRegions = e.Regions;
            }
        }
    }
}
