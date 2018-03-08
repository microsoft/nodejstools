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
        ProviderType, "TsConfigFileProvider",
        new string[] { NodejsConstants.TsConfigJsonFile, NodejsConstants.JsConfigJsonFile },
        new Type[] { typeof(IReadOnlyCollection<FileDataValue>), typeof(IReadOnlyCollection<FileReferenceInfo>) },
        ProviderPriority.Normal)]
    public sealed class TsConfigScannerFactory : IWorkspaceProviderFactory<IFileScanner>
    {

        private const string ProviderType = "AE211DC5-CE20-41DA-91A0-3D44A7652C6F";

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
                Debug.Assert(await this.IsValidFileAsync(filePath), $"{filePath} should be a tsconfig.json file.");

                var tsconfig = await TsConfigJsonFactory.CreateAsync(filePath);

                Debug.Assert(!string.IsNullOrEmpty(tsconfig?.OutFile), "Should have an outfile specified.");

                var tsconfigFolder = Path.GetDirectoryName(filePath);
                var outFile = Path.Combine(tsconfigFolder, tsconfig.OutFile);

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
                Debug.Assert(await this.IsValidFileAsync(filePath), $"{filePath} should be a tsconfig.json file.");

                var tsconfig = await TsConfigJsonFactory.CreateAsync(filePath);

                Debug.Assert(!string.IsNullOrEmpty(tsconfig?.OutFile), "Should have an outfile specified.");

                var tsconfigFolder = Path.GetDirectoryName(filePath);
                var outFile = Path.Combine(tsconfigFolder, tsconfig.OutFile);

                var launchSettings = new PropertySettings
                {
                    [LaunchConfigurationConstants.NameKey] = $"node {outFile} {NodejsConstants.TsConfigJsonFile}",
                    [LaunchConfigurationConstants.TypeKey] = "default"
                };

                var fileDataValues = new List<FileDataValue>
                {
                    new FileDataValue(
                        DebugLaunchActionContext.ContextTypeGuid,
                        DebugLaunchActionContext.IsDefaultStartupProjectEntry,
                       launchSettings,
                       target: outFile),

                    new FileDataValue(BuildConfigurationContext.ContextTypeGuid, outFile, null,
                        context: "Debug", target: outFile),

                    new FileDataValue(BuildConfigurationContext.ContextTypeGuid, outFile, null,
                        context: "Debug", target: null)
                };

                return fileDataValues;
            }

            protected override async Task<bool> IsValidFileAsync(string filePath)
            {
                // Only use the tsconfig.json determine the debug target when there is an outfile specified,
                // otherwise each .ts file can (in theory) be the entry point.
                if (TypeScriptHelpers.IsTsJsConfigJsonFile(filePath))
                {
                    var tsconfig = await TsConfigJsonFactory.CreateAsync(filePath);
                    return !string.IsNullOrEmpty(tsconfig?.OutFile);
                }

                return false;
            }
        }
    }
}
