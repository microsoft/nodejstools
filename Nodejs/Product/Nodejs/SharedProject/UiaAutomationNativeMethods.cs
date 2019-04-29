// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.InteropServices;
using System.Windows.Automation.Provider;

namespace Microsoft.NodejsTools.SharedProject
{
    internal class UiaAutomationNativeMethods
    {
        public enum AutomationNotificationKind
        {
            ItemAdded = 0,
            ItemRemoved = 1,
            ActionCompleted = 2,
            ActionAborted = 3,
            Other = 4
        }

        public enum AutomationNotificationProcessing
        {
            ImportantAll = 0,
            ImportantMostRecent = 1,
            All = 2,
            MostRecent = 3,
            CurrentThenMostRecent = 4
        }

        [DllImport("UIAutomationCore.dll", CharSet = CharSet.Unicode)]
        public static extern int UiaRaiseNotificationEvent(
            IRawElementProviderSimple provider,
            AutomationNotificationKind notificationKind,
            AutomationNotificationProcessing notificationProcessing,
            string notificationText,
            string notificationGuid);

        [DllImport("UIAutomationCore.dll")]
        public static extern bool UiaClientsAreListening();
    }
}
