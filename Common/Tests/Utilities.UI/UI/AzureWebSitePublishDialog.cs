// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    public class AzureWebSitePublishDialog : AutomationDialog
    {
        public AzureWebSitePublishDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public static AzureWebSitePublishDialog FromDte(VisualStudioApp app)
        {
            var publishDialogHandle = app.OpenDialogWithDteExecuteCommand("Build.PublishSelection");
            return new AzureWebSitePublishDialog(app, AutomationElement.FromHandle(publishDialogHandle));
        }

        public AzureWebSiteImportPublishSettingsDialog ClickImportSettings()
        {
            WaitForInputIdle();
            ClickButtonByAutomationId("ImportSettings");
            return new AzureWebSiteImportPublishSettingsDialog(App, AutomationElement.FromHandle(App.WaitForDialogToReplace(Element)));
        }

        public void ClickPublish()
        {
            WaitForInputIdle();
            WaitForClosed(TimeSpan.FromSeconds(10.0), () => ClickButtonByAutomationId("PublishButton"));
        }
    }
}

