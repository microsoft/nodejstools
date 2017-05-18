// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudioTools.Parsing;

namespace Microsoft.VisualStudioTools.Navigation
{
    internal interface IScopeNode
    {
        LibraryNodeType NodeType
        {
            get;
        }

        string Name
        {
            get;
        }

        string Description
        {
            get;
        }

        SourceLocation Start
        {
            get;
        }
        SourceLocation End
        {
            get;
        }

        IEnumerable<IScopeNode> NestedScopes
        {
            get;
        }
    }
}
