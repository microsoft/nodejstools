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
            _textBuffer = textBuffer;
            _regionBuilder = regionBuilder;
            _regionBuilder.RegionsChanged += OnRegionsChanged;
        }

        #region ITagger<IOutliningRegionTag>
        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || _currentRegions == null || _currentRegions.Count == 0)
                yield break;

            var snapshot = _textBuffer.CurrentSnapshot;
            SnapshotSpan entire = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End).TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);

            int startPosition = entire.Start.GetContainingLine().Start;
            int endPosition = entire.End.GetContainingLine().End;

            foreach (var region in _currentRegions)
            {
                int end = Math.Min(region.End, snapshot.Length);

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
            var snapshot = _textBuffer.CurrentSnapshot;

            if (e.Regions.TextBufferVersion == _textBuffer.CurrentSnapshot.Version.VersionNumber)
            {
                if (TagsChanged != null)
                {
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
