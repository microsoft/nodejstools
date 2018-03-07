using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.TypeScript;
using Microsoft.VisualStudio.Workspace;

namespace Microsoft.NodejsTools.Workspace
{
    internal static class WorkspaceExtensions
    {
        public static async Task<(bool, TsConfigJson)> IsContainedByTsConfig(this IWorkspace workspace, string filePath)
        {
            var fileService = workspace.GetFindFilesService();
            var collector = new FileCollector();
            await fileService.FindFilesAsync(NodejsConstants.TsConfigJsonFile, collector);

            if (collector.FoundFiles.Count > 0)
            {
                foreach (var file in collector.FoundFiles)
                {
                    var directory = Path.GetDirectoryName(file);
                    if (filePath.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
                    {
                        return (true, await TsConfigJsonFactory.CreateAsync(file));
                    }
                }
            }

            return (false, null);
        }

        private sealed class FileCollector : IProgress<string>
        {
            public readonly List<string> FoundFiles = new List<string>();

            public void Report(string value)
            {
                this.FoundFiles.Add(value);
            }
        }
    }
}
