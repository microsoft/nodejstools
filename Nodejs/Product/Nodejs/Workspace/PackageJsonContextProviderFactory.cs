// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Workspace;

namespace Microsoft.NodejsTools.Workspace
{
    [ExportFileContextProvider(ProviderType, Guids.PackageJsonContextTypeString)]
    public sealed class PackageJsonContextProviderFactory : IWorkspaceProviderFactory<IFileContextProvider>
    {
        private const string ProviderType = "{3662A7C3-F991-48F3-9810-F639DB7AEEC5}";
        private readonly static Guid ProviderTypeGuid = new Guid(ProviderType);

        public IFileContextProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new PackageJsonContextProvider(workspaceContext);
        }

        private class PackageJsonContextProvider : IFileContextProvider
        {
            private readonly IWorkspace workspaceContext;

            public PackageJsonContextProvider(IWorkspace workspace)
            {
                this.workspaceContext = workspace;
            }

            public Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string filePath, CancellationToken cancellationToken)
            {
                var fileName = Path.GetFileName(filePath);
                if (StringComparer.OrdinalIgnoreCase.Equals(fileName, "package.json"))
                {
                    var fileContext = new FileContext(ProviderTypeGuid, Guids.PackageJsonContextType, filePath, Array.Empty<string>());

                    return Task.FromResult<IReadOnlyCollection<FileContext>>(new[] { fileContext });
                }

                return Task.FromResult<IReadOnlyCollection<FileContext>>(Array.Empty<FileContext>());
            }
        }
    }
}
