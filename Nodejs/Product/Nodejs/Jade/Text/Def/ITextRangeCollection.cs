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


using System.Collections.Generic;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// Represents collection of ITextRange items
    /// </summary>
    interface ITextRangeCollection<T> : ICompositeTextRange, IEnumerable<T> {
        /// <summary>
        /// Number of items in collection
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Retrieves Nth item in the collection
        /// </summary>
        T this[int index] { get; }

        /// <summary>
        /// Adds comment to collection unless it is already there.
        /// </summary>
        /// <param name="commentToken">Comment token to add</param>
        void Add(T item);

        /// <summary>
        /// Add a set of items to the collection
        /// </summary>
        /// <param name="items">Items to add</param>
        void Add(IEnumerable<T> items);

        /// <summary>
        /// Returns index of item that starts at the given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        int GetItemAtPosition(int position);

        /// <summary>
        /// Returns index of items that contains given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        int GetItemContaining(int position);

        /// <summary>
        /// Removes all items from collection
        /// </summary>
        void Clear();

        /// <summary>
        /// Sorts ranges in collection by start position.
        /// </summary>
        void Sort();
    }
}
