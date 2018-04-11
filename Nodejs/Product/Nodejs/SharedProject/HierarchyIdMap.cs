// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.VisualStudioTools.Project
{
    internal sealed class HierarchyIdMap
    {
        private readonly List<WeakReference<HierarchyNode>> ids = new List<WeakReference<HierarchyNode>>();
        private readonly Stack<int> freedIds = new Stack<int>();

        private readonly object theLock = new object();

        /// <summary>
        /// Must be called from the UI thread
        /// </summary>
        public uint Add(HierarchyNode node)
        {
#if DEBUG
            foreach (var reference in this.ids)
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

            VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            lock (this.theLock)
            {
                if (this.freedIds.Count > 0)
                {
                    var i = this.freedIds.Pop();
                    this.ids[i] = new WeakReference<HierarchyNode>(node);
                    return (uint)i + 1;
                }
                else
                {
                    this.ids.Add(new WeakReference<HierarchyNode>(node));
                    // ids are 1 based
                    return (uint)this.ids.Count;
                }
            }
        }

        /// <summary>
        /// Must be called from the UI thread
        /// </summary>
        public void Remove(HierarchyNode node)
        {
            VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            lock (this.theLock)
            {
                var i = (int)node.ID - 1;
                if (0 > i || i >= this.ids.Count)
                {
                    throw new InvalidOperationException($"Invalid id. {i}");
                }

                var weakRef = this.ids[i];
                if (weakRef == null)
                {
                    throw new InvalidOperationException("Trying to retrieve a node before adding.");
                }

                if (weakRef.TryGetTarget(out var found) && !object.ReferenceEquals(node, found))
                {
                    throw new InvalidOperationException("The node has the wrong id.");
                }

                this.ids[i] = null;
                this.freedIds.Push(i);
            }
        }

        /// <summary>
        /// Must be called from the UI thread
        /// </summary>
        public HierarchyNode this[uint itemId]
        {
            get
            {
                var i = (int)itemId - 1;
                if (0 <= i && i < this.ids.Count)
                {
                    var reference = this.ids[i];
                    if (reference != null && reference.TryGetTarget(out var node))
                    {
                        return node;
                    }
                }
                return null;
            }
        }
    }
}
