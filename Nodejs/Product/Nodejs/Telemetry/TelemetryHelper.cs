// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        public static void LogProjectImported()
        {
            TelemetryService.DefaultSession?.PostUserTask(ProjectImported, TelemetryResult.Success);
        }

        public static void LogDebuggingStarted(string debuggerName, string nodeVersion, bool isProject = true)
        {
            LogUserTaskEvent(DebbugerStarted, (DebuggerEngine, debuggerName), (NodeVersion, nodeVersion), (IsProject, isProject));
        }

        public static void LogSearchNpm(bool isProject)
        {
            LogUserTaskEvent(SearchNpm, isProject);
        }

        public static void LogInstallNpmPackage(bool isProject)
        {
            LogUserTaskEvent(InstallNpm, isProject);
        }

        public static void LogUnInstallNpmPackage(bool isProject)
        {
            LogUserTaskEvent(UnInstallNpm, isProject);
        }

        public static void LogReplUse()
        {
            TelemetryService.DefaultSession?.PostUserTask(UsedRepl, TelemetryResult.Success);
        }

        public static void LogTestDiscoveryStarted(string testAdapterName)
        {
            LogUserTaskEvent(TestDiscoveryStarted, (TestAdapterName, testAdapterName));
        }

        public static void LogUserMigratedToJsps()
        {
            LogUserTaskEvent(MigratedToJsps);
        }

        public static void LogUserRevertedBackToNtvs()
        {
            LogUserTaskEvent(RevertedBackToNtvs);
        }

        public static void LogUserTaskEvent(string eventName, bool isProject)
        {
            LogUserTaskEvent(eventName, (IsProject, isProject));
        }

        private static void LogUserTaskEvent(string eventName, params (string PropertName, object Value)[] args)
        {
            if (TelemetryService.DefaultSession != null)
            {
                var userTask = new UserTaskEvent(eventName, TelemetryResult.Success);
                foreach (var (PropertName, Value) in args)
                {
                    userTask.Properties[PropertName] = Value;
                }

                TelemetryService.DefaultSession.PostEvent(userTask);
            }
        }
    }
}
