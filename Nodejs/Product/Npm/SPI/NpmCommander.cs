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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmCommander : AbstractNpmLogSource, INpmCommander {
        private class NmpCommandRunner : IDisposable {
            private NpmCommander _commander;
            private NpmCommand _command;

            public static async Task<bool> ExecuteAsync(NpmCommander commander, NpmCommand command) {
                using (var runner = new NmpCommandRunner(commander, command)) {
                    return await runner._command.ExecuteAsync();
                }
            }

            private NmpCommandRunner(NpmCommander commander, NpmCommand command) {
                this._commander = commander;
                this._command = command;

                _commander._executingCommand = command;

                _command.CommandStarted += _commander.command_CommandStarted;
                _command.OutputLogged += _commander.command_OutputLogged;
                _command.CommandCompleted += _commander.command_CommandCompleted;

                _command.ErrorLogged += _commander.command_ErrorLogged;
                _command.ExceptionLogged += _commander.command_ExceptionLogged;
            }

            public void Dispose() {
                _commander._executingCommand = null;

                _command.CommandStarted -= _commander.command_CommandStarted;
                _command.OutputLogged -= _commander.command_OutputLogged;
                _command.CommandCompleted -= _commander.command_CommandCompleted;

                _command.ErrorLogged -= _commander.command_ErrorLogged;
                _command.ExceptionLogged -= _commander.command_ExceptionLogged;
            }
        }

        private NpmController _npmController;
        private NpmCommand _executingCommand;
        private bool _disposed;

        public NpmCommander(NpmController controller) {
            _npmController = controller;
            CommandStarted += _npmController.LogCommandStarted;
            OutputLogged += _npmController.LogOutput;
            ErrorLogged += _npmController.LogError;
            ExceptionLogged += _npmController.LogException;
            CommandCompleted += _npmController.LogCommandCompleted;
        }

        public void Dispose() {
            if (!_disposed) {
                _disposed = true;
                CommandStarted -= _npmController.LogCommandStarted;
                OutputLogged -= _npmController.LogOutput;
                ErrorLogged -= _npmController.LogError;
                ExceptionLogged -= _npmController.LogException;
                CommandCompleted -= _npmController.LogCommandCompleted;
            }
        }

        public void CancelCurrentCommand() {
            if (null != _executingCommand) {
                _executingCommand.CancelCurrentTask();
            }
        }

        private void command_CommandStarted(object sender, EventArgs e) {
            OnCommandStarted();
        }

        private void command_ExceptionLogged(object sender, NpmExceptionEventArgs e) {
            OnExceptionLogged(e.Exception);
        }

        private void command_ErrorLogged(object sender, NpmLogEventArgs e) {
            OnErrorLogged(e.LogText);
        }

        private void command_OutputLogged(object sender, NpmLogEventArgs e) {
            OnOutputLogged(e.LogText);
        }

        private void command_CommandCompleted(object sender, NpmCommandCompletedEventArgs e) {
            OnCommandCompleted(e.Arguments, e.WithErrors, e.Cancelled);
        }

        private async Task<bool> DoCommandExecute(bool refreshNpmController, NpmCommand command) {
            Debug.Assert(_executingCommand == null, "Attempting to execute multiple commands at the same time.");
            try {
                bool success = await NmpCommandRunner.ExecuteAsync(this, command);
                if (refreshNpmController) {
                    _npmController.Refresh();
                }
                return success;
            } catch (Exception e) {
                OnOutputLogged(e.ToString());
            }
            return false;
        }

        public async Task<bool> Install() {
            return await DoCommandExecute(true,
                new NpmInstallCommand(
                    _npmController.FullPathToRootPackageDirectory,
                    _npmController.PathToNpm));
        }

        private Task<bool> InstallPackageByVersionAsync(
            string pathToRootDirectory,
            string packageName,
            string versionRange,
            DependencyType type,
            bool global,
            bool saveToPackageJson) {
            return DoCommandExecute(true,
                new NpmInstallCommand(
                    pathToRootDirectory,
                    packageName,
                    versionRange,
                    type,
                    global,
                    saveToPackageJson,
                    _npmController.PathToNpm));
        }

        public Task<bool> InstallPackageByVersionAsync(
            string packageName,
            string versionRange,
            DependencyType type,
            bool saveToPackageJson) {
            return InstallPackageByVersionAsync(_npmController.FullPathToRootPackageDirectory, packageName, versionRange, type, false, saveToPackageJson);
        }

        public Task<bool> InstallPackageToFolderByVersionAsync(string pathToRootDirectory, string packageName, string versionRange, bool saveToPackageJson) {
            return InstallPackageByVersionAsync(pathToRootDirectory, packageName, versionRange, DependencyType.Standard, false, saveToPackageJson);
        }

        private DependencyType GetDependencyType(string packageName) {
            var type = DependencyType.Standard;
            var root = _npmController.RootPackage;
            if (null != root) {
                var match = root.Modules[packageName];
                if (null != match) {
                    if (match.IsDevDependency) {
                        type = DependencyType.Development;
                    } else if (match.IsOptionalDependency) {
                        type = DependencyType.Optional;
                    }
                }
            }
            return type;
        }

        public async Task<bool> UninstallPackageAsync(string packageName) {
            return await DoCommandExecute(true,
                new NpmUninstallCommand(
                    _npmController.FullPathToRootPackageDirectory,
                    packageName,
                    GetDependencyType(packageName),
                    false,
                    _npmController.PathToNpm));
        }

        public async Task<IPackageCatalog> GetCatalogAsync(bool forceDownload, IProgress<string> progress) {
            var command = new NpmGetCatalogCommand(
                _npmController.FullPathToRootPackageDirectory,
                _npmController.CachePath,
                forceDownload,
                pathToNpm:_npmController.PathToNpm,
                progress: progress);
            await DoCommandExecute(false, command);
            return (command as NpmGetCatalogCommand).Catalog;
        }

        public async Task<bool> UpdatePackagesAsync() {
            return await UpdatePackagesAsync(new List<IPackage>());
        }

        public async Task<bool> UpdatePackagesAsync(IEnumerable<IPackage> packages) {
            return await DoCommandExecute(true,
                new NpmUpdateCommand(
                    _npmController.FullPathToRootPackageDirectory,
                    packages,
                    false,
                    _npmController.PathToNpm));
        }

        public async Task<bool> ExecuteNpmCommandAsync(string arguments) {
            return await DoCommandExecute(true,
                new GenericNpmCommand(
                    _npmController.FullPathToRootPackageDirectory,
                    arguments,
                    _npmController.PathToNpm));
        }
    }
}