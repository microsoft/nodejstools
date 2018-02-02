// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudioTools.Project
{
    internal class ProjectDesignerDocumentManager : DocumentManager
    {
        #region ctors
        public ProjectDesignerDocumentManager(ProjectNode node)
            : base(node)
        {
        }
        #endregion

        #region overriden methods

        public override int Open(ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame windowFrame, WindowFrameShowAction windowFrameAction)
        {
            var editorGuid = VSConstants.GUID_ProjectDesignerEditor;
            return this.OpenWithSpecific(0, ref editorGuid, string.Empty, ref logicalView, docDataExisting, out windowFrame, windowFrameAction);
        }

        public override int OpenWithSpecific(uint editorFlags, ref Guid editorType, string physicalView, ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame frame, WindowFrameShowAction windowFrameAction)
        {
            frame = null;
            Debug.Assert(editorType == VSConstants.GUID_ProjectDesignerEditor, "Cannot open project designer with guid " + editorType.ToString());

            if (this.Node == null || this.Node.ProjectMgr == null || this.Node.ProjectMgr.IsClosed)
            {
                return VSConstants.E_FAIL;
            }

            var uiShellOpenDocument = this.Node.ProjectMgr.Site.GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;

            if (this.Node.ProjectMgr.Site.GetService(typeof(IOleServiceProvider)) is IOleServiceProvider serviceProvider && uiShellOpenDocument != null)
            {
                var fullPath = this.GetFullPathForDocument();
                var caption = this.GetOwnerCaption();

                var parentHierarchy = this.Node.ProjectMgr.GetProperty((int)__VSHPROPID.VSHPROPID_ParentHierarchy) as IVsUIHierarchy;

                var parentHierarchyItemId = (IntPtr)this.Node.ProjectMgr.GetProperty((int)__VSHPROPID.VSHPROPID_ParentHierarchyItemid);

                ErrorHandler.ThrowOnFailure(uiShellOpenDocument.OpenSpecificEditor(editorFlags, fullPath, ref editorType, physicalView, ref logicalView, caption, parentHierarchy, (uint)(parentHierarchyItemId.ToInt32()), docDataExisting, serviceProvider, out frame));

                if (frame != null)
                {
                    if (windowFrameAction == WindowFrameShowAction.Show)
                    {
                        ErrorHandler.ThrowOnFailure(frame.Show());
                    }
                }
            }

            return VSConstants.S_OK;
        }
        #endregion

    }
}
