// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Navigation
{
    public interface ISimpleObject
    {
        bool CanDelete { get; }
        bool CanGoToSource { get; }
        bool CanRename { get; }
        string Name { get; }
        string UniqueName { get; }
        string FullName { get; }
        string GetTextRepresentation(VSTREETEXTOPTIONS options);
        string TooltipText { get; }
        object BrowseObject { get; }
        CommandID ContextMenuID { get; }
        VSTREEDISPLAYDATA DisplayData { get; }

        uint CategoryField(LIB_CATEGORY lIB_CATEGORY);

        void Delete();
        void DoDragDrop(OleDataObject dataObject, uint grfKeyState, uint pdwEffect);
        void Rename(string pszNewName, uint grfFlags);
        void GotoSource(VSOBJGOTOSRCTYPE SrcType);

        void SourceItems(out IVsHierarchy ppHier, out uint pItemid, out uint pcItems);
        uint EnumClipboardFormats(_VSOBJCFFLAGS _VSOBJCFFLAGS, VSOBJCLIPFORMAT[] rgcfFormats);
        void FillDescription(_VSOBJDESCOPTIONS _VSOBJDESCOPTIONS, IVsObjectBrowserDescription3 pobDesc);

        IVsSimpleObjectList2 FilterView(uint ListType);
    }
}

