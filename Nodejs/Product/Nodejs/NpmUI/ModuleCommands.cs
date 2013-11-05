using System.Windows.Input;

namespace Microsoft.NodejsTools.NpmUI
{
	public class ModuleCommands
	{
		private static readonly RoutedUICommand installOrUninstall;
		private static readonly RoutedUICommand search;
		private static readonly RoutedUICommand updatePackageJson;
		private static readonly RoutedUICommand dismiss;

		static ModuleCommands()
		{
			installOrUninstall = new RoutedUICommand("InstallOrUninstall", "InstallOrUninstall", typeof(ModuleCommands));
			search = new RoutedUICommand("Search", "Search", typeof(ModuleCommands));
			dismiss = new RoutedUICommand("Dismiss", "Dismiss", typeof(ModuleCommands));
			updatePackageJson = new RoutedUICommand("UpdatePackageJson", "UpdatePackageJson", typeof(ModuleCommands));
		}

		public static RoutedUICommand InstallOrUninstall { get { return installOrUninstall; } }
		public static RoutedUICommand Search { get { return search; } }
		public static RoutedUICommand Dismiss { get { return dismiss; } }
		public static RoutedUICommand UpdatePackageJson { get { return updatePackageJson; } }
	}
}