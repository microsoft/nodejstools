// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;

namespace TestUtilities.UI.Nodejs
{
    public class ComparePerfReports : AutomationWrapper
    {
        public ComparePerfReports(IntPtr hwnd)
            : base(AutomationElement.FromHandle(hwnd))
        {
        }

        public void Ok()
        {
            ClickButtonByName("OK");
        }

        public void Cancel()
        {
            ClickButtonByName("Cancel");
        }

        public string ComparisonFile
        {
            get
            {
                return ComparisonFileTextBox.GetValue();
            }
            set
            {
                ComparisonFileTextBox.SetValue(value);
            }
        }

        private AutomationWrapper ComparisonFileTextBox
        {
            get
            {
                return new AutomationWrapper(FindByAutomationId("ComparisonFile"));
            }
        }

        public string BaselineFile
        {
            get
            {
                return BaselineFileTextBox.GetValue();
            }
            set
            {
                BaselineFileTextBox.SetValue(value);
            }
        }

        private AutomationWrapper BaselineFileTextBox
        {
            get
            {
                return new AutomationWrapper(FindByAutomationId("BaselineFile"));
            }
        }
    }
}

