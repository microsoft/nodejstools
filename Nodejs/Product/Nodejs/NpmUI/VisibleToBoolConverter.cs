using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.NodejsTools.NpmUI
{
	class VisibleToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Visibility))
				return null;

			var invert = (parameter is string) && (parameter as string) == "true";

			var visible = (Visibility) value;
			
			if (visible == Visibility.Visible)
				return !invert;

			return invert;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
