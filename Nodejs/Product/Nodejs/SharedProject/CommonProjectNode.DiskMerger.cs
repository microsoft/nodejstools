// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
            private readonly ConcurrentStack<(string Name, HierarchyNode Parent)> remainingDirs = new ConcurrentStack<(string, HierarchyNode)>();
            private readonly CommonProjectNode project;

            public DiskMerger(CommonProjectNode project, HierarchyNode parent, string dir)
            {
                this.project = project;
                this.remainingDirs.Push((dir, parent));
            }

            /// <summary>
            /// Continues processing the merge request, performing a portion of the full merge possibly
            /// returning before the merge has completed.
            /// 
            /// Returns true if the merge needs to continue, or false if the merge has completed.
            /// </summary>
            public async Task<bool> ContinueMergeAsync(bool hierarchyCreated)
            {
                if (this.remainingDirs.Count == 0)
                {
                    // all done
                    await this.InvokeOnUIThread(this.project.BoldStartupItem);
                    return false;
                }

                if (!this.remainingDirs.TryPop(out var dir) || !Directory.Exists(dir.Name))
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

                        var existing = this.project.FindNodeByFullPath(curDir);
                        if (existing == null)
                        {
                            existing = await this.InvokeOnUIThread(() => this.project.AddAllFilesFolder(dir.Parent, curDir + Path.DirectorySeparatorChar, hierarchyCreated));
                        }
                        else
                        {
                            missingChildren.Remove(existing);
                        }
                        this.remainingDirs.Push((curDir, existing));
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

                        // todo: batch files
                        var existing = this.project.FindNodeByFullPath(file);
                        if (existing == null)
                        {
                            existing = await this.InvokeOnUIThread(() => this.project.AddAllFilesFile(dir.Parent, file));
                        }
                        else
                        {
                            missingChildren.Remove(existing);
                        }
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
                        await this.InvokeOnUIThread(() => this.project.RemoveSubTree(child));
                    }
                }

                if (hierarchyCreated)
                {
                    dir.Parent.ExpandItem(wasExpanded ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder);
                }

                return true;
            }

            private Task InvokeOnUIThread(Action action)
            {
                return this.project.Site.GetUIThread().InvokeAsync(action);
            }

            private Task<T> InvokeOnUIThread<T>(Func<T> func)
            {
                return this.project.Site.GetUIThread().InvokeAsync(func);
            }
        }
    }
}
