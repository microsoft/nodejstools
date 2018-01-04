// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    internal abstract partial class CommonProjectNode
    {
        /// <summary>
        /// Performs merging of the file system state with the current project hierarchy, bringing them
        /// back into sync.
        /// 
        /// The worker thread runs continuously to sync the files on disk with the DiskNodes collection of 
        /// the project.
        /// 
        /// Callers can call the Wait method to blocking wait on the completion of the merge, i.e. till the collection 
        /// is empty.
        /// 
        /// Checks for IsDisposed should be done before calling any of the methods of the BlockingCollection, which could
        /// throw ObjectDisposed exceptions otherwise.
        /// </summary>
        private sealed class DiskMerger : IDisposable
        {
            private readonly BlockingCollection<DirState> dirs = new BlockingCollection<DirState>(new ConcurrentStack<DirState>());

            private readonly CommonProjectNode project;

            private readonly Thread worker;

            private readonly ManualResetEventSlim processingDirsEvent = new ManualResetEventSlim(initialState: true);

            private bool isDisposed = false;

            public DiskMerger(CommonProjectNode project)
            {
                this.project = project;

                this.worker = new Thread(this.Merge)
                {
                    Name = "NodeTools Project Disk Sync",
                    IsBackground = true,
                };
            }

            public void Start()
            {
                if (this.isDisposed || this.dirs.IsAddingCompleted)
                {
                    throw new ObjectDisposedException(nameof(DiskMerger));
                }

                if (!this.worker.IsAlive)
                {
                    this.worker.Start();
                }
            }

            public void AddDir(HierarchyNode parent, string folderPath)
            {
                if (this.isDisposed || this.dirs.IsAddingCompleted)
                {
                    throw new ObjectDisposedException(nameof(DiskMerger));
                }
                this.dirs.Add(new DirState(folderPath, parent));
            }

            public bool CompletedMerge => this.dirs.Count == 0;

            public void Wait()
            {
                if (!this.isDisposed)
                {
                    this.processingDirsEvent.Wait();
                }
            }

            private void Merge()
            {
                while (!this.isDisposed && !this.dirs.IsCompleted)
                {
                    if (this.dirs.TryTake(out var dir, Timeout.Infinite))
                    {
                        if (string.IsNullOrEmpty(dir.Path) || !Directory.Exists(dir.Path))
                        {
                            continue;
                        }

                        // Signal we're processing the dirs so anybody interested and calling Wait is blocked
                        this.processingDirsEvent.Reset();

                        var hierarchyCreated = this.project.ParentHierarchy != null;

                        var missingChildren = new HashSet<HierarchyNode>(dir.Parent.AllChildren);
                        try
                        {
                            foreach (var curDir in Directory.EnumerateDirectories(dir.Path))
                            {
                                if (this.project.IsFileHidden(curDir))
                                {
                                    continue;
                                }
                                if (IsFileSymLink(curDir))
                                {
                                    if (IsRecursiveSymLink(dir.Path, curDir))
                                    {
                                        // don't add recursive sym links
                                        continue;
                                    }

                                    // track symlinks, we won't get events on the directory
                                    this.project.CreateSymLinkWatcher(curDir);
                                }

                                var folderNode = this.project.FindNodeByFullPath(CommonUtils.EnsureEndSeparator(curDir));
                                if (folderNode == null)
                                {
                                    folderNode = this.InvokeOnUIThread(() => this.project.AddAllFilesFolder(dir.Parent, CommonUtils.EnsureEndSeparator(curDir), hierarchyCreated));
                                }
                                else
                                {
                                    missingChildren.Remove(folderNode);
                                }
                                this.dirs.Add(new DirState(curDir, folderNode));
                            }
                        }
                        catch (Exception exc) when (IsExpectedException(exc))
                        {
                            // directory was deleted, we don't have access, etc...
                        }

                        var wasExpanded = this.project.ParentHierarchy != null ? dir.Parent.GetIsExpanded() : false;
                        try
                        {
                            foreach (var file in Directory.EnumerateFiles(dir.Path))
                            {
                                if (this.project.IsFileHidden(file))
                                {
                                    continue;
                                }
                                var fileNode = this.project.FindNodeByFullPath(file);
                                if (fileNode == null)
                                {
                                    fileNode = this.InvokeOnUIThread(() => this.project.AddAllFilesFile(dir.Parent, file));
                                }
                                else
                                {
                                    missingChildren.Remove(fileNode);
                                }
                            }
                        }
                        catch (Exception exc) when (IsExpectedException(exc))
                        {
                            // directory was deleted, we don't have access, etc...

                            // We are about to return and some of the previous operations may have affect the Parent's Expanded
                            // state.  Set it back to what it was
                            if (hierarchyCreated)
                            {
                                dir.Parent.ExpandItem(wasExpanded ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder);
                            }
                        }

                        // remove the excluded children which are no longer there
                        foreach (var child in missingChildren)
                        {
                            if (child.ItemNode.IsExcluded)
                            {
                                this.InvokeOnUIThread(() => this.project.RemoveSubTree(child));
                            }
                        }

                        if (hierarchyCreated)
                        {
                            dir.Parent.ExpandItem(wasExpanded ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder);
                        }
                    }
                    if (!this.isDisposed && this.CompletedMerge)
                    {
                        InvokeOnUIThread(this.project.BoldStartupItem);
                        // raise completed event, and unblock anybody waiting
                        this.processingDirsEvent.Set();
                    }
                }

                bool IsExpectedException(Exception exc)
                {
                    return exc is UnauthorizedAccessException || exc is IOException;
                }
            }

            private void InvokeOnUIThread(Action action)
            {
                this.project.Site.GetUIThread().Invoke(action);
            }

            private T InvokeOnUIThread<T>(Func<T> func)
            {
                return this.project.Site.GetUIThread().Invoke(func);
            }

            public void Clear()
            {
                // BlockingCollection has no clear method
                // use TryTake, to prevent race condition with worker thread
                while (this.dirs.TryTake(out var _))
                {
                }
            }

            public void Dispose()
            {
                if (!this.isDisposed)
                {
                    this.dirs.CompleteAdding();
                    this.Clear();
                    this.dirs.Dispose();

                    this.processingDirsEvent.Set();
                    this.processingDirsEvent.Dispose();

                    this.isDisposed = true;
                }
            }

            private struct DirState
            {
                public readonly string Path;
                public readonly HierarchyNode Parent;

                public DirState(string path, HierarchyNode parent)
                {
                    this.Path = path;
                    this.Parent = parent;
                }
            }
        }
    }
}
