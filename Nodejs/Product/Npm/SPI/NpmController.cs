using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal class NpmController : INpmController{
        //  *Really* don't want to retrieve this more than once:
        //  47,000 packages takes a while.
        private static IEnumerable<IPackage> _sRepoCatalogue;

        private string _fullPathToRootPackageDirectory;
        private bool _showMissingDevOptionalSubPackages;
        private string _pathToNpm;
        private IRootPackage _rootPackage;
        private IGlobalPackages _globalPackage;
        private readonly object _lock = new object();

        public NpmController(
            string fullPathToRootPackageDirectory,
            bool showMissingDevOptionalSubPackages = false,
            string pathToNpm = null){
            _fullPathToRootPackageDirectory = fullPathToRootPackageDirectory;
            _showMissingDevOptionalSubPackages = showMissingDevOptionalSubPackages;
            _pathToNpm = pathToNpm;
            Refresh();
        }

        public event EventHandler StartingRefresh;

        private void Fire(EventHandler handlers){
            if (null != handlers){
                handlers(this, EventArgs.Empty);
            }
        }

        private void OnStartingRefresh(){
            Fire(StartingRefresh);
        }

        public event EventHandler FinishedRefresh;

        private void OnFinishedRefresh(){
            Fire(FinishedRefresh);
        }

        public void Refresh(){
            OnStartingRefresh();
            lock (_lock){
                RootPackage = RootPackageFactory.Create(
                    _fullPathToRootPackageDirectory,
                    _showMissingDevOptionalSubPackages);

                var command = new NpmLsCommand(_fullPathToRootPackageDirectory, true, _pathToNpm);
                GlobalPackages = AsyncHelpers.RunSync<bool>(() => command.ExecuteAsync())
                                     ? RootPackageFactory.Create(command.ListBaseDirectory)
                                     : null;
            }
            OnFinishedRefresh();
        }

        public IRootPackage RootPackage{
            get{
                lock (_lock){
                    return _rootPackage;
                }
            }

            private set{
                lock (_lock){
                    _rootPackage = value;
                }
            }
        }

        public IGlobalPackages GlobalPackages{
            get{
                lock (_lock){
                    return _globalPackage;
                }
            }
            private set{
                lock (_lock){
                    _globalPackage = value;
                }
            }
        }

        //  TODO: events should be fired as data is logged, not in one massive barf at the end
        private void FireLogEvents(NpmCommand command){
            OnOutputLogged(command.StandardOutput);
            OnErrorLogged(command.StandardError);
        }

        private async Task<bool> InstallPackageByVersionAsync(
            string packageName,
            string versionRange,
            DependencyType type,
            bool global){
            var command = new NpmInstallCommand(
                _fullPathToRootPackageDirectory,
                packageName,
                versionRange,
                type,
                global,
                _pathToNpm);

            var retVal = await command.ExecuteAsync();
            FireLogEvents(command);
            Refresh();
            return retVal;
        }

        public async Task<bool> InstallPackageByVersionAsync(
            string packageName,
            string versionRange,
            DependencyType type){
            return await InstallPackageByVersionAsync(packageName, versionRange, type, false);
        }

        public async Task<bool> InstallGlobalPackageByVersionAsync(string packageName, string versionRange){
            return await InstallPackageByVersionAsync(packageName, versionRange, DependencyType.Standard, true);
        }

        private DependencyType GetDependencyType(string packageName){
            var type = DependencyType.Standard;
            var root = RootPackage;
            if (null != root){
                var match = root.Modules[packageName];
                if (null != match){
                    if (match.IsDevDependency){
                        type = DependencyType.Development;
                    } else if (match.IsOptionalDependency){
                        type = DependencyType.Optional;
                    }
                }
            }
            return type;
        }

        private async Task<bool> UninstallPackageAsync(string packageName, bool global){
            var command = new NpmUninstallCommand(
                _fullPathToRootPackageDirectory,
                packageName,
                GetDependencyType(packageName),
                global,
                _pathToNpm);

            var retVal = await command.ExecuteAsync();
            FireLogEvents(command);
            Refresh();
            return retVal;
        }

        public async Task<bool> UninstallPackageAsync(string packageName){
            return await UninstallPackageAsync(packageName, false);
        }

        public async Task<bool> UninstallGlobalPackageAsync(string packageName){
            return await UninstallPackageAsync(packageName, true);
        }

        public async Task<IEnumerable<IPackage>> SearchAsync(string searchText){
            var command = new NpmSearchCommand(_fullPathToRootPackageDirectory, searchText, _pathToNpm);
            var success = await command.ExecuteAsync();
            FireLogEvents(command);
            return success ? command.Results : new List<IPackage>();
        }

        public async Task<IEnumerable<IPackage>> GetRepositoryCatalogueAsync(){
            //  This should really be thread-safe but await can't be inside a lock so
            //  we'll just have to hope and pray this doesn't happen concurrently. Worst
            //  case is we'll end up with two retrievals, one of which will be binned,
            //  which isn't the end of the world.
            if (null == _sRepoCatalogue){
                _sRepoCatalogue = await SearchAsync(null);
            }
            return _sRepoCatalogue;
        }

        public async Task<bool> UpdatePackagesAsync(){
            return await UpdatePackagesAsync(new List<IPackage>());
        }

        public async Task<bool> UpdatePackagesAsync(IEnumerable<IPackage> packages){
            var command = new NpmUpdateCommand(_fullPathToRootPackageDirectory, packages, _pathToNpm);
            var success = await command.ExecuteAsync();
            FireLogEvents(command);
            Refresh();
            return success;
        }

        private void FireNpmLogEvent(string logText, EventHandler<NpmLogEventArgs> handlers){
            if (null != handlers && ! string.IsNullOrEmpty(logText)){
                handlers(this, new NpmLogEventArgs(logText));
            }
        }

        public event EventHandler<NpmLogEventArgs> OutputLogged;

        private void OnOutputLogged(string logText){
            FireNpmLogEvent(logText, OutputLogged);
        }

        public event EventHandler<NpmLogEventArgs> ErrorLogged;

        private void OnErrorLogged(string logText){
            FireNpmLogEvent(logText, ErrorLogged);
        }
    }
}