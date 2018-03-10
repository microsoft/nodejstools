// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.NodejsTools.NpmUI
{
    public sealed class FilterStateVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string state && parameter is string expected)
            {
                if (targetType == typeof(Visibility))
                {
                    return StringComparer.OrdinalIgnoreCase.Equals(state, expected) ? Visibility.Visible : Visibility.Hidden;
                }

                if (targetType == typeof(bool))
                {
                    return StringComparer.OrdinalIgnoreCase.Equals(state, expected);
                }
            }

            throw new InvalidCastException("Wrong input type.");
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
