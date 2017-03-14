// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI
{
    public class CheckListView : AutomationWrapper
    {
        private List<CheckBox> _items;
        private Header _header;

        public Header Header
        {
            get
            {
                if (_header == null)
                {
                    var headerel = FindFirstByControlType(ControlType.Header);
                    if (headerel != null)
                        _header = new Header(FindFirstByControlType(ControlType.Header));
                }
                return _header;
            }
        }

        public List<CheckBox> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new List<CheckBox>();
                    AutomationElementCollection rawItems = FindAllByControlType(ControlType.CheckBox);
                    foreach (AutomationElement el in rawItems)
                    {
                        _items.Add(new CheckBox(el, this));
                    }
                }
                return _items;
            }
        }

        public CheckListView(AutomationElement element) : base(element) { }

        public CheckBox GetFirstByName(string name)
        {
            foreach (CheckBox r in Items)
            {
                if (r.Name.Equals(name, StringComparison.CurrentCulture)) return r;
            }
            Assert.Fail("No item found with Name == {0}", name);
            return null;
        }
    }
}

