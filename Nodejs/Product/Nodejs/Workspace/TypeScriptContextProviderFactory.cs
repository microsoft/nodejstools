// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.TypeScript;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;

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
            private readonly IWorkspace workspace;

            public TypeScriptContextProvider(IWorkspace workspaceContext)
            {
                this.workspace = workspaceContext;
            }

            public Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string filePath, CancellationToken cancellationToken)
            {
                return this.GetContextsForFileAsync(filePath, string.Empty, cancellationToken);
            }

            public async Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string filePath, string context, CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(filePath) || !IsSupportedFile(filePath))
                {
                    return FileContext.EmptyFileContexts;
                }

                var (isContained, tsconfigJson) = await this.workspace.IsContainedByTsConfig(filePath);

                if (isContained)
                {
                    return FileContext.EmptyFileContexts;
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

                return list;
            }

            private static bool IsSupportedFile(string filePath)
            {
                return TypeScriptHelpers.IsTypeScriptFile(filePath) || TsConfigJsonFactory.IsTsConfigJsonFile(filePath);
            }
        }
    }
}
