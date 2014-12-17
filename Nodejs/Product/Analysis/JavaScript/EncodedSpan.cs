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

namespace Microsoft.NodejsTools.Parsing {
    /// <summary>
    /// Represents a span encoded as a single integer.  The spans
    /// offset and length are encoded into the lower 31 bits if they
    /// fit.  Otherwise the high bit is set and the span needs to be
    /// resolved via a LocationResolver instance.
    /// </summary>
    [Serializable]
    internal struct EncodedSpan : IEquatable<EncodedSpan> {
        internal readonly int Span;
        private const int _offsetBits = 16;
        private const int _lengthBits = 15;

        public EncodedSpan(LocationResolver resolver, int start, int length) {
            if (start < (1 << _offsetBits) && length < (1 << _lengthBits)) {
                Span = start | (length << _offsetBits);
            } else {
                Span = resolver.AddSpan(start, length);
            }
        }

        public EncodedSpan(LocationResolver resolver, IndexSpan span)
            : this(resolver, span.Start, span.Length) {
        }

        public EncodedSpan(int location) {
            Span = location;
        }

        public override int GetHashCode() {
            return Span;
        }

        public override bool Equals(object obj) {
            if (obj is EncodedSpan) {
                return Equals((EncodedSpan)obj);
            }
            return false;
        }

        #region IEquatable<EncodedLocation> Members

        public bool Equals(EncodedSpan other) {
            return Span == other.Span;
        }

        #endregion

        public EncodedSpan CombineWith(LocationResolver resolver, EncodedSpan otherSpan) {
            return new EncodedSpan(
                resolver,
                GetSpan(resolver).CombineWith(otherSpan.GetSpan(resolver))
            );
        }

        public IndexSpan GetSpan(LocationResolver resolver) {
            if ((Span & 0x80000000) == 0) {
                return new IndexSpan(
                    Span & ((1 << _offsetBits) - 1),
                    (Span >> _offsetBits) & ((1 << _lengthBits) - 1)
                );
            }

            return resolver._spans[(int)(Span & ~(0x80000000))];
        }

        public int GetStartIndex(LocationResolver resolver) {
            return GetSpan(resolver).Start;
        }

        public int GetEndIndex(LocationResolver resolver) {
            return GetSpan(resolver).End;
        }
    }
}
