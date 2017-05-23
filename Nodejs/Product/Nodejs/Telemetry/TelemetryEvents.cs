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

        /// <summary>
        /// User started debugging.
        /// </summary>
        public const string DebbugerStarted = Prefix + "DebuggerStarted";
    }

    internal static class TelemetryProperties
    {
        private const string Prefix = "VS.NodejsTools.";

        /// <summary>
        /// The engine the user is using to debug node. Expected entries are:
        ///  * Node6
        ///  * Chrome
        ///  * ChromeV2
        ///  * None
        /// </summary>
        public const string DebuggerEngine = Prefix + "DebuggerEngine";

        /// <summary>
        /// Wether the user started debugging inside a project or in AnyCode.
        /// </summary>
        public const string IsProject = Prefix + "IsProject";

        /// <summary>
        /// The version of Node the user is using.
        /// </summary>
        public const string NodeVersion = Prefix + "NodeVersion";
    }
}
