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
        private IVsHierarchy _ownerHierarchy;
        private uint _itemId;

        public ModuleId(IVsHierarchy owner, uint id)
        {
            this._ownerHierarchy = owner;
            this._itemId = id;
        }

        public IVsHierarchy Hierarchy => this._ownerHierarchy;
        public uint ItemID => this._itemId;
        public override int GetHashCode()
        {
            var hash = 0;
            if (null != this._ownerHierarchy)
            {
                hash = this._ownerHierarchy.GetHashCode();
            }
            hash = hash ^ (int)this._itemId;
            return hash;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ModuleId;
            if (null == other)
            {
                return false;
            }
            if (!this._ownerHierarchy.Equals(other._ownerHierarchy))
            {
                return false;
            }
            return (this._itemId == other._itemId);
        }
    }
}

