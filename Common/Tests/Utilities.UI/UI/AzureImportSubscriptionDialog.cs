// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    public class AzureImportSubscriptionDialog : AutomationDialog
    {
        public AzureImportSubscriptionDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public void ClickImport()
        {
            WaitForInputIdle();
            WaitForClosed(TimeSpan.FromSeconds(10.0), () => ClickButtonByAutomationId("OKButton"));
        }

        public string FileName
        {
            get
            {
                return GetFileNameBox().GetValuePattern().Current.Value;
            }
            set
            {
                GetFileNameBox().GetValuePattern().SetValue(value);
            }
        }

        private AutomationElement GetFileNameBox()
        {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "PublishSettingsFileTextBox"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)
                )
            );
        }
    }
}

