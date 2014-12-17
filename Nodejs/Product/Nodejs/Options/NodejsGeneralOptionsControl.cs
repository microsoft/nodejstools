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
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Options {
    public partial class NodejsGeneralOptionsControl : UserControl {
        private const int SurveyNewsNeverIndex = 0;
        private const int SurveyNewsOnceDayIndex = 1;
        private const int SurveyNewsOnceWeekIndex = 2;
        private const int SurveyNewsOnceMonthIndex = 3;

        public NodejsGeneralOptionsControl() {
            InitializeComponent();
        }

        internal SurveyNewsPolicy SurveyNewsCheckCombo {
            get {
                switch (_surveyNewsCheckCombo.SelectedIndex) {
                    case SurveyNewsNeverIndex:
                        return SurveyNewsPolicy.Disabled;
                    case SurveyNewsOnceDayIndex:
                        return SurveyNewsPolicy.CheckOnceDay;
                    case SurveyNewsOnceWeekIndex:
                        return SurveyNewsPolicy.CheckOnceWeek;
                    case SurveyNewsOnceMonthIndex:
                        return SurveyNewsPolicy.CheckOnceMonth;
                    default:
                        return SurveyNewsPolicy.Disabled;
                }
            }
            set {
                switch (value) {
                    case SurveyNewsPolicy.Disabled:
                        _surveyNewsCheckCombo.SelectedIndex = SurveyNewsNeverIndex;
                        break;
                    case SurveyNewsPolicy.CheckOnceDay:
                        _surveyNewsCheckCombo.SelectedIndex = SurveyNewsOnceDayIndex;
                        break;
                    case SurveyNewsPolicy.CheckOnceWeek:
                        _surveyNewsCheckCombo.SelectedIndex = SurveyNewsOnceWeekIndex;
                        break;
                    case SurveyNewsPolicy.CheckOnceMonth:
                        _surveyNewsCheckCombo.SelectedIndex = SurveyNewsOnceMonthIndex;
                        break;
                }
            }
        }

        internal void SyncControlWithPageSettings(NodejsGeneralOptionsPage page) {
            SurveyNewsCheckCombo = page.SurveyNewsCheck;
            _waitOnAbnormalExit.Checked = page.WaitOnAbnormalExit;
            _waitOnNormalExit.Checked = page.WaitOnNormalExit;
            _editAndContinue.Checked = page.EditAndContinue;
            _checkForLongPaths.Checked = page.CheckForLongPaths;
        }

        internal void SyncPageWithControlSettings(NodejsGeneralOptionsPage page) {
            page.SurveyNewsCheck = SurveyNewsCheckCombo;
            page.WaitOnAbnormalExit = _waitOnAbnormalExit.Checked;
            page.WaitOnNormalExit = _waitOnNormalExit.Checked;
            page.EditAndContinue = _editAndContinue.Checked;
            page.CheckForLongPaths = _checkForLongPaths.Checked;
        }
    }
}