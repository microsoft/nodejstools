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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Options {
    [ComVisible(true)]
    public class NodejsIntellisenseOptionsPage : NodejsDialogPage {
        private NodejsIntellisenseOptionsControl _window;

        public NodejsIntellisenseOptionsPage()
            : base("IntelliSense")
        { }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window {
            get {
                if (_window == null) {
                    _window = new NodejsIntellisenseOptionsControl();
                    LoadSettingsFromStorage();
                }
                return _window;
            }
        }

        internal bool EnableAutomaticTypingsAcquisition { get; set; }

        internal bool ShowTypingsInfoBar { get; set; }

        internal bool SaveChangesToConfigFile { get; set; }

        /// <summary>
        /// Resets settings back to their defaults. This should be followed by
        /// a call to <see cref="SaveSettingsToStorage"/> to commit the new
        /// values.
        /// </summary>
        public override void ResetSettings() {
        }

        private const string EnableAutomaticTypingsAcquisitionSetting = "EnableAutomaticTypingsAcquisition";
        private const string ShowTypingsInfoBarSetting = "ShowTypingsInfoBar";
        private const string SaveChangesToConfigFileSetting = "SaveChangesToConfigFile";

        public override void LoadSettingsFromStorage() {
            // Load settings from storage.
            EnableAutomaticTypingsAcquisition = LoadBool(EnableAutomaticTypingsAcquisitionSetting) ?? true;
            ShowTypingsInfoBar = LoadBool(ShowTypingsInfoBarSetting) ?? true;
            SaveChangesToConfigFile = LoadBool(SaveChangesToConfigFileSetting) ?? false;

            // Synchronize UI with backing properties.
            if (_window != null) {
                _window.SyncControlWithPageSettings(this);
            }

            // Settings values can change after loading them from storage as there
            // are conditions which could make them fallback to default values.
            // Save the final settings back to storage.
            SaveSettingsToStorage();
        }

        public override void SaveSettingsToStorage() {
            // Synchronize backing properties with UI.
            if (_window != null) {
                _window.SyncPageWithControlSettings(this);
            }

            // Save settings.
#if DEV14
            // This setting needs to be present for the TypeScript editor to handle .js files in NTVS
            SaveString("AnalysisLevel", "Preview");
#endif
            SaveBool(EnableAutomaticTypingsAcquisitionSetting, EnableAutomaticTypingsAcquisition);
            SaveBool(ShowTypingsInfoBarSetting, ShowTypingsInfoBar);
            SaveBool(SaveChangesToConfigFileSetting, SaveChangesToConfigFile);
        }
    }
}
