// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Microsoft.VisualStudioTools.Project
{
    internal sealed class HierarchyIdMap
    {
        private readonly ConcurrentDictionary<int, WeakReference<HierarchyNode>> nodes = new ConcurrentDictionary<int, WeakReference<HierarchyNode>>();
        private readonly ConcurrentStack<int> freedIds = new ConcurrentStack<int>();

        /// <summary>
        /// Must be called from the UI thread
        /// </summary>
        public uint Add(HierarchyNode node)
        {
            Debug.Assert(VisualStudio.Shell.ThreadHelper.CheckAccess());

#if DEBUG
            foreach (var reference in this.nodes.Values)
            {
                if (reference != null)
                {
                    if (reference.TryGetTarget(out var item))
                    {
                        Debug.Assert(node != item);
                    }
                }
            }

#endif
            if (!this.freedIds.TryPop(out var idx))
            {
                idx = this.nodes.Count;
            }

            var addSuccess = this.nodes.TryAdd(idx, new WeakReference<HierarchyNode>(node));
            Debug.Assert(addSuccess, "Failed to add a new item");

            return (uint)idx + 1;
        }

        /// <summary>
        /// Must be called from the UI thread
        /// </summary>
        public void Remove(HierarchyNode node)
        {
            Debug.Assert(VisualStudio.Shell.ThreadHelper.CheckAccess());

            Debug.Assert(node != null, "Called with null node");

            var idx = (int)node.ID - 1;

            var removeCheck = this.nodes.TryRemove(idx, out var weakRef);

            Debug.Assert(removeCheck, "How did we get an id, which we haven't seen before");
            Debug.Assert(weakRef != null, "Double delete is not expected.");
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
                Debug.Assert(VisualStudio.Shell.ThreadHelper.CheckAccess());

                var idx = (int)itemId - 1;
                if (0 <= idx && idx < this.nodes.Count)
                {
                    if (this.nodes.TryGetValue(idx, out var reference) && reference != null && reference.TryGetTarget(out var node))
                    {
                        Debug.Assert(node != null);
                        return node;
                    }
                }

                // This is a valid return value, this gets called by VS after we deleted the item
                return null;
            }
        }
    }
}
