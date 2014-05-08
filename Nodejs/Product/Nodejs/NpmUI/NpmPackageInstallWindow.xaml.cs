/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.NpmUI {
    /// <summary>
    /// Interaction logic for NpmPackageInstallWindow.xaml
    /// </summary>
    sealed partial class NpmPackageInstallWindow : DialogWindowVersioningWorkaround, IDisposable {
        private readonly NpmPackageInstallViewModel _vm;
        
        internal NpmPackageInstallWindow(INpmController controller, NpmOutputControlViewModel executeVm) {
            DataContext = _vm = new NpmPackageInstallViewModel(executeVm, Dispatcher);
            _vm.NpmController = controller;
            InitializeComponent();
            ExecuteControl.DataContext = executeVm;
        }

        public void Dispose() {
            //  This will unregister event handlers on the controller and prevent
            //  us from leaking view models.
            _vm.NpmController = null;
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
            e.CanExecute = _vm.CanInstall(e.Parameter as PackageCatalogEntryViewModel);
            e.Handled = true;
        }

        private void RefreshCatalogCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
            _vm.RefreshCatalog();
        }

        private void RefreshCatalogCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = _vm.CanRefreshCatalog;
            e.Handled = true;
        }

        private void FilterTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if ((e.NewValue as bool?) ?? false) {
                ((UIElement)sender).Focus();
            }
        }
    }
}
