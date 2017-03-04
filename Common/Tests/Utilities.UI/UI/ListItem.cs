// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI
{
    public class ListItem : AutomationWrapper
    {
        private ListView _parent;
        private AutomationElementCollection _columns;
        public ListItem(AutomationElement element, ListView parent) : base(element)
        {
            _parent = parent;
            _columns = FindAllByControlType(ControlType.Text);
        }

        public string this[int index]
        {
            get
            {
                Assert.IsNotNull(_columns);
                Assert.IsTrue(0 <= index && index < _columns.Count, "Index {0} is out of range of column count {1}", index, _columns.Count);
                return _columns[index].GetCurrentPropertyValue(AutomationElement.NameProperty) as string;
            }
        }

        public string this[string columnName]
        {
            get
            {
                Assert.IsNotNull(_parent.Header, "Parent List does not define column headers!");
                return this[_parent.Header[columnName]];
            }
        }
    }
}

