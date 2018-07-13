// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.NodejsTools.TypeScript;
using Microsoft.VisualStudio.Workspace;

namespace Microsoft.NodejsTools.Workspace
{
    internal static class WorkspaceExtensions
    {
        public static async Task<TsConfigJson> IsContainedByTsConfig(this IWorkspace workspace, string filePath)
        {
            var fileService = workspace.GetFindFilesService();
            var collector = new FileCollector();
            await fileService.FindFilesAsync("sconfig.json", collector);

            foreach (var configFile in collector.FoundFiles)
            {
                if (TypeScriptHelpers.IsTsJsConfigJsonFile(configFile))
                {
                    var directory = Path.GetDirectoryName(configFile);
                    if (filePath.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
                    {
                        return await TsConfigJsonFactory.CreateAsync(configFile);
                    }
                }
            }

            return null;
        }

        public sealed class FileCollector : IProgress<string>
        {
            public readonly List<string> FoundFiles = new List<string>();

            public void Report(string value)
            {
                this.FoundFiles.Add(value);
            }
        }
    }
}
