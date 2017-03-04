// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI
{
    public class SelectFolderDialog : AutomationDialog
    {
        public SelectFolderDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public static SelectFolderDialog AddExistingFolder(VisualStudioApp app)
        {
            return new SelectFolderDialog(
                app,
                AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand("Project.AddExistingFolder"))
            );
        }

        public static SelectFolderDialog AddFolderToSearchPath(VisualStudioApp app)
        {
            return new SelectFolderDialog(
                app,
                AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand("Project.AddSearchPathFolder"))
            );
        }

        public void SelectFolder()
        {
            ClickButtonByName("Select Folder");
        }

        public string FolderName
        {
            get
            {
                return GetFilenameEditBox().GetValuePattern().Current.Value;
            }
            set
            {
                GetFilenameEditBox().GetValuePattern().SetValue(value);
            }
        }

        public string Address
        {
            get
            {
                foreach (AutomationElement e in Element.FindAll(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "ToolbarWindow32"))
                )
                {
                    var name = e.Current.Name;
                    if (name.StartsWith("Address: ", StringComparison.CurrentCulture))
                    {
                        return name.Substring("Address: ".Length);
                    }
                }

                Assert.Fail("Unable to find address");
                return null;
            }
        }

        private AutomationElement GetFilenameEditBox()
        {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.ClassNameProperty, "Edit"),
                    new PropertyCondition(AutomationElement.NameProperty, "Folder:")
                )
            );
        }
    }
}

