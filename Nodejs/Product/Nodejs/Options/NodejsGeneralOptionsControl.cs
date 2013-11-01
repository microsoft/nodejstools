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
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Options {
    public partial class NodejsGeneralOptionsControl : UserControl {
        private const int SurveyNewsNeverIndex = 0;
        private const int SurveyNewsOnceDayIndex = 1;
        private const int SurveyNewsOnceWeekIndex = 2;
        private const int SurveyNewsOnceMonthIndex = 3;

        public NodejsGeneralOptionsControl() {
            InitializeComponent();

            switch (NodejsPackage.Instance.GeneralOptionsPage.SurveyNewsCheck) {
                case SurveyNewsPolicy.Disabled: _surveyNewsCheckCombo.SelectedIndex = SurveyNewsNeverIndex; break;
                case SurveyNewsPolicy.CheckOnceDay: _surveyNewsCheckCombo.SelectedIndex = SurveyNewsOnceDayIndex; break;
                case SurveyNewsPolicy.CheckOnceWeek: _surveyNewsCheckCombo.SelectedIndex = SurveyNewsOnceWeekIndex; break;
                case SurveyNewsPolicy.CheckOnceMonth: _surveyNewsCheckCombo.SelectedIndex = SurveyNewsOnceMonthIndex; break;
            }
        }

        private void _surveyNewsCheckCombo_SelectedIndexChanged(object sender, EventArgs e) {
            switch (_surveyNewsCheckCombo.SelectedIndex) {
                case SurveyNewsNeverIndex: NodejsPackage.Instance.GeneralOptionsPage.SurveyNewsCheck = SurveyNewsPolicy.Disabled; break;
                case SurveyNewsOnceDayIndex: NodejsPackage.Instance.GeneralOptionsPage.SurveyNewsCheck = SurveyNewsPolicy.CheckOnceDay; break;
                case SurveyNewsOnceWeekIndex: NodejsPackage.Instance.GeneralOptionsPage.SurveyNewsCheck = SurveyNewsPolicy.CheckOnceWeek; break;
                case SurveyNewsOnceMonthIndex: NodejsPackage.Instance.GeneralOptionsPage.SurveyNewsCheck = SurveyNewsPolicy.CheckOnceMonth; break;
            }
        }
    }
}
