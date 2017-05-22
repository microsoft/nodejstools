// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Telemetry
{
    /// <summary>
    /// Telemetry event names
    /// </summary>
    internal static class TelemetryEvents
    {
        private const string Prefix = "VS/NodejsTools/";

        /// <summary>
        /// User created a new project from existing code.
        /// </summary>
        public const string ProjectImported = Prefix + "ProjectImported";

    }
}
