// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Microsoft.VisualStudioTools.Project
{
    internal sealed class HierarchyIdMap
    {
        private readonly ConcurrentDictionary<uint, WeakReference<HierarchyNode>> nodes = new ConcurrentDictionary<uint, WeakReference<HierarchyNode>>();
        private readonly ConcurrentStack<uint> freedIds = new ConcurrentStack<uint>();

        public HierarchyIdMap()
        {
        }

        /// <summary>
        /// Must be called from the UI thread
        /// </summary>
        public uint Add(HierarchyNode node)
        {
            VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            Debug.Assert(node != null, "The node added here should never be null.");
#if DEBUG
            foreach (var kv in this.nodes)
            {
                Debug.Assert(kv.Value.TryGetTarget(out var item), "should not be GC-ed before we remove");
                Debug.Assert(kv.Key == item.ID, "the key should match the id of the node");
                Debug.Assert(node != item, "don't double insert");
            }
#endif
            if (!this.freedIds.TryPop(out var idx))
            {
                // +1 since 0 is not a valid HierarchyId
                idx = (uint)this.nodes.Count + 1;
            }

            var addSuccess = this.nodes.TryAdd(idx, new WeakReference<HierarchyNode>(node));
            Debug.Assert(addSuccess, "Failed to add a new item");

            return idx;
        }

        /// <summary>
        /// Must be called from the UI thread
        /// </summary>
        public void Remove(HierarchyNode node)
        {
            VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            Debug.Assert(node != null, "Called with null node");

            var idx = node.ID;

            var removeCheck = this.nodes.TryRemove(idx, out var weakRef);

            Debug.Assert(removeCheck, "How did we get an id, which we haven't seen before");
            Debug.Assert(weakRef != null, "How did we insert a null value.");
            Debug.Assert(weakRef.TryGetTarget(out var found) && object.ReferenceEquals(node, found), "The node has the wrong id, or was GC-ed before.");

            this.freedIds.Push(idx);
        }

        /// <summary>
        /// Must be called from the UI thread
        /// </summary>
        public HierarchyNode this[uint itemId]
        {
            get
            {
                VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

                if (this.nodes.TryGetValue(itemId, out var reference) && reference.TryGetTarget(out var node))
                {
                    Debug.Assert(node != null);
                    return node;
                }

                // This is a valid return value, this gets called by VS after we deleted the item
                return null;
            }
        }
    }
}
