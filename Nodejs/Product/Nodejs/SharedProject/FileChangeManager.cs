// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.VisualStudioTools.Project
{
    public sealed class FolderChangedEventArgs : EventArgs
    {
        public readonly string FolderName;
        public readonly string FileName;
        public _VSFILECHANGEFLAGS FileChangeFlag;

        public FolderChangedEventArgs(string folderName, string fileName, _VSFILECHANGEFLAGS fileChangeFlag)
        {
            this.FolderName = folderName;
            this.FileName = fileName;
            this.FileChangeFlag = fileChangeFlag;
        }
    }

    /// <summary>
    /// This object is in charge of watching for changes to files and folders.
    /// </summary>
    internal sealed class FileChangeManager
    {
        private sealed class FileChangeEvents : IVsFreeThreadedFileChangeEvents
        {
            private readonly FileChangeManager fileChangeManager;

            public FileChangeEvents(FileChangeManager fileChangeManager)
            {
                this.fileChangeManager = fileChangeManager;
            }

            /// <summary>
            /// Called when one of the file have changed on disk.
            /// </summary>
            /// <param name="numberOfFilesChanged">Number of files changed.</param>
            /// <param name="filesChanged">Array of file names.</param>
            /// <param name="flags">Array of flags indicating the type of changes. See _VSFILECHANGEFLAGS.</param>
            /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
            public int FilesChanged(uint numberOfFilesChanged, string[] filesChanged, uint[] flags)
            {
                if (filesChanged == null)
                {
                    throw new ArgumentNullException(nameof(filesChanged));
                }

                if (flags == null)
                {
                    throw new ArgumentNullException(nameof(flags));
                }

                for (var i = 0; i < numberOfFilesChanged; i++)
                {
                    var fullFileName = Utilities.CanonicalizeFileName(filesChanged[i]);
                    if (this.fileChangeManager.observedFiles.TryGetValue(fullFileName, out var value))
                    {
                        var (ItemID, FileChangeCookie) = value;
                        this.fileChangeManager.FileChangedOnDisk?.Invoke(this, new FileChangedOnDiskEventArgs(fullFileName, ItemID, (_VSFILECHANGEFLAGS)flags[i]));
                    }
                }

                return VSConstants.S_OK;
            }

            /// <summary>
            /// Notifies clients of changes made to a directory. 
            /// </summary>
            /// <param name="directory">Name of the directory that had a change.</param>
            /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code. </returns>
            public int DirectoryChanged(string directory)
            {
                return VSConstants.S_OK;
            }

            public int DirectoryChangedEx(string pszDirectory, string pszFile)
            {
                if (pszDirectory == null)
                {
                    throw new ArgumentNullException(nameof(pszDirectory));
                }

                if (pszFile == null)
                {
                    throw new ArgumentNullException(nameof(pszFile));
                }

                this.fileChangeManager.FolderChangedOnDisk?.Invoke(this, new FolderChangedEventArgs(pszDirectory, pszFile, (_VSFILECHANGEFLAGS)0 /* default for now, untill VS implements API that returns actual change */));

                return VSConstants.S_OK;
            }
        }

        /// <summary>
        /// Event that is raised when one of the observed file names have changed on disk.
        /// </summary>
        public event EventHandler<FileChangedOnDiskEventArgs> FileChangedOnDisk;

        public event EventHandler<FolderChangedEventArgs> FolderChangedOnDisk;

        /// <summary>
        /// Reference to the FileChange service.
        /// </summary>
        private readonly IVsFileChangeEx fileChangeService;

        /// <summary>
        /// Maps between the observed file identified by its filename (in canonicalized form) and the cookie used for subscribing 
        /// to the events.
        /// </summary>
        private readonly ConcurrentDictionary<string, (uint ItemID, uint FileChangeCookie)> observedFiles = new ConcurrentDictionary<string, (uint, uint)>();

        /// <summary>
        /// Maps between the observer folder identified by its foldername (in canonicalized form) and the cookie used for subscribing 
        /// to the events.
        /// </summary>
        private readonly ConcurrentDictionary<string, uint> observedFolders = new ConcurrentDictionary<string, uint>();

        /// <summary>
        /// Has Disposed already been called?
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Sink for the events raised by the FileChange service.
        /// </summary>
        private readonly FileChangeEvents fileChangeEvents;

        /// <summary>
        /// Overloaded ctor.
        /// </summary>
        /// <param name="nodeParam">An instance of a project item.</param>
        public FileChangeManager(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            this.fileChangeService = (IVsFileChangeEx)serviceProvider.GetService(typeof(SVsFileChangeEx));

            if (this.fileChangeService == null)
            {
                // VS is in bad state, since the SVsFileChangeEx could not be proffered.
                throw new InvalidOperationException();
            }

            this.fileChangeEvents = new FileChangeEvents(this);
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            // Don't dispose more than once
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            // Unsubscribe from the observed source files.
            foreach (var (ItemID, FileChangeCookie) in this.observedFiles.Values)
            {
                ErrorHandler.ThrowOnFailure(this.fileChangeService.UnadviseFileChange(FileChangeCookie));
            }

            // Clean the observerItems list
            this.observedFiles.Clear();

            // Unsubscribe from the observed source files.
            foreach (var folderCookie in this.observedFolders.Values)
            {
                ErrorHandler.ThrowOnFailure(this.fileChangeService.UnadviseDirChange(folderCookie));
            }

            // Clean the observerItems list
            this.observedFolders.Clear();
        }

        /// <summary>
        /// Observe when the given file is updated on disk. In this case we do not care about the item id that represents the file in the hierarchy.
        /// </summary>
        /// <param name="fileName">File to observe.</param>
        public void ObserveFile(string fileName)
        {
            this.CheckDisposed();

            this.ObserveFile(fileName, VSConstants.VSITEMID_NIL);
        }

        /// <summary>
        /// Observe when the given file is updated on disk.
        /// </summary>
        /// <param name="fileName">File to observe.</param>
        /// <param name="id">The item id of the item to observe.</param>
        public void ObserveFile(string fileName, uint id)
        {
            this.CheckDisposed();

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter), nameof(fileName));
            }

            var fullFileName = Utilities.CanonicalizeFileName(fileName);
            if (!this.observedFiles.ContainsKey(fullFileName))
            {
                // Observe changes to the file
                ErrorHandler.ThrowOnFailure(this.fileChangeService.AdviseFileChange(fullFileName, (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Del), this.fileChangeEvents, out var fileChangeCookie));

                // Remember that we're observing this file (used in FilesChanged event handler)
                this.observedFiles.TryAdd(fullFileName, (id, fileChangeCookie));
            }
        }

        public void ObserveFolder(string folderName)
        {
            this.CheckDisposed();

            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter), nameof(folderName));
            }

            var fullFolderName = Utilities.CanonicalizeFileName(folderName);
            if (!this.observedFolders.ContainsKey(fullFolderName))
            {
                // Observe changes to the file
                ErrorHandler.ThrowOnFailure(this.fileChangeService.AdviseDirChange(fullFolderName, /*fWatchSubDir*/1, this.fileChangeEvents, out var folderChangeCookie));

                // Remember that we're observing this file (used in FilesChanged event handler)
                this.observedFolders.TryAdd(fullFolderName, folderChangeCookie);
            }
        }

        /// <summary>
        /// Ignore item file changes for the specified item.
        /// </summary>
        /// <param name="fileName">File to ignore observing.</param>
        /// <param name="ignore">Flag indicating whether or not to ignore changes (1 to ignore, 0 to stop ignoring).</param>
        public void IgnoreItemChanges(string fileName, bool ignore)
        {
            this.CheckDisposed();

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter), nameof(fileName));
            }

            var fullFileName = Utilities.CanonicalizeFileName(fileName);
            if (this.observedFiles.ContainsKey(fullFileName))
            {
                // Call ignore file with the flags specified.
                ErrorHandler.ThrowOnFailure(this.fileChangeService.IgnoreFile(0, fileName, ignore ? 1 : 0));
            }
        }

        /// <summary>
        /// Stop observing when the file is updated on disk.
        /// </summary>
        /// <param name="fileName">File to stop observing.</param>
        public void StopObservingFile(string fileName)
        {
            this.CheckDisposed();

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter), nameof(fileName));
            }

            var fullFileName = Utilities.CanonicalizeFileName(fileName);

            // Remove the file from our observed list. It's important that this is done before the call to 
            // UnadviseFileChange, because for some reason, the call to UnadviseFileChange can trigger a 
            // FilesChanged event, and we want to be able to filter that event away.
            if (this.observedFiles.TryRemove(fullFileName, out var value))
            {
                // Get the cookie that was used for this.observedItems to this file.
                var (ItemID, FileChangeCookie) = value;

                // Stop observing the file
                ErrorHandler.ThrowOnFailure(this.fileChangeService.UnadviseFileChange(FileChangeCookie));
            }
        }

        public bool StopObservingFolder(string folderName)
        {
            this.CheckDisposed();

            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter), nameof(folderName));
            }

            var fullFolderName = Utilities.CanonicalizeFileName(folderName);

            if (this.observedFolders.TryRemove(fullFolderName, out var cookie))
            {
                // Stop observing the file
                return ErrorHandler.Succeeded(this.fileChangeService.UnadviseDirChange(cookie));
            }
            return false;
        }

        private void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(FileChangeManager));
            }
        }
    }
}
