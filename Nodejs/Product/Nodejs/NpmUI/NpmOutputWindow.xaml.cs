// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows;

namespace Microsoft.NodejsTools.NpmUI
{
    /// <summary>
    /// Interaction logic for NpmOutputControl.xaml
    /// </summary>
    public partial class NpmOutputWindow : Window
    {
        public NpmOutputWindow()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "DataContext":
                    var vm = this.DataContext as NpmOutputViewModel;
                    if (null != vm)
                    {
                        this._textBox.Document = vm.Output;
                        vm.OutputWritten += this.vm_OutputWritten;
                    }
                    break;
            }

            base.OnPropertyChanged(e);
        }

        private void vm_OutputWritten(object sender, EventArgs e)
        {
            this._textBox.ScrollToEnd();
        }

        private void OnClickCancel(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as NpmOutputViewModel;
            if (null != vm)
            {
                vm.Cancel();
            }
        }
    }
}

