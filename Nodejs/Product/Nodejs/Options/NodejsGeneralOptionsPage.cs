/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Options {
    [ComVisible(true)]
    public class NodejsGeneralOptionsPage : NodejsDialogPage {
        private SurveyNewsPolicy _surveyNewsCheck;
        private DateTime _surveyNewsLastCheck;
        private string _surveyNewsFeedUrl;
        private string _surveyNewsIndexUrl;
        private NodejsGeneralOptionsControl _window;

        public NodejsGeneralOptionsPage()
            : base("General") {
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window {
            get {
                if (_window == null) {
                    _window = new NodejsGeneralOptionsControl();
                }
                return _window;
            }
        }

        /// <summary>
        /// Indicates whether or not the Output window should be shown when
        /// npm commands are being executed.
        /// </summary>
        public bool ShowOutputWindowWhenExecutingNpm { get; set; }

        /// <summary>
        /// The frequency at which to check for updated news. Default is once
        /// per week.
        /// </summary>
        public SurveyNewsPolicy SurveyNewsCheck {
            get { return _surveyNewsCheck; }
            set { _surveyNewsCheck = value; }
        }

        /// <summary>
        /// The date/time when the last check for news occurred.
        /// </summary>
        public DateTime SurveyNewsLastCheck {
            get { return _surveyNewsLastCheck; }
            set { _surveyNewsLastCheck = value; }
        }

        /// <summary>
        /// The url of the news feed.
        /// </summary>
        public string SurveyNewsFeedUrl {
            get { return _surveyNewsFeedUrl; }
            set { _surveyNewsFeedUrl = value; }
        }

        /// <summary>
        /// The url of the news index page.
        /// </summary>
        public string SurveyNewsIndexUrl {
            get { return _surveyNewsIndexUrl; }
            set { _surveyNewsIndexUrl = value; }
        }

        /// <summary>
        /// Resets settings back to their defaults. This should be followed by
        /// a call to <see cref="SaveSettingsToStorage"/> to commit the new
        /// values.
        /// </summary>
        public override void ResetSettings(){
            ShowOutputWindowWhenExecutingNpm = true;
            _surveyNewsCheck = SurveyNewsPolicy.CheckOnceWeek;
            _surveyNewsLastCheck = DateTime.MinValue;
            _surveyNewsFeedUrl = DefaultSurveyNewsFeedUrl;
            _surveyNewsIndexUrl = DefaultSurveyNewsIndexUrl;
        }

        private const string DefaultSurveyNewsFeedUrl = "http://go.microsoft.com/fwlink/?LinkId=328027";
        private const string DefaultSurveyNewsIndexUrl = "http://go.microsoft.com/fwlink/?LinkId=328029";

        private const string ShowOutputWindowRunningNpm = "ShowOutputWindowRunningNpm";
        private const string SurveyNewsCheckSetting = "SurveyNewsCheck";
        private const string SurveyNewsLastCheckSetting = "SurveyNewsLastCheck";
        private const string SurveyNewsFeedUrlSetting = "SurveyNewsFeedUrl";
        private const string SurveyNewsIndexUrlSetting = "SurveyNewsIndexUrl";

        public override void LoadSettingsFromStorage(){
            ShowOutputWindowWhenExecutingNpm = LoadBool(ShowOutputWindowRunningNpm) ?? true;
            _surveyNewsCheck = LoadEnum<SurveyNewsPolicy>(SurveyNewsCheckSetting) ?? SurveyNewsPolicy.CheckOnceWeek;
            _surveyNewsLastCheck = LoadDateTime(SurveyNewsLastCheckSetting) ?? DateTime.MinValue;
            _surveyNewsFeedUrl = LoadString(SurveyNewsFeedUrlSetting) ?? DefaultSurveyNewsFeedUrl;
            _surveyNewsIndexUrl = LoadString(SurveyNewsIndexUrlSetting) ?? DefaultSurveyNewsIndexUrl;
        }

        public override void SaveSettingsToStorage() {
            SaveBool(ShowOutputWindowRunningNpm, ShowOutputWindowWhenExecutingNpm);
            SaveEnum(SurveyNewsCheckSetting, _surveyNewsCheck);
            SaveDateTime(SurveyNewsLastCheckSetting, _surveyNewsLastCheck);
        }
    }
}
