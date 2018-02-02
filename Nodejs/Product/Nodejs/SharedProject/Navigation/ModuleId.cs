// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Navigation
{
    /// <summary>
    /// Class used to identify a module. The module is identified using the hierarchy that
    /// contains it and its item id inside the hierarchy.
    /// </summary>
    public sealed class ModuleId
    {
        public ModuleId(IVsHierarchy owner, uint id)
        {
            this.Hierarchy = owner;
            this.ItemId = id;
        }

        public IVsHierarchy Hierarchy { get; }
        public uint ItemId { get; }
        public override int GetHashCode()
        {
            var hash = 0;
            if (null != this.Hierarchy)
            {
                hash = this.Hierarchy.GetHashCode();
            }
            hash = hash ^ (int)this.ItemId;
            return hash;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ModuleId;
            if (null == other)
            {
                return false;
            }
            if (!this.Hierarchy.Equals(other.Hierarchy))
            {
                return false;
            }
            return (this.ItemId == other.ItemId);
        }
    }
}
