// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Logging
{
    /// <summary>
    /// Defines the list of events which PTVS will log to a INodejsToolsLogger.
    /// </summary>
    public enum NodejsToolsLogEvent
    {
        /// <summary>
        /// Logs a debug launch.  Data supplied should be 1 or 0 indicating whether
        /// the launch was without debugging or with.
        /// </summary>
        Launch,
        /// <summary>
        /// Logs the analysis detail level
        /// 
        /// Data is an int enum mapping to AnalysisLevel* setting
        /// </summary>
        AnalysisLevel
    }
}
