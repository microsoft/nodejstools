// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    internal class Menu : AutomationWrapper
    {
        public Menu(AutomationElement element)
            : base(element)
        {
        }

        public List<MenuItem> Items
        {
            get
            {
                Condition con = new PropertyCondition(
                                    AutomationElement.LocalizedControlTypeProperty,
                                    "menu item"
                                );
                AutomationElementCollection ell = Element.FindAll(TreeScope.Children, con);
                List<MenuItem> items = new List<MenuItem>();
                for (int i = 0; i < ell.Count; i++)
                {
                    items.Add(new MenuItem(ell[i]));
                }
                return items;
            }
        }
    }
}

