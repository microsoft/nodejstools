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
