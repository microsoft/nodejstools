// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    public class ToolsOptionsDialog : AutomationDialog
    {
        public ToolsOptionsDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public static ToolsOptionsDialog FromDte(VisualStudioApp app)
        {
            return new ToolsOptionsDialog(
                app,
                AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand("Tools.Options"))
            );
        }

        public override void OK()
        {
            ClickButtonAndClose("1", nameIsAutomationId: true);
        }

        public override void Cancel()
        {
            ClickButtonAndClose("2", nameIsAutomationId: true);
        }

        public string SelectedView
        {
            set
            {
                var treeView = new TreeView(Element.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "SysTreeView32")
                ));

                treeView.FindItem(value.Split('\\', '/')).SetFocus();
            }
        }
    }
}

