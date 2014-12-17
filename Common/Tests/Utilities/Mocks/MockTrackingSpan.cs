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

namespace TestUtilities.Mocks {
    public class MockTrackingSpan : ITrackingSpan {
        private readonly int _start, _length;
        private readonly MockTextSnapshot _snapshot;

        public MockTrackingSpan(MockTextSnapshot snapshot, int start, int length) {
            _start = start;
            _length = length;
            _snapshot = snapshot;
        }

        public SnapshotPoint GetEndPoint(ITextSnapshot snapshot) {
            return new SnapshotPoint(_snapshot, _start + _length);
        }

        public Span GetSpan(ITextVersion version) {
            var current = _snapshot.Version;
            var target = version;
            if (current.VersionNumber == target.VersionNumber) {
                return new Span(_start, _length);
            } else if (current.VersionNumber > target.VersionNumber) {
                // Apply the changes in reverse
                var changesStack = new Stack<INormalizedTextChangeCollection>();

                for (var v = target; v.VersionNumber < current.VersionNumber; v = v.Next) {
                    changesStack.Push(v.Changes);
                }

                var newStart = _start;
                var newLength = _length;

                while (changesStack.Count > 0) {
                    foreach (var change in changesStack.Pop()) {
                        if (change.NewPosition <= newStart) {
                            newStart -= change.Delta;
                        } else if (change.NewPosition <= newStart + newLength) {
                            newLength -= change.Delta;
                        }
                    }
                }

                return new Span(newStart, newLength);
            } else {
                // Apply the changes normally
                var newStart = _start;
                var newLength = _length;

                for (var v = current; v.VersionNumber < target.VersionNumber; v = v.Next) {
                    foreach (var change in v.Changes) {
                        if (change.OldPosition < newStart) {
                            newStart += change.Delta;
                        } else if (change.OldPosition < newStart + newLength) {
                            newLength += change.Delta;
                        }
                    }
                }

                return new Span(newStart, newLength);
            }
        }

        public SnapshotSpan GetSpan(ITextSnapshot snapshot) {
            return new SnapshotSpan(snapshot, GetSpan(snapshot.Version));
        }

        public SnapshotPoint GetStartPoint(ITextSnapshot snapshot) {
            var span = GetSpan(snapshot.Version);
            return new SnapshotPoint(snapshot, span.Start);
        }

        public string GetText(ITextSnapshot snapshot) {
            var span = GetSpan(snapshot.Version);
            return snapshot.GetText(span);
        }

        public ITextBuffer TextBuffer {
            get { return _snapshot.TextBuffer; }
        }

        public TrackingFidelityMode TrackingFidelity {
            get { throw new NotImplementedException(); }
        }

        public SpanTrackingMode TrackingMode {
            get { throw new NotImplementedException(); }
        }
    }
}
