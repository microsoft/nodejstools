// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Telemetry;

namespace Microsoft.NodejsTools.Telemetry
{
    /// <summary>
    /// Extensions for logging telemetry events.
    /// </summary>
    internal static class TelemetryHelper
    {
        private static TelemetrySession defaultSession;

        public static void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            defaultSession = TelemetryService.DefaultSession;
        }

        public static void LogProjectImported()
        {
            defaultSession.PostUserTask(TelemetryEvents.ProjectImported, TelemetryResult.Success);
        }
    }
}

