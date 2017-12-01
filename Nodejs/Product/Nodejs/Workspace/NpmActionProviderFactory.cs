// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.NpmUI;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Extensions.VS;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Workspace
{
    [ExportFileContextActionProvider(ProviderType, Guids.PackageJsonContextTypeString)]
    public class NpmActionProviderFactory : IWorkspaceProviderFactory<IFileContextActionProvider>
    {
        private const string ProviderType = "{971EA159-8F0F-44FF-A279-B11DF1D7F319}";

        [Import]
        private OutputPaneWrapper OutputPane { get; set; }

        public IFileContextActionProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new FileContextActionProvider(workspaceContext, this.OutputPane);
        }

        private class FileContextActionProvider : IFileContextActionProvider
        {
            public readonly IWorkspace workspaceContext;
            public readonly OutputPaneWrapper outputPane;

            public FileContextActionProvider(IWorkspace workspaceContext, OutputPaneWrapper outputPane)
            {
                this.workspaceContext = workspaceContext;
                this.outputPane = outputPane;
            }

            public async Task<IReadOnlyList<IFileContextAction>> GetActionsAsync(string filePath, FileContext fileContext, CancellationToken cancellationToken)
            {
                // ensure we initialize the NPM Output window once, so we don't 
                // need to switch to the UI thread for each command
                await this.workspaceContext.JTF.SwitchToMainThreadAsync();
                this.outputPane.ShowWindow();

                return new IFileContextAction[] {
                    new InstallMissingNpmPackagesAction(filePath, fileContext, this.outputPane.WriteLine),
                    new InstallNewNpmPackagesAction(filePath, fileContext, this.outputPane.WriteLine),
                    new UpdateNpmPackagesAction(filePath, fileContext, this.outputPane.WriteLine)
                };
            }
        }

        private class InstallMissingNpmPackagesAction : NpmPackagesAction
        {
            public InstallMissingNpmPackagesAction(string packageJsonPath, FileContext fileContext, Action<string> writeLine) :
                base(packageJsonPath, fileContext, PkgCmdId.cmdidWorkSpaceNpmInstallMissing, writeLine)
            {
            }

            public override async Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken)
            {
                using (var npmController = this.CreateController())
                using (var commander = npmController.CreateNpmCommander())
                {
                    var result = await commander.Install();
                    return new FileContextActionResult(result);
                }
            }
        }

        private class InstallNewNpmPackagesAction : NpmPackagesAction
        {
            public InstallNewNpmPackagesAction(string packageJsonPath, FileContext fileContext, Action<string> writeLine) :
                base(packageJsonPath, fileContext, PkgCmdId.cmdidWorkSpaceNpmInstallNew, writeLine)
            {
            }

            public override Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken)
            {
                using (var npmController = this.CreateController())
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
            public UpdateNpmPackagesAction(string packageJsonPath, FileContext fileContext, Action<string> writeLine) :
                base(packageJsonPath, fileContext, PkgCmdId.cmdidWorkSpaceNpmUpdate, writeLine)
            {
            }

            public override async Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken)
            {
                using (var npmController = this.CreateController())
                using (var commander = npmController.CreateNpmCommander())
                {
                    var result = await commander.UpdatePackagesAsync();
                    return new FileContextActionResult(result);
                }
            }
        }

        private abstract class NpmPackagesAction : IFileContextAction, IVsCommandItem
        {
            public NpmPackagesAction(string packageJsonPath, FileContext source, uint commandId, Action<string> writeLine)
            {
                this.Source = source;
                this.CommandId = commandId;
                this.PackageJsonPath = packageJsonPath;
                this.WriteLine = writeLine;
            }

            public INpmController CreateController()
            {
                var projectHome = Path.GetDirectoryName(this.PackageJsonPath);

                var npmController = NpmControllerFactory.Create(
                      projectHome,
                      NodejsConstants.NpmCachePath);

                npmController.ErrorLogged += this.WriteNpmOutput;
                npmController.ExceptionLogged += this.WriteNpmException;
                npmController.OutputLogged += this.WriteNpmOutput;

                return npmController;
            }

            private void WriteNpmException(object sender, NpmExceptionEventArgs e)
            {
                this.WriteLine(ErrorHelper.GetExceptionDetailsText(e.Exception));
            }

            private void WriteNpmOutput(object sender, NpmLogEventArgs e)
            {
                if (!string.IsNullOrWhiteSpace(e.LogText))
                {
                    this.WriteLine(e.LogText.TrimEnd('\r', '\n'));
                }
            }

            protected readonly Action<string> WriteLine;

            public FileContext Source { get; }

            public string PackageJsonPath { get; }

            // Unused, since we also implement IVsCommandItem the Display Name used comes from the vsct file
            // Can't throw a NotImeplementedException, since that crashes the Open Folder scanner
            public string DisplayName => null;

            public Guid CommandGroup => Guids.NodeToolsWorkspaceCmdSet;

            public uint CommandId { get; }

            public abstract Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken);
        }
    }
}
