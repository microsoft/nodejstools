// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Telemetry
{
    /// <summary>
    /// Extensions for logging telemetry events.
    /// </summary>
    internal static class NodejsTelemetryExtensions
    {
        public static void LogProjectImported(this ITelemetryLogger logger, Guid projectGuid)
        {
            logger.ReportEvent(
                TelemetryEvents.ProjectImported,
                TelemetryProperties.ProjectGuid,
                projectGuid.ToString("B"));
        }
    }
}

