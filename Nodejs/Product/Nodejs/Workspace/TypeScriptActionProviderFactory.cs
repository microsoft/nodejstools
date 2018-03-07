// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;
using Microsoft.VisualStudio.Workspace.Extensions;
using Microsoft.VisualStudio.Workspace.Extensions.VS;

namespace Microsoft.NodejsTools.Workspace
{
    [ExportFileContextActionProvider(
        (FileContextActionProviderOptions)SolutionWorkspaceProviderOptions.Supported,
        ProviderType,
        ProviderPriority.Normal,
        BuildContextTypes.BuildContextType)]
    public sealed class TypeScriptActionProviderFactory : IWorkspaceProviderFactory<IFileContextActionProvider>, IVsCommandActionProvider
    {
        public const string ProviderType = "F8C470E5-55A3-498C-80B8-DA2674A82B88";

        public IFileContextActionProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new FileContextActionProvider(workspaceContext);
        }

        public IReadOnlyCollection<CommandID> GetSupportedVsCommands()
        {
            return new[] 
            {
                new CommandID(Guids.GuidWorkspaceExplorerBuildActionCmdSet, PkgCmdId.cmdid_BuildActionContext),
            };
        }

        private sealed class FileContextActionProvider : IFileContextActionProvider
        {
            private readonly IWorkspace workspaceContext;

            public FileContextActionProvider(IWorkspace workspaceContext)
            {
                this.workspaceContext = workspaceContext;
            }

            public Task<IReadOnlyList<IFileContextAction>> GetActionsAsync(string filePath, FileContext fileContext, CancellationToken cancellationToken)
            {
                var actions = new List<IFileContextAction>
                {
                    new BuildFileContextAction(fileContext)
                };

                return Task.FromResult<IReadOnlyList<IFileContextAction>>(actions);
            }
        }

        private sealed class BuildFileContextAction : IFileContextAction, IVsCommandItem
        {
            public BuildFileContextAction(FileContext source)
            {
                this.Source = source;
            }

            public Guid CommandGroup => Guids.GuidWorkspaceExplorerBuildActionCmdSet;
            public uint CommandId => PkgCmdId.cmdid_BuildActionContext;
            public FileContext Source { get; }
            public string DisplayName => "";

            public Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken)
            {
                foreach (var input in this.Source.InputFiles)
                {
                    var outFile = Path.ChangeExtension(input, ".js");
                    File.Copy(input, outFile);
                }

                return Task.FromResult(CreateBuildProjectIncrementalResultFromBoolean(buildSucceeded: true));
            }

            private IFileContextActionResult CreateBuildProjectIncrementalResultFromBoolean(bool buildSucceeded)
            {
                // Assuming there is only project being compiled.
                return new BuildProjectIncrementalResult(
                    isSuccess: buildSucceeded,
                    succeeded: buildSucceeded ? 1 : 0,
                    failed: (!buildSucceeded) ? 1 : 0,
                    upToDate: 0);
            }
        }
    }
}
