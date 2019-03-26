// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NpmController : AbstractNpmLogSource, INpmController
    {
        private string cachePath;
        private bool showMissingDevOptionalSubPackages;
        private INpmPathProvider npmPathProvider;
        private IRootPackage rootPackage;
        private readonly object packageLock = new object();

        private readonly FileSystemWatcher localWatcher;

        private Timer fileSystemWatcherTimer;
        private int refreshRetryCount;

        private readonly object fileBitsLock = new object();

        private bool isDisposed;
        private bool isReloadingModules = false;

        public NpmController(
            string fullPathToRootPackageDirectory,
            string cachePath,
            bool isProject,
            bool showMissingDevOptionalSubPackages = false,
            INpmPathProvider npmPathProvider = null)
        {
            this.IsProject = isProject;
            this.FullPathToRootPackageDirectory = fullPathToRootPackageDirectory;
            this.cachePath = cachePath;
            this.showMissingDevOptionalSubPackages = showMissingDevOptionalSubPackages;
            this.npmPathProvider = npmPathProvider;

            this.localWatcher = CreateModuleDirectoryWatcherIfDirectoryExists(this.FullPathToRootPackageDirectory);

            try
            {
                ReloadModules();
            }
            catch (NpmNotFoundException) { }
        }

        public bool IsProject { get; }

        internal string FullPathToRootPackageDirectory { get; }

        internal string PathToNpm
        {
            get
            {
                try
                {
                    return this.npmPathProvider?.PathToNpm;
                }
                catch (NpmNotFoundException)
                {
                    return null;
                }
            }
        }

        public event EventHandler StartingRefresh;

        private void RaiseEvents(EventHandler handlers)
        {
            if (null != handlers)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        private void OnStartingRefresh()
        {
            RaiseEvents(StartingRefresh);
        }

        public event EventHandler FinishedRefresh;

        private void OnFinishedRefresh()
        {
            RaiseEvents(FinishedRefresh);
        }

        public void Refresh()
        {
            try
            {
                RefreshImplementation();
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    OnOutputLogged(ex.ToString());
#if DEBUG
                    Debug.Fail(ex.ToString());
#endif
                }
            }
        }

        private void RefreshImplementation()
        {
            OnStartingRefresh();
            try
            {
                lock (this.fileBitsLock)
                {
                    if (this.isReloadingModules)
                    {
                        RestartFileSystemWatcherTimer();
                        return;
                    }
                    else
                    {
                        this.isReloadingModules = true;
                    }
                }

                this.RootPackage = RootPackageFactory.Create(
                            this.FullPathToRootPackageDirectory,
                            this.showMissingDevOptionalSubPackages);
                return;
            }
            catch (IOException)
            {
                // Can sometimes happen when packages are still installing because the file may still be used by another process
            }
            finally
            {
                lock (this.fileBitsLock)
                {
                    this.isReloadingModules = false;
                }
                if (this.RootPackage == null)
                {
                    OnOutputLogged("Error - Cannot load local packages.");
                }
                OnFinishedRefresh();
            }
        }

        public IRootPackage RootPackage
        {
            get
            {
                lock (this.packageLock)
                {
                    return this.rootPackage;
                }
            }

            private set
            {
                lock (this.packageLock)
                {
                    this.rootPackage = value;
                }
            }
        }

        public INpmCommander CreateNpmCommander()
        {
            return new NpmCommander(this);
        }

        public void LogCommandStarted(object sender, NpmCommandStartedEventArgs args)
        {
            OnCommandStarted(args.Arguments);
        }

        public void LogOutput(object sender, NpmLogEventArgs e)
        {
            OnOutputLogged(e.LogText);
        }

        public void LogError(object sender, NpmLogEventArgs e)
        {
            OnErrorLogged(e.LogText);
        }

        public void LogException(object sender, NpmExceptionEventArgs e)
        {
            OnExceptionLogged(e.Exception);
        }

        public void LogCommandCompleted(object sender, NpmCommandCompletedEventArgs e)
        {
            OnCommandCompleted(e.Arguments, e.WithErrors, e.Cancelled);
        }

        private FileSystemWatcher CreateModuleDirectoryWatcherIfDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return null;
            }

            FileSystemWatcher watcher = null;
            try
            {
                watcher = new FileSystemWatcher(directory)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                    IncludeSubdirectories = true
                };

                watcher.Changed += this.WatcherModified;
                watcher.Created += this.WatcherModified;
                watcher.Deleted += this.WatcherModified;
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                if (watcher != null)
                {
                    watcher.Dispose();
                }
                if (ex is IOException || ex is ArgumentException)
                {
                    Debug.WriteLine("Error starting FileSystemWatcher:\r\n{0}", ex);
                }
                else
                {
                    throw;
                }
            }

            return watcher;
        }

        private void WatcherModified(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath;

            // Check that the file is either a package.json file, or exists in the node_modules directory
            // This allows us to properly detect both installed and uninstalled/linked packages (where we don't receive an event for package.json)
            if (path.EndsWith("package.json", StringComparison.OrdinalIgnoreCase) || path.IndexOf(NodejsConstants.NodeModulesFolder, StringComparison.OrdinalIgnoreCase) != -1)
            {
                RestartFileSystemWatcherTimer();
            }

            return;
        }

        private void RestartFileSystemWatcherTimer()
        {
            lock (this.fileBitsLock)
            {
                if (null != this.fileSystemWatcherTimer)
                {
                    this.fileSystemWatcherTimer.Dispose();
                }

                // Be sure to update the FileWatcher in NodejsProjectNode if the dueTime value changes.
                this.fileSystemWatcherTimer = new Timer(o => UpdateModulesFromTimer(), null, 1000, Timeout.Infinite);
            }
        }

        private void UpdateModulesFromTimer()
        {
            lock (this.fileBitsLock)
            {
                if (null != this.fileSystemWatcherTimer)
                {
                    this.fileSystemWatcherTimer.Dispose();
                    this.fileSystemWatcherTimer = null;
                }
            }

            ReloadModules();
        }

        private void ReloadModules()
        {
            var retry = false;
            Exception ex = null;
            try
            {
                this.Refresh();
            }
            catch (PackageJsonException pje)
            {
                retry = true;
                ex = pje;
            }
            catch (AggregateException ae)
            {
                retry = true;
                ex = ae;
            }
            catch (FileLoadException fle)
            {
                //  Fixes bug reported in work item 447 - just wait a bit and retry!
                retry = true;
                ex = fle;
            }

            if (retry)
            {
                if (this.refreshRetryCount < 5)
                {
                    ++this.refreshRetryCount;
                    RestartFileSystemWatcherTimer();
                }
                else
                {
                    OnExceptionLogged(ex);
                }
            }
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                lock (this.fileBitsLock)
                {
                    if (this.localWatcher != null)
                    {
                        this.localWatcher.Changed -= this.WatcherModified;
                        this.localWatcher.Created -= this.WatcherModified;
                        this.localWatcher.Deleted -= this.WatcherModified;
                        this.localWatcher.Dispose();
                    }
                }

                lock (this.fileBitsLock)
                {
                    if (null != this.fileSystemWatcherTimer)
                    {
                        this.fileSystemWatcherTimer.Dispose();
                        this.fileSystemWatcherTimer = null;
                    }
                }

                this.isDisposed = true;
            }
        }
    }
}
