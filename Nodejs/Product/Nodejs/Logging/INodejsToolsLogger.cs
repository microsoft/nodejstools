// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Logging
{
    /// <summary>
    /// Provides an interface for logging events and statistics inside of PTVS.
    /// 
    /// Multiple loggers can be created which send stats to different locations.
    /// 
    /// By default there is one logger which shows the stats in 
    /// Tools->Node.js Tools->Diagnostic Info.
    /// </summary>
    public interface INodejsToolsLogger
    {
        /// <summary>
        /// Informs the logger of an event.  Unknown events should be ignored.
        /// </summary>
        void LogEvent(NodejsToolsLogEvent logEvent, object argument);
    }
}
