// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.VisualStudioTools.Project
{
    internal class FileNode : HierarchyNode, IDiskBasedNode
    {
        private bool _isLinkFile;
        private uint _docCookie;
        private static readonly string[] _defaultOpensWithDesignViewExtensions = new[] { ".aspx", ".ascx", ".asax", ".asmx", ".xsd", ".resource", ".xaml" };
        private static readonly string[] _supportsDesignViewExtensions = new[] { ".aspx", ".ascx", ".asax", ".asmx" };
        private static readonly string[] _supportsDesignViewSubTypes = new[] { ProjectFileAttributeValue.Code, ProjectFileAttributeValue.Form, ProjectFileAttributeValue.UserControl, ProjectFileAttributeValue.Component, ProjectFileAttributeValue.Designer };
        private string _caption;

        #region overriden Properties

        public override bool DefaultOpensWithDesignView
        {
            get
            {
                // ASPX\ASCX files support design view but should be opened by default with
                // LOGVIEWID_Primary - this is because they support design and html view which
                // is a tools option setting for them. If we force designview this option
                // gets bypassed. We do a similar thing for asax/asmx/xsd. By doing so, we don't force
                // the designer to be invoked when double-clicking on the - it will now go through the
                // shell's standard open mechanism.
                var extension = Path.GetExtension(this.Url);
                return !_defaultOpensWithDesignViewExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) &&
                    !this.IsCodeBehindFile &&
                    this.SupportsDesignView;
            }
        }

        public override bool SupportsDesignView
        {
            get
            {
                if (this.ItemNode != null && !this.ItemNode.IsExcluded)
                {
                    var extension = Path.GetExtension(this.Url);
                    if (_supportsDesignViewExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) ||
                        this.IsCodeBehindFile)
                    {
                        return true;
                    }
                    else
                    {
                        var subType = this.ItemNode.GetMetadata("SubType");
                        if (subType != null && _supportsDesignViewExtensions.Contains(subType, StringComparer.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public override bool IsNonMemberItem => this.ItemNode is AllFilesProjectElement;

        /// <summary>
        /// overwrites of the generic hierarchyitem.
        /// </summary>
        [System.ComponentModel.BrowsableAttribute(false)]
        public override string Caption => this._caption;

        private void UpdateCaption()
        {
            // Use LinkedIntoProjectAt property if available
            var caption = this.ItemNode.GetMetadata(ProjectFileConstants.LinkedIntoProjectAt);
            if (caption == null || caption.Length == 0)
            {
                // Otherwise use filename
                caption = this.ItemNode.GetMetadata(ProjectFileConstants.Include);
                caption = Path.GetFileName(caption);
            }
            this._caption = caption;
        }

        public override string GetEditLabel()
        {
            if (this.IsLinkFile)
            {
                // cannot rename link files
                return null;
            }
            return base.GetEditLabel();
        }

        public uint DocCookie
        {
            get
            {
                return this._docCookie;
            }
            set
            {
                this._docCookie = value;
            }
        }

        public override bool IsLinkFile => this._isLinkFile;

        internal void SetIsLinkFile(bool value)
        {
            this._isLinkFile = value;
        }

        protected override VSOVERLAYICON OverlayIconIndex
        {
            get
            {
                if (this.IsLinkFile)
                {
                    return VSOVERLAYICON.OVERLAYICON_SHORTCUT;
                }
                return VSOVERLAYICON.OVERLAYICON_NONE;
            }
        }

        public override Guid ItemTypeGuid => VSConstants.GUID_ItemType_PhysicalFile;
        public override int MenuCommandId => VsMenus.IDM_VS_CTXT_ITEMNODE;
        public override string Url => this.ItemNode.Url;

        #endregion

        #region ctor
        /// <summary>
        /// Constructor for the FileNode
        /// </summary>
        /// <param name="root">Root of the hierarchy</param>
        /// <param name="e">Associated project element</param>
        public FileNode(ProjectNode root, ProjectElement element)
            : base(root, element)
        {
            UpdateCaption();
        }
        #endregion

        #region overridden methods
        protected override NodeProperties CreatePropertiesObject()
        {
            if (this.IsLinkFile)
            {
                return new LinkFileNodeProperties(this);
            }
            else if (this.IsNonMemberItem)
            {
                return new ExcludedFileNodeProperties(this);
            }

            return new IncludedFileNodeProperties(this);
        }

        /// <summary>
        /// Get an instance of the automation object for a FileNode
        /// </summary>
        /// <returns>An instance of the Automation.OAFileNode if succeeded</returns>
        public override object GetAutomationObject()
        {
            if (this.ProjectMgr == null || this.ProjectMgr.IsClosed)
            {
                return null;
            }

            return new Automation.OAFileItem(this.ProjectMgr.GetAutomationObject() as Automation.OAProject, this);
        }

        /// <summary>
        /// Renames a file node.
        /// </summary>
        /// <param name="label">The new name.</param>
        /// <returns>An errorcode for failure or S_OK.</returns>
        /// <exception cref="InvalidOperationException" if the file cannot be validated>
        /// <devremark> 
        /// We are going to throw instead of showing messageboxes, since this method is called from various places where a dialog box does not make sense.
        /// For example the FileNodeProperties are also calling this method. That should not show directly a messagebox.
        /// Also the automation methods are also calling SetEditLabel
        /// </devremark>

        public override int SetEditLabel(string label)
        {
            // IMPORTANT NOTE: This code will be called when a parent folder is renamed. As such, it is
            //                 expected that we can be called with a label which is the same as the current
            //                 label and this should not be considered a NO-OP.
            if (this.ProjectMgr == null || this.ProjectMgr.IsClosed)
            {
                return VSConstants.E_FAIL;
            }

            // Validate the filename. 
            if (string.IsNullOrEmpty(label))
            {
                throw new InvalidOperationException(SR.GetString(SR.ErrorInvalidFileName, label));
            }
            else if (label.Length > NativeMethods.MAX_PATH)
            {
                throw new InvalidOperationException(SR.GetString(SR.PathTooLong, label));
            }
            else if (Utilities.IsFileNameInvalid(label))
            {
                throw new InvalidOperationException(SR.GetString(SR.ErrorInvalidFileName, label));
            }

            for (var n = this.Parent.FirstChild; n != null; n = n.NextSibling)
            {
                // TODO: Distinguish between real Urls and fake ones (eg. "References")
                if (n != this && StringComparer.OrdinalIgnoreCase.Equals(n.GetItemName(), label))
                {
                    //A file or folder with the name '{0}' already exists on disk at this location. Please choose another name.
                    //If this file or folder does not appear in the Solution Explorer, then it is not currently part of your project. To view files which exist on disk, but are not in the project, select Show All Files from the Project menu.
                    throw new InvalidOperationException(SR.GetString(SR.FileOrFolderAlreadyExists, label));
                }
            }

            var fileName = Path.GetFileNameWithoutExtension(label);

            // Verify that the file extension is unchanged
            var strRelPath = Path.GetFileName(this.ItemNode.GetMetadata(ProjectFileConstants.Include));
            if (!Utilities.IsInAutomationFunction(this.ProjectMgr.Site) &&
                !StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(strRelPath), Path.GetExtension(label)))
            {
                // Prompt to confirm that they really want to change the extension of the file
                var message = SR.GetString(SR.ConfirmExtensionChange, label);
                var shell = this.ProjectMgr.Site.GetService(typeof(SVsUIShell)) as IVsUIShell;

                Utilities.CheckNotNull(shell, "Could not get the UI shell from the project");

                if (!VsShellUtilities.PromptYesNo(message, null, OLEMSGICON.OLEMSGICON_INFO, shell))
                {
                    // The user cancelled the confirmation for changing the extension.
                    // Return S_OK in order not to show any extra dialog box
                    return VSConstants.S_OK;
                }
            }

            // Build the relative path by looking at folder names above us as one scenarios
            // where we get called is when a folder above us gets renamed (in which case our path is invalid)
            var parent = this.Parent;
            while (parent != null && (parent is FolderNode))
            {
                strRelPath = Path.Combine(parent.GetItemName(), strRelPath);
                parent = parent.Parent;
            }

            return SetEditLabel(label, strRelPath);
        }

        public override string GetMkDocument()
        {
            Debug.Assert(!string.IsNullOrEmpty(this.Url), "No url specified for this node");
            Debug.Assert(Path.IsPathRooted(this.Url), "Url should not be a relative path");

            return this.Url;
        }

        /// <summary>
        /// Delete the item corresponding to the specified path from storage.
        /// </summary>
        /// <param name="path"></param>
        protected internal override void DeleteFromStorage(string path)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal); // make sure it's not readonly.
                File.Delete(path);
            }
        }

        /// <summary>
        /// Rename the underlying document based on the change the user just made to the edit label.
        /// </summary>
        protected internal override int SetEditLabel(string label, string relativePath)
        {
            var returnValue = VSConstants.S_OK;
            var oldId = this.ID;
            var strSavePath = Path.GetDirectoryName(relativePath);

            strSavePath = CommonUtils.GetAbsoluteDirectoryPath(this.ProjectMgr.ProjectHome, strSavePath);
            var newName = Path.Combine(strSavePath, label);

            if (StringComparer.OrdinalIgnoreCase.Equals(newName, this.Url))
            {
                // This is really a no-op (including changing case), so there is nothing to do
                return VSConstants.S_FALSE;
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals(newName, this.Url))
            {
                // This is a change of file casing only.
            }
            else
            {
                // If the renamed file already exists then quit (unless it is the result of the parent having done the move).
                if (IsFileOnDisk(newName)
                    && (IsFileOnDisk(this.Url)
                    || !StringComparer.OrdinalIgnoreCase.Equals(Path.GetFileName(newName), Path.GetFileName(this.Url))))
                {
                    throw new InvalidOperationException(SR.GetString(SR.FileCannotBeRenamedToAnExistingFile, label));
                }
                else if (newName.Length > NativeMethods.MAX_PATH)
                {
                    throw new InvalidOperationException(SR.GetString(SR.PathTooLong, label));
                }
            }

            var oldName = this.Url;
            // must update the caption prior to calling RenameDocument, since it may
            // cause queries of that property (such as from open editors).
            var oldrelPath = this.ItemNode.GetMetadata(ProjectFileConstants.Include);

            try
            {
                if (!RenameDocument(oldName, newName))
                {
                    this.ItemNode.Rename(oldrelPath);
                }

                if (this is DependentFileNode)
                {
                    this.ProjectMgr.OnInvalidateItems(this.Parent);
                }
            }
            catch (Exception e)
            {
                // Just re-throw the exception so we don't get duplicate message boxes.
                Trace.WriteLine("Exception : " + e.Message);
                this.RecoverFromRenameFailure(newName, oldrelPath);
                returnValue = Marshal.GetHRForException(e);
                throw;
            }
            // Return S_FALSE if the hierarchy item id has changed.  This forces VS to flush the stale
            // hierarchy item id.
            if (returnValue == (int)VSConstants.S_OK || returnValue == (int)VSConstants.S_FALSE || returnValue == VSConstants.OLE_E_PROMPTSAVECANCELLED)
            {
                return (oldId == this.ID) ? VSConstants.S_OK : (int)VSConstants.S_FALSE;
            }

            return returnValue;
        }

        /// <summary>
        /// Returns a specific Document manager to handle files
        /// </summary>
        /// <returns>Document manager object</returns>
        protected internal override DocumentManager GetDocumentManager()
        {
            return new FileDocumentManager(this);
        }

        public override int QueryService(ref Guid guidService, out object result)
        {
            if (guidService == typeof(EnvDTE.Project).GUID)
            {
                result = this.ProjectMgr.GetAutomationObject();
                return VSConstants.S_OK;
            }
            else if (guidService == typeof(EnvDTE.ProjectItem).GUID)
            {
                result = GetAutomationObject();
                return VSConstants.S_OK;
            }

            return base.QueryService(ref guidService, out result);
        }

        /// <summary>
        /// Called by the drag&drop implementation to ask the node
        /// which is being dragged/droped over which nodes should
        /// process the operation.
        /// This allows for dragging to a node that cannot contain
        /// items to let its parent accept the drop, while a reference
        /// node delegate to the project and a folder/project node to itself.
        /// </summary>
        /// <returns></returns>
        protected internal override HierarchyNode GetDragTargetHandlerNode()
        {
            Debug.Assert(this.ProjectMgr != null, " The project manager is null for the filenode");
            HierarchyNode handlerNode = this;
            while (handlerNode != null && !(handlerNode is ProjectNode || handlerNode is FolderNode))
                handlerNode = handlerNode.Parent;
            if (handlerNode == null)
                handlerNode = this.ProjectMgr;
            return handlerNode;
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (this.ProjectMgr == null || this.ProjectMgr.IsClosed)
            {
                return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
            }

            // Exec on special filenode commands
            if (cmdGroup == VsMenus.guidStandardCommandSet97)
            {
                IVsWindowFrame windowFrame = null;

                switch ((VsCommands)cmd)
                {
                    case VsCommands.ViewCode:
                        return ((FileDocumentManager)this.GetDocumentManager()).Open(false, false, VSConstants.LOGVIEWID_Code, out windowFrame, WindowFrameShowAction.Show);

                    case VsCommands.ViewForm:
                        return ((FileDocumentManager)this.GetDocumentManager()).Open(false, false, VSConstants.LOGVIEWID_Designer, out windowFrame, WindowFrameShowAction.Show);

                    case VsCommands.Open:
                        return ((FileDocumentManager)this.GetDocumentManager()).Open(false, false, WindowFrameShowAction.Show);

                    case VsCommands.OpenWith:
                        return ((FileDocumentManager)this.GetDocumentManager()).Open(false, true, VSConstants.LOGVIEWID_UserChooseView, out windowFrame, WindowFrameShowAction.Show);
                }
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
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

                    case VsCommands.ViewCode:
                    //case VsCommands.Delete: goto case VsCommands.OpenWith;
                    case VsCommands.Open:
                    case VsCommands.OpenWith:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
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
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        protected override void DoDefaultAction()
        {
            var manager = this.GetDocumentManager() as FileDocumentManager;
            Utilities.CheckNotNull(manager, "Could not get the FileDocumentManager");
            manager.Open(false, false, WindowFrameShowAction.Show);
        }

        /// <summary>
        /// Performs a SaveAs operation of an open document. Called from SaveItem after the running document table has been updated with the new doc data.
        /// </summary>
        /// <param name="docData">A pointer to the document in the rdt</param>
        /// <param name="newFilePath">The new file path to the document</param>
        /// <returns></returns>
        internal override int AfterSaveItemAs(IntPtr docData, string newFilePath)
        {
            Utilities.ArgumentNotNullOrEmpty("newFilePath", newFilePath);

            var returnCode = VSConstants.S_OK;
            newFilePath = newFilePath.Trim();

            //Identify if Path or FileName are the same for old and new file
            var newDirectoryName = CommonUtils.NormalizeDirectoryPath(Path.GetDirectoryName(newFilePath));
            var oldDirectoryName = CommonUtils.NormalizeDirectoryPath(Path.GetDirectoryName(this.GetMkDocument()));
            var isSamePath = CommonUtils.IsSameDirectory(newDirectoryName, oldDirectoryName);
            var isSameFile = CommonUtils.IsSamePath(newFilePath, this.Url);

            //Get target container
            HierarchyNode targetContainer = null;
            var isLink = false;
            if (isSamePath)
            {
                targetContainer = this.Parent;
            }
            else if (!CommonUtils.IsSubpathOf(this.ProjectMgr.ProjectHome, newDirectoryName))
            {
                targetContainer = this.Parent;
                isLink = true;
            }
            else if (CommonUtils.IsSameDirectory(this.ProjectMgr.ProjectHome, newDirectoryName))
            {
                //the projectnode is the target container
                targetContainer = this.ProjectMgr;
            }
            else
            {
                //search for the target container among existing child nodes
                targetContainer = this.ProjectMgr.FindNodeByFullPath(newDirectoryName);
                if (targetContainer != null && (targetContainer is FileNode))
                {
                    // We already have a file node with this name in the hierarchy.
                    throw new InvalidOperationException(SR.GetString(SR.FileAlreadyExistsAndCannotBeRenamed, Path.GetFileName(newFilePath)));
                }
            }

            if (targetContainer == null)
            {
                // Add a chain of subdirectories to the project.
                var relativeUri = CommonUtils.GetRelativeDirectoryPath(this.ProjectMgr.ProjectHome, newDirectoryName);
                targetContainer = this.ProjectMgr.CreateFolderNodes(relativeUri);
            }
            Utilities.CheckNotNull(targetContainer, "Could not find a target container");

            //Suspend file changes while we rename the document
            var oldrelPath = this.ItemNode.GetMetadata(ProjectFileConstants.Include);
            var oldName = CommonUtils.GetAbsoluteFilePath(this.ProjectMgr.ProjectHome, oldrelPath);
            var sfc = new SuspendFileChanges(this.ProjectMgr.Site, oldName);
            sfc.Suspend();

            try
            {
                // Rename the node.
                DocumentManager.UpdateCaption(this.ProjectMgr.Site, Path.GetFileName(newFilePath), docData);
                // Check if the file name was actually changed.
                // In same cases (e.g. if the item is a file and the user has changed its encoding) this function
                // is called even if there is no real rename.
                if (!isSameFile || (this.Parent.ID != targetContainer.ID))
                {
                    // The path of the file is changed or its parent is changed; in both cases we have
                    // to rename the item.
                    if (isLink != this.IsLinkFile)
                    {
                        if (isLink)
                        {
                            var newPath = CommonUtils.GetRelativeFilePath(
                                this.ProjectMgr.ProjectHome,
                                Path.Combine(Path.GetDirectoryName(this.Url), Path.GetFileName(newFilePath))
                            );

                            this.ItemNode.SetMetadata(ProjectFileConstants.Link, newPath);
                        }
                        else
                        {
                            this.ItemNode.SetMetadata(ProjectFileConstants.Link, null);
                        }
                        SetIsLinkFile(isLink);
                    }

                    RenameFileNode(oldName, newFilePath, targetContainer);
                    this.ProjectMgr.OnInvalidateItems(this.Parent);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception : " + e.Message);
                this.RecoverFromRenameFailure(newFilePath, oldrelPath);
                throw;
            }
            finally
            {
                sfc.Resume();
            }

            return returnCode;
        }

        /// <summary>
        /// Determines if this is node a valid node for painting the default file icon.
        /// </summary>
        /// <returns></returns>
        protected override bool CanShowDefaultIcon()
        {
            var moniker = this.GetMkDocument();

            return File.Exists(moniker);
        }

        #endregion

        #region virtual methods

        public override object GetProperty(int propId)
        {
            switch ((__VSHPROPID)propId)
            {
                case __VSHPROPID.VSHPROPID_ItemDocCookie:
                    if (this.DocCookie != 0)
                        return (IntPtr)this.DocCookie; //cast to IntPtr as some callers expect VT_INT
                    break;
            }
            return base.GetProperty(propId);
        }

        public virtual string FileName
        {
            get
            {
                return this.GetItemName();
            }
            set
            {
                this.SetEditLabel(value);
            }
        }

        /// <summary>
        /// Determine if this item is represented physical on disk and shows a messagebox in case that the file is not present and a UI is to be presented.
        /// </summary>
        /// <param name="showMessage">true if user should be presented for UI in case the file is not present</param>
        /// <returns>true if file is on disk</returns>
        internal protected virtual bool IsFileOnDisk(bool showMessage)
        {
            var fileExist = IsFileOnDisk(this.Url);

            if (!fileExist && showMessage && !Utilities.IsInAutomationFunction(this.ProjectMgr.Site))
            {
                var message = SR.GetString(SR.ItemDoesNotExistInProjectDirectory, GetItemName());
                var title = string.Empty;
                var icon = OLEMSGICON.OLEMSGICON_CRITICAL;
                var buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK;
                var defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
                Utilities.ShowMessageBox(this.ProjectMgr.Site, title, message, icon, buttons, defaultButton);
            }

            return fileExist;
        }

        /// <summary>
        /// Determine if the file represented by "path" exist in storage.
        /// Override this method if your files are not persisted on disk.
        /// </summary>
        /// <param name="path">Url representing the file</param>
        /// <returns>True if the file exist</returns>
        internal protected virtual bool IsFileOnDisk(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Renames the file in the hierarchy by removing old node and adding a new node in the hierarchy.
        /// </summary>
        /// <param name="oldFileName">The old file name.</param>
        /// <param name="newFileName">The new file name</param>
        /// <param name="newParentId">The new parent id of the item.</param>
        /// <returns>The newly added FileNode.</returns>
        /// <remarks>While a new node will be used to represent the item, the underlying MSBuild item will be the same and as a result file properties saved in the project file will not be lost.</remarks>
        internal FileNode RenameFileNode(string oldFileName, string newFileName, HierarchyNode newParent)
        {
            if (CommonUtils.IsSamePath(oldFileName, newFileName))
            {
                // We do not want to rename the same file
                return null;
            }

            //If we are included in the project and our parent isn't then
            //we need to bring our parent into the project
            if (!this.IsNonMemberItem && newParent.IsNonMemberItem)
            {
                ErrorHandler.ThrowOnFailure(newParent.IncludeInProject(false));
            }

            // Retrieve child nodes to add later.
            var childNodes = this.GetChildNodes();

            FileNode renamedNode;
            using (this.ProjectMgr.ExtensibilityEventsDispatcher.Suspend())
            {
                // Remove this from its parent.
                this.ProjectMgr.OnItemDeleted(this);
                this.Parent.RemoveChild(this);

                // Update name in MSBuild
                this.ItemNode.Rename(CommonUtils.GetRelativeFilePath(this.ProjectMgr.ProjectHome, newFileName));

                // Request a new file node be made.  This is used to replace the old file node.  This way custom
                // derived FileNode types will be used and correctly associated on rename.  This is useful for things 
                // like .txt -> .js where the file would now be able to be a startup project/file.
                renamedNode = this.ProjectMgr.CreateFileNode(this.ItemNode);

                renamedNode.ItemNode.RefreshProperties();
                renamedNode.UpdateCaption();
                newParent.AddChild(renamedNode);
                renamedNode.Parent = newParent;
            }

            UpdateCaption();
            this.ProjectMgr.ReDrawNode(renamedNode, UIHierarchyElement.Caption);

            renamedNode.ProjectMgr.ExtensibilityEventsDispatcher.FireItemRenamed(this, oldFileName);

            //Update the new document in the RDT.
            DocumentManager.RenameDocument(renamedNode.ProjectMgr.Site, oldFileName, newFileName, renamedNode.ID);

            //Select the new node in the hierarchy
            renamedNode.ExpandItem(EXPANDFLAGS.EXPF_SelectItem);

            // Add children to new node and rename them appropriately.
            childNodes.ForEach(x => renamedNode.AddChild(x));
            RenameChildNodes(renamedNode);

            return renamedNode;
        }

        /// <summary>
        /// Rename all childnodes
        /// </summary>
        /// <param name="newFileNode">The newly added Parent node.</param>
        protected virtual void RenameChildNodes(FileNode parentNode)
        {
            foreach (var childNode in GetChildNodes().OfType<FileNode>())
            {
                string newfilename;
                if (childNode.HasParentNodeNameRelation)
                {
                    var relationalName = childNode.Parent.GetRelationalName();
                    var extension = childNode.GetRelationNameExtension();
                    newfilename = relationalName + extension;
                    newfilename = CommonUtils.GetAbsoluteFilePath(Path.GetDirectoryName(childNode.Parent.GetMkDocument()), newfilename);
                }
                else
                {
                    newfilename = CommonUtils.GetAbsoluteFilePath(Path.GetDirectoryName(childNode.Parent.GetMkDocument()), childNode.GetItemName());
                }

                childNode.RenameDocument(childNode.GetMkDocument(), newfilename);

                //We must update the DependsUpon property since the rename operation will not do it if the childNode is not renamed
                //which happens if the is no name relation between the parent and the child
                var dependentOf = childNode.ItemNode.GetMetadata(ProjectFileConstants.DependentUpon);
                if (!string.IsNullOrEmpty(dependentOf))
                {
                    childNode.ItemNode.SetMetadata(ProjectFileConstants.DependentUpon, childNode.Parent.ItemNode.GetMetadata(ProjectFileConstants.Include));
                }
            }
        }

        /// <summary>
        /// Tries recovering from a rename failure.
        /// </summary>
        /// <param name="fileThatFailed"> The file that failed to be renamed.</param>
        /// <param name="originalFileName">The original filenamee</param>
        protected virtual void RecoverFromRenameFailure(string fileThatFailed, string originalFileName)
        {
            if (this.ItemNode != null && !string.IsNullOrEmpty(originalFileName))
            {
                this.ItemNode.Rename(originalFileName);
            }
        }

        internal override bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
        {
            if (deleteOperation == __VSDELETEITEMOPERATION.DELITEMOP_DeleteFromStorage)
            {
                return this.ProjectMgr.CanProjectDeleteItems;
            }
            return false;
        }

        /// <summary>
        /// This should be overriden for node that are not saved on disk
        /// </summary>
        /// <param name="oldName">Previous name in storage</param>
        /// <param name="newName">New name in storage</param>
        internal virtual void RenameInStorage(string oldName, string newName)
        {
            // Make a few attempts over a short time period
            for (var retries = 4; retries > 0; --retries)
            {
                try
                {
                    File.Move(oldName, newName);
                    return;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(50);
                }
            }

            // Final attempt has no handling so exception propagates
            File.Move(oldName, newName);
        }

        /// <summary>
        /// This method should be overridden to provide the list of special files and associated flags for source control.
        /// </summary>
        /// <param name="sccFile">One of the file associated to the node.</param>
        /// <param name="files">The list of files to be placed under source control.</param>
        /// <param name="flags">The flags that are associated to the files.</param>
        protected internal override void GetSccSpecialFiles(string sccFile, IList<string> files, IList<tagVsSccFilesFlags> flags)
        {
            if (this.ExcludeNodeFromScc)
            {
                return;
            }

            Utilities.ArgumentNotNull("files", files);
            Utilities.ArgumentNotNull("flags", flags);

            foreach (var node in this.GetChildNodes())
            {
                files.Add(node.GetMkDocument());
            }
        }

        #endregion

        #region Helper methods
        /// <summary>
        /// Gets called to rename the eventually running document this hierarchyitem points to
        /// </summary>
        /// returns FALSE if the doc can not be renamed
        internal bool RenameDocument(string oldName, string newName)
        {
            var pRDT = this.GetService(typeof(IVsRunningDocumentTable)) as IVsRunningDocumentTable;
            if (pRDT == null)
                return false;
            var docData = IntPtr.Zero;
            IVsHierarchy pIVsHierarchy;
            uint itemId;
            uint uiVsDocCookie;

            SuspendFileChanges sfc = null;

            if (File.Exists(oldName))
            {
                sfc = new SuspendFileChanges(this.ProjectMgr.Site, oldName);
                sfc.Suspend();
            }

            try
            {
                // Suspend ms build since during a rename operation no msbuild re-evaluation should be performed until we have finished.
                // Scenario that could fail if we do not suspend.
                // We have a project system relying on MPF that triggers a Compile target build (re-evaluates itself) whenever the project changes. (example: a file is added, property changed.)
                // 1. User renames a file in  the above project sytem relying on MPF
                // 2. Our rename funstionality implemented in this method removes and readds the file and as a post step copies all msbuild entries from the removed file to the added file.
                // 3. The project system mentioned will trigger an msbuild re-evaluate with the new item, because it was listening to OnItemAdded. 
                //    The problem is that the item at the "add" time is only partly added to the project, since the msbuild part has not yet been copied over as mentioned in part 2 of the last step of the rename process.
                //    The result is that the project re-evaluates itself wrongly.
                var renameflag = VSRENAMEFILEFLAGS.VSRENAMEFILEFLAGS_NoFlags;
                ErrorHandler.ThrowOnFailure(pRDT.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, oldName, out pIVsHierarchy, out itemId, out docData, out uiVsDocCookie));

                if (pIVsHierarchy != null && !Utilities.IsSameComObject(pIVsHierarchy, this.ProjectMgr))
                {
                    // Don't rename it if it wasn't opened by us.
                    return false;
                }

                // ask other potentially running packages
                if (!this.ProjectMgr.Tracker.CanRenameItem(oldName, newName, renameflag))
                {
                    return false;
                }

                if (IsFileOnDisk(oldName))
                {
                    RenameInStorage(oldName, newName);
                }

                // For some reason when ignoreFileChanges is called in Resume, we get an ArgumentException because
                // Somewhere a required fileWatcher is null.  This issue only occurs when you copy and rename a typescript file,
                // Calling Resume here prevents said fileWatcher from being null. Don't know why it works, but it does.
                // Also fun! This is the only location it can go (between RenameInStorage and RenameFileNode)
                // So presumably there is some condition that is no longer met once both of these methods are called with a ts file.
                // https://nodejstools.codeplex.com/workitem/1510
                if (sfc != null)
                {
                    sfc.Resume();
                    sfc.Suspend();
                }

                if (!CommonUtils.IsSamePath(oldName, newName))
                {
                    // Check out the project file if necessary.
                    if (!this.ProjectMgr.QueryEditProjectFile(false))
                    {
                        throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
                    }

                    this.RenameFileNode(oldName, newName);
                }
                else
                {
                    this.RenameCaseOnlyChange(oldName, newName);
                }

                DocumentManager.UpdateCaption(this.ProjectMgr.Site, this.Caption, docData);

                // changed from MPFProj:
                // http://mpfproj10.codeplex.com/WorkItem/View.aspx?WorkItemId=8231
                this.ProjectMgr.Tracker.OnItemRenamed(oldName, newName, renameflag);
            }
            finally
            {
                if (sfc != null)
                {
                    sfc.Resume();
                }
                if (docData != IntPtr.Zero)
                {
                    Marshal.Release(docData);
                }
            }

            return true;
        }

        internal virtual FileNode RenameFileNode(string oldFileName, string newFileName)
        {
            var newFolder = Path.GetDirectoryName(newFileName) + Path.DirectorySeparatorChar;
            var parentFolder = this.ProjectMgr.FindNodeByFullPath(newFolder);
            if (parentFolder == null)
            {
                Debug.Assert(newFolder == this.ProjectMgr.ProjectHome);
                parentFolder = this.ProjectMgr;
            }

            return this.RenameFileNode(oldFileName, newFileName, parentFolder);
        }

        /// <summary>
        /// Renames the file node for a case only change.
        /// </summary>
        /// <param name="newFileName">The new file name.</param>
        private void RenameCaseOnlyChange(string oldName, string newName)
        {
            //Update the include for this item.
            var relName = CommonUtils.GetRelativeFilePath(this.ProjectMgr.ProjectHome, newName);
            Debug.Assert(StringComparer.OrdinalIgnoreCase.Equals(this.ItemNode.GetMetadata(ProjectFileConstants.Include), relName), "Not just changing the filename case");

            this.ItemNode.Rename(relName);
            this.ItemNode.RefreshProperties();

            UpdateCaption();
            this.ProjectMgr.ReDrawNode(this, UIHierarchyElement.Caption);
            this.RenameChildNodes(this);

            // Refresh the property browser.
            var shell = this.ProjectMgr.Site.GetService(typeof(SVsUIShell)) as IVsUIShell;
            Utilities.CheckNotNull(shell, "Could not get the UI shell from the project");

            ErrorHandler.ThrowOnFailure(shell.RefreshPropertyBrowser(0));

            //Select the new node in the hierarchy
            ExpandItem(EXPANDFLAGS.EXPF_SelectItem);
        }

        #endregion

        #region helpers

        /// <summary>
        /// Update the ChildNodes after the parent node has been renamed
        /// </summary>
        /// <param name="newFileNode">The new FileNode created as part of the rename of this node</param>
        private void SetNewParentOnChildNodes(FileNode newFileNode)
        {
            foreach (var childNode in GetChildNodes())
            {
                childNode.Parent = newFileNode;
            }
        }

        private List<HierarchyNode> GetChildNodes()
        {
            var childNodes = new List<HierarchyNode>();
            var childNode = this.FirstChild;
            while (childNode != null)
            {
                childNodes.Add(childNode);
                childNode = childNode.NextSibling;
            }
            return childNodes;
        }
        #endregion

        void IDiskBasedNode.RenameForDeferredSave(string basePath, string baseNewPath)
        {
            var oldLoc = CommonUtils.GetAbsoluteFilePath(basePath, this.ItemNode.GetMetadata(ProjectFileConstants.Include));
            var newLoc = CommonUtils.GetAbsoluteFilePath(baseNewPath, this.ItemNode.GetMetadata(ProjectFileConstants.Include));

            this.ProjectMgr.UpdatePathForDeferredSave(oldLoc, newLoc);
            // make sure the directory is there
            Directory.CreateDirectory(Path.GetDirectoryName(newLoc));
            if (File.Exists(oldLoc))
            {
                File.Move(oldLoc, newLoc);
            }
        }
    }
}

