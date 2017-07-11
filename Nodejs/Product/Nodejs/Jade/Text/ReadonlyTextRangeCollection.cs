// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.NodejsTools.Jade
{
    internal class ReadOnlyTextRangeCollection<T> : IEnumerable<T>, IEnumerable where T : ITextRange
    {
        private TextRangeCollection<T> _collection;

        public ReadOnlyTextRangeCollection(TextRangeCollection<T> collection)
        {
            this._collection = collection;
        }

        public int Start => this._collection.Start; public int End => this._collection.End;
        public int Length => this._collection.Length;
        public bool Contains(int position) { return this._collection.Contains(position); }

        public IList<T> ItemsInRange(ITextRange range)
        {
            return this._collection.ItemsInRange(range);
        }

        public IList<T> ItemsInRange(int start)
        {
            return this._collection.ItemsInRange(TextRange.FromBounds(start, start));
        }

        public IList<int> GetItemsContainingInclusiveEnd(int position)
        {
            return this._collection.GetItemsContainingInclusiveEnd(position);
        }

        public int Count => this._collection.Count;
        /// <summary>
        /// Sorted list of comment tokens in the document.
        /// A new readonly collection is generated on every call to this method
        /// </summary>
        public ReadOnlyCollection<T> Items => new ReadOnlyCollection<T>(this._collection.Items);
        /// <summary>
        /// Retrieves Nth item in the collection
        /// </summary>
        public T this[int index] { get { return this._collection[index]; } }

        /// <summary>
        /// Returns index of item that starts at the given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public int GetItemAtPosition(int position)
        {
            return this._collection.GetItemAtPosition(position);
        }

        public virtual int GetItemContaining(int position)
        {
            return this._collection.GetItemContaining(position);
        }

        public virtual int GetFirstItemBeforePosition(int position)
        {
            return this._collection.GetFirstItemBeforePosition(position);
        }

        public virtual int GetFirstItemAfterPosition(int position)
        {
            return this._collection.GetFirstItemAfterPosition(position);
        }

        public T[] ToArray()
        {
            return this._collection.ToArray();
        }

        //class RangeItemComparer : IComparer<T>
        //{
        //    #region IComparer<T> Members
        //    public int Compare(T x, T y)
        //    {
        //        return x.Start - y.Start;
        //    }
        //    #endregion
        //}

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }
        #endregion
    }
}
