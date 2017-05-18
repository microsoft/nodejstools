// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI
{
    public class ListBox : AutomationWrapper
    {
        public ListBox(AutomationElement element)
            : base(element)
        {
        }

        public ListBoxItem this[int index]
        {
            get
            {
                var items = FindAllByControlType(ControlType.ListItem);
                Assert.IsTrue(0 <= index && index < items.Count, "Index {0} is out of range of item count {1}", index, items.Count);
                return new ListBoxItem(items[index], this);
            }
        }

        public int Count
        {
            get
            {
                var items = FindAllByControlType(ControlType.ListItem);
                return items.Count;
            }
        }
    }
}

