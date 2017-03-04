// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudioTools.Project;

namespace Microsoft.VisualStudioTools.Navigation
{
    internal class NodeFileLibraryNode : LibraryNode
    {
        public NodeFileLibraryNode(LibraryNode parent, HierarchyNode hierarchy, string name, string filename, LibraryNodeType libraryNodeType)
            : base(parent, name, filename, libraryNodeType)
        {
        }
    }
}

