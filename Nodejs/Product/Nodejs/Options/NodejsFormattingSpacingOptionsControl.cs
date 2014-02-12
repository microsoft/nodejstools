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
    public partial class NodejsFormattingSpacingOptionsControl : UserControl {
        private bool _loading = true;

        public NodejsFormattingSpacingOptionsControl() {
            InitializeComponent();
            _spaceAfterCommaDelimiter.Checked = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterComma;
            _spaceAfterFunction.Checked = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterFunctionKeywordForAnonymousFunctions;
            _spacesAfterKeywordsInControlFlow.Checked = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterKeywordsInControlFlow;
            _nonEmptyParenthesis.Checked = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterOpeningAndBeforeClosingNonEmptyParens;
            _afterSemicolonFor.Checked = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterSemicolonInFor;
            _binaryOperators.Checked = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceBeforeAndAfterBinaryOperator;
            _loading = false;
        }

        private void CheckedChanged(object sender, EventArgs e) {
            if (!_loading) {
                NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterComma = _spaceAfterCommaDelimiter.Checked;
                NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterFunctionKeywordForAnonymousFunctions = _spaceAfterFunction.Checked;
                NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterKeywordsInControlFlow = _spacesAfterKeywordsInControlFlow.Checked;
                NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterOpeningAndBeforeClosingNonEmptyParens = _nonEmptyParenthesis.Checked;
                NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterSemicolonInFor = _afterSemicolonFor.Checked;
                NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceBeforeAndAfterBinaryOperator = _binaryOperators.Checked;
            }
        }
    }
}
