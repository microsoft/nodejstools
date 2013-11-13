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
using Microsoft.NodejsTools.Project;

namespace Microsoft.NodejsTools.Intellisense {
    /// <summary>
    /// Groups a file into a reference group balancing them across the available groups.
    /// 
    /// We would usually just write out each file on its own, but that breaks down and
    /// references start getting ignored for some reason...
    /// </summary>
    class ReferenceGroupDispenser {
        public readonly LinkedList<ReferenceGroup> Groups = new LinkedList<ReferenceGroup>();
        private const int _maxGroups = 100;

        public ReferenceGroup AddFile(NodejsFileNode file) {
            if (Groups.Count < _maxGroups) {
                var group = new ReferenceGroup();
                Groups.AddFirst(group);
                group.Included.Add(file);
                return group;
            } else {
                var group = Groups.First;
                group.Value.Included.Add(file);

                UpdatePosition(group);
                return group.Value;
            }
        }

        public void RemoveFile(NodejsFileNode file) {
            var node = Groups.Find(file._refGroup);
            file._refGroup.Included.Remove(file);
            UpdatePosition(node);
        }

        private void UpdatePosition(LinkedListNode<ReferenceGroup> group) {
            Groups.Remove(group);
            var insertBefore = Groups.First;
            while (insertBefore != null) {
                if (insertBefore.Value.Included.Count > group.Value.Included.Count) {
                    break;
                }
                insertBefore = insertBefore.Next;
            }
            if (insertBefore == null) {
                Groups.AddLast(group);
            } else {
                Groups.AddBefore(insertBefore, group);
            }
        }
    }
}
