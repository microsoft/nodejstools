//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
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
        private readonly object _lock = new object();

        private readonly FileSystemWatcher _localWatcher;

        private Timer _fileSystemWatcherTimer;
        private int _refreshRetryCount;

        private readonly object _fileBitsLock = new object();


        private bool _isDisposed;
        private bool _isReloadingModules = false;

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

            try {
                ReloadModules();
            } catch (NpmNotFoundException) { }
        }

        internal string FullPathToRootPackageDirectory {
            get { return _fullPathToRootPackageDirectory; }
        }

        internal string PathToNpm {
            get {
                try {
                    return null == _npmPathProvider ? null : _npmPathProvider.PathToNpm;
                } catch (NpmNotFoundException) {
                    return null;
                }
            }
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
            try {
                RefreshImplementation();
            } catch (Exception ex) {
                if (ex != null) {
                    OnOutputLogged(ex.ToString());
#if DEBUG
                    Debug.Fail(ex.ToString());
#endif
                }
            }
        }

        private void RefreshImplementation() {
            OnStartingRefresh();
            try {
                lock (_fileBitsLock) {
                    if (_isReloadingModules) {
                        RestartFileSystemWatcherTimer();
                        return;
                    } else {
                        _isReloadingModules = true;
                    }
                }

                RootPackage = RootPackageFactory.Create(
                            _fullPathToRootPackageDirectory,
                            _showMissingDevOptionalSubPackages);
                return;
            } catch (IOException) {
                // Can sometimes happen when packages are still installing because the file may still be used by another process
            } finally {
                lock (_fileBitsLock) {
                    _isReloadingModules = false;
                }
                if (RootPackage == null) {
                    OnOutputLogged("Error - Cannot load local packages.");
                }
                OnFinishedRefresh();
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

            // Check that the file is either a package.json file, or exists in the node_modules directory
            // This allows us to properly detect both installed and uninstalled/linked packages (where we don't receive an event for package.json)
            if (path.EndsWith("package.json", StringComparison.OrdinalIgnoreCase) || path.IndexOf(NodejsConstants.NodeModulesFolder, StringComparison.OrdinalIgnoreCase) != -1) {
                RestartFileSystemWatcherTimer();
            }

            return;
        }


        private void RestartFileSystemWatcherTimer() {
            lock (_fileBitsLock) {
                if (null != _fileSystemWatcherTimer) {
                    _fileSystemWatcherTimer.Dispose();
                }

                // Be sure to update the FileWatcher in NodejsProjectNode if the dueTime value changes.
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