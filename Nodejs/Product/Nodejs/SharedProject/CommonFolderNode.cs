// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.VisualStudioTools.Project
{
    internal class CommonFolderNode : FolderNode
    {
        private CommonProjectNode _project;

        public CommonFolderNode(CommonProjectNode root, ProjectElement element)
            : base(root, element)
        {
            this._project = root;
        }

        public override bool IsNonMemberItem => this.ItemNode is AllFilesProjectElement;

        protected override ImageMoniker GetIconMoniker(bool open)
        {
            if (this.ItemNode.IsExcluded)
            {
                return open ? KnownMonikers.HiddenFolderOpened : KnownMonikers.HiddenFolderClosed;
            }
            return base.GetIconMoniker(open);
        }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            //Hide Exclude from Project command, show everything else normal Folder node supports
            if (cmdGroup == Microsoft.VisualStudioTools.Project.VsMenus.guidStandardCommandSet2K)
            {
                switch ((VsCommands2K)cmd)
                {
                    case VsCommands2K.EXCLUDEFROMPROJECT:
                        if (this.ItemNode.IsExcluded)
                        {
                            result |= QueryStatusResult.NOTSUPPORTED | QueryStatusResult.INVISIBLE;
                            return VSConstants.S_OK;
                        }
                        break;
                    case VsCommands2K.INCLUDEINPROJECT:
                        if (this.ItemNode.IsExcluded)
                        {
                            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                            return VSConstants.S_OK;
                        }
                        break;
                    case CommonConstants.OpenFolderInExplorerCmdId:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }
            else if (cmdGroup == this.ProjectMgr.SharedCommandGuid)
            {
                switch ((SharedCommands)cmd)
                {
                    case SharedCommands.AddExistingFolder:
                        if (!this.ItemNode.IsExcluded)
                        {
                            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                            return VSConstants.S_OK;
                        }
                        break;
                }
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (cmdGroup == Microsoft.VisualStudioTools.Project.VsMenus.guidStandardCommandSet2K)
            {
                if ((VsCommands2K)cmd == CommonConstants.OpenFolderInExplorerCmdId)
                {
                    Process.Start(this.Url);
                    return VSConstants.S_OK;
                }
            }
            else if (cmdGroup == this.ProjectMgr.SharedCommandGuid)
            {
                switch ((SharedCommands)cmd)
                {
                    case SharedCommands.AddExistingFolder:
                        return this.ProjectMgr.AddExistingFolderToNode(this);
                    case SharedCommands.OpenCommandPromptHere:
                        var psi = new ProcessStartInfo(
                            Path.Combine(
                                Environment.SystemDirectory,
                                "cmd.exe"
                            )
                        );
                        psi.WorkingDirectory = this.FullPathToChildren;
                        Process.Start(psi);
                        return VSConstants.S_OK;
                }
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        /// <summary>
        /// Handles the exclude from project command.
        /// </summary>
        /// <returns></returns>
        internal override int ExcludeFromProject()
        {
            this.ProjectMgr.Site.GetUIThread().MustBeCalledFromUIThread();

            Debug.Assert(this.ProjectMgr != null, "The project item " + this.ToString() + " has not been initialised correctly. It has a null ProjectMgr");
            if (!this.ProjectMgr.QueryEditProjectFile(false) ||
                !this.ProjectMgr.QueryFolderRemove(this.Parent, this.Url))
            {
                return VSConstants.E_FAIL;
            }

            for (var child = this.FirstChild; child != null; child = child.NextSibling)
            {
                // we automatically exclude all children below us too
                var hr = child.ExcludeFromProject();
                if (ErrorHandler.Failed(hr))
                {
                    return hr;
                }
            }

            ResetNodeProperties();
            this.ItemNode.RemoveFromProjectFile();
            if (!Directory.Exists(CommonUtils.TrimEndSeparator(this.Url)))
            {
                this.ProjectMgr.OnItemDeleted(this);
                this.Parent.RemoveChild(this);
            }
            else
            {
                this.ItemNode = new AllFilesProjectElement(this.Url, this.ItemNode.ItemTypeName, this.ProjectMgr);
                if (!this.ProjectMgr.IsShowingAllFiles)
                {
                    this.IsVisible = false;
                    this.ProjectMgr.OnInvalidateItems(this.Parent);
                }
                this.ProjectMgr.ReDrawNode(this, UIHierarchyElement.Icon);
                this.ProjectMgr.OnPropertyChanged(this, (int)__VSHPROPID.VSHPROPID_IsNonMemberItem, 0);
            }
            ((IVsUIShell)GetService(typeof(SVsUIShell))).RefreshPropertyBrowser(0);

            return VSConstants.S_OK;
        }

        internal override int ExcludeFromProjectWithProgress()
        {
            using (new WaitDialog(
                "Excluding files and folders...",
                "Excluding files and folders in your project, this may take several seconds...",
                this.ProjectMgr.Site))
            {
                return base.ExcludeFromProjectWithProgress();
            }
        }

        internal override int IncludeInProject(bool includeChildren)
        {
            if (this.Parent.ItemNode != null && this.Parent.ItemNode.IsExcluded)
            {
                // if our parent is excluded it needs to first be included
                var hr = this.Parent.IncludeInProject(false);
                if (ErrorHandler.Failed(hr))
                {
                    return hr;
                }
            }

            if (!this.ProjectMgr.QueryEditProjectFile(false) ||
                !this.ProjectMgr.QueryFolderAdd(this.Parent, this.Url))
            {
                return VSConstants.E_FAIL;
            }

            ResetNodeProperties();
            this.ItemNode = this.ProjectMgr.CreateMsBuildFileItem(
                CommonUtils.GetRelativeDirectoryPath(this.ProjectMgr.ProjectHome, this.Url),
                ProjectFileConstants.Folder
            );
            this.IsVisible = true;

            if (includeChildren)
            {
                for (var child = this.FirstChild; child != null; child = child.NextSibling)
                {
                    // we automatically include all children below us too
                    var hr = child.IncludeInProject(includeChildren);
                    if (ErrorHandler.Failed(hr))
                    {
                        return hr;
                    }
                }
            }
            this.ProjectMgr.ReDrawNode(this, UIHierarchyElement.Icon);
            this.ProjectMgr.OnPropertyChanged(this, (int)__VSHPROPID.VSHPROPID_IsNonMemberItem, 0);
            ((IVsUIShell)GetService(typeof(SVsUIShell))).RefreshPropertyBrowser(0);

            // On include, the folder should be added to source control.
            this.ProjectMgr.Tracker.OnFolderAdded(this.Url, VSADDDIRECTORYFLAGS.VSADDDIRECTORYFLAGS_NoFlags);

            return VSConstants.S_OK;
        }

        internal override int IncludeInProjectWithProgress(bool includeChildren)
        {
            using (new WaitDialog(
                "Including files and folders...",
                "Including files and folders to your project, this may take several seconds...",
                this.ProjectMgr.Site))
            {
                return IncludeInProject(includeChildren);
            }
        }

        public override void RenameFolder(string newName)
        {
            var oldName = this.Url;
            this._project.SuppressFileChangeNotifications();
            try
            {
                base.RenameFolder(newName);
            }
            finally
            {
                this._project.RestoreFileChangeNotifications();
            }

            if (this.ProjectMgr.TryDeactivateSymLinkWatcher(this))
            {
                this.ProjectMgr.CreateSymLinkWatcher(this.Url);
            }
        }

        public override void Remove(bool removeFromStorage)
        {
            // if we were a symlink folder, we need to stop watching now.
            this.ProjectMgr.TryDeactivateSymLinkWatcher(this);

            base.Remove(removeFromStorage);
        }

        public override void Close()
        {
            // make sure this thing isn't hanging around...
            this.ProjectMgr.TryDeactivateSymLinkWatcher(this);

            base.Close();
        }

        /// <summary>
        /// Common Folder Node can only be deleted from file system.
        /// </summary>        
        internal override bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
        {
            return deleteOperation == __VSDELETEITEMOPERATION.DELITEMOP_DeleteFromStorage;
        }

        public new CommonProjectNode ProjectMgr => (CommonProjectNode)base.ProjectMgr;
    }
}
