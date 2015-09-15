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
            _previewIntelliSenseRadioButton.Enabled = NodejsPackage.Instance.IntellisenseOptionsPage.EnableES6Preview;
        }

        internal bool SaveToDisk {
            get {
                return _saveToDiskEnabledRadioButton.Checked;
            }
            set {
                if (value == true) {
                    _saveToDiskEnabledRadioButton.Checked = true;
                } else {
                    _saveToDiskDisabledRadioButton.Checked = true;
                }
            }
        }

        internal AnalysisLevel AnalysisLevel {
            get {
                if (_previewIntelliSenseRadioButton.Checked) {
                    return AnalysisLevel.Preview;
                } else if (_fullIntelliSenseRadioButton.Checked) {
                    return AnalysisLevel.High;
                } else if (_mediumIntelliSenseRadioButton.Checked) {
                    return AnalysisLevel.Medium;
                } else {
                    return AnalysisLevel.None;
                }
            }
            set {
                switch (value) {
                    case AnalysisLevel.Preview:
                        _previewIntelliSenseRadioButton.Checked = true;
                        break;
                    case AnalysisLevel.High:
                        _fullIntelliSenseRadioButton.Checked = true;
                        break;
                    case AnalysisLevel.Medium:
                        _mediumIntelliSenseRadioButton.Checked = true;
                        break;
                    case AnalysisLevel.None:
                        _noIntelliSenseRadioButton.Checked = true;
                        break;
                    default:
                        Debug.Fail("Unrecognized AnalysisLevel: " + value);
                        break;
                }
            }
        }

        internal int AnalysisLogMaximum {
            get {
                int max;
                // The Max Value is described by 'Max' instead of 'Int32.MaxValue'
                if (_analysisLogMax.Text == "Max") {
                    return Int32.MaxValue;
                }
                if (Int32.TryParse(_analysisLogMax.Text, out max)) {
                    return max;
                }
                return 0;
            }
            set {
                if (value == 0) {
                    _analysisLogMax.SelectedIndex = 0;
                    return;
                }
                // Handle case where value is the Max.
                string index = value == Int32.MaxValue ? "Max" : value.ToString();
                for (int i = 0; i < _analysisLogMax.Items.Count; i++) {
                    if (_analysisLogMax.Items[i].ToString() == index) {
                        _analysisLogMax.SelectedIndex = i;
                        return;
                    }
                }

                _analysisLogMax.Text = index;
            }
        }

        internal bool OnlyTabOrEnterToCommit {
            get {
                return _onlyTabOrEnterToCommit.Checked;
            }
            set {
                _onlyTabOrEnterToCommit.Checked = value;
            }
        }

        internal bool ShowCompletionListAfterCharacterTyped {
            get {
                return _showCompletionListAfterCharacterTyped.Checked;
            }
            set {
                _showCompletionListAfterCharacterTyped.Checked = value;
            }
        }

        internal void SyncPageWithControlSettings(NodejsIntellisenseOptionsPage page) {
            page.AnalysisLevel = AnalysisLevel;
            page.AnalysisLogMax = AnalysisLogMaximum;
            page.SaveToDisk = SaveToDisk;
            page.OnlyTabOrEnterToCommit = OnlyTabOrEnterToCommit;
            page.ShowCompletionListAfterCharacterTyped = ShowCompletionListAfterCharacterTyped;
        }

        internal void SyncControlWithPageSettings(NodejsIntellisenseOptionsPage page) {
            AnalysisLevel = page.AnalysisLevel;
            AnalysisLogMaximum = page.AnalysisLogMax;
            SaveToDisk = page.SaveToDisk;
            OnlyTabOrEnterToCommit = page.OnlyTabOrEnterToCommit;
            ShowCompletionListAfterCharacterTyped = page.ShowCompletionListAfterCharacterTyped;
        }

        private void _analysisPreviewFeedbackLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("http://aka.ms/NtvsEs6Preview");
        }
    }
}
