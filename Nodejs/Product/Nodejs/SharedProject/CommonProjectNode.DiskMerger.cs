// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    internal abstract partial class CommonProjectNode
    {
        /// <summary>
        /// Performs merging of the file system state with the current project hierarchy, bringing them
        /// back into sync.
        /// 
        /// The class can be created, and ContinueMerge should be called until it returns false, at which
        /// point the file system has been merged.  
        /// 
        /// You can wait between calls to ContinueMerge to enable not blocking the UI.
        /// 
        /// If there were changes which came in while the DiskMerger was processing then those changes will still need
        /// to be processed after the DiskMerger completes.
        /// </summary>
        private sealed class DiskMerger
        {
            private readonly Stack<DirState> remainingDirs = new Stack<DirState>();
            private readonly CommonProjectNode project;

            public DiskMerger(CommonProjectNode project, HierarchyNode parent, string dir)
            {
                this.project = project;
                this.remainingDirs.Push(new DirState(dir, parent));
            }

            /// <summary>
            /// Continues processing the merge request, performing a portion of the full merge possibly
            /// returning before the merge has completed.
            /// 
            /// Returns true if the merge needs to continue, or false if the merge has completed.
            /// </summary>
            public bool ContinueMerge(bool hierarchyCreated)
            {
                if (this.remainingDirs.Count == 0)
                {
                    // all done
                    this.project.BoldStartupItem();
                    return false;
                }

                var dir = this.remainingDirs.Pop();
                if (!Directory.Exists(dir.Name))
                {
                    return true;
                }

                var wasExpanded = hierarchyCreated ? dir.Parent.GetIsExpanded() : false;
                var missingChildren = new HashSet<HierarchyNode>(dir.Parent.AllChildren);
                try
                {
                    foreach (var curDir in Directory.EnumerateDirectories(dir.Name))
                    {
                        if (this.project.IsFileHidden(curDir))
                        {
                            continue;
                        }
                        if (IsFileSymLink(curDir))
                        {
                            if (IsRecursiveSymLink(dir.Name, curDir))
                            {
                                // don't add recursive sym links
                                continue;
                            }

                            // track symlinks, we won't get events on the directory
                            this.project.CreateSymLinkWatcher(curDir);
                        }

                        var existing = this.project.AddAllFilesFolder(dir.Parent, curDir + Path.DirectorySeparatorChar, hierarchyCreated);
                        missingChildren.Remove(existing);
                        this.remainingDirs.Push(new DirState(curDir, existing));
                    }
                }
                catch
                {
                    // directory was deleted, we don't have access, etc...
                    return true;
                }

                try
                {
                    foreach (var file in Directory.EnumerateFiles(dir.Name))
                    {
                        if (this.project.IsFileHidden(file))
                        {
                            continue;
                        }
                        missingChildren.Remove(this.project.AddAllFilesFile(dir.Parent, file));
                    }
                }
                catch
                {
                    // directory was deleted, we don't have access, etc...

                    // We are about to return and some of the previous operations may have affect the Parent's Expanded
                    // state.  Set it back to what it was
                    if (hierarchyCreated)
                    {
                        dir.Parent.ExpandItem(wasExpanded ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder);
                    }
                    return true;
                }

                // remove the excluded children which are no longer there
                foreach (var child in missingChildren)
                {
                    if (child.ItemNode.IsExcluded)
                    {
                        this.project.RemoveSubTree(child);
                    }
                }

                if (hierarchyCreated)
                {
                    dir.Parent.ExpandItem(wasExpanded ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder);
                }

                return true;
            }

            private struct DirState
            {
                public readonly string Name;
                public readonly HierarchyNode Parent;

                public DirState(string name, HierarchyNode parent)
                {
                    this.Name = name;
                    this.Parent = parent;
                }
            }
        }
    }
}
