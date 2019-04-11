// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace Microsoft.NodejsTools.SharedProject.Wpf
{
    internal class NotificationTextBox : TextBox
    {
        // This control's AutomationPeer is the object that actually raises the UIA Notification event.
        private NotificationTextBoxAutomationPeer notificationTextBoxAutomationPeer;

        // Assume the UIA Notification event is available until we learn otherwise.
        // If we learn that the UIA Notification event is not available, no instance
        // of the NotificationTextBox should attempt to raise it.
        private static bool notificationEventAvailable = true;
        public bool NotificationEventAvailable
        {
            get
            {
                return notificationEventAvailable;
            }
            set
            {
                notificationEventAvailable = value;
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            this.notificationTextBoxAutomationPeer = new NotificationTextBoxAutomationPeer(this);

            return this.notificationTextBoxAutomationPeer;
        }

        public void RaiseNotificationEvent(string notificationText, string notificationGuid)
        {
            // Only attempt to raise the event if we already have an AutomationPeer.
            if (this.notificationTextBoxAutomationPeer != null)
            {
                this.notificationTextBoxAutomationPeer.RaiseNotificationEvent(notificationText, notificationGuid);
            }
        }
    }

    internal class NotificationTextBoxAutomationPeer : TextBoxAutomationPeer
    {
        private NotificationTextBox notificationTextBox;

        // The UIA Notification event requires the IRawElementProviderSimple
        // associated with this AutomationPeer.
        private IRawElementProviderSimple rawElementProviderSimple;

        public NotificationTextBoxAutomationPeer(NotificationTextBox owner) : base(owner)
        {
            this.notificationTextBox = owner;
        }

        public void RaiseNotificationEvent(string notificationText, string notificationGuid)
        {
            // If we already know that the UIA Notification event is not available, do not
            // attempt to raise it.
            if (this.notificationTextBox.NotificationEventAvailable)
            {
                // If no UIA clients are listening for events, don't bother raising one.
                if (UiaAutomationNativeMethods.UiaClientsAreListening())
                {
                    // Get the IRawElementProviderSimple for this AutomationPeer if we don't
                    // have it already.
                    if (this.rawElementProviderSimple == null)
                    {
                        var automationPeer = FromElement(this.notificationTextBox);
                        if (automationPeer != null)
                        {
                            this.rawElementProviderSimple = this.ProviderFromPeer(automationPeer);
                        }
                    }

                    if (this.rawElementProviderSimple != null)
                    {
                        try
                        {
                            UiaAutomationNativeMethods.UiaRaiseNotificationEvent(
                                this.rawElementProviderSimple,
                                UiaAutomationNativeMethods.AutomationNotificationKind.ActionCompleted,
                                UiaAutomationNativeMethods.AutomationNotificationProcessing.All,
                                notificationText,
                                notificationGuid);
                        }
                        catch (EntryPointNotFoundException)
                        {
                            // The UIA Notification event is not not available, so don't attempt
                            // to raise it again.
                            this.notificationTextBox.NotificationEventAvailable = false;
                        }
                    }
                }
            }
        }
    }
}
