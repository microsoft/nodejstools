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
            private NpmCommander commander;
            private NpmCommand command;

            public static async Task<bool> ExecuteAsync(NpmCommander commander, NpmCommand command)
            {
                using (var runner = new NmpCommandRunner(commander, command))
                {
                    return await runner.command.ExecuteAsync();
                }
            }

            private NmpCommandRunner(NpmCommander commander, NpmCommand command)
            {
                this.commander = commander;
                this.command = command;

                this.commander.executingCommand = command;

                this.command.CommandStarted += this.commander.command_CommandStarted;
                this.command.OutputLogged += this.commander.command_OutputLogged;
                this.command.CommandCompleted += this.commander.command_CommandCompleted;

                this.command.ErrorLogged += this.commander.command_ErrorLogged;
                this.command.ExceptionLogged += this.commander.command_ExceptionLogged;
            }

            public void Dispose()
            {
                this.commander.executingCommand = null;

                this.command.CommandStarted -= this.commander.command_CommandStarted;
                this.command.OutputLogged -= this.commander.command_OutputLogged;
                this.command.CommandCompleted -= this.commander.command_CommandCompleted;

                this.command.ErrorLogged -= this.commander.command_ErrorLogged;
                this.command.ExceptionLogged -= this.commander.command_ExceptionLogged;
            }
        }

        private readonly NpmController npmController;
        private NpmCommand executingCommand;
        private bool disposed;

        public NpmCommander(NpmController controller)
        {
            this.npmController = controller;

            CommandStarted += this.npmController.LogCommandStarted;
            OutputLogged += this.npmController.LogOutput;
            ErrorLogged += this.npmController.LogError;
            ExceptionLogged += this.npmController.LogException;
            CommandCompleted += this.npmController.LogCommandCompleted;
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                CommandStarted -= this.npmController.LogCommandStarted;
                OutputLogged -= this.npmController.LogOutput;
                ErrorLogged -= this.npmController.LogError;
                ExceptionLogged -= this.npmController.LogException;
                CommandCompleted -= this.npmController.LogCommandCompleted;
            }
        }

        private void command_CommandStarted(object sender, NpmCommandStartedEventArgs e)
        {
            OnCommandStarted(e.Arguments);
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
            Debug.Assert(this.executingCommand == null, "Attempting to execute multiple commands at the same time.");
            try
            {
                var success = await NmpCommandRunner.ExecuteAsync(this, command);
                if (refreshNpmController)
                {
                    this.npmController.Refresh();
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
                    this.npmController.FullPathToRootPackageDirectory,
                    this.npmController.PathToNpm));
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
                    this.npmController.PathToNpm));
        }

        public Task<bool> InstallPackageByVersionAsync(
            string packageName,
            string versionRange,
            DependencyType type,
            bool saveToPackageJson)
        {
            return InstallPackageByVersionAsync(this.npmController.FullPathToRootPackageDirectory, packageName, versionRange, type, false, saveToPackageJson);
        }

        private DependencyType GetDependencyType(string packageName)
        {
            var type = DependencyType.Standard;
            var root = this.npmController.RootPackage;
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
                    this.npmController.FullPathToRootPackageDirectory,
                    packageName,
                    GetDependencyType(packageName),
                    false,
                    this.npmController.PathToNpm));
        }

        public async Task<bool> UpdatePackagesAsync()
        {
            return await UpdatePackagesAsync(new List<IPackage>());
        }

        public async Task<bool> UpdatePackagesAsync(IEnumerable<IPackage> packages)
        {
            return await DoCommandExecute(true,
                new NpmUpdateCommand(
                    this.npmController.FullPathToRootPackageDirectory,
                    packages,
                    false,
                    this.npmController.PathToNpm));
        }

        public async Task<bool> ExecuteNpmCommandAsync(string arguments, bool showConsole)
        {
            return await DoCommandExecute(true,
                new GenericNpmCommand(
                    this.npmController.FullPathToRootPackageDirectory,
                    arguments,
                    showConsole,
                    this.npmController.PathToNpm));
        }
    }
}
