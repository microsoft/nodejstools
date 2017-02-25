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
                    HierarchyNode item;
                    if (reference.TryGetTarget(out item))
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
            int i = (int)node.ID - 1;
            HierarchyNode found;
            if (i < 0 ||
                i >= this._ids.Count ||
                (this._ids[i].TryGetTarget(out found) && !object.ReferenceEquals(node, found)))
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
                int i = (int)itemId - 1;
                if (0 <= i && i < this._ids.Count)
                {
                    var reference = this._ids[i];
                    HierarchyNode node;
                    if (reference != null && reference.TryGetTarget(out node))
                    {
                        return node;
                    }
                }
                return null;
            }
        }
    }
}
