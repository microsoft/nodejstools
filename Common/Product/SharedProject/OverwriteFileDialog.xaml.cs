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
