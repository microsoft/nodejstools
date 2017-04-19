// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using System.Text;

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

        #region INodejsToolsLogger Members

        public void LogEvent(NodejsToolsLogEvent logEvent, object argument)
        {
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
            }
        }

        #endregion

        public override string ToString()
        {
            var res = new StringBuilder();
            res.AppendLine("    Debug Launches: " + this._debugLaunchCount);
            res.AppendLine("    Normal Launches: " + this._normalLaunchCount);
            return res.ToString();
        }
    }
}
