// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    public class AzureCloudServiceCreateDialog : AutomationDialog
    {
        public AzureCloudServiceCreateDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public void ClickCreate()
        {
            // Wait for the create button to be enabled
            WaitFor(OkButton, btn => btn.Element.Current.IsEnabled);

            WaitForInputIdle();
            WaitForClosed(TimeSpan.FromSeconds(30.0), () => OkButton.Click());
        }

        public string ServiceName
        {
            get
            {
                return GetServiceNameBox().GetValuePattern().Current.Value;
            }
            set
            {
                WaitForInputIdle();
                GetServiceNameBox().GetValuePattern().SetValue(value);
            }
        }

        public string Location
        {
            get
            {
                return LocationComboBox.GetSelectedItemName();
            }
            set
            {
                WaitForInputIdle();
                WaitFor(LocationComboBox, combobox => combobox.GetSelectedItemName() != "<Loading...>");
                LocationComboBox.SelectItem(value);
            }
        }

        private Button OkButton
        {
            get
            {
                return new Button(FindByAutomationId("OkButton"));
            }
        }

        private ComboBox LocationComboBox
        {
            get
            {
                return new ComboBox(FindByAutomationId("LocationComboBox"));
            }
        }

        private AutomationElement GetServiceNameBox()
        {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "ServiceNameTextBox"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)
                )
            );
        }
    }
}

