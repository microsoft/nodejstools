// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.VisualStudioTools
{
    internal static class ObservableCollectionExtensions
    {
        public static void Merge<T>(
            this ObservableCollection<T> left,
            IEnumerable<T> right,
            IEqualityComparer<T> compareId,
            IComparer<T> compareSortKey
        )
        {
            var toAdd = new SortedList<T, T>(compareSortKey);
            var toRemove = new Dictionary<T, int>(compareId);
            var alsoRemove = new List<int>();
            var index = 0;
            foreach (var item in left)
            {
                if (toRemove.ContainsKey(item))
                {
                    alsoRemove.Add(index);
                }
                else
                {
                    toRemove[item] = index;
                }
                index += 1;
            }

            foreach (var r in right.OrderBy(k => k, compareSortKey))
            {
                if (toRemove.TryGetValue(r, out index))
                {
                    toRemove.Remove(r);
                    left[index] = r;
                }
                else
                {
                    toAdd[r] = r;
                }
            }

            foreach (var removeAt in toRemove.Values.Concat(alsoRemove).OrderByDescending(i => i))
            {
                left.RemoveAt(removeAt);
            }

            index = 0;
            foreach (var item in toAdd.Values)
            {
                while (index < left.Count && compareSortKey.Compare(left[index], item) <= 0)
                {
                    index += 1;
                }
                left.Insert(index, item);
            }
        }
    }
}
