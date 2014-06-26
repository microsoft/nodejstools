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
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Options {
    [ComVisible(true)]
    public class NodejsFormattingSpacingOptionsPage : NodejsDialogPage {
        private NodejsFormattingSpacingOptionsControl _window;

        public NodejsFormattingSpacingOptionsPage()
            : base("Formatting") {
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window {
            get {
                if (_window == null) {
                    _window = new NodejsFormattingSpacingOptionsControl();
                }
                return _window;
            }
        }

        public bool SpaceAfterComma { get; set; }
        public bool SpaceAfterSemicolonInFor { get; set; }
        public bool SpaceBeforeAndAfterBinaryOperator { get; set; }
        public bool SpaceAfterKeywordsInControlFlow { get; set; }
        public bool SpaceAfterFunctionKeywordForAnonymousFunctions { get; set; }
        public bool SpaceAfterOpeningAndBeforeClosingNonEmptyParens { get; set; }

        /// <summary>
        /// Resets settings back to their defaults. This should be followed by
        /// a call to <see cref="SaveSettingsToStorage"/> to commit the new
        /// values.
        /// </summary>
        public override void ResetSettings() {
            SpaceAfterComma = SpaceAfterSemicolonInFor = SpaceBeforeAndAfterBinaryOperator = SpaceAfterKeywordsInControlFlow = SpaceAfterFunctionKeywordForAnonymousFunctions = true;
            SpaceAfterOpeningAndBeforeClosingNonEmptyParens = false;
        }

        private const string SpaceAfterCommaSetting = "SpaceAfterComma";
        private const string SpaceAfterSemicolonInForSetting = "SpaceAfterSemicolonInFor";
        private const string SpaceBeforeAndAfterBinaryOperatorSetting = "SpaceBeforeAndAfterBinaryOperator";
        private const string SpaceAfterKeywordsInControlFlowSetting = "SpaceAfterKeywordsInControlFlow";
        private const string SpaceAfterFunctionKeywordForAnonymousFunctionsSetting = "SpaceAfterFunctionKeywordForAnonymousFunctions";
        private const string SpaceAfterOpeningAndBeforeClosingNonEmptyParensSetting = "SpaceAfterOpeningAndBeforeClosingNonEmptyParens";

        public override void LoadSettingsFromStorage() {
            SpaceAfterComma = LoadBool(SpaceAfterCommaSetting) ?? true;
            SpaceAfterSemicolonInFor = LoadBool(SpaceAfterSemicolonInForSetting) ?? true;
            SpaceBeforeAndAfterBinaryOperator = LoadBool(SpaceBeforeAndAfterBinaryOperatorSetting) ?? true;
            SpaceAfterKeywordsInControlFlow = LoadBool(SpaceAfterKeywordsInControlFlowSetting) ?? true;
            SpaceAfterFunctionKeywordForAnonymousFunctions = LoadBool(SpaceAfterFunctionKeywordForAnonymousFunctionsSetting) ?? true;
            SpaceAfterOpeningAndBeforeClosingNonEmptyParens = LoadBool(SpaceAfterOpeningAndBeforeClosingNonEmptyParensSetting) ?? false;
        }

        public override void SaveSettingsToStorage() {
            SaveBool(SpaceAfterCommaSetting, SpaceAfterComma);
            SaveBool(SpaceAfterSemicolonInForSetting, SpaceAfterSemicolonInFor);
            SaveBool(SpaceBeforeAndAfterBinaryOperatorSetting, SpaceBeforeAndAfterBinaryOperator);
            SaveBool(SpaceAfterKeywordsInControlFlowSetting, SpaceAfterKeywordsInControlFlow);
            SaveBool(SpaceAfterFunctionKeywordForAnonymousFunctionsSetting, SpaceAfterFunctionKeywordForAnonymousFunctions);
            SaveBool(SpaceAfterOpeningAndBeforeClosingNonEmptyParensSetting, SpaceAfterOpeningAndBeforeClosingNonEmptyParens);
        }
    }
}
