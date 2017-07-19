// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Microsoft.VisualStudioTools
{
    /// <summary>
    ///     Listens to the file system change notifications and raises events when a directory,
    ///     or file in a directory, changes.  This replaces using FileSystemWatcher as this
    ///     implementation/wrapper uses a guard to make sure we don't try to operate during disposal.
    /// </summary>
    internal sealed class FileWatcher : IDisposable
    {
        private FileSystemWatcher _fsw;

        public FileWatcher(string path = "", string filter = "*.*")
        {
            this._fsw = new FileSystemWatcher(path, filter);
        }

        public bool IsDisposing { get; private set; }

        public bool EnableRaisingEvents
        {
            get { return !this.IsDisposing ? this._fsw.EnableRaisingEvents : false; }
            set { if (!this.IsDisposing) { this._fsw.EnableRaisingEvents = value; } }
        }

        public bool IncludeSubdirectories
        {
            get { return !this.IsDisposing ? this._fsw.IncludeSubdirectories : false; }
            set { if (!this.IsDisposing) { this._fsw.IncludeSubdirectories = value; } }
        }

        /// <summary>
        /// The internal buffer size in bytes. The default is 8192 (8 KB).
        /// </summary>
        public int InternalBufferSize
        {
            get { return !this.IsDisposing ? this._fsw.InternalBufferSize : 0; }
            set { if (!this.IsDisposing) { this._fsw.InternalBufferSize = value; } }
        }

        public NotifyFilters NotifyFilter
        {
            get { return !this.IsDisposing ? this._fsw.NotifyFilter : new NotifyFilters(); }
            set { if (!this.IsDisposing) { this._fsw.NotifyFilter = value; } }
        }

        public event FileSystemEventHandler Changed
        {
            add
            {
                this._fsw.Changed += value;
            }
            remove
            {
                this._fsw.Changed -= value;
            }
        }

        public event FileSystemEventHandler Created
        {
            add
            {
                this._fsw.Created += value;
            }
            remove
            {
                this._fsw.Created -= value;
            }
        }

        public event FileSystemEventHandler Deleted
        {
            add
            {
                this._fsw.Deleted += value;
            }
            remove
            {
                this._fsw.Deleted -= value;
            }
        }

        public event ErrorEventHandler Error
        {
            add
            {
                this._fsw.Error += value;
            }
            remove
            {
                this._fsw.Error -= value;
            }
        }

        public event RenamedEventHandler Renamed
        {
            add
            {
                this._fsw.Renamed += value;
            }
            remove
            {
                this._fsw.Renamed -= value;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_fsw",
            Justification = "Will be disposed on a separate thread to avoid deadlocks")]
        public void Dispose()
        {
            if (!this.IsDisposing)
            {
                this.IsDisposing = true;

                // Call the _fsw dispose method from the background so it doesn't block anything else.
                var backgroundDispose = new Thread(this.BackgroundDispose);
                backgroundDispose.IsBackground = true;
                backgroundDispose.Start();
            }
        }

        private void BackgroundDispose()
        {
            try
            {
                this._fsw.Dispose();
            }
            catch (Exception) { }
        }
    }
}
