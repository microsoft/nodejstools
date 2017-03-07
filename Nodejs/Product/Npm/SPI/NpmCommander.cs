// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NpmCommander : AbstractNpmLogSource, INpmCommander
    {
        private class NmpCommandRunner : IDisposable
        {
            private NpmCommander _commander;
            private NpmCommand _command;

            public static async Task<bool> ExecuteAsync(NpmCommander commander, NpmCommand command)
            {
                using (var runner = new NmpCommandRunner(commander, command))
                {
                    return await runner._command.ExecuteAsync();
                }
            }

            private NmpCommandRunner(NpmCommander commander, NpmCommand command)
            {
                this._commander = commander;
                this._command = command;

                this._commander._executingCommand = command;

                this._command.CommandStarted += this._commander.command_CommandStarted;
                this._command.OutputLogged += this._commander.command_OutputLogged;
                this._command.CommandCompleted += this._commander.command_CommandCompleted;

                this._command.ErrorLogged += this._commander.command_ErrorLogged;
                this._command.ExceptionLogged += this._commander.command_ExceptionLogged;
            }

            public void Dispose()
            {
                this._commander._executingCommand = null;

                this._command.CommandStarted -= this._commander.command_CommandStarted;
                this._command.OutputLogged -= this._commander.command_OutputLogged;
                this._command.CommandCompleted -= this._commander.command_CommandCompleted;

                this._command.ErrorLogged -= this._commander.command_ErrorLogged;
                this._command.ExceptionLogged -= this._commander.command_ExceptionLogged;
            }
        }

        private NpmController _npmController;
        private NpmCommand _executingCommand;
        private bool _disposed;

        public NpmCommander(NpmController controller)
        {
            this._npmController = controller;
            CommandStarted += this._npmController.LogCommandStarted;
            OutputLogged += this._npmController.LogOutput;
            ErrorLogged += this._npmController.LogError;
            ExceptionLogged += this._npmController.LogException;
            CommandCompleted += this._npmController.LogCommandCompleted;
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this._disposed = true;
                CommandStarted -= this._npmController.LogCommandStarted;
                OutputLogged -= this._npmController.LogOutput;
                ErrorLogged -= this._npmController.LogError;
                ExceptionLogged -= this._npmController.LogException;
                CommandCompleted -= this._npmController.LogCommandCompleted;
            }
        }

        public void CancelCurrentCommand()
        {
            if (null != this._executingCommand)
            {
                this._executingCommand.CancelCurrentTask();
            }
        }

        private void command_CommandStarted(object sender, EventArgs e)
        {
            OnCommandStarted();
        }

        private void command_ExceptionLogged(object sender, NpmExceptionEventArgs e)
        {
            OnExceptionLogged(e.Exception);
        }

        private void command_ErrorLogged(object sender, NpmLogEventArgs e)
        {
            OnErrorLogged(e.LogText);
        }

        private void command_OutputLogged(object sender, NpmLogEventArgs e)
        {
            OnOutputLogged(e.LogText);
        }

        private void command_CommandCompleted(object sender, NpmCommandCompletedEventArgs e)
        {
            OnCommandCompleted(e.Arguments, e.WithErrors, e.Cancelled);
        }

        private async Task<bool> DoCommandExecute(bool refreshNpmController, NpmCommand command)
        {
            Debug.Assert(this._executingCommand == null, "Attempting to execute multiple commands at the same time.");
            try
            {
                bool success = await NmpCommandRunner.ExecuteAsync(this, command);
                if (refreshNpmController)
                {
                    this._npmController.Refresh();
                }
                return success;
            }
            catch (Exception e)
            {
                OnOutputLogged(e.ToString());
            }
            return false;
        }

        public async Task<bool> Install()
        {
            return await DoCommandExecute(true,
                new NpmInstallCommand(
                    this._npmController.FullPathToRootPackageDirectory,
                    this._npmController.PathToNpm));
        }

        private Task<bool> InstallPackageByVersionAsync(
            string pathToRootDirectory,
            string packageName,
            string versionRange,
            DependencyType type,
            bool global,
            bool saveToPackageJson)
        {
            return DoCommandExecute(true,
                new NpmInstallCommand(
                    pathToRootDirectory,
                    packageName,
                    versionRange,
                    type,
                    global,
                    saveToPackageJson,
                    this._npmController.PathToNpm));
        }

        public Task<bool> InstallPackageByVersionAsync(
            string packageName,
            string versionRange,
            DependencyType type,
            bool saveToPackageJson)
        {
            return InstallPackageByVersionAsync(this._npmController.FullPathToRootPackageDirectory, packageName, versionRange, type, false, saveToPackageJson);
        }

        public Task<bool> InstallPackageToFolderByVersionAsync(string pathToRootDirectory, string packageName, string versionRange, bool saveToPackageJson)
        {
            return InstallPackageByVersionAsync(pathToRootDirectory, packageName, versionRange, DependencyType.Standard, false, saveToPackageJson);
        }

        private DependencyType GetDependencyType(string packageName)
        {
            var type = DependencyType.Standard;
            var root = this._npmController.RootPackage;
            if (null != root)
            {
                var match = root.Modules[packageName];
                if (null != match)
                {
                    if (match.IsDevDependency)
                    {
                        type = DependencyType.Development;
                    }
                    else if (match.IsOptionalDependency)
                    {
                        type = DependencyType.Optional;
                    }
                }
            }
            return type;
        }

        public async Task<bool> UninstallPackageAsync(string packageName)
        {
            return await DoCommandExecute(true,
                new NpmUninstallCommand(
                    this._npmController.FullPathToRootPackageDirectory,
                    packageName,
                    GetDependencyType(packageName),
                    false,
                    this._npmController.PathToNpm));
        }

        public async Task<IPackageCatalog> GetCatalogAsync(bool forceDownload, IProgress<string> progress)
        {
            var command = new NpmGetCatalogCommand(
                this._npmController.FullPathToRootPackageDirectory,
                this._npmController.CachePath,
                forceDownload,
                pathToNpm: this._npmController.PathToNpm,
                progress: progress);
            await DoCommandExecute(false, command);
            return (command as NpmGetCatalogCommand).Catalog;
        }

        public async Task<bool> UpdatePackagesAsync()
        {
            return await UpdatePackagesAsync(new List<IPackage>());
        }

        public async Task<bool> UpdatePackagesAsync(IEnumerable<IPackage> packages)
        {
            return await DoCommandExecute(true,
                new NpmUpdateCommand(
                    this._npmController.FullPathToRootPackageDirectory,
                    packages,
                    false,
                    this._npmController.PathToNpm));
        }

        public async Task<bool> ExecuteNpmCommandAsync(string arguments)
        {
            return await DoCommandExecute(true,
                new GenericNpmCommand(
                    this._npmController.FullPathToRootPackageDirectory,
                    arguments,
                    this._npmController.PathToNpm));
        }
    }
}

