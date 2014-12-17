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
                    LoadSettingsFromStorage();
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
            // Load settings from storage.
            SpaceAfterComma = LoadBool(SpaceAfterCommaSetting) ?? true;
            SpaceAfterSemicolonInFor = LoadBool(SpaceAfterSemicolonInForSetting) ?? true;
            SpaceBeforeAndAfterBinaryOperator = LoadBool(SpaceBeforeAndAfterBinaryOperatorSetting) ?? true;
            SpaceAfterKeywordsInControlFlow = LoadBool(SpaceAfterKeywordsInControlFlowSetting) ?? true;
            SpaceAfterFunctionKeywordForAnonymousFunctions = LoadBool(SpaceAfterFunctionKeywordForAnonymousFunctionsSetting) ?? true;
            SpaceAfterOpeningAndBeforeClosingNonEmptyParens = LoadBool(SpaceAfterOpeningAndBeforeClosingNonEmptyParensSetting) ?? false;

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
            SaveBool(SpaceAfterCommaSetting, SpaceAfterComma);
            SaveBool(SpaceAfterSemicolonInForSetting, SpaceAfterSemicolonInFor);
            SaveBool(SpaceBeforeAndAfterBinaryOperatorSetting, SpaceBeforeAndAfterBinaryOperator);
            SaveBool(SpaceAfterKeywordsInControlFlowSetting, SpaceAfterKeywordsInControlFlow);
            SaveBool(SpaceAfterFunctionKeywordForAnonymousFunctionsSetting, SpaceAfterFunctionKeywordForAnonymousFunctions);
            SaveBool(SpaceAfterOpeningAndBeforeClosingNonEmptyParensSetting, SpaceAfterOpeningAndBeforeClosingNonEmptyParens);
        }
    }
}
