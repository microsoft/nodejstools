// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.VisualStudioTools.Project
{
    internal sealed class HierarchyIdMap
    {
        private readonly List<WeakReference<HierarchyNode>> _ids = new List<WeakReference<HierarchyNode>>();
        private readonly Stack<int> _freedIds = new Stack<int>();

        /// <summary>
        /// Must be called from the UI thread
        /// </summary>
        public uint Add(HierarchyNode node)
        {
#if DEBUG
            foreach (var reference in this._ids)
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
            if (this._freedIds.Count > 0)
            {
                var i = this._freedIds.Pop();
                this._ids[i] = new WeakReference<HierarchyNode>(node);
                return (uint)i + 1;
            }
            else
            {
                this._ids.Add(new WeakReference<HierarchyNode>(node));
                // ids are 1 based
                return (uint)this._ids.Count;
            }
        }

        /// <summary>
        /// Must be called from the UI thread
        /// </summary>
        public void Remove(HierarchyNode node)
        {
            var i = (int)node.ID - 1;
            if (i < 0 ||
                i >= this._ids.Count ||
                (this._ids[i].TryGetTarget(out var found) && !object.ReferenceEquals(node, found)))
            {
                throw new InvalidOperationException("Removing node with invalid ID or map is corrupted");
            }

            this._ids[i] = null;
            this._freedIds.Push(i);
        }

        /// <summary>
        /// Must be called from the UI thread
        /// </summary>
        public HierarchyNode this[uint itemId]
        {
            get
            {
                var i = (int)itemId - 1;
                if (0 <= i && i < this._ids.Count)
                {
                    var reference = this._ids[i];
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
