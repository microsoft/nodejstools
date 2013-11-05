using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
	class FlagsToInstallConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is PackageFlags))
				return null;

            var flags = (PackageFlags)value;

            return (flags & PackageFlags.Installed) != 0 ? "Uninstall" : "Install";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
