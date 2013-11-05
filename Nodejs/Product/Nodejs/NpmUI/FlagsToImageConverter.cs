using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
	class FlagsToImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            if (!(value is PackageFlags))
				return Visibility.Collapsed;

            var flags = (PackageFlags)value;

            if ((flags & PackageFlags.Missing) != 0)
			{
			    if ( ( flags & PackageFlags.Bundled ) != 0 )
			    {
                    return "/Microsoft.NodejsTools;component/Resources/DependencyBundledMissing_16.png";
			    }

			    if ( ( flags & PackageFlags.Dev ) != 0 )
			    {
                    return "/Microsoft.NodejsTools;component/Resources/DependencyDevMissing_16.png";
			    }

			    if ( ( flags & PackageFlags.Optional ) != 0 )
			    {
                    return "/Microsoft.NodejsTools;component/Resources/DependencyOptionalMissing_16.png";
			    }

                return "/Microsoft.NodejsTools;component/Resources/DependencyMissing_16.png";
			}

            if ((flags & PackageFlags.NotListedAsDependency) != 0)
			{
			    if ( ( flags & PackageFlags.Bundled ) != 0 )
			    {
                    return "/Microsoft.NodejsTools;component/Resources/DependencyExtraneousBundled_16.png";
			    }

                return "/Microsoft.NodejsTools;component/Resources/DependencyExtraneous_16.png";
			}

            return "/Microsoft.NodejsTools;component/Resources/Dependency_16.png";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
