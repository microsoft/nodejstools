// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.NpmUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.VSIntegration.UI;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Workspace
{
    [Export(typeof(INodeExtender))]
    public sealed class ContextMenuProvider : INodeExtender
    {
        private readonly IWorkspaceCommandHandler npmHandler;

        [ImportingConstructor]
        public ContextMenuProvider(OutputPaneWrapper outputPane)
        {
            this.npmHandler = new NpmCommandHandler(outputPane);
        }

        public IChildrenSource ProvideChildren(WorkspaceVisualNodeBase parentNode)
        {
            // We don't provide new children so return null
            return null;
        }

        public IWorkspaceCommandHandler ProvideCommandHandler(WorkspaceVisualNodeBase parentNode)
        {
            if (EnsurePackageJson(parentNode))
            {
                return this.npmHandler;
            }

            // we only have a command handler for 'package.json' files
            return null;
        }

        private static bool EnsurePackageJson(WorkspaceVisualNodeBase node)
        {
            return (node is IFileNode fileNode && PackageJsonFactory.IsPackageJsonFile(fileNode.FileName));
        }

        private sealed class NpmCommandHandler : IWorkspaceCommandHandler
        {
            private readonly OutputPaneWrapper outputPane;
            private static readonly NullBuildProgressUpdater DefaultBuildProgressUpdater = new NullBuildProgressUpdater();

            public NpmCommandHandler(OutputPaneWrapper outputPane)
            {
                this.outputPane = outputPane;
            }

            public int Priority => 1000;

            public bool IgnoreOnMultiselect => true;

            public int Exec(List<WorkspaceVisualNodeBase> selection, Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
            {
                if (selection.Count != 1 || !EnsurePackageJson(selection.Single()))
                {
                    return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
                }

                var node = selection.Single();

                if (pguidCmdGroup == Guids.NodeToolsWorkspaceCmdSet)
                {
                    this.outputPane.InitializeOutputPanes();

                    var fileNode = ((IFileNode)node).FullPath;

                    switch (nCmdID)
                    {
                        case PkgCmdId.cmdidWorkSpaceNpmInstallMissing:
                            ExecNpmInstallMissing(fileNode);
                            return VSConstants.S_OK;

                        case PkgCmdId.cmdidWorkSpaceNpmInstallNew:
                            ExecNpmInstallNew(fileNode);
                            return VSConstants.S_OK;

                        case PkgCmdId.cmdidWorkSpaceNpmUpdate:
                            ExecNpmUpdate(fileNode);
                            return VSConstants.S_OK;
                    }

                    if (nCmdID >= PkgCmdId.cmdidWorkSpaceNpmDynamicScript && nCmdID < PkgCmdId.cmdidWorkSpaceNpmDynamicScriptMax)
                    {
                        ExecDynamic(fileNode, nCmdID);
                        return VSConstants.S_OK;
                    }
                }

                if (pguidCmdGroup == Guids.WorkspaceExplorerDebugActionCmdSet)
                {
                    switch (nCmdID)
                    {
                        // for package.json we only support Debug
                        case PkgCmdId.cmdid_DebugActionContext:
                            ExecDebugAsync(node);
                            return VSConstants.S_OK;
                    }
                }

                return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
            }

            // Note: all the Exec commands are async, this allows us to call them in a fire and forget
            // pattern, without blocking the UI or losing any logging

            private async void ExecNpmInstallMissing(string filePath)
            {
                using (var npmController = this.CreateController(filePath))
                using (var commander = npmController.CreateNpmCommander())
                {
                    await commander.Install();
                }
            }

            private void ExecNpmInstallNew(string filePath)
            {
                using (var npmController = this.CreateController(filePath))
                using (var npmWorker = new NpmWorker(npmController))
                using (var manager = new NpmPackageInstallWindow(npmController, npmWorker))
                {
                    manager.ShowModal();
                }
            }

            private async void ExecNpmUpdate(string filePath)
            {
                using (var npmController = this.CreateController(filePath))
                using (var commander = npmController.CreateNpmCommander())
                {
                    await commander.UpdatePackagesAsync();
                }
            }

            private async void ExecDynamic(string filePath, uint nCmdID)
            {
                // Unfortunately the NpmController (and NpmCommander), used for the install and update commands
                // doesn't support running arbitrary scripts. And changing that is outside
                // the scope of these changes.
                if (TryGetCommand(nCmdID, filePath, out var commandName))
                {
                    using (var npmController = this.CreateController(filePath))
                    using (var commander = npmController.CreateNpmCommander())
                    {
                        await commander.ExecuteNpmCommandAsync($"run-script {commandName}", showConsole: true);
                    }
                }
            }

            private async void ExecDebugAsync(WorkspaceVisualNodeBase node)
            {
                var workspace = node.Workspace;
                var packageJson = PackageJsonFactory.Create(((IFileNode)node).FullPath);

                if (string.IsNullOrEmpty(packageJson.Main))
                {
                    return;
                }

                //invoke debuglaunchtargetprovider on this file
                var fileContextActions = await workspace.GetFileContextActionsAsync(packageJson.Main, new[] { DebugLaunchActionContext.ContextTypeGuid });
                if (fileContextActions.Any())
                {
                    // we requested a single context, so there should be a single grouping. Use the First action, since they're ordered by priority.
                    var action = fileContextActions.Single().FirstOrDefault();
                    Debug.Assert(action != null, "Why is action null, when we did get a fileContextActions?");
                    await action.ExecuteAsync(DefaultBuildProgressUpdater, CancellationToken.None);
                }
            }

            public bool QueryStatus(List<WorkspaceVisualNodeBase> selection, Guid pguidCmdGroup, uint nCmdID, ref uint cmdf, ref string customTitle)
            {
                if (selection.Count != 1 || !EnsurePackageJson(selection.Single()))
                {
                    return false;
                }

                var node = (IFileNode)selection.Single();

                if (pguidCmdGroup == Guids.NodeToolsWorkspaceCmdSet)
                {
                    switch (nCmdID)
                    {
                        // we can always install missing, new, and update npm packages
                        case PkgCmdId.cmdidWorkSpaceNpmInstallMissing:
                        case PkgCmdId.cmdidWorkSpaceNpmInstallNew:
                        case PkgCmdId.cmdidWorkSpaceNpmUpdate:
                            cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            return true;
                    }

                    // Each id we return true for, this method is called with id + 1
                    // this way we can add each script to the context menu
                    if (nCmdID >= PkgCmdId.cmdidWorkSpaceNpmDynamicScript && nCmdID <= PkgCmdId.cmdidWorkSpaceNpmDynamicScriptMax)
                    {
                        if (QueryDynamic(nCmdID, node.FullPath, ref customTitle))
                        {
                            cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            return true;
                        }
                    }
                }

                if (pguidCmdGroup == Guids.WorkspaceExplorerDebugActionCmdSet)
                {
                    switch (nCmdID)
                    {
                        // for package.json we only support Debug
                        case PkgCmdId.cmdid_DebugActionContext:
                            if (QueryDebug(node.FullPath))
                            {
                                cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                                return true;
                            }
                            break;
                        default:
                            break;
                    }
                }

                return false;
            }

            private bool QueryDynamic(uint nCmdID, string filePath, ref string customTitle)
            {
                if (TryGetCommand(nCmdID, filePath, out var commandName))
                {
                    customTitle = $"npm run-script {commandName}";
                    return true;
                }
                return false;
            }

            private bool QueryDebug(string filePath)
            {
                var packageJson = PackageJsonFactory.Create(filePath);

                return !string.IsNullOrEmpty(packageJson.Main);
            }

            private static bool TryGetCommand(uint nCmdID, string filePath, out string commandName)
            {
                var index = nCmdID - PkgCmdId.cmdidWorkSpaceNpmDynamicScript;
                var packageJson = PackageJsonFactory.Create(filePath);

                Debug.Assert(packageJson != null, "Failed to create package.json");

                var scripts = packageJson.Scripts;
                if (index < scripts.Length)
                {
                    commandName = packageJson.Scripts[index].CommandName;
                    return true;
                }

                commandName = null;
                return false;
            }

            private INpmController CreateController(string packageJsonPath)
            {
                Debug.Assert(Path.IsPathRooted(packageJsonPath));
                Debug.Assert(PackageJsonFactory.IsPackageJsonFile(packageJsonPath));

                var projectHome = Path.GetDirectoryName(packageJsonPath);

                var npmController = NpmControllerFactory.Create(
                      projectHome,
                      NodejsConstants.NpmCachePath,
                      isProject: false);

                npmController.ErrorLogged += this.WriteNpmOutput;
                npmController.ExceptionLogged += this.WriteNpmException;
                npmController.OutputLogged += this.WriteNpmOutput;

                return npmController;
            }

            private void WriteNpmException(object sender, NpmExceptionEventArgs e)
            {
                this.outputPane.WriteLine(ErrorHelper.GetExceptionDetailsText(e.Exception));
            }

            private void WriteNpmOutput(object sender, NpmLogEventArgs e)
            {
                if (!string.IsNullOrWhiteSpace(e.LogText))
                {
                    this.outputPane.WriteLine(e.LogText.TrimEnd('\r', '\n'));
                }
            }

            private sealed class NullBuildProgressUpdater : IProgress<IFileContextActionProgressUpdate>
            {
                void IProgress<IFileContextActionProgressUpdate>.Report(IFileContextActionProgressUpdate value)
                {
                    // unused in our scenario
                }
            }
        }
    }
}
