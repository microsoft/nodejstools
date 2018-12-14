// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Diagnostics
{
    internal class HierarchyItem
    {
        public uint ItemId { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
    }
}
