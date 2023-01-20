// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.TypeScript;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;
using Microsoft.VisualStudio.Workspace.Extensions;
using Microsoft.VisualStudio.Workspace.Extensions.VS;
using Microsoft.VisualStudioTools.Project;

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

        private readonly OutputPaneWrapper outputPane;

        [ImportingConstructor]
        public TypeScriptActionProviderFactory(OutputPaneWrapper outputPane)
        {
            this.outputPane = outputPane;
        }

        public IFileContextActionProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new FileContextActionProvider(workspaceContext, this.outputPane);
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
            private readonly OutputPaneWrapper outputPane;

            public FileContextActionProvider(IWorkspace workspaceContext, OutputPaneWrapper outputPane)
            {
                this.workspaceContext = workspaceContext;
                this.outputPane = outputPane;
            }

            public async Task<IReadOnlyList<IFileContextAction>> GetActionsAsync(string filePath, FileContext fileContext, CancellationToken cancellationToken)
            {
                await this.workspaceContext.JTF.SwitchToMainThreadAsync();

                var actions = new List<IFileContextAction>();

                if (TypeScriptHelpers.IsTsJsConfigJsonFile(filePath))
                {
                    actions.Add(new BuildTsConfigContextAction(filePath, fileContext, this.outputPane));
                }
                else if (TypeScriptHelpers.IsTypeScriptFile(filePath))
                {
                    var tsconfig = await this.workspaceContext.IsContainedByTsConfig(filePath);

                    if (tsconfig == null)
                    {
                        actions.Add(new BuildTsFileContextAction(filePath, fileContext, this.outputPane));
                    }
                }

                if (actions.Count > 0)
                {
                    this.outputPane.InitializeOutputPanes();
                }

                return actions;
            }
        }

        private sealed class BuildTsFileContextAction : BuildFileContextAction, IFileContextAction, IVsCommandItem
        {
            public BuildTsFileContextAction(string filePath, FileContext fileContext, OutputPaneWrapper outputPane)
                : base(filePath, fileContext, outputPane)
            {
            }

            public override async Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken)
            {
                var result = await TypeScriptCompile.CompileFileAsync(this.FilePath, this.OutputPane);
                return CreateBuildProjectIncrementalResultFromBoolean(result);
            }
        }

        private sealed class BuildTsConfigContextAction : BuildFileContextAction, IFileContextAction, IVsCommandItem
        {
            public BuildTsConfigContextAction(string filePath, FileContext fileContext, OutputPaneWrapper outputPane)
                : base(filePath, fileContext, outputPane)
            {
            }

            public override async Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken)
            {
                var result = await TypeScriptCompile.CompileProjectAsync(this.FilePath, this.OutputPane);
                return CreateBuildProjectIncrementalResultFromBoolean(result);
            }
        }

        private abstract class BuildFileContextAction
        {
            public BuildFileContextAction(string filePath, FileContext fileContext, OutputPaneWrapper outputPane)
            {
                this.Source = fileContext;
                this.FilePath = filePath;
                this.OutputPane = outputPane;
            }

            public Guid CommandGroup => Guids.GuidWorkspaceExplorerBuildActionCmdSet;
            public uint CommandId => PkgCmdId.cmdid_BuildActionContext;
            public FileContext Source { get; }
            public string FilePath { get; }

            public OutputPaneWrapper OutputPane { get; }
            public string DisplayName => "Open Folder uses the name defined in .vsct file.";

            public abstract Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken);

            protected static IFileContextActionResult CreateBuildProjectIncrementalResultFromBoolean(bool buildSucceeded)
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
