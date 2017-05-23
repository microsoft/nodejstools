// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Telemetry;

namespace Microsoft.NodejsTools.Telemetry
{
    using System;
    using static TelemetryEvents;
    using static TelemetryProperties;

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
            defaultSession.PostUserTask(ProjectImported, TelemetryResult.Success);
        }

        public static void LogDebuggingStarted(string debuggerName, string nodeVersion, bool isProject = true)
        {
            var userTask = new UserTaskEvent(DebbugerStarted, TelemetryResult.Success);
            userTask.Properties[DebuggerEngine] = debuggerName;
            userTask.Properties[NodeVersion] = nodeVersion;
            userTask.Properties[IsProject] = isProject;

            defaultSession.PostEvent(userTask);
        }

        public static void LogSearchNpm(string query)
        {
            var userTask = new UserTaskEvent(SearchNpm, TelemetryResult.Success);
            userTask.Properties[NpmSearchQuery] = query;

            defaultSession.PostEvent(userTask);
        }

        public static void LogInstallNpmPackage(string packageName, string version)
        {
            var userTask = new UserTaskEvent(InstallNpm, TelemetryResult.Success);
            userTask.Properties[NpmPackageName] = packageName;
            userTask.Properties[NpmPackageVersion] = packageName;

            defaultSession.PostEvent(userTask);
        }

        public static void LogUnInstallNpmPackage(string packageName)
        {
            var userTask = new UserTaskEvent(UnInstallNpm, TelemetryResult.Success);
            userTask.Properties[NpmPackageName] = packageName;

            defaultSession.PostEvent(userTask);
        }

        public static void LogUpdateNpmPackage(string packageName, string version)
        {
            var userTask = new UserTaskEvent(UpdateNpm, TelemetryResult.Success);
            userTask.Properties[NpmPackageName] = packageName;
            userTask.Properties[NpmPackageVersion] = version;

            defaultSession.PostEvent(userTask);
        }
    }
}
