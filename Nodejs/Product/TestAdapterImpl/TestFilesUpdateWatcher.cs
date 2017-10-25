// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Microsoft.VisualStudioTools.TestAdapter
{
    internal class TestFilesUpdateWatcher : IVsFileChangeEvents, IDisposable
    {
        private readonly IDictionary<string, uint> watchedFiles = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, uint> watchedFolders = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);

        private bool disposed;

        public event EventHandler<TestFileChangedEventArgs> FileChangedEvent;

        private /*readonly*/ IVsFileChangeEx fileWatcher; // writeable for dispose

        public TestFilesUpdateWatcher(IServiceProvider serviceProvider)
        {
            ValidateArg.NotNull(serviceProvider, nameof(serviceProvider));

            this.fileWatcher = serviceProvider.GetService<IVsFileChangeEx>(typeof(SVsFileChangeEx));
        }

        public bool AddFileWatch(string path)
        {
            ValidateArg.NotNull(path, nameof(path));
            this.CheckDisposed();

            const uint mask = (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Add | _VSFILECHANGEFLAGS.VSFILECHG_Del | _VSFILECHANGEFLAGS.VSFILECHG_Size | _VSFILECHANGEFLAGS.VSFILECHG_Time);

            if (!string.IsNullOrEmpty(path) && !this.watchedFiles.ContainsKey(path) && ErrorHandler.Succeeded(this.fileWatcher.AdviseFileChange(path, mask, this, out uint cookie)))
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

            if (!string.IsNullOrEmpty(path) && !this.watchedFolders.ContainsKey(path) && ErrorHandler.Succeeded(this.fileWatcher.AdviseDirChange(path, VSConstants.S_OK, this, out uint cookie)))
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
                return ErrorHandler.Succeeded(this.fileWatcher.UnadviseFileChange(cookie));
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
                return ErrorHandler.Succeeded(this.fileWatcher.UnadviseDirChange(cookie));
            }
            return false;
        }

        int IVsFileChangeEvents.FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            if (this.FileChangedEvent != null)
            {
                var evt = FileChangedEvent;
                for (var i = 0; i < cChanges; i++)
                {
                    evt.Invoke(this, new TestFileChangedEventArgs(null, rgpszFile[i], ConvertVSFILECHANGEFLAGS(rggrfChange[i])));
                }
            }
            return VSConstants.S_OK;
        }

        int IVsFileChangeEvents.DirectoryChanged(string pszDirectory)
        {
            FileChangedEvent?.Invoke(this, new TestFileChangedEventArgs(null, pszDirectory, TestFileChangedReason.Changed));
            return VSConstants.S_OK;
        }

        private static TestFileChangedReason ConvertVSFILECHANGEFLAGS(uint flag)
        {
            if ((flag & (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Size | _VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Attr)) != 0)
            {
                return TestFileChangedReason.Changed;
            }
            if ((flag & (uint)_VSFILECHANGEFLAGS.VSFILECHG_Add) != 0)
            {
                return TestFileChangedReason.Added;
            }
            if ((flag & (uint)_VSFILECHANGEFLAGS.VSFILECHG_Del) != 0)
            {
                return TestFileChangedReason.Removed;
            }

            Debug.Fail($"Unexpected value for the file changed event: \'{flag}\'");
            return TestFileChangedReason.Changed;
        }

        private void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(TestFilesUpdateWatcher));
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                foreach (var cookie in this.watchedFiles.Values)
                {
                    this.fileWatcher.UnadviseFileChange(cookie);
                }
                foreach (var cookie in this.watchedFolders.Values)
                {
                    this.fileWatcher.UnadviseDirChange(cookie);
                }
                this.watchedFiles.Clear();
                this.watchedFolders.Clear();
                this.fileWatcher = null;
            }
        }
    }
}
