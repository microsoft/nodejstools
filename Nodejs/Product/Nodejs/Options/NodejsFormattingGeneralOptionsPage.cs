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
    public class NodejsFormattingGeneralOptionsPage : NodejsDialogPage {
        private NodejsFormattingGeneralOptionsControl _window;

        public NodejsFormattingGeneralOptionsPage()
            : base("Formatting") {
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window {
            get {
                if (_window == null) {
                    _window = new NodejsFormattingGeneralOptionsControl();
                }
                return _window;
            }
        }

        public bool FormatOnEnter { get; set; }
        public bool FormatOnSemiColon { get; set; }
        public bool FormatOnCloseBrace { get; set; }
        public bool FormatOnPaste { get; set; }

        /// <summary>
        /// Resets settings back to their defaults. This should be followed by
        /// a call to <see cref="SaveSettingsToStorage"/> to commit the new
        /// values.
        /// </summary>
        public override void ResetSettings(){
            FormatOnEnter = FormatOnSemiColon = FormatOnCloseBrace = FormatOnPaste = true;
        }

        private const string FormatOnEnterSetting = "FormatOnEnter";
        private const string FormatOnSemiColonSetting = "FormatOnSemiColon";
        private const string FormatOnCloseBraceSetting = "FormatOnCloseBrace";
        private const string FormatOnPasteSetting = "FormatOnPaste";

        public override void LoadSettingsFromStorage(){
            FormatOnEnter = LoadBool(FormatOnEnterSetting) ?? true;
            FormatOnSemiColon = LoadBool(FormatOnSemiColonSetting) ?? true;
            FormatOnCloseBrace = LoadBool(FormatOnCloseBraceSetting) ?? true;
            FormatOnPaste = LoadBool(FormatOnPasteSetting) ?? true;
        }

        public override void SaveSettingsToStorage() {
            SaveBool(FormatOnEnterSetting, FormatOnEnter);
            SaveBool(FormatOnSemiColonSetting, FormatOnSemiColon);
            SaveBool(FormatOnCloseBraceSetting, FormatOnCloseBrace);
            SaveBool(FormatOnPasteSetting, FormatOnPaste);
        }
    }
}
