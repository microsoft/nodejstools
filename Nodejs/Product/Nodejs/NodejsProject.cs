// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Azure;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools
{
    [Guid("78D985FC-2CA0-4D08-9B6B-35ACD5E5294A")]
    internal class NodejsProject : FlavoredProjectBase, IOleCommandTarget, IVsProjectFlavorCfgProvider, IVsProject, IVsProject2, IAzureRoleProject
    {
        internal IVsProject _innerProject;
        internal IVsProject3 _innerProject3;
        internal NodejsPackage _package;
        private OleMenuCommandService _menuService;
        private List<OleMenuCommand> _commands = new List<OleMenuCommand>();
        private IVsProjectFlavorCfgProvider _innerVsProjectFlavorCfgProvider;

        protected override void Close()
        {
            // Disregard this branch. There's no change, this is only used for testing the Pull Request Validation. More changes.
            if (this._menuService != null)
            {
                foreach (var command in this._commands)
                {
                    this._menuService.RemoveCommand(command);
                }
                this._menuService.Dispose();
            }
            this._commands.Clear();
            base.Close();
        }

        protected override void InitializeForOuter(string fileName, string location, string name, uint flags, ref Guid guidProject, out bool cancel)
        {
            var menuCommandID = new CommandID(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Open);
            var menuItem = new OleMenuCommand(this.OpenFile, null, this.OpenFileBeforeQueryStatus, menuCommandID);
            AddCommand(menuItem);

            menuCommandID = new CommandID(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.ViewCode);
            menuItem = new OleMenuCommand(this.OpenFile, null, this.OpenFileBeforeQueryStatus, menuCommandID);
            AddCommand(menuItem);

            menuCommandID = new CommandID(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.ECMD_VIEWMARKUP);
            menuItem = new OleMenuCommand(this.OpenFile, null, this.OpenFileBeforeQueryStatus, menuCommandID);
            AddCommand(menuItem);

            base.InitializeForOuter(fileName, location, name, flags, ref guidProject, out cancel);

            ErrorHandler.ThrowOnFailure(
                this._innerVsHierarchy.GetProperty(
                    VSConstants.VSITEMID_ROOT,
                    (int)__VSHPROPID.VSHPROPID_ExtObject,
                    out var extObject
                )
            );

            if (extObject is EnvDTE.Project proj)
            {
                try
                {
                    var webAppExtender = proj.get_Extender("WebApplication");
                    if (webAppExtender != null && webAppExtender is WebAppExtenderFilter)
                    {
                        ((dynamic)((WebAppExtenderFilter)webAppExtender).InnerObject).StartWebServerOnDebug = false;
                    }
                }
                catch (COMException)
                {
                    // extender doesn't exist...
                }
            }
        }

        private void AddCommand(OleMenuCommand menuItem)
        {
            this._menuService.AddCommand(menuItem);
            this._commands.Add(menuItem);
        }

        private void OpenFile(object sender, EventArgs e)
        {
            var oleMenu = sender as OleMenuCommand;
            oleMenu.Supported = false;

            foreach (var vsItemSelection in GetSelectedItems())
            {
                if (IsJavaScriptFile(Name(vsItemSelection)))
                {
                    ErrorHandler.ThrowOnFailure(OpenWithNodejsEditor(vsItemSelection.itemid));
                }
                else
                {
                    ErrorHandler.ThrowOnFailure(OpenWithDefaultEditor(vsItemSelection.itemid));
                }
            }
        }

        private void OpenFileBeforeQueryStatus(object sender, EventArgs e)
        {
            var oleMenu = sender as OleMenuCommand;
            oleMenu.Supported = false;

            foreach (var vsItemSelection in GetSelectedItems())
            {
                ErrorHandler.ThrowOnFailure(vsItemSelection.pHier.GetProperty(vsItemSelection.itemid, (int)__VSHPROPID.VSHPROPID_Name, out var name));

                if (IsJavaScriptFile(Name(vsItemSelection)))
                {
                    oleMenu.Supported = true;
                }
            }
        }

        internal static string Name(VSITEMSELECTION item)
        {
            return GetItemName(item.pHier, item.itemid);
        }

        internal static string GetItemName(IVsHierarchy hier, uint itemid)
        {
            ErrorHandler.ThrowOnFailure(hier.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out var name));
            return (string)name;
        }

        private int OpenWithDefaultEditor(uint selectionItemId)
        {
            var view = Guid.Empty;
            var hr = ((IVsProject)this._innerVsHierarchy).OpenItem(
                selectionItemId,
                ref view,
                IntPtr.Zero,
                out var frame
            );
            if (ErrorHandler.Succeeded(hr))
            {
                hr = frame.Show();
            }
            return hr;
        }

        protected override void SetInnerProject(IntPtr innerIUnknown)
        {
            var inner = Marshal.GetObjectForIUnknown(innerIUnknown);

            // The reason why we keep a reference to those is that doing a QI after being
            // aggregated would do the AddRef on the outer object.
            this._innerProject = inner as IVsProject;
            this._innerProject3 = inner as IVsProject3;
            this._innerVsHierarchy = inner as IVsHierarchy;
            this._innerVsProjectFlavorCfgProvider = inner as IVsProjectFlavorCfgProvider;

            // Ensure we have a service provider as this is required for menu items to work
            if (this.serviceProvider == null)
            {
                this.serviceProvider = (System.IServiceProvider)this._package;
            }

            // Now let the base implementation set the inner object
            base.SetInnerProject(innerIUnknown);

            // Add our commands (this must run after we called base.SetInnerProject)            
            this._menuService = ((System.IServiceProvider)this).GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
        }

        private bool TryHandleRightClick(IntPtr pvaIn, out int res)
        {
            var itemType = GetSelectedItemType();

            if (TryShowContextMenu(pvaIn, itemType, out res))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all of the currently selected items.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<VSITEMSELECTION> GetSelectedItems()
        {
            var monitorSelection = this._package.GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

            var hierarchyPtr = IntPtr.Zero;
            var selectionContainer = IntPtr.Zero;
            try
            {
                ErrorHandler.ThrowOnFailure(monitorSelection.GetCurrentSelection(out hierarchyPtr, out var selectionItemId, out var multiItemSelect, out selectionContainer));

                if (selectionItemId != VSConstants.VSITEMID_NIL && hierarchyPtr != IntPtr.Zero)
                {
                    var hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;

                    if (selectionItemId != VSConstants.VSITEMID_SELECTION)
                    {
                        // This is a single selection. Compare hirarchy with our hierarchy and get node from itemid
                        if (Utilities.IsSameComObject(this, hierarchy))
                        {
                            yield return new VSITEMSELECTION() { itemid = selectionItemId, pHier = hierarchy };
                        }
                    }
                    else if (multiItemSelect != null)
                    {
                        ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectionInfo(out var numberOfSelectedItems, out var isSingleHierarchyInt));
                        var isSingleHierarchy = (isSingleHierarchyInt != 0);

                        // Now loop all selected items and add to the list only those that are selected within this hierarchy
                        if (!isSingleHierarchy || (isSingleHierarchy && Utilities.IsSameComObject(this, hierarchy)))
                        {
                            Debug.Assert(numberOfSelectedItems > 0, "Bad number of selected itemd");
                            var vsItemSelections = new VSITEMSELECTION[numberOfSelectedItems];
                            var flags = (isSingleHierarchy) ? (uint)__VSGSIFLAGS.GSI_fOmitHierPtrs : 0;
                            ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectedItems(flags, numberOfSelectedItems, vsItemSelections));

                            foreach (var vsItemSelection in vsItemSelections)
                            {
                                yield return new VSITEMSELECTION() { itemid = vsItemSelection.itemid, pHier = hierarchy };
                            }
                        }
                    }
                }
            }
            finally
            {
                if (hierarchyPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPtr);
                }
                if (selectionContainer != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainer);
                }
            }
        }

        private Guid GetSelectedItemType()
        {
            var itemType = Guid.Empty;
            foreach (var vsItemSelection in GetSelectedItems())
            {
                var typeGuid = GetItemType(vsItemSelection);

                if (itemType == Guid.Empty)
                {
                    itemType = typeGuid;
                }
                else if (itemType != typeGuid)
                {
                    // we have multiple item types
                    itemType = Guid.Empty;
                    break;
                }
            }
            return itemType;
        }

        private bool TryShowContextMenu(IntPtr pvaIn, Guid itemType, out int res)
        {
            if (itemType == new Guid(Guids.NodejsProjectFactoryString))
            {
                // multiple Node prjoect nodes selected
                res = ShowContextMenu(pvaIn, VsMenus.IDM_VS_CTXT_PROJNODE/*IDM_VS_CTXT_WEBPROJECT*/);
                return true;
            }
            else if (itemType == VSConstants.GUID_ItemType_PhysicalFile)
            {
                // multiple files selected
                res = ShowContextMenu(pvaIn, VsMenus.IDM_VS_CTXT_ITEMNODE);
                return true;
            }
            else if (itemType == VSConstants.GUID_ItemType_PhysicalFolder)
            {
                res = ShowContextMenu(pvaIn, VsMenus.IDM_VS_CTXT_FOLDERNODE);
                return true;
            }
            res = VSConstants.E_FAIL;
            return false;
        }

        private int ShowContextMenu(IntPtr pvaIn, int ctxMenu)
        {
            var variant = Marshal.GetObjectForNativeVariant(pvaIn);
            var pointsAsUint = (uint)variant;
            var x = (short)(pointsAsUint & 0x0000ffff);
            var y = (short)((pointsAsUint & 0xffff0000) / 0x10000);

            var points = new POINTS
            {
                x = x,
                y = y
            };

            return ShowContextMenu(ctxMenu, VsMenus.guidSHLMainMenu, points);
        }

        /// <summary>
        /// Shows the specified context menu at a specified location.
        /// </summary>
        /// <param name="menuId">The context menu ID.</param>
        /// <param name="groupGuid">The GUID of the menu group.</param>
        /// <param name="points">The location at which to show the menu.</param>
        internal int ShowContextMenu(int menuId, Guid menuGroup, POINTS points)
        {
            var shell = this._package.GetService(typeof(SVsUIShell)) as IVsUIShell;

            Debug.Assert(shell != null, "Could not get the ui shell from the project");
            if (shell == null)
            {
                return VSConstants.E_FAIL;
            }
            var pnts = new POINTS[1];
            pnts[0].x = points.x;
            pnts[0].y = points.y;
            return shell.ShowContextMenu(0, ref menuGroup, menuId, pnts, (Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget)this);
        }

        protected override int ExecCommand(uint itemid, ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VsMenus.guidVsUIHierarchyWindowCmds)
            {
                switch ((VSConstants.VsUIHierarchyWindowCmdIds)nCmdID)
                {
                    case VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_RightClick:
                        int res;
                        if (TryHandleRightClick(pvaIn, out res))
                        {
                            return res;
                        }
                        break;
                    case VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_DoubleClick:
                    case VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_EnterKey:
                        // open the document if it's an JavaScript file
                        if (IsJavaScriptFile(this._innerVsHierarchy, itemid))
                        {
                            var hr = OpenWithNodejsEditor(itemid);

                            if (ErrorHandler.Succeeded(hr))
                            {
                                return hr;
                            }
                        }
                        break;
                }
            }

            var result = base.ExecCommand(itemid, ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            return result;
        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            return ((IOleCommandTarget)this._menuService).Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private bool IsJavaScriptFile(IVsHierarchy iVsHierarchy, uint itemid)
        {
            ErrorHandler.ThrowOnFailure(iVsHierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out var name));

            return IsJavaScriptFile(name);
        }

        private static bool IsJavaScriptFile(object name)
        {
            if (name is string strName)
            {
                var ext = Path.GetExtension(strName);
                if (StringComparer.OrdinalIgnoreCase.Equals(ext, ".js"))
                {
                    return true;
                }
            }
            return false;
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == Guids.Eureka)
            {
                for (var i = 0; i < prgCmds.Length; i++)
                {
                    switch (prgCmds[i].cmdID)
                    {
                        case 0x102: // View in Web Page Inspector from Eureka web tools
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            return VSConstants.S_OK;
                    }
                }
            }
            else if (pguidCmdGroup == Guids.VenusCommandId)
            {
                for (var i = 0; i < prgCmds.Length; i++)
                {
                    switch (prgCmds[i].cmdID)
                    {
                        case 0x034: /* add app assembly folder */
                        case 0x035: /* add app code folder */
                        case 0x036: /* add global resources */
                        case 0x037: /* add local resources */
                        case 0x038: /* add web refs folder */
                        case 0x039: /* add data folder */
                        case 0x040: /* add browser folders */
                        case 0x041: /* theme */
                        case 0x054: /* package settings */
                        case 0x055: /* context package settings */

                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            return VSConstants.S_OK;
                    }
                }
            }
            else if (pguidCmdGroup == Guids.WebPackageCommandId)
            {
                if (prgCmds[0].cmdID == 0x101 /*  EnablePublishToWindowsAzureMenuItem*/)
                {
                }
            }
            else if (pguidCmdGroup == Guids.WebAppCmdId)
            {
                for (var i = 0; i < prgCmds.Length; i++)
                {
                    switch (prgCmds[i].cmdID)
                    {
                        case 0x06A: /* check accessibility */
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_DEFHIDEONCTXTMENU | OLECMDF.OLECMDF_ENABLED);
                            return VSConstants.S_OK;
                    }
                }
            }
            else if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                for (var i = 0; i < prgCmds.Length; i++)
                {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd2KCmdID.SETASSTARTPAGE:
                        case VSConstants.VSStd2KCmdID.CHECK_ACCESSIBILITY:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_DEFHIDEONCTXTMENU | OLECMDF.OLECMDF_ENABLED);
                            return VSConstants.S_OK;
                    }
                }
            }
            else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                for (var i = 0; i < prgCmds.Length; i++)
                {
                    switch ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd97CmdID.PreviewInBrowser:
                        case VSConstants.VSStd97CmdID.BrowseWith:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_DEFHIDEONCTXTMENU | OLECMDF.OLECMDF_ENABLED);
                            return VSConstants.S_OK;
                    }
                }
            }

            return ((IOleCommandTarget)this._menuService).QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #region IVsProjectFlavorCfgProvider Members

        public int CreateProjectFlavorCfg(IVsCfg pBaseProjectCfg, out IVsProjectFlavorCfg ppFlavorCfg)
        {
            // We're flavored with a Web Application project and our normal project...  But we don't
            // want the web application project to influence our config as that alters our debug
            // launch story.  We control that w/ the Django project which is actually just letting the
            // base Node.js project handle it.  So we keep the base Node.js project config here.
            ErrorHandler.ThrowOnFailure(
                this._innerVsProjectFlavorCfgProvider.CreateProjectFlavorCfg(
                    pBaseProjectCfg,
                    out var webCfg
                )
            );
            ppFlavorCfg = new NodejsProjectConfig(pBaseProjectCfg, webCfg);

            return VSConstants.S_OK;
        }

        #endregion

        protected override int GetProperty(uint itemId, int propId, out object property)
        {
            switch ((__VSHPROPID)propId)
            {
                case __VSHPROPID.VSHPROPID_IconIndex:
                case __VSHPROPID.VSHPROPID_OpenFolderIconIndex:
                    // Venus wants to change the icon for special folders using the IconIndex.  All of our
                    // folders respond to IconHandles so we just force folders down that code path rather
                    // than trying to hand out the correct IconIndex here
                    if (GetItemType(new VSITEMSELECTION() { itemid = itemId, pHier = this }) == VSConstants.GUID_ItemType_PhysicalFolder)
                    {
                        property = null;
                        return VSConstants.DISP_E_MEMBERNOTFOUND;
                    }
                    break;
            }
            switch ((__VSHPROPID4)propId)
            {
                case __VSHPROPID4.VSHPROPID_TargetFrameworkMoniker:
                    // really only here for testing so WAP projects load correctly...
                    // But this also impacts the toolbox by filtering what available items there are.
                    property = ".NETFramework,Version=v4.5,Profile=Client";
                    return VSConstants.S_OK;
            }
            switch ((__VSHPROPID2)propId)
            {
                case __VSHPROPID2.VSHPROPID_CfgPropertyPagesCLSIDList:
                    {
                        var res = base.GetProperty(itemId, propId, out property);
                        property = RemovePropertyPagesFromList((string)property, CfgSpecificPropertyPagesToRemove);
                        return res;
                    }
                case __VSHPROPID2.VSHPROPID_PropertyPagesCLSIDList:
                    {
                        var res = base.GetProperty(itemId, propId, out property);
                        property = RemovePropertyPagesFromList((string)property, PropertyPagesToRemove);
                        return res;
                    }
            }
            switch ((__VSHPROPID8)propId)
            {
                case __VSHPROPID8.VSHPROPID_SupportsIconMonikers:
                    property = true;
                    return VSConstants.S_OK;
            }

            return base.GetProperty(itemId, propId, out property);
        }

        internal static string[] CfgSpecificPropertyPagesToRemove = new[] {
            "{A553AD0B-2F9E-4BCE-95B3-9A1F7074BC27}",   // Package/Publish Web 
            "{9AB2347D-948D-4CD2-8DBE-F15F0EF78ED3}",   // Package/Publish SQL 
        };

        internal static string[] PropertyPagesToRemove = new[] {
            "{8C0201FE-8ECA-403C-92A3-1BC55F031979}",   // typeof(DeployPropertyPageComClass)
            "{ED3B544C-26D8-4348-877B-A1F7BD505ED9}",   // typeof(DatabaseDeployPropertyPageComClass)
            "{909D16B3-C8E8-43D1-A2B8-26EA0D4B6B57}",   // Microsoft.VisualStudio.Web.Application.WebPropertyPage
            "{379354F2-BBB3-4BA9-AA71-FBE7B0E5EA94}"    // Microsoft.VisualStudio.Web.Application.SilverlightLinksPage
        };

        internal string RemovePropertyPagesFromList(string propertyPagesList, string[] pagesToRemove)
        {
            if (pagesToRemove != null)
            {
                propertyPagesList = propertyPagesList.ToUpper(CultureInfo.InvariantCulture);
                foreach (var s in pagesToRemove)
                {
                    var index = propertyPagesList.IndexOf(s, StringComparison.Ordinal);
                    if (index != -1)
                    {
                        // Guids are separated by ';' so if we remove the last one also remove the last ';'
                        var index2 = index + s.Length + 1;
                        if (index2 >= propertyPagesList.Length)
                        {
                            propertyPagesList = propertyPagesList.Substring(0, index).TrimEnd(';');
                        }
                        else
                        {
                            propertyPagesList = propertyPagesList.Substring(0, index) + propertyPagesList.Substring(index2);
                        }
                    }
                }
            }
            return propertyPagesList;
        }

        internal static Guid GetItemType(VSITEMSELECTION vsItemSelection)
        {
            Guid typeGuid;
            try
            {
                ErrorHandler.ThrowOnFailure(
                    vsItemSelection.pHier.GetGuidProperty(
                        vsItemSelection.itemid,
                        (int)__VSHPROPID.VSHPROPID_TypeGuid,
                        out typeGuid
                    )
                );
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                return Guid.Empty;
            }
            return typeGuid;
        }

        private static EnvDTE.ProjectItem GetExtensionObject(IVsHierarchy hierarchy, uint itemId)
        {

            ErrorHandler.ThrowOnFailure(
                hierarchy.GetProperty(
                    itemId,
                    (int)__VSHPROPID.VSHPROPID_ExtObject,
                    out var project
                )
            );

            return (project as EnvDTE.ProjectItem);
        }

        private int OpenWithNodejsEditor(uint selectionItemId)
        {
            // If the item type of this file is not compile, we don't actually want to open with Nodejs and should instead use the default.
            var ourEditor = Guid.Empty;
            var properties = GetExtensionObject(this._innerVsHierarchy, selectionItemId).Properties;

            var view = Guid.Empty;

            // DOCDATAEXISTING_UNKNOWN http://msdn.microsoft.com/en-us/library/vstudio/bb139396(v=vs.110).aspx
            // Force OpenStandardEditor to lookup if the document is currently open or not, and if it is.  If it's
            // open in a different editor the user will be prompted to close it.
            var docDataExistingUnknown = new IntPtr(-1);
            var hr = ((IVsProject3)this._innerVsHierarchy).OpenItemWithSpecific(
                selectionItemId,
                0,
                ref ourEditor,
                null,
                ref view,
                docDataExistingUnknown,
                out var frame
            );
            if (frame != null && ErrorHandler.Succeeded(hr))
            {
                hr = frame.Show();
            }
            return hr;
        }

        #region IVsProject Members

        public int AddItem(uint itemidLoc, VSADDITEMOPERATION dwAddItemOperation, string pszItemName, uint cFilesToOpen, string[] rgpszFilesToOpen, IntPtr hwndDlgOwner, VSADDRESULT[] pResult)
        {
            // Check if we are adding an item to a folder that consists of browser-side code.
            // In this case, we will want to open the file with the default editor.
            var project = this._innerVsHierarchy.GetProject().GetNodejsProject();

            var selectedItems = this.GetSelectedItems().GetEnumerator();
            if (selectedItems.MoveNext())
            {
                var currentId = selectedItems.Current.itemid;
                GetCanonicalName(currentId, out var name);
                var nodeFolderNode = project.FindNodeByFullPath(name) as NodejsFolderNode;
            }

            if (this._innerProject3 != null && IsJavaScriptFile(pszItemName))
            {
                var ourEditor = Guid.Empty;
                var view = Guid.Empty;
                return this._innerProject3.AddItemWithSpecific(
                    itemidLoc,
                    dwAddItemOperation,
                    pszItemName,
                    cFilesToOpen,
                    rgpszFilesToOpen,
                    hwndDlgOwner,
                    dwAddItemOperation == VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE ?
                        (uint)__VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen :
                        0,
                    ref ourEditor,
                    string.Empty,
                    ref view,
                    pResult
                );
            }
            return this._innerProject.AddItem(itemidLoc, dwAddItemOperation, pszItemName, cFilesToOpen, rgpszFilesToOpen, hwndDlgOwner, pResult);
        }

        public int GenerateUniqueItemName(uint itemidLoc, string pszExt, string pszSuggestedRoot, out string pbstrItemName)
        {
            return this._innerProject.GenerateUniqueItemName(itemidLoc, pszExt, pszSuggestedRoot, out pbstrItemName);
        }

        public int GetItemContext(uint itemid, out VisualStudio.OLE.Interop.IServiceProvider ppSP)
        {
            return this._innerProject.GetItemContext(itemid, out ppSP);
        }

        public int GetMkDocument(uint itemid, out string pbstrMkDocument)
        {
            return this._innerProject.GetMkDocument(itemid, out pbstrMkDocument);
        }

        public int IsDocumentInProject(string pszMkDocument, out int pfFound, VSDOCUMENTPRIORITY[] pdwPriority, out uint pitemid)
        {
            return this._innerProject.IsDocumentInProject(pszMkDocument, out pfFound, pdwPriority, out pitemid);
        }

        public int OpenItem(uint itemid, ref Guid rguidLogicalView, IntPtr punkDocDataExisting, out IVsWindowFrame ppWindowFrame)
        {
            if (this._innerProject3 != null && IsJavaScriptFile(GetItemName(this._innerVsHierarchy, itemid)))
            {
                // force .js files opened w/o an editor type to be opened w/ our editor factory.
                var guid = Guid.Empty;
                var view = Guid.Empty;
                var hr = this._innerProject3.OpenItemWithSpecific(
                    itemid,
                    0,
                    ref guid,
                    null,
                    rguidLogicalView,
                    punkDocDataExisting,
                    out ppWindowFrame
                );
                return hr;
            }

            return this._innerProject.OpenItem(itemid, rguidLogicalView, punkDocDataExisting, out ppWindowFrame);
        }

        #endregion

        #region IVsProject2 Members

        public int RemoveItem(uint dwReserved, uint itemid, out int pfResult)
        {
            if (this._innerProject3 != null)
            {
                return this._innerProject3.RemoveItem(dwReserved, itemid, out pfResult);
            }
            pfResult = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int ReopenItem(uint itemid, ref Guid rguidEditorType, string pszPhysicalView, ref Guid rguidLogicalView, IntPtr punkDocDataExisting, out IVsWindowFrame ppWindowFrame)
        {
            if (this._innerProject3 != null)
            {
                if (IsJavaScriptFile(GetItemName(this._innerVsHierarchy, itemid)))
                {
                    // force .js files opened w/o an editor type to be opened w/ our editor factory.
                    // If the item type of this file is not compile, we don't actually want to open with Nodejs and should instead use the default.
                    var itemType = GetExtensionObject(this._innerVsHierarchy, itemid).Properties.Item("ItemType").Value;
                    var guid = Guid.Empty;

                    return this._innerProject3.ReopenItem(
                        itemid,
                        ref guid,
                        pszPhysicalView,
                        ref rguidLogicalView,
                        punkDocDataExisting,
                        out ppWindowFrame
                    );
                }
                return this._innerProject3.ReopenItem(itemid, ref rguidEditorType, pszPhysicalView, ref rguidLogicalView, punkDocDataExisting, out ppWindowFrame);
            }
            ppWindowFrame = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        public void AddedAsRole(object azureProjectHierarchy, string roleType)
        {
            var hier = azureProjectHierarchy as IVsHierarchy;

            if (hier == null)
            {
                return;
            }

            this._package.GetUIThread().Invoke(() =>
            {
                string caption;
                if (ErrorHandler.Failed(this._innerVsHierarchy.GetProperty(
                    (uint)VSConstants.VSITEMID.Root,
                    (int)__VSHPROPID.VSHPROPID_Caption,
                    out var captionObj
                )) || string.IsNullOrEmpty(caption = captionObj as string))
                {
                    return;
                }

                UpdateServiceDefinition(
                    hier,
                    roleType,
                    caption,
                    new ServiceProvider(GetSite())
                );
            });
        }

        private static bool TryGetItemId(object obj, out uint id)
        {
            const uint nil = (uint)VSConstants.VSITEMID.Nil;
            id = obj as uint? ?? nil;
            if (id == nil)
            {
                var asInt = obj as int?;
                if (asInt.HasValue)
                {
                    id = unchecked((uint)asInt.Value);
                }
            }
            return id != nil;
        }

        /// <summary>
        /// Updates the ServiceDefinition.csdef file in
        /// <paramref name="project"/> to include the default startup and
        /// runtime tasks for Python projects.
        /// </summary>
        /// <param name="project">
        /// The Cloud Service project to update.
        /// </param>
        /// <param name="roleType">
        /// The type of role being added, either "Web" or "Worker".
        /// </param>
        /// <param name="projectName">
        /// The name of the role. This typically matches the Caption property.
        /// </param>
        /// <param name="site">
        /// VS service provider.
        /// </param>
        internal static void UpdateServiceDefinition(
            IVsHierarchy project,
            string roleType,
            string projectName,
            System.IServiceProvider site
        )
        {
            Utilities.ArgumentNotNull(nameof(project), project);

            ErrorHandler.ThrowOnFailure(project.GetProperty(
                (uint)VSConstants.VSITEMID.Root,
                (int)__VSHPROPID.VSHPROPID_FirstChild,
                out var obj
            ));

            while (TryGetItemId(obj, out var id))
            {

                if (ErrorHandler.Succeeded(project.GetGuidProperty(id, (int)__VSHPROPID.VSHPROPID_TypeGuid, out var itemType)) &&
                    itemType == VSConstants.GUID_ItemType_PhysicalFile &&
                    ErrorHandler.Succeeded(project.GetProperty(id, (int)__VSHPROPID.VSHPROPID_Name, out obj)) &&
                    "ServiceDefinition.csdef".Equals(obj as string, StringComparison.InvariantCultureIgnoreCase) &&
                    ErrorHandler.Succeeded(project.GetCanonicalName(id, out var mkDoc)) &&
                    !string.IsNullOrEmpty(mkDoc)
                )
                {
                    // We have found the file
                    var rdt = site.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;

                    var updateFileOnDisk = true;

                    if (ErrorHandler.Succeeded(rdt.FindAndLockDocument(
                        (uint)_VSRDTFLAGS.RDT_EditLock,
                        mkDoc,
                        out var docHier,
                        out var docId,
                        out var pDocData,
                        out var docCookie
                    )))
                    {
                        try
                        {
                            if (pDocData != IntPtr.Zero)
                            {
                                try
                                {
                                    // File is open, so edit it through the document
                                    UpdateServiceDefinition(
                                        Marshal.GetObjectForIUnknown(pDocData) as IVsTextLines,
                                        roleType,
                                        projectName
                                    );

                                    ErrorHandler.ThrowOnFailure(rdt.SaveDocuments(
                                        (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_ForceSave,
                                        docHier,
                                        docId,
                                        docCookie
                                    ));

                                    updateFileOnDisk = false;
                                }
                                catch (ArgumentException)
                                {
                                }
                                catch (InvalidOperationException)
                                {
                                }
                                catch (COMException)
                                {
                                }
                                finally
                                {
                                    Marshal.Release(pDocData);
                                }
                            }
                        }
                        finally
                        {
                            ErrorHandler.ThrowOnFailure(rdt.UnlockDocument(
                                (uint)_VSRDTFLAGS.RDT_Unlock_SaveIfDirty | (uint)_VSRDTFLAGS.RDT_RequestUnlock,
                                docCookie
                            ));
                        }
                    }

                    if (updateFileOnDisk)
                    {
                        // File is not open, so edit it on disk
                        FileStream stream = null;
                        try
                        {
                            UpdateServiceDefinition(mkDoc, roleType, projectName);
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Close();
                            }
                        }
                    }

                    break;
                }

                if (ErrorHandler.Failed(project.GetProperty(id, (int)__VSHPROPID.VSHPROPID_NextSibling, out obj)))
                {
                    break;
                }
            }
        }

        private class StringWriterWithEncoding : StringWriter
        {
            private readonly Encoding _encoding;

            public StringWriterWithEncoding(Encoding encoding)
            {
                this._encoding = encoding;
            }

            public override Encoding Encoding => this._encoding;
        }

        private static void UpdateServiceDefinition(IVsTextLines lines, string roleType, string projectName)
        {
            if (lines == null)
            {
                throw new ArgumentException("No IVsTextLines specified", nameof(lines));
            }

            ErrorHandler.ThrowOnFailure(lines.GetLastLineIndex(out var lastLine, out var lastIndex));
            ErrorHandler.ThrowOnFailure(lines.GetLineText(0, 0, lastLine, lastIndex, out var text));

            var doc = new XmlDocument();
            doc.LoadXml(text);

            UpdateServiceDefinition(doc, roleType, projectName);

            var encoding = Encoding.UTF8;

            if (lines is IVsUserData userData)
            {
                var guid = VSConstants.VsTextBufferUserDataGuid.VsBufferEncodingVSTFF_guid;
                int cp;
                if (ErrorHandler.Succeeded(userData.GetData(ref guid, out var data)) &&
                    (cp = (data as int? ?? (int)(data as uint? ?? 0)) & (int)__VSTFF.VSTFF_CPMASK) != 0)
                {
                    try
                    {
                        encoding = Encoding.GetEncoding(cp);
                    }
                    catch (NotSupportedException)
                    {
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            var sw = new StringWriterWithEncoding(encoding);
            doc.Save(XmlWriter.Create(
                sw,
                new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = " ",
                    NewLineHandling = NewLineHandling.Entitize,
                    Encoding = encoding
                }
            ));

            var sb = sw.GetStringBuilder();
            var len = sb.Length;
            var pStr = Marshal.StringToCoTaskMemUni(sb.ToString());

            try
            {
                ErrorHandler.ThrowOnFailure(lines.ReplaceLines(0, 0, lastLine, lastIndex, pStr, len, new TextSpan[1]));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pStr);
            }
        }

        private static void UpdateServiceDefinition(string path, string roleType, string projectName)
        {
            var doc = new XmlDocument();
            doc.Load(path);

            UpdateServiceDefinition(doc, roleType, projectName);

            doc.Save(XmlWriter.Create(
                path,
                new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = " ",
                    NewLineHandling = NewLineHandling.Entitize,
                    Encoding = Encoding.UTF8
                }
            ));
        }

        /// <summary>
        /// Modifies the provided XML document to contain the service definition
        /// nodes needed for the specified project.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="roleType"/> is not one of "Web" or "Worker".
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// A required element is missing from the document.
        /// </exception>
        internal static void UpdateServiceDefinition(XmlDocument doc, string roleType, string projectName)
        {
            var isWeb = roleType == "Web";
            var isWorker = roleType == "Worker";
            if (isWeb == isWorker)
            {
                throw new ArgumentException("Unknown role type: " + (roleType ?? "(null)"), nameof(roleType));
            }

            var nav = doc.CreateNavigator();

            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("sd", "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceDefinition");

            var role = nav.SelectSingleNode(string.Format(CultureInfo.InvariantCulture,
                "/sd:ServiceDefinition/sd:{0}Role[@name='{1}']", roleType, projectName
            ), ns);

            if (role == null)
            {
                throw new InvalidOperationException("Missing role entry");
            }

            var startup = role.SelectSingleNode("sd:Startup", ns);
            if (startup != null)
            {
                startup.DeleteSelf();
            }

            role.AppendChildElement(null, "Startup", null, null);
            startup = role.SelectSingleNode("sd:Startup", ns);
            if (startup == null)
            {
                throw new InvalidOperationException("Missing Startup entry");
            }

            startup.ReplaceSelf(string.Format(CultureInfo.InvariantCulture, @"<Startup>
  <Task commandLine=""setup_{0}.cmd &gt; log.txt"" executionContext=""elevated"" taskType=""simple"">
    <Environment>
      <Variable name=""EMULATED"">
        <RoleInstanceValue xpath=""/RoleEnvironment/Deployment/@emulated"" />
      </Variable>
      <Variable name=""RUNTIMEID"" value=""node"" />
      <Variable name=""RUNTIMEURL"" value=""http://az413943.vo.msecnd.net/node/0.10.21.exe;http://nodertncu.blob.core.windows.net/iisnode/0.1.21.exe"" />
    </Environment>
  </Task>{1}
</Startup>", roleType.ToLowerInvariant(), isWorker ? @"<Task commandLine=""node.cmd .\startup.js"" executionContext=""elevated"" />" : string.Empty));

            if (isWorker)
            {
                var runtime = role.SelectSingleNode("sd:Runtime", ns);
                if (runtime != null)
                {
                    runtime.DeleteSelf();
                }
                role.AppendChildElement(null, "Runtime", null, null);

                runtime = role.SelectSingleNode("sd:Runtime", ns);
                if (startup == null)
                {
                    throw new InvalidOperationException("Missing Runtime entry");
                }

                runtime.ReplaceSelf(@"<Runtime>
  <Environment>
    <Variable name=""EMULATED"">
      <RoleInstanceValue xpath=""/RoleEnvironment/Deployment/@emulated"" />
    </Variable>
  </Environment>
  <EntryPoint>
    <ProgramEntryPoint commandLine=""node.cmd .\server.js"" setReadyOnProcessStart=""true"" />
  </EntryPoint>
</Runtime>");
            }
        }
    }
}
