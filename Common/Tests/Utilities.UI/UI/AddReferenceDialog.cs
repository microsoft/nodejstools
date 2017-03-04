// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    /// <summary>
    /// Wrapps VS's Add Reference Dialog
    /// </summary>
    internal class AddReferenceDialog : AutomationWrapper
    {
        public AddReferenceDialog(AutomationElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Clicks the OK button on the dialog.
        /// </summary>
        public void ClickOK()
        {
            ClickButtonByName("OK");
        }

        public void ActivateBrowseTab()
        {
            for (int i = 0; i < 20; i++)
            {
                var tabItem = Element.FindFirst(
                    TreeScope.Descendants,
                    new AndCondition(
                        new PropertyCondition(
                            AutomationElement.ControlTypeProperty,
                            ControlType.TabItem
                        ),
                        new PropertyCondition(
                            AutomationElement.NameProperty,
                            "Browse"
                        )
                    )
                );
                if (tabItem == null)
                {
                    System.Threading.Thread.Sleep(500);
                    continue;
                }

                var pattern = (SelectionItemPattern)tabItem.GetCurrentPattern(SelectionItemPattern.Pattern);
                pattern.Select();
            }
        }

        public string BrowseFilename
        {
            get
            {
                return GetFilenameValuePattern().Current.Value;
            }
            set
            {
                GetFilenameValuePattern().SetValue(value);
            }
        }

        private ValuePattern GetFilenameValuePattern()
        {
            var filename = Element.FindFirst(
                TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(
                        AutomationElement.ControlTypeProperty,
                        ControlType.Edit
                    ),
                    new PropertyCondition(
                        AutomationElement.NameProperty,
                        "File name:"
                    )
                )
            );

            return (ValuePattern)filename.GetCurrentPattern(ValuePattern.Pattern);
        }
    }
}

