// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    public class ComboBox : AutomationWrapper
    {
        public ComboBox(AutomationElement element)
            : base(element)
        {
        }

        public void SelectItem(string name)
        {
            ExpandCollapsePattern pat = (ExpandCollapsePattern)Element.GetCurrentPattern(ExpandCollapsePattern.Pattern);
            pat.Expand();
            try
            {
                var item = Element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, name));

                if (item == null)
                {
                    throw new ElementNotAvailableException(name + " is not in the combobox");
                }
                ((SelectionItemPattern)item.GetCurrentPattern(SelectionItemPattern.Pattern)).Select();
            }
            finally
            {
                pat.Collapse();
            }
        }

        /// <summary>
        /// Selects an item in the combo box by clicking on it.
        /// Only use this if SelectItem doesn't work!
        /// </summary
        /// <param name="name"></param>
        public void ClickItem(string name)
        {
            ExpandCollapsePattern pat = (ExpandCollapsePattern)Element.GetCurrentPattern(ExpandCollapsePattern.Pattern);
            pat.Expand();

            var item = Element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, name));
            if (item == null)
            {
                throw new ElementNotAvailableException(name + " is not in the combobox");
            }

            // On Win8, we need to move mouse onto the text, otherwise we cannot select the item 
            AutomationElement innerText = item.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));
            Mouse.MoveTo(innerText.GetClickablePoint());
            Mouse.Click();
        }

        public string GetSelectedItemName()
        {
            var selection = Element.GetSelectionPattern().Current.GetSelection();
            if (selection == null || selection.Length == 0)
            {
                return null;
            }
            return selection[0].Current.Name;
        }

        public string GetEnteredText()
        {
            return GetValue();
        }
    }
}

