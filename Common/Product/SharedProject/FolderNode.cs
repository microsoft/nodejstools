// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.VisualStudioTools.Project
{
    internal class FolderNode : HierarchyNode, IDiskBasedNode
    {
        #region ctors
        /// <summary>
        /// Constructor for the FolderNode
        /// </summary>
        /// <param name="root">Root node of the hierarchy</param>
        /// <param name="relativePath">relative path from root i.e.: "NewFolder1\\NewFolder2\\NewFolder3</param>
        /// <param name="element">Associated project element</param>
        public FolderNode(ProjectNode root, ProjectElement element)
            : base(root, element)
        {
        }
        #endregion

        #region overridden properties
        public override bool CanOpenCommandPrompt => true;

        internal override string FullPathToChildren => this.Url;

        public override int SortPriority => DefaultSortOrderNode.FolderNode;
        /// <summary>
        /// This relates to the SCC glyph
        /// </summary>
        public override VsStateIcon StateIconIndex =>
                // The SCC manager does not support being asked for the state icon of a folder (result of the operation is undefined)
                VsStateIcon.STATEICON_NOSTATEICON;

        public override bool CanAddFiles => true;

        #endregion

        #region overridden methods
        protected override NodeProperties CreatePropertiesObject()
        {
            return new FolderNodeProperties(this);
        }

        protected internal override void DeleteFromStorage(string path)
        {
            this.DeleteFolder(path);
        }

        /// <summary>
        /// Get the automation object for the FolderNode
        /// </summary>
        /// <returns>An instance of the Automation.OAFolderNode type if succeeded</returns>
        public override object GetAutomationObject()
        {
            if (this.ProjectMgr == null || this.ProjectMgr.IsClosed)
            {
                return null;
            }

            return new Automation.OAFolderItem(this.ProjectMgr.GetAutomationObject() as Automation.OAProject, this);
        }

        protected override bool SupportsIconMonikers => true;
        protected override ImageMoniker GetIconMoniker(bool open)
        {
            return open ? KnownMonikers.FolderOpened : KnownMonikers.FolderClosed;
        }

        /// <summary>
        /// Rename Folder
        /// </summary>
        /// <param name="label">new Name of Folder</param>
        /// <returns>VSConstants.S_OK, if succeeded</returns>
        public override int SetEditLabel(string label)
        {
            if (this.IsBeingCreated)
            {
                return FinishFolderAdd(label, false);
            }
            else
            {
                if (String.Equals(CommonUtils.GetFileOrDirectoryName(this.Url), label, StringComparison.Ordinal))
                {
                    // Label matches current Name
                    return VSConstants.S_OK;
                }

                var newPath = CommonUtils.GetAbsoluteDirectoryPath(CommonUtils.GetParent(this.Url), label);

                // Verify that No Directory/file already exists with the new name among current children
                var existingChild = this.Parent.FindImmediateChildByName(label);
                if (existingChild != null && existingChild != this)
                {
                    return ShowFileOrFolderAlreadyExistsErrorMessage(newPath);
                }

                // Verify that No Directory/file already exists with the new name on disk.
                // Unless the path exists because it is the path to the source file also.
                if ((Directory.Exists(newPath) || File.Exists(newPath)) && !CommonUtils.IsSamePath(this.Url, newPath))
                {
                    return ShowFileOrFolderAlreadyExistsErrorMessage(newPath);
                }

                if (!this.ProjectMgr.Tracker.CanRenameItem(this.Url, newPath, VSRENAMEFILEFLAGS.VSRENAMEFILEFLAGS_Directory))
                {
                    return VSConstants.S_OK;
                }
            }

            try
            {
                var oldTriggerFlag = this.ProjectMgr.EventTriggeringFlag;
                this.ProjectMgr.EventTriggeringFlag |= ProjectNode.EventTriggering.DoNotTriggerTrackerQueryEvents;
                try
                {
                    RenameFolder(label);
                }
                finally
                {
                    this.ProjectMgr.EventTriggeringFlag = oldTriggerFlag;
                }

                //Refresh the properties in the properties window
                var shell = this.ProjectMgr.GetService(typeof(SVsUIShell)) as IVsUIShell;
                Utilities.CheckNotNull(shell, "Could not get the UI shell from the project");
                ErrorHandler.ThrowOnFailure(shell.RefreshPropertyBrowser(0));

                // Notify the listeners that the name of this folder is changed. This will
                // also force a refresh of the SolutionExplorer's node.
                this.ProjectMgr.OnPropertyChanged(this, (int)__VSHPROPID.VSHPROPID_Caption, 0);
            }
            catch (Exception e)
            {
                if (e.IsCriticalException())
                {
                    throw;
                }
                throw new InvalidOperationException(SR.GetString(SR.RenameFolder, e.Message));
            }
            return VSConstants.S_OK;
        }

        internal static string PathTooLongMessage => SR.GetString(SR.PathTooLongShortMessage);

        private int FinishFolderAdd(string label, bool wasCancelled)
        {
            // finish creation
            var filename = label.Trim();
            if (filename == "." || filename == "..")
            {
                Debug.Assert(!wasCancelled);   // cancelling leaves us with a valid label
                NativeMethods.SetErrorDescription("{0} is an invalid filename.", filename);
                return VSConstants.E_FAIL;
            }

            var path = Path.Combine(this.Parent.FullPathToChildren, label);
            if (path.Length >= NativeMethods.MAX_FOLDER_PATH)
            {
                if (wasCancelled)
                {
                    // cancelling an edit label doesn't result in the error
                    // being displayed, so we'll display one for the user.
                    Utilities.ShowMessageBox(
                        this.ProjectMgr.Site,
                        null,
                        PathTooLongMessage,
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST
                    );
                }
                else
                {
                    NativeMethods.SetErrorDescription(PathTooLongMessage);
                }
                return VSConstants.E_FAIL;
            }

            if (filename == GetItemName() || this.Parent.FindImmediateChildByName(filename) == null)
            {
                if (this.ProjectMgr.QueryFolderAdd(this.Parent, path))
                {
                    Directory.CreateDirectory(path);
                    this.IsBeingCreated = false;
                    var relativePath = CommonUtils.GetRelativeDirectoryPath(
                        this.ProjectMgr.ProjectHome,
                        CommonUtils.GetAbsoluteDirectoryPath(CommonUtils.GetParent(this.Url), label)
                    );
                    this.ItemNode.Rename(relativePath);

                    this.ProjectMgr.OnItemDeleted(this);
                    this.Parent.RemoveChild(this);
                    this.ProjectMgr.Site.GetUIThread().MustBeCalledFromUIThread();
                    this.ID = this.ProjectMgr.ItemIdMap.Add(this);
                    this.Parent.AddChild(this);

                    ExpandItem(EXPANDFLAGS.EXPF_SelectItem);

                    this.ProjectMgr.Tracker.OnFolderAdded(
                        path,
                        VSADDDIRECTORYFLAGS.VSADDDIRECTORYFLAGS_NoFlags
                    );
                }
            }
            else
            {
                Debug.Assert(!wasCancelled);    // we choose a label which didn't exist when we started the edit
                // Set error: folder already exists
                NativeMethods.SetErrorDescription("The folder {0} already exists.", filename);
                return VSConstants.E_FAIL;
            }
            return VSConstants.S_OK;
        }

        public override int MenuCommandId => VsMenus.IDM_VS_CTXT_FOLDERNODE;
        public override Guid ItemTypeGuid => VSConstants.GUID_ItemType_PhysicalFolder;

        public override string Url => CommonUtils.EnsureEndSeparator(this.ItemNode.Url);

        public override string Caption =>
                // it might have a backslash at the end... 
                // and it might consist of Grandparent\parent\this\
                CommonUtils.GetFileOrDirectoryName(this.Url);

        public override string GetMkDocument()
        {
            Debug.Assert(!string.IsNullOrEmpty(this.Url), "No url specified for this node");
            Debug.Assert(Path.IsPathRooted(this.Url), "Url should not be a relative path");

            return this.Url;
        }

        /// <summary>
        /// Recursively walks the folder nodes and redraws the state icons
        /// </summary>
        protected internal override void UpdateSccStateIcons()
        {
            for (var child = this.FirstChild; child != null; child = child.NextSibling)
            {
                child.UpdateSccStateIcons();
            }
        }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (cmdGroup == VsMenus.guidStandardCommandSet97)
            {
                switch ((VsCommands)cmd)
                {
                    case VsCommands.Copy:
                    case VsCommands.Paste:
                    case VsCommands.Cut:
                    case VsCommands.Rename:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;

                    case VsCommands.NewFolder:
                        if (!this.IsNonMemberItem)
                        {
                            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                            return VSConstants.S_OK;
                        }
                        break;
                }
            }
            else if (cmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                if ((VsCommands2K)cmd == VsCommands2K.EXCLUDEFROMPROJECT)
                {
                    result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                    return VSConstants.S_OK;
                }
            }
            else if (cmdGroup != this.ProjectMgr.SharedCommandGuid)
            {
                return (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP;
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        internal override bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
        {
            if (deleteOperation == __VSDELETEITEMOPERATION.DELITEMOP_DeleteFromStorage)
            {
                return this.ProjectMgr.CanProjectDeleteItems;
            }
            return false;
        }

        protected internal override void GetSccFiles(IList<string> files, IList<tagVsSccFilesFlags> flags)
        {
            for (var n = this.FirstChild; n != null; n = n.NextSibling)
            {
                n.GetSccFiles(files, flags);
            }
        }

        protected internal override void GetSccSpecialFiles(string sccFile, IList<string> files, IList<tagVsSccFilesFlags> flags)
        {
            for (var n = this.FirstChild; n != null; n = n.NextSibling)
            {
                n.GetSccSpecialFiles(sccFile, files, flags);
            }
        }

        #endregion

        #region virtual methods
        /// <summary>
        /// Override if your node is not a file system folder so that
        /// it does nothing or it deletes it from your storage location.
        /// </summary>
        /// <param name="path">Path to the folder to delete</param>
        public virtual void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    try
                    {
                        Directory.Delete(path, true);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // probably one or more files are read only
                        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                        {
                            // We will ignore all exceptions here and rethrow when
                            // we retry the Directory.Delete.
                            try
                            {
                                File.SetAttributes(file, FileAttributes.Normal);
                            }
                            catch (UnauthorizedAccessException)
                            {
                            }
                            catch (IOException)
                            {
                            }
                        }
                        Directory.Delete(path, true);
                    }
                }
                catch (IOException ioEx)
                {
                    // re-throw with a friendly path
                    throw new IOException(ioEx.Message.Replace(path, GetItemName()));
                }
            }
        }

        /// <summary>
        /// creates the physical directory for a folder node
        /// Override if your node does not use file system folder
        /// </summary>
        public virtual void CreateDirectory()
        {
            if (Directory.Exists(this.Url) == false)
            {
                Directory.CreateDirectory(this.Url);
            }
        }
        /// <summary>
        /// Creates a folder nodes physical directory
        /// Override if your node does not use file system folder
        /// </summary>
        /// <param name="newName"></param>
        /// <returns></returns>
        public virtual void CreateDirectory(string newName)
        {
            if (String.IsNullOrEmpty(newName))
            {
                throw new ArgumentException(SR.GetString(SR.ParameterCannotBeNullOrEmpty), "newName");
            }

            // on a new dir && enter, we get called with the same name (so do nothing if name is the same
            var strNewDir = CommonUtils.GetAbsoluteDirectoryPath(CommonUtils.GetParent(this.Url), newName);

            if (!CommonUtils.IsSameDirectory(this.Url, strNewDir))
            {
                if (Directory.Exists(strNewDir))
                {
                    throw new InvalidOperationException(SR.GetString(SR.DirectoryExistsShortMessage));
                }
                Directory.CreateDirectory(strNewDir);
            }
        }

        /// <summary>
        /// Rename the physical directory for a folder node
        /// Override if your node does not use file system folder
        /// </summary>
        /// <returns></returns>
        public virtual void RenameDirectory(string newPath)
        {
            if (Directory.Exists(this.Url))
            {
                if (CommonUtils.IsSamePath(this.Url, newPath))
                {
                    // This is a rename to the same location with (possible) capitilization changes.
                    // Directory.Move does not allow renaming to the same name so P/Invoke MoveFile to bypass this.
                    if (!NativeMethods.MoveFile(this.Url, newPath))
                    {
                        // Rather than perform error handling, Call Directory.Move and let it handle the error handling.  
                        // If this succeeds, then we didn't really have errors that needed handling.
                        Directory.Move(this.Url, newPath);
                    }
                }
                else if (Directory.Exists(newPath))
                {
                    // Directory exists and it wasn't the source.  Item cannot be moved as name exists.
                    ShowFileOrFolderAlreadyExistsErrorMessage(newPath);
                }
                else
                {
                    Directory.Move(this.Url, newPath);
                }
            }
        }

        void IDiskBasedNode.RenameForDeferredSave(string basePath, string baseNewPath)
        {
            var oldPath = Path.Combine(basePath, this.ItemNode.GetMetadata(ProjectFileConstants.Include));
            var newPath = Path.Combine(baseNewPath, this.ItemNode.GetMetadata(ProjectFileConstants.Include));
            Directory.CreateDirectory(newPath);

            this.ProjectMgr.UpdatePathForDeferredSave(oldPath, newPath);
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Renames the folder to the new name.
        /// </summary>
        public virtual void RenameFolder(string newName)
        {
            // Do the rename (note that we only do the physical rename if the leaf name changed)
            var newPath = Path.Combine(this.Parent.FullPathToChildren, newName);
            var oldPath = this.Url;
            if (!String.Equals(Path.GetFileName(this.Url), newName, StringComparison.Ordinal))
            {
                RenameDirectory(CommonUtils.GetAbsoluteDirectoryPath(this.ProjectMgr.ProjectHome, newPath));
            }

            var wasExpanded = GetIsExpanded();

            ReparentFolder(newPath);

            var oldTriggerFlag = this.ProjectMgr.EventTriggeringFlag;
            this.ProjectMgr.EventTriggeringFlag |= ProjectNode.EventTriggering.DoNotTriggerTrackerEvents;
            try
            {
                // Let all children know of the new path
                for (var child = this.FirstChild; child != null; child = child.NextSibling)
                {
                    var node = child as FolderNode;

                    if (node == null)
                    {
                        child.SetEditLabel(child.GetEditLabel());
                    }
                    else
                    {
                        node.RenameFolder(node.GetItemName());
                    }
                }
            }
            finally
            {
                this.ProjectMgr.EventTriggeringFlag = oldTriggerFlag;
            }

            this.ProjectMgr.Tracker.OnItemRenamed(oldPath, newPath, VSRENAMEFILEFLAGS.VSRENAMEFILEFLAGS_Directory);

            // Some of the previous operation may have changed the selection so set it back to us
            ExpandItem(wasExpanded ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder);
            ExpandItem(EXPANDFLAGS.EXPF_SelectItem);
        }

        /// <summary>
        /// Moves the HierarchyNode from the old path to be a child of the
        /// newly specified node.
        /// 
        /// This is a low-level operation that only updates the hierarchy and our MSBuild
        /// state.  The parents of the node must already be created. 
        /// 
        /// To do a general rename, call RenameFolder instead.
        /// </summary>
        internal void ReparentFolder(string newPath)
        {
            // Reparent the folder
            this.ProjectMgr.OnItemDeleted(this);
            this.Parent.RemoveChild(this);
            this.ProjectMgr.Site.GetUIThread().MustBeCalledFromUIThread();
            this.ID = this.ProjectMgr.ItemIdMap.Add(this);

            this.ItemNode.Rename(CommonUtils.GetRelativeDirectoryPath(this.ProjectMgr.ProjectHome, newPath));
            var parent = this.ProjectMgr.GetParentFolderForPath(newPath);
            Debug.Assert(parent != null, "ReparentFolder called without full path to parent being created");
            parent.AddChild(this);
        }

        /// <summary>
        /// Show error message if not in automation mode, otherwise throw exception
        /// </summary>
        /// <param name="newPath">path of file or folder already existing on disk</param>
        /// <returns>S_OK</returns>
        private int ShowFileOrFolderAlreadyExistsErrorMessage(string newPath)
        {
            //A file or folder with the name '{0}' already exists on disk at this location. Please choose another name.
            //If this file or folder does not appear in the Solution Explorer, then it is not currently part of your project. To view files which exist on disk, but are not in the project, select Show All Files from the Project menu.
            var errorMessage = SR.GetString(SR.FileOrFolderAlreadyExists, newPath);
            if (!Utilities.IsInAutomationFunction(this.ProjectMgr.Site))
            {
                string title = null;
                var icon = OLEMSGICON.OLEMSGICON_CRITICAL;
                var buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK;
                var defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
                Utilities.ShowMessageBox(this.ProjectMgr.Site, title, errorMessage, icon, buttons, defaultButton);
                return VSConstants.S_OK;
            }
            else
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        protected override void OnCancelLabelEdit()
        {
            if (this.IsBeingCreated)
            {
                // finish the creation
                FinishFolderAdd(GetItemName(), true);
            }
        }

        internal bool IsBeingCreated
        {
            get
            {
                return this.ProjectMgr.FolderBeingCreated == this;
            }
            set
            {
                if (value)
                {
                    this.ProjectMgr.FolderBeingCreated = this;
                }
                else
                {
                    this.ProjectMgr.FolderBeingCreated = null;
                }
            }
        }

        #endregion

        protected override void RaiseOnItemRemoved(string documentToRemove, string[] filesToBeDeleted)
        {
            var removeFlags = new VSREMOVEDIRECTORYFLAGS[1];
            this.ProjectMgr.Tracker.OnFolderRemoved(documentToRemove, removeFlags[0]);
        }
    }
}

