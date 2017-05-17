// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Navigation;

namespace Microsoft.NodejsTools.Project
{
    internal class NodeLibraryNode : CommonLibraryNode
    {
        public NodeLibraryNode(LibraryNode parent, IScopeNode scope, string namePrefix, IVsHierarchy hierarchy, uint itemId) :
            base(parent, scope, namePrefix, hierarchy, itemId)
        {
        }
    }
}

