// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.VisualStudioTools.Navigation
{
    /// <summary>
    /// Implements a simple library that tracks project symbols, objects etc.
    /// </summary>
    internal class Library : IVsSimpleLibrary2
    {
        private readonly Guid guid;
        private readonly object syncLock = new object();
        private LibraryNode root;
        private uint updateCount;

        public Library(Guid libraryGuid)
        {
            this.guid = libraryGuid;
            this.root = new LibraryNode(null, string.Empty, string.Empty, LibraryNodeType.Package);
        }

        public _LIB_FLAGS2 LibraryCapabilities { get; set; }

        internal void AddNode(LibraryNode node)
        {
            lock (syncLock)
            {
                // re-create root node here because we may have handed out the node before and don't want to mutate it's list.
                this.root = this.root.Clone();
                this.root.AddNode(node);
                this.updateCount++;
            }
        }

        internal void RemoveNode(LibraryNode node)
        {
            lock (syncLock)
            {
                this.root = this.root.Clone();
                this.root.RemoveNode(node);
                this.updateCount++;
            }
        }

        #region IVsSimpleLibrary2 Members

        public int AddBrowseContainer(VSCOMPONENTSELECTORDATA[] pcdComponent, ref uint pgrfOptions, out string pbstrComponentAdded)
        {
            pbstrComponentAdded = null;
            return VSConstants.E_NOTIMPL;
        }

        public int CreateNavInfo(SYMBOL_DESCRIPTION_NODE[] rgSymbolNodes, uint ulcNodes, out IVsNavInfo ppNavInfo)
        {
            ppNavInfo = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetBrowseContainersForHierarchy(IVsHierarchy pHierarchy, uint celt, VSBROWSECONTAINER[] rgBrowseContainers, uint[] pcActual)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int GetGuid(out Guid pguidLib)
        {
            pguidLib = this.guid;
            return VSConstants.S_OK;
        }

        public int GetLibFlags2(out uint pgrfFlags)
        {
            pgrfFlags = (uint)this.LibraryCapabilities;
            return VSConstants.S_OK;
        }

        public int GetList2(uint ListType, uint flags, VSOBSEARCHCRITERIA2[] pobSrch, out IVsSimpleObjectList2 ppIVsSimpleObjectList2)
        {
            if ((flags & (uint)_LIB_LISTFLAGS.LLF_RESOURCEVIEW) != 0)
            {
                ppIVsSimpleObjectList2 = null;
                return VSConstants.E_NOTIMPL;
            }

            ICustomSearchListProvider listProvider;
            if (pobSrch != null &&
                pobSrch.Length > 0)
            {
                if ((listProvider = pobSrch[0].pIVsNavInfo as ICustomSearchListProvider) != null)
                {
                    switch ((_LIB_LISTTYPE)ListType)
                    {
                        case _LIB_LISTTYPE.LLT_NAMESPACES:
                            ppIVsSimpleObjectList2 = listProvider.GetSearchList();
                            break;
                        default:
                            ppIVsSimpleObjectList2 = null;
                            return VSConstants.E_FAIL;
                    }
                }
                else
                {
                    if (pobSrch[0].eSrchType == VSOBSEARCHTYPE.SO_ENTIREWORD && ListType == (uint)_LIB_LISTTYPE.LLT_MEMBERS)
                    {
                        var srchText = pobSrch[0].szName;
                        int colonIndex;
                        if ((colonIndex = srchText.LastIndexOf(':')) != -1)
                        {
                            var filename = srchText.Substring(0, srchText.LastIndexOf(':'));
                            foreach (var project in this.root.Children)
                            {
                                foreach (var item in project.Children)
                                {
                                    if (item.FullName == filename)
                                    {
                                        ppIVsSimpleObjectList2 = item.DoSearch(pobSrch[0]);
                                        if (ppIVsSimpleObjectList2 != null)
                                        {
                                            return VSConstants.S_OK;
                                        }
                                    }
                                }
                            }
                        }

                        ppIVsSimpleObjectList2 = null;
                        return VSConstants.E_FAIL;
                    }
                    else if (pobSrch[0].eSrchType == VSOBSEARCHTYPE.SO_SUBSTRING && ListType == (uint)_LIB_LISTTYPE.LLT_NAMESPACES)
                    {
                        var lib = new LibraryNode(null, "Search results " + pobSrch[0].szName, "Search results " + pobSrch[0].szName, LibraryNodeType.Package);
                        foreach (var item in SearchNodes(pobSrch[0], new SimpleObjectList<LibraryNode>(), this.root).Children)
                        {
                            lib.Children.Add(item);
                        }
                        ppIVsSimpleObjectList2 = lib;
                        return VSConstants.S_OK;
                    }
                    else
                    {
                        ppIVsSimpleObjectList2 = null;
                        return VSConstants.E_FAIL;
                    }
                }
            }
            else
            {
                ppIVsSimpleObjectList2 = this.root as IVsSimpleObjectList2;
            }
            return VSConstants.S_OK;
        }

        private static SimpleObjectList<LibraryNode> SearchNodes(VSOBSEARCHCRITERIA2 srch, SimpleObjectList<LibraryNode> list, LibraryNode curNode)
        {
            foreach (var child in curNode.Children)
            {
                if (child.Name.IndexOf(srch.szName, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    list.Children.Add(child.Clone(child.Name));
                }

                SearchNodes(srch, list, child);
            }
            return list;
        }

        public void VisitNodes(ILibraryNodeVisitor visitor, CancellationToken ct = default(CancellationToken))
        {
            lock (syncLock)
            {
                this.root.Visit(visitor, ct);
            }
        }

        public int GetSeparatorStringWithOwnership(out string pbstrSeparator)
        {
            pbstrSeparator = ".";
            return VSConstants.S_OK;
        }

        public int GetSupportedCategoryFields2(int Category, out uint pgrfCatField)
        {
            pgrfCatField = (uint)_LIB_CATEGORY2.LC_HIERARCHYTYPE | (uint)_LIB_CATEGORY2.LC_PHYSICALCONTAINERTYPE;
            return VSConstants.S_OK;
        }

        public int LoadState(IStream pIStream, LIB_PERSISTTYPE lptType)
        {
            return VSConstants.S_OK;
        }

        public int RemoveBrowseContainer(uint dwReserved, string pszLibName)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int SaveState(IStream pIStream, LIB_PERSISTTYPE lptType)
        {
            return VSConstants.S_OK;
        }

        public int UpdateCounter(out uint pCurUpdate)
        {
            pCurUpdate = this.updateCount;
            return VSConstants.S_OK;
        }

        public void Update()
        {
            this.updateCount++;
            this.root.Update();
        }
        #endregion
    }
}
