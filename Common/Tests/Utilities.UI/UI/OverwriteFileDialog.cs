// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI
{
    public class OverwriteFileDialog : AutomationDialog, IOverwriteFile
    {
        private OverwriteFileDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public static OverwriteFileDialog Wait(VisualStudioApp app)
        {
            var hwnd = app.WaitForDialog();
            Assert.AreNotEqual(IntPtr.Zero, hwnd, "Did not find OverwriteFileDialog");
            var element = AutomationElement.FromHandle(hwnd);

            try
            {
                Assert.IsNotNull(element.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "_allItems")
                ), "Not correct dialog - missing '_allItems'");
                Assert.IsNotNull(element.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "_yes")
                ), "Not correct dialog - missing '_yes'");

                var res = new OverwriteFileDialog(app, element);
                element = null;
                return res;
            }
            finally
            {
                if (element != null)
                {
                    AutomationWrapper.DumpElement(element);
                }
            }
        }

        public override void OK()
        {
            ClickButtonAndClose("_yes", nameIsAutomationId: true);
        }

        public void No()
        {
            ClickButtonAndClose("_no", nameIsAutomationId: true);
        }

        public void Yes()
        {
            OK();
        }

        public override void Cancel()
        {
            ClickButtonAndClose("_cancel", nameIsAutomationId: true);
        }


        public bool AllItems
        {
            get
            {
                return FindByAutomationId("_allItems").GetTogglePattern().Current.ToggleState == ToggleState.On;
            }
            set
            {
                if (AllItems)
                {
                    if (!value)
                    {
                        FindByAutomationId("_allItems").GetTogglePattern().Toggle();
                    }
                }
                else
                {
                    if (value)
                    {
                        FindByAutomationId("_allItems").GetTogglePattern().Toggle();
                    }
                }
            }
        }


        public string Text
        {
            get
            {
                return FindByAutomationId("_message").GetValuePattern().Current.Value;
            }
        }
    }
}

