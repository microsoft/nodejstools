// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.NodejsTools.ProjectWizard;

namespace Microsoft.NodejsTools.Project.ImportWizard
{
    public sealed class ProjectLanguageBoolConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ProjectLanguage)value)
            {
                case ProjectLanguage.TypeScript:
                    return StringComparer.OrdinalIgnoreCase.Equals((string)parameter, "typescript");
                case ProjectLanguage.JavaScript:
                    return StringComparer.OrdinalIgnoreCase.Equals((string)parameter, "javascript");
                default:
                    return null;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
            {
                Enum.TryParse<ProjectLanguage>((string)parameter, ignoreCase: true, result: out var result);
                return result;
            }

            throw new InvalidOperationException("Expected to only be called when the radio button is checked.");
        }
    }
}
