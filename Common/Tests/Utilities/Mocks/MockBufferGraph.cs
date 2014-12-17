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
using System.Collections.ObjectModel;
using System.Windows.Documents;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace TestUtilities.Mocks {
    public class MockBufferGraph : IBufferGraph {
        private readonly MockTextView _view;
        private readonly List<ITextBuffer> _buffers = new List<ITextBuffer>();

        public MockBufferGraph(MockTextView view) {
            _view = view;
            _buffers.Add(view.TextBuffer);
        }

        public IMappingPoint CreateMappingPoint(SnapshotPoint point, PointTrackingMode trackingMode) {
            throw new NotImplementedException();
        }

        public IMappingSpan CreateMappingSpan(SnapshotSpan span, SpanTrackingMode trackingMode) {
            throw new NotImplementedException();
        }

        public Collection<ITextBuffer> GetTextBuffers(Predicate<ITextBuffer> match) {
            var res = new Collection<ITextBuffer>();
            foreach (var buffer in _buffers) {
                if (match(buffer)) {
                    res.Add(buffer);
                }
            }
            return res;
        }

        public void AddBuffer(ITextBuffer buffer) {
            _buffers.Add(buffer);
        }

        public event EventHandler<GraphBufferContentTypeChangedEventArgs> GraphBufferContentTypeChanged {
            add { }
            remove { }
        }

        public event EventHandler<GraphBuffersChangedEventArgs> GraphBuffersChanged {
            add {
            }
            remove {
            }
        }

        public NormalizedSnapshotSpanCollection MapDownToBuffer(SnapshotSpan span, SpanTrackingMode trackingMode, ITextBuffer targetBuffer) {
            throw new NotImplementedException();
        }

        public SnapshotPoint? MapDownToBuffer(SnapshotPoint position, PointTrackingMode trackingMode, ITextBuffer targetBuffer, PositionAffinity affinity) {
            throw new NotImplementedException();
        }

        public NormalizedSnapshotSpanCollection MapDownToFirstMatch(SnapshotSpan span, SpanTrackingMode trackingMode, Predicate<ITextSnapshot> match) {
            throw new NotImplementedException();
        }

        public SnapshotPoint? MapDownToFirstMatch(SnapshotPoint position, PointTrackingMode trackingMode, Predicate<ITextSnapshot> match, PositionAffinity affinity) {
            return position;
        }

        public SnapshotPoint? MapDownToInsertionPoint(SnapshotPoint position, PointTrackingMode trackingMode, Predicate<ITextSnapshot> match) {
            throw new NotImplementedException();
        }

        public NormalizedSnapshotSpanCollection MapDownToSnapshot(SnapshotSpan span, SpanTrackingMode trackingMode, ITextSnapshot targetSnapshot) {
            throw new NotImplementedException();
        }

        public SnapshotPoint? MapDownToSnapshot(SnapshotPoint position, PointTrackingMode trackingMode, ITextSnapshot targetSnapshot, PositionAffinity affinity) {
            throw new NotImplementedException();
        }

        public NormalizedSnapshotSpanCollection MapUpToBuffer(SnapshotSpan span, SpanTrackingMode trackingMode, ITextBuffer targetBuffer) {
            throw new NotImplementedException();
        }

        public SnapshotPoint? MapUpToBuffer(SnapshotPoint point, PointTrackingMode trackingMode, PositionAffinity affinity, ITextBuffer targetBuffer) {
            int position = 0;
            for (int i = 0; i < _buffers.Count; i++) {
                if (_buffers[i] == targetBuffer) {
                    return new SnapshotPoint(point.Snapshot, position + point.Position);
                }
                position += _buffers[i].CurrentSnapshot.Length;
            }
            return null;
        }

        public NormalizedSnapshotSpanCollection MapUpToFirstMatch(SnapshotSpan span, SpanTrackingMode trackingMode, Predicate<ITextSnapshot> match) {
            throw new NotImplementedException();
        }

        public SnapshotPoint? MapUpToFirstMatch(SnapshotPoint point, PointTrackingMode trackingMode, Predicate<ITextSnapshot> match, PositionAffinity affinity) {
            throw new NotImplementedException();
        }

        public NormalizedSnapshotSpanCollection MapUpToSnapshot(SnapshotSpan span, SpanTrackingMode trackingMode, ITextSnapshot targetSnapshot) {
            throw new NotImplementedException();
        }

        public SnapshotPoint? MapUpToSnapshot(SnapshotPoint point, PointTrackingMode trackingMode, PositionAffinity affinity, ITextSnapshot targetSnapshot) {
            throw new NotImplementedException();
        }

        public ITextBuffer TopBuffer {
            get { throw new NotImplementedException(); }
        }
    }
}
