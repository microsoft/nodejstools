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
using System.ComponentModel.Composition;
using System.Text;
using Microsoft.NodejsTools.Options;


namespace Microsoft.NodejsTools.Logging {
    /// <summary>
    /// Keeps track of logged events and makes them available for display in the diagnostics window.
    /// </summary>
    [Export(typeof(INodejsToolsLogger))]
    [Export(typeof(InMemoryLogger))]
    class InMemoryLogger : INodejsToolsLogger {
        private int _debugLaunchCount, _normalLaunchCount;

        private SurveyNewsPolicy _surveyNewsPolicy;
        private AnalysisLevel _analysisLevel;

        #region INodejsToolsLogger Members

        public void LogEvent(NodejsToolsLogEvent logEvent, object argument) {
            int val;
            switch (logEvent) {
                case NodejsToolsLogEvent.Launch:
                    if ((int)argument != 0) {
                        _debugLaunchCount++;
                    } else {
                        _normalLaunchCount++;
                    }
                    break;
                case NodejsToolsLogEvent.SurveyNewsFrequency:
                    val = (int)argument;
                    if (Enum.IsDefined(typeof(SurveyNewsPolicy), val)) {
                        _surveyNewsPolicy = (SurveyNewsPolicy)val;
                    }
                    break;

                case NodejsToolsLogEvent.AnalysisLevel:
                    val = (int)argument;
                    if (Enum.IsDefined(typeof(AnalysisLevel), val)) {
                        _analysisLevel = (AnalysisLevel)val;
                    }
                    break;
            }
        }

        #endregion

        public override string ToString() {
            StringBuilder res = new StringBuilder();
            res.AppendLine("    Analysis Level: " + _analysisLevel);
            res.AppendLine("    SurveyNewsFrequency: " + _surveyNewsPolicy);
            res.AppendLine("    Debug Launches: " + _debugLaunchCount);
            res.AppendLine("    Normal Launches: " + _normalLaunchCount);
            return res.ToString();
        }
    }
}
