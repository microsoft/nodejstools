// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI
{
    public class Header : AutomationWrapper
    {
        private Dictionary<string, int> _columns = new Dictionary<string, int>();
        public Dictionary<string, int> Columns
        {
            get
            {
                return _columns;
            }
        }

        public Header(AutomationElement element) : base(element)
        {
            AutomationElementCollection headerItems = FindAllByControlType(ControlType.HeaderItem);
            for (int i = 0; i < headerItems.Count; i++)
            {
                string colName = headerItems[i].GetCurrentPropertyValue(AutomationElement.NameProperty) as string;
                if (colName != null && !_columns.ContainsKey(colName)) _columns[colName] = i;
            }
        }

        public int this[string colName]
        {
            get
            {
                Assert.IsTrue(_columns.ContainsKey(colName), "Header does not define header item {0}", colName);
                return _columns[colName];
            }
        }
    }
}

