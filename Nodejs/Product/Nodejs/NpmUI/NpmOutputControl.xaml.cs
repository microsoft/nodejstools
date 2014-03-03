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

namespace Microsoft.NodejsTools.NpmUI {
    /// <summary>
    /// Interaction logic for NpmOutputControl.xaml
    /// </summary>
    public partial class NpmOutputControl : UserControl {
        public NpmOutputControl() {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            switch (e.Property.Name) {
                case "DataContext":
                    var vm = DataContext as NpmOutputControlViewModel;
                    if (null != vm) {
                        _textBox.Document = vm.Output;
                        vm.OutputWritten += vm_OutputWritten;
                    }
                    break;
            }

            base.OnPropertyChanged(e);
        }

        void vm_OutputWritten(object sender, EventArgs e) {
            _textBox.ScrollToEnd();
        }

        private void OnClickCancel(object sender, RoutedEventArgs e) {
            var vm = DataContext as NpmOutputControlViewModel;
            if (null != vm) {
                vm.Cancel();
            }
        }
    }
}
