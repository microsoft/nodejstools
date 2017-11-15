// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.NpmUI;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Extensions.VS;

namespace Microsoft.NodejsTools.Workspace
{
    [ExportFileContextActionProvider(ProviderType, Guids.PackageJsonContextTypeString)]
    public class NpmActionProviderFactory : IWorkspaceProviderFactory<IFileContextActionProvider>
    {
        private const string ProviderType = "{971EA159-8F0F-44FF-A279-B11DF1D7F319}";

        public IFileContextActionProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new FileContextActionProvider(workspaceContext);
        }

        private class FileContextActionProvider : IFileContextActionProvider
        {
            private IWorkspace workspaceContext;

            public FileContextActionProvider(IWorkspace workspaceContext)
            {
                this.workspaceContext = workspaceContext;
            }

            public Task<IReadOnlyList<IFileContextAction>> GetActionsAsync(string filePath, FileContext fileContext, CancellationToken cancellationToken)
            {
                return Task.FromResult<IReadOnlyList<IFileContextAction>>(new IFileContextAction[] {
                    new InstallMissingNpmPackagesAction(filePath, fileContext),
                    new InstallNewNpmPackagesAction(filePath, fileContext),
                    new UpdateNpmPackagesAction(filePath, fileContext)
                });
            }
        }

        private class InstallMissingNpmPackagesAction : NpmPackagesAction
        {
            public InstallMissingNpmPackagesAction(string packageJsonPath, FileContext fileContext) :
                base(packageJsonPath, fileContext, PkgCmdId.cmdidWorkSpaceNpmInstallMissing)
            {
            }

            public override async Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken)
            {
                var projectHome = Path.GetDirectoryName(this.PackageJsonPath);

                var npmController = NpmControllerFactory.Create(
                      projectHome,
                      NodejsConstants.NpmCachePath);

                using (var commander = npmController.CreateNpmCommander())
                {
                    var result = await commander.Install();
                    return new FileContextActionResult(result);
                }
            }
        }

        private class InstallNewNpmPackagesAction : NpmPackagesAction
        {
            public InstallNewNpmPackagesAction(string packageJsonPath, FileContext fileContext) :
                base(packageJsonPath, fileContext, PkgCmdId.cmdidWorkSpaceNpmInstallNew)
            {
            }

            public override Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken)
            {
                var projectHome = Path.GetDirectoryName(this.PackageJsonPath);

                var npmController = NpmControllerFactory.Create(
                      projectHome,
                      NodejsConstants.NpmCachePath);

                using (var npmWorker = new NpmWorker(npmController))
                using (var manager = new NpmPackageInstallWindow(npmController, npmWorker))
                {
                    manager.ShowModal();
                }

                return Task.FromResult<IFileContextActionResult>(new FileContextActionResult(true));
            }
        }

        private class UpdateNpmPackagesAction : NpmPackagesAction
        {
            public UpdateNpmPackagesAction(string packageJsonPath, FileContext fileContext) :
                base(packageJsonPath, fileContext, PkgCmdId.cmdidWorkSpaceNpmUpdate)
            {
            }

            public override async Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken)
            {
                var projectHome = Path.GetDirectoryName(this.PackageJsonPath);

                var npmController = NpmControllerFactory.Create(
                      projectHome,
                      NodejsConstants.NpmCachePath);

                using (var commander = npmController.CreateNpmCommander())
                {
                    var result = await commander.UpdatePackagesAsync();
                    return new FileContextActionResult(result);
                }
            }
        }
    }

    public abstract class NpmPackagesAction : IFileContextAction, IVsCommandItem
    {
        public NpmPackagesAction(string packageJsonPath, FileContext source, uint commandId)
        {
            this.Source = source;
            this.CommandId = commandId;
            this.PackageJsonPath = packageJsonPath;
        }

        public FileContext Source { get; }

        public string PackageJsonPath { get; }

        // Unused, since we also implement IVsCommandItem the Display Name used comes from the vsct file
        public string DisplayName => "";

        public Guid CommandGroup { get; } = Guids.NodeToolsWorkspaceCmdSet;

        public uint CommandId { get; }

        public abstract Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken);
    }
}
