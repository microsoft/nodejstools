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
            _collection = collection;
        }

        public int Start { get { return _collection.Start; } }
        public int End { get { return _collection.End; } }

        public int Length { get { return _collection.Length; } }

        public bool Contains(int position) { return _collection.Contains(position); }

        public IList<T> ItemsInRange(ITextRange range)
        {
            return _collection.ItemsInRange(range);
        }

        public IList<T> ItemsInRange(int start)
        {
            return _collection.ItemsInRange(TextRange.FromBounds(start, start));
        }

        public IList<int> GetItemsContainingInclusiveEnd(int position)
        {
            return _collection.GetItemsContainingInclusiveEnd(position);
        }

        public int Count { get { return _collection.Count; } }

        /// <summary>
        /// Sorted list of comment tokens in the document.
        /// A new readonly collection is generated on every call to this method
        /// </summary>
        public ReadOnlyCollection<T> Items { get { return new ReadOnlyCollection<T>(_collection.Items); } }

        /// <summary>
        /// Retrieves Nth item in the collection
        /// </summary>
        public T this[int index] { get { return _collection[index]; } }

        /// <summary>
        /// Returns index of item that starts at the given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public int GetItemAtPosition(int position)
        {
            return _collection.GetItemAtPosition(position);
        }

        public virtual int GetItemContaining(int position)
        {
            return _collection.GetItemContaining(position);
        }

        public virtual int GetFirstItemBeforePosition(int position)
        {
            return _collection.GetFirstItemBeforePosition(position);
        }

        public virtual int GetFirstItemAfterPosition(int position)
        {
            return _collection.GetFirstItemAfterPosition(position);
        }

        public T[] ToArray()
        {
            return _collection.ToArray();
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
            return _collection.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
        #endregion
    }
}
