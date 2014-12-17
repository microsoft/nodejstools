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
using Microsoft.VisualStudio.Text.Classification;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif
    /// <summary>
    /// Classifies regions for REPL error output spans.  These are always classified as errors.
    /// </summary>
    class ReplOutputClassifier : IClassifier {
        private readonly ReplOutputClassifierProvider _provider;
        internal static object ColorKey = new object();
        private readonly ITextBuffer _buffer;

        public ReplOutputClassifier(ReplOutputClassifierProvider provider, ITextBuffer buffer) {
            _provider = provider;
            _buffer = buffer;
        }

        #region IClassifier Members

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged {
            add { }
            remove { }
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span) {
            List<ColoredSpan> coloredSpans;
            if (!_buffer.Properties.TryGetProperty(ColorKey, out coloredSpans)) {
                return new ClassificationSpan[0];
            }

            List<ClassificationSpan> classifications = new List<ClassificationSpan>();

            int startIndex = coloredSpans.BinarySearch(new ColoredSpan(span, ConsoleColor.White), SpanStartComparer.Instance);
            if (startIndex < 0) {
                startIndex = ~startIndex - 1;
            }

            int spanEnd = span.End.Position;
            for (int i = startIndex; i < coloredSpans.Count && coloredSpans[i].Span.Start < spanEnd; i++) {
                IClassificationType type;
                if (_provider._classTypes.TryGetValue(coloredSpans[i].Color, out type)) {
                    var overlap = span.Overlap(coloredSpans[i].Span);
                    if (overlap != null) {
                        classifications.Add(new ClassificationSpan(overlap.Value, type));
                    }
                }
            }

            return classifications;
        }

        private sealed class SpanStartComparer : IComparer<ColoredSpan> {
            internal static SpanStartComparer Instance = new SpanStartComparer();

            public int Compare(ColoredSpan x, ColoredSpan y) {
                return x.Span.Start - y.Span.Start;
            }
        }

        #endregion
    }
}
