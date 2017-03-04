// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows.Automation;

namespace TestUtilities.UI
{
    public class TextBox : AutomationWrapper
    {
        public TextBox(AutomationElement element)
            : base(element)
        {
        }

        public string Value
        {
            get
            {
                return Element.GetTextPattern().DocumentRange.GetText(-1);
            }
            set
            {
                Element.GetValuePattern().SetValue(value);
            }
        }
    }
}

