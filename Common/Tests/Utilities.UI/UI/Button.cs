// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows.Automation;

namespace TestUtilities.UI
{
    public class Button : AutomationWrapper
    {
        public Button(AutomationElement element)
            : base(element)
        {
        }

        public void Click()
        {
            Invoke(Element);
        }
    }
}

