// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Represents a range in a text buffer or a string. Specified start and end of text. 
    /// End is exclusive, i.e. Length = End - Start. Implements IComparable that compares
    /// range start positions. 
    /// </summary>
    [DebuggerDisplay("[{Start}...{End}], Length = {Length}")]
    internal class TextRange : IExpandableTextRange, ICloneable, IComparable
    {
        private static TextRange _emptyRange = new TextRange(0, 0);

        private int _start;
        private int _end;

        /// <summary>
        /// Returns an empty, invalid range.
        /// </summary>
        public static TextRange EmptyRange => _emptyRange;
        /// <summary>
        /// Creates text range starting at position 0
        /// and length of 1
        /// </summary>
        public TextRange()
            : this(0)
        {
        }

        /// <summary>
        /// Creates text range starting at given position 
        /// and length of 1.
        /// </summary>
        /// <param name="position">Start position</param>
        public TextRange(int position)
        {
            this._start = position;
            this._end = position < int.MaxValue ? position + 1 : position;
        }

        /// <summary>
        /// Creates text range based on start and end positions.
        /// End is exclusive, Length = End - Start
        /// <param name="start">Range start</param>
        /// <param name="length">Range length</param>
        /// </summary>
        public TextRange(int start, int length)
        {
            if (length < 0)
            {
                throw new ArgumentException("Length must not be negative", nameof(length));
            }

            this._start = start;
            this._end = start + length;
        }

        /// <summary>
        /// Creates text range based on another text range
        /// </summary>
        /// <param name="range">Text range to use as position source</param>
        public TextRange(ITextRange range)
            : this(range.Start, range.Length)
        {
        }

        /// <summary>
        /// Resets text range to (0, 0)
        /// </summary>
        public void Empty()
        {
            this._start = 0;
            this._end = 0;
        }

        /// <summary>
        /// Creates text range based on start and end positions.
        /// End is exclusive, Length = End - Start
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public static TextRange FromBounds(int start, int end)
        {
            return new TextRange(start, end - start);
        }

        /// <summary>
        /// Finds out of range intersects another range
        /// </summary>
        /// <param name="start">Start of another range</param>
        /// <param name="length">Length of another range</param>
        /// <returns>True if ranges intersect</returns>
        public virtual bool Intersect(int start, int length)
        {
            return TextRange.Intersect(this, start, length);
        }

        /// <summary>
        /// Finds out of range intersects another range
        /// </summary>
        /// <param name="start">Text range</param>
        /// <returns>True if ranges intersect</returns>
        public virtual bool Intersect(ITextRange range)
        {
            return TextRange.Intersect(this, range.Start, range.Length);
        }
        /// <summary>
        /// Finds out if range represents valid text range (it's length is greater than zero)
        /// </summary>
        /// <returns>True if range is valid</returns>
        public virtual bool IsValid()
        {
            return TextRange.IsValid(this);
        }

        #region ITextRange
        /// <summary>
        /// Text range start position
        /// </summary>
        public int Start => this._start;
        /// <summary>
        /// Text range end position (excluded)
        /// </summary>
        public int End => this._end;
        /// <summary>
        /// Text range length
        /// </summary>
        public int Length => this.End - this.Start;
        /// <summary>
        /// Determines if range contains given position
        /// </summary>
        public virtual bool Contains(int position)
        {
            return TextRange.Contains(this, position);
        }

        /// <summary>
        /// Determines if text range fully contains another range
        /// </summary>
        /// <param name="range"></param>
        public virtual bool Contains(ITextRange range)
        {
            return Contains(range.Start) && Contains(range.End);
        }

        /// <summary>
        /// Determines if element contains one or more of the ranges
        /// </summary>
        /// <returns></returns>
        public virtual bool Contains(IEnumerable<ITextRange> ranges)
        {
            if (ranges == null)
            {
                return false;
            }

            var contains = false;

            foreach (var range in ranges)
            {
                if (Contains(range))
                {
                    contains = true;
                    break;
                }
            }

            return contains;
        }

        /// <summary>
        /// Shifts text range by a given offset
        /// </summary>
        public void Shift(int offset)
        {
            this._start += offset;
            this._end += offset;
        }

        public void Expand(int startOffset, int endOffset)
        {
            if (this._start + startOffset > this._end + endOffset)
            {
                throw new ArgumentException("Combination of start and end offsets should not be making range invalid");
            }

            this._start += startOffset;
            this._end += endOffset;
        }
        #endregion

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0}...{1}]", this.Start, this.End);
        }

        /// <summary>
        /// Determines if ranges are equal. Ranges are equal when they are either both null
        /// or both are not null and their coordinates are equal.
        /// </summary>
        /// <param name="left">First range</param>
        /// <param name="right">Second range</param>
        /// <returns>True if ranges are equal</returns>
        public static bool AreEqual(ITextRange left, ITextRange right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)left == null) || ((object)right == null))
            {
                return false;
            }

            return (left.Start == right.Start) && (left.End == right.End);
        }

        /// <summary>
        /// Determines if range contains given position
        /// </summary>
        /// <param name="range">Text range</param>
        /// <param name="position">Position</param>
        /// <returns>True if position is inside the range</returns>
        public static bool Contains(ITextRange range, int position)
        {
            return Contains(range.Start, range.Length, position);
        }

        /// <summary>
        /// Determines if range contains another range
        /// </summary>
        public static bool Contains(ITextRange range, ITextRange other)
        {
            var textRange = new TextRange(range);
            return textRange.Contains(other);
        }

        /// <summary>
        /// Determines if range contains all ranges in a collection
        /// </summary>
        public static bool Contains(ITextRange range, IEnumerable<ITextRange> ranges)
        {
            var textRange = new TextRange(range);
            return textRange.Contains(ranges);
        }

        /// <summary>
        /// Determines if range contains given position
        /// </summary>
        /// <param name="rangeStart">Start of the text range</param>
        /// <param name="rangeLength">Length of the text range</param>
        /// <param name="position">Position</param>
        /// <returns>Tru if position is inside the range</returns>
        public static bool Contains(int rangeStart, int rangeLength, int position)
        {
            if (rangeLength == 0 && position == rangeStart)
            {
                return true;
            }

            return position >= rangeStart && position < rangeStart + rangeLength;
        }

        /// <summary>
        /// Finds out if range intersects another range
        /// </summary>
        /// <param name="range1">First text range</param>
        /// <param name="range2">Second text range</param>
        /// <returns>True if ranges intersect</returns>
        public static bool Intersect(ITextRange range1, ITextRange range2)
        {
            return Intersect(range1, range2.Start, range2.Length);
        }

        /// <summary>
        /// Finds out if range intersects another range
        /// </summary>
        /// <param name="range">First text range</param>
        /// <param name="rangeStart2">Start of the second range</param>
        /// <param name="rangeLength2">Length of the second range</param>
        /// <returns>True if ranges intersect</returns>
        public static bool Intersect(ITextRange range1, int rangeStart2, int rangeLength2)
        {
            return Intersect(range1.Start, range1.Length, rangeStart2, rangeLength2);
        }

        /// <summary>
        /// Finds out if range intersects another range
        /// </summary>
        /// <param name="rangeStart1">Start of the first range</param>
        /// <param name="rangeLength1">Length of the first range</param>
        /// <param name="rangeStart2">Start of the second range</param>
        /// <param name="rangeLength2">Length of the second range</param>
        /// <returns>True if ranges intersect</returns>
        public static bool Intersect(int rangeStart1, int rangeLength1, int rangeStart2, int rangeLength2)
        {
            // !(rangeEnd2 <= rangeStart1 || rangeStart2 >= rangeEnd1)

            // Support intersection with empty ranges

            if (rangeLength1 == 0 && rangeLength2 == 0)
            {
                return rangeStart1 == rangeStart2;
            }

            if (rangeLength1 == 0)
            {
                return Contains(rangeStart2, rangeLength2, rangeStart1);
            }

            if (rangeLength2 == 0)
            {
                return Contains(rangeStart1, rangeLength1, rangeStart2);
            }

            return rangeStart2 + rangeLength2 > rangeStart1 && rangeStart2 < rangeStart1 + rangeLength1;
        }

        /// <summary>
        /// Finds out if range represents valid text range (when range is not null and it's length is greater than zero)
        /// </summary>
        /// <returns>True if range is valid</returns>
        public static bool IsValid(ITextRange range)
        {
            return range != null && range.Length > 0;
        }

        /// <summary>
        /// Calculates range that includes both supplied ranges.
        /// </summary>
        public static ITextRange Union(ITextRange range1, ITextRange range2)
        {
            var start = Math.Min(range1.Start, range2.Start);
            var end = Math.Max(range1.End, range2.End);

            return start <= end ? TextRange.FromBounds(start, end) : TextRange.EmptyRange;
        }

        /// <summary>
        /// Calculates range that includes both supplied ranges.
        /// </summary>
        public static ITextRange Union(ITextRange range1, int rangeStart, int rangeLength)
        {
            var start = Math.Min(range1.Start, rangeStart);
            var end = Math.Max(range1.End, rangeStart + rangeLength);

            return start <= end ? TextRange.FromBounds(start, end) : TextRange.EmptyRange;
        }

        /// <summary>
        /// Calculates range that is an intersection of the supplied ranges.
        /// </summary>
        /// <returns>Intersection or empty range if ranges don't intersect</returns>
        public static ITextRange Intersection(ITextRange range1, ITextRange range2)
        {
            var start = Math.Max(range1.Start, range2.Start);
            var end = Math.Min(range1.End, range2.End);

            return start <= end ? TextRange.FromBounds(start, end) : TextRange.EmptyRange;
        }

        /// <summary>
        /// Calculates range that is an intersection of the supplied ranges.
        /// </summary>
        /// <returns>Intersection or empty range if ranges don't intersect</returns>
        public static ITextRange Intersection(ITextRange range1, int rangeStart, int rangeLength)
        {
            var start = Math.Max(range1.Start, rangeStart);
            var end = Math.Min(range1.End, rangeStart + rangeLength);

            return start <= end ? TextRange.FromBounds(start, end) : TextRange.EmptyRange;
        }

        /// <summary>
        /// Creates copy of the text range object via memberwise cloning
        /// </summary>
        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        public int CompareTo(object obj)
        {
            var other = obj as TextRange;

            if (other == null)
            {
                return -1;
            }

            return this.Start.CompareTo(other.Start);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(TextRange range1, TextRange range2)
        {
            if ((object)range1 == null && (object)range2 == null)
            {
                return true;
            }

            if ((object)range1 == null || (object)range2 == null)
            {
                return false;
            }

            return range1.Equals(range2);
        }

        public static bool operator !=(TextRange range1, TextRange range2)
        {
            return !(range1 == range2);
        }

        public static bool operator <(TextRange range1, TextRange range2)
        {
            if ((object)range1 == null || (object)range2 == null)
            {
                return false;
            }

            return range1.CompareTo(range2) < 0;
        }

        public static bool operator >(TextRange range1, TextRange range2)
        {
            if ((object)range1 == null || (object)range2 == null)
            {
                return false;
            }

            return range1.CompareTo(range2) > 0;
        }
    }
}
