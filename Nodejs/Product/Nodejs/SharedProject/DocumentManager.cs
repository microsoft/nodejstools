// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// This abstract class handles opening, saving of items in the hierarchy.
    /// </summary>

    internal abstract class DocumentManager
    {
        #region fields
        private readonly HierarchyNode node = null;
        #endregion

        #region properties
        protected HierarchyNode Node => this.node;
        #endregion

        #region ctors
        protected DocumentManager(HierarchyNode node)
        {
            Utilities.ArgumentNotNull(nameof(node), node);
            this.node = node;
        }
        #endregion

        #region virtual methods

        /// <summary>
        /// Open a document using the standard editor. This method has no implementation since a document is abstract in this context
        /// </summary>
        /// <param name="logicalView">In MultiView case determines view to be activated by IVsMultiViewDocumentView. For a list of logical view GUIDS, see constants starting with LOGVIEWID_ defined in NativeMethods class</param>
        /// <param name="docDataExisting">IntPtr to the IUnknown interface of the existing document data object</param>
        /// <param name="windowFrame">A reference to the window frame that is mapped to the document</param>
        /// <param name="windowFrameAction">Determine the UI action on the document window</param>
        /// <returns>NotImplementedException</returns>
        /// <remarks>See FileDocumentManager class for an implementation of this method</remarks>
        public virtual int Open(ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame windowFrame, WindowFrameShowAction windowFrameAction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Open a document using a specific editor. This method has no implementation.
        /// </summary>
        /// <param name="editorFlags">Specifies actions to take when opening a specific editor. Possible editor flags are defined in the enumeration Microsoft.VisualStudio.Shell.Interop.__VSOSPEFLAGS</param>
        /// <param name="editorType">Unique identifier of the editor type</param>
        /// <param name="physicalView">Name of the physical view. If null, the environment calls MapLogicalView on the editor factory to determine the physical view that corresponds to the logical view. In this case, null does not specify the primary view, but rather indicates that you do not know which view corresponds to the logical view</param>
        /// <param name="logicalView">In MultiView case determines view to be activated by IVsMultiViewDocumentView. For a list of logical view GUIDS, see constants starting with LOGVIEWID_ defined in NativeMethods class</param>
        /// <param name="docDataExisting">IntPtr to the IUnknown interface of the existing document data object</param>
        /// <param name="frame">A reference to the window frame that is mapped to the document</param>
        /// <param name="windowFrameAction">Determine the UI action on the document window</param>
        /// <returns>NotImplementedException</returns>
        /// <remarks>See FileDocumentManager for an implementation of this method</remarks>
        public virtual int OpenWithSpecific(uint editorFlags, ref Guid editorType, string physicalView, ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame frame, WindowFrameShowAction windowFrameAction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Open a document using a specific editor. This method has no implementation.
        /// </summary>
        /// <param name="editorFlags">Specifies actions to take when opening a specific editor. Possible editor flags are defined in the enumeration Microsoft.VisualStudio.Shell.Interop.__VSOSPEFLAGS</param>
        /// <param name="editorType">Unique identifier of the editor type</param>
        /// <param name="physicalView">Name of the physical view. If null, the environment calls MapLogicalView on the editor factory to determine the physical view that corresponds to the logical view. In this case, null does not specify the primary view, but rather indicates that you do not know which view corresponds to the logical view</param>
        /// <param name="logicalView">In MultiView case determines view to be activated by IVsMultiViewDocumentView. For a list of logical view GUIDS, see constants starting with LOGVIEWID_ defined in NativeMethods class</param>
        /// <param name="docDataExisting">IntPtr to the IUnknown interface of the existing document data object</param>
        /// <param name="frame">A reference to the window frame that is mapped to the document</param>
        /// <param name="windowFrameAction">Determine the UI action on the document window</param>
        /// <returns>NotImplementedException</returns>
        /// <remarks>See FileDocumentManager for an implementation of this method</remarks>
        public virtual int ReOpenWithSpecific(uint editorFlags, ref Guid editorType, string physicalView, ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame frame, WindowFrameShowAction windowFrameAction)
        {
            return OpenWithSpecific(editorFlags, ref editorType, physicalView, ref logicalView, docDataExisting, out frame, windowFrameAction);
        }

        /// <summary>
        /// Close an open document window
        /// </summary>
        /// <param name="closeFlag">Decides how to close the document</param>
        /// <returns>S_OK if successful, otherwise an error is returned</returns>
        public virtual int Close(__FRAMECLOSE closeFlag)
        {
            if (this.node == null || this.node.ProjectMgr == null || this.node.ProjectMgr.IsClosed || this.node.ProjectMgr.IsClosing)
            {
                return VSConstants.E_FAIL;
            }

            if (this.IsOpenedByUs)
            {
                var shell = this.Node.ProjectMgr.Site.GetService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
                var logicalView = Guid.Empty;
                uint grfIDO = 0;
                var itemIdOpen = new uint[1];
                ErrorHandler.ThrowOnFailure(shell.IsDocumentOpen(this.Node.ProjectMgr, this.Node.ID, this.Node.Url, ref logicalView, grfIDO, out var pHierOpen, itemIdOpen, out var windowFrame, out var fOpen));

                if (windowFrame != null)
                {
                    return windowFrame.CloseFrame((uint)closeFlag);
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Silently saves an open document
        /// </summary>
        /// <param name="saveIfDirty">Save the open document only if it is dirty</param>
        /// <remarks>The call to SaveDocData may return Microsoft.VisualStudio.Shell.Interop.PFF_RESULTS.STG_S_DATALOSS to indicate some characters could not be represented in the current codepage</remarks>
        public virtual void Save(bool saveIfDirty)
        {
            if (saveIfDirty && this.IsDirty)
            {
                var persistDocData = this.DocData;
                if (persistDocData != null)
                {
                    ErrorHandler.ThrowOnFailure(persistDocData.SaveDocData(VSSAVEFLAGS.VSSAVE_SilentSave, out var name, out var cancelled));
                }
            }
        }

        #endregion

        /// <summary>
        /// Queries the RDT to see if the document is currently edited and not saved.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                var docTable = (IVsRunningDocumentTable4)this.node.ProjectMgr.GetService(typeof(SVsRunningDocumentTable));
                if (!docTable.IsMonikerValid(this.node.GetMkDocument()))
                {
                    return false;
                }

                return docTable.IsDocumentDirty(docTable.GetDocumentCookie(this.node.GetMkDocument()));
            }
        }

        /// <summary>
        /// Queries the RDT to see if the document was opened by our project.
        /// </summary>
        public bool IsOpenedByUs
        {
            get
            {
                var docTable = (IVsRunningDocumentTable4)this.node.ProjectMgr.GetService(typeof(SVsRunningDocumentTable));
                if (!docTable.IsMonikerValid(this.node.GetMkDocument()))
                {
                    return false;
                }
                docTable.GetDocumentHierarchyItem(
                    docTable.GetDocumentCookie(this.node.GetMkDocument()),
                    out var hierarchy,
                    out var itemId
                );
                return Utilities.IsSameComObject(this.node.ProjectMgr, hierarchy);
            }
        }

        /// <summary>
        /// Returns the doc cookie in the RDT for the associated file.
        /// </summary>
        public uint DocCookie
        {
            get
            {
                var docTable = (IVsRunningDocumentTable4)this.node.ProjectMgr.GetService(typeof(SVsRunningDocumentTable));
                if (!docTable.IsMonikerValid(this.node.GetMkDocument()))
                {
                    return (uint)ShellConstants.VSDOCCOOKIE_NIL;
                }

                return docTable.GetDocumentCookie(this.node.GetMkDocument());
            }
        }

        /// <summary>
        /// Returns the IVsPersistDocData associated with the document, or null if there isn't one.
        /// </summary>
        public IVsPersistDocData DocData
        {
            get
            {
                var docTable = (IVsRunningDocumentTable4)this.node.ProjectMgr.GetService(typeof(SVsRunningDocumentTable));
                if (!docTable.IsMonikerValid(this.node.GetMkDocument()))
                {
                    return null;
                }

                return docTable.GetDocumentData(docTable.GetDocumentCookie(this.node.GetMkDocument())) as IVsPersistDocData;
            }
        }

        #region helper methods

        protected string GetOwnerCaption()
        {
            Debug.Assert(this.node != null, "No node has been initialized for the document manager");

            ErrorHandler.ThrowOnFailure(this.node.ProjectMgr.GetProperty(this.node.ID, (int)__VSHPROPID.VSHPROPID_Caption, out var pvar));

            return (pvar as string);
        }

        protected static void CloseWindowFrame(ref IVsWindowFrame windowFrame)
        {
            if (windowFrame != null)
            {
                try
                {
                    ErrorHandler.ThrowOnFailure(windowFrame.CloseFrame(0));
                }
                finally
                {
                    windowFrame = null;
                }
            }
        }

        protected string GetFullPathForDocument()
        {
            var fullPath = string.Empty;

            // Get the URL representing the item
            fullPath = this.node.GetMkDocument();

            Debug.Assert(!string.IsNullOrEmpty(fullPath), "Could not retrive the fullpath for the node" + this.Node.ID.ToString(CultureInfo.CurrentCulture));
            return fullPath;
        }

        #endregion

        #region static methods
        /// <summary>
        /// Updates the caption for all windows associated to the document.
        /// </summary>
        /// <param name="site">The service provider.</param>
        /// <param name="caption">The new caption.</param>
        /// <param name="docData">The IUnknown interface to a document data object associated with a registered document.</param>
        public static void UpdateCaption(IServiceProvider site, string caption, IntPtr docData)
        {
            Utilities.ArgumentNotNull(nameof(site), site);

            if (string.IsNullOrEmpty(caption))
            {
                throw new ArgumentException(SR.GetString(SR.ParameterCannotBeNullOrEmpty), nameof(caption));
            }

            var uiShell = site.GetService(typeof(SVsUIShell)) as IVsUIShell;

            // We need to tell the windows to update their captions. 
            ErrorHandler.ThrowOnFailure(uiShell.GetDocumentWindowEnum(out var windowFramesEnum));
            var windowFrames = new IVsWindowFrame[1];
            while (windowFramesEnum.Next(1, windowFrames, out var fetched) == VSConstants.S_OK && fetched == 1)
            {
                var windowFrame = windowFrames[0];
                ErrorHandler.ThrowOnFailure(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out var data));
                var ptr = Marshal.GetIUnknownForObject(data);
                try
                {
                    if (ptr == docData)
                    {
                        ErrorHandler.ThrowOnFailure(windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_OwnerCaption, caption));
                    }
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.Release(ptr);
                    }
                }
            }
        }

        /// <summary>
        /// Rename document in the running document table from oldName to newName.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        /// <param name="oldName">Full path to the old name of the document.</param>
        /// <param name="newName">Full path to the new name of the document.</param>
        /// <param name="newItemId">The new item id of the document</param>
        public static void RenameDocument(IServiceProvider site, string oldName, string newName, uint newItemId)
        {
            Utilities.ArgumentNotNull(nameof(site), site);

            if (string.IsNullOrEmpty(oldName))
            {
                throw new ArgumentException(SR.GetString(SR.ParameterCannotBeNullOrEmpty), nameof(oldName));
            }

            if (string.IsNullOrEmpty(newName))
            {
                throw new ArgumentException(SR.GetString(SR.ParameterCannotBeNullOrEmpty), nameof(newName));
            }

            if (newItemId == VSConstants.VSITEMID_NIL)
            {
                throw new ArgumentNullException(nameof(newItemId));
            }

            var pRDT = site.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;

            if (pRDT == null)
            {
                return;
            }

            ErrorHandler.ThrowOnFailure(pRDT.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, oldName, out var pIVsHierarchy, out var itemId, out var docData, out var uiVsDocCookie));

            if (docData != IntPtr.Zero && pIVsHierarchy != null)
            {
                try
                {
                    var pUnk = Marshal.GetIUnknownForObject(pIVsHierarchy);
                    var iid = typeof(IVsHierarchy).GUID;
                    Marshal.QueryInterface(pUnk, ref iid, out var pHier);
                    try
                    {
                        ErrorHandler.ThrowOnFailure(pRDT.RenameDocument(oldName, newName, pHier, newItemId));
                    }
                    finally
                    {
                        if (pHier != IntPtr.Zero)
                        {
                            Marshal.Release(pHier);
                        }

                        if (pUnk != IntPtr.Zero)
                        {
                            Marshal.Release(pUnk);
                        }
                    }
                }
                finally
                {
                    Marshal.Release(docData);
                }
            }
        }
        #endregion
    }
}
