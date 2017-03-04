// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Text;
using Microsoft.NodejsTools.Options;

namespace Microsoft.NodejsTools.Logging
{
    /// <summary>
    /// Keeps track of logged events and makes them available for display in the diagnostics window.
    /// </summary>
    [Export(typeof(INodejsToolsLogger))]
    [Export(typeof(InMemoryLogger))]
    internal class InMemoryLogger : INodejsToolsLogger
    {
        private int _debugLaunchCount, _normalLaunchCount;

        private SurveyNewsPolicy _surveyNewsPolicy;

        #region INodejsToolsLogger Members

        public void LogEvent(NodejsToolsLogEvent logEvent, object argument)
        {
            int val;
            switch (logEvent)
            {
                case NodejsToolsLogEvent.Launch:
                    if ((int)argument != 0)
                    {
                        this._debugLaunchCount++;
                    }
                    else
                    {
                        this._normalLaunchCount++;
                    }
                    break;
                case NodejsToolsLogEvent.SurveyNewsFrequency:
                    val = (int)argument;
                    if (Enum.IsDefined(typeof(SurveyNewsPolicy), val))
                    {
                        this._surveyNewsPolicy = (SurveyNewsPolicy)val;
                    }
                    break;
            }
        }

        #endregion

        public override string ToString()
        {
            var res = new StringBuilder();
            res.AppendLine("    SurveyNewsFrequency: " + this._surveyNewsPolicy);
            res.AppendLine("    Debug Launches: " + this._debugLaunchCount);
            res.AppendLine("    Normal Launches: " + this._normalLaunchCount);
            return res.ToString();
        }
    }
}

