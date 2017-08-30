// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            this.fileWatcher = serviceProvider.GetService<IVsFileChangeEx>(typeof(SVsFileChangeEx));
        }

        public bool AddFileWatch(string path)
        {
            ValidateArg.NotNull(path, "path");

            const uint mask = (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Add | _VSFILECHANGEFLAGS.VSFILECHG_Del | _VSFILECHANGEFLAGS.VSFILECHG_Size | _VSFILECHANGEFLAGS.VSFILECHG_Time);

            if (!this.watchedFiles.ContainsKey(path) && ErrorHandler.Succeeded(this.fileWatcher.AdviseFileChange(path, mask, this, out uint cookie)))
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

        public bool RemoveFileWatch(string path)
        {
            ValidateArg.NotNull(path, "path");

            if (this.watchedFiles.TryGetValue(path, out uint cookie))
            {
                this.watchedFiles.Remove(path);
                return ErrorHandler.Succeeded(this.fileWatcher.UnadviseFileChange(cookie));
            }
            return false;
        }

        public bool RemoveFolderWatch(string path)
        {
            ValidateArg.NotNull(path, "path");

            if (this.watchedFiles.TryGetValue(path, out uint cookie))
            {
                this.watchedFiles.Remove(path);
                return ErrorHandler.Succeeded(this.fileWatcher.UnadviseDirChange(cookie));
            }
            return false;
        }

        int IVsFileChangeEvents.FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            for (var i = 0; i < cChanges; i++)
            {
                FileChangedEvent?.Invoke(this, new TestFileChangedEventArgs(null, rgpszFile[i], ConvertVSFILECHANGEFLAGS(rggrfChange[i])));
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

            Debug.Fail($"Unexpected value for the file changed event \'{flag}\'");
            return TestFileChangedReason.Changed;
        }
    }
}
