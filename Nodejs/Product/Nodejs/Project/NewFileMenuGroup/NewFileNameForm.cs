// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Project
{
    public partial class NewFileNameForm : Form
    {
        public NewFileNameForm(string initialFileName)
        {
            InitializeComponent();
            this.TextBox.Text = initialFileName;
        }

        public TextBox TextBox => this.textBox;

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.TextBox.Text.Trim().Length == 0)
            {
                this.okButton.Enabled = false;
            }
            else
            {
                this.okButton.Enabled = true;
            }
        }
    }
}
