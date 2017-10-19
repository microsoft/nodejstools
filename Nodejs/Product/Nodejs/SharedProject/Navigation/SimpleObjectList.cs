// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.VisualStudioTools.Navigation
{
    /// <summary>
    /// Represents a simple list which the VS UI can query for items.
    /// 
    /// VS assumes that these lists do not change once VS has gotten ahold of them.  Therefore if the
    /// list is changing over time it should be thrown away and a new list should be placed in the parent.
    /// </summary>
    internal class SimpleObjectList<T> : IVsSimpleObjectList2 where T : ISimpleObject
    {
        private readonly List<T> _children;
        private uint _updateCount;

        public SimpleObjectList()
        {
            this._children = new List<T>();
        }

        public List<T> Children => this._children;

        public virtual void Update()
        {
            this._updateCount++;
        }

        public uint UpdateCount
        {
            get { return this._updateCount; }
            set { this._updateCount = value; }
        }

        public const uint NullIndex = (uint)0xFFFFFFFF;

        int IVsSimpleObjectList2.CanDelete(uint index, out int pfOK)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            pfOK = this._children[(int)index].CanDelete ? 1 : 0;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.CanGoToSource(uint index, VSOBJGOTOSRCTYPE SrcType, out int pfOK)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            pfOK = this._children[(int)index].CanGoToSource ? 1 : 0;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.CanRename(uint index, string pszNewName, out int pfOK)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            pfOK = this._children[(int)index].CanRename ? 1 : 0;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.CountSourceItems(uint index, out IVsHierarchy ppHier, out uint pItemid, out uint pcItems)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this._children[(int)index].SourceItems(out ppHier, out pItemid, out pcItems);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.DoDelete(uint index, uint grfFlags)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this._children[(int)index].Delete();
            this._children.RemoveAt((int)index);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.DoDragDrop(uint index, IDataObject pDataObject, uint grfKeyState, ref uint pdwEffect)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            var dataObject = new OleDataObject(pDataObject);
            this._children[(int)index].DoDragDrop(dataObject, grfKeyState, pdwEffect);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.DoRename(uint index, string pszNewName, uint grfFlags)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this._children[(int)index].Rename(pszNewName, grfFlags);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.EnumClipboardFormats(uint index, uint grfFlags, uint celt, VSOBJCLIPFORMAT[] rgcfFormats, uint[] pcActual)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            var copied = this._children[(int)index].EnumClipboardFormats((_VSOBJCFFLAGS)grfFlags, rgcfFormats);
            if ((null != pcActual) && (pcActual.Length > 0))
            {
                pcActual[0] = copied;
            }
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.FillDescription2(uint index, uint grfOptions, IVsObjectBrowserDescription3 pobDesc)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this._children[(int)index].FillDescription((_VSOBJDESCOPTIONS)grfOptions, pobDesc);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetBrowseObject(uint index, out object ppdispBrowseObj)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            ppdispBrowseObj = this._children[(int)index].BrowseObject;
            if (null == ppdispBrowseObj)
            {
                return VSConstants.E_NOTIMPL;
            }
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetCapabilities2(out uint pgrfCapabilities)
        {
            pgrfCapabilities = (uint)this.Capabilities;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetCategoryField2(uint index, int Category, out uint pfCatField)
        {
            if (NullIndex == index)
            {
                pfCatField = CategoryField((LIB_CATEGORY)Category);
            }
            else if (index < (uint)this._children.Count)
            {
                pfCatField = this._children[(int)index].CategoryField((LIB_CATEGORY)Category);
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetClipboardFormat(uint index, uint grfFlags, FORMATETC[] pFormatetc, STGMEDIUM[] pMedium)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetContextMenu(uint index, out Guid pclsidActive, out int pnMenuId, out IOleCommandTarget ppCmdTrgtActive)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            var commandId = this._children[(int)index].ContextMenuID;
            if (null == commandId)
            {
                pclsidActive = Guid.Empty;
                pnMenuId = 0;
                ppCmdTrgtActive = null;
                return VSConstants.E_NOTIMPL;
            }
            pclsidActive = commandId.Guid;
            pnMenuId = commandId.ID;
            ppCmdTrgtActive = this._children[(int)index] as IOleCommandTarget;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetDisplayData(uint index, VSTREEDISPLAYDATA[] pData)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            pData[0] = this._children[(int)index].DisplayData;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetExpandable3(uint index, uint ListTypeExcluded, out int pfExpandable)
        {
            // There is a not empty implementation of GetCategoryField2, so this method should
            // return E_NOTIMPL.
            pfExpandable = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetExtendedClipboardVariant(uint index, uint grfFlags, VSOBJCLIPFORMAT[] pcfFormat, out object pvarFormat)
        {
            pvarFormat = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetFlags(out uint pFlags)
        {
            pFlags = (uint)this.Flags;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetItemCount(out uint pCount)
        {
            pCount = (uint)this._children.Count;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetList2(uint index, uint ListType, uint flags, VSOBSEARCHCRITERIA2[] pobSrch, out IVsSimpleObjectList2 ppIVsSimpleObjectList2)
        {
            // TODO: Use the flags and list type to actually filter the result.
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            ppIVsSimpleObjectList2 = this._children[(int)index].FilterView(ListType);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetMultipleSourceItems(uint index, uint grfGSI, uint cItems, VSITEMSELECTION[] rgItemSel)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetNavInfo(uint index, out IVsNavInfo ppNavInfo)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            ppNavInfo = this._children[(int)index] as IVsNavInfo;
            return ppNavInfo == null ? VSConstants.E_NOTIMPL : VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetNavInfoNode(uint index, out IVsNavInfoNode ppNavInfoNode)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            ppNavInfoNode = this._children[(int)index] as IVsNavInfoNode;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetProperty(uint index, int propid, out object pvar)
        {
            if (propid == (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FULLNAME)
            {
                pvar = this._children[(int)index].FullName;
                return VSConstants.S_OK;
            }

            pvar = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetSourceContextWithOwnership(uint index, out string pbstrFilename, out uint pulLineNum)
        {
            pbstrFilename = null;
            pulLineNum = (uint)0;
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetTextWithOwnership(uint index, VSTREETEXTOPTIONS tto, out string pbstrText)
        {
            // TODO: make use of the text option.
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            pbstrText = this._children[(int)index].GetTextRepresentation(tto);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetTipTextWithOwnership(uint index, VSTREETOOLTIPTYPE eTipType, out string pbstrText)
        {
            // TODO: Make use of the tooltip type.
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            pbstrText = this._children[(int)index].TooltipText;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetUserContext(uint index, out object ppunkUserCtx)
        {
            ppunkUserCtx = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GoToSource(uint index, VSOBJGOTOSRCTYPE SrcType)
        {
            if (index >= (uint)this._children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this._children[(int)index].GotoSource(SrcType);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.LocateNavInfoNode(IVsNavInfoNode pNavInfoNode, out uint pulIndex)
        {
            Utilities.ArgumentNotNull("pNavInfoNode", pNavInfoNode);

            pulIndex = NullIndex;
            ErrorHandler.ThrowOnFailure(pNavInfoNode.get_Name(out var nodeName));
            for (var i = 0; i < this._children.Count; i++)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(this._children[i].UniqueName, nodeName))
                {
                    pulIndex = (uint)i;
                    return VSConstants.S_OK;
                }
            }
            return VSConstants.S_FALSE;
        }

        int IVsSimpleObjectList2.OnClose(VSTREECLOSEACTIONS[] ptca)
        {
            // Do Nothing.
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.QueryDragDrop(uint index, IDataObject pDataObject, uint grfKeyState, ref uint pdwEffect)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.ShowHelp(uint index)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.UpdateCounter(out uint pCurUpdate)
        {
            pCurUpdate = this._updateCount;
            return VSConstants.S_OK;
        }

        public virtual uint Capabilities => 0;
        public virtual _VSTREEFLAGS Flags => 0;
        public virtual uint CategoryField(LIB_CATEGORY lIB_CATEGORY)
        {
            return 0;
        }
    }
}
