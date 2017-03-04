// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    public class AzureWebSiteImportPublishSettingsDialog : AutomationDialog
    {
        public AzureWebSiteImportPublishSettingsDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public void ClickImportFromWindowsAzureWebSite()
        {
            WaitForInputIdle();
            ImportFromWindowsAzureWebSiteRadioButton().Select();
        }

        public void ClickSignOut()
        {
            WaitForInputIdle();
            var sign = new AutomationWrapper(FindByAutomationId("AzureSigninControl"));
            sign.ClickButtonByName("Sign Out");
        }

        public AzureManageSubscriptionsDialog ClickImportOrManageSubscriptions()
        {
            WaitForInputIdle();
            var importElement = ImportSubscriptionsHyperlink();
            if (importElement == null)
            {
                importElement = ManageSubscriptionsHyperlink();
            }
            importElement.GetInvokePattern().Invoke();
            return new AzureManageSubscriptionsDialog(App, AutomationElement.FromHandle(App.WaitForDialogToReplace(Element)));
        }

        public AzureWebSiteCreateDialog ClickNew()
        {
            WaitForInputIdle();
            ClickButtonByAutomationId("NewButton");
            return new AzureWebSiteCreateDialog(App, AutomationElement.FromHandle(App.WaitForDialogToReplace(Element)));
        }

        public void ClickOK()
        {
            WaitForInputIdle();
            WaitForClosed(TimeSpan.FromSeconds(10.0), () => ClickButtonByAutomationId("OKButton"));
        }

        private AutomationElement ImportFromWindowsAzureWebSiteRadioButton()
        {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "ImportLabel"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.RadioButton)
                )
            );
        }

        private AutomationElement ImportSubscriptionsHyperlink()
        {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "Import subscriptions"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Hyperlink)
                )
            );
        }

        private AutomationElement ManageSubscriptionsHyperlink()
        {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "Manage subscriptions"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Hyperlink)
                )
            );
        }
    }
}

