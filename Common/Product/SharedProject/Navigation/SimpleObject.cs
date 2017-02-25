/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Navigation
{
    internal class SimpleObject : ISimpleObject
    {
        #region ISimpleObject Members

        public virtual bool CanDelete => false;
        public virtual bool CanGoToSource => false;
        public virtual bool CanRename => false;
        public virtual string Name => String.Empty;
        public virtual string UniqueName => String.Empty;
        public virtual string FullName => this.Name;

        public virtual string GetTextRepresentation(VSTREETEXTOPTIONS options)
        {
            return this.Name;
        }

        public virtual string TooltipText => String.Empty;
        public virtual object BrowseObject => null;
        public virtual System.ComponentModel.Design.CommandID ContextMenuID => null;
        public virtual VSTREEDISPLAYDATA DisplayData => new VSTREEDISPLAYDATA();
        public virtual uint CategoryField(LIB_CATEGORY lIB_CATEGORY)
        {
            return 0;
        }

        public virtual void Delete()
        {
        }

        public virtual void DoDragDrop(OleDataObject dataObject, uint grfKeyState, uint pdwEffect)
        {
        }

        public virtual void Rename(string pszNewName, uint grfFlags)
        {
        }

        public virtual void GotoSource(VSOBJGOTOSRCTYPE SrcType)
        {
        }

        public virtual void SourceItems(out IVsHierarchy ppHier, out uint pItemid, out uint pcItems)
        {
            ppHier = null;
            pItemid = 0;
            pcItems = 0;
        }

        public virtual uint EnumClipboardFormats(_VSOBJCFFLAGS _VSOBJCFFLAGS, VSOBJCLIPFORMAT[] rgcfFormats)
        {
            return 0;
        }

        public virtual void FillDescription(_VSOBJDESCOPTIONS _VSOBJDESCOPTIONS, IVsObjectBrowserDescription3 pobDesc)
        {
        }

        public virtual IVsSimpleObjectList2 FilterView(uint ListType)
        {
            return null;
        }

        #endregion
    }
}
