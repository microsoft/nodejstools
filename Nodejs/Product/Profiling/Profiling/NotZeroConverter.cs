// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.NodejsTools.Profiling
{
    public class NotZeroConverter : ValueConverter<int, bool>
    {
        protected override bool Convert(int value, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != 0;
        }
    }
}

