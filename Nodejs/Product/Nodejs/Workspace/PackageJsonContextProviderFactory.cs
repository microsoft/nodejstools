// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;

namespace Microsoft.NodejsTools.Workspace
{
    [ExportFileContextProvider(
        ProviderType,
        ProviderPriority.Normal,
        new Type[] { typeof(string) },
        Guids.PackageJsonContextTypeString,
        BuildContextTypes.BuildContextType)]
    public sealed class PackageJsonContextProviderFactory : IWorkspaceProviderFactory<IFileContextProvider>
    {
        private const string ProviderType = "{3662A7C3-F991-48F3-9810-F639DB7AEEC5}";
        private readonly static Guid ProviderTypeGuid = new Guid(ProviderType);

        public IFileContextProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new PackageJsonContextProvider(workspaceContext);
        }

        private class PackageJsonContextProvider : IFileContextProvider, IFileContextProvider<string>
        {
            private readonly IWorkspace workspaceContext;

            public PackageJsonContextProvider(IWorkspace workspace)
            {
                this.workspaceContext = workspace;
            }

            public Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string filePath, CancellationToken cancellationToken)
            {
                return this.GetContextsForFileAsync(filePath, string.Empty, cancellationToken);
            }
            public Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string filePath, string context, CancellationToken cancellationToken)
            {
                if (PackageJsonHelpers.IsPackageJsonFile(filePath))
                {
                    var fileContexts = new List<FileContext>();

                    var packageJson = PackageJsonFactory.Create(this.workspaceContext.MakeRooted(filePath));

                    Debug.Assert(packageJson != null);

                    fileContexts.Add(new FileContext(ProviderTypeGuid,
                        Guids.PackageJsonContextType,
                        "",
                        new[] { filePath },
                        "package.json context"));

                    return Task.FromResult<IReadOnlyCollection<FileContext>>(fileContexts);
                }

                return Task.FromResult(FileContext.EmptyFileContexts);
            }
        }

        private sealed class PackageJsonBuildContext : IBuildConfigurationContext
        {
            public string BuildConfiguration => "Debug";
        }
    }
}
