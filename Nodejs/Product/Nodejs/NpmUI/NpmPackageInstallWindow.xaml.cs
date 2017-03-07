// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        private readonly NpmPackageInstallViewModel viewModel;
        private NpmOutputWindow outputWindow;

        internal NpmPackageInstallWindow(INpmController controller, NpmOutputViewModel executeVm, DependencyType dependencyType = DependencyType.Standard)
        {
            this.DataContext = this.viewModel = new NpmPackageInstallViewModel(executeVm, this.Dispatcher);
            this.viewModel.NpmController = controller;
            InitializeComponent();
            this.DependencyComboBox.SelectedIndex = (int)dependencyType;
        }

        public void Dispose()
        {
            //  This will unregister event handlers on the controller and prevent
            //  us from leaking view models.
            if (this.outputWindow != null)
            {
                this.outputWindow.Closing -= this.outputWindow_Closing;
                this.outputWindow.Close();
            }

            this.viewModel.NpmController = null;

            // The catalog refresh operation spawns many long-lived Gen 2 objects,
            // so the garbage collector will take a while to get to them otherwise.
            GC.Collect();
        }

        private void outputWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.outputWindow.Hide();
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
            this.viewModel.Install(e.Parameter as PackageCatalogEntryViewModel);
        }

        private void InstallCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.FilterTextBox.IsFocused && this.viewModel.CanInstall(e.Parameter as PackageCatalogEntryViewModel);
            e.Handled = true;
        }

        private void RefreshCatalogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.viewModel.RefreshCatalog();
        }

        private void RefreshCatalogCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.viewModel.CanRefreshCatalog;
            e.Handled = true;
        }

        private void OpenHomepageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.viewModel.OpenHomepage(e.Parameter as string);
        }

        private void OpenHomepageCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.viewModel.CanOpenHomepage(e.Parameter as string);
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
                    if (this.packageList.SelectedIndex == -1 && this.packageList.Items.Count > 0)
                    {
                        this.packageList.SelectedIndex = 0;
                    }

                    FocusOnSelectedItemInPackageList();
                    e.Handled = true;
                    break;
            }
        }

        private void FocusOnSelectedItemInPackageList()
        {
            this.packageList.ScrollIntoView(this.packageList.SelectedItem);
            var itemContainer = (ListViewItem)this.packageList.ItemContainerGenerator.ContainerFromItem(this.packageList.SelectedItem);
            if (itemContainer != null)
            {
                itemContainer.Focus();
            }
        }

        private void packageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.packageList.ScrollIntoView(this.packageList.SelectedItem);
        }

        private void packageList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && this.packageList.SelectedIndex == 0)
            {
                this.FilterTextBox.Focus();
                e.Handled = true;
            }
        }

        private void ShowOutputWindow_Click(object sender, RoutedEventArgs e)
        {
            if (this.outputWindow == null)
            {
                this.outputWindow = new NpmOutputWindow()
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };

                this.outputWindow.Left = Math.Max(0, this.Left - this.outputWindow.Width - 30);
                this.outputWindow.Top = Math.Max(0, this.Top);

                this.outputWindow.Closing += this.outputWindow_Closing;
                this.outputWindow.DataContext = this.viewModel.ExecuteViewModel;
            }

            this.outputWindow.Show();
            if (this.outputWindow.WindowState == WindowState.Minimized)
            {
                this.outputWindow.WindowState = WindowState.Normal;
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
