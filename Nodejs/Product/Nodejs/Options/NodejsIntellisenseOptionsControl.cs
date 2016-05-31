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
using System.Windows.Forms;
using Microsoft.NodejsTools.Project;

namespace Microsoft.NodejsTools.Options {
    public partial class NodejsIntellisenseOptionsControl : UserControl {
        public NodejsIntellisenseOptionsControl() {
            InitializeComponent();
            _intelliSenseModeDropdown.Enabled = NodejsPackage.Instance.IntellisenseOptionsPage.EnableES6Preview;
        }

        internal AnalysisLevel AnalysisLevel {
            get {
                if (_intelliSenseModeDropdown.SelectedIndex == 0) {
                    return AnalysisLevel.Preview;
                } else {
                    return _nodejsES5IntelliSenseOptionsControl.AnalysisLevel;
                }
            }
            set {
                if (_intelliSenseModeDropdown.SelectedIndex == -1) {
                    _intelliSenseModeDropdown.SelectedIndex = value == AnalysisLevel.Preview ? 0 : 1;
                }

                switch (value) {
                    case AnalysisLevel.Preview:
                        // Set default ES5 IntelliSense level - this setting will not take effect if ES6 Preview is enabled.
                        _nodejsES5IntelliSenseOptionsControl.AnalysisLevel = AnalysisLevel.NodeLsMedium;
                        break;
                    case AnalysisLevel.NodeLsHigh:
                    case AnalysisLevel.NodeLsMedium:
                    case AnalysisLevel.NodeLsNone:
                        _nodejsES5IntelliSenseOptionsControl.AnalysisLevel = value;
                        break;
                    default:
                        Debug.Fail("Unrecognized AnalysisLevel: " + value);
                        break;
                }
            }
        }

        internal void SyncPageWithControlSettings(NodejsIntellisenseOptionsPage page) {
            page.AnalysisLevel = AnalysisLevel;
            _nodejsES5IntelliSenseOptionsControl.SyncPageWithControlSettings(page);
            _salsaLsIntellisenseOptionsControl.SyncPageWithControlSettings(page);
        }

        internal void SyncControlWithPageSettings(NodejsIntellisenseOptionsPage page) {
            AnalysisLevel = page.AnalysisLevel;
            _nodejsES5IntelliSenseOptionsControl.SyncControlWithPageSettings(page);
            _salsaLsIntellisenseOptionsControl.SyncControlWithPageSettings(page);
        }

        private void _analysisPreviewFeedbackLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("http://aka.ms/NtvsEs6Preview");
        }

        private void _intelliSenseModeDropdown_SelectedValueChanged(object sender, EventArgs e) {
            bool isES6PreviewIntelliSense = _intelliSenseModeDropdown.SelectedIndex == 0;
            //AnalysisLevel = AnalysisLevel;
            _nodejsES5IntelliSenseOptionsControl.Visible = !isES6PreviewIntelliSense;
            _salsaLsIntellisenseOptionsControl.Visible = isES6PreviewIntelliSense;
            _es5DeprecatedWarning.Visible = !isES6PreviewIntelliSense;
        }
    }
}
