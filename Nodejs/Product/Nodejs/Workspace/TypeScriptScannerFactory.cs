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
        ProviderType, "TypeScriptProject",
        new string[] { "tsconfig.json", "*.ts" },
        new Type[] { typeof(IReadOnlyCollection<FileDataValue>), typeof(IReadOnlyCollection<FileReferenceInfo>) })]
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
                Debug.Assert(await this.IsValidFileAsync(filePath), $"{filePath} should be a tsconfig.json file, or a TypeScript file.");

                if (filePath.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
                {
                    var outFile = this.workspace.MakeRelative(Path.ChangeExtension(filePath, "js"));

                    var fileReferences = new List<FileReferenceInfo>
                    {
                        new FileReferenceInfo(outFile,
                                              context: "Debug",
                                              target: outFile,
                                              referenceType: (int)FileReferenceInfoType.Output)
                    };

                    return fileReferences;
                }

                return new List<FileReferenceInfo>(0);
            }

            protected override Task<List<FileDataValue>> ComputeFileDataValuesAsync(string filePath, CancellationToken cancellationToken)
            {
                var outFile = this.workspace.MakeRelative(Path.ChangeExtension(filePath, "js"));

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
                return Task.FromResult(fileDataValues);
            }

            protected override Task<bool> IsValidFileAsync(string filePath)
            {
                var isValidFile = TsConfigJsonFactory.IsTsConfigJsonFile(filePath) || TypeScriptHelpers.IsTypeScriptFile(filePath);
                return Task.FromResult(isValidFile);
            }
        }
    }
}
