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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.NodejsTools.Jade {

    class OutliningTagger : ITagger<IOutliningRegionTag> {
        private OutlineRegionCollection _currentRegions;
        private OutlineRegionBuilder _regionBuilder;
        private ITextBuffer _textBuffer;

        public OutliningTagger(ITextBuffer textBuffer, OutlineRegionBuilder regionBuilder) {
            _textBuffer = textBuffer;
            _regionBuilder = regionBuilder;
            _regionBuilder.RegionsChanged += OnRegionsChanged;
        }

        #region ITagger<IOutliningRegionTag>
        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
            if (spans.Count == 0 || _currentRegions == null || _currentRegions.Count == 0)
                yield break;

            var snapshot = _textBuffer.CurrentSnapshot;
            SnapshotSpan entire = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End).TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);

            int startPosition = entire.Start.GetContainingLine().Start;
            int endPosition = entire.End.GetContainingLine().End;

            foreach (var region in _currentRegions) {
                int end = Math.Min(region.End, snapshot.Length);

                if (region.Start <= endPosition && end >= startPosition) {
                    yield return new TagSpan<IOutliningRegionTag>(
                        new SnapshotSpan(snapshot, Span.FromBounds(region.Start, end)),
                        new OutliningRegionTag(false, false, region.DisplayText, region.HoverText));
                }
            }

        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        #endregion

        void OnRegionsChanged(object sender, OutlineRegionsChangedEventArgs e) {
            var snapshot = _textBuffer.CurrentSnapshot;

            if (e.Regions.TextBufferVersion == _textBuffer.CurrentSnapshot.Version.VersionNumber) {
                if (TagsChanged != null) {
                    int start = Math.Min(e.ChangedRange.Start, snapshot.Length);
                    int end = Math.Min(e.ChangedRange.End, snapshot.Length);

                    TagsChanged(this, new SnapshotSpanEventArgs(
                        new SnapshotSpan(snapshot, Span.FromBounds(start, end))));
                }

                _currentRegions = e.Regions;
            }
        }
    }
}
