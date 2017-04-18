// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        public SurveyNewsPolicy SurveyNewsCheck { get; set; }

        /// <summary>
        /// The date/time when the last check for news occurred.
        /// </summary>
        public DateTime SurveyNewsLastCheck { get; set; }

        /// <summary>
        /// The url of the news feed.
        /// </summary>
        public string SurveyNewsFeedUrl { get; set; }

        /// <summary>
        /// The url of the news index page.
        /// </summary>
        public string SurveyNewsIndexUrl { get; set; }

        /// <summary>
        /// Resets settings back to their defaults. This should be followed by
        /// a call to <see cref="SaveSettingsToStorage" /> to commit the new
        /// values.
        /// </summary>
        public override void ResetSettings()
        {
            this.SurveyNewsCheck = SurveyNewsPolicy.CheckOnceWeek;
            this.SurveyNewsLastCheck = DateTime.MinValue;
            this.SurveyNewsFeedUrl = DefaultSurveyNewsFeedUrl;
            this.SurveyNewsIndexUrl = DefaultSurveyNewsIndexUrl;
            this.WaitOnAbnormalExit = true;
            this.WaitOnNormalExit = false;
            this.EditAndContinue = true;
            this.CheckForLongPaths = true;
        }

        public override void LoadSettingsFromStorage()
        {
            // Load settings from storage.
            this.SurveyNewsCheck = LoadEnum<SurveyNewsPolicy>(SurveyNewsCheckSetting) ?? SurveyNewsPolicy.CheckOnceWeek;
            this.SurveyNewsLastCheck = LoadDateTime(SurveyNewsLastCheckSetting) ?? DateTime.MinValue;
            this.SurveyNewsFeedUrl = LoadString(SurveyNewsFeedUrlSetting) ?? DefaultSurveyNewsFeedUrl;
            this.SurveyNewsIndexUrl = LoadString(SurveyNewsIndexUrlSetting) ?? DefaultSurveyNewsIndexUrl;
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
            SaveEnum(SurveyNewsCheckSetting, this.SurveyNewsCheck);
            SaveDateTime(SurveyNewsLastCheckSetting, this.SurveyNewsLastCheck);
            SaveBool(WaitOnNormalExitSetting, this.WaitOnNormalExit);
            SaveBool(WaitOnAbnormalExitSetting, this.WaitOnAbnormalExit);
            SaveBool(EditAndContinueSetting, this.EditAndContinue);
            SaveBool(CheckForLongPathsSetting, this.CheckForLongPaths);
        }
    }
}

