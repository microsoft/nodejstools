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

using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Options {
    [ComVisible(true)]
    public class NodejsAdvancedEditorOptionsPage : NodejsDialogPage {
        private const string EnterOutliningOnOpenSetting = "EnterOutliningOnOpenSetting";
        private NodejsAdvancedEditorOptionsControl _window;

        public NodejsAdvancedEditorOptionsPage()
            : base("Advanced") {
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override IWin32Window Window {
            get {
                if (_window == null) {
                    _window = new NodejsAdvancedEditorOptionsControl();
                    LoadSettingsFromStorage();
                }
                return _window;
            }
        }

        /// <summary>
        /// Indicates whether to enable outlining mode when files open.
        /// </summary>
        public bool EnterOutliningOnOpen { get; set; }
        
        /// <summary>
        /// Resets settings back to their defaults. This should be followed by
        /// a call to <see cref="SaveSettingsToStorage" /> to commit the new
        /// values.
        /// </summary>
        public override void ResetSettings() {
            EnterOutliningOnOpen = true;
        }

        public override void LoadSettingsFromStorage() {
            // Load settings from storage.
            EnterOutliningOnOpen = LoadBool(EnterOutliningOnOpenSetting) ?? true;

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
            SaveBool(EnterOutliningOnOpenSetting, EnterOutliningOnOpen);
        }
    }
}