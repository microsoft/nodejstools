// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Navigation;
using Microsoft.VisualStudioTools.Project.Automation;
using MSBuild = Microsoft.Build.Evaluation;
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
        StartupFile
    }

    internal abstract class CommonProjectNode : ProjectNode, IVsProjectSpecificEditorMap2, IVsDeferredSaveProject
    {
        private Guid _mruPageGuid = new Guid(CommonConstants.AddReferenceMRUPageGuid);
        private VSLangProj.VSProject _vsProject = null;
        private static ImageList _imageList;
        private ProjectDocumentsListenerForStartupFileUpdates _projectDocListenerForStartupFileUpdates;
        private int _imageOffset;
        private FileSystemWatcher _watcher, _attributesWatcher;
        private int _suppressFileWatcherCount;
        private bool _isRefreshing;
        private bool _showingAllFiles;
        private object _automationObject;
        private CommonPropertyPage _propPage;
        private readonly Dictionary<string, FileSystemEventHandler> _fileChangedHandlers = new Dictionary<string, FileSystemEventHandler>();
        private Queue<FileSystemChange> _fileSystemChanges = new Queue<FileSystemChange>();
        private object _fileSystemChangesLock = new object();
        private MSBuild.Project _userBuildProject;
        private readonly Dictionary<string, FileSystemWatcher> _symlinkWatchers = new Dictionary<string, FileSystemWatcher>();
        private DiskMerger _currentMerger;
        private IdleManager _idleManager;
        private IVsHierarchyItemManager _hierarchyManager;
        private Dictionary<uint, bool> _needBolding;
        private int _idleTriggered;

        public CommonProjectNode(IServiceProvider serviceProvider, ImageList imageList)
            : base(serviceProvider)
        {
            this.CanFileNodesHaveChilds = true;
            this.SupportsProjectDesigner = true;
            if (imageList != null)
            {
                _imageList = imageList;

                //Store the number of images in ProjectNode so we know the offset of the language icons.
#pragma warning disable 0618
                this._imageOffset = this.ImageHandler.ImageList.Images.Count;
                foreach (Image img in ImageList.Images)
                {
                    this.ImageHandler.AddImage(img);
                }
#pragma warning restore 0618
            }

            //Initialize a new object to track project document changes so that we can update the StartupFile Property accordingly
            this._projectDocListenerForStartupFileUpdates = new ProjectDocumentsListenerForStartupFileUpdates(this.Site, this);
            this._projectDocListenerForStartupFileUpdates.Init();

            UpdateHierarchyManager(alwaysCreate: false);

            this._idleManager = new IdleManager(this.Site);
            this._idleManager.OnIdle += this.OnIdle;
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

        public int ImageOffset => this._imageOffset;
        /// <summary>
        /// Get the VSProject corresponding to this project
        /// </summary>
        protected internal VSLangProj.VSProject VSProject
        {
            get
            {
                if (this._vsProject == null)
                    this._vsProject = new OAVSProject(this);
                return this._vsProject;
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
        public bool IsRefreshing
        {
            get { return this._isRefreshing; }
            set { this._isRefreshing = value; }
        }

        /// <summary>
        /// Language specific project images
        /// </summary>
        public static ImageList ImageList
        {
            get
            {
                return _imageList;
            }
            set
            {
                _imageList = value;
            }
        }

        public CommonPropertyPage PropertyPage
        {
            get { return this._propPage; }
            set { this._propPage = value; }
        }

        protected internal MSBuild.Project UserBuildProject => this._userBuildProject;

        protected bool IsUserProjectFileDirty => this._userBuildProject != null &&
                    this._userBuildProject.Xml.HasUnsavedChanges;

        #endregion

        #region overridden properties

        public override bool CanShowAllFiles => true;

        public override bool IsShowingAllFiles => this._showingAllFiles;

        /// <summary>
        /// Since we appended the language images to the base image list in the constructor,
        /// this should be the offset in the ImageList of the langauge project icon.
        /// </summary>
        [Obsolete("Use GetIconMoniker() to specify the icon and GetIconHandle() for back-compat")]
        public override int ImageIndex => this._imageOffset + (int)CommonImageName.Project;

        public override Guid ProjectGuid => GetProjectFactoryType().GUID;
        public override string ProjectType => GetProjectName();
        internal override object Object => this.VSProject;
        #endregion

        #region overridden methods

        public override object GetAutomationObject()
        {
            if (this._automationObject == null)
            {
                this._automationObject = base.GetAutomationObject();
            }
            return this._automationObject;
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
                string.Format("Add Existing Folder - {0}", this.Caption)
            );
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

            // track file creation/deletes and update our glyphs when files start/stop existing for files in the project.
            if (this._watcher != null)
            {
                this._watcher.EnableRaisingEvents = false;
                this._watcher.Dispose();
            }

            var userProjectFilename = this.FileName + PerUserFileExtension;
            if (File.Exists(userProjectFilename))
            {
                this._userBuildProject = this.BuildProject.ProjectCollection.LoadProject(userProjectFilename);
            }

            bool? showAllFiles = null;
            if (this._userBuildProject != null)
            {
                showAllFiles = GetShowAllFilesSetting(this._userBuildProject.GetPropertyValue(CommonConstants.ProjectView));
            }

            this._showingAllFiles = showAllFiles ??
                GetShowAllFilesSetting(this.BuildProject.GetPropertyValue(CommonConstants.ProjectView)) ??
                false;

            this._watcher = CreateFileSystemWatcher(this.ProjectHome);
            this._attributesWatcher = CreateAttributesWatcher(this.ProjectHome);

            this._currentMerger = new DiskMerger(this, this, this.ProjectHome);
        }

        /// <summary>
        /// Called to ensure that the hierarchy's show all files nodes are in
        /// sync with the file system.
        /// </summary>
        protected void SyncFileSystem()
        {
            if (this._currentMerger == null)
            {
                this._currentMerger = new DiskMerger(this, this, this.ProjectHome);
            }
            while (this._currentMerger.ContinueMerge(this.ParentHierarchy != null))
            {
            }
            this._currentMerger = null;
        }

        private void BoldStartupItem()
        {
            var startupPath = GetStartupFile();
            if (!string.IsNullOrEmpty(startupPath))
            {
                var startup = FindNodeByFullPath(startupPath);
                if (startup != null)
                {
                    BoldItem(startup, true);
                }
            }
        }

        private FileSystemWatcher CreateFileSystemWatcher(string dir)
        {
            var watcher = new FileSystemWatcher(dir)
            {
                InternalBufferSize = 1024 * 4,  // 4k is minimum buffer size
                IncludeSubdirectories = true
            };

            // Set Event Handlers
            watcher.Created += new FileSystemEventHandler(this.FileExistanceChanged);
            watcher.Deleted += new FileSystemEventHandler(this.FileExistanceChanged);
            watcher.Renamed += new RenamedEventHandler(this.FileNameChanged);
            watcher.Changed += this.FileContentsChanged;
            watcher.Renamed += this.FileContentsChanged;
            watcher.Error += this.WatcherError;

            // Delay setting EnableRaisingEvents until everything else is initialized.
            watcher.EnableRaisingEvents = true;

            return watcher;
        }

        private FileSystemWatcher CreateAttributesWatcher(string dir)
        {
            var watcher = new FileSystemWatcher(dir)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.Attributes,
                InternalBufferSize = 1024 * 4, // 4k is minimum buffer size
            };

            // Set Event Handlers
            watcher.Changed += this.FileAttributesChanged;
            watcher.Error += this.WatcherError;

            // Delay setting EnableRaisingEvents until everything else is initialized.
            watcher.EnableRaisingEvents = true;

            return watcher;
        }

        /// <summary>
        /// When the file system watcher buffer overflows we need to scan the entire 
        /// directory for changes.
        /// </summary>
        private void WatcherError(object sender, ErrorEventArgs e)
        {
            lock (this._fileSystemChanges)
            {
                this._fileSystemChanges.Clear(); // none of the other changes matter now, we'll rescan the world
                this._currentMerger = null;  // abort any current merge now that we have a new one
                this._fileSystemChanges.Enqueue(new FileSystemChange(this, WatcherChangeTypes.All, null, watcher: sender as FileWatcher));
                TriggerIdle();
            }
        }

        protected override void SaveMSBuildProjectFileAs(string newFileName)
        {
            base.SaveMSBuildProjectFileAs(newFileName);

            if (this._userBuildProject != null)
            {
                this._userBuildProject.Save(this.FileName + PerUserFileExtension);
            }
        }

        protected override void SaveMSBuildProjectFile(string filename)
        {
            base.SaveMSBuildProjectFile(filename);

            if (this._userBuildProject != null)
            {
                this._userBuildProject.Save(filename + PerUserFileExtension);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.HierarchyManager = null;

                var pdl = this._projectDocListenerForStartupFileUpdates;
                this._projectDocListenerForStartupFileUpdates = null;
                if (pdl != null)
                {
                    pdl.Dispose();
                }

                if (this._userBuildProject != null)
                {
                    this._userBuildProject.ProjectCollection.UnloadProject(this._userBuildProject);
                }
                if (this._idleManager != null)
                {
                    this._idleManager.OnIdle -= this.OnIdle;
                    this._idleManager.Dispose();
                }
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

            if (this._showingAllFiles)
            {
                UpdateShowAllFiles(this, enabled: false);
            }
            else
            {
                UpdateShowAllFiles(this, enabled: true);
                ExpandItem(EXPANDFLAGS.EXPF_ExpandFolder);
            }

            BoldStartupItem();

            this._showingAllFiles = !this._showingAllFiles;

            var newPropValue = this._showingAllFiles ? CommonConstants.ShowAllFiles : CommonConstants.ProjectFiles;

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
            if (StringComparer.OrdinalIgnoreCase.Equals(showAllFilesSetting, CommonConstants.ProjectFiles))
            {
                showAllFiles = false;
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals(showAllFilesSetting, CommonConstants.ShowAllFiles))
            {
                showAllFiles = true;
            }
            return showAllFiles;
        }

        /// <summary>
        /// Performs merging of the file system state with the current project hierarchy, bringing them
        /// back into sync.
        /// 
        /// The class can be created, and ContinueMerge should be called until it returns false, at which
        /// point the file system has been merged.  
        /// 
        /// You can wait between calls to ContinueMerge to enable not blocking the UI.
        /// 
        /// If there were changes which came in while the DiskMerger was processing then those changes will still need
        /// to be processed after the DiskMerger completes.
        /// </summary>
        private class DiskMerger
        {
            private readonly string _initialDir;
            private readonly Stack<DirState> _remainingDirs = new Stack<DirState>();
            private readonly CommonProjectNode _project;
            private readonly FileWatcher _watcher;

            public DiskMerger(CommonProjectNode project, HierarchyNode parent, string dir, FileWatcher watcher = null)
            {
                this._project = project;
                this._initialDir = dir;
                this._remainingDirs.Push(new DirState(dir, parent));
                this._watcher = watcher;
            }

            /// <summary>
            /// Continues processing the merge request, performing a portion of the full merge possibly
            /// returning before the merge has completed.
            /// 
            /// Returns true if the merge needs to continue, or false if the merge has completed.
            /// </summary>
            public bool ContinueMerge(bool hierarchyCreated = true)
            {
                if (this._remainingDirs.Count == 0)
                {   // all done
                    if (this._watcher != null)
                    {
                        this._watcher.EnableRaisingEvents = true;
                    }
                    this._project.BoldStartupItem();
                    return false;
                }

                var dir = this._remainingDirs.Pop();
                if (!Directory.Exists(dir.Name))
                {
                    return true;
                }

                var missingChildren = new HashSet<HierarchyNode>(dir.Parent.AllChildren);
                IEnumerable<string> dirs;
                try
                {
                    dirs = Directory.EnumerateDirectories(dir.Name);
                }
                catch
                {
                    // directory was deleted, we don't have access, etc...
                    return true;
                }

                var wasExpanded = hierarchyCreated ? dir.Parent.GetIsExpanded() : false;
                foreach (var curDir in dirs)
                {
                    if (this._project.IsFileHidden(curDir))
                    {
                        continue;
                    }
                    if (IsFileSymLink(curDir))
                    {
                        if (IsRecursiveSymLink(dir.Name, curDir))
                        {
                            // don't add recursive sym links
                            continue;
                        }

                        // track symlinks, we won't get events on the directory
                        this._project.CreateSymLinkWatcher(curDir);
                    }

                    var existing = this._project.AddAllFilesFolder(dir.Parent, curDir + Path.DirectorySeparatorChar, hierarchyCreated);
                    missingChildren.Remove(existing);
                    this._remainingDirs.Push(new DirState(curDir, existing));
                }

                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(dir.Name);
                }
                catch
                {
                    // directory was deleted, we don't have access, etc...

                    // We are about to return and some of the previous operations may have affect the Parent's Expanded
                    // state.  Set it back to what it was
                    if (hierarchyCreated)
                    {
                        dir.Parent.ExpandItem(wasExpanded ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder);
                    }
                    return true;
                }

                foreach (var file in files)
                {
                    if (this._project.IsFileHidden(file))
                    {
                        continue;
                    }
                    missingChildren.Remove(this._project.AddAllFilesFile(dir.Parent, file));
                }

                // remove the excluded children which are no longer there
                foreach (var child in missingChildren)
                {
                    if (child.ItemNode.IsExcluded)
                    {
                        this._project.RemoveSubTree(child);
                    }
                }

                if (hierarchyCreated)
                {
                    dir.Parent.ExpandItem(wasExpanded ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder);
                }

                return true;
            }

            private class DirState
            {
                public readonly string Name;
                public readonly HierarchyNode Parent;

                public DirState(string name, HierarchyNode parent)
                {
                    this.Name = name;
                    this.Parent = parent;
                }
            }
        }

        private void MergeDiskNodes(HierarchyNode curParent, string dir)
        {
            var merger = new DiskMerger(this, curParent, dir);
            while (merger.ContinueMerge(this.ParentHierarchy != null))
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
            this._diskNodes.Remove(node.Url);
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
                    for (;;)
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
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        private bool IsFileHidden(string path)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(path, this.FileName) ||
                StringComparer.OrdinalIgnoreCase.Equals(path, this.FileName + ".user") ||
                StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(path), ".sln") ||
                StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(path), ".suo"))
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
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (FileNotFoundException)
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
            var folderNode = FindNodeByFullPath(curDir);
            if (folderNode == null)
            {
                folderNode = CreateFolderNode(new AllFilesProjectElement(curDir, "Folder", this));
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

        private void FileContentsChanged(object sender, FileSystemEventArgs e)
        {
            if (this.IsClosed)
            {
                return;
            }

            FileSystemEventHandler handler;
            if (this._fileChangedHandlers.TryGetValue(e.FullPath, out handler))
            {
                handler(sender, e);
            }
        }

        private void FileAttributesChanged(object sender, FileSystemEventArgs e)
        {
            lock (this._fileSystemChanges)
            {
                if (NoPendingFileSystemRescan())
                {
                    this._fileSystemChanges.Enqueue(new FileSystemChange(this, WatcherChangeTypes.Changed, e.FullPath));
                    TriggerIdle();
                }
            }
        }

        private bool NoPendingFileSystemRescan()
        {
            return this._fileSystemChanges.Count == 0 || this._fileSystemChanges.Peek()._type != WatcherChangeTypes.All;
        }

        internal void RegisterFileChangeNotification(FileNode node, FileSystemEventHandler handler)
        {
            this._fileChangedHandlers[node.Url] = handler;
        }

        internal void UnregisterFileChangeNotification(FileNode node)
        {
            this._fileChangedHandlers.Remove(node.Url);
        }

        protected override ReferenceContainerNode CreateReferenceContainerNode()
        {
            return new CommonReferenceContainerNode(this);
        }

        private void FileNameChanged(object sender, RenamedEventArgs e)
        {
            if (this.IsClosed)
            {
                return;
            }

            try
            {
                lock (this._fileSystemChanges)
                {
                    // we just generate a delete and creation here - we're just updating the hierarchy
                    // either changing icons or updating the non-project elements, so we don't need to
                    // generate rename events or anything like that.  This saves us from having to 
                    // handle updating the hierarchy in a special way for renames.
                    if (NoPendingFileSystemRescan())
                    {
                        this._fileSystemChanges.Enqueue(new FileSystemChange(this, WatcherChangeTypes.Deleted, e.OldFullPath, true));
                        this._fileSystemChanges.Enqueue(new FileSystemChange(this, WatcherChangeTypes.Created, e.FullPath, true));
                        TriggerIdle();
                    }
                }
            }
            catch (PathTooLongException)
            {
                // A rename event can be reported for a path that's too long, and then access to RenamedEventArgs
                // properties will throw this - nothing we can do other than ignoring it.
            }
        }

        private void FileExistanceChanged(object sender, FileSystemEventArgs e)
        {
            if (this.IsClosed)
            {
                return;
            }

            lock (this._fileSystemChanges)
            {
                if (NoPendingFileSystemRescan())
                {
                    this._fileSystemChanges.Enqueue(new FileSystemChange(this, e.ChangeType, e.FullPath));
                    TriggerIdle();
                }
            }
        }

        /// <summary>
        /// If VS is already idle, we won't keep getting idle events, so we need to post a
        /// new event to the queue to flip away from idle and back again.
        /// </summary>
        private void TriggerIdle()
        {
            if (Interlocked.CompareExchange(ref this._idleTriggered, 1, 0) == 0)
            {
                this.Site.GetUIThread().InvokeAsync(Nop).DoNotWait();
            }
        }

        private static readonly Action Nop = () => { };

        internal void CreateSymLinkWatcher(string curDir)
        {
            if (!CommonUtils.HasEndSeparator(curDir))
            {
                curDir = curDir + Path.DirectorySeparatorChar;
            }
            this._symlinkWatchers[curDir] = CreateFileSystemWatcher(curDir);
        }

        internal bool TryDeactivateSymLinkWatcher(HierarchyNode child)
        {
            FileSystemWatcher watcher;
            if (this._symlinkWatchers.TryGetValue(child.Url, out watcher))
            {
                this._symlinkWatchers.Remove(child.Url);
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                return true;
            }
            return false;
        }

        private void OnIdle(object sender, ComponentManagerEventArgs e)
        {
            Interlocked.Exchange(ref this._idleTriggered, 0);
            do
            {
                using (new DebugTimer("ProcessFileChanges while Idle", 100))
                {
                    if (this.IsClosed)
                    {
                        return;
                    }

                    DiskMerger merger;
                    FileSystemChange change = null;
                    lock (this._fileSystemChanges)
                    {
                        merger = this._currentMerger;
                        if (merger == null)
                        {
                            if (this._fileSystemChanges.Count == 0)
                            {
                                break;
                            }

                            change = this._fileSystemChanges.Dequeue();
                        }
                    }

                    if (merger != null)
                    {
                        // we have more file merges to process, do this
                        // before reflecting any other pending updates...
                        if (!merger.ContinueMerge(this.ParentHierarchy != null))
                        {
                            this._currentMerger = null;
                        }
                        continue;
                    }
#if DEBUG
                    try
                    {
#endif
                        if (change._type == WatcherChangeTypes.All)
                        {
                            this._currentMerger = new DiskMerger(this, this, this.ProjectHome, change._watcher);
                            continue;
                        }
                        else
                        {
                            change.ProcessChange();
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

        /// <summary>
        /// Represents an individual change to the file system.  We process these in bulk on the
        /// UI thread.
        /// </summary>
        private class FileSystemChange
        {
            private readonly CommonProjectNode _project;
            internal readonly WatcherChangeTypes _type;
            private readonly string _path;
            private readonly bool _isRename;
            internal readonly FileWatcher _watcher;

            public FileSystemChange(CommonProjectNode node, WatcherChangeTypes changeType, string path, bool isRename = false, FileWatcher watcher = null)
            {
                this._project = node;
                this._type = changeType;
                this._path = path;
                this._isRename = isRename;
                this._watcher = watcher;
            }

            public override string ToString()
            {
                return "FileSystemChange: " + this._isRename + " " + this._type + " " + this._path;
            }

            private void RedrawIcon(HierarchyNode node)
            {
                this._project.ReDrawNode(node, UIHierarchyElement.Icon);

                for (var child = node.FirstChild; child != null; child = child.NextSibling)
                {
                    RedrawIcon(child);
                }
            }

            public void ProcessChange()
            {
                var child = this._project.FindNodeByFullPath(this._path);
                if ((this._type == WatcherChangeTypes.Deleted || this._type == WatcherChangeTypes.Changed) && child == null)
                {
                    child = this._project.FindNodeByFullPath(this._path + Path.DirectorySeparatorChar);
                }
                switch (this._type)
                {
                    case WatcherChangeTypes.Deleted:
                        ChildDeleted(child);
                        break;
                    case WatcherChangeTypes.Created: ChildCreated(child); break;
                    case WatcherChangeTypes.Changed:
                        // we only care about the attributes
                        if (this._project.IsFileHidden(this._path))
                        {
                            if (child != null)
                            {
                                // attributes must of changed to hidden, remove the file
                                goto case WatcherChangeTypes.Deleted;
                            }
                        }
                        else
                        {
                            if (child == null)
                            {
                                // attributes must of changed from hidden, add the file
                                goto case WatcherChangeTypes.Created;
                            }
                        }
                        break;
                }
            }

            private void RemoveAllFilesChildren(HierarchyNode parent)
            {
                for (var current = parent.FirstChild; current != null; current = current.NextSibling)
                {
                    // remove our children first
                    RemoveAllFilesChildren(current);

                    this._project.TryDeactivateSymLinkWatcher(current);

                    // then remove us if we're an all files node
                    if (current.ItemNode is AllFilesProjectElement)
                    {
                        this._project.OnItemDeleted(current);
                        parent.RemoveChild(current);
                    }
                }
            }

            private void ChildDeleted(HierarchyNode child)
            {
                if (child != null)
                {
                    this._project.TryDeactivateSymLinkWatcher(child);
                    this._project.Site.GetUIThread().MustBeCalledFromUIThread();

                    // rapid changes can arrive out of order, if the file or directory 
                    // actually exists ignore the event.
                    if ((!File.Exists(child.Url) && !Directory.Exists(child.Url)) ||
                        this._project.IsFileHidden(child.Url))
                    {
                        if (child.ItemNode == null)
                        {
                            // nodes should all have ItemNodes, the project is special.
                            Debug.Assert(child is ProjectNode);
                            return;
                        }

                        if (child.ItemNode.IsExcluded)
                        {
                            RemoveAllFilesChildren(child);
                            // deleting a show all files item, remove the node.
                            this._project.OnItemDeleted(child);
                            child.Parent.RemoveChild(child);
                            child.Close();
                        }
                        else
                        {
                            Debug.Assert(!child.IsNonMemberItem);
                            // deleting an item in the project, fix the icon, also
                            // fix the icon of all children which we may have not
                            // received delete notifications for
                            RedrawIcon(child);
                        }
                    }
                }
            }

            private void ChildCreated(HierarchyNode child)
            {
                if (child != null)
                {
                    // creating an item which was in the project, fix the icon.
                    this._project.ReDrawNode(child, UIHierarchyElement.Icon);
                }
                else
                {
                    if (this._project.IsFileHidden(this._path))
                    {
                        // don't add hidden files/folders
                        return;
                    }

                    // creating a new item, need to create the on all files node
                    var parent = this._project.GetParentFolderForPath(this._path);

                    if (parent == null)
                    {
                        // we've hit an error while adding too many files, the file system watcher
                        // couldn't keep up.  That's alright, we'll merge the files in correctly 
                        // in a little while...
                        return;
                    }

                    var wasExpanded = parent.GetIsExpanded();

                    if (Directory.Exists(this._path))
                    {
                        if (IsFileSymLink(this._path))
                        {
                            var parentDir = CommonUtils.GetParent(this._path);
                            if (IsRecursiveSymLink(parentDir, this._path))
                            {
                                // don't add recusrive sym link directory
                                return;
                            }

                            // otherwise we're going to need a new file system watcher
                            this._project.CreateSymLinkWatcher(this._path);
                        }

                        var folderNode = this._project.AddAllFilesFolder(parent, this._path + Path.DirectorySeparatorChar);
                        var folderNodeWasExpanded = folderNode.GetIsExpanded();

                        // then add the folder nodes
                        this._project.MergeDiskNodes(folderNode, this._path);
                        this._project.OnInvalidateItems(folderNode);

                        folderNode.ExpandItem(folderNodeWasExpanded ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder);
                    }
                    else if (File.Exists(this._path))
                    { // rapid changes can arrive out of order, make sure the file still exists
                        this._project.AddAllFilesFile(parent, this._path);
                        if (StringComparer.OrdinalIgnoreCase.Equals(this._project.GetStartupFile(), this._path))
                        {
                            this._project.BoldStartupItem();
                        }
                    }

                    parent.ExpandItem(wasExpanded ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder);
                }
            }
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
            if (null != this._projectDocListenerForStartupFileUpdates)
            {
                this._projectDocListenerForStartupFileUpdates.Dispose();
                this._projectDocListenerForStartupFileUpdates = null;
            }
            var libraryManager = this.Site.GetService(GetLibraryManagerType()) as LibraryManager;
            if (null != libraryManager)
            {
                libraryManager.UnregisterHierarchy(this.InteropSafeHierarchy);
            }
            if (this._watcher != null)
            {
                this._watcher.EnableRaisingEvents = false;
                this._watcher.Dispose();
                this._watcher = null;
            }

            if (this._attributesWatcher != null)
            {
                this._attributesWatcher.EnableRaisingEvents = false;
                this._attributesWatcher.Dispose();
                this._attributesWatcher = null;
            }

            this._fileChangedHandlers.Clear();

            foreach (var pair in this._symlinkWatchers)
            {
                pair.Value.Dispose();
            }
            this._symlinkWatchers.Clear();
            this._needBolding = null;

            base.Close();
        }

        public override void Load(string filename, string location, string name, uint flags, ref Guid iidProject, out int canceled)
        {
            base.Load(filename, location, name, flags, ref iidProject, out canceled);
            var libraryManager = this.Site.GetService(GetLibraryManagerType()) as LibraryManager;
            if (null != libraryManager)
            {
                libraryManager.RegisterHierarchy(this.InteropSafeHierarchy);
            }
        }

        internal IVsHierarchyItemManager HierarchyManager
        {
            get
            {
                if (this._hierarchyManager == null)
                {
                    UpdateHierarchyManager(true);
                }
                return this._hierarchyManager;
            }
            private set
            {
                if (this._hierarchyManager != null)
                {
                    this._hierarchyManager.OnItemAdded -= this.HierarchyManager_OnItemAdded;
                }
                this._hierarchyManager = value;
                if (this._hierarchyManager != null)
                {
                    this._hierarchyManager.OnItemAdded += this.HierarchyManager_OnItemAdded;

                    // We now have a hierarchy manager, so bold any items that
                    // were waiting to be bolded.
                    if (this._needBolding != null)
                    {
                        var items = this._needBolding;
                        this._needBolding = null;
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
            if (this.Site != null && (alwaysCreate || this._needBolding != null && this._needBolding.Any()))
            {
                var componentModel = this.Site.GetService(typeof(SComponentModel)) as IComponentModel;
                var newManager = componentModel != null ?
                    componentModel.GetService<IVsHierarchyItemManager>() :
                    null;

                if (newManager != this._hierarchyManager)
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
            if (this._needBolding == null)
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
                bool isBold;
                if (this._needBolding.TryGetValue(e.Item.HierarchyIdentity.ItemID, out isBold))
                {
                    e.Item.IsBold = isBold;
                    this._needBolding.Remove(e.Item.HierarchyIdentity.ItemID);
                    if (!this._needBolding.Any())
                    {
                        this._needBolding = null;
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
                var items = this._needBolding;
                this._needBolding = null;
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
                if (this._needBolding == null)
                {
                    this._needBolding = new Dictionary<uint, bool>();
                }
                this._needBolding[id] = isBold;
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
                if (this._needBolding == null)
                {
                    this._needBolding = new Dictionary<uint, bool>();
                }
                this._needBolding[id] = isBold;
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
            if (IsCodeFile(filename))
            {
                return "Compile";
            }
            else
            {
                return "Content";
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
        public abstract IProjectLauncher/*!*/ GetLauncher();

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

        /// <summary>
        /// Set value of user project property
        /// </summary>
        /// <param name="propertyName">Name of property</param>
        /// <param name="propertyValue">Value of property</param>
        public virtual void SetUserProjectProperty(string propertyName, string propertyValue)
        {
            Utilities.ArgumentNotNull("propertyName", propertyName);

            if (this._userBuildProject == null)
            {
                // user project file doesn't exist yet, create it.
                // We set the content of user file explictly so VS2013 won't add ToolsVersion="12" which would result in incompatibility with VS2010,2012   
                var root = Microsoft.Build.Construction.ProjectRootElement.Create(this.BuildProject.ProjectCollection);
                root.ToolsVersion = "4.0";
                this._userBuildProject = new MSBuild.Project(root, null, null, this.BuildProject.ProjectCollection);
                this._userBuildProject.FullPath = this.FileName + PerUserFileExtension;
            }
            this._userBuildProject.SetProperty(propertyName, propertyValue ?? string.Empty);
        }

        /// <summary>
        /// Get value of user project property
        /// </summary>
        /// <param name="propertyName">Name of property</param>
        public virtual string GetUserProjectProperty(string propertyName)
        {
            Utilities.ArgumentNotNull("propertyName", propertyName);

            if (this._userBuildProject == null)
                return null;

            // If user project file exists during project load/reload userBuildProject is initiated 
            return this._userBuildProject.GetPropertyValue(propertyName);
        }

        #endregion

        #region IVsProjectSpecificEditorMap2 Members

        public int GetSpecificEditorProperty(string mkDocument, int propid, out object result)
        {
            // initialize output params
            result = null;

            //Validate input
            if (string.IsNullOrEmpty(mkDocument))
                throw new ArgumentException("Was null or empty", "mkDocument");

            // Make sure that the document moniker passed to us is part of this project
            // We also don't care if it is not a dynamic language file node
            uint itemid;
            int hr;
            if (ErrorHandler.Failed(hr = ParseCanonicalName(mkDocument, out itemid)))
            {
                return hr;
            }
            var hierNode = NodeFromItemId(itemid);
            if (hierNode == null || ((hierNode as CommonFileNode) == null))
                return VSConstants.E_NOTIMPL;

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

            int canContinue;
            vsSolution.QueryRenameProject(this, this.FileName, pszProjectFilename, 0, out canContinue);
            if (canContinue == 0)
            {
                return VSConstants.OLE_E_PROMPTSAVECANCELLED;
            }

            this._watcher.EnableRaisingEvents = false;
            this._watcher.Dispose();

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

            this._watcher = CreateFileSystemWatcher(this.ProjectHome);
            this._attributesWatcher = CreateAttributesWatcher(this.ProjectHome);

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
                        int cancelled;
                        child.ProjectMgr.SaveItem(
                            VSSAVEFLAGS.VSSAVE_Save,
                            null,
                            docMgr.DocCookie,
                            IntPtr.Zero,
                            out cancelled
                        );
                    }

                    var diskNode = child as IDiskBasedNode;
                    if (diskNode != null)
                    {
                        diskNode.RenameForDeferredSave(basePath, baseNewPath);
                    }

                    MoveFilesForDeferredSave(child, basePath, baseNewPath);
                }
            }
        }

        #endregion

        internal void SuppressFileChangeNotifications()
        {
            this._watcher.EnableRaisingEvents = false;
            this._suppressFileWatcherCount++;
        }

        internal void RestoreFileChangeNotifications()
        {
            if (--this._suppressFileWatcherCount == 0)
            {
                this._watcher.EnableRaisingEvents = true;
            }
        }
    }
}

