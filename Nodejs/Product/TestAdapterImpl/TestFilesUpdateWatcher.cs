// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.NodejsTools.Telemetry;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Microsoft.NodejsTools.TestAdapter
{
    // TODO: consider replacing this with Microsoft.VisualStudioTools.Project.FileChangeManager. (When we have an assembly we can use to share code)

    internal sealed class TestFilesUpdateWatcher : IVsFreeThreadedFileChangeEvents2, IDisposable
    {
        private readonly IDictionary<string, uint> watchedFiles = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, uint> watchedFolders = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);

        const int NOT_DISPOSED = 0;
        const int DISPOSED = 1;

        private int disposed = NOT_DISPOSED;
        private System.IServiceProvider serviceProvider;

        public event EventHandler<TestFileChangedEventArgs> FileChangedEvent;

        private /*readonly*/ Lazy<IVsFileChangeEx> fileWatcher => new Lazy<IVsFileChangeEx> (() => { return serviceProvider.GetService<IVsFileChangeEx>(typeof(SVsFileChangeEx)); }); // writeable for dispose

        public TestFilesUpdateWatcher(System.IServiceProvider serviceProvider)
        {
            ValidateArg.NotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
        }

        public bool AddFileWatch(string path)
        {
            ValidateArg.NotNull(path, nameof(path));
            this.CheckDisposed();

            const uint mask = (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Add | _VSFILECHANGEFLAGS.VSFILECHG_Del | _VSFILECHANGEFLAGS.VSFILECHG_Size | _VSFILECHANGEFLAGS.VSFILECHG_Time);

            if (!string.IsNullOrEmpty(path) && !this.watchedFiles.ContainsKey(path) && ErrorHandler.Succeeded(this.fileWatcher.Value.AdviseFileChange(path, mask, this, out uint cookie)))
            {
                this.watchedFiles.Add(path, cookie);
                return true;
            }
            return false;
        }

        public bool AddFolderWatch(string path)
        {
            ValidateArg.NotNull(path, nameof(path));
            this.CheckDisposed();

            if (!string.IsNullOrEmpty(path) && !this.watchedFolders.ContainsKey(path) && ErrorHandler.Succeeded(this.fileWatcher.Value.AdviseDirChange(path, VSConstants.S_OK, this, out uint cookie)))
            {
                this.watchedFolders.Add(path, cookie);
                return true;
            }
            return false;
        }

        public bool RemoveFileWatch(string path)
        {
            ValidateArg.NotNull(path, nameof(path));
            this.CheckDisposed();

            if (!string.IsNullOrEmpty(path) && this.watchedFiles.TryGetValue(path, out uint cookie))
            {
                this.watchedFiles.Remove(path);
                return ErrorHandler.Succeeded(this.fileWatcher.Value.UnadviseFileChange(cookie));
            }
            return false;
        }

        public bool RemoveFolderWatch(string path)
        {
            ValidateArg.NotNull(path, nameof(path));
            this.CheckDisposed();

            if (!string.IsNullOrEmpty(path) && this.watchedFolders.TryGetValue(path, out uint cookie))
            {
                this.watchedFolders.Remove(path);
                return ErrorHandler.Succeeded(this.fileWatcher.Value.UnadviseDirChange(cookie));
            }
            return false;
        }

        public int FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                for (var i = 0; i < cChanges; i++)
                {
                    if(this.IsDisposed)
                    {
                        return;
                    }

                    this.FileChangedEvent?.Invoke(this, new TestFileChangedEventArgs(rgpszFile[i], ConvertVSFILECHANGEFLAGS(rggrfChange[i])));
                }
            }).FileAndForget(TelemetryEvents.TestFilesWatcherEventFaulted);

            return VSConstants.S_OK;
        }

        public int DirectoryChanged(string directory)
        {
            // not called since we implement DirectoryChangedEx2
            return VSConstants.E_NOTIMPL;
        }

        public int DirectoryChangedEx(string directory, string file)
        {
            // not called since we implement DirectoryChangedEx2
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Notifies clients of changes made to a directory. 
        /// </summary>
        /// <param name="directory">Name of the directory that had a change.</param>
        /// <param name="numberOfFilesChanged">Number of files changed.</param>
        /// <param name="filesChanged">Array of file names.</param>
        /// <param name="flags">Array of flags indicating the type of changes. See _VSFILECHANGEFLAGS.</param>

        public int DirectoryChangedEx2(string directory, uint numberOfFilesChanged, string[] filesChanged, uint[] flags)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync (async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                for (var i = 0; i < numberOfFilesChanged; i++)
                {
                    if (this.IsDisposed)
                    {
                        return;
                    }

                    this.FileChangedEvent?.Invoke(this, new TestFileChangedEventArgs(filesChanged[i], ConvertVSFILECHANGEFLAGS(flags[i])));
                }
            }).FileAndForget(TelemetryEvents.TestFilesWatcherEventFaulted);

            return VSConstants.S_OK;
        }

        private static WatcherChangeTypes ConvertVSFILECHANGEFLAGS(uint flag)
        {
            if ((flag & (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Size | _VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Attr)) != 0)
            {
                return WatcherChangeTypes.Changed;
            }
            if ((flag & (uint)_VSFILECHANGEFLAGS.VSFILECHG_Add) != 0)
            {
                return WatcherChangeTypes.Created;
            }
            if ((flag & (uint)_VSFILECHANGEFLAGS.VSFILECHG_Del) != 0)
            {
                return WatcherChangeTypes.Deleted;
            }

            Debug.Fail($"Unexpected value for the file changed event: \'{flag}\'");
            return WatcherChangeTypes.Changed;
        }

        private void CheckDisposed()
        {
            if (this.disposed == DISPOSED)
            {
                throw new ObjectDisposedException(nameof(TestFilesUpdateWatcher));   
            }
        }

        private bool IsDisposed => this.disposed == DISPOSED;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this.disposed, DISPOSED, NOT_DISPOSED) == NOT_DISPOSED
                && this.fileWatcher.IsValueCreated)
            {
                foreach (var cookie in this.watchedFiles.Values)
                {
                    this.fileWatcher.Value.UnadviseFileChange(cookie);
                }
                foreach (var cookie in this.watchedFolders.Values)
                {
                    this.fileWatcher.Value.UnadviseDirChange(cookie);
                }
                this.watchedFiles.Clear();
                this.watchedFolders.Clear();
            }
        }
    }
}
