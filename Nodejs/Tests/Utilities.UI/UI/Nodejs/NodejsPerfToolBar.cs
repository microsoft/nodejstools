// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows.Automation;

namespace TestUtilities.UI.Nodejs
{
    public class NodejsPerfToolBar : AutomationWrapper
    {
        public NodejsPerfToolBar(AutomationElement element)
            : base(element)
        {
        }

        public void NewPerfSession()
        {
            ClickButtonByName("Add Performance Session");
        }

        public void StartProfiling()
        {
            ClickButtonByName("Start Profiling");
        }

        public void StopProfiling()
        {
            var button = FindByName("Stop Profiling");
            for (int i = 0; i < 20 && !button.Current.IsEnabled; i++)
            {
                System.Threading.Thread.Sleep(500);
            }

            Invoke(button);
        }
    }
}

