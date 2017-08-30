// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Microsoft.VisualStudioTools.TestAdapter
{
    internal class TestFilesUpdateWatcher : IVsFileChangeEvents
    {
        private readonly IDictionary<string, uint> watchedFiles = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        public event EventHandler<TestFileChangedEventArgs> FileChangedEvent;

        private readonly IVsFileChangeEx fileWatcher;

        public TestFilesUpdateWatcher(IServiceProvider serviceProvider)
        {
            this.fileWatcher = (IVsFileChangeEx)serviceProvider.GetService<SVsFileChangeEx>();
        }

        public bool AddWatch(string path)
        {
            ValidateArg.NotNull(path, "path");
            if (!this.watchedFiles.ContainsKey(path) && ErrorHandler.Succeeded(this.fileWatcher.AdviseFileChange(path, (uint)_VSFILECHANGEFLAGS.VSFILECHG_Add, this, out uint cookie)))
            {
                this.watchedFiles.Add(path, cookie);
                return true;
            }
            return false;
        }

        public bool AddDirectoryWatch(string path)
        {
            ValidateArg.NotNull(path, "path");

            if (!this.watchedFiles.ContainsKey(path) && ErrorHandler.Succeeded(this.fileWatcher.AdviseDirChange(path, VSConstants.S_OK, this, out uint cookie)))
            {
                this.watchedFiles.Add(path, cookie);
                return true;
            }
            return false;
        }

        public bool RemoveWatch(string path)
        {
            ValidateArg.NotNull(path, "path");

            if (this.watchedFiles.TryGetValue(path, out uint cookie))
            {
                this.watchedFiles.Remove(path);
                if (Path.HasExtension(path))
                {
                    return ErrorHandler.Succeeded(this.fileWatcher.UnadviseFileChange(cookie));
                }
                else
                {
                    return ErrorHandler.Succeeded(this.fileWatcher.UnadviseDirChange(cookie));
                }
            }
            return false;
        }

        int IVsFileChangeEvents.FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            for (var i = 0; i < cChanges; i++)
            {
                FileChangedEvent?.Invoke(this, new TestFileChangedEventArgs(null, rgpszFile[i], TestFileChangedReason.Changed));
            }

            return VSConstants.S_OK;
        }

        int IVsFileChangeEvents.DirectoryChanged(string pszDirectory)
        {
            FileChangedEvent?.Invoke(this, new TestFileChangedEventArgs(null, pszDirectory, TestFileChangedReason.Changed));
            return VSConstants.S_OK;
        }
    }
}
