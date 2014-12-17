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
    public class MockTrackingPoint : ITrackingPoint {
        private readonly int _position;
        private readonly MockTextSnapshot _snapshot;
        private readonly PointTrackingMode _mode;

        public MockTrackingPoint(MockTextSnapshot snapshot, int position, PointTrackingMode mode = PointTrackingMode.Positive) {
            _position = position;
            _snapshot = snapshot;
            _mode = mode;
        }

        private SnapshotPoint GetPoint(ITextVersion version) {
            var current = _snapshot.Version;
            var target = version;
            if (current.VersionNumber == target.VersionNumber) {
                return new SnapshotPoint(_snapshot, _position);
            } else if (current.VersionNumber > target.VersionNumber) {
                // Apply the changes in reverse
                var changesStack = new Stack<INormalizedTextChangeCollection>();

                for (var v = target; v.VersionNumber < current.VersionNumber; v = v.Next) {
                    changesStack.Push(v.Changes);
                }

                var newPos = _position;

                while (changesStack.Count > 0) {
                    foreach (var change in changesStack.Pop()) {
                        if (change.Delta > 0 && change.NewPosition <= newPos && change.NewPosition - change.Delta > newPos) {
                            // point was deleted
                            newPos = change.NewPosition;
                        } else if (change.NewPosition == newPos) {
                            if (_mode == PointTrackingMode.Positive) {
                                newPos -= change.Delta;
                            }
                        } else if (change.NewPosition < newPos) {
                            newPos -= change.Delta;
                        }
                    }
                }

                return new SnapshotPoint(((MockTextVersion)target)._snapshot, newPos);
            } else {
                // Apply the changes normally
                var newPos = _position;
                for (var v = current; v.VersionNumber < target.VersionNumber; v = v.Next) {
                    foreach (var change in v.Changes) {
                        if (change.Delta < 0 && change.OldPosition <= newPos && change.OldPosition - change.Delta > newPos) {
                            // point was deleted
                            newPos = change.OldPosition;
                        } else if (change.OldPosition == newPos) {
                            if (_mode == PointTrackingMode.Positive) {
                                newPos += change.Delta;
                            }
                        } else if(change.OldPosition < newPos) {
                            newPos += change.Delta;
                        }
                    }
                }

                return new SnapshotPoint(((MockTextVersion)target)._snapshot, newPos);
            }
        }

        public SnapshotPoint GetPoint(ITextSnapshot snapshot) {
            return GetPoint(snapshot.Version);
        }

        public char GetCharacter(ITextSnapshot snapshot) {
            return GetPoint(snapshot.Version).GetChar();
        }

        public int GetPosition(ITextVersion version) {
            return GetPoint(version).Position;
        }

        public int GetPosition(ITextSnapshot snapshot) {
            return GetPoint(snapshot).Position;
        }

        public ITextBuffer TextBuffer {
            get { return _snapshot.TextBuffer; }
        }

        public TrackingFidelityMode TrackingFidelity {
            get { throw new NotImplementedException(); }
        }

        public PointTrackingMode TrackingMode {
            get { throw new NotImplementedException(); }
        }

    }
}
