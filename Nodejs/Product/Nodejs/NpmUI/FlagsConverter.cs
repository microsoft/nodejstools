using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
	class FlagsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            if (!(value is PackageFlags))
				return Visibility.Collapsed;

            var flags = (PackageFlags)value;

            var flag = (PackageFlags)parameter;

			return (flags & flag) == flag ? Visibility.Visible : Visibility.Collapsed;

		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
