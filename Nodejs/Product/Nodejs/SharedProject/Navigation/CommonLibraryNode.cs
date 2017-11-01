// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools.Parsing;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.VisualStudioTools.Navigation
{
    /// <summary>
    /// This is a specialized version of the LibraryNode that handles the dynamic languages
    /// items. The main difference from the generic one is that it supports navigation
    /// to the location inside the source code where the element is defined.
    /// </summary>
    internal abstract class CommonLibraryNode : LibraryNode
    {
        private readonly IVsHierarchy _ownerHierarchy;
        private readonly uint _fileId;
        private readonly TextSpan _sourceSpan;
        private readonly IScopeNode _scope;
        private string _fileMoniker;

        protected CommonLibraryNode(LibraryNode parent, IScopeNode scope, string namePrefix, IVsHierarchy hierarchy, uint itemId) :
            base(parent, GetLibraryNodeName(scope, namePrefix), namePrefix + scope.Name, scope.NodeType)
        {
            this._ownerHierarchy = hierarchy;
            this._fileId = itemId;

            // Now check if we have all the information to navigate to the source location.
            if ((null != this._ownerHierarchy) && (VSConstants.VSITEMID_NIL != this._fileId))
            {
                if ((SourceLocation.Invalid != scope.Start) && (SourceLocation.Invalid != scope.End))
                {
                    this._sourceSpan = new TextSpan();
                    this._sourceSpan.iStartIndex = scope.Start.Column - 1;
                    if (scope.Start.Line > 0)
                    {
                        this._sourceSpan.iStartLine = scope.Start.Line - 1;
                    }
                    this._sourceSpan.iEndIndex = scope.End.Column;
                    if (scope.End.Line > 0)
                    {
                        this._sourceSpan.iEndLine = scope.End.Line - 1;
                    }
                    this.CanGoToSource = true;
                }
            }
            this._scope = scope;
        }

        internal IScopeNode ScopeNode => this._scope;

        public TextSpan SourceSpan => this._sourceSpan;

        private static string GetLibraryNodeName(IScopeNode node, string namePrefix)
        {
            namePrefix = namePrefix.Substring(namePrefix.LastIndexOf(':') + 1); // remove filename prefix
            return node.NodeType == LibraryNodeType.Members ? node.Name : string.Format(CultureInfo.InvariantCulture, "{0}{1}", namePrefix, node.Name);
        }

        protected CommonLibraryNode(CommonLibraryNode node) :
            base(node)
        {
            this._fileId = node._fileId;
            this._ownerHierarchy = node._ownerHierarchy;
            this._fileMoniker = node._fileMoniker;
            this._sourceSpan = node._sourceSpan;
        }

        protected CommonLibraryNode(CommonLibraryNode node, string newFullName) :
            base(node, newFullName)
        {
            this._scope = node._scope;
            this._fileId = node._fileId;
            this._ownerHierarchy = node._ownerHierarchy;
            this._fileMoniker = node._fileMoniker;
            this._sourceSpan = node._sourceSpan;
        }

        public override uint CategoryField(LIB_CATEGORY category)
        {
            switch (category)
            {
                case (LIB_CATEGORY)_LIB_CATEGORY2.LC_MEMBERINHERITANCE:
                    if (this.NodeType == LibraryNodeType.Members || this.NodeType == LibraryNodeType.Definitions)
                    {
                        return (uint)_LIBCAT_MEMBERINHERITANCE.LCMI_IMMEDIATE;
                    }
                    break;
            }
            return base.CategoryField(category);
        }

        public override void GotoSource(VSOBJGOTOSRCTYPE gotoType)
        {
            // We do not support the "Goto Reference"
            if (VSOBJGOTOSRCTYPE.GS_REFERENCE == gotoType)
            {
                return;
            }

            // There is no difference between definition and declaration, so here we
            // don't check for the other flags.

            IVsWindowFrame frame = null;
            var documentData = FindDocDataFromRDT();
            try
            {
                // Now we can try to open the editor. We assume that the owner hierarchy is
                // a project and we want to use its OpenItem method.
                var project = this._ownerHierarchy as IVsProject3;
                if (null == project)
                {
                    return;
                }
                var viewGuid = VSConstants.LOGVIEWID_Code;
                ErrorHandler.ThrowOnFailure(project.OpenItem(this._fileId, ref viewGuid, documentData, out frame));
            }
            finally
            {
                if (IntPtr.Zero != documentData)
                {
                    Marshal.Release(documentData);
                    documentData = IntPtr.Zero;
                }
            }

            // Make sure that the document window is visible.
            ErrorHandler.ThrowOnFailure(frame.Show());

            // Get the code window from the window frame.
            ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var docView));
            var codeWindow = docView as IVsCodeWindow;
            if (null == codeWindow)
            {
                ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out var docData));
                codeWindow = docData as IVsCodeWindow;
                if (null == codeWindow)
                {
                    return;
                }
            }

            // Get the primary view from the code window.
            ErrorHandler.ThrowOnFailure(codeWindow.GetPrimaryView(out var textView));

            // Set the cursor at the beginning of the declaration.
            ErrorHandler.ThrowOnFailure(textView.SetCaretPos(this._sourceSpan.iStartLine, this._sourceSpan.iStartIndex));
            // Make sure that the text is visible.
            var visibleSpan = new TextSpan();
            visibleSpan.iStartLine = this._sourceSpan.iStartLine;
            visibleSpan.iStartIndex = this._sourceSpan.iStartIndex;
            visibleSpan.iEndLine = this._sourceSpan.iStartLine;
            visibleSpan.iEndIndex = this._sourceSpan.iStartIndex + 1;
            ErrorHandler.ThrowOnFailure(textView.EnsureSpanVisible(visibleSpan));
        }

        public override void SourceItems(out IVsHierarchy hierarchy, out uint itemId, out uint itemsCount)
        {
            hierarchy = this._ownerHierarchy;
            itemId = this._fileId;
            itemsCount = 1;
        }

        public override string UniqueName
        {
            get
            {
                if (string.IsNullOrEmpty(this._fileMoniker))
                {
                    ErrorHandler.ThrowOnFailure(this._ownerHierarchy.GetCanonicalName(this._fileId, out this._fileMoniker));
                }
                return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", this._fileMoniker, this.Name);
            }
        }

        private IntPtr FindDocDataFromRDT()
        {
            // Get a reference to the RDT.
            var rdt = Package.GetGlobalService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            if (null == rdt)
            {
                return IntPtr.Zero;
            }

            // Get the enumeration of the running documents.
            ErrorHandler.ThrowOnFailure(rdt.GetRunningDocumentsEnum(out var documents));

            var documentData = IntPtr.Zero;
            var docCookie = new uint[1];
            while ((VSConstants.S_OK == documents.Next(1, docCookie, out var fetched)) && (1 == fetched))
            {
                var docData = IntPtr.Zero;
                try
                {
                    ErrorHandler.ThrowOnFailure(
                        rdt.GetDocumentInfo(docCookie[0], out var flags, out var readLocks, out var editLocks, out var moniker, out var docHierarchy, out var docId, out docData));
                    // Check if this document is the one we are looking for.
                    if ((docId == this._fileId) && (this._ownerHierarchy.Equals(docHierarchy)))
                    {
                        documentData = docData;
                        docData = IntPtr.Zero;
                        break;
                    }
                }
                finally
                {
                    if (IntPtr.Zero != docData)
                    {
                        Marshal.Release(docData);
                    }
                }
            }

            return documentData;
        }
    }
}
