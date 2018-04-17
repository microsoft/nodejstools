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
            VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

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
            this.nodes[idx] = new WeakReference<HierarchyNode>(node);

            return (uint)idx + 1;
        }

        /// <summary>
        /// Must be called from the UI thread
        /// </summary>
        public void Remove(HierarchyNode node)
        {
            VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            Debug.Assert(node != null, "Called with null node");

            var idx = (int)node.ID - 1;

            var removeCheck = this.nodes.TryGetValue(idx, out var weakRef);

            Debug.Assert(removeCheck, "How did we get an id, which we haven't seen before");
            Debug.Assert(weakRef != null, "Double delete is not expected.");

            var checkReference = weakRef.TryGetTarget(out var found) && object.ReferenceEquals(node, found);

            Debug.Assert(checkReference, "The node has the wrong id.");

            this.nodes[idx] = null;
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

                var i = (int)itemId - 1;
                if (0 <= i && i < this.nodes.Count)
                {
                    var reference = this.nodes[i];
                    if (reference != null && reference.TryGetTarget(out var node))
                    {
                        Debug.Assert(node != null);
                        return node;
                    }
                }
                return null;
            }
        }
    }
}
