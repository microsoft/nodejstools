// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Input;

namespace TestUtilities.UI
{
    public class SaveDialog : AutomationDialog
    {
        public SaveDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public static SaveDialog FromDte(VisualStudioApp app)
        {
            return new SaveDialog(
                app,
                AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand("File.SaveSelectedItemsAs"))
            );
        }

        public void Save()
        {
            WaitForInputIdle();
            // The Save button on this dialog is broken and so UIA cannot invoke
            // it (though somehow Inspect is able to...). We use the keyboard
            // instead.
            WaitForClosed(DefaultTimeout, () => Keyboard.PressAndRelease(Key.S, Key.LeftAlt));
        }

        public override void OK()
        {
            Save();
        }

        public string FileName
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

        private AutomationElement GetFilenameEditBox()
        {
            return FindByAutomationId("FileNameControlHost");
        }
    }
}

