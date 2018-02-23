// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.TypeScript;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.Extensions.Build;
using Microsoft.VisualStudio.Workspace.Extensions.VS;
using Microsoft.VisualStudio.Workspace.Extensions.VS.Debug;

namespace Microsoft.NodejsTools.Workspace
{
   [ExportFileContextProvider(ProviderType, BuildActionContext.ContextType, DebugLaunchActionContext.ContextType)]
    public class TypeScriptBuildContextProviderFactory : IWorkspaceProviderFactory<IFileContextProvider>
    {
        private const string ProviderType = "{71FC8D62-E001-4806-A319-BE897E1A603D}";

        [Import]
        private ILaunchDebugTargetProvider VsDebugLaunchTargetProvider { get; set; }

        public IFileContextProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new TypeScriptFileContextProvider(workspaceContext, this.VsDebugLaunchTargetProvider);
        }

        private class TypeScriptFileContextProvider : IFileContextProvider
        {
            private IWorkspace workspaceContext;
            private ILaunchDebugTargetProvider vsDebugLaunchTargetProvider;

            public TypeScriptFileContextProvider(IWorkspace workspaceContext, ILaunchDebugTargetProvider vsDebugLaunchTargetProvider)
            {
                this.workspaceContext = workspaceContext;
                this.vsDebugLaunchTargetProvider = vsDebugLaunchTargetProvider;
            }

            public async Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string filePath, CancellationToken cancellationToken)
            {
                if (TypeScriptHelpers.IsTypeScriptFile(filePath))
                {
                    var outFile = Path.ChangeExtension(filePath, "js");
                    var buildActionContext = new BuildActionContext("notepad.exe", buildConfiguration: "debug");

                    var fileContext = new FileContext(new Guid(ProviderType), BuildActionContext.ContextTypeGuid, buildActionContext, new[] { filePath });

                    var projectTargetFileContext = new ProjectTargetFileContext(filePath, outFile);

                    var debugLaunchContext = new DebugLaunchActionContext(outFile, this.vsDebugLaunchTargetProvider, projectTargetFileContext, "debug");
                    var debugContext = new FileContext(new Guid(ProviderType), DebugLaunchActionContext.ContextTypeGuid, debugLaunchContext, new[] { filePath });

                    return await Task.FromResult<IReadOnlyCollection<FileContext>>(new[] { fileContext, debugContext });
                }

                return Array.Empty<FileContext>();
            }
        }
    }

    [ExportFileContextActionProvider((FileContextActionProviderOptions)VsCommandActionProviderOptions.SupportVsCommands,
        ProviderType, 
        ProviderPriority.Highest,
        BuildActionContext.ContextType, DebugLaunchActionContext.ContextType)]
    public class TypeScriptBuildFileContextActionProviderFactory : IWorkspaceProviderFactory<IFileContextActionProvider>, IVsCommandActionProvider
    {
        private const string ProviderType = "{0D08A397-BE8E-4622-8D8D-7CE4F0CF042A}";

        public IFileContextActionProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new FileContextActionProvider(workspaceContext);
        }

        public IReadOnlyCollection<CommandID> GetSupportedVsCommands()
        {
            return new CommandID[] { new CommandID(new Guid("5ea148a6-40af-4ff2-ab0f-60ed173c9f98"), 0x1000) };
        }

        private class FileContextActionProvider : IFileContextActionProvider
        {
            private IWorkspace workspaceContext;

            public FileContextActionProvider(IWorkspace workspaceContext)
            {
                this.workspaceContext = workspaceContext;
            }

            public async Task<IReadOnlyList<IFileContextAction>> GetActionsAsync(string filePath, FileContext fileContext, CancellationToken cancellationToken)
            {
                if (IsTypeScriptFile(filePath))
                {
                    return await Task.FromResult(new[] { new BuildFileContextAction(fileContext) });
                }

                return Array.Empty<IFileContextAction>();
            }

            private static bool IsTypeScriptFile(string filePath)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(filePath), ".ts");
            }
        }

        private class BuildFileContextAction : IFileContextAction, IVsCommandItem
        {
            public BuildFileContextAction(FileContext fileContext)
            {
                this.Source = fileContext;
            }

            public FileContext Source { get; }

            public string DisplayName => "TypeScriptBuild";

            public Guid CommandGroup => new Guid("5ea148a6-40af-4ff2-ab0f-60ed173c9f98");

            public uint CommandId => 0x1000;

            public Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken)
            {
                return Task.FromResult<IFileContextActionResult>(new SuccessResult());
            }
        }

        private class SuccessResult : IFileContextActionResult
        {
            public bool IsSuccess => true;
        }
    }
}
