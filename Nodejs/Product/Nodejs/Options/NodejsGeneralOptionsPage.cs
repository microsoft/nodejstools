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
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Options
{
    [ComVisible(true)]
    public class NodejsGeneralOptionsPage : NodejsDialogPage
    {
        private const string DefaultSurveyNewsFeedUrl = "https://go.microsoft.com/fwlink/?LinkId=328027";
        private const string DefaultSurveyNewsIndexUrl = "https://go.microsoft.com/fwlink/?LinkId=328029";
        private const string SurveyNewsCheckSetting = "SurveyNewsCheck";
        private const string SurveyNewsLastCheckSetting = "SurveyNewsLastCheck";
        private const string SurveyNewsFeedUrlSetting = "SurveyNewsFeedUrl";
        private const string SurveyNewsIndexUrlSetting = "SurveyNewsIndexUrl";
        private const string WaitOnAbnormalExitSetting = "WaitOnAbnormalExit";
        private const string WaitOnNormalExitSetting = "WaitOnNormalExit";
        private const string EditAndContinueSetting = "EditAndContinue";
        private const string CheckForLongPathsSetting = "CheckForLongPaths";
        private SurveyNewsPolicy _surveyNewsCheck;
        private string _surveyNewsFeedUrl;
        private string _surveyNewsIndexUrl;
        private DateTime _surveyNewsLastCheck;
        private NodejsGeneralOptionsControl _window;

        public NodejsGeneralOptionsPage()
            : base("General")
        {
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override IWin32Window Window
        {
            get
            {
                if (this._window == null)
                {
                    this._window = new NodejsGeneralOptionsControl();
                    LoadSettingsFromStorage();
                }
                return this._window;
            }
        }

        /// <summary>
        /// True if Node processes should pause for input before exiting
        /// if they exit abnormally.
        /// </summary>
        public bool WaitOnAbnormalExit { get; set; }

        /// <summary>
        /// True if Node processes should pause for input before exiting
        /// if they exit normally.
        /// </summary>
        public bool WaitOnNormalExit { get; set; }

        /// <summary>
        /// Indicates whether Edit and Continue feature should be enabled.
        /// </summary>
        public bool EditAndContinue { get; set; }

        /// <summary>
        /// Indicates whether checks for long paths (exceeding MAX_PATH) are performed after installing packages.
        /// </summary>
        public bool CheckForLongPaths { get; set; }

        /// <summary>
        /// The frequency at which to check for updated news. Default is once
        /// per week.
        /// </summary>
        public SurveyNewsPolicy SurveyNewsCheck
        {
            get { return this._surveyNewsCheck; }
            set { this._surveyNewsCheck = value; }
        }

        /// <summary>
        /// The date/time when the last check for news occurred.
        /// </summary>
        public DateTime SurveyNewsLastCheck
        {
            get { return this._surveyNewsLastCheck; }
            set { this._surveyNewsLastCheck = value; }
        }

        /// <summary>
        /// The url of the news feed.
        /// </summary>
        public string SurveyNewsFeedUrl
        {
            get { return this._surveyNewsFeedUrl; }
            set { this._surveyNewsFeedUrl = value; }
        }

        /// <summary>
        /// The url of the news index page.
        /// </summary>
        public string SurveyNewsIndexUrl
        {
            get { return this._surveyNewsIndexUrl; }
            set { this._surveyNewsIndexUrl = value; }
        }

        /// <summary>
        /// Resets settings back to their defaults. This should be followed by
        /// a call to <see cref="SaveSettingsToStorage" /> to commit the new
        /// values.
        /// </summary>
        public override void ResetSettings()
        {
            this._surveyNewsCheck = SurveyNewsPolicy.CheckOnceWeek;
            this._surveyNewsLastCheck = DateTime.MinValue;
            this._surveyNewsFeedUrl = DefaultSurveyNewsFeedUrl;
            this._surveyNewsIndexUrl = DefaultSurveyNewsIndexUrl;
            this.WaitOnAbnormalExit = true;
            this.WaitOnNormalExit = false;
            this.EditAndContinue = true;
            this.CheckForLongPaths = true;
        }

        public override void LoadSettingsFromStorage()
        {
            // Load settings from storage.
            this._surveyNewsCheck = LoadEnum<SurveyNewsPolicy>(SurveyNewsCheckSetting) ?? SurveyNewsPolicy.CheckOnceWeek;
            this._surveyNewsLastCheck = LoadDateTime(SurveyNewsLastCheckSetting) ?? DateTime.MinValue;
            this._surveyNewsFeedUrl = LoadString(SurveyNewsFeedUrlSetting) ?? DefaultSurveyNewsFeedUrl;
            this._surveyNewsIndexUrl = LoadString(SurveyNewsIndexUrlSetting) ?? DefaultSurveyNewsIndexUrl;
            this.WaitOnAbnormalExit = LoadBool(WaitOnAbnormalExitSetting) ?? true;
            this.WaitOnNormalExit = LoadBool(WaitOnNormalExitSetting) ?? false;
            this.EditAndContinue = LoadBool(EditAndContinueSetting) ?? true;
            this.CheckForLongPaths = LoadBool(CheckForLongPathsSetting) ?? true;

            // Synchronize UI with backing properties.
            if (this._window != null)
            {
                this._window.SyncControlWithPageSettings(this);
            }
        }

        public override void SaveSettingsToStorage()
        {
            // Synchronize backing properties with UI.
            if (this._window != null)
            {
                this._window.SyncPageWithControlSettings(this);
            }

            // Save settings.
            SaveEnum(SurveyNewsCheckSetting, this._surveyNewsCheck);
            SaveDateTime(SurveyNewsLastCheckSetting, this._surveyNewsLastCheck);
            SaveBool(WaitOnNormalExitSetting, this.WaitOnNormalExit);
            SaveBool(WaitOnAbnormalExitSetting, this.WaitOnAbnormalExit);
            SaveBool(EditAndContinueSetting, this.EditAndContinue);
            SaveBool(CheckForLongPathsSetting, this.CheckForLongPaths);
        }
    }
}