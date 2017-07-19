// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Represents various boolean states for the HiearchyNode
    /// </summary>
    [Flags]
    internal enum HierarchyNodeFlags
    {
        None,
        ExcludeFromScc = 0x01,
        IsExpanded = 0x02,
        HasParentNodeNameRelation = 0x04,
        IsVisible = 0x08
    }
}
