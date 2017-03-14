// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Navigation;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project
{
    /// <summary>
    /// Implementation of the service that builds the information to expose to the symbols
    /// navigation tools (class view or object browser) from the Node.js files inside a
    /// hierarchy.
    /// </summary>
    [Guid("1CCB584B-2876-4416-99B0-60C91B938147")]
    internal class NodejsLibraryManager : LibraryManager
    {
        public NodejsLibraryManager(NodejsPackage/*!*/ package)
            : base(package)
        { }

        protected override LibraryNode CreateLibraryNode(LibraryNode parent, IScopeNode subItem, string namePrefix, IVsHierarchy hierarchy, uint itemid)
        {
            return new NodeLibraryNode(parent, subItem, namePrefix, hierarchy, itemid);
        }

        public override LibraryNode CreateFileLibraryNode(LibraryNode parent, HierarchyNode hierarchy, string name, string filename, LibraryNodeType libraryNodeType)
        {
            return new NodeFileLibraryNode(parent, hierarchy, hierarchy.Caption, filename, libraryNodeType);
        }

        protected override void OnNewFile(LibraryTask task)
        {
        }
    }
}

