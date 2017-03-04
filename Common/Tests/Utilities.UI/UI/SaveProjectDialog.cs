// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    internal class SaveProjectDialog : AutomationDialog
    {
        public SaveProjectDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element)
        {
        }

        public void Save()
        {
            Invoke(FindButton("Save"));
        }

        public string ProjectName
        {
            get
            {
                var patterns = GetProjectNameBox().GetSupportedPatterns();
                var filename = (ValuePattern)GetProjectNameBox().GetCurrentPattern(ValuePattern.Pattern);
                return filename.Current.Value;
            }
            set
            {
                var patterns = GetProjectNameBox().GetSupportedPatterns();
                var filename = (ValuePattern)GetProjectNameBox().GetCurrentPattern(ValuePattern.Pattern);
                filename.SetValue(value);
            }
        }

        private AutomationElement GetProjectNameBox()
        {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "Name:"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)
                )
            );
        }
    }
}

