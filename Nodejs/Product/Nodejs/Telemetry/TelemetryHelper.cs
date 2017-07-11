// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Telemetry;
using static Microsoft.NodejsTools.Telemetry.TelemetryEvents;
using static Microsoft.NodejsTools.Telemetry.TelemetryProperties;

namespace Microsoft.NodejsTools.Telemetry
{
    /// <summary>
    /// Extensions for logging telemetry events.
    /// </summary>
    internal static class TelemetryHelper
    {
        public static void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var defaultSession = TelemetryService.DefaultSession;
        }

        public static void LogProjectImported()
        {
            TelemetryService.DefaultSession.PostUserTask(ProjectImported, TelemetryResult.Success);
        }

        public static void LogDebuggingStarted(string debuggerName, string nodeVersion, bool isProject = true)
        {
            var userTask = new UserTaskEvent(DebbugerStarted, TelemetryResult.Success);
            userTask.Properties[DebuggerEngine] = debuggerName;
            userTask.Properties[NodeVersion] = nodeVersion;
            userTask.Properties[IsProject] = isProject;

            TelemetryService.DefaultSession.PostEvent(userTask);
        }

        public static void LogSearchNpm()
        {
            TelemetryService.DefaultSession.PostUserTask(SearchNpm, TelemetryResult.Success);
        }

        public static void LogInstallNpmPackage()
        {
            TelemetryService.DefaultSession.PostUserTask(InstallNpm, TelemetryResult.Success);
        }

        public static void LogUnInstallNpmPackage()
        {
            TelemetryService.DefaultSession.PostUserTask(UnInstallNpm, TelemetryResult.Success);
        }

        public static void LogReplUse()
        {
            TelemetryService.DefaultSession.PostUserTask(UsedRepl, TelemetryResult.Success);
        }
    }
}
