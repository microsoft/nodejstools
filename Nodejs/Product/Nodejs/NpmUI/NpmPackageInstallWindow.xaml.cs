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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.NodejsTools.NpmUI {
    /// <summary>
    /// Interaction logic for NpmPackageInstallWindow.xaml
    /// </summary>
    public partial class NpmPackageInstallWindow : Window {
        internal NpmPackageInstallWindow(NpmPackageInstallViewModel vm) {
            InitializeComponent();
            DataContext = vm;
        }

        private void ExecuteClose(object sender, ExecutedRoutedEventArgs e) {
            Close();
        }

        private void CanExecuteClose(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        public NpmOutputControl NpmExecuteControl {
            get { return this.ExecuteControl; }
        }

        private static object GetItemObjectAtPoint(ItemsControl control, Point p) {
            var obj = GetListItemAtPoint(control, p);
            return null == obj ? null : control.ItemContainerGenerator.ItemFromContainer(obj);
        }

        private static ListBoxItem GetListItemAtPoint(ItemsControl control, Point p) {
            var result = VisualTreeHelper.HitTest(control, p);
            var obj = result.VisualHit;
            while (VisualTreeHelper.GetParent(obj) != null && !(obj is ListBoxItem)) {
                obj = VisualTreeHelper.GetParent(obj);
            }
            return obj as ListBoxItem;
        }

        private void _packageList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton != MouseButton.Left) {
                return;
            }
            var vm = DataContext as NpmPackageInstallViewModel;
            if (null != vm
                && GetItemObjectAtPoint(_packageList, e.GetPosition(_packageList)) == vm.SelectedPackage
                && ! vm.IsCatalogEmpty) {
                var cmd = vm.InstallCommand;
                if (null != cmd && cmd.CanExecute(null)) {
                    cmd.Execute(null);
                }
            }
        }
    }
}
