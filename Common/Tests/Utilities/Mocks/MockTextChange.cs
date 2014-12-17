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
using Microsoft.VisualStudio.Text;

namespace TestUtilities.Mocks {
    class MockTextChange : ITextChange {
        private readonly SnapshotSpan _removed;
        private readonly string _inserted;
        private readonly int _newStart;

        public MockTextChange(SnapshotSpan removedSpan, int newStart, string insertedText) {
            _removed = removedSpan;
            _inserted = insertedText;
            _newStart = newStart;
        }

        public int Delta {
            get { return _inserted.Length - _removed.Length; }
        }

        public int LineCountDelta {
            get { throw new NotImplementedException(); }
        }

        public int NewEnd {
            get {
                return NewPosition + _inserted.Length;
            }
        }

        public int NewLength {
            get { return _inserted.Length; }
        }

        public int NewPosition {
            get { return _newStart; }
        }

        public Span NewSpan {
            get {
                return new Span(NewPosition, NewLength);
            }
        }

        public string NewText {
            get { return _inserted; }
        }

        public int OldEnd {
            get { return _removed.End; }
        }

        public int OldLength {
            get { return _removed.Length; }
        }

        public int OldPosition {
            get { return _removed.Start; }
        }

        public Span OldSpan {
            get { return _removed.Span; }
        }

        public string OldText {
            get { return _removed.GetText(); }
        }
    }
}
