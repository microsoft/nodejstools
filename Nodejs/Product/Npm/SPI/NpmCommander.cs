using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal class NpmCommander : INpmCommander{
        private NpmController _npmController;
        private NpmCommand _command;
        private bool _disposed;

        public NpmCommander(NpmController controller){
            _npmController = controller;
            OutputLogged += _npmController.LogOutput;
            ErrorLogged += _npmController.LogError;
            ExceptionLogged += _npmController.LogException;
        }

        public void Dispose(){
            if (!_disposed){
                _disposed = true;
                OutputLogged -= _npmController.LogOutput;
                ErrorLogged -= _npmController.LogError;
                ExceptionLogged -= _npmController.LogException;
            }
        }

        private void FireNpmLogEvent(string logText, EventHandler<NpmLogEventArgs> handlers){
            if (null != handlers && !string.IsNullOrEmpty(logText)){
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

        public event EventHandler<NpmExceptionEventArgs> ExceptionLogged;

        private void OnExceptionLogged(Exception e){
            var handlers = ExceptionLogged;
            if (null != handlers){
                handlers(this, new NpmExceptionEventArgs(e));
            }
        }

        public event EventHandler CommandCompleted;

        private void OnCommandCompleted(){
            var handlers = CommandCompleted;
            if (null != handlers){
                handlers(this, new EventArgs());
            }
        }

        public void CancelCurrentCommand(){
            if (null != _command){
                _command.CancelCurrentTask();
            }
        }

        //  TODO: events should be fired as data is logged, not in one massive barf at the end
        private void FireLogEvents(NpmCommand command){
            //  Filter this out because we ony using search to return the entire module catalogue,
            //  which will spew 47,000+ lines of total guff that the user probably isn't interested
            //  in to the npm log in the output window.
            if (command is NpmSearchCommand){
                return;
            }
            OnOutputLogged(command.StandardOutput);
            OnErrorLogged(command.StandardError);
        }

        public async Task<bool> Install()
        {
            bool retVal = false;
            try
            {
                _command = new NpmInstallCommand(
                    _npmController.FullPathToRootPackageDirectory,
                    _npmController.PathToNpm,
                    _npmController.UseFallbackIfNpmNotFound);

                retVal = await _command.ExecuteAsync();
                FireLogEvents(_command);
                _npmController.Refresh();
            }
            catch (Exception e)
            {
                OnExceptionLogged(e);
            }
            finally
            {
                OnCommandCompleted();
            }
            return retVal;
        }

        private async Task<bool> InstallPackageByVersionAsync(
            string packageName,
            string versionRange,
            DependencyType type,
            bool global){
            bool retVal = false;
            try{
                _command = new NpmInstallCommand(
                    _npmController.FullPathToRootPackageDirectory,
                    packageName,
                    versionRange,
                    type,
                    global,
                    _npmController.PathToNpm,
                    _npmController.UseFallbackIfNpmNotFound);

                retVal = await _command.ExecuteAsync();
                FireLogEvents(_command);
                _npmController.Refresh();
            } catch (Exception e){
                OnExceptionLogged(e);
            } finally{
                OnCommandCompleted();
            }
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
            var root = _npmController.RootPackage;
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
            bool retVal = false;
            try{
                _command = new NpmUninstallCommand(
                    _npmController.FullPathToRootPackageDirectory,
                    packageName,
                    GetDependencyType(packageName),
                    global,
                    _npmController.PathToNpm,
                    _npmController.UseFallbackIfNpmNotFound);

                retVal = await _command.ExecuteAsync();
                FireLogEvents(_command);
                _npmController.Refresh();
            } catch (Exception e){
                OnExceptionLogged(e);
            } finally{
                OnCommandCompleted();
            }
            return retVal;
        }

        public async Task<bool> UninstallPackageAsync(string packageName){
            return await UninstallPackageAsync(packageName, false);
        }

        public async Task<bool> UninstallGlobalPackageAsync(string packageName){
            return await UninstallPackageAsync(packageName, true);
        }

        public async Task<IList<IPackage>> SearchAsync(string searchText){
            IList<IPackage> results = null;
            try{
                _command = new NpmSearchCommand(
                    _npmController.FullPathToRootPackageDirectory,
                    searchText,
                    _npmController.PathToNpm,
                    _npmController.UseFallbackIfNpmNotFound);
                var success = await _command.ExecuteAsync();
                FireLogEvents(_command);
                if (success){
                    results = (_command as NpmSearchCommand).Results;
                }
            } catch (Exception e){
                OnExceptionLogged(e);
            } finally{
                OnCommandCompleted();
            }
            return results ?? new List<IPackage>();
        }

        public async Task<IList<IPackage>> GetCatalogueAsync(bool forceDownload){
            IList<IPackage> results = null;
            try
            {
                _command = new NpmGetCatalogueCommand(
                    _npmController.FullPathToRootPackageDirectory,
                    forceDownload,
                    _npmController.PathToNpm,
                    _npmController.UseFallbackIfNpmNotFound);
                var success = await _command.ExecuteAsync();
                FireLogEvents(_command);
                if (success)
                {
                    results = (_command as NpmSearchCommand).Results;
                }
            }
            catch (Exception e)
            {
                OnExceptionLogged(e);
            }
            finally
            {
                OnCommandCompleted();
            }
            return results ?? new List<IPackage>();
        }

        public async Task<bool> UpdatePackagesAsync(){
            return await UpdatePackagesAsync(new List<IPackage>());
        }

        public async Task<bool> UpdatePackagesAsync(IEnumerable<IPackage> packages){
            bool success = false;
            try{
                _command = new NpmUpdateCommand(
                    _npmController.FullPathToRootPackageDirectory,
                    packages,
                    _npmController.PathToNpm,
                    _npmController.UseFallbackIfNpmNotFound);
                success = await _command.ExecuteAsync();
                FireLogEvents(_command);
                _npmController.Refresh();
            } catch (Exception e){
                OnExceptionLogged(e);
            } finally{
                OnCommandCompleted();
            }
            return success;
        }
    }
}