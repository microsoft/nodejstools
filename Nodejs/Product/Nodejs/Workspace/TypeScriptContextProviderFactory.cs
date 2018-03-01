// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.TypeScript;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;
using Microsoft.VisualStudio.Workspace.Debug;

namespace Microsoft.NodejsTools.Workspace
{
    public sealed class TypeScriptBuildContext : IBuildConfigurationContext
    {
        public string BuildConfiguration => "Debug";
    }

    [ExportFileContextProvider(
              ProviderType,
              ProviderPriority.Normal,
              new Type[] { typeof(string) },
              BuildContextTypes.BuildContextType)]
    public sealed class TypeScriptContextProviderFactory : IWorkspaceProviderFactory<IFileContextProvider>
    {
        private const string ProviderType = "{72D3FCEF-5787-4266-B8DD-D3ED06E35A2B}";
        private readonly static Guid ProviderTypeGuid = new Guid(ProviderType);

        public IFileContextProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new TypeScriptContextProvider(workspaceContext);
        }

        private sealed class TypeScriptContextProvider : IFileContextProvider, IFileContextProvider<string>
        {
            private readonly IWorkspace workspaceContext;

            public TypeScriptContextProvider(IWorkspace workspaceContext)
            {
                this.workspaceContext = workspaceContext;
            }

            public Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string filePath, CancellationToken cancellationToken)
            {
                return this.GetContextsForFileAsync(filePath, string.Empty, cancellationToken);
            }

            public Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string filePath, string context, CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(filePath) || !TypeScriptHelpers.IsTypeScriptFile(filePath))
                {
                    return Task.FromResult(FileContext.EmptyFileContexts);
                }

                var list = new List<FileContext>
                {
                    new FileContext(
                        ProviderTypeGuid,
                        BuildContextTypes.BuildContextTypeGuid,
                        new TypeScriptBuildContext(),
                        new[]{ filePath },
                        displayName: "TypeScript Build")
                };

                return Task.FromResult<IReadOnlyCollection<FileContext>>(list);
            }
        }
    }
}
