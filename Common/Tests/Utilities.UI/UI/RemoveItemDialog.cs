// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    /// <summary>
    /// Wraps the Delete/Remove/Cancel dialog displayed when removing something from a hierarchy window (such as the solution explorer).
    /// </summary>
    public class RemoveItemDialog : AutomationDialog
    {
        public RemoveItemDialog(IntPtr hwnd)
            : base(null, AutomationElement.FromHandle(hwnd))
        {
        }

        public RemoveItemDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public static RemoveItemDialog FromDte(VisualStudioApp app)
        {
            return new RemoveItemDialog(app, AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand("Edit.Delete")));
        }

        public override void OK()
        {
            throw new NotSupportedException();
        }

        public void Remove()
        {
            WaitForInputIdle();
            WaitForClosed(DefaultTimeout, () => ClickButtonByName("Remove"));
        }

        public void Delete()
        {
            WaitForInputIdle();
            WaitForClosed(DefaultTimeout, () => ClickButtonByName("Delete"));
        }
    }
}

