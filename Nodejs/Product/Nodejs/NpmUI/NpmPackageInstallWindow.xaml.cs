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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.NpmUI {
    /// <summary>
    /// Interaction logic for NpmPackageInstallWindow.xaml
    /// </summary>
    sealed partial class NpmPackageInstallWindow : DialogWindowVersioningWorkaround, IDisposable {
        private readonly NpmPackageInstallViewModel _vm;
        private NpmOutputWindow _outputWindow;
        
        internal NpmPackageInstallWindow(INpmController controller, NpmOutputViewModel executeVm, DependencyType dependencyType = DependencyType.Standard) {
            DataContext = _vm = new NpmPackageInstallViewModel(executeVm, Dispatcher);
            _vm.NpmController = controller;
            InitializeComponent();
            DependencyComboBox.SelectedIndex = (int)dependencyType;
        }

        public void Dispose() {
            //  This will unregister event handlers on the controller and prevent
            //  us from leaking view models.
            if (_outputWindow != null) {
                _outputWindow.Closing -= _outputWindow_Closing;
                _outputWindow.Close();
            }
            
            _vm.NpmController = null;

            // The catalog refresh operation spawns many long-lived Gen 2 objects,
            // so the garbage collector will take a while to get to them otherwise.
            GC.Collect();
        }

        void _outputWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            _outputWindow.Hide();
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e) {
            Close();
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void InstallCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
            _vm.Install(e.Parameter as PackageCatalogEntryViewModel);
        }

        private void InstallCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = !FilterTextBox.IsFocused && _vm.CanInstall(e.Parameter as PackageCatalogEntryViewModel);
            e.Handled = true;
        }

        private void RefreshCatalogCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
            _vm.RefreshCatalog();
        }

        private void RefreshCatalogCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = _vm.CanRefreshCatalog;
            e.Handled = true;
        }

        private void OpenHomepageCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
            _vm.OpenHomepage(e.Parameter as string);
        }

        private void OpenHomepageCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = _vm.CanOpenHomepage(e.Parameter as string);
            e.Handled = true;
        }

        private void FilterTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if ((e.NewValue as bool?) ?? false) {
                ((UIElement)sender).Focus();
            }
        }

        private void FilterTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Down:
                case Key.Enter:
                    if (_packageList.SelectedIndex == -1 && _packageList.Items.Count > 0) {
                        _packageList.SelectedIndex = 0;
                    }
                    
                    FocusOnSelectedItemInPackageList();
                    e.Handled = true;
                    break;
            }
        }

        private void FocusOnSelectedItemInPackageList() {
            _packageList.ScrollIntoView(_packageList.SelectedItem);
            var itemContainer = (ListViewItem)_packageList.ItemContainerGenerator.ContainerFromItem(_packageList.SelectedItem);
            if (itemContainer != null) {
                itemContainer.Focus();
            }
        }

        private void _packageList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _packageList.ScrollIntoView(_packageList.SelectedItem);
        }

        private void _packageList_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Up && _packageList.SelectedIndex == 0) {
                FilterTextBox.Focus();
                e.Handled = true;
            }
        }

        private void ShowOutputWindow_Click(object sender, RoutedEventArgs e) {
            if (_outputWindow == null) {
                _outputWindow = new NpmOutputWindow() {
                    Owner = this,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.Manual
                };
                
                _outputWindow.Left = Math.Max(0, this.Left - _outputWindow.Width - 30);
                _outputWindow.Top = Math.Max(0, this.Top);

                _outputWindow.Closing += _outputWindow_Closing;
                _outputWindow.DataContext = _vm.ExecuteViewModel;
            }

            _outputWindow.Show();
            if (_outputWindow.WindowState == WindowState.Minimized) {
                _outputWindow.WindowState = WindowState.Normal;
            }
        }

        private void ResetOptionsButton_Click(object sender, RoutedEventArgs e) {
            this.DependencyComboBox.SelectedIndex = (int)DependencyType.Standard;
            this.SaveToPackageJsonCheckbox.IsChecked = true;

            ArgumentsTextBox.Text = string.Empty;
            ArgumentsTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void SelectedVersionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (this.SelectedVersionComboBox.SelectedIndex == -1) {
                SelectedVersionComboBox.SelectedIndex = 0;
            }
        }
    }
}
