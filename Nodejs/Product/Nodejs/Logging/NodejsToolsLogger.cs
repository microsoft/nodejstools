// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Logging
{
    /// <summary>
    /// Main entry point for logging events.  A single instance of this logger is created
    /// by our package and can be used to dispatch log events to all installed loggers.
    /// </summary>
    internal class NodejsToolsLogger
    {
        private readonly INodejsToolsLogger[] _loggers;

        public NodejsToolsLogger(INodejsToolsLogger[] loggers)
        {
            this._loggers = loggers;
        }

        public void LogEvent(NodejsToolsLogEvent logEvent, object data = null)
        {
            foreach (var logger in this._loggers)
            {
                logger.LogEvent(logEvent, data);
            }
        }
    }
}

