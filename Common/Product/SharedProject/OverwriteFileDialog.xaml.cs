// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows;

namespace Microsoft.VisualStudioTools
{
    /// <summary>
    /// Interaction logic for OverwriteFileDialog.xaml
    /// </summary>
    internal partial class OverwriteFileDialog : DialogWindowVersioningWorkaround
    {
        public bool ShouldOverwrite;

        public OverwriteFileDialog()
        {
            InitializeComponent();
        }

        public OverwriteFileDialog(string message, bool doForAllItems)
        {
            InitializeComponent();

            if (!doForAllItems)
            {
                this._allItems.Visibility = Visibility.Hidden;
            }

            this._message.Text = message;
        }


        private void YesClick(object sender, RoutedEventArgs e)
        {
            this.ShouldOverwrite = true;
            this.DialogResult = true;
            Close();
        }

        private void NoClick(object sender, RoutedEventArgs e)
        {
            this.ShouldOverwrite = false;
            this.DialogResult = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public bool AllItems => this._allItems.IsChecked.Value;
    }
}

