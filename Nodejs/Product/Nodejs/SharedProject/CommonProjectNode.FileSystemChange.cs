// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    internal abstract partial class CommonProjectNode
    {
        /// <summary>
        /// Represents an individual change to the file system.  We process these in bulk on the
        /// UI thread.
        /// </summary>
        [DebuggerDisplay("FileSystemChange: {Type} {path}")]
        private sealed class FileSystemChange
        {
            private readonly CommonProjectNode project;
            private readonly string path;

            public readonly WatcherChangeTypes Type;

            public FileSystemChange(CommonProjectNode project, WatcherChangeTypes changeType, string path)
            {
                this.project = project;
                this.Type = changeType;
                this.path = path;
            }

            private void RedrawIcon(HierarchyNode node)
            {
                this.project.ReDrawNode(node, UIHierarchyElement.Icon);

                for (var child = node.FirstChild; child != null; child = child.NextSibling)
                {
                    this.RedrawIcon(child);
                }
            }

            public async Task ProcessChangeAsync()
            {
                this.project.Site.GetUIThread().MustBeCalledFromUIThread();

                var child = this.project.FindNodeByFullPath(this.path);
                if ((this.Type == WatcherChangeTypes.Deleted || this.Type == WatcherChangeTypes.Changed) && child == null)
                {
                    child = this.project.FindNodeByFullPath(this.path + Path.DirectorySeparatorChar);
                }

                switch (this.Type)
                {
                    case WatcherChangeTypes.Deleted:
                        if (child != null)
                        {
                            ChildDeleted(child);
                        }
                        break;
                    case WatcherChangeTypes.Created:
                        if (child != null)
                        {
                            // this child already exists redraw the icon
                            this.project.ReDrawNode(child, UIHierarchyElement.Icon);
                        }
                        else
                        {
                            await ChildCreatedAsync(child);
                        }
                        break;
                    case WatcherChangeTypes.Changed:
                        // we only care about the attributes
                        if (this.project.IsFileHidden(this.path))
                        {
                            if (child != null)
                            {
                                // attributes must have changed to hidden, remove the file
                                ChildDeleted(child);
                            }
                        }
                        else
                        {
                            if (child == null)
                            {
                                // attributes must of changed from hidden, add the file
                                await ChildCreatedAsync(null);
                            }
                        }
                        break;
                }
            }

            private void RemoveAllFilesChildren(HierarchyNode parent)
            {
                for (var current = parent.FirstChild; current != null; current = current.NextSibling)
                {
                    // remove our children first
                    this.RemoveAllFilesChildren(current);

                    this.project.TryDeactivateSymLinkWatcher(current);

                    // then remove us if we're an all files node
                    if (current.ItemNode is AllFilesProjectElement)
                    {
                        this.project.OnItemDeleted(current);
                        parent.RemoveChild(current);
                    }
                }
            }

            private void ChildDeleted(HierarchyNode child)
            {
                if (child == null)
                {
                    throw new InvalidOperationException("Deleted, but child is null.");
                }

                this.project.TryDeactivateSymLinkWatcher(child);

                // rapid changes can arrive out of order, if the file or directory 
                // actually exists ignore the event.
                if ((!File.Exists(child.Url) && !Directory.Exists(child.Url)) ||
                    this.project.IsFileHidden(child.Url))
                {
                    if (child.ItemNode == null)
                    {
                        // nodes should all have ItemNodes, the project is special.
                        Debug.Assert(child is ProjectNode);
                        return;
                    }

                    if (child.ItemNode.IsExcluded)
                    {
                        this.RemoveAllFilesChildren(child);
                        // deleting a show all files item, remove the node.
                        this.project.OnItemDeleted(child);
                        child.Parent.RemoveChild(child);
                        child.Close();
                    }
                    else
                    {
                        Debug.Assert(!child.IsNonMemberItem);
                        // deleting an item in the project, fix the icon, also
                        // fix the icon of all children which we may have not
                        // received delete notifications for
                        this.RedrawIcon(child);
                    }
                }
            }

            private async Task ChildCreatedAsync(HierarchyNode child)
            {
                if (this.project.IsFileHidden(this.path))
                {
                    // don't add hidden files/folders
                    return;
                }

                // creating a new item, need to create the on all files node
                var parent = this.project.GetParentFolderForPath(this.path);

                if (parent == null)
                {
                    // we've hit an error while adding too many files, the file system watcher
                    // couldn't keep up.  That's alright, we'll merge the files in correctly 
                    // in a little while...
                    return;
                }

                var wasExpanded = parent.GetIsExpanded();

                if (Directory.Exists(this.path))
                {
                    if (IsFileSymLink(this.path))
                    {
                        var parentDir = CommonUtils.GetParent(this.path);
                        if (IsRecursiveSymLink(parentDir, this.path))
                        {
                            // don't add recusrive sym link directory
                            return;
                        }

                        // otherwise we're going to need a new file system watcher
                        this.project.CreateSymLinkWatcher(this.path);
                    }

                    var folderNode = this.project.AddAllFilesFolder(parent, this.path);
                    var folderNodeWasExpanded = folderNode.GetIsExpanded();

                    // then add the folder nodes
                    await this.project.MergeDiskNodesAsync(folderNode, this.path).ConfigureAwait(true);

                    // Assert we're back on the UI thread
                    this.project.Site.GetUIThread().MustBeCalledFromUIThread();

                    this.project.OnInvalidateItems(folderNode);

                    folderNode.ExpandItem(folderNodeWasExpanded ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder);
                }
                else if (File.Exists(this.path))
                {
                    // rapid changes can arrive out of order, make sure the file still exists
                    this.project.AddAllFilesFile(parent, this.path);
                    if (StringComparer.OrdinalIgnoreCase.Equals(this.project.GetStartupFile(), this.path))
                    {
                        this.project.BoldStartupItem();
                    }
                }

                parent.ExpandItem(wasExpanded ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder);
            }
        }
    }
}
