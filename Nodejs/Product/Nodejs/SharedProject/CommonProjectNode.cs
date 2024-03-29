// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Project;
using Microsoft.NodejsTools.TypeScript;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Navigation;
using Microsoft.VisualStudioTools.Project.Automation;
using MSBuild = Microsoft.Build.Evaluation;
using TPL = System.Threading.Tasks;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.VisualStudioTools.Project
{
    public enum CommonImageName
    {
        File = 0,
        Project = 1,
        SearchPathContainer,
        SearchPath,
        MissingSearchPath,
        StartupFile,
    }

    internal abstract partial class CommonProjectNode : ProjectNode, IVsProjectSpecificEditorMap2, IVsDeferredSaveProject
    {
        private Guid mruPageGuid = new Guid(CommonConstants.AddReferenceMRUPageGuid);
        private VSLangProj.VSProject vsProject = null;
        private ProjectDocumentsListenerForStartupFileUpdates projectDocListenerForStartupFileUpdates;

        private readonly FileChangeManager fileWatcher;
        private readonly IdleManager idleManager;

        private bool isShowingAllFiles;
        private object automationObject;
        private ConcurrentQueue<FileSystemChange> fileSystemChanges = new ConcurrentQueue<FileSystemChange>();

        private DiskMerger currentMerger;
        private IVsHierarchyItemManager hierarchyManager;
        private Dictionary<uint, bool> needBolding;
        private int idleTriggered;

        public CommonProjectNode(IServiceProvider serviceProvider, ImageList imageList)
            : base(serviceProvider)
        {
            this.CanFileNodesHaveChilds = true;
            this.SupportsProjectDesigner = true;
            if (imageList != null)
            {
                //Store the number of images in ProjectNode so we know the offset of the language icons.
#pragma warning disable 0618
                this.ImageOffset = this.ImageHandler.ImageList.Images.Count;
                foreach (Image img in ImageList.Images)
                {
                    this.ImageHandler.AddImage(img);
                }
#pragma warning restore 0618
            }

            //Initialize a new object to track project document changes so that we can update the StartupFile Property accordingly
            this.projectDocListenerForStartupFileUpdates = new ProjectDocumentsListenerForStartupFileUpdates(this.Site, this);
            this.projectDocListenerForStartupFileUpdates.Init();

            UpdateHierarchyManager(alwaysCreate: false);

            this.idleManager = new IdleManager(this.Site);
            this.idleManager.OnIdle += this.OnIdle;

            this.fileWatcher = new FileChangeManager(serviceProvider);
            this.fileWatcher.FileChangedOnDisk += this.FileChangedOnDisk;
        }

        public override int QueryService(ref Guid guidService, out object result)
        {
            if (guidService == typeof(VSLangProj.VSProject).GUID)
            {
                result = this.VSProject;
                return VSConstants.S_OK;
            }

            return base.QueryService(ref guidService, out result);
        }

        #region abstract methods

        public abstract Type GetProjectFactoryType();
        public abstract Type GetEditorFactoryType();
        public abstract string GetProjectName();

        public virtual CommonFileNode CreateCodeFileNode(ProjectElement item)
        {
            return new CommonFileNode(this, item);
        }
        public virtual CommonFileNode CreateNonCodeFileNode(ProjectElement item)
        {
            return new CommonNonCodeFileNode(this, item);
        }
        public abstract string GetFormatList();
        public abstract Type GetGeneralPropertyPageType();
        public abstract Type GetLibraryManagerType();

        #endregion

        #region Properties

        public int ImageOffset { get; }
        /// <summary>
        /// Get the VSProject corresponding to this project
        /// </summary>
        protected internal VSLangProj.VSProject VSProject
        {
            get
            {
                if (this.vsProject == null)
                {
                    this.vsProject = new OAVSProject(this);
                }

                return this.vsProject;
            }
        }

        private IVsHierarchy InteropSafeHierarchy
        {
            get
            {
                var unknownPtr = Utilities.QueryInterfaceIUnknown(this);
                if (IntPtr.Zero == unknownPtr)
                {
                    return null;
                }
                try
                {
                    var hier = Marshal.GetObjectForIUnknown(unknownPtr) as IVsHierarchy;
                    return hier;
                }
                finally
                {
                    Marshal.Release(unknownPtr);
                }
            }
        }

        /// <summary>
        /// Indicates whether the project is currently is busy refreshing its hierarchy.
        /// </summary>
        public bool IsRefreshing { get; set; }

        /// <summary>
        /// Language specific project images
        /// </summary>
        public static ImageList ImageList { get; set; }

        public CommonPropertyPage PropertyPage { get; set; }
        #endregion

        #region overridden properties

        public override bool CanShowAllFiles => true;

        public override bool IsShowingAllFiles => this.isShowingAllFiles;

        /// <summary>
        /// Returns true if the item should be included in search results
        /// </summary>
        /// 
        // Starting with 16.0 Preview 2, it is important for the
        // project to report as not searchable. Otherwise, find in
        // files will return results in project file, and IsItemDirty
        // and SaveItem won't work properly.
        public override bool IsSearchable => false;

        /// <summary>
        /// Since we appended the language images to the base image list in the constructor,
        /// this should be the offset in the ImageList of the langauge project icon.
        /// </summary>
        [Obsolete("Use GetIconMoniker() to specify the icon and GetIconHandle() for back-compat")]
        public override int ImageIndex => this.ImageOffset + (int)CommonImageName.Project;

        public override Guid ProjectGuid => GetProjectFactoryType().GUID;
        public override string ProjectType => GetProjectName();
        internal override object Object => this.VSProject;
        #endregion

        #region overridden methods

        public override object GetAutomationObject()
        {
            if (this.automationObject == null)
            {
                this.automationObject = base.GetAutomationObject();
            }
            return this.automationObject;
        }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (cmdGroup == Microsoft.VisualStudioTools.Project.VsMenus.guidStandardCommandSet2K)
            {
                switch ((VsCommands2K)cmd)
                {
                    case VsCommands2K.ECMD_PUBLISHSELECTION:
                        if (pCmdText != IntPtr.Zero && NativeMethods.OLECMDTEXT.GetFlags(pCmdText) == NativeMethods.OLECMDTEXT.OLECMDTEXTF.OLECMDTEXTF_NAME)
                        {
                            NativeMethods.OLECMDTEXT.SetText(pCmdText, "Publish " + this.Caption);
                        }

                        if (this.IsPublishingEnabled)
                        {
                            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        }
                        else
                        {
                            result |= QueryStatusResult.SUPPORTED;
                        }
                        return VSConstants.S_OK;

                    case VsCommands2K.ECMD_PUBLISHSLNCTX:
                        if (this.IsPublishingEnabled)
                        {
                            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        }
                        else
                        {
                            result |= QueryStatusResult.SUPPORTED;
                        }
                        return VSConstants.S_OK;
                    case CommonConstants.OpenFolderInExplorerCmdId:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }
            else if (cmdGroup == this.SharedCommandGuid)
            {
                switch ((SharedCommands)cmd)
                {
                    case SharedCommands.AddExistingFolder:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        private bool IsPublishingEnabled => !string.IsNullOrWhiteSpace(GetProjectProperty(CommonConstants.PublishUrl));

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (cmdGroup == Microsoft.VisualStudioTools.Project.VsMenus.guidStandardCommandSet2K)
            {
                switch ((VsCommands2K)cmd)
                {
                    case VsCommands2K.ECMD_PUBLISHSELECTION:
                    case VsCommands2K.ECMD_PUBLISHSLNCTX:
                        Publish(PublishProjectOptions.Default, true);
                        return VSConstants.S_OK;
                    case CommonConstants.OpenFolderInExplorerCmdId:
                        Process.Start(this.ProjectHome);
                        return VSConstants.S_OK;
                }
            }
            else if (cmdGroup == this.SharedCommandGuid)
            {
                switch ((SharedCommands)cmd)
                {
                    case SharedCommands.AddExistingFolder:
                        return AddExistingFolderToNode(this);
                }
            }
            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        internal int AddExistingFolderToNode(HierarchyNode parent)
        {
            var dir = Dialogs.BrowseForDirectory(
                IntPtr.Zero,
                parent.FullPathToChildren,
                string.Format("Add Existing Folder - {0}", this.Caption));

            if (dir != null)
            {
                DropFilesOrFolders(new[] { dir }, parent);
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Publishes the project as configured by the user in the Publish option page.
        /// 
        /// If async is true this function begins the publishing and returns w/o waiting for it to complete.  No errors are reported.
        /// 
        /// If async is false this function waits for the publish to finish and raises a PublishFailedException with an
        /// inner exception indicating the underlying reason for the publishing failure.
        /// 
        /// Returns true if the publish was succeessfully started, false if the project is not configured for publishing
        /// </summary>
        public virtual bool Publish(PublishProjectOptions publishOptions, bool async)
        {
            var publishUrl = publishOptions.DestinationUrl ?? GetProjectProperty(CommonConstants.PublishUrl);
            var found = false;
            if (!string.IsNullOrWhiteSpace(publishUrl))
            {
                var url = new Url(publishUrl);

                var publishers = ((IComponentModel)this.Site.GetService(typeof(SComponentModel))).GetExtensions<IProjectPublisher>();
                foreach (var publisher in publishers)
                {
                    if (publisher.Schema == url.Uri.Scheme)
                    {
                        var project = new PublishProject(this, publishOptions);
                        Exception failure = null;
                        var frame = new DispatcherFrame();
                        var thread = new System.Threading.Thread(x =>
                        {
                            try
                            {
                                publisher.PublishFiles(project, url.Uri);
                                project.Done();
                                frame.Continue = false;
                            }
                            catch (Exception e)
                            {
                                failure = e;
                                project.Failed(e.Message);
                                frame.Continue = false;
                            }
                        });
                        thread.Start();
                        found = true;
                        if (!async)
                        {
                            Dispatcher.PushFrame(frame);
                            if (failure != null)
                            {
                                throw new PublishFailedException(string.Format("Publishing of the project {0} failed", this.Caption), failure);
                            }
                        }
                        break;
                    }
                }

                if (!found)
                {
                    var statusBar = (IVsStatusbar)this.Site.GetService(typeof(SVsStatusbar));
                    statusBar.SetText(string.Format("Publish failed: Unknown publish scheme ({0})", url.Uri.Scheme));
                }
            }
            else
            {
                var statusBar = (IVsStatusbar)this.Site.GetService(typeof(SVsStatusbar));
                statusBar.SetText(string.Format("Project is not configured for publishing in project properties."));
            }
            return found;
        }

        public virtual CommonProjectConfig MakeConfiguration(string activeConfigName)
        {
            return new CommonProjectConfig(this, activeConfigName);
        }

        /// <summary>
        /// As we don't register files/folders in the project file, removing an item is a noop.
        /// </summary>
        public override int RemoveItem(uint reserved, uint itemId, out int result)
        {
            result = 1;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Overriding main project loading method to inject our hierarachy of nodes.
        /// </summary>
        protected override void Reload()
        {
            base.Reload();

            BoldStartupItem();

            OnProjectPropertyChanged += this.CommonProjectNode_OnProjectPropertyChanged;

            bool? showAllFiles = null;
            if (this.UserBuildProject != null)
            {
                showAllFiles = GetShowAllFilesSetting(this.UserBuildProject.GetPropertyValue(CommonConstants.ProjectView));
            }

            this.isShowingAllFiles = showAllFiles ??
                GetShowAllFilesSetting(this.BuildProject.GetPropertyValue(CommonConstants.ProjectView)) ??
                false;

            this.StartWatchingFiles();

            this.currentMerger = new DiskMerger(this, this, this.ProjectHome);
        }

        private void BoldStartupItem()
        {
            string startupPath = null;
            try
            {
                startupPath = GetStartupFile();
            }
            catch (ObjectDisposedException) { } // Supress the exception as VS crashes when closing a solution.

            if (!string.IsNullOrEmpty(startupPath))
            {
                var startup = FindNodeByFullPath(startupPath);
                if (startup != null)
                {
                    BoldItem(startup, true);
                }
            }
        }

        private void StartWatchingFiles()
        {
            this.fileWatcher.ObserveFolder(this.ProjectHome);
        }

        private void StopWatchingFiles()
        {
            // stop watching old dir
            this.fileWatcher.StopObservingFolder(this.ProjectHome);
        }

        protected override void SaveMSBuildProjectFileAs(string newFileName)
        {
            base.SaveMSBuildProjectFileAs(newFileName);

            if (this.UserBuildProject != null)
            {
                this.UserBuildProject.Save(this.FileName + PerUserFileExtension);
            }
        }

        protected override void SaveMSBuildProjectFile(string filename)
        {
            base.SaveMSBuildProjectFile(filename);

            if (this.UserBuildProject != null)
            {
                this.UserBuildProject.Save(filename + PerUserFileExtension);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.HierarchyManager = null;

                var pdl = this.projectDocListenerForStartupFileUpdates;
                this.projectDocListenerForStartupFileUpdates = null;
                if (pdl != null)
                {
                    pdl.Dispose();
                }

                if (this.UserBuildProject != null)
                {
                    try
                    {
                        this.UserBuildProject.ProjectCollection.UnloadProject(this.UserBuildProject);
                    }
                    catch(InvalidOperationException)
                    {
                        // The project was already been unloaded. Ignore the exception and continue execution.
                    }
                    
                }

                this.idleManager.OnIdle -= this.OnIdle;
                this.idleManager.Dispose();

                this.fileWatcher.FileChangedOnDisk -= this.FileChangedOnDisk;
                this.fileWatcher.Dispose();
            }

            base.Dispose(disposing);
        }

        protected internal override int ShowAllFiles()
        {
            this.Site.GetUIThread().MustBeCalledFromUIThread();

            if (!QueryEditProjectFile(false))
            {
                return VSConstants.E_FAIL;
            }

            if (this.isShowingAllFiles)
            {
                UpdateShowAllFiles(this, enabled: false);
            }
            else
            {
                UpdateShowAllFiles(this, enabled: true);
                ExpandItem(EXPANDFLAGS.EXPF_ExpandFolder);
            }

            BoldStartupItem();

            this.isShowingAllFiles = !this.isShowingAllFiles;

            var newPropValue = this.isShowingAllFiles ? CommonConstants.ShowAllFiles : CommonConstants.ProjectFiles;

            var projProperty = this.BuildProject.GetProperty(CommonConstants.ProjectView);
            if (projProperty != null &&
                !projProperty.IsImported &&
                !string.IsNullOrWhiteSpace(projProperty.EvaluatedValue))
            {
                // setting is persisted in main project file, update it there.
                this.BuildProject.SetProperty(CommonConstants.ProjectView, newPropValue);
            }
            else
            {
                // save setting in user project file
                SetUserProjectProperty(CommonConstants.ProjectView, newPropValue);
            }

            // update project state
            return VSConstants.S_OK;
        }

        private void UpdateShowAllFiles(HierarchyNode node, bool enabled)
        {
            for (var curNode = node.FirstChild; curNode != null; curNode = curNode.NextSibling)
            {
                UpdateShowAllFiles(curNode, enabled);

                var allFiles = curNode.ItemNode as AllFilesProjectElement;
                if (allFiles != null)
                {
                    curNode.IsVisible = enabled;
                    if (enabled)
                    {
                        OnItemAdded(node, curNode);
                    }
                    else
                    {
                        RaiseItemDeleted(curNode);
                    }
                }
            }
        }

        private static bool? GetShowAllFilesSetting(string showAllFilesValue)
        {
            bool? showAllFiles = null;
            var showAllFilesSetting = showAllFilesValue.Trim();
            if (StringComparer.Ordinal.Equals(showAllFilesSetting, CommonConstants.ProjectFiles))
            {
                showAllFiles = false;
            }
            else if (StringComparer.Ordinal.Equals(showAllFilesSetting, CommonConstants.ShowAllFiles))
            {
                showAllFiles = true;
            }
            return showAllFiles;
        }

        private async TPL.Task MergeDiskNodesAsync(HierarchyNode curParent, string dir)
        {
            var merger = new DiskMerger(this, curParent, dir);
            while (await merger.ContinueMergeAsync(this.ParentHierarchy != null))
            {
            }
        }

        private void RemoveSubTree(HierarchyNode node)
        {
            this.Site.GetUIThread().MustBeCalledFromUIThread();
            foreach (var child in node.AllChildren)
            {
                RemoveSubTree(child);
            }
            node.Parent.RemoveChild(node);
            this.DiskNodes.TryRemove(node.Url, out _);
        }

        private static string GetFinalPathName(string dir)
        {
            using (var dirHandle = NativeMethods.CreateFile(
                dir,
                NativeMethods.FileDesiredAccess.FILE_LIST_DIRECTORY,
                NativeMethods.FileShareFlags.FILE_SHARE_DELETE |
                NativeMethods.FileShareFlags.FILE_SHARE_READ |
                NativeMethods.FileShareFlags.FILE_SHARE_WRITE,
                IntPtr.Zero,
                NativeMethods.FileCreationDisposition.OPEN_EXISTING,
                NativeMethods.FileFlagsAndAttributes.FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero
            ))
            {
                if (!dirHandle.IsInvalid)
                {
                    uint pathLen = NativeMethods.MAX_PATH + 1;
                    uint res;
                    StringBuilder filePathBuilder;
                    for (; ; )
                    {
                        filePathBuilder = new StringBuilder(checked((int)pathLen));
                        res = NativeMethods.GetFinalPathNameByHandle(
                            dirHandle,
                            filePathBuilder,
                            pathLen,
                            0
                        );
                        if (res != 0 && res < pathLen)
                        {
                            // we had enough space, and got the filename.
                            break;
                        }
                    }

                    if (res != 0)
                    {
                        Debug.Assert(filePathBuilder.ToString().StartsWith("\\\\?\\"));
                        return filePathBuilder.ToString().Substring(4);
                    }
                }
            }
            return dir;
        }

        private static bool IsRecursiveSymLink(string parentDir, string childDir)
        {
            if (IsFileSymLink(parentDir))
            {
                // figure out the real parent dir so the check below works w/ multiple
                // symlinks pointing at each other
                parentDir = GetFinalPathName(parentDir);
            }

            var finalPath = GetFinalPathName(childDir);
            // check and see if we're recursing infinitely...
            if (CommonUtils.IsSubpathOf(finalPath, parentDir))
            {
                // skip this file
                return true;
            }
            return false;
        }

        private static bool IsFileSymLink(string path)
        {
            try
            {
                return (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
            }
            catch (Exception exc) when (exc is IOException || exc is UnauthorizedAccessException)
            {
                return false;
            }
        }

        private bool IsFileHidden(string path)
        {
            if (string.IsNullOrWhiteSpace(path) ||
                StringComparer.OrdinalIgnoreCase.Equals(path, this.FileName) ||
                StringComparer.OrdinalIgnoreCase.Equals(path, this.FileName + ".user") ||
                path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".suo", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if ((path.Length >= NativeMethods.MAX_FOLDER_PATH || !Directory.Exists(path)
                && (path.Length >= NativeMethods.MAX_PATH || !File.Exists(path))))
            {
                // if the file has disappeared avoid the exception...
                // same if file is outside of path limits.
                return true; // Files/directories that don't exist should be hidden. This also fix DiskMerger when adds files that were already deleted
            }

            try
            {
                return (File.GetAttributes(path) & (FileAttributes.Hidden | FileAttributes.System)) != 0;
            }
            catch (Exception exc) when (exc is IOException || exc is UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a file which is displayed when Show All Files is enabled
        /// </summary>
        private HierarchyNode AddAllFilesFile(HierarchyNode curParent, string file)
        {
            var existing = FindNodeByFullPath(file);
            if (existing == null)
            {
                var newFile = CreateFileNode(new AllFilesProjectElement(file, GetItemType(file), this));
                AddAllFilesNode(curParent, newFile);
                return newFile;
            }
            return existing;
        }

        /// <summary>
        /// Adds a folder which is displayed when Show All files is enabled
        /// </summary>
        private HierarchyNode AddAllFilesFolder(HierarchyNode curParent, string curDir, bool hierarchyCreated = true)
        {
            var safePath = CommonUtils.EnsureEndSeparator(curDir);

            var folderNode = FindNodeByFullPath(safePath);
            if (folderNode == null)
            {
                folderNode = CreateFolderNode(new AllFilesProjectElement(safePath, "Folder", this));
                AddAllFilesNode(curParent, folderNode);

                if (hierarchyCreated)
                {
                    // Solution Explorer will expand the parent when an item is
                    // added, which we don't want
                    folderNode.ExpandItem(EXPANDFLAGS.EXPF_CollapseFolder);
                }
            }
            return folderNode;
        }

        /// <summary>
        /// Initializes and adds a file or folder visible only when Show All files is enabled
        /// </summary>
        private void AddAllFilesNode(HierarchyNode parent, HierarchyNode newNode)
        {
            newNode.IsVisible = this.IsShowingAllFiles;
            parent.AddChild(newNode);
        }

        private bool NoPendingFileSystemRescan()
        {
            return !this.fileSystemChanges.TryPeek(out var change) || change.Type != WatcherChangeTypes.All;
        }

        protected override ReferenceContainerNode CreateReferenceContainerNode()
        {
            return new CommonReferenceContainerNode(this);
        }

        private void FileChangedOnDisk(object sender, FileChangedOnDiskEventArgs e)
        {
            var file = e.FileName;
            this.QueueFileSystemChanges(new FileSystemChange(this, e.FileChange, file));
        }

        private void QueueFileSystemChanges(params FileSystemChange[] changes)
        {
            if (this.IsClosed)
            {
                return;
            }

            try
            {
                if (NoPendingFileSystemRescan())
                {
                    foreach (var change in changes)
                    {
                        this.fileSystemChanges.Enqueue(change);
                    }
                    TriggerIdle();
                }
            }
            catch (PathTooLongException)
            {
                // A rename event can be reported for a path that's too long, and then access to RenamedEventArgs
                // properties will throw this - nothing we can do other than ignoring it.
            }
        }

        /// <summary>
        /// If VS is already idle, we won't keep getting idle events, so we need to post a
        /// new event to the queue to flip away from idle and back again.
        /// </summary>
        private void TriggerIdle()
        {
            if (Interlocked.CompareExchange(ref this.idleTriggered, 1, 0) == 0)
            {
                this.Site.GetUIThread().InvokeAsync(Nop).DoNotWait();
            }
        }

        private static readonly Action Nop = () => { };

        internal void CreateSymLinkWatcher(string curDir)
        {
            curDir = CommonUtils.EnsureEndSeparator(curDir);
            this.fileWatcher.ObserveFolder(curDir);
        }

        internal bool TryDeactivateSymLinkWatcher(HierarchyNode child)
        {
            var dir = CommonUtils.EnsureEndSeparator(child.Url);
            return this.fileWatcher.StopObservingFolder(dir);
        }

        /// <summary>
        /// Handles the Idle event, this is raised on the UI thread
        /// </summary>
        private async void OnIdle(object sender, ComponentManagerEventArgs e)
        {
            Interlocked.Exchange(ref this.idleTriggered, 0);
            do
            {
                using (new DebugTimer("ProcessFileChanges while Idle", 100))
                {
                    if (this.IsClosed)
                    {
                        return;
                    }

                    FileSystemChange change = null;

                    var merger = this.currentMerger;
                    if (merger == null)
                    {
                        if (!this.fileSystemChanges.TryDequeue(out change))
                        {
                            break;
                        }
                    }

                    if (merger != null)
                    {
                        // we have more file merges to process, do this
                        // before reflecting any other pending updates...
                        if (!(await merger.ContinueMergeAsync(this.ParentHierarchy != null)))
                        {
                            this.currentMerger = null;
                        }
                        continue;
                    }
#if DEBUG
                    try
                    {
#endif
                        if (change.Type == WatcherChangeTypes.All)
                        {
                            this.currentMerger = new DiskMerger(this, this, this.ProjectHome);
                            continue;
                        }
                        else
                        {
                            await change.ProcessChangeAsync();
                        }
#if DEBUG
                    }
                    catch (Exception ex)
                    {
                        Debug.Fail("Unexpected exception while processing change", ex.ToString());
                        throw;
                    }
#endif
                }
            } while (e.ComponentManager.FContinueIdle() != 0);
        }

        public override int GetGuidProperty(int propid, out Guid guid)
        {
            if ((__VSHPROPID)propid == __VSHPROPID.VSHPROPID_PreferredLanguageSID)
            {
                guid = new Guid("{EFB9A1D6-EA71-4F38-9BA7-368C33FCE8DC}");// GetLanguageServiceType().GUID;
            }
            else
            {
                return base.GetGuidProperty(propid, out guid);
            }
            return VSConstants.S_OK;
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new CommonProjectNodeProperties(this);
        }

        public override void Close()
        {
            if (this.projectDocListenerForStartupFileUpdates != null)
            {
                this.projectDocListenerForStartupFileUpdates.Dispose();
                this.projectDocListenerForStartupFileUpdates = null;
            }

            if (this.Site.GetService(GetLibraryManagerType()) is LibraryManager libraryManager)
            {
                libraryManager.UnregisterHierarchy(this.InteropSafeHierarchy);
            }

            this.needBolding = null;

            base.Close();
        }

        public override void Load(string filename, string location, string name, uint flags, ref Guid iidProject, out int canceled)
        {
            base.Load(filename, location, name, flags, ref iidProject, out canceled);

            if (this.Site.GetService(GetLibraryManagerType()) is LibraryManager libraryManager)
            {
                libraryManager.RegisterHierarchy(this.InteropSafeHierarchy);
            }

            var shell = GetService(typeof(SVsShell)) as IVsShell;
            var infoBarUiFactory = GetService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
            MigrateToJspsInfoBar.Show(shell, infoBarUiFactory, filename);
        }

        internal IVsHierarchyItemManager HierarchyManager
        {
            get
            {
                if (this.hierarchyManager == null)
                {
                    UpdateHierarchyManager(true);
                }
                return this.hierarchyManager;
            }
            private set
            {
                if (this.hierarchyManager != null)
                {
                    this.hierarchyManager.OnItemAdded -= this.HierarchyManager_OnItemAdded;
                }
                this.hierarchyManager = value;
                if (this.hierarchyManager != null)
                {
                    this.hierarchyManager.OnItemAdded += this.HierarchyManager_OnItemAdded;

                    // We now have a hierarchy manager, so bold any items that
                    // were waiting to be bolded.
                    if (this.needBolding != null)
                    {
                        var items = this.needBolding;
                        this.needBolding = null;
                        foreach (var keyValue in items)
                        {
                            BoldItem(keyValue.Key, keyValue.Value);
                        }
                    }
                }
            }
        }

        private void UpdateHierarchyManager(bool alwaysCreate)
        {
            if (this.Site != null && (alwaysCreate || this.needBolding != null && this.needBolding.Any()))
            {
                var componentModel = this.Site.GetService(typeof(SComponentModel)) as IComponentModel;
                var newManager = componentModel?.GetService<IVsHierarchyItemManager>();

                if (newManager != this.hierarchyManager)
                {
                    this.HierarchyManager = newManager;
                }
            }
            else
            {
                this.HierarchyManager = null;
            }
        }

        private void HierarchyManager_OnItemAdded(object sender, HierarchyItemEventArgs e)
        {
            if (this.needBolding == null)
            {
                return;
            }
            if (e.Item.HierarchyIdentity.Hierarchy == GetOuterInterface<IVsUIHierarchy>())
            {
                // An item has been added to our hierarchy, so bold it if we
                // need to.
                // Typically these are references/environments, since files are
                // added lazily through a mechanism that does not raise this
                // event.
                if (this.needBolding.TryGetValue(e.Item.HierarchyIdentity.ItemID, out var isBold))
                {
                    e.Item.IsBold = isBold;
                    this.needBolding.Remove(e.Item.HierarchyIdentity.ItemID);
                    if (!this.needBolding.Any())
                    {
                        this.needBolding = null;
                    }
                }
            }
            else if (e.Item.HierarchyIdentity.Hierarchy == GetService(typeof(SVsSolution)) &&
              e.Item.HierarchyIdentity.NestedHierarchy == GetOuterInterface<IVsUIHierarchy>())
            {
                // Our project is being added to the solution, and we have
                // something to bold, so look up all pending items and force
                // them to be created.
                // Typically these are files, which are lazily created as the
                // containing folders are expanded.
                // Under VS 2010, this would cause multiple items to be added to
                // Solution Explorer, but VS 2012 fixed this issue.
                var items = this.needBolding;
                this.needBolding = null;
                foreach (var keyValue in items)
                {
                    BoldItem(keyValue.Key, keyValue.Value, force: true);
                }
            }
        }

        private void BoldItem(uint id, bool isBold, bool force = false)
        {
            if (this.HierarchyManager == null)
            {
                // We don't have a hierarchy manager yet (so really we shouldn't
                // even be here...), so defer bolding until we get one.
                if (this.needBolding == null)
                {
                    this.needBolding = new Dictionary<uint, bool>();
                }
                this.needBolding[id] = isBold;
                return;
            }

            IVsHierarchyItem item;
            if (force)
            {
                item = this.HierarchyManager.GetHierarchyItem(GetOuterInterface<IVsUIHierarchy>(), id);
            }
            else if (!this.HierarchyManager.TryGetHierarchyItem(GetOuterInterface<IVsUIHierarchy>(), id, out item))
            {
                item = null;
            }

            if (item != null)
            {
                item.IsBold = isBold;
            }
            else
            {
                // Item hasn't been created yet, so defer bolding until we get
                // the notification from the hierarchy manager.
                if (this.needBolding == null)
                {
                    this.needBolding = new Dictionary<uint, bool>();
                }
                this.needBolding[id] = isBold;
            }
        }

        public void BoldItem(HierarchyNode node, bool isBold)
        {
            BoldItem(node.ID, isBold);
        }

        /// <summary>
        /// Overriding to provide project general property page
        /// </summary>
        /// <returns></returns>
        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            var pageType = GetGeneralPropertyPageType();
            if (pageType != null)
            {
                return new[] { pageType.GUID };
            }
            return Array.Empty<Guid>();
        }

        /// <summary>
        /// Create a file node based on an msbuild item.
        /// </summary>
        /// <param name="item">The msbuild item to be analyzed</param>
        public override FileNode CreateFileNode(ProjectElement item)
        {
            Utilities.ArgumentNotNull("item", item);

            var url = item.Url;
            CommonFileNode newNode;
            if (IsCodeFile(url))
            {
                newNode = CreateCodeFileNode(item);
            }
            else
            {
                newNode = CreateNonCodeFileNode(item);
            }

            var link = item.GetMetadata(ProjectFileConstants.Link);
            if (!string.IsNullOrWhiteSpace(link) ||
                !CommonUtils.IsSubpathOf(this.ProjectHome, url))
            {
                newNode.SetIsLinkFile(true);
            }

            return newNode;
        }

        /// <summary>
        /// Create a file node based on absolute file name.
        /// </summary>
        public override FileNode CreateFileNode(string absFileName)
        {
            // Avoid adding files to the project multiple times.  Ultimately
            // we should not use project items and instead should have virtual items.

            var path = CommonUtils.GetRelativeFilePath(this.ProjectHome, absFileName);
            return CreateFileNode(new MsBuildProjectElement(this, path, GetItemType(path)));
        }

        internal virtual string GetItemType(string filename)
        {
            if (TypeScriptHelpers.IsTypeScriptFile(filename))
            {
                return NodejsConstants.NoneItemType;
            }
            else
            {
                return NodejsConstants.ContentItemType;
            }
        }

        public ProjectElement MakeProjectElement(string type, string path)
        {
            var item = this.BuildProject.AddItem(type, MSBuild.ProjectCollection.Escape(path))[0];
            return new MsBuildProjectElement(this, item);
        }

        public override int IsDirty(out int isDirty)
        {
            var hr = base.IsDirty(out isDirty);

            if (ErrorHandler.Failed(hr))
            {
                return hr;
            }

            if (isDirty == 0 && this.IsUserProjectFileDirty)
            {
                isDirty = 1;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Creates the format list for the open file dialog
        /// </summary>
        /// <param name="formatlist">The formatlist to return</param>
        /// <returns>Success</returns>
        public override int GetFormatList(out string formatlist)
        {
            formatlist = GetFormatList();
            return VSConstants.S_OK;
        }

        protected override ConfigProvider CreateConfigProvider()
        {
            return new CommonConfigProvider(this);
        }
        #endregion

        #region Methods

        /// <summary>
        /// This method retrieves an instance of a service that 
        /// allows to start a project or a file with or without debugging.
        /// </summary>
        public abstract IProjectLauncher GetLauncher();

        /// <summary>
        /// Returns resolved value of the current working directory property.
        /// </summary>
        public string GetWorkingDirectory()
        {
            var workDir = CommonUtils.UnquotePath(GetProjectProperty(CommonConstants.WorkingDirectory, resetCache: false));
            return CommonUtils.GetAbsoluteDirectoryPath(this.ProjectHome, workDir);
        }

        /// <summary>
        /// Returns resolved value of the startup file property.
        /// </summary>
        internal string GetStartupFile()
        {
            var startupFile = GetProjectProperty(CommonConstants.StartupFile, resetCache: false);

            if (string.IsNullOrEmpty(startupFile))
            {
                //No startup file is assigned
                return null;
            }

            return CommonUtils.GetAbsoluteFilePath(this.ProjectHome, startupFile);
        }

        /// <summary>
        /// Whenever project property has changed - refresh project hierarachy.
        /// </summary>
        private void CommonProjectNode_OnProjectPropertyChanged(object sender, ProjectPropertyChangedArgs e)
        {
            switch (e.PropertyName)
            {
                case CommonConstants.StartupFile:
                    RefreshStartupFile(this,
                        CommonUtils.GetAbsoluteFilePath(this.ProjectHome, e.OldValue),
                        CommonUtils.GetAbsoluteFilePath(this.ProjectHome, e.NewValue));
                    break;
            }
        }

        /// <summary>
        /// Returns first immediate child node (non-recursive) of a given type.
        /// </summary>
        private void RefreshStartupFile(HierarchyNode parent, string oldFile, string newFile)
        {
            AssertHasParentHierarchy();
            var windows = UIHierarchyUtilities.GetUIHierarchyWindow(
                this.Site,
                new Guid(ToolWindowGuids80.SolutionExplorer)) as IVsUIHierarchyWindow2;

            if (windows == null)
            {
                return;
            }

            for (var n = parent.FirstChild; n != null; n = n.NextSibling)
            {
                // TODO: Distinguish between real Urls and fake ones (eg. "References")
                if (windows != null)
                {
                    var absUrl = CommonUtils.GetAbsoluteFilePath(parent.ProjectMgr.ProjectHome, n.Url);
                    if (CommonUtils.IsSamePath(oldFile, absUrl))
                    {
                        windows.SetItemAttribute(
                            this,
                            n.ID,
                            (uint)__VSHIERITEMATTRIBUTE.VSHIERITEMATTRIBUTE_Bold,
                            false
                        );
                        ReDrawNode(n, UIHierarchyElement.Icon);
                    }
                    else if (CommonUtils.IsSamePath(newFile, absUrl))
                    {
                        windows.SetItemAttribute(
                            this,
                            n.ID,
                            (uint)__VSHIERITEMATTRIBUTE.VSHIERITEMATTRIBUTE_Bold,
                            true
                        );
                        ReDrawNode(n, UIHierarchyElement.Icon);
                    }
                }

                RefreshStartupFile(n, oldFile, newFile);
            }
        }

        /// <summary>
        /// Provide mapping from our browse objects and automation objects to our CATIDs
        /// </summary>
        protected override void InitializeCATIDs()
        {
            // The following properties classes are specific to current language so we can use their GUIDs directly
            AddCATIDMapping(typeof(OAProject), typeof(OAProject).GUID);
            // The following is not language specific and as such we need a separate GUID
            AddCATIDMapping(typeof(FolderNodeProperties), new Guid(CommonConstants.FolderNodePropertiesGuid));
            // These ones we use the same as language file nodes since both refer to files
            AddCATIDMapping(typeof(FileNodeProperties), typeof(FileNodeProperties).GUID);
            AddCATIDMapping(typeof(IncludedFileNodeProperties), typeof(IncludedFileNodeProperties).GUID);
            // Because our property page pass itself as the object to display in its grid, 
            // we need to make it have the same CATID
            // as the browse object of the project node so that filtering is possible.
            var genPropPage = GetGeneralPropertyPageType();
            if (genPropPage != null)
            {
                AddCATIDMapping(genPropPage, genPropPage.GUID);
            }
            // We could also provide CATIDs for references and the references container node, if we wanted to.
        }

        /// <summary>
        /// Creates the services exposed by this project.
        /// </summary>
        protected virtual object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(VSLangProj.VSProject) == serviceType)
            {
                service = this.VSProject;
            }
            else if (typeof(EnvDTE.Project) == serviceType)
            {
                service = GetAutomationObject();
            }

            return service;
        }

        #endregion

        #region IVsProjectSpecificEditorMap2 Members

        public int GetSpecificEditorProperty(string mkDocument, int propid, out object result)
        {
            // initialize output params
            result = null;

            //Validate input
            if (string.IsNullOrEmpty(mkDocument))
            {
                throw new ArgumentException("Was null or empty", nameof(mkDocument));
            }

            // Make sure that the document moniker passed to us is part of this project
            // We also don't care if it is not a dynamic language file node
            int hr;
            if (ErrorHandler.Failed(hr = ParseCanonicalName(mkDocument, out var itemid)))
            {
                return hr;
            }
            var hierNode = NodeFromItemId(itemid);
            if (hierNode == null || ((hierNode as CommonFileNode) == null))
            {
                return VSConstants.E_NOTIMPL;
            }

            switch (propid)
            {
                case (int)__VSPSEPROPID.VSPSEPROPID_UseGlobalEditorByDefault:
                    // don't show project default editor, every file supports Python.
                    result = false;
                    return VSConstants.E_FAIL;
                    /*case (int)__VSPSEPROPID.VSPSEPROPID_ProjectDefaultEditorName:
                        result = "Python Editor";
                        break;*/
            }

            return VSConstants.S_OK;
        }

        public int GetSpecificEditorType(string mkDocument, out Guid guidEditorType)
        {
            // Ideally we should at this point initalize a File extension to EditorFactory guid Map e.g.
            // in the registry hive so that more editors can be added without changing this part of the
            // code. Dynamic languages only make usage of one Editor Factory and therefore we will return 
            // that guid
            guidEditorType = GetEditorFactoryType().GUID;
            return VSConstants.S_OK;
        }

        public int GetSpecificLanguageService(string mkDocument, out Guid guidLanguageService)
        {
            guidLanguageService = Guid.Empty;
            return VSConstants.E_NOTIMPL;
        }

        public int SetSpecificEditorProperty(string mkDocument, int propid, object value)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IVsDeferredSaveProject Members

        /// <summary>
        /// Implements deferred save support.  Enabled by unchecking Tools->Options->Solutions and Projects->Save New Projects Created.
        /// 
        /// In this mode we save the project when the user selects Save All.  We need to move all the files in the project
        /// over to the new location.
        /// </summary>
        public virtual int SaveProjectToLocation(string pszProjectFilename)
        {
            var oldName = this.Url;
            var basePath = CommonUtils.NormalizeDirectoryPath(Path.GetDirectoryName(this.FileName));
            var newName = Path.GetDirectoryName(pszProjectFilename);

            var shell = this.Site.GetService(typeof(SVsUIShell)) as IVsUIShell;
            var vsSolution = (IVsSolution)this.GetService(typeof(SVsSolution));

            vsSolution.QueryRenameProject(this, this.FileName, pszProjectFilename, 0, out var canContinue);
            if (canContinue == 0)
            {
                return VSConstants.OLE_E_PROMPTSAVECANCELLED;
            }

            this.StopWatchingFiles();

            // we don't use RenameProjectFile because it sends the OnAfterRenameProject event too soon
            // and causes VS to think the solution has changed on disk.  We need to send it after all 
            // updates are complete.

            // save the new project to to disk
            SaveMSBuildProjectFileAs(pszProjectFilename);

            if (CommonUtils.IsSameDirectory(this.ProjectHome, basePath))
            {
                // ProjectHome was set by SaveMSBuildProjectFileAs to point to the temporary directory.
                this.BuildProject.SetProperty(CommonConstants.ProjectHome, ".");

                // save the project again w/ updated file info
                BuildProjectLocationChanged();

                // remove all the children, saving any dirty files, and collecting the list of open files
                MoveFilesForDeferredSave(this, basePath, newName);
            }
            else
            {
                // Project referenced external files only, so just update its location without moving
                // files around.
                BuildProjectLocationChanged();
            }

            SaveMSBuildProjectFile(this.FileName);

            // update VS that we've changed the project
            this.OnPropertyChanged(this, (int)__VSHPROPID.VSHPROPID_Caption, 0);

            // Update solution
            // Note we ignore the errors here because reporting them to the user isn't really helpful.
            // We've already completed all of the work to rename everything here.  If OnAfterNameProject
            // fails for some reason then telling the user it failed is just confusing because all of
            // the work is done.  And if someone wanted to prevent us from renaming the project file they
            // should have responded to QueryRenameProject.  Likewise if we can't refresh the property browser 
            // for any reason then that's not too interesting either - the users project has been saved to 
            // the new location.
            // http://pytools.codeplex.com/workitem/489
            vsSolution.OnAfterRenameProject((IVsProject)this, oldName, pszProjectFilename, 0);

            shell.RefreshPropertyBrowser(0);

            this.StartWatchingFiles();

            return VSConstants.S_OK;
        }

        private void MoveFilesForDeferredSave(HierarchyNode node, string basePath, string baseNewPath)
        {
            if (node != null)
            {
                for (var child = node.FirstChild; child != null; child = child.NextSibling)
                {
                    var docMgr = child.GetDocumentManager();
                    if (docMgr != null && docMgr.IsDirty)
                    {
                        child.ProjectMgr.SaveItem(
                            VSSAVEFLAGS.VSSAVE_Save,
                            null,
                            docMgr.DocCookie,
                            IntPtr.Zero,
                            out var cancelled
                        );
                    }

                    if (child is IDiskBasedNode diskNode)
                    {
                        diskNode.RenameForDeferredSave(basePath, baseNewPath);
                    }

                    MoveFilesForDeferredSave(child, basePath, baseNewPath);
                }
            }
        }

        #endregion
    }
}
