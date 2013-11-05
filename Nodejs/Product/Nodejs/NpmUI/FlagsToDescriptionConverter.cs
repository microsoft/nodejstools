using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
	class FlagsToDescriptionConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            var flags = value is PackageFlags ? (PackageFlags)value : 0;

			if (targetType != typeof(string))
				return null;

			var output = new StringBuilder();
            if ((flags & PackageFlags.Missing) != 0)
			{
				output.AppendLine("Missing");
			}
            if ((flags & PackageFlags.NotListedAsDependency) != 0)
			{
				output.AppendLine("Not listed");
			}

            output.AppendLine((flags & PackageFlags.Installed) != 0 ? "Installed" : "Not installed");

			return output;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
