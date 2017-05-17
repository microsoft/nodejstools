// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    [Export(typeof(IVsHierarchyItemManager))]
    public class MockVsHierarchyItemManager : IVsHierarchyItemManager
    {
        public event EventHandler<HierarchyItemEventArgs> AfterInvalidateItems { add { } remove { } }

        public IVsHierarchyItem GetHierarchyItem(IVsHierarchy hierarchy, uint itemid)
        {
            throw new NotImplementedException();
        }

        public bool IsChangingItems
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<HierarchyItemEventArgs> OnItemAdded { add { } remove { } }

        public bool TryGetHierarchyItem(IVsHierarchy hierarchy, uint itemid, out IVsHierarchyItem item)
        {
            item = null;
            return false;
        }

        public bool TryGetHierarchyItemIdentity(IVsHierarchy hierarchy, uint itemid, out IVsHierarchyItemIdentity identity)
        {
            throw new NotImplementedException();
        }
    }
}

