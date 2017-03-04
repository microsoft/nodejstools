// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI
{
    public class ChooseLocationDialog : AutomationDialog
    {
        public ChooseLocationDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public static ChooseLocationDialog FromDte(VisualStudioApp app)
        {
            return new ChooseLocationDialog(
                app,
                AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand("File.ProjectPickerMoveInto"))
            );
        }

        public void SelectProject(string name)
        {
            var item = Element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, name));
            Assert.IsNotNull(item, "Did not find item " + name);
            item.GetSelectionItemPattern().Select();
        }
    }
}

