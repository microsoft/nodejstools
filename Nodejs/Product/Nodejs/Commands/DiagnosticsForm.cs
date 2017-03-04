// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Commands
{
    public partial class DiagnosticsForm : Form
    {
        public DiagnosticsForm(string content)
        {
            InitializeComponent();
            this._textBox.Text = content;
        }

        public TextBox TextBox => this._textBox;

        private void _ok_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void _copy_Click(object sender, EventArgs e)
        {
            this._textBox.SelectAll();
            Clipboard.SetText(this._textBox.SelectedText);
        }

        private void _diagnosticLoggingCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            NodejsPackage.Instance.DiagnosticsOptionsPage.IsLiveDiagnosticsEnabled = this._diagnosticLoggingCheckbox.Checked;
        }
    }
}

