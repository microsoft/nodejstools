// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    internal class CredentialsDialog : AutomationDialog
    {
        public CredentialsDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public static CredentialsDialog PublishSelection(VisualStudioApp app)
        {
            return new CredentialsDialog(
                app,
                AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand("Build.PublishSelection"))
            );
        }

        public string UserName
        {
            get
            {
                return GetUsernameEditBox().GetValuePattern().Current.Value;
            }
            set
            {
                GetUsernameEditBox().GetValuePattern().SetValue(value);
            }
        }

        public string Password
        {
            get
            {
                return GetPasswordEditBox().GetValuePattern().Current.Value;
            }
            set
            {
                GetPasswordEditBox().GetValuePattern().SetValue(value);
            }
        }

        private AutomationElement GetUsernameEditBox()
        {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "User name:"),
                    new PropertyCondition(AutomationElement.ClassNameProperty, "Edit")
                )
            );
        }

        private AutomationElement GetPasswordEditBox()
        {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "Password:"),
                    new PropertyCondition(AutomationElement.ClassNameProperty, "Edit")
                )
            );
        }
    }
}

