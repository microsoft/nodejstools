// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI
{
    public class AttachToProcessDialog : AutomationWrapper
    {
        private ListView _processList;
        private int _hwnd;

        public AttachToProcessDialog(AutomationElement element) : base(element) { _hwnd = element.Current.NativeWindowHandle; }

        public AttachToProcessDialog(IntPtr hwnd) : this(AutomationElement.FromHandle(hwnd)) { _hwnd = (int)hwnd; }

        public SelectCodeTypeDialog SelectCodeTypeForDebugging()
        {
            ThreadPool.QueueUserWorkItem(x =>
            {
                try
                {
                    ClickSelect();
                }
                catch (Exception e)
                {
                    Assert.Fail("Unexpected Exception - ClickSelect(){0}{1}", Environment.NewLine, e.ToString());
                }
            });
            AutomationElement sctel = FindByName("Select Code Type");
            Assert.IsNotNull(sctel, "Could not find the Select Code Type dialog!");
            return new SelectCodeTypeDialog(sctel);
        }

        public void ClickSelect()
        {
            ClickButtonByAutomationId("4103"); // AutomationId discovered with UISpy
        }

        public void ClickAttach()
        {
            ClickButtonByName("Attach"); // AutomationId discovered with UISpy
        }

        public void ClickCancel()
        {
            ClickButtonByName("Cancel");
        }

        public void SelectProcessForDebuggingByPid(int pid)
        {
            Select(_processList.GetFirstByColumnNameAndValue("ID", pid.ToString()).Element);
        }

        public void SelectProcessForDebuggingByName(string name)
        {
            Select(_processList.GetFirstByColumnNameAndValue("Process", name).Element);
        }

        // Available Processes list: AutomationId 4102
        public ListView ProcessList
        {
            get
            {
                if (_processList == null)
                {
                    var plElement = Element.FindFirst(
                        TreeScope.Descendants,
                        new PropertyCondition(
                            AutomationElement.AutomationIdProperty,
                            "4102"));
                    _processList = new ListView(plElement);
                }
                return _processList;
            }
        }
    }
}

