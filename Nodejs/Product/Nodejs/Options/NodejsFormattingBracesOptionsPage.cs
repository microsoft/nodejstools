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
    public class NodejsFormattingBracesOptionsPage : NodejsDialogPage {
        private NodejsFormattingBracesOptionsControl _window;

        public NodejsFormattingBracesOptionsPage()
            : base("Formatting") {
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window {
            get {
                if (_window == null) {
                    _window = new NodejsFormattingBracesOptionsControl();
                    LoadSettingsFromStorage();
                }

                return _window;
            }
        }

        public bool BraceOnNewLineForFunctions { get; set; }

        public bool BraceOnNewLineForControlBlocks { get; set; }

        /// <summary>
        /// Resets settings back to their defaults. This should be followed by
        /// a call to <see cref="SaveSettingsToStorage"/> to commit the new
        /// values.
        /// </summary>
        public override void ResetSettings(){
            BraceOnNewLineForFunctions = BraceOnNewLineForControlBlocks = false;
        }

        private const string BraceOnNewLineForFunctionsSetting = "BraceOnNewLineForFunctions";
        private const string BraceOnNewLineForControlBlocksSetting = "BraceOnNewLineForControlBlocks";

        public override void LoadSettingsFromStorage(){
            // Load settings from storage.
            BraceOnNewLineForFunctions = LoadBool(BraceOnNewLineForFunctionsSetting) ?? false;
            BraceOnNewLineForControlBlocks = LoadBool(BraceOnNewLineForControlBlocksSetting) ?? false;

            // Synchronize UI with backing properties.
            if (_window != null) {
                _window.SyncControlWithPageSettings(this);
            }
        }

        public override void SaveSettingsToStorage() {
            // Synchronize backing properties with UI.
            if (_window != null) {
                _window.SyncPageWithControlSettings(this);
            }

            // Save settings.
            SaveBool(BraceOnNewLineForFunctionsSetting, BraceOnNewLineForFunctions);
            SaveBool(BraceOnNewLineForControlBlocksSetting, BraceOnNewLineForControlBlocks);
        }
    }
}
