// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// A collection of text ranges or objects that implement <seealso cref="ITextRange"/>. 
    /// Ranges must not overlap. Can be sorted by range start positions. Can be searched 
    /// in order to locate range that contains given position or range that starts 
    /// at a given position. The search is a binary search. Collection implements 
    /// <seealso cref="ITextRangeCollection"/>
    /// </summary>
    /// <typeparam name="T">A class or an interface that derives from <seealso cref="ITextRange"/></typeparam>
    [DebuggerDisplay("Count={Count}")]
    internal class TextRangeCollection<T> : IEnumerable<T>, ITextRangeCollection<T> where T : ITextRange
    {
        private static readonly IList<T> _emptyList = Array.Empty<T>();

        private List<T> _items = new List<T>();

        #region Construction
        public TextRangeCollection()
        {
        }

        public TextRangeCollection(IEnumerable<T> ranges)
        {
            Add(ranges);
            Sort();
        }
        #endregion

        #region ITextRange

        public int Start => this.Count > 0 ? this[0].Start : 0;
        public int End => this.Count > 0 ? this[this.Count - 1].End : 0;
        public int Length => this.End - this.Start;
        public virtual bool Contains(int position)
        {
            return TextRange.Contains(this, position);
        }

        public void Shift(int offset)
        {
            foreach (var ct in this._items)
            {
                ct.Shift(offset);
            }
        }
        #endregion

        #region ITextRangeCollection

        /// <summary>
        /// Number of comments in the collection.
        /// </summary>
        public int Count => this._items.Count;
        /// <summary>
        /// Sorted list of comment tokens in the document.
        /// </summary>
        public IList<T> Items => this._items;
        /// <summary>
        /// Retrieves Nth item in the collection
        /// </summary>
        public T this[int index] { get { return this._items[index]; } }

        /// <summary>
        /// Adds item to collection.
        /// </summary>
        /// <param name="item">Item to add</param>
        public virtual void Add(T item)
        {
            this._items.Add(item);
        }

        /// <summary>
        /// Add a range of items to the collection
        /// </summary>
        /// <param name="items">Items to add</param>
        public void Add(IEnumerable<T> items)
        {
            this._items.AddRange(items);
        }

        /// <summary>
        /// Returns index of item that starts at the given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public virtual int GetItemAtPosition(int position)
        {
            if (this.Count == 0)
            {
                return -1;
            }

            if (position < this[0].Start)
            {
                return -1;
            }

            if (position >= this[this.Count - 1].End)
            {
                return -1;
            }

            var min = 0;
            var max = this.Count - 1;

            while (min <= max)
            {
                var mid = min + (max - min) / 2;
                var item = this[mid];

                if (item.Start == position)
                {
                    return mid;
                }

                if (position < item.Start)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns index of items that contains given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public virtual int GetItemContaining(int position)
        {
            if (this.Count == 0)
            {
                return -1;
            }

            if (position < this[0].Start)
            {
                return -1;
            }

            if (position > this[this.Count - 1].End)
            {
                return -1;
            }

            var min = 0;
            var max = this.Count - 1;

            while (min <= max)
            {
                var mid = min + (max - min) / 2;
                var item = this[mid];

                if (item.Contains(position))
                {
                    return mid;
                }

                if (mid < this.Count - 1 && item.End <= position && position < this[mid + 1].Start)
                {
                    return -1;
                }

                if (position < item.Start)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Retrieves first item that is after a given position
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public virtual int GetFirstItemAfterPosition(int position)
        {
            if (this.Count == 0 || position > this[this.Count - 1].End)
            {
                return -1;
            }

            if (position < this[0].Start)
            {
                return 0;
            }

            var min = 0;
            var max = this.Count - 1;

            while (min <= max)
            {
                var mid = min + (max - min) / 2;
                var item = this[mid];

                if (item.Contains(position))
                {
                    // Note that there may be multiple items with the same range.
                    // To be sure we do pick the first one, walk back until we include
                    // all elements containing passed position
                    return GetFirstElementContainingPosition(mid, position);
                }

                if (mid > 0 && this[mid - 1].End <= position && item.Start >= position)
                {
                    return mid;
                }

                if (position < item.Start)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return -1;
        }

        private int GetFirstElementContainingPosition(int index, int position)
        {
            for (var i = index - 1; i >= 0; i--)
            {
                var item = this[i];

                if (!item.Contains(position))
                {
                    index = i + 1;
                    break;
                }
                else if (i == 0)
                {
                    return 0;
                }
            }

            return index;
        }

        // assuming the item at index already contains the position
        private int GetLastElementContainingPosition(int index, int position)
        {
            for (var i = index; i < this.Count; i++)
            {
                var item = this[i];

                if (!item.Contains(position))
                {
                    return i - 1;
                }
            }

            return this.Count - 1;
        }

        /// <summary>
        /// Retrieves first item that is after a given position
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public virtual int GetLastItemBeforeOrAtPosition(int position)
        {
            if (this.Count == 0 || position < this[0].Start)
            {
                return -1;
            }

            if (position >= this[this.Count - 1].End)
            {
                return this.Count - 1;
            }

            var min = 0;
            var max = this.Count - 1;

            while (min <= max)
            {
                var mid = min + (max - min) / 2;
                var item = this[mid];

                if (item.Contains(position))
                {
                    // Note that there may be multiple items with the same range.
                    // To be sure we do pick the first one, walk back until we include
                    // all elements containing passed position
                    return GetLastElementContainingPosition(mid, position);
                }

                // position is in between two tokens
                if (mid > 0 && this[mid - 1].End <= position && item.Start >= position)
                {
                    return mid - 1;
                }

                if (position < item.Start)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return -1;
        }
        /// <summary>
        /// Retrieves first item that is before a given position, not intersecting or touching with position
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public virtual int GetFirstItemBeforePosition(int position)
        {
            if (this.Count == 0 || position < this[0].End)
            {
                return -1;
            }

            var min = 0;
            var lastIndex = this.Count - 1;
            var max = this.Count - 1;

            if (position >= this[lastIndex].End)
            {
                return max;
            }

            while (min <= max)
            {
                var mid = min + (max - min) / 2;
                var item = this[mid];

                if (item.Contains(position)) // guaranteed not to be negative by the first if in this method
                {
                    return mid - 1;
                }

                if (mid < lastIndex && this[mid + 1].Start >= position && item.End <= position)
                {
                    return mid;
                }

                if (position < item.Start)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns index of items that contains given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public virtual IList<int> GetItemsContainingInclusiveEnd(int position)
        {
            IList<int> list = new List<int>();

            if (this.Count > 0 &&
                position >= this[0].Start &&
                position <= this[this.Count - 1].End)
            {
                var min = 0;
                var max = this.Count - 1;

                while (min <= max)
                {
                    var mid = min + (max - min) / 2;
                    var item = this[mid];

                    if (item.Contains(position) || item.End == position)
                    {
                        list = GetItemsContainingInclusiveEndLinearFromAPoint(mid, position);
                        break;
                    }

                    if (mid < this.Count - 1 && item.End <= position && position < this[mid + 1].Start)
                    {
                        break;
                    }

                    if (position < item.Start)
                    {
                        max = mid - 1;
                    }
                    else
                    {
                        min = mid + 1;
                    }
                }
            }

            return list;
        }

        private IList<int> GetItemsContainingInclusiveEndLinearFromAPoint(int startingPoint, int position)
        {
            Debug.Assert(this.Count > 0 && startingPoint < this.Count, "Starting point not in token list");

            var list = new List<int>();

            for (var i = startingPoint; i >= 0; i--)
            {
                var item = this[i];

                if (item.Contains(position) || item.End == position)
                {
                    list.Insert(0, i);
                }
                else
                {
                    break;
                }
            }

            if (startingPoint + 1 < this.Count)
            {
                for (var i = startingPoint + 1; i < this.Count; i++)
                {
                    var item = this[i];

                    if (item.Contains(position) || item.End == position)
                    {
                        list.Add(i);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return list;
        }

        #region ICompositeTextRange
        /// <summary>
        /// Shifts all tokens at of below given position by the specified offset.
        /// </summary>
        public virtual void ShiftStartingFrom(int position, int offset)
        {
            var min = 0;
            var max = this.Count - 1;

            if (this.Count == 0)
            {
                return;
            }

            if (position <= this[0].Start)
            {
                // all children are below the shifting point
                Shift(offset);
            }
            else
            {
                while (min <= max)
                {
                    var mid = min + (max - min) / 2;

                    if (this[mid].Contains(position))
                    {
                        // Found: item contains start position
                        if (this[mid] is ICompositeTextRange composite)
                        {
                            composite.ShiftStartingFrom(position, offset);
                        }
                        else
                        {
                            if (this[mid] is IExpandableTextRange expandable)
                            {
                                expandable.Expand(0, offset);
                            }
                        }

                        // Now shift all remaining siblings that are below this one
                        for (var i = mid + 1; i < this.Count; i++)
                        {
                            this[i].Shift(offset);
                        }

                        return;
                    }
                    else if (mid < this.Count - 1 && this[mid].End <= position && position <= this[mid + 1].Start)
                    {
                        // Between this item and the next sibling. Shift siblings
                        for (var i = mid + 1; i < this.Count; i++)
                        {
                            this[i].Shift(offset);
                        }

                        return;
                    }

                    // Position does not belong to this item and is not between item end and next item start
                    if (position < this[mid].Start)
                    {
                        // Item is after the given position. There may be better items 
                        // before this one so limit search to the range ending in this item.
                        max = mid - 1;
                    }
                    else
                    {
                        // Proceed forward
                        min = mid + 1;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Finds out items that overlap a text range
        /// </summary>
        /// <param name="range">Text range</param>
        /// <returns>List of items that overlap the range</returns>
        public virtual IList<T> ItemsInRange(ITextRange range)
        {
            var list = _emptyList;

            var first = GetItemContaining(range.Start);
            if (first < 0)
            {
                first = GetFirstItemAfterPosition(range.Start);
            }

            if (first >= 0)
            {
                for (var i = first; i < this.Count; i++)
                {
                    if (this._items[i].Start >= range.End)
                    {
                        break;
                    }

                    if (TextRange.Intersect(this._items[i], range))
                    {
                        if (list == _emptyList)
                        {
                            list = new List<T>();
                        }

                        list.Add(this._items[i]);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Removes items that overlap given text range
        /// </summary>
        /// <param name="range">Range to remove items in</param>
        /// <returns>Collection of removed items</returns>
        public ICollection<T> RemoveInRange(ITextRange range)
        {
            return RemoveInRange(range, false);
        }

        /// <summary>
        /// Removes items that overlap given text range
        /// </summary>
        /// <param name="range">Range to remove items in</param>
        /// <param name="inclusiveEnds">True if range end is inclusive</param>
        /// <returns>Collection of removed items</returns>
        public virtual ICollection<T> RemoveInRange(ITextRange range, bool inclusiveEnds)
        {
            var removed = _emptyList;

            var first = GetFirstItemAfterPosition(range.Start);

            if (first < 0 || (!inclusiveEnds && this[first].Start >= range.End) || (inclusiveEnds && this[first].Start > range.End))
            {
                return removed;
            }

            var lastCandidate = GetLastItemBeforeOrAtPosition(range.End);
            var last = -1;

            if (lastCandidate < first)
            {
                lastCandidate = first;
            }

            if (!inclusiveEnds && first >= 0)
            {
                for (var i = lastCandidate; i >= first; i--)
                {
                    var item = this._items[i];

                    if (item.Start < range.End)
                    {
                        last = i;
                        break;
                    }
                }
            }
            else
            {
                last = lastCandidate;
            }

            if (first >= 0 && last >= 0)
            {
                if (removed == _emptyList)
                {
                    removed = new List<T>();
                }

                for (var i = first; i <= last; i++)
                {
                    removed.Add(this._items[i]);
                }

                this._items.RemoveRange(first, last - first + 1);
            }

            return removed;
        }

        /// <summary>
        /// Reflects multiple changes in text to the collection
        /// Items are expanded and/or shifted according to the changes
        /// passed. If change affects more than one range then affected items are removed
        /// </summary>
        /// <param name="changes">Collection of changes. Must be non-overlapping and sorted by position.</param>
        /// <param name="startInclusive">True if insertion at range start falls inside the range rather than outside.</param>
        /// <returns>Collection or removed blocks</returns>
        public ICollection<T> ReflectTextChange(IEnumerable<TextChangeEventArgs> changes, bool startInclusive = false)
        {
            var list = new List<T>();

            foreach (var change in changes)
            {
                var removed = ReflectTextChange(change.Start, change.OldLength, change.NewLength, startInclusive);
                list.AddRange(removed);
            }

            return list;
        }

        /// <summary>
        /// Reflects changes in text to the collection. Items are expanded and/or
        /// shifted according to the change. If change affects more than one
        /// range then affected items are removed.
        /// </summary>
        /// <param name="start">Starting position of the change.</param>
        /// <param name="oldLength">Length of the changed fragment before the change.</param>
        /// <param name="newLength">Length of text fragment after the change.</param>
        /// <returns>Collection or removed blocks</returns>
        public ICollection<T> ReflectTextChange(int start, int oldLength, int newLength)
        {
            return ReflectTextChange(start, oldLength, newLength, false);
        }

        /// <summary>
        /// Reflects changes in text to the collection. Items are expanded and/or
        /// shifted according to the change. If change affects more than one
        /// range then affected items are removed.
        /// </summary>
        /// <param name="start">Starting position of the change.</param>
        /// <param name="oldLength">Length of the changed fragment before the change.</param>
        /// <param name="newLength">Length of text fragment after the change.</param>
        /// <param name="startInclusive">True if insertion at range start falls inside the range rather than outside.</param>
        /// <returns>Collection or removed blocks</returns>
        public virtual ICollection<T> ReflectTextChange(int start, int oldLength, int newLength, bool startInclusive)
        {
            var indexStart = GetItemContaining(start);
            var indexEnd = GetItemContaining(start + oldLength);
            ICollection<T> removed = _emptyList;

            // Make sure that end of the deleted range is not simply touching start 
            // of an existing range since deleting span that is touching an existing
            // range does not invalidate the existing range: |__r1__|deleted|__r2__|

            if (indexEnd > indexStart && indexStart >= 0)
            {
                if (this[indexEnd].Start == start + oldLength)
                {
                    indexEnd--;
                }
            }

            if (indexStart != indexEnd || (indexStart < 0 && indexEnd < 0))
            {
                removed = RemoveInRange(new TextRange(start, oldLength));
            }

            if (this.Count > 0)
            {
                var offset = newLength - oldLength;

                if (removed != _emptyList && removed.Count > 0)
                {
                    indexStart = GetItemContaining(start);
                }

                if (indexStart >= 0)
                {
                    // If range length is 0 it still contains the position.
                    // Don't try and shrink zero length ranges and instead
                    // shift them.
                    var range = this[indexStart];

                    if (range.Length == 0 && offset < 0)
                    {
                        range.Shift(offset);
                    }
                    else if (!startInclusive && oldLength == 0 && start == range.Start)
                    {
                        // range.Contains(start) is true but we don't want to expand
                        // the range if change is actually an insert right before
                        // the existing range like in {some text inserted}|__r1__|
                        range.Shift(offset);
                    }
                    else
                    {
                        // In Razor ranges may have end-inclusive set which
                        // may cause us to try and shrink zero-length ranges.
                        if (range.Length > 0 || offset > 0)
                        {
                            // In the case when range is end-inclusive as in Razor,
                            // and change is right at the end of the range, we may end up 
                            // trying to shrink range that is really must be deleted.
                            // If offset is bigger than the range length, delete it instead.

                            if ((range is IExpandableTextRange) && (range.Length + offset >= 0))
                            {
                                var expandable = range as IExpandableTextRange;
                                expandable.Expand(0, offset);
                            }
                            else if (range is ICompositeTextRange)
                            {
                                var composite = range as ICompositeTextRange;
                                composite.ShiftStartingFrom(start, offset);
                            }
                            else
                            {
                                RemoveAt(indexStart);
                                indexStart--;

                                if (removed == _emptyList)
                                {
                                    removed = new List<T>();
                                }

                                removed.Add(range);
                            }
                        }
                    }

                    for (var i = indexStart + 1; i < this.Count; i++)
                    {
                        this[i].Shift(offset);
                    }
                }
                else
                {
                    ShiftStartingFrom(start, offset);
                }
            }

            return removed;
        }

        public bool IsEqual(IEnumerable<T> other)
        {
            var otherCount = 0;

            foreach (var item in other)
            {
                otherCount++;
            }

            if (this.Count != otherCount)
            {
                return false;
            }

            var i = 0;
            foreach (var item in other)
            {
                if (this[i].Start != item.Start)
                {
                    return false;
                }

                if (this[i].Length != item.Length)
                {
                    return false;
                }

                i++;
            }

            return true;
        }

        public virtual void RemoveAt(int index)
        {
            this.Items.RemoveAt(index);
        }

        public virtual void RemoveRange(int startIndex, int count)
        {
            this._items.RemoveRange(startIndex, count);
        }

        public virtual void Clear()
        {
            this._items.Clear();
        }

        public virtual void ReplaceAt(int index, T newItem)
        {
            this._items[index] = newItem;
        }

        /// <summary>
        /// Sorts comment collection by token start positions.
        /// </summary>
        public void Sort()
        {
            this._items.Sort(new RangeItemComparer());
        }
        #endregion

        /// <summary>
        /// Returns collection of items in an array
        /// </summary>
        public T[] ToArray()
        {
            return this._items.ToArray();
        }

        /// <summary>
        /// Returns collection of items in a list
        /// </summary>
        public IList<T> ToList()
        {
            var list = new List<T>();

            list.AddRange(this._items);
            return list;
        }

        /// <summary>
        /// Compares two collections and calculates 'changed' range. In case this collection
        /// or comparand are empty, uses lowerBound and upperBound values as range
        /// delimiters. Typically lowerBound is 0 and upperBound is lentgh of the file.
        /// </summary>
        /// <param name="otherCollection">Collection to compare to</param>
        public virtual ITextRange RangeDifference(IEnumerable<ITextRange> otherCollection, int lowerBound, int upperBound)
        {
            if (otherCollection == null)
            {
                return TextRange.FromBounds(lowerBound, upperBound);
            }

            var other = new TextRangeCollection<ITextRange>(otherCollection);

            if (this.Count == 0 && other.Count == 0)
            {
                return TextRange.EmptyRange;
            }

            if (this.Count == 0)
            {
                return TextRange.FromBounds(lowerBound, upperBound);
            }

            if (other.Count == 0)
            {
                return TextRange.FromBounds(lowerBound, upperBound);
            }

            var minCount = Math.Min(this.Count, other.Count);
            var start = 0;
            var end = 0;
            int i, j;

            for (i = 0; i < minCount; i++)
            {
                start = Math.Min(this[i].Start, other[i].Start);

                if (this[i].Start != other[i].Start || this[i].Length != other[i].Length)
                {
                    break;
                }
            }

            if (i == minCount)
            {
                if (this.Count == other.Count)
                {
                    return TextRange.EmptyRange;
                }

                if (this.Count > other.Count)
                {
                    return TextRange.FromBounds(Math.Min(upperBound, other[minCount - 1].Start), upperBound);
                }
                else
                {
                    return TextRange.FromBounds(Math.Min(this[minCount - 1].Start, upperBound), upperBound);
                }
            }

            for (i = this.Count - 1, j = other.Count - 1; i >= 0 && j >= 0; i--, j--)
            {
                end = Math.Max(this[i].End, other[j].End);

                if (this[i].Start != other[j].Start || this[i].Length != other[j].Length)
                {
                    break;
                }
            }

            if (start < end)
            {
                return TextRange.FromBounds(start, end);
            }

            return TextRange.FromBounds(lowerBound, upperBound);
        }

        /// <summary>
        /// Merges another collection into existing one. Only adds elements
        /// that are not present in this collection. Both collections must
        /// be sorted by position for the method to work properly.
        /// </summary>
        /// <param name="other"></param>
        public void Merge(TextRangeCollection<T> other)
        {
            var i = 0;
            var j = 0;
            var count = this.Count;

            while (true)
            {
                if (i > count - 1)
                {
                    // Add elements remaining in the other collection
                    for (; j < other.Count; j++)
                    {
                        this.Add(other[j]);
                    }

                    break;
                }

                if (j > other.Count - 1)
                {
                    break;
                }

                if (this[i].Start < other[j].Start)
                {
                    i++;
                }
                else if (other[j].Start < this[i].Start)
                {
                    this.Add(other[j++]);
                }
                else
                {
                    // Element is already in the collection
                    j++;
                }
            }

            this.Sort();
        }

        private class RangeItemComparer : IComparer<T>
        {
            #region IComparer<T> Members
            public int Compare(T x, T y)
            {
                return x.Start - y.Start;
            }
            #endregion
        }

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._items.GetEnumerator();
        }
        #endregion
    }
}
