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

using System.Windows.Forms;

namespace Microsoft.NodejsTools.Options
{
    public partial class NodejsGeneralOptionsControl : UserControl
    {
        private const int SurveyNewsNeverIndex = 0;
        private const int SurveyNewsOnceDayIndex = 1;
        private const int SurveyNewsOnceWeekIndex = 2;
        private const int SurveyNewsOnceMonthIndex = 3;

        public NodejsGeneralOptionsControl()
        {
            InitializeComponent();
        }

        internal SurveyNewsPolicy SurveyNewsCheckCombo
        {
            get
            {
                switch (this._surveyNewsCheckCombo.SelectedIndex)
                {
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
            set
            {
                switch (value)
                {
                    case SurveyNewsPolicy.Disabled:
                        this._surveyNewsCheckCombo.SelectedIndex = SurveyNewsNeverIndex;
                        break;
                    case SurveyNewsPolicy.CheckOnceDay:
                        this._surveyNewsCheckCombo.SelectedIndex = SurveyNewsOnceDayIndex;
                        break;
                    case SurveyNewsPolicy.CheckOnceWeek:
                        this._surveyNewsCheckCombo.SelectedIndex = SurveyNewsOnceWeekIndex;
                        break;
                    case SurveyNewsPolicy.CheckOnceMonth:
                        this._surveyNewsCheckCombo.SelectedIndex = SurveyNewsOnceMonthIndex;
                        break;
                }
            }
        }

        internal void SyncControlWithPageSettings(NodejsGeneralOptionsPage page)
        {
            this.SurveyNewsCheckCombo = page.SurveyNewsCheck;
            this._waitOnAbnormalExit.Checked = page.WaitOnAbnormalExit;
            this._waitOnNormalExit.Checked = page.WaitOnNormalExit;
            this._editAndContinue.Checked = page.EditAndContinue;
            this._checkForLongPaths.Checked = page.CheckForLongPaths;
        }

        internal void SyncPageWithControlSettings(NodejsGeneralOptionsPage page)
        {
            page.SurveyNewsCheck = this.SurveyNewsCheckCombo;
            page.WaitOnAbnormalExit = this._waitOnAbnormalExit.Checked;
            page.WaitOnNormalExit = this._waitOnNormalExit.Checked;
            page.EditAndContinue = this._editAndContinue.Checked;
            page.CheckForLongPaths = this._checkForLongPaths.Checked;
        }
    }
}