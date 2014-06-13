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

namespace Microsoft.NodejsTools.Parsing
{
    /// <summary>
    /// This structure represents an immutable integer interval that describes a range of values, from Start to End. 
    /// 
    /// It is closed on the left and open on the right: [Start .. End). 
    /// </summary>
    [Serializable]
    public struct IndexSpan : IEquatable<IndexSpan> {
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
            if (other == null) {
                return this;
            }

            return IndexSpan.FromBounds(Start, other.End);
        }

        public IndexSpan UpdateWith(IndexSpan other) {
            if (other != null) {
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

            return this;
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
