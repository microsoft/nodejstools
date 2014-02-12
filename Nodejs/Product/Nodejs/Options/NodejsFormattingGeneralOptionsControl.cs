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
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Options {
    public partial class NodejsFormattingGeneralOptionsControl : UserControl {
        private bool _loading = true;

        public NodejsFormattingGeneralOptionsControl() {
            InitializeComponent();
            _formatOnCloseBrace.Checked = NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnCloseBrace;
            _formatOnEnter.Checked = NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnEnter;
            _formatOnPaste.Checked = NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnPaste;
            _formatOnSemicolon.Checked = NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnSemiColon;
            _loading = false;
        }

        private void CheckedChanged(object sender, EventArgs e) {
            if (!_loading) {
                NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnCloseBrace = _formatOnCloseBrace.Checked;
                NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnEnter = _formatOnEnter.Checked;
                NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnPaste = _formatOnPaste.Checked;
                NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnSemiColon = _formatOnSemicolon.Checked;
            }
        }
    }
}
