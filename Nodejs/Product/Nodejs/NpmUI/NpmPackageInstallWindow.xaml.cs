//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.NpmUI
{
    /// <summary>
    /// Interaction logic for NpmPackageInstallWindow.xaml
    /// </summary>
    internal sealed partial class NpmPackageInstallWindow : DialogWindowVersioningWorkaround, IDisposable
    {
        private readonly NpmPackageInstallViewModel _vm;
        private NpmOutputWindow _outputWindow;

        internal NpmPackageInstallWindow(INpmController controller, NpmOutputViewModel executeVm, DependencyType dependencyType = DependencyType.Standard)
        {
            this.DataContext = this._vm = new NpmPackageInstallViewModel(executeVm, this.Dispatcher);
            this._vm.NpmController = controller;
            InitializeComponent();
            this.DependencyComboBox.SelectedIndex = (int)dependencyType;
        }

        public void Dispose()
        {
            //  This will unregister event handlers on the controller and prevent
            //  us from leaking view models.
            if (this._outputWindow != null)
            {
                this._outputWindow.Closing -= this._outputWindow_Closing;
                this._outputWindow.Close();
            }

            this._vm.NpmController = null;

            // The catalog refresh operation spawns many long-lived Gen 2 objects,
            // so the garbage collector will take a while to get to them otherwise.
            GC.Collect();
        }

        private void _outputWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this._outputWindow.Hide();
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void InstallCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this._vm.Install(e.Parameter as PackageCatalogEntryViewModel);
        }

        private void InstallCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.FilterTextBox.IsFocused && this._vm.CanInstall(e.Parameter as PackageCatalogEntryViewModel);
            e.Handled = true;
        }

        private void RefreshCatalogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this._vm.RefreshCatalog();
        }

        private void RefreshCatalogCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this._vm.CanRefreshCatalog;
            e.Handled = true;
        }

        private void OpenHomepageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this._vm.OpenHomepage(e.Parameter as string);
        }

        private void OpenHomepageCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this._vm.CanOpenHomepage(e.Parameter as string);
            e.Handled = true;
        }

        private void FilterTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((e.NewValue as bool?) ?? false)
            {
                ((UIElement)sender).Focus();
            }
        }

        private void FilterTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                case Key.Enter:
                    if (this._packageList.SelectedIndex == -1 && this._packageList.Items.Count > 0)
                    {
                        this._packageList.SelectedIndex = 0;
                    }

                    FocusOnSelectedItemInPackageList();
                    e.Handled = true;
                    break;
            }
        }

        private void FocusOnSelectedItemInPackageList()
        {
            this._packageList.ScrollIntoView(this._packageList.SelectedItem);
            var itemContainer = (ListViewItem)this._packageList.ItemContainerGenerator.ContainerFromItem(this._packageList.SelectedItem);
            if (itemContainer != null)
            {
                itemContainer.Focus();
            }
        }

        private void _packageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._packageList.ScrollIntoView(this._packageList.SelectedItem);
        }

        private void _packageList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && this._packageList.SelectedIndex == 0)
            {
                this.FilterTextBox.Focus();
                e.Handled = true;
            }
        }

        private void ShowOutputWindow_Click(object sender, RoutedEventArgs e)
        {
            if (this._outputWindow == null)
            {
                this._outputWindow = new NpmOutputWindow()
                {
                    Owner = this,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.Manual
                };

                this._outputWindow.Left = Math.Max(0, this.Left - this._outputWindow.Width - 30);
                this._outputWindow.Top = Math.Max(0, this.Top);

                this._outputWindow.Closing += this._outputWindow_Closing;
                this._outputWindow.DataContext = this._vm.ExecuteViewModel;
            }

            this._outputWindow.Show();
            if (this._outputWindow.WindowState == WindowState.Minimized)
            {
                this._outputWindow.WindowState = WindowState.Normal;
            }
        }

        private void ResetOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            this.DependencyComboBox.SelectedIndex = (int)DependencyType.Standard;
            this.SaveToPackageJsonCheckbox.IsChecked = true;

            this.ArgumentsTextBox.Text = string.Empty;
            this.ArgumentsTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void SelectedVersionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedVersionComboBox.SelectedIndex == -1)
            {
                this.SelectedVersionComboBox.SelectedIndex = 0;
            }
        }
    }
}
