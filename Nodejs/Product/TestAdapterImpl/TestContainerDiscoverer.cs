﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.NodejsTools.TypeScript;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.TestAdapter
{
    [Export(typeof(ITestContainerDiscoverer))]
    internal class TestContainerDiscoverer : ITestContainerDiscoverer, IDisposable
    {
        private readonly IServiceProvider serviceProvider;
        private readonly TestFileAddRemoveListener testFilesAddRemoveListener;
        private readonly TestFilesUpdateWatcher testFilesUpdateWatcher;
        private readonly SolutionEventsListener solutionListener;
        private readonly Dictionary<string, string> fileRootMap;
        private readonly Dictionary<string, ProjectInfo> knownProjects;
        private bool firstLoad, isDisposed, building, detectingChanges;
        private DateTime lastWrite = DateTime.MinValue;

        [ImportingConstructor]
        private TestContainerDiscoverer([Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider, [Import(typeof(IOperationState))]IOperationState operationState)
        {
            ValidateArg.NotNull(serviceProvider, "serviceProvider");
            ValidateArg.NotNull(operationState, "operationState");

            this.fileRootMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.knownProjects = new Dictionary<string, ProjectInfo>(StringComparer.OrdinalIgnoreCase);

            this.serviceProvider = serviceProvider;

            this.testFilesAddRemoveListener = new TestFileAddRemoveListener(serviceProvider, Guids.NodejsBaseProjectFactory);
            this.testFilesAddRemoveListener.TestFileChanged += this.OnProjectItemChanged;

            this.solutionListener = new SolutionEventsListener(serviceProvider);
            this.solutionListener.ProjectLoaded += this.OnProjectLoaded;
            this.solutionListener.ProjectUnloading += this.OnProjectUnloaded;
            this.solutionListener.ProjectClosing += this.OnProjectUnloaded;
            this.solutionListener.ProjectRenamed += this.OnProjectRenamed;
            this.solutionListener.BuildCompleted += this.OnBuildCompleted;
            this.solutionListener.BuildStarted += this.OnBuildStarted;

            this.testFilesUpdateWatcher = new TestFilesUpdateWatcher(serviceProvider);
            this.testFilesUpdateWatcher.FileChangedEvent += this.OnProjectItemChanged;
            operationState.StateChanged += this.OperationStateChanged;

            this.firstLoad = true;
        }

        private static IEnumerable<IVsProject> EnumerateLoadedProjects(IVsSolution solution)
        {
            var ignored = Guid.Empty;
            ErrorHandler.ThrowOnFailure((solution.GetProjectEnum(
                (uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION,
                ref ignored,
                out var hierarchies)));

            var current = new IVsHierarchy[1];
            while (ErrorHandler.Succeeded(hierarchies.Next(1, current, out var fetchCount)) && fetchCount == 1)
            {
                if (current[0] is IVsProject project)
                {
                    yield return project;
                }
            }
        }

        internal bool IsTestFile(string pathToFile)
        {
            var project = this.GetTestProjectFromFile(pathToFile);
            if (project == null)
            {
                //The file is not included in the project. 
                //Don't look for tests in it.
                return false;
            }

            //
            //Check to see if we are dealing with a TypeScript file
            //  If we are then switch the test container to the underlying js file
            //
            if (TypeScriptHelpers.IsTypeScriptFile(pathToFile))
            {
                var jsFile = TypeScriptHelpers.GetTypeScriptBackedJavaScriptFile(project, pathToFile);
                if (jsFile == null || !File.Exists(jsFile))
                {
                    //Ignore the file for now.  On the next build event the typescript compiler will generate the file
                    //  at that point this function gets invoked again on the .ts file and we'll see the newly created .js file
                    return false;
                }
            }
            else if (!TypeScriptHelpers.IsJavaScriptFile(pathToFile))
            {
                return false;
            }

            if (TryGetProjectUnitTestProperties(project, out var testRoot, out _)
                && !string.IsNullOrEmpty(testRoot)
                && project.TryGetProjectDirectory(out var root))
            {
                var testRootPath = Path.Combine(root, testRoot);

                return CommonUtils.IsSubpathOf(testRootPath, pathToFile);
            }

            ErrorHandler.Succeeded(((IVsHierarchy)project).ParseCanonicalName(pathToFile, out var itemId));

            return IsTestFile(itemId, project);
        }

        private static bool TryGetProjectUnitTestProperties(IVsProject project, out string testRoot, out string testFramework)
        {
            if (!(project is IVsBuildPropertyStorage propStore))
            {
                Debug.Fail($"Why is {nameof(project)} not of type {nameof(IVsBuildPropertyStorage)}?");

                testRoot = null;
                testFramework = null;

                return false;
            }

            // If a property has not been configured, depending on the project type the result and output will be diffent.
            // .NET Framework returns a failure code and null as output, .NET Core returns a success code and "" as output, NTVS returns success code and null as output
            var hrTestRoot = propStore.GetPropertyValue(NodeProjectProperty.TestRoot, /*configuration*/"", (uint)_PersistStorageType.PST_PROJECT_FILE, out testRoot);
            var hrTestFramework = propStore.GetPropertyValue(NodeProjectProperty.TestFramework, /*configuration*/"", (uint)_PersistStorageType.PST_PROJECT_FILE, out testFramework);

            // If it doesnt succeed it's a .NET Framework project not configured.
            // If it succeeds but it's empty, it might be a .NET Core project not configured or a misconfigured project.
            // Anything else can be a NTVS project, in which case, we want to always return true.
            return ErrorHandler.Succeeded(hrTestRoot) && testRoot != string.Empty
                || ErrorHandler.Succeeded(hrTestFramework) && testFramework != string.Empty;
        }

        private static bool IsTestFile(uint itemId, IVsProject project)
        {
            if (!(project is IVsHierarchy hierarchy))
            {
                return false;
            }

            hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out var extObject);

            if (!(extObject is EnvDTE.ProjectItem projectItem))
            {
                return false;
            }

            var props = projectItem.Properties;
            if (props == null)
            {
                return false;
            }

            try
            {
                var testFile = props.Item("TestFramework");
                return !string.IsNullOrEmpty(testFile?.Value as string);
            }
            catch (ArgumentException)
            {
                //If we fail to retrieve the property then this isn't a test file
            }
            return false;
        }

        private void OperationStateChanged(object sender, OperationStateChangedEventArgs e)
        {
            if (e.State == TestOperationStates.ChangeDetectionStarting)
            {
                this.detectingChanges = true;
            }
            else if (e.State == TestOperationStates.ChangeDetectionFinished)
            {
                this.detectingChanges = false;
            }
        }

        private void OnBuildStarted(object sender, EventArgs e)
        {
            this.building = true;
        }

        private void OnBuildCompleted(object sender, EventArgs e)
        {
            var solution = (IVsSolution)this.serviceProvider.GetService(typeof(SVsSolution));
            foreach (var project in EnumerateLoadedProjects(solution))
            {
                if (this.OnTestContainersChanged(project))
                {
                    // We only need to fire the event once as the event 
                    // is not per-project, but we shouldn't fire it if 
                    // there's not yet any projects which we've been 
                    // queried for. 
                    break;
                }
            }
            this.building = false;
        }

        #region IDisposable
        void IDisposable.Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
                this.testFilesAddRemoveListener.Dispose();
                this.testFilesUpdateWatcher.Dispose();
                this.solutionListener.Dispose();
            }
        }
        #endregion

        #region ITestContainerDiscoverer

        public event EventHandler TestContainersUpdated;

        public Uri ExecutorUri => NodejsConstants.ExecutorUri;

        public IEnumerable<ITestContainer> TestContainers
        {
            get
            {
                // Get current solution
                var solution = (IVsSolution)this.serviceProvider.GetService(typeof(SVsSolution));

                if (this.firstLoad)
                {
                    // The first time through, we don't know about any loaded
                    // projects.
                    this.firstLoad = false;
                    foreach (var project in EnumerateLoadedProjects(solution))
                    {
                        this.OnProjectLoaded(null, new ProjectEventArgs(project));
                    }
                    this.testFilesAddRemoveListener.StartListeningForTestFileChanges();
                    this.solutionListener.StartListeningForChanges();
                }

                // Get all loaded projects
                return EnumerateLoadedProjects(solution).SelectMany(p => this.GetTestContainers(p));
            }
        }

        #endregion

        public IEnumerable<ITestContainer> GetTestContainers(IVsProject project)
        {
            if (!project.TryGetProjectPath(out var path))
            {
                yield break;
            }

            // No need to search for tests in not supported projects.
            if (!TypeScriptHelpers.IsSupportedTestProjectFile(path))
            {
                yield break;
            }

            if (!this.knownProjects.TryGetValue(path, out var projectInfo) || !TryGetProjectUnitTestProperties(projectInfo.Project, out _, out var testFramework))
            {
                // Don't return any containers for projects we don't know about or that we know that they are not configured for JavaScript unit tests.
                yield break;
            }
            projectInfo.HasRequestedContainers = true;

            var latestWrite = project.GetProjectItemPaths().Aggregate(
                this.lastWrite,
                (latest, filePath) =>
                {
                    try
                    {
                        var ft = File.GetLastWriteTimeUtc(filePath);
                        return (ft > latest) ? ft : latest;
                    }
                    catch (Exception exc) when (exc is UnauthorizedAccessException || exc is ArgumentException || exc is IOException)
                    {
                    }
                    return latest;
                });

            if (!string.IsNullOrEmpty(testFramework))
            { 
                Microsoft.NodejsTools.Telemetry.TelemetryHelper.LogTestDiscoveryStarted(testFramework);
            }

            yield return new TestContainer(this, path, latestWrite);
        }

        private bool ShouldDiscover(string pathToItem)
        {
            if (string.IsNullOrEmpty(pathToItem))
            {
                return false;
            }

            if (pathToItem.IndexOf("\\node_modules\\", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            //Setting/updating "TestFramework" property on a file item will cause metedata change in the project file,
            //so we need to re-discover when file change happens.
            if (TypeScriptHelpers.IsSupportedTestProjectFile(pathToItem))
            {
                return true;
            }

            if (this.IsTestFile(pathToItem))
            {
                if (EqtTrace.IsVerboseEnabled)
                {
                    EqtTrace.Verbose("TestContainerDiscoverer: Found a test {0}.", pathToItem);
                }

                return true;
            }

            return false;
        }

        private void OnProjectLoaded(object sender, ProjectEventArgs e)
        {
            if (e.Project != null)
            {
                string root = null;
                try
                {
                    root = e.Project.GetProjectHome();
                }
                catch (Exception ex)
                {
                    if (EqtTrace.IsVerboseEnabled)
                    {
                        EqtTrace.Warning("TestContainerDiscoverer: Failed to get project home {0}", ex);
                    }
                    // If we fail to get ProjectHome, we still want to track the
                    // project. We just won't get the benefits of merging
                    // watchers into a single recursive watcher.
                }

                if (e.Project.TryGetProjectPath(out var path) && !this.knownProjects.ContainsKey(path))
                {
                    var dteProject = ((IVsHierarchy)e.Project).GetProject();

                    var projectInfo = new ProjectInfo(
                        e.Project,
                        discoverer: this
                    );

                    this.knownProjects.Add(path, projectInfo);

                    foreach (var p in e.Project.GetProjectItemPaths())
                    {
                        if (!string.IsNullOrEmpty(root) && CommonUtils.IsSubpathOf(root, p))
                        {
                            this.testFilesUpdateWatcher.AddFolderWatch(root);
                            this.fileRootMap[p] = root;
                        }
                        else
                        {
                            this.testFilesUpdateWatcher.AddFileWatch(p);
                        }
                    }
                }
            }

            this.OnTestContainersChanged(e.Project);
        }

        private void OnProjectUnloaded(object sender, ProjectEventArgs e)
        {
            if (e.Project != null)
            {
                string root = null;
                try
                {
                    root = e.Project.GetProjectHome();
                }
                catch (Exception ex)
                {
                    if (EqtTrace.IsVerboseEnabled)
                    {
                        EqtTrace.Warning("TestContainerDiscoverer: Failed to get project home {0}", ex);
                    }
                    // If we fail to get ProjectHome, we still want to track the
                    // project. We just won't get the benefits of merging
                    // watchers into a single recursive watcher.
                }

                if (e.Project.TryGetProjectPath(out var projectPath) && this.knownProjects.TryGetValue(projectPath, out var projectInfo))
                {
                    this.knownProjects.Remove(projectPath);

                    foreach (var p in e.Project.GetProjectItemPaths())
                    {
                        if (string.IsNullOrEmpty(root) || !CommonUtils.IsSubpathOf(root, p))
                        {
                            this.testFilesUpdateWatcher.RemoveFileWatch(p);
                        }

                        this.fileRootMap.Remove(p);
                    }

                    if (!string.IsNullOrEmpty(root))
                    {
                        this.testFilesUpdateWatcher.RemoveFolderWatch(root);
                    }
                }
            }

            this.OnTestContainersChanged(e.Project);
        }

        private void OnProjectRenamed(object sender, ProjectEventArgs e)
        {
            this.OnProjectUnloaded(this, e);
            this.OnProjectLoaded(this, e);
        }

        /// <summary>
        /// Handler to react to test file Add/remove/rename events
        /// </summary>
        private void OnProjectItemChanged(object sender, TestFileChangedEventArgs e)
        {
            if (e != null && this.ShouldDiscover(e.File))
            {
                string root = null;
                var project = e.Project ?? this.GetTestProjectFromFile(e.File);
                switch (e.ChangedReason)
                {
                    case WatcherChangeTypes.Created:
                        root = project.GetProjectHome();

                        if (!string.IsNullOrEmpty(root) && CommonUtils.IsSubpathOf(root, e.File))
                        {
                            this.testFilesUpdateWatcher.AddFolderWatch(root);
                            this.fileRootMap[e.File] = root;
                        }
                        else
                        {
                            this.testFilesUpdateWatcher.AddFileWatch(e.File);
                        }

                        this.OnTestContainersChanged(project);
                        break;
                    case WatcherChangeTypes.Deleted:
                        if (this.fileRootMap.TryGetValue(e.File, out root))
                        {
                            this.fileRootMap.Remove(e.File);

                            if (!this.fileRootMap.Values.Contains(root))
                            {
                                this.testFilesUpdateWatcher.RemoveFolderWatch(root);
                            }
                        }
                        else
                        {
                            this.testFilesUpdateWatcher.RemoveFileWatch(e.File);
                        }

                        // https://pytools.codeplex.com/workitem/1546
                        // track the last delete as an update as our file system scan won't see it
                        this.lastWrite = DateTime.Now.ToUniversalTime();

                        this.OnTestContainersChanged(project);
                        break;

                    // Dev12 renames files instead of overwriting them when
                    // saving, so we need to listen for renames where the new
                    // path is part of the project.
                    case WatcherChangeTypes.Renamed:
                    case WatcherChangeTypes.Changed:
                        this.OnTestContainersChanged(project);
                        break;
                }
            }
        }

        /// <summary>
        /// Given a filename that might be in an open project returns the test
        /// project which contains it, or null if it is not in a test project.
        /// </summary>
        private IVsProject GetTestProjectFromFile(string filename)
        {
            var solution = (IVsSolution)this.serviceProvider.GetService(typeof(SVsSolution));

            foreach (var project in EnumerateLoadedProjects(solution))
            {
                var hierarchy = project as IVsHierarchy;
                if (project.TryGetProjectPath(out var projectPath) &&
                    CommonUtils.IsSamePath(projectPath, filename) ||
                    (hierarchy != null && ErrorHandler.Succeeded(hierarchy.ParseCanonicalName(filename, out _))))
                {
                    return project;
                }
            }
            return null;
        }

        /// <summary>
        /// Raises the test containers changed event, returns true if the event was delivered
        /// or would be delivered and there were no listeners.
        /// </summary>
        /// <param name="project">The project which the event is being raised for</param>
        private bool OnTestContainersChanged(IVsProject project)
        {
            // https://pytools.codeplex.com/workitem/1271 
            // When test explorer kicks off a run it kicks off a test discovery 
            // phase, which kicks off a build, which results in us saving files. 
            // If we raise the files changed event then test explorer immediately turns 
            // around and queries us for the changed files.  Then it continues 
            // along with the test discovery phase it was already initiating, and  
            // discovers that no changes have occured - because it already updated 
            // to the latest changes when we informed it our containers had changed.   
            // Therefore if we are both building and detecting changes then we  
            // don't want to raise the event, instead it'll query us in a little  
            // bit and get the most recent changes. 

            if (project != null &&
                project.TryGetProjectPath(out var projectPath) &&
                this.knownProjects.TryGetValue(projectPath, out var projectInfo) &&
                projectInfo != null &&
                projectInfo.HasRequestedContainers)
            {
                if (!this.building || !this.detectingChanges)
                {
                    TestContainersUpdated?.Invoke(this, EventArgs.Empty);
                    return true;
                }
            }
            return false;
        }

        internal bool IsProjectKnown(IVsProject project)
        {
            if (project.TryGetProjectPath(out var projectPath))
            {
                return this.knownProjects.ContainsKey(projectPath);
            }
            return false;
        }

        private sealed class ProjectInfo
        {
            public readonly IVsProject Project;
            public readonly TestContainerDiscoverer Discoverer;
            /// <summary>
            /// used to track when the test window has asked us for tests from this project, 
            /// so we don't deliver events before it's ready.  It will ask when the project 
            /// is opened the 1st time, and then we can start delivering events as things
            /// change.  Fixes 2nd round of https://pytools.codeplex.com/workitem/1252
            /// </summary>
            public bool HasRequestedContainers;

            public ProjectInfo(IVsProject project, TestContainerDiscoverer discoverer)
            {
                this.Project = project;
                this.Discoverer = discoverer;
            }
        }
    }
}
