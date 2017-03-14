// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    public class AzureManageSubscriptionsDialog : AutomationDialog
    {
        public AzureManageSubscriptionsDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public void ClickCertificates()
        {
            WaitForInputIdle();
            CertificatesTab().Select();
        }

        public AzureImportSubscriptionDialog ClickImport()
        {
            WaitForInputIdle();
            ClickButtonByAutomationId("ImportButton");

            return new AzureImportSubscriptionDialog(App, AutomationElement.FromHandle(App.WaitForDialogToReplace(Element)));
        }

        public void ClickRemove()
        {
            WaitForInputIdle();
            var button = new Button(FindByAutomationId("DeleteButton"));
            WaitFor(button, btn => btn.Element.Current.IsEnabled);
            button.Click();
        }

        public void Close()
        {
            WaitForInputIdle();
            WaitForClosed(TimeSpan.FromSeconds(10.0), () => ClickButtonByName("Close"));
        }

        public ListBox SubscriptionsListBox
        {
            get
            {
                return new ListBox(FindByAutomationId("SubscriptionsListBox"));
            }
        }

        private AutomationElement CertificatesTab()
        {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "CertificatesTab"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem)
                )
            );
        }
    }
}

