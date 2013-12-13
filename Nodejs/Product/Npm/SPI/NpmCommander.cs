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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmCommander : AbstractNpmLogSource, INpmCommander {
        private NpmController _npmController;
        private NpmCommand _command;
        private bool _disposed;

        public NpmCommander(NpmController controller) {
            _npmController = controller;
            OutputLogged += _npmController.LogOutput;
            ErrorLogged += _npmController.LogError;
            ExceptionLogged += _npmController.LogException;
        }

        public void Dispose() {
            if (!_disposed) {
                _disposed = true;
                OutputLogged -= _npmController.LogOutput;
                ErrorLogged -= _npmController.LogError;
                ExceptionLogged -= _npmController.LogException;
            }
        }

        public event EventHandler CommandCompleted;

        private void OnCommandCompleted() {
            var handlers = CommandCompleted;
            if (null != handlers) {
                handlers(this, new EventArgs());
            }
        }

        public void CancelCurrentCommand() {
            if (null != _command) {
                _command.CancelCurrentTask();
            }
        }

        ////  TODO: events should be fired as data is logged, not in one massive barf at the end
        //private void FireLogEvents(NpmCommand command) {
        //    //  Filter this out because we ony using search to return the entire module catalogue,
        //    //  which will spew 47,000+ lines of total guff that the user probably isn't interested
        //    //  in to the npm log in the output window.
        //    if (command is NpmSearchCommand) {
        //        return;
        //    }
        //    OnOutputLogged(command.StandardOutput);
        //    OnErrorLogged(command.StandardError);
        //}

        void command_ExceptionLogged(object sender, NpmExceptionEventArgs e)
        {
            OnExceptionLogged(e.Exception);
        }

        void command_ErrorLogged(object sender, NpmLogEventArgs e)
        {
            OnErrorLogged(e.LogText);
        }

        void command_OutputLogged(object sender, NpmLogEventArgs e)
        {
            OnOutputLogged(e.LogText);
        }

        private void RegisterLogEvents(NpmCommand command)
        {
            if (command is NpmSearchCommand || command is NpmGetCatalogueCommand){
                return;
            }

            command.OutputLogged += command_OutputLogged;
            command.ErrorLogged += command_ErrorLogged;
            command.ExceptionLogged += command_ExceptionLogged;
        }

        private void UnregisterLogEvents(NpmCommand command){
            if (command is NpmSearchCommand || command is NpmGetCatalogueCommand)
            {
                return;
            }

            command.OutputLogged -= command_OutputLogged;
            command.ErrorLogged -= command_ErrorLogged;
            command.ExceptionLogged -= command_ExceptionLogged;
        }

        private async Task<bool> DoCommandExecute(bool refreshNpmController){
            RegisterLogEvents(_command);
            bool success = await _command.ExecuteAsync();
            UnregisterLogEvents(_command);
            _npmController.Refresh();
            OnCommandCompleted();
            return success;
        }

        public async Task<bool> Install()
        {
            _command = new NpmInstallCommand(
                _npmController.FullPathToRootPackageDirectory,
                _npmController.PathToNpm,
                _npmController.UseFallbackIfNpmNotFound);
            return await DoCommandExecute(true);
        }

        private async Task<bool> InstallPackageByVersionAsync(
            string packageName,
            string versionRange,
            DependencyType type,
            bool global) {
            _command = new NpmInstallCommand(
                _npmController.FullPathToRootPackageDirectory,
                packageName,
                versionRange,
                type,
                global,
                _npmController.PathToNpm,
                _npmController.UseFallbackIfNpmNotFound);
            return await DoCommandExecute(true);
        }

        public async Task<bool> InstallPackageByVersionAsync(
            string packageName,
            string versionRange,
            DependencyType type) {
            return await InstallPackageByVersionAsync(packageName, versionRange, type, false);
        }

        public async Task<bool> InstallGlobalPackageByVersionAsync(string packageName, string versionRange) {
            return await InstallPackageByVersionAsync(packageName, versionRange, DependencyType.Standard, true);
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

        private async Task<bool> UninstallPackageAsync(string packageName, bool global) {
            _command = new NpmUninstallCommand(
                _npmController.FullPathToRootPackageDirectory,
                packageName,
                GetDependencyType(packageName),
                global,
                _npmController.PathToNpm,
                _npmController.UseFallbackIfNpmNotFound);
            return await DoCommandExecute(true);
        }

        public async Task<bool> UninstallPackageAsync(string packageName) {
            return await UninstallPackageAsync(packageName, false);
        }

        public async Task<bool> UninstallGlobalPackageAsync(string packageName) {
            return await UninstallPackageAsync(packageName, true);
        }

        public async Task<IList<IPackage>> SearchAsync(string searchText) {
            _command = new NpmSearchCommand(
                _npmController.FullPathToRootPackageDirectory,
                searchText,
                _npmController.PathToNpm,
                _npmController.UseFallbackIfNpmNotFound);
            var success = await DoCommandExecute(false);
            return success ? (_command as NpmSearchCommand).Results : new List<IPackage>();
        }

        public async Task<IPackageCatalog> GetCatalogueAsync(bool forceDownload) {
            _command = new NpmGetCatalogueCommand(
                _npmController.FullPathToRootPackageDirectory,
                forceDownload,
                _npmController.PathToNpm,
                _npmController.UseFallbackIfNpmNotFound);
            await DoCommandExecute(false);
            return (_command as NpmGetCatalogueCommand).Catalog;
        }

        public async Task<bool> UpdatePackagesAsync() {
            return await UpdatePackagesAsync(new List<IPackage>());
        }

        public async Task<bool> UpdatePackagesAsync(IEnumerable<IPackage> packages) {
            _command = new NpmUpdateCommand(
                _npmController.FullPathToRootPackageDirectory,
                packages,
                _npmController.PathToNpm,
                _npmController.UseFallbackIfNpmNotFound);
            return await DoCommandExecute(true);
        }
    }
}