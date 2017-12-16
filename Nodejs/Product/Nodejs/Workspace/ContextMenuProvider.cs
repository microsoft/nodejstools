// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.NpmUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.VSIntegration.UI;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Workspace
{
    [Export(typeof(INodeExtender))]
    internal sealed class ContextMenuProvider : INodeExtender
    {
        private readonly IWorkspaceCommandHandler npmHandler;

        private readonly OutputPaneWrapper outputPane;

        [ImportingConstructor]
        public ContextMenuProvider(OutputPaneWrapper outputPane)
        {
            this.outputPane = outputPane;
            this.npmHandler = new NpmCommandHandler(this.outputPane);
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
                this.outputPane.ShowWindow();
                return this.npmHandler;
            }

            return null;
        }

        private static bool EnsurePackageJson(WorkspaceVisualNodeBase node)
        {
            return (node is IFileNode fileNode && StringComparer.OrdinalIgnoreCase.Equals(fileNode.FileName, "package.json"));
        }

        private sealed class NpmCommandHandler : IWorkspaceCommandHandler
        {
            private readonly OutputPaneWrapper outputPane;

            public NpmCommandHandler(OutputPaneWrapper outputPane)
            {
                this.outputPane = outputPane;
            }

            public int Priority => 1000;

            public bool IgnoreOnMultiselect => true;

            public int Exec(List<WorkspaceVisualNodeBase> selection, Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
            {
                var node = selection.FirstOrDefault();
                if (selection.Count != 1 || !EnsurePackageJson(node))
                {
                    return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
                }

                if (pguidCmdGroup == Guids.NodeToolsWorkspaceCmdSet)
                {
                    switch (nCmdID)
                    {
                        case PkgCmdId.cmdidWorkSpaceNpmInstallMissing:
                            ExecNpmInstallMissing(node);
                            return VSConstants.S_OK;

                        case PkgCmdId.cmdidWorkSpaceNpmInstallNew:
                            ExecNpmInstallNew(node);
                            return VSConstants.S_OK;

                        case PkgCmdId.cmdidWorkSpaceNpmUpdate:
                            ExecNpmUpdate(node);
                            return VSConstants.S_OK;
                    }

                    if (nCmdID >= PkgCmdId.cmdidWorkSpaceNpmDynamicScript && nCmdID < PkgCmdId.cmdidWorkSpaceNpmDynamicScriptMax)
                    {
                        ExecDynamic(node, nCmdID);
                        return VSConstants.S_OK;
                    }
                }

                return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
            }

            // Note: all the Exec commands are async, this allows us to call them in a fire and forget
            // pattern, without blocking the UI or losing any logging

            private async void ExecNpmInstallMissing(WorkspaceVisualNodeBase node)
            {
                using (var npmController = this.CreateController(node.Workspace))
                using (var commander = npmController.CreateNpmCommander())
                {
                    await commander.Install();
                }
            }

            private void ExecNpmInstallNew(WorkspaceVisualNodeBase node)
            {
                using (var npmController = this.CreateController(node.Workspace))
                using (var npmWorker = new NpmWorker(npmController))
                using (var manager = new NpmPackageInstallWindow(npmController, npmWorker))
                {
                    manager.ShowModal();
                }
            }

            private async void ExecNpmUpdate(WorkspaceVisualNodeBase node)
            {
                using (var npmController = this.CreateController(node.Workspace))
                using (var commander = npmController.CreateNpmCommander())
                {
                    await commander.UpdatePackagesAsync();
                }
            }

            private async void ExecDynamic(WorkspaceVisualNodeBase node, uint nCmdID)
            {
                // Unfortunately the NpmController (and NpmCommander), used for the install, update commands
                // doesn't support running arbitrary scripts. And changing that is outside
                // the scope of these changes.
                var filePath = ((IFileNode)node).FullPath;
                if (TryGetCommand(nCmdID, filePath, out var commandName))
                {
                    var npmPath = NpmHelpers.GetPathToNpm();

                    await NpmWorker.ExecuteNpmCommandAsync(
                        npmPath,
                        executionDirectory: Path.GetDirectoryName(filePath),
                        arguments: new[] { "run-script", commandName },
                        visible: true); // show the CMD window
                }
            }

            public bool QueryStatus(List<WorkspaceVisualNodeBase> selection, Guid pguidCmdGroup, uint nCmdID, ref uint cmdf, ref string customTitle)
            {
                if (selection.Count != 1 || !EnsurePackageJson(selection.Single()))
                {
                    return false;
                }

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
                        var node = (IFileNode)selection.First();

                        if (QueryDynamic(nCmdID, node.FullPath, ref customTitle))
                        {
                            cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            return true;
                        }
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

            private INpmController CreateController(IWorkspace workspace)
            {
                var projectHome = workspace.Location;

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
                this.outputPane.WriteLine(ErrorHelper.GetExceptionDetailsText(e.Exception));
            }

            private void WriteNpmOutput(object sender, NpmLogEventArgs e)
            {
                if (!string.IsNullOrWhiteSpace(e.LogText))
                {
                    this.outputPane.WriteLine(e.LogText.TrimEnd('\r', '\n'));
                }
            }
        }
    }
}
