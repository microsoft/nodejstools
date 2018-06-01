// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Indexing;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;

namespace Microsoft.NodejsTools.Workspace
{
    [Export(typeof(ITestContainerDiscoverer))]
    public sealed class PackageJsonTestContainerDiscoverer : ITestContainerDiscoverer
    {
        private readonly IVsFolderWorkspaceService workspaceService;

        private readonly List<PackageJsonTestContainer> containers = new List<PackageJsonTestContainer>();
        private readonly object containerLock = new object();

        private IWorkspace activeWorkspace;

        [ImportingConstructor]
        public PackageJsonTestContainerDiscoverer(IVsFolderWorkspaceService workspaceService)
        {
            this.workspaceService = workspaceService;
            this.workspaceService.OnActiveWorkspaceChanged += this.OnActiveWorkspaceChangedAsync;

            if (this.workspaceService.CurrentWorkspace != null)
            {
                this.activeWorkspace = this.workspaceService.CurrentWorkspace;
                this.RegisterEvents();

                this.activeWorkspace.JTF.RunAsync(async () =>
                {
                    // Yield so we don't do this now. Don't want to block the constructor.
                    await Task.Yield();

                    // See if we have an update
                    await AttemptUpdateAsync();
                });
            }
        }

        public Uri ExecutorUri => NodejsConstants.PackageJsonExecutorUri;

        public IEnumerable<ITestContainer> TestContainers
        {
            get
            {
                lock (this.containerLock)
                {
                    return this.containers.ToArray();
                }
            }
        }

        public event EventHandler TestContainersUpdated;

        private async Task AttemptUpdateAsync()
        {
            var workspace = this.activeWorkspace;
            if (workspace != null)
            {
                var indexService = workspace.GetIndexWorkspaceService();
                var filesDataValues = await indexService.GetFilesDataValuesAsync<string>(NodejsConstants.TestRootDataValueGuid);

                lock (this.containerLock)
                {
                    this.containers.Clear();
                    foreach (var dataValue in filesDataValues)
                    {
                        var rootFilePath = workspace.MakeRooted(dataValue.Key);
                        var testRoot = dataValue.Value.Where(f => f.Name == NodejsConstants.TestRootDataValueName).FirstOrDefault()?.Value;

                        if (!string.IsNullOrEmpty(testRoot))
                        {
                            var testRootPath = workspace.MakeRooted(testRoot);
                            this.containers.Add(new PackageJsonTestContainer(this, rootFilePath, testRootPath));
                        }
                    }
                }
            }
            this.TestContainersUpdated?.Invoke(this, EventArgs.Empty);
        }

        private async Task OnActiveWorkspaceChangedAsync(object sender, EventArgs e)
        {
            this.UnRegisterEvents();

            this.activeWorkspace = this.workspaceService.CurrentWorkspace;

            this.RegisterEvents();

            await AttemptUpdateAsync();
        }

        private void RegisterEvents()
        {
            var workspace = this.activeWorkspace;
            if (workspace != null)
            {
                var fileWatcherService = workspace.GetFileWatcherService();
                if (fileWatcherService != null)
                {
                    fileWatcherService.OnFileSystemChanged += this.FileSystemChangedAsync;
                }

                var indexService = workspace.GetIndexWorkspaceService();
                if (indexService != null)
                {
                    indexService.OnFileScannerCompleted += this.FileScannerCompletedAsync;
                }
            }
        }

        private void UnRegisterEvents()
        {
            var fileWatcherService = this.activeWorkspace?.GetFileWatcherService();
            if (fileWatcherService != null)
            {
                fileWatcherService.OnFileSystemChanged -= this.FileSystemChangedAsync;
            }

            var indexService = this.activeWorkspace?.GetIndexWorkspaceService();
            if (indexService != null)
            {
                indexService.OnFileScannerCompleted -= this.FileScannerCompletedAsync;
            }
        }

        private Task FileSystemChangedAsync(object sender, FileSystemEventArgs args)
        {
            // We only need to raise the containers updated event in case a js file contained in the
            // test root is updated.
            // Any changes to the 'package.json' will be handled by the FileScannerCompleted event.
            if (IsJavaScriptFile(args.FullPath) || args.IsDirectoryChanged())
            {
                // use a flag so we don't raise the event while under the lock
                var testsUpdated = false;
                lock (this.containerLock)
                {
                    foreach (var container in this.containers)
                    {
                        if (container.IsContained(args.FullPath))
                        {
                            container.IncreaseVersion();
                            testsUpdated = true;
                            break;
                        }
                    }
                }
                if (testsUpdated)
                {
                    this.TestContainersUpdated?.Invoke(this, EventArgs.Empty);
                }
            }

            return Task.CompletedTask;
        }

        private async Task FileScannerCompletedAsync(object sender, FileScannerEventArgs args)
        {
            await AttemptUpdateAsync();
        }

        private static bool IsJavaScriptFile(string path)
        {
            var ext = Path.GetExtension(path);
            if (StringComparer.OrdinalIgnoreCase.Equals(ext, ".js") || StringComparer.OrdinalIgnoreCase.Equals(ext, ".jsx"))
            {
                return true;
            }

            return false;
        }
    }
}
