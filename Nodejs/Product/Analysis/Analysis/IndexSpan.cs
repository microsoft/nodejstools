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

namespace Microsoft.NodejsTools.Parsing
{
    /// <summary>
    /// This structure represents an immutable integer interval that describes a range of values, from Start to End. 
    /// 
    /// It is closed on the left and open on the right: [Start .. End). 
    /// </summary>
    [Serializable]
    internal struct IndexSpan : IEquatable<IndexSpan> {
        private readonly int _start, _length;

        public IndexSpan(int start, int length) {
            _start = start;
            _length = length;
        }

        public static IndexSpan FromBounds(int start, int end) {
            return new IndexSpan(start, end - start);
        }

        public int Start {
            get {
                return _start;
            }
        }

        public int End {
            get {
                return _start + _length;
            }
        }

        public int Length {
            get {
                return _length;
            }
        }

        public override int GetHashCode() {
            return Length.GetHashCode() ^ Start.GetHashCode() ^ End.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is IndexSpan) {
                return Equals((IndexSpan)obj);
            }
            return false;
        }

        public static bool operator ==(IndexSpan self, IndexSpan other) {
            return self.Equals(other);
        }

        public static bool operator !=(IndexSpan self, IndexSpan other) {
            return !self.Equals(other);
        }

        public IndexSpan FlattenToStart() {
            return new IndexSpan(Start, 0);
        }

        public IndexSpan FlattenToEnd() {
            return new IndexSpan(End, 0);
        }

        public IndexSpan CombineWith(IndexSpan other) {
            return IndexSpan.FromBounds(Start, other.End);
        }

        public IndexSpan UpdateWith(IndexSpan other) {
            int startPosition = Start;
            int endPosition = End;

            if (other.Start < Start) {
                startPosition = other.Start;
            }

            if (other.End > End) {
                endPosition = other.End;
            }

            return IndexSpan.FromBounds(startPosition, endPosition);
        }

        #region IEquatable<IndexSpan> Members

        public bool Equals(IndexSpan other) {
            return _length == other._length && _start == other._start;
        }

        #endregion

        public override string ToString() {
            return String.Format("({0}, {1}) length: {2}", Start, End, Length);
        }
    }
}
