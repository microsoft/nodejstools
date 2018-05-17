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

        private IWorkspace currentWorkspace;

        [ImportingConstructor]
        public PackageJsonTestContainerDiscoverer(IVsFolderWorkspaceService workspaceService)
        {
            this.workspaceService = workspaceService;
            this.workspaceService.OnActiveWorkspaceChanged += this.OnActiveWorkspaceChangedAsync;

            if (this.workspaceService.CurrentWorkspace != null)
            {
                this.currentWorkspace = this.workspaceService.CurrentWorkspace;
                this.RegisterEvents();

                this.currentWorkspace.JTF.RunAsync(async () =>
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
            if (this.currentWorkspace != null)
            {
                var indexServce = this.currentWorkspace.GetIndexWorkspaceService();
                var filesDataValues = await indexServce.GetFilesDataValuesAsync<string>(NodejsConstants.TestRootDataValueGuid);

                lock (this.containerLock)
                {
                    this.containers.Clear();
                    foreach (var dataValue in filesDataValues)
                    {
                        var rootFilePath = this.currentWorkspace.MakeRooted(dataValue.Key);
                        var testRoot = dataValue.Value.Where(f => f.Name == "TestRoot").Select(f => f.Value).FirstOrDefault();

                        if (!string.IsNullOrEmpty(testRoot))
                        {
                            var testRootPath = this.currentWorkspace.MakeRooted(testRoot);
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

            this.currentWorkspace = this.workspaceService.CurrentWorkspace;

            this.RegisterEvents();

            await AttemptUpdateAsync();
        }

        private void RegisterEvents()
        {
            var fileWatcherService = this.currentWorkspace?.GetFileWatcherService();
            if (fileWatcherService != null)
            {
                fileWatcherService.OnFileSystemChanged += this.FileSystemChangedAsync;
            }

            var indexService = this.currentWorkspace?.GetIndexWorkspaceService();
            if (indexService != null)
            {
                indexService.OnFileScannerCompleted += this.FileScannerCompletedAsync;
            }
        }

        private void UnRegisterEvents()
        {
            var fileWatcherService = this.currentWorkspace?.GetFileWatcherService();
            if (fileWatcherService != null)
            {
                fileWatcherService.OnFileSystemChanged -= this.FileSystemChangedAsync;
            }

            var indexService = this.currentWorkspace?.GetIndexWorkspaceService();
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
                // use a flag so we don't deadlock
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
