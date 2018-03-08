// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.TypeScript;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Build;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.Indexing;

namespace Microsoft.NodejsTools.Workspace
{
    [ExportFileScanner(
        ProviderType, "TypeScriptFile",
        new string[] { "*.ts" },
        new Type[] { typeof(IReadOnlyCollection<FileDataValue>), typeof(IReadOnlyCollection<FileReferenceInfo>) },
        ProviderPriority.Normal)]
    public sealed class TypeScriptScannerFactory : IWorkspaceProviderFactory<IFileScanner>
    {
        private const string ProviderType = "{0EBD9DE4-22CE-4281-A5D6-CB078794E4CE}";

        public IFileScanner CreateProvider(IWorkspace workspaceContext)
        {
            return new TypeScriptScanner(workspaceContext);
        }

        private sealed class TypeScriptScanner : BaseFileScanner, IFileScanner, IFileScannerUpToDateCheck
        {
            public TypeScriptScanner(IWorkspace workspaceContext)
                : base(workspaceContext)
            {
            }

            protected override async Task<List<FileReferenceInfo>> ComputeFileReferencesAsync(string filePath, CancellationToken cancellationToken)
            {
                Debug.Assert(await this.IsValidFileAsync(filePath), $"{filePath} should be a TypeScript file.");

                var outFile = await DetermineOutFileAsync(filePath);

                if (string.IsNullOrEmpty(outFile))
                {
                    return new List<FileReferenceInfo>(0);
                }

                var fileReferences = new List<FileReferenceInfo>
                {
                    new FileReferenceInfo(outFile,
                                          context: "Debug",
                                          target: outFile,
                                          referenceType: (int)FileReferenceInfoType.Output)
                };

                return fileReferences;
            }

            protected override async Task<List<FileDataValue>> ComputeFileDataValuesAsync(string filePath, CancellationToken cancellationToken)
            {
                var outFile = await DetermineOutFileAsync(filePath);

                if (string.IsNullOrEmpty(outFile))
                {
                    return new List<FileDataValue>(0);
                }

                var fileDataValues = new List<FileDataValue>();
                if (!string.IsNullOrEmpty(outFile))
                {
                    var launchSettings = new PropertySettings
                    {
                        [LaunchConfigurationConstants.NameKey] = $"node {outFile}",
                        [LaunchConfigurationConstants.TypeKey] = "default"
                    };

                    fileDataValues.Add(new FileDataValue(
                        DebugLaunchActionContext.ContextTypeGuid,
                        DebugLaunchActionContext.IsDefaultStartupProjectEntry,
                        launchSettings,
                        target: outFile));

                    fileDataValues.Add(
                        new FileDataValue(BuildConfigurationContext.ContextTypeGuid, outFile, null,
                        context: "Debug", target: outFile));

                    fileDataValues.Add(
                        new FileDataValue(BuildConfigurationContext.ContextTypeGuid, outFile, null,
                        context: "Debug", target: null));
                }
                return fileDataValues;
            }

            protected override Task<bool> IsValidFileAsync(string filePath)
            {
                var isValidFile = TypeScriptHelpers.IsTypeScriptFile(filePath);
                return Task.FromResult(isValidFile);
            }

            private async Task<string> DetermineOutFileAsync(string filePath)
            {
                // check if there's a tsconfig which could be the root of this file. Use that to check the output file.
                var tsConfig = await this.workspace.IsContainedByTsConfig(filePath);
                if (tsConfig != null)
                {
                    if (!string.IsNullOrEmpty(tsConfig.OutFile))
                    {
                        // we don't support debugging individual files when there's an outfile
                        return null;
                    }

                    var rootDir = Path.GetDirectoryName(tsConfig.FilePath);
                    var relativeTsFile = filePath.Substring(rootDir.Length).TrimStart('\\'); // save since we already know they have the same root

                    return this.workspace.MakeRelative(Path.Combine(rootDir, tsConfig.OutDir, Path.ChangeExtension(relativeTsFile, "js"))); // this works if outdir is null
                }
                else
                {
                    return this.workspace.MakeRelative(Path.ChangeExtension(filePath, "js"));
                }
            }
        }
    }
}
