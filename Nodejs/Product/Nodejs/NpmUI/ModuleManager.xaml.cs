using System.Windows.Input;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.NodejsTools.NpmUI
{
	/// <summary>
	/// Interaction logic for ModuleManager.xaml
	/// </summary>
	public partial class ModuleManager : DialogWindow
	{
		public ModuleManager(INpmController controller)
		{
			InitializeComponent();
			CommandBinding close = new CommandBinding(ApplicationCommands.Close);
			close.Executed += (o, e) => Close();
			CommandBindings.Add(close);

			var viewModel = new ModuleSourcesViewModel(controller);
			DataContext = viewModel;
		}

		private async void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			await (DataContext as ModuleSourcesViewModel).InstallationAction(sender, e);
		}

		private void CommandBinding_Search(object sender, ExecutedRoutedEventArgs e)
		{
			(DataContext as ModuleSourcesViewModel).Search();
		}

		private void CommandBinding_UpdatePackagesJson(object sender, ExecutedRoutedEventArgs e)
		{
			(DataContext as ModuleSourcesViewModel).UpdatePackagesJson();
		}

		private void CommandBinding_Dismiss(object sender, ExecutedRoutedEventArgs e)
		{
			(DataContext as ModuleSourcesViewModel).DismissProgressBar();
		}
	}
}
