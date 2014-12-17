//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

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
                    LoadSettingsFromStorage();
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
            // Load settings from storage.
            FormatOnEnter = LoadBool(FormatOnEnterSetting) ?? true;
            FormatOnSemiColon = LoadBool(FormatOnSemiColonSetting) ?? true;
            FormatOnCloseBrace = LoadBool(FormatOnCloseBraceSetting) ?? true;
            FormatOnPaste = LoadBool(FormatOnPasteSetting) ?? true;

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
            SaveBool(FormatOnEnterSetting, FormatOnEnter);
            SaveBool(FormatOnSemiColonSetting, FormatOnSemiColon);
            SaveBool(FormatOnCloseBraceSetting, FormatOnCloseBrace);
            SaveBool(FormatOnPasteSetting, FormatOnPaste);
        }
    }
}
