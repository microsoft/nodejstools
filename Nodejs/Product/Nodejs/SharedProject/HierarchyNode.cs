// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// An object that deals with user interaction via a GUI in the form a hierarchy: a parent node with zero or more child nodes, each of which
    /// can itself be a hierarchy.  
    /// </summary>
    internal abstract class HierarchyNode :
        IDisposable,
        IOleServiceProvider
    {
        public static readonly Guid SolutionExplorer = new Guid(EnvDTE.Constants.vsWindowKindSolutionExplorer);
        public const int NoImage = -1;
#if DEBUG
        internal static int LastTracedProperty;
#endif

        private ProjectElement itemNode;
        private ProjectNode projectMgr;
        private HierarchyNode parentNode;
        private HierarchyNode nextSibling;
        private HierarchyNode firstChild;

        /// <summary>
        /// Remember the last child in the list,
        /// so we can add new nodes quickly during project load.
        /// </summary>
        private HierarchyNode lastChild;

        private uint hierarchyId;
        private HierarchyNodeFlags flags;

        private NodeProperties nodeProperties;

        #region abstract properties
        /// <summary>
        /// The URL of the node.
        /// </summary>
        /// <value></value>
        public abstract string Url
        {
            get;
        }

        /// <summary>
        /// The Caption of the node.
        /// </summary>
        /// <value></value>
        public abstract string Caption
        {
            get;
        }

        /// <summary>
        /// The item type guid associated to a node.
        /// </summary>
        /// <value></value>
        public abstract Guid ItemTypeGuid
        {
            get;
        }
        #endregion

        #region virtual properties

        public virtual bool CanOpenCommandPrompt => false;

        public virtual bool IsNonMemberItem => false;

        /// <summary>
        /// Returns true if the item should be included in search results
        /// 
        /// By default all items in the project are searchable.
        /// </summary>
        public virtual bool IsSearchable => !this.IsNonMemberItem;

        /// <summary>
        /// Gets the full path to where children of this node live on disk.
        /// 
        /// This should only be called on nodes which actually can have children, such
        /// as folders and project nodes.  For all other nodes this will raise an
        /// InvalidOperationException.
        /// 
        /// For a project node, this returns the project home folder.  For folder
        /// nodes this returns the folder's path.  
        /// </summary>
        internal virtual string FullPathToChildren
        {
            get
            {
                Debug.Fail("This node cannot have children");
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Defines a string that is used to separate the name relation from the extension
        /// </summary>
        public virtual string NameRelationSeparator => ".";

        public virtual int MenuCommandId => VsMenus.IDM_VS_CTXT_NOCOMMANDS;
        public virtual Guid MenuGroupId => VsMenus.guidSHLMainMenu;
        /// <summary>
        /// Return an imageindex
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use GetIconMoniker() to specify the icon")]
        public virtual int ImageIndex => NoImage;
        /// <summary>
        /// Return an state icon index
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Sets the state icon for a file.
        /// </summary>
        public virtual VsStateIcon StateIconIndex
        {
            get
            {
                if (!this.ExcludeNodeFromScc)
                {
                    if (this.ProjectMgr.Site.GetService(typeof(SVsSccManager)) is IVsSccManager2 sccManager)
                    {
                        var mkDocument = this.GetMkDocument();
                        if (!string.IsNullOrEmpty(mkDocument))
                        {
                            var statIcons = new VsStateIcon[1] { VsStateIcon.STATEICON_NOSTATEICON };
                            var sccStatus = new uint[1] { 0 };
                            // Get the glyph from the scc manager. Note that it will fail in command line
                            // scenarios.
                            if (ErrorHandler.Succeeded(sccManager.GetSccGlyph(1, new string[] { mkDocument }, statIcons, sccStatus)))
                            {
                                return statIcons[0];
                            }
                        }
                    }
                }

                return VsStateIcon.STATEICON_NOSTATEICON;
            }
        }

        public virtual bool IsLinkFile => false;

        protected virtual VSOVERLAYICON OverlayIconIndex => VSOVERLAYICON.OVERLAYICON_NONE;

        /// <summary>
        /// Defines whether a node can execute a command if in selection.
        /// </summary>
        public virtual bool CanExecuteCommand => true;

        /// <summary>
        /// Used to determine the sort order of different node types
        /// in the solution explorer window.
        /// Nodes with the same priorities are sorted based on their captions.
        /// </summary>
        public virtual int SortPriority => DefaultSortOrderNode.HierarchyNode;
        /// <summary>
        /// Returns an object that is a special view over this object; this is the value
        /// returned by the Object property of the automation objects.
        /// </summary>
        internal virtual object Object => this;
        #endregion

        #region properties

        /// <summary>
        /// Defines the properties attached to this node.
        /// </summary>
        public NodeProperties NodeProperties
        {
            get
            {
                if (null == this.nodeProperties)
                {
                    this.nodeProperties = CreatePropertiesObject();
                }
                return this.nodeProperties;
            }
        }

        [System.ComponentModel.BrowsableAttribute(false)]
        public ProjectNode ProjectMgr
        {
            get
            {
                return this.projectMgr;
            }
            set
            {
                this.projectMgr = value;
            }
        }

        [System.ComponentModel.BrowsableAttribute(false)]
        public HierarchyNode NextSibling
        {
            get
            {
                return this.nextSibling;
            }
            set
            {
                this.nextSibling = value;
            }
        }

        public HierarchyNode FirstChild
        {
            get
            {
                return this.firstChild;
            }
            set
            {
                this.firstChild = value;
            }
        }

        /// <summary>
        /// Returns a sequence containing all of this node's children.
        /// </summary>
        public IEnumerable<HierarchyNode> AllChildren
        {
            get
            {
                for (var node = this.firstChild; node != null; node = node.nextSibling)
                {
                    yield return node;
                }
            }
        }

        public IEnumerable<HierarchyNode> AllDescendants
        {
            get
            {
                var queue = new Queue<HierarchyNode>();
                queue.Enqueue(this);
                while (queue.Count > 0)
                {
                    var node = queue.Dequeue();
                    for (var child = node.firstChild; child != null; child = child.nextSibling)
                    {
                        yield return child;
                        if (child.firstChild != null)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }
        }

        public IEnumerable<HierarchyNode> AllVisibleDescendants
        {
            get
            {
                var queue = new Queue<HierarchyNode>();
                queue.Enqueue(this);
                while (queue.Count > 0)
                {
                    var node = queue.Dequeue();
                    for (var child = node.firstChild; child != null; child = child.nextSibling)
                    {
                        if (child.IsNonMemberItem)
                        {
                            continue;
                        }
                        yield return child;
                        if (child.firstChild != null)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the node is currently visible in the hierarchy.
        /// 
        /// Enables subsetting or supersetting the hierarchy view.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return (this.flags & HierarchyNodeFlags.IsVisible) == HierarchyNodeFlags.IsVisible;
            }
            set
            {
                if (value)
                {
                    this.flags |= HierarchyNodeFlags.IsVisible;
                }
                else
                {
                    this.flags &= ~HierarchyNodeFlags.IsVisible;
                }
            }
        }

        public HierarchyNode PreviousVisibleSibling
        {
            get
            {
                HierarchyNode prev = null;

                if (this.parentNode != null)
                {
                    for (var child = this.parentNode.firstChild; child != null; child = child.nextSibling)
                    {
                        if (child == this)
                        {
                            break;
                        }

                        if (child.IsVisible)
                        {
                            prev = child;
                        }
                    }
                }

                return prev;
            }
        }

        public HierarchyNode NextVisibleSibling
        {
            get
            {
                var next = this.nextSibling;
                while (next != null && !next.IsVisible)
                {
                    next = next.NextSibling;
                }
                return next;
            }
        }

        public HierarchyNode FirstVisibleChild
        {
            get
            {
                var next = this.FirstChild;
                while (next != null && !next.IsVisible)
                {
                    next = next.NextSibling;
                }
                return next;
            }
        }

        [System.ComponentModel.BrowsableAttribute(false)]
        public HierarchyNode Parent
        {
            get
            {
                return this.parentNode;
            }
            set
            {
                this.parentNode = value;
                OnParentSet(value);
            }
        }

        [System.ComponentModel.BrowsableAttribute(false)]
        public uint ID
        {
            get
            {
                return this.hierarchyId;
            }
            internal set
            {
                this.hierarchyId = value;
            }
        }

        [System.ComponentModel.BrowsableAttribute(false)]
        public ProjectElement ItemNode
        {
            get
            {
                return this.itemNode;
            }
            set
            {
                this.itemNode = value;
            }
        }

        [System.ComponentModel.BrowsableAttribute(false)]
        public bool IsExpanded
        {
            get
            {
                return (this.flags & HierarchyNodeFlags.IsExpanded) == HierarchyNodeFlags.IsExpanded;
            }
            set
            {
                if (value)
                {
                    this.flags |= HierarchyNodeFlags.IsExpanded;
                }
                else
                {
                    this.flags &= ~HierarchyNodeFlags.IsExpanded;
                }
            }
        }

        public HierarchyNode PreviousSibling
        {
            get
            {
                if (this.parentNode == null)
                {
                    return null;
                }

                HierarchyNode prev = null;
                for (var child = this.parentNode.firstChild; child != null; child = child.nextSibling)
                {
                    if (child == this)
                    {
                        break;
                    }

                    prev = child;
                }
                return prev;
            }
        }

        /// <summary>
        /// Specifies if a Node is under source control.
        /// </summary>
        public bool ExcludeNodeFromScc
        {
            get
            {
                return (this.flags & HierarchyNodeFlags.ExcludeFromScc) == HierarchyNodeFlags.ExcludeFromScc;
            }
            set
            {
                if (value)
                {
                    this.flags |= HierarchyNodeFlags.ExcludeFromScc;
                }
                else
                {
                    this.flags &= ~HierarchyNodeFlags.ExcludeFromScc;
                }
            }
        }

        /// <summary>
        /// Defines if a node a name relation to its parent node
        /// 
        /// </summary>
        public bool HasParentNodeNameRelation
        {
            get
            {
                return (this.flags & HierarchyNodeFlags.HasParentNodeNameRelation) == HierarchyNodeFlags.HasParentNodeNameRelation;
            }
            set
            {
                if (value)
                {
                    this.flags |= HierarchyNodeFlags.HasParentNodeNameRelation;
                }
                else
                {
                    this.flags &= ~HierarchyNodeFlags.HasParentNodeNameRelation;
                }
            }
        }

        #endregion

        #region ctors

        protected HierarchyNode()
        {
            this.IsExpanded = true;
            this.IsVisible = true;
        }

        protected HierarchyNode(ProjectNode root, ProjectElement element)
        {
            Utilities.ArgumentNotNull(nameof(root), root);
            root.Site.GetUIThread().MustBeCalledFromUIThread();

            this.projectMgr = root;
            this.itemNode = element;
            this.hierarchyId = this.projectMgr.ItemIdMap.Add(this);
            this.IsVisible = true;
        }

        /// <summary>
        /// Overloaded ctor. 
        /// </summary>
        /// <param name="root"></param>
        protected HierarchyNode(ProjectNode root)
        {
            Utilities.ArgumentNotNull(nameof(root), root);
            root.Site.GetUIThread().MustBeCalledFromUIThread();

            this.projectMgr = root;
            this.itemNode = new VirtualProjectElement(this.projectMgr);
            this.hierarchyId = this.projectMgr.ItemIdMap.Add(this);
            this.IsVisible = true;
        }
        #endregion

        #region virtual methods
        /// <summary>
        /// Creates an object derived from NodeProperties that will be used to expose properties
        /// spacific for this object to the property browser.
        /// </summary>
        /// <returns></returns>
        protected virtual NodeProperties CreatePropertiesObject()
        {
            return null;
        }

        protected virtual bool SupportsIconMonikers => false;
        /// <summary>
        /// Returns the icon to use.
        /// </summary>
        protected virtual ImageMoniker GetIconMoniker(bool open)
        {
            return default(ImageMoniker);
        }

        [Obsolete("Use GetIconMoniker() to specify the icon")]
        /// <summary>
        /// Return an icon handle
        /// </summary>
        /// <param name="open"></param>
        /// <returns></returns>
        public virtual object GetIconHandle(bool open)
        {
            var index = this.ImageIndex;
            return index == NoImage ? null : (object)this.ProjectMgr.ImageHandler.GetIconHandle(index);
        }

        /// <summary>
        /// Removes a node from the hierarchy.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        public virtual void RemoveChild(HierarchyNode node)
        {
            Utilities.ArgumentNotNull(nameof(node), node);
            this.projectMgr.Site.GetUIThread().MustBeCalledFromUIThread();
            this.projectMgr.ItemIdMap.Remove(node);

            HierarchyNode previous = null;
            for (var current = this.firstChild; current != null; current = current.nextSibling)
            {
                if (current == node)
                {
                    if (previous != null)
                    {
                        previous.nextSibling = current.nextSibling;
                    }
                    if (current == this.firstChild)
                    {
                        this.firstChild = current.nextSibling;
                    }
                    if (object.ReferenceEquals(node, this.lastChild))
                    {
                        this.lastChild = previous;
                    }
                    return;
                }
                previous = current;
            }
            // Node is no longer in the tree
        }

        /// <summary>
        /// Returns an automation object representing this node
        /// </summary>
        /// <returns>The automation object</returns>
        public virtual object GetAutomationObject()
        {
            return new Automation.OAProjectItem(this.projectMgr.GetAutomationObject() as Automation.OAProject, this);
        }

        /// <summary>
        /// Returns a property object based on a property id 
        /// </summary>
        /// <param name="propId">the property id of the property requested</param>
        /// <returns>the property object requested</returns>
        public virtual object GetProperty(int propId)
        {
            object result = null;
            switch ((__VSHPROPID)propId)
            {
                case __VSHPROPID.VSHPROPID_Expandable:
                    result = (this.firstChild != null);
                    break;

                case __VSHPROPID.VSHPROPID_Caption:
                    result = this.Caption;
                    break;

                case __VSHPROPID.VSHPROPID_Name:
                    result = this.GetItemName();
                    break;

                case __VSHPROPID.VSHPROPID_ExpandByDefault:
                    result = false;
                    break;

#pragma warning disable 0618, 0612
                // Project subclasses decide whether or not to support using image
                // monikers, and so we need to keep the ImageIndex overrides in case
                // they choose not to.
                case __VSHPROPID.VSHPROPID_IconImgList:
                    result = this.ProjectMgr.ImageHandler.ImageList.Handle;
                    break;
                case __VSHPROPID.VSHPROPID_OpenFolderIconIndex:
                case __VSHPROPID.VSHPROPID_IconIndex:
                    var index = this.ImageIndex;
                    if (index != NoImage)
                    {
                        result = index;
                    }
                    break;
                case __VSHPROPID.VSHPROPID_IconHandle:
                    result = GetIconHandle(false);
                    break;
                case __VSHPROPID.VSHPROPID_OpenFolderIconHandle:
                    result = GetIconHandle(true);
                    break;
#pragma warning restore 0618, 0612
                case __VSHPROPID.VSHPROPID_StateIconIndex:
                    result = (int)this.StateIconIndex;
                    break;

                case __VSHPROPID.VSHPROPID_OverlayIconIndex:
                    result = (int)this.OverlayIconIndex;
                    break;

                case __VSHPROPID.VSHPROPID_NextVisibleSibling:
                    var nextVisible = this.NextVisibleSibling;
                    result = (int)((nextVisible != null) ? nextVisible.ID : VSConstants.VSITEMID_NIL);
                    break;

                case __VSHPROPID.VSHPROPID_NextSibling:
                    result = (int)((this.nextSibling != null) ? this.nextSibling.hierarchyId : VSConstants.VSITEMID_NIL);
                    break;

                case __VSHPROPID.VSHPROPID_IsNonMemberItem:
                    result = this.IsNonMemberItem;
                    break;

                case __VSHPROPID.VSHPROPID_IsHiddenItem:
                    result = !this.IsVisible;
                    break;

                case __VSHPROPID.VSHPROPID_IsNonSearchable:
                    result = !this.IsSearchable;
                    break;

                case __VSHPROPID.VSHPROPID_FirstChild:
                    result = (int)((this.firstChild != null) ? this.firstChild.hierarchyId : VSConstants.VSITEMID_NIL);
                    break;

                case __VSHPROPID.VSHPROPID_FirstVisibleChild:
                    var firstVisible = this.FirstVisibleChild;
                    result = (int)((firstVisible != null) ? firstVisible.hierarchyId : VSConstants.VSITEMID_NIL);
                    break;

                case __VSHPROPID.VSHPROPID_Parent:
                    if (null == this.parentNode)
                    {
                        unchecked { result = new IntPtr((int)VSConstants.VSITEMID_NIL); }
                    }
                    else
                    {
                        result = new IntPtr((int)this.parentNode.hierarchyId);  // see bug 176470
                    }
                    break;

                case __VSHPROPID.VSHPROPID_Root:
                    result = Marshal.GetIUnknownForObject(this.projectMgr);
                    break;

                case __VSHPROPID.VSHPROPID_Expanded:
                    result = this.IsExpanded;
                    break;

                case __VSHPROPID.VSHPROPID_BrowseObject:
                    result = this.NodeProperties;
                    if (result != null)
                    {
                        result = new DispatchWrapper(result);
                    }

                    break;

                case __VSHPROPID.VSHPROPID_EditLabel:
                    if (this.ProjectMgr != null && !this.ProjectMgr.IsClosed && !this.ProjectMgr.IsCurrentStateASuppressCommandsMode())
                    {
                        result = GetEditLabel();
                    }
                    break;

                case __VSHPROPID.VSHPROPID_SaveName:
                    //SaveName is the name shown in the Save and the Save Changes dialog boxes.
                    result = this.GetItemName();
                    break;

                case __VSHPROPID.VSHPROPID_ExtObject:
#if DEBUG
                    try
                    {
#endif
                        result = GetAutomationObject();
#if DEBUG
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format("Failed to get automation object for node {1}: {0}", e, this));
                        throw;
                    }
#endif
                    break;
            }

            var id2 = (__VSHPROPID2)propId;
            switch (id2)
            {
                case __VSHPROPID2.VSHPROPID_IsLinkFile:
                    result = this.IsLinkFile;
                    break;

                case __VSHPROPID2.VSHPROPID_NoDefaultNestedHierSorting:
                    return true; // We are doing the sorting ourselves through VSHPROPID_FirstChild and VSHPROPID_NextSibling
                case __VSHPROPID2.VSHPROPID_CfgBrowseObjectCATID:
                case __VSHPROPID2.VSHPROPID_BrowseObjectCATID:
                    {
                        // If there is a browse object and it is a NodeProperties, then get it's CATID
                        var browseObject = this.GetProperty((int)__VSHPROPID.VSHPROPID_BrowseObject);
                        if (browseObject != null)
                        {
                            if (browseObject is DispatchWrapper)
                            {
                                browseObject = ((DispatchWrapper)browseObject).WrappedObject;
                            }
                            result = this.ProjectMgr.GetCATIDForType(browseObject.GetType()).ToString("B");
                            if (StringComparer.Ordinal.Equals(result as string, Guid.Empty.ToString("B")))
                            {
                                result = null;
                            }
                        }
                        break;
                    }
                case __VSHPROPID2.VSHPROPID_ExtObjectCATID:
                    {
                        // If there is a extensibility object and it is a NodeProperties, then get it's CATID
                        var extObject = this.GetProperty((int)__VSHPROPID.VSHPROPID_ExtObject);
                        if (extObject != null)
                        {
                            if (extObject is DispatchWrapper)
                            {
                                extObject = ((DispatchWrapper)extObject).WrappedObject;
                            }
                            result = this.ProjectMgr.GetCATIDForType(extObject.GetType()).ToString("B");
                            if (StringComparer.Ordinal.Equals(result as string, Guid.Empty.ToString("B")))
                            {
                                result = null;
                            }
                        }
                        break;
                    }
            }

            var id5 = (__VSHPROPID5)propId;
            switch (id5)
            {
                case __VSHPROPID5.VSHPROPID_ProvisionalViewingStatus:
                    result = this.ProvisionalViewingStatus;
                    break;
            }
            var id8 = (__VSHPROPID8)propId;
            switch (id8)
            {
                case __VSHPROPID8.VSHPROPID_SupportsIconMonikers:
                    result = this.SupportsIconMonikers;
                    break;

                case __VSHPROPID8.VSHPROPID_IconMonikerGuid:
                    result = GetIconMoniker(false).Guid;
                    break;

                case __VSHPROPID8.VSHPROPID_IconMonikerId:
                    result = GetIconMoniker(false).Id;
                    break;

                case __VSHPROPID8.VSHPROPID_OpenFolderIconMonikerGuid:
                    result = GetIconMoniker(true).Guid;
                    break;

                case __VSHPROPID8.VSHPROPID_OpenFolderIconMonikerId:
                    result = GetIconMoniker(true).Id;
                    break;
            }

#if DEBUG
            if (propId != LastTracedProperty)
            {
                var trailer = (result == null) ? "null" : result.ToString();
                LastTracedProperty = propId; // some basic filtering here...
            }
#endif
            return result;
        }

        public virtual __VSPROVISIONALVIEWINGSTATUS ProvisionalViewingStatus => __VSPROVISIONALVIEWINGSTATUS.PVS_Disabled;

        /// <summary>
        /// Sets the value of a property for a given property id
        /// </summary>
        /// <param name="propid">the property id of the property to be set</param>
        /// <param name="value">value of the property</param>
        /// <returns>S_OK if succeeded</returns>
        public virtual int SetProperty(int propid, object value)
        {
            var id = (__VSHPROPID)propid;

            switch (id)
            {
                case __VSHPROPID.VSHPROPID_Expanded:
                    this.IsExpanded = (bool)value;
                    break;

                case __VSHPROPID.VSHPROPID_EditLabel:
                    return SetEditLabel((string)value);

                default:
                    break;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Get a guid property
        /// </summary>
        /// <param name="propid">property id for the guid property requested</param>
        /// <param name="guid">the requested guid</param>
        /// <returns>S_OK if succeded</returns>
        public virtual int GetGuidProperty(int propid, out Guid guid)
        {
            guid = Guid.Empty;
            if (propid == (int)__VSHPROPID.VSHPROPID_TypeGuid)
            {
                guid = this.ItemTypeGuid;
            }
            var id8 = (__VSHPROPID8)propid;
            switch (id8)
            {
                case __VSHPROPID8.VSHPROPID_IconMonikerGuid:
                    guid = GetIconMoniker(false).Guid;
                    break;

                case __VSHPROPID8.VSHPROPID_OpenFolderIconMonikerGuid:
                    guid = GetIconMoniker(true).Guid;
                    break;
            }

            if (guid.Equals(Guid.Empty))
            {
                return VSConstants.DISP_E_MEMBERNOTFOUND;
            }

            return VSConstants.S_OK;
        }

        public virtual bool CanAddFiles => false;

        /// <summary>
        /// Set a guid property.
        /// </summary>
        /// <param name="propid">property id of the guid property to be set</param>
        /// <param name="guid">the guid to be set</param>
        /// <returns>E_NOTIMPL</returns>
        public virtual int SetGuidProperty(int propid, ref Guid guid)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Called by the shell when a node has been renamed from the GUI
        /// </summary>
        /// <param name="label"></param>
        /// <returns>E_NOTIMPL</returns>
        public virtual int SetEditLabel(string label)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Called by the shell to get the node caption when the user tries to rename from the GUI
        /// </summary>
        /// <returns>the node cation</returns>
        public virtual string GetEditLabel()
        {
            return GetItemName();
        }

        /// <summary>
        /// Returns the underlying file or directory name based on the Url.
        /// </summary>
        /// <returns></returns>
        public string GetItemName()
        {
            return CommonUtils.GetFileOrDirectoryName(this.Url);
        }

        /// <summary>
        /// This method is called by the interface method GetMkDocument to specify the item moniker.
        /// </summary>
        /// <returns>The moniker for this item</returns>
        public virtual string GetMkDocument()
        {
            return string.Empty;
        }

        /// <summary>
        /// Removes items from the hierarchy. Project overwrites this
        /// </summary>
        /// <param name="removeFromStorage"></param>
        public virtual void Remove(bool removeFromStorage)
        {
            var documentToRemove = this.GetMkDocument();

            // Ask Document tracker listeners if we can remove the item.
            var filesToBeDeleted = new string[1] { documentToRemove };
            if (!string.IsNullOrWhiteSpace(documentToRemove))
            {
                var queryRemoveFlags = this.GetQueryRemoveFileFlags(filesToBeDeleted);
                if (!this.ProjectMgr.Tracker.CanRemoveItems(filesToBeDeleted, queryRemoveFlags))
                {
                    return;
                }
            }

            // Close the document if it has a manager.
            var manager = this.GetDocumentManager();
            if (manager != null)
            {
                if (manager.Close(!removeFromStorage ? __FRAMECLOSE.FRAMECLOSE_PromptSave : __FRAMECLOSE.FRAMECLOSE_NoSave) == VSConstants.E_ABORT)
                {
                    // User cancelled operation in message box.
                    return;
                }
            }

            if (removeFromStorage)
            {
                this.DeleteFromStorage(documentToRemove);
            }

            RemoveNonDocument(removeFromStorage);

            // Close the document window if opened.
            CloseDocumentWindow(this);

            RaiseOnItemRemoved(documentToRemove, filesToBeDeleted);

            // Notify hierarchy event listeners that items have been invalidated
            this.ProjectMgr.OnInvalidateItems(this);

            // Dispose the node now that is deleted.
            this.Dispose(true);
        }

        /// <summary>
        /// Determines if the node should open with the designer by default, or if we should
        /// just open with the default editor.
        /// </summary>
        public virtual bool DefaultOpensWithDesignView =>
                // ASPX\ASCX files support design view but should be opened by default with
                // LOGVIEWID_Primary - this is because they support design and html view which
                // is a tools option setting for them. If we force designview this option
                // gets bypassed. We do a similar thing for asax/asmx/xsd. By doing so, we don't force
                // the designer to be invoked when double-clicking on the node - it will now go through the
                // shell's standard open mechanism.
                false;

        /// <summary>
        /// Returns true if the node supports a design view.
        /// </summary>
        public virtual bool SupportsDesignView => false;

        /// <summary>
        /// Returns true if the node represents a code behind file.
        /// </summary>
        public virtual bool IsCodeBehindFile => false;

        /// <summary>
        /// Invoked when the Node's parent is updated.
        /// </summary>
        /// <param name="parent">New parent node.</param>
        protected virtual void OnParentSet(HierarchyNode parent) { /*noop*/ }

        protected virtual void RaiseOnItemRemoved(string documentToRemove, string[] filesToBeDeleted)
        {
            if (!string.IsNullOrWhiteSpace(documentToRemove))
            {
                // Notify document tracker listeners that we have removed the item.
                var removeFlags = this.GetRemoveFileFlags(filesToBeDeleted);
                Debug.Assert(removeFlags != null, "At least an empty array should be returned for the GetRemoveFileFlags");
                this.ProjectMgr.Tracker.OnItemRemoved(documentToRemove, removeFlags[0]);
            }
        }

        internal void RemoveNonDocument(bool removeFromStorage)
        {
            // Check out the project file.
            if (!this.ProjectMgr.QueryEditProjectFile(false))
            {
                throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
            }

            // Notify hierarchy event listeners that the file is going to be removed.
            this.ProjectMgr.OnItemDeleted(this);

            // Remove child if any before removing from the hierarchy
            var child = this.FirstChild;
            while (child != null)
            {
                // Need to read NextSibling before calling Remove
                var next = child.NextSibling;
                child.Remove(removeFromStorage);
                child = next;
            }

            // the project node has no parentNode
            if (this.parentNode != null)
            {
                // Remove from the Hierarchy
                this.parentNode.RemoveChild(this);
            }

            this.itemNode.RemoveFromProjectFile();
        }

        /// <summary>
        /// Returns the relational name which is defined as the first part of the edit label until indexof NameRelationSeparator
        /// </summary>
        public virtual string GetRelationalName()
        {
            //Get the first part of the caption
            var partsOfParent = this.GetItemName().Split(new string[] { this.NameRelationSeparator }, StringSplitOptions.None);
            return partsOfParent[0];
        }

        /// <summary>
        /// Returns the 'extension' of the relational name
        /// e.g. form1.resx returns .resx, form1.designer.cs returns .designer.cs
        /// </summary>
        /// <returns>The extension</returns>
        public virtual string GetRelationNameExtension()
        {
            return this.GetItemName().Substring(this.GetItemName().IndexOf(this.NameRelationSeparator, StringComparison.Ordinal));
        }

        /// <summary>
        /// Close open document frame for a specific fnode.
        /// </summary> 
        protected void CloseDocumentWindow(HierarchyNode node)
        {
            Utilities.ArgumentNotNull(nameof(node), node);

            // We walk the RDT looking for all running documents attached to this hierarchy and itemid. There
            // are cases where there may be two different editors (not views) open on the same document.
            var pRdt = this.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            Utilities.CheckNotNull(pRdt);

            if (ErrorHandler.Succeeded(pRdt.GetRunningDocumentsEnum(out var pEnumRdt)))
            {
                var cookie = new uint[1];
                var saveOptions = (uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_NoSave;
                var srpOurHier = node.projectMgr as IVsHierarchy;

                ErrorHandler.ThrowOnFailure(pEnumRdt.Reset());
                while (VSConstants.S_OK == pEnumRdt.Next(1, cookie, out var fetched))
                {
                    var itemid = VSConstants.VSITEMID_NIL;

                    ErrorHandler.ThrowOnFailure(pRdt.GetDocumentInfo(
                                         cookie[0],
                                         out _,
                                         out _,
                                         out _,
                                         out _,
                                         out var srpHier,
                                         out itemid,
                                         out var ppunkDocData));

                    // Is this one of our documents?
                    if (Utilities.IsSameComObject(srpOurHier, srpHier) && itemid == node.ID)
                    {
                        var soln = GetService(typeof(SVsSolution)) as IVsSolution;
                        ErrorHandler.ThrowOnFailure(soln.CloseSolutionElement(saveOptions, srpOurHier, cookie[0]));
                    }
                    if (ppunkDocData != IntPtr.Zero)
                    {
                        Marshal.Release(ppunkDocData);
                    }
                }
            }
        }

        /// <summary>
        /// Redraws the state icon if the node is not excluded from source control.
        /// </summary>
        protected internal virtual void UpdateSccStateIcons()
        {
            if (!this.ExcludeNodeFromScc)
            {
                this.ProjectMgr.ReDrawNode(this, UIHierarchyElement.SccState);
            }
        }

        /// <summary>
        /// To be overwritten by descendants.
        /// </summary>
        protected internal virtual int SetEditLabel(string label, string relativePath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called by the drag and drop implementation to ask the node
        /// which is being dragged/droped over which nodes should
        /// process the operation.
        /// This allows for dragging to a node that cannot contain
        /// items to let its parent accept the drop
        /// </summary>
        /// <returns>HierarchyNode that accept the drop handling</returns>
        protected internal virtual HierarchyNode GetDragTargetHandlerNode()
        {
            return this;
        }

        /// <summary>
        /// Add a new Folder to the project hierarchy.
        /// </summary>
        /// <returns>S_OK if succeeded, otherwise an error</returns>
        protected virtual int AddNewFolder()
        {
            // Check out the project file.
            if (!this.ProjectMgr.QueryEditProjectFile(false))
            {
                throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
            }

            try
            {
                // Generate a new folder name
                ErrorHandler.ThrowOnFailure(this.projectMgr.GenerateUniqueItemName(this.hierarchyId, string.Empty, string.Empty, out var newFolderName));

                // create the folder node, this will add it to MS build but we won't have the directory created yet.
                var folderNode = this.ProjectMgr.CreateFolderNode(Path.Combine(this.FullPathToChildren, newFolderName));
                folderNode.IsBeingCreated = true;
                AddChild(folderNode);

                folderNode.ExpandItem(EXPANDFLAGS.EXPF_SelectItem);
                var shell = this.projectMgr.Site.GetService(typeof(SVsUIShell)) as IVsUIShell;

                // let the user rename the folder which will create the directory when finished
                int hr;
                object dummy = null;
                var cmdGroup = VsMenus.guidStandardCommandSet97;
                if (ErrorHandler.Failed(hr = shell.PostExecCommand(ref cmdGroup, (uint)VsCommands.Rename, 0, ref dummy)))
                {
                    // make sure the directory is created...
                    folderNode.OnCancelLabelEdit();
                }
            }
            catch (COMException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
                return e.ErrorCode;
            }

            return VSConstants.S_OK;
        }

        protected virtual int AddItemToHierarchy(HierarchyAddType addType)
        {
            IVsAddProjectItemDlg addItemDialog;

            var strFilter = string.Empty;
            uint uiFlags;
            var project = (IVsProject3)this.projectMgr;

            var strBrowseLocations = this.projectMgr.ProjectHome;

            var projectGuid = this.projectMgr.ProjectGuid;

            addItemDialog = this.GetService(typeof(IVsAddProjectItemDlg)) as IVsAddProjectItemDlg;

            if (addType == HierarchyAddType.AddNewItem)
            {
                uiFlags = (uint)(__VSADDITEMFLAGS.VSADDITEM_AddNewItems | __VSADDITEMFLAGS.VSADDITEM_SuggestTemplateName | __VSADDITEMFLAGS.VSADDITEM_AllowHiddenTreeView);
            }
            else
            {
                uiFlags = (uint)(__VSADDITEMFLAGS.VSADDITEM_AddExistingItems | __VSADDITEMFLAGS.VSADDITEM_ProjectHandlesLinks | __VSADDITEMFLAGS.VSADDITEM_AllowMultiSelect | __VSADDITEMFLAGS.VSADDITEM_AllowStickyFilter);
            }

            return addItemDialog.AddProjectItemDlg(this.hierarchyId, ref projectGuid, project, uiFlags, null, null, ref strBrowseLocations, ref strFilter, out var iDontShowAgain); /*&fDontShowAgain*/
        }

        /// <summary>
        /// Overwritten in subclasses
        /// </summary>
        protected virtual void DoDefaultAction()
        {
        }

        /// <summary>
        /// Handles the exclude from project command.
        /// </summary>
        /// <returns></returns>
        internal virtual int ExcludeFromProject()
        {
            Debug.Assert(this.ProjectMgr != null, "The project item " + this.ToString() + " has not been initialised correctly. It has a null ProjectMgr");
            this.Remove(false);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Handles the exclude from project command potentially displaying
        /// a progress bar if the operation can take a long time.
        /// </summary>
        /// <returns></returns>
        internal virtual int ExcludeFromProjectWithProgress()
        {
            var hr = ExcludeFromProject();
            if (ErrorHandler.Succeeded(hr))
            {
                // https://pytools.codeplex.com/workitem/1996
                // Mark the previous sibling or direct parent as the active item
                var windows = UIHierarchyUtilities.GetUIHierarchyWindow(
                    this.ProjectMgr.Site,
                    new Guid(ToolWindowGuids80.SolutionExplorer)) as IVsUIHierarchyWindow2;
                windows.ExpandItem(
                    this.ProjectMgr,
                    this.PreviousVisibleSibling != null ?
                        this.PreviousVisibleSibling.ID :
                        this.Parent.ID,
                    EXPANDFLAGS.EXPF_SelectItem
                );
            }
            return hr;
        }

        /// <summary>
        /// Handles the include in project command.
        /// </summary>
        /// <returns></returns>
        internal virtual int IncludeInProject(bool includeChildren)
        {
            return VSConstants.E_FAIL;
        }

        /// <summary>
        /// Handles the include in project command showing a progress bar
        /// if the operation can potentially take a long time.
        /// </summary>
        internal virtual int IncludeInProjectWithProgress(bool includeChildren)
        {
            return IncludeInProject(includeChildren);
        }

        /// <summary>
        /// Handles the Show in Designer command.
        /// </summary>
        /// <returns></returns>
        protected virtual int ShowInDesigner(IList<HierarchyNode> selectedNodes)
        {
            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <summary>
        /// Prepares a selected node for clipboard. 
        /// It takes the the project reference string of this item and adds it to a stringbuilder. 
        /// </summary>
        /// <returns>A stringbuilder.</returns>
        /// <devremark>This method has to be public since seleceted nodes will call it.</devremark>
        protected internal virtual string PrepareSelectedNodesForClipBoard()
        {
            Debug.Assert(this.ProjectMgr != null, " No project mananager available for this node " + ToString());
            Debug.Assert(this.ProjectMgr.ItemsDraggedOrCutOrCopied != null, " The itemsdragged list should have been initialized prior calling this method");
            if (this.ProjectMgr == null || this.ProjectMgr.ItemsDraggedOrCutOrCopied == null)
            {
                return null;
            }

            if (this.hierarchyId == VSConstants.VSITEMID_ROOT)
            {
                if (this.ProjectMgr.ItemsDraggedOrCutOrCopied != null)
                {
                    this.ProjectMgr.ItemsDraggedOrCutOrCopied.Clear();// abort
                }
                return null;
            }

            if (this.ProjectMgr.ItemsDraggedOrCutOrCopied != null)
            {
                this.ProjectMgr.ItemsDraggedOrCutOrCopied.Add(this);
            }

            var projref = string.Empty;
            if (this.GetService(typeof(IVsSolution)) is IVsSolution solution)
            {
                ErrorHandler.ThrowOnFailure(solution.GetProjrefOfItem(this.ProjectMgr, this.hierarchyId, out projref));
                if (string.IsNullOrEmpty(projref))
                {
                    if (this.ProjectMgr.ItemsDraggedOrCutOrCopied != null)
                    {
                        this.ProjectMgr.ItemsDraggedOrCutOrCopied.Clear();// abort
                    }
                    return null;
                }
            }

            // Append the projectref and a null terminator to the string builder

            return projref + '\0';
        }

        /// <summary>
        /// Returns the Cannonical Name
        /// </summary>
        /// <returns>Cannonical Name</returns>
        internal virtual string GetCanonicalName()
        {
            return this.GetMkDocument();
        }

        /// <summary>
        /// Factory method for the Document Manager object
        /// </summary>
        /// <returns>null object, since a hierarchy node does not know its kind of document</returns>
        /// <remarks>Must be overriden by derived node classes if a document manager is needed</remarks>
        protected internal virtual DocumentManager GetDocumentManager()
        {
            return null;
        }

        /// <summary>
        /// Displays the context menu.
        /// </summary>
        /// <param name="selectedNodes">list of selected nodes.</param>
        /// <param name="pointerToVariant">contains the location (x,y) at which to show the menu.</param>
        protected virtual int DisplayContextMenu(IList<HierarchyNode> selectedNodes, IntPtr pointerToVariant)
        {
            if (selectedNodes == null || selectedNodes.Count == 0 || pointerToVariant == IntPtr.Zero)
            {
                return NativeMethods.OLECMDERR_E_NOTSUPPORTED;
            }

            var projectsSelected = 0;
            var menuId = 0;
            var menuGroup = Guid.Empty;

            var groupIsConsistent = false;
            var cmdidIsConsistent = false;

            foreach (var node in selectedNodes)
            {
                var cmdId = node.MenuCommandId;
                var grpId = node.MenuGroupId;
                if (cmdId == VsMenus.IDM_VS_CTXT_PROJNODE)
                {
                    projectsSelected += 1;
                }

                // We check here whether we have a multiple selection of
                // nodes of differing type.
                if (menuId == 0)
                {
                    // First time through or single node case
                    menuId = cmdId;
                    cmdidIsConsistent = true;
                    menuGroup = grpId;
                    groupIsConsistent = true;
                }
                else
                {
                    if (menuGroup != grpId)
                    {
                        // We have very different node types. If a project is in
                        // the selection, we will eventually display its context
                        // menu. More likely, we will display nothing.
                        groupIsConsistent = false;
                    }
                    else if (menuId != node.MenuCommandId)
                    {
                        // We have different node types.
                        cmdidIsConsistent = false;
                    }
                }
            }

            if (groupIsConsistent && !cmdidIsConsistent)
            {
                // The selected items agree on a menu group, but not the ID.
                if (projectsSelected == 0)
                {
                    // We will use IDM_VS_CTXT_XPROJ_MULTIITEM (0x0419) with
                    // whatever group they agreed on. This allows people to create
                    // multi-selection context menus in custom groups.
                    menuId = VsMenus.IDM_VS_CTXT_XPROJ_MULTIITEM;
                    cmdidIsConsistent = true;
                }
                else
                {
                    // One or more projects were selected, so we will use
                    // IDM_VS_CTXT_XPROJ_PROJITEM (0x0417) with whatever group
                    // they agreed on.
                    menuId = VsMenus.IDM_VS_CTXT_XPROJ_PROJITEM;
                    cmdidIsConsistent = true;
                }
            }

            if (!groupIsConsistent)
            {
                // The selected items could not agree on a group. If projects
                // are selected, display the project context menu. Otherwise,
                // show nothing.
                if (projectsSelected > 0)
                {
                    menuId = projectsSelected == 1 ?
                        VsMenus.IDM_VS_CTXT_PROJNODE :
                        VsMenus.IDM_VS_CTXT_XPROJ_PROJITEM;
                    menuGroup = VsMenus.guidSHLMainMenu;
                    groupIsConsistent = true;
                    cmdidIsConsistent = true;
                }
            }

            if (groupIsConsistent && cmdidIsConsistent)
            {
                var variant = Marshal.GetObjectForNativeVariant(pointerToVariant);
                var pointsAsUint = (UInt32)variant;
                var x = (short)(pointsAsUint & 0x0000ffff);
                var y = (short)((pointsAsUint & 0xffff0000) / 0x10000);

                var points = new POINTS();
                points.x = x;
                points.y = y;
                return ShowContextMenu(menuId, menuGroup, points);
            }
            else
            {
                return VSConstants.S_OK;
            }
        }

        /// <summary>
        /// Shows the specified context menu at a specified location.
        /// </summary>
        /// <param name="menuId">The context menu ID.</param>
        /// <param name="groupGuid">The GUID of the menu group.</param>
        /// <param name="points">The location at which to show the menu.</param>
        protected virtual int ShowContextMenu(int menuId, Guid menuGroup, POINTS points)
        {
            var shell = this.projectMgr.Site.GetService(typeof(SVsUIShell)) as IVsUIShell;

            Debug.Assert(shell != null, "Could not get the UI shell from the project");
            if (shell == null)
            {
                return VSConstants.E_FAIL;
            }
            var pnts = new POINTS[1];
            pnts[0].x = points.x;
            pnts[0].y = points.y;
            return shell.ShowContextMenu(0, ref menuGroup, menuId, pnts, (Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget)this.ProjectMgr);
        }

        #region initiation of command execution
        /// <summary>
        /// Handles command execution.
        /// </summary>
        /// <param name="cmdGroup">Unique identifier of the command group</param>
        /// <param name="cmd">The command to be executed.</param>
        /// <param name="nCmdexecopt">Values describe how the object should execute the command.</param>
        /// <param name="pvaIn">Pointer to a VARIANTARG structure containing input arguments. Can be NULL</param>
        /// <param name="pvaOut">VARIANTARG structure to receive command output. Can be NULL.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        internal virtual int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (InvalidProject())
            {
                return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
            }

            if (cmdGroup == Guid.Empty)
            {
                return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
            }
            else if (cmdGroup == VsMenus.guidVsUIHierarchyWindowCmds)
            {
                switch (cmd)
                {
                    case (uint)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_DoubleClick:
                    case (uint)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_EnterKey:
                        this.DoDefaultAction();
                        return VSConstants.S_OK;
                    case (uint)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_CancelLabelEdit:
                        this.OnCancelLabelEdit();
                        return VSConstants.S_OK;
                }
                return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
            }
            else if (cmdGroup == VsMenus.guidStandardCommandSet97)
            {
                var nodeToAddTo = this.GetDragTargetHandlerNode();
                switch ((VsCommands)cmd)
                {
                    case VsCommands.AddNewItem:
                        return nodeToAddTo.AddItemToHierarchy(HierarchyAddType.AddNewItem);

                    case VsCommands.AddExistingItem:
                        return nodeToAddTo.AddItemToHierarchy(HierarchyAddType.AddExistingItem);

                    case VsCommands.NewFolder:
                        return nodeToAddTo.AddNewFolder();

                    case VsCommands.Paste:
                        return this.ProjectMgr.PasteFromClipboard(this);
                }
            }
            else if (cmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                switch ((VsCommands2K)cmd)
                {
                    case VsCommands2K.EXCLUDEFROMPROJECT:
                        return this.ExcludeFromProjectWithProgress();
                    case VsCommands2K.INCLUDEINPROJECT:
                        return this.IncludeInProjectWithProgress(true);
                }
            }
            else if (cmdGroup == this.ProjectMgr.SharedCommandGuid)
            {
                switch ((SharedCommands)cmd)
                {
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
                    case SharedCommands.CopyFullPath:
                        System.Windows.Clipboard.SetText(this.Url);
                        return VSConstants.S_OK;
                }
            }

            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        #endregion

        #region query command handling

        /// <summary>
        /// Handles command status on a node. Should be overridden by descendant nodes. If a command cannot be handled then the base should be called.
        /// </summary>
        /// <param name="cmdGroup">A unique identifier of the command group. The pguidCmdGroup parameter can be NULL to specify the standard group.</param>
        /// <param name="cmd">The command to query status for.</param>
        /// <param name="pCmdText">Pointer to an OLECMDTEXT structure in which to return the name and/or status information of a single command. Can be NULL to indicate that the caller does not require this information.</param>
        /// <param name="result">An out parameter specifying the QueryStatusResult of the command.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        internal virtual int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (cmdGroup == VsMenus.guidStandardCommandSet97)
            {
                switch ((VsCommands)cmd)
                {
                    case VsCommands.AddNewItem:
                    case VsCommands.AddExistingItem:
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
                // http://social.msdn.microsoft.com/Forums/en/vsx/thread/f348aaed-cdcc-4709-9118-c0fd8b9e154d
                switch ((VsCommands2K)cmd)
                {
                    case VsCommands2K.SHOWALLFILES:
                        if (this.ProjectMgr.CanShowAllFiles)
                        {
                            if (this.ProjectMgr.IsShowingAllFiles)
                            {
                                result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED | QueryStatusResult.LATCHED;
                            }
                            else
                            {
                                result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                            }
                        }
                        else
                        {
                            result |= QueryStatusResult.NOTSUPPORTED | QueryStatusResult.INVISIBLE;
                        }
                        return VSConstants.S_OK;
                }
            }
            else if (cmdGroup == this.ProjectMgr.SharedCommandGuid)
            {
                switch ((SharedCommands)cmd)
                {
                    case SharedCommands.OpenCommandPromptHere:
                        if (this.CanOpenCommandPrompt)
                        {
                            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                            return VSConstants.S_OK;
                        }
                        break;
                    case SharedCommands.CopyFullPath:
                        if (this is IDiskBasedNode || this is ProjectNode)
                        {
                            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                            return VSConstants.S_OK;
                        }
                        break;
                }
            }

            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        #endregion
        internal virtual bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
        {
            return this.ProjectMgr.CanProjectDeleteItems;
        }

        /// <summary>
        /// Overwrite this method to tell that you support the default icon for this node.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CanShowDefaultIcon()
        {
            return false;
        }

        /// <summary>
        /// Performs save as operation for an item after the save as dialog has been processed.
        /// </summary>
        /// <param name="docData">A pointer to the rdt</param>
        /// <param name="newName">The newName of the item</param>
        /// <returns></returns>
        internal virtual int AfterSaveItemAs(IntPtr docData, string newName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked when the node receives UIHWCMDID_CancelLabelEdit hierarchy window command, which occurs
        /// when user cancels the label editing operation.
        /// </summary>
        protected virtual void OnCancelLabelEdit()
        {
        }

        /// <summary>
        /// The method that does the cleanup.
        /// </summary>
        /// <param name="disposing">Is the Dispose called by some internal member, or it is called by from GC.</param>
        protected virtual void Dispose(bool disposing)
        {
            this.firstChild = null;
            this.lastChild = null;
            this.nextSibling = null;
        }

        /// <summary>
        /// Sets the VSQUERYADDFILEFLAGS that will be used to call the  IVsTrackProjectDocumentsEvents2 OnQueryAddFiles
        /// </summary>
        /// <param name="files">The files to which an array of VSADDFILEFLAGS has to be specified.</param>
        /// <returns></returns>
        protected internal virtual VSQUERYADDFILEFLAGS[] GetQueryAddFileFlags(string[] files)
        {
            if (files == null || files.Length == 0)
            {
                return new VSQUERYADDFILEFLAGS[1] { VSQUERYADDFILEFLAGS.VSQUERYADDFILEFLAGS_NoFlags };
            }

            var queryAddFileFlags = new VSQUERYADDFILEFLAGS[files.Length];

            for (var i = 0; i < files.Length; i++)
            {
                queryAddFileFlags[i] = VSQUERYADDFILEFLAGS.VSQUERYADDFILEFLAGS_NoFlags;
            }

            return queryAddFileFlags;
        }

        /// <summary>
        /// Sets the VSREMOVEFILEFLAGS that will be used to call the  IVsTrackProjectDocumentsEvents2 OnRemoveFiles
        /// </summary>
        /// <param name="files">The files to which an array of VSREMOVEFILEFLAGS has to be specified.</param>
        /// <returns></returns>
        protected internal virtual VSREMOVEFILEFLAGS[] GetRemoveFileFlags(string[] files)
        {
            if (files == null || files.Length == 0)
            {
                return new VSREMOVEFILEFLAGS[1] { VSREMOVEFILEFLAGS.VSREMOVEFILEFLAGS_NoFlags };
            }

            var removeFileFlags = new VSREMOVEFILEFLAGS[files.Length];

            for (var i = 0; i < files.Length; i++)
            {
                removeFileFlags[i] = VSREMOVEFILEFLAGS.VSREMOVEFILEFLAGS_NoFlags;
            }

            return removeFileFlags;
        }

        /// <summary>
        /// Sets the VSQUERYREMOVEFILEFLAGS that will be used to call the  IVsTrackProjectDocumentsEvents2 OnQueryRemoveFiles
        /// </summary>
        /// <param name="files">The files to which an array of VSQUERYREMOVEFILEFLAGS has to be specified.</param>
        /// <returns></returns>
        protected internal virtual VSQUERYREMOVEFILEFLAGS[] GetQueryRemoveFileFlags(string[] files)
        {
            if (files == null || files.Length == 0)
            {
                return new VSQUERYREMOVEFILEFLAGS[1] { VSQUERYREMOVEFILEFLAGS.VSQUERYREMOVEFILEFLAGS_NoFlags };
            }

            var queryRemoveFileFlags = new VSQUERYREMOVEFILEFLAGS[files.Length];

            for (var i = 0; i < files.Length; i++)
            {
                queryRemoveFileFlags[i] = VSQUERYREMOVEFILEFLAGS.VSQUERYREMOVEFILEFLAGS_NoFlags;
            }

            return queryRemoveFileFlags;
        }

        /// <summary>
        /// This method should be overridden to provide the list of files and associated flags for source control.
        /// </summary>
        /// <param name="files">The list of files to be placed under source control.</param>
        /// <param name="flags">The flags that are associated to the files.</param>
        protected internal virtual void GetSccFiles(IList<string> files, IList<tagVsSccFilesFlags> flags)
        {
            if (this.ExcludeNodeFromScc || this.IsNonMemberItem)
            {
                return;
            }
            Utilities.ArgumentNotNull(nameof(files), files);
            Utilities.ArgumentNotNull(nameof(flags), flags);

            files.Add(this.GetMkDocument());

            var flagsToAdd = (this.firstChild != null && (this.firstChild is DependentFileNode)) ? tagVsSccFilesFlags.SFF_HasSpecialFiles : tagVsSccFilesFlags.SFF_NoFlags;

            flags.Add(flagsToAdd);
        }

        /// <summary>
        /// This method should be overridden to provide the list of special files and associated flags for source control.
        /// </summary>
        /// <param name="sccFile">One of the file associated to the node.</param>
        /// <param name="files">The list of files to be placed under source control.</param>
        /// <param name="flags">The flags that are associated to the files.</param>
        protected internal virtual void GetSccSpecialFiles(string sccFile, IList<string> files, IList<tagVsSccFilesFlags> flags)
        {
            if (this.ExcludeNodeFromScc)
            {
                return;
            }

            Utilities.ArgumentNotNull(nameof(files), files);
            Utilities.ArgumentNotNull(nameof(flags), flags);
        }

        /// <summary>
        /// Delete the item corresponding to the specified path from storage.
        /// </summary>
        /// <param name="path">Url of the item to delete</param>
        internal protected virtual void DeleteFromStorage(string path)
        {
        }

        /// <summary>
        /// Determines whether a file change should be ignored or not.
        /// </summary>
        /// <param name="ignoreFlag">Flag indicating whether or not to ignore changes (true to ignore changes).</param>
        protected internal virtual void IgnoreItemFileChanges(bool ignoreFlag)
        {
        }

        /// <summary>
        /// Called to determine whether a project item is reloadable. 
        /// </summary>
        /// <returns>True if the project item is reloadable.</returns>
        protected internal virtual bool IsItemReloadable()
        {
            return true;
        }

        /// <summary>
        /// Reloads an item.
        /// </summary>
        /// <param name="reserved">Reserved parameter defined at the IVsPersistHierarchyItem2::ReloadItem parameter.</param>
        protected internal virtual void ReloadItem(uint reserved)
        {
        }

        protected internal virtual void ShowDeleteMessage(IList<HierarchyNode> nodes, __VSDELETEITEMOPERATION action, out bool cancel, out bool useStandardDialog)
        {
            useStandardDialog = true;
            cancel = true;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Clears the cached node properties so that it will be recreated on the next request.
        /// </summary>
        public void ResetNodeProperties()
        {
            this.nodeProperties = null;
        }

        public void ExpandItem(EXPANDFLAGS flags)
        {
            if (this.ProjectMgr == null || this.ProjectMgr.Site == null)
            {
                return;
            }
            this.ProjectMgr.AssertHasParentHierarchy();
            var windows = UIHierarchyUtilities.GetUIHierarchyWindow(
                this.ProjectMgr.Site,
                new Guid(ToolWindowGuids80.SolutionExplorer)) as IVsUIHierarchyWindow2;

            if (windows == null)
            {
                return;
            }

            ErrorHandler.ThrowOnFailure(windows.ExpandItem(this.ProjectMgr.GetOuterInterface<IVsUIHierarchy>(), this.ID, flags));
        }

        public bool GetIsExpanded()
        {
            if (this.ProjectMgr == null || this.ProjectMgr.Site == null || this.ProjectMgr.ParentHierarchy == null)
            {
                return false;
            }

            var windows = UIHierarchyUtilities.GetUIHierarchyWindow(
                this.ProjectMgr.Site,
                new Guid(ToolWindowGuids80.SolutionExplorer)) as IVsUIHierarchyWindow2;

            if (windows == null)
            {
                return false;
            }

            if (ErrorHandler.Succeeded(windows.GetItemState(this.ProjectMgr.GetOuterInterface<IVsUIHierarchy>(),
                this.ID,
                (uint)__VSHIERARCHYITEMSTATE.HIS_Expanded,
                out var state)))
            {
                return state != 0;
            }
            return false;
        }

        /// <summary>
        /// AddChild - add a node, sorted in the right location.
        /// </summary>
        /// <param name="node">The node to add.</param>
        public virtual void AddChild(HierarchyNode node)
        {
            Utilities.ArgumentNotNull(nameof(node), node);

            Debug.Assert(this.ProjectMgr.ItemIdMap[node.hierarchyId] == null || this.ProjectMgr.ItemIdMap[node.hierarchyId] == node);

            HierarchyNode previous = null;
            HierarchyNode previousVisible = null;
            if (this.lastChild != null && this.ProjectMgr.CompareNodes(node, this.lastChild) < 0)
            {
                // we can add the node at the end of the list quickly:
                previous = this.lastChild;
                previous.nextSibling = node;
                if (previous.IsVisible)
                {
                    previousVisible = previous;
                }

                this.lastChild = node;
                node.nextSibling = null;
            }
            else
            {
                // merge node into the list:
                for (var n = this.firstChild; n != null; n = n.nextSibling)
                {
                    if (this.ProjectMgr.CompareNodes(node, n) > 0)
                    {
                        break;
                    }

                    previous = n;
                    if (previous.IsVisible)
                    {
                        previousVisible = previous;
                    }
                }
                // insert "node" after "previous".
                if (previous != null)
                {
                    node.nextSibling = previous.nextSibling;
                    previous.nextSibling = node;
                }
                else
                {
                    node.nextSibling = this.firstChild;
                    this.firstChild = node;
                }
                if (node.nextSibling == null)
                {
                    this.lastChild = node;
                }
            }

            node.Parent = this;

            this.ProjectMgr.OnItemAdded(this, node, previousVisible);
        }

        public object GetService(Type type)
        {
            Utilities.ArgumentNotNull(nameof(type), type);

            if (this.projectMgr == null || this.projectMgr.Site == null)
            {
                return null;
            }

            return this.projectMgr.Site.GetService(type);
        }

        #endregion

        #region IDisposable
        /// <summary>
        /// The IDispose interface Dispose method for disposing the object determinastically.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public virtual void Close()
        {
            var manager = this.GetDocumentManager();
            try
            {
                if (manager != null)
                {
                    manager.Close(__FRAMECLOSE.FRAMECLOSE_PromptSave);
                }
            }
            catch
            {
            }
            finally
            {
                this.Dispose(true);
            }
        }

        internal uint HierarchyId => this.hierarchyId;

        #region helper methods

        /// <summary>
        /// Searches the immediate children of this node for a node which matches the specified predicate.
        /// </summary>
        internal HierarchyNode FindImmediateChild(Func<HierarchyNode, bool> predicate)
        {
            for (var child = this.firstChild; child != null; child = child.NextSibling)
            {
                if (predicate(child))
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Searches the immediate children of this node for a file who's filename (w/o path) matches
        /// the requested name.
        /// </summary>
        internal HierarchyNode FindImmediateChildByName(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(GetMkDocument()));

            for (var child = this.firstChild; child != null; child = child.NextSibling)
            {
                var filename = CommonUtils.GetFileOrDirectoryName(child.ItemNode.GetMetadata(ProjectFileConstants.Include));

                if (StringComparer.OrdinalIgnoreCase.Equals(filename, name))
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Recursively find all nodes of type T
        /// </summary>
        /// <typeparam name="T">The type of hierachy node being serched for</typeparam>
        /// <param name="nodes">A list of nodes of type T</param>
        internal void FindNodesOfType<T>(List<T> nodes)
            where T : HierarchyNode
        {
            for (var n = this.FirstChild; n != null; n = n.NextSibling)
            {
                if (n is T nodeAsT)
                {
                    nodes.Add(nodeAsT);
                }

                n.FindNodesOfType<T>(nodes);
            }
        }

        /// <summary>
        /// Recursively find all nodes of type T
        /// </summary>
        /// <typeparam name="T">The type of hierachy node being serched for</typeparam>
        /// <param name="nodes">A list of nodes of type T</param>
        internal IEnumerable<T> EnumNodesOfType<T>()
            where T : HierarchyNode
        {
            for (var n = this.FirstChild; n != null; n = n.NextSibling)
            {
                if (n is T nodeAsT)
                {
                    yield return nodeAsT;
                }

                foreach (var node in n.EnumNodesOfType<T>())
                {
                    yield return node;
                }
            }
        }

        #endregion

        private bool InvalidProject()
        {
            return this.projectMgr == null || this.projectMgr.IsClosed;
        }

        #region nested types
        /// <summary>
        /// DropEffect as defined in oleidl.h
        /// </summary>
        internal enum DropEffect
        {
            None,
            Copy = 1,
            Move = 2,
            Link = 4
        };
        #endregion

        #region IOleServiceProvider

        int IOleServiceProvider.QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject)
        {
            var hr = QueryService(ref guidService, out var obj);
            if (ErrorHandler.Succeeded(hr))
            {
                if (riid.Equals(NativeMethods.IID_IUnknown))
                {
                    ppvObject = Marshal.GetIUnknownForObject(obj);
                    return VSConstants.S_OK;
                }

                var pUnk = IntPtr.Zero;
                try
                {
                    pUnk = Marshal.GetIUnknownForObject(obj);
                    return Marshal.QueryInterface(pUnk, ref riid, out ppvObject);
                }
                finally
                {
                    if (pUnk != IntPtr.Zero)
                    {
                        Marshal.Release(pUnk);
                    }
                }
            }

            ppvObject = IntPtr.Zero;
            return hr;
        }

        /// <summary>
        /// Provides services for this hierarchy node.  These services are proffered to consumers
        /// via IVsProject.GetItemContext.  When a service provider is requested we hand out
        /// the hierarchy node which implements IServiceProvider directly.  Nodes can override
        /// this function to provide the underlying object which implements the service.
        /// 
        /// By default we support handing out the parent project when IVsHierarchy is requested.
        /// Project nodes support handing their own automation object out, and other services
        /// such as the Xaml designer context type can also be provided.
        /// </summary>
        public virtual int QueryService(ref Guid guidService, out object result)
        {
            if (guidService == typeof(IVsHierarchy).GUID)
            {
                result = this.ProjectMgr;
                return VSConstants.S_OK;
            }

            result = null;
            return VSConstants.E_FAIL;
        }

        #endregion
    }
}
