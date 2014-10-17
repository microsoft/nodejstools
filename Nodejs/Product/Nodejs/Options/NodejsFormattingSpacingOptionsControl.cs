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
        public NodejsFormattingSpacingOptionsControl() {
            InitializeComponent();
        }

        internal void SyncControlWithPageSettings(NodejsFormattingSpacingOptionsPage page) {
            _spaceAfterCommaDelimiter.Checked = page.SpaceAfterComma;
            _spaceAfterFunction.Checked = page.SpaceAfterFunctionKeywordForAnonymousFunctions;
            _spacesAfterKeywordsInControlFlow.Checked = page.SpaceAfterKeywordsInControlFlow;
            _nonEmptyParenthesis.Checked = page.SpaceAfterOpeningAndBeforeClosingNonEmptyParens;
            _afterSemicolonFor.Checked = page.SpaceAfterSemicolonInFor;
            _binaryOperators.Checked = page.SpaceBeforeAndAfterBinaryOperator;
        }

        internal void SyncPageWithControlSettings(NodejsFormattingSpacingOptionsPage page) {
            page.SpaceAfterComma = _spaceAfterCommaDelimiter.Checked;
            page.SpaceAfterFunctionKeywordForAnonymousFunctions = _spaceAfterFunction.Checked;
            page.SpaceAfterKeywordsInControlFlow = _spacesAfterKeywordsInControlFlow.Checked;
            page.SpaceAfterOpeningAndBeforeClosingNonEmptyParens = _nonEmptyParenthesis.Checked;
            page.SpaceAfterSemicolonInFor = _afterSemicolonFor.Checked;
            page.SpaceBeforeAndAfterBinaryOperator = _binaryOperators.Checked;
        }
    }
}
