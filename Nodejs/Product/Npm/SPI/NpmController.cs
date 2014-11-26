/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmController : AbstractNpmLogSource, INpmController {

        private IPackageCatalog _sRepoCatalog;
        private string _fullPathToRootPackageDirectory;
        private string _cachePath;
        private bool _showMissingDevOptionalSubPackages;
        private INpmPathProvider _npmPathProvider;
        private IRootPackage _rootPackage;
        private IGlobalPackages _globalPackage;
        private readonly object _lock = new object();

        private readonly FileSystemWatcher _localWatcher;
        private readonly FileSystemWatcher _globalWatcher;

        private Timer _fileSystemWatcherTimer;
        private int _refreshRetryCount;

        private readonly object _fileBitsLock = new object();


        private bool _isDisposed;

        public NpmController(
            string fullPathToRootPackageDirectory,
            string cachePath,
            bool showMissingDevOptionalSubPackages = false,
            INpmPathProvider npmPathProvider = null) {
            _fullPathToRootPackageDirectory = fullPathToRootPackageDirectory;
            _cachePath = cachePath;
            _showMissingDevOptionalSubPackages = showMissingDevOptionalSubPackages;
            _npmPathProvider = npmPathProvider;

            _localWatcher = CreateModuleDirectoryWatcherIfDirectoryExists(_fullPathToRootPackageDirectory);
            _globalWatcher = CreateModuleDirectoryWatcherIfDirectoryExists(this.ListBaseDirectory);

            try {
                ReloadModules();
            }
            catch (NpmNotFoundException) { }
        }

        internal string FullPathToRootPackageDirectory {
            get { return _fullPathToRootPackageDirectory; }
        }

        internal string PathToNpm {
            get { return null == _npmPathProvider ? null : _npmPathProvider.PathToNpm; }
        }

        internal string CachePath {
            get { return _cachePath; }
        }

        public event EventHandler StartingRefresh;

        private void Fire(EventHandler handlers) {
            if (null != handlers) {
                handlers(this, EventArgs.Empty);
            }
        }

        private void OnStartingRefresh() {
            Fire(StartingRefresh);
        }

        public event EventHandler FinishedRefresh;

        private void OnFinishedRefresh() {
            Fire(FinishedRefresh);
        }

        public void Refresh() {
            RefreshAsync().ContinueWith(t => {
                var ex = t.Exception;
                if (ex != null) {
                    OnOutputLogged(ex.ToString());
#if DEBUG
                    Debug.Fail(ex.ToString());
#endif
                }
            });
        }
        
        public async Task RefreshAsync() {
            OnStartingRefresh();
            try {
                RootPackage = RootPackageFactory.Create(
                            _fullPathToRootPackageDirectory,
                            _showMissingDevOptionalSubPackages);
                
                var command = new NpmLsCommand(_fullPathToRootPackageDirectory, true, PathToNpm);

                GlobalPackages = (await command.ExecuteAsync())
                    ? RootPackageFactory.Create(command.ListBaseDirectory)
                    : null;
            } catch (IOException) {
                // Can sometimes happen when packages are still installing because the file may still be used by another process
            } finally {
                if (RootPackage == null) {
                    OnOutputLogged("Error - Cannot load local packages.");
                }
                if (GlobalPackages == null) {
                    OnOutputLogged("Error - Cannot load global packages.");
                }
                OnFinishedRefresh();
            }
        }

        public string ListBaseDirectory {
            get {
                var command = new NpmLsCommand(_fullPathToRootPackageDirectory, true, PathToNpm);

                if (Task.Run(async () => { return await command.ExecuteAsync(); } ).Result) {
                    return command.ListBaseDirectory;
                }

                return null;
            }
        }

        public IRootPackage RootPackage {
            get {
                lock (_lock) {
                    return _rootPackage;
                }
            }

            private set {
                lock (_lock) {
                    _rootPackage = value;
                }
            }
        }

        public IGlobalPackages GlobalPackages {
            get {
                lock (_lock) {
                    return _globalPackage;
                }
            }
            private set {
                lock (_lock) {
                    _globalPackage = value;
                }
            }
        }

        public INpmCommander CreateNpmCommander() {
            return new NpmCommander(this);
        }

        public void LogCommandStarted(object sender, EventArgs args) {
            OnCommandStarted();
        }

        public void LogOutput(object sender, NpmLogEventArgs e) {
            OnOutputLogged(e.LogText);
        }

        public void LogError(object sender, NpmLogEventArgs e) {
            OnErrorLogged(e.LogText);
        }

        public void LogException(object sender, NpmExceptionEventArgs e) {
            OnExceptionLogged(e.Exception);
        }

        public void LogCommandCompleted(object sender, NpmCommandCompletedEventArgs e) {
            OnCommandCompleted(e.Arguments, e.WithErrors, e.Cancelled);
        }

        public async Task<IPackageCatalog> GetRepositoryCatalogAsync(bool forceDownload, IProgress<string> progress) {
            //  This should really be thread-safe but await can't be inside a lock so
            //  we'll just have to hope and pray this doesn't happen concurrently. Worst
            //  case is we'll end up with two retrievals, one of which will be binned,
            //  which isn't the end of the world.
            _sRepoCatalog = null;
            if (null == _sRepoCatalog || _sRepoCatalog.ResultsCount == 0 || forceDownload) {
                Exception ex = null;
                using (var commander = CreateNpmCommander()) {
                    EventHandler<NpmExceptionEventArgs> exHandler = (sender, args) => { LogException(sender, args); ex = args.Exception; };
                    commander.ErrorLogged += LogError;
                    commander.ExceptionLogged += exHandler;
                    _sRepoCatalog = await commander.GetCatalogAsync(forceDownload, progress);
                    commander.ErrorLogged -= LogError;
                    commander.ExceptionLogged -= exHandler;
                }
                if (null != ex) {
                    OnOutputLogged(ex.ToString());
                    throw ex;
                }
            }
            return _sRepoCatalog;
        }

        public IPackageCatalog MostRecentlyLoadedCatalog {
            get { return _sRepoCatalog; }
        }


        private FileSystemWatcher CreateModuleDirectoryWatcherIfDirectoryExists(string directory) {
            if (!Directory.Exists(directory)) {
                return null;
            }

            FileSystemWatcher watcher = null;
            try {
                watcher = new FileSystemWatcher(directory) {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                    IncludeSubdirectories = true
                };

                watcher.Changed += Watcher_Modified;
                watcher.Created += Watcher_Modified;
                watcher.Deleted += Watcher_Modified;
                watcher.EnableRaisingEvents = true;
            } catch (Exception ex) {
                if (watcher != null) {
                    watcher.Dispose();
                }
                if (ex is IOException || ex is ArgumentException) {
                    Debug.WriteLine("Error starting FileSystemWatcher:\r\n{0}", ex);
                } else {
                    throw;
                }
            }

            return watcher;
        }

        private void Watcher_Modified(object sender, FileSystemEventArgs e) {
            string path = e.FullPath;
            if (!path.EndsWith("package.json", StringComparison.OrdinalIgnoreCase) && path.IndexOf("\\node_modules", StringComparison.OrdinalIgnoreCase) == -1) {
                return;
            }

            RestartFileSystemWatcherTimer();
        }


        private void RestartFileSystemWatcherTimer() {
            lock (_fileBitsLock) {
                if (null != _fileSystemWatcherTimer) {
                    _fileSystemWatcherTimer.Dispose();
                }

                _fileSystemWatcherTimer = new Timer(o => UpdateModulesFromTimer(), null, 1000, Timeout.Infinite);
            }
        }

        private void UpdateModulesFromTimer() {
            lock (_fileBitsLock) {
                if (null != _fileSystemWatcherTimer) {
                    _fileSystemWatcherTimer.Dispose();
                    _fileSystemWatcherTimer = null;
                }
            }

            ReloadModules();
        }


        private void ReloadModules() {
            var retry = false;
            Exception ex = null;
            try {
                this.Refresh();
            } catch (PackageJsonException pje) {
                retry = true;
                ex = pje;
            } catch (AggregateException ae) {
                retry = true;
                ex = ae;
            } catch (FileLoadException fle) {
                //  Fixes bug reported in work item 447 - just wait a bit and retry!
                retry = true;
                ex = fle;
            }

            if (retry) {
                if (_refreshRetryCount < 5) {
                    ++_refreshRetryCount;
                    RestartFileSystemWatcherTimer();
                } else {
                    OnExceptionLogged(ex);
                }
            }
        }

        public void Dispose() {
            if (!_isDisposed) {
                lock (_fileBitsLock) {
                    if (_localWatcher != null) {
                        _localWatcher.Changed -= Watcher_Modified;
                        _localWatcher.Created -= Watcher_Modified;
                        _localWatcher.Deleted -= Watcher_Modified;
                        _localWatcher.Dispose();
                    }

                    if (_globalWatcher != null) {
                        _globalWatcher.Changed -= Watcher_Modified;
                        _globalWatcher.Created -= Watcher_Modified;
                        _globalWatcher.Deleted -= Watcher_Modified;
                        _globalWatcher.Dispose();
                    }
                }

                lock (_fileBitsLock) {
                    if (null != _fileSystemWatcherTimer) {
                        _fileSystemWatcherTimer.Dispose();
                        _fileSystemWatcherTimer = null;
                    }
                }
                
                _isDisposed = true;
            }
        }
    }
}