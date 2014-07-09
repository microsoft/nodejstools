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

namespace Microsoft.NodejsTools.Logging {
    /// <summary>
    /// Defines the list of events which PTVS will log to a INodejsToolsLogger.
    /// </summary>
    public enum NodejsToolsLogEvent {
        /// <summary>
        /// Logs a debug launch.  Data supplied should be 1 or 0 indicating whether
        /// the launch was without debugging or with.
        /// </summary>
        Launch,
        /// <summary>
        /// Logs the frequency at which users check for new Survey\News
        /// 
        /// Data is an int enum mapping to SurveyNews* setting
        /// </summary>
        SurveyNewsFrequency,
        /// <summary>
        /// Logs the analysis detail level
        /// 
        /// Data is an int enum mapping to AnalysisLevel* setting
        /// </summary>
        AnalysisLevel
    }
}
