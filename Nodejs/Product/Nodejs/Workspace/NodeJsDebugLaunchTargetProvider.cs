// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Web;
using Microsoft.NodejsTools.Debugger.DebugEngine;
using Microsoft.NodejsTools.Project;
using Microsoft.NodejsTools.Telemetry;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.Extensions.VS.Debug;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;

namespace Microsoft.NodejsTools.Workspace
{
    /// <summary>
    /// Extension to Vs Launch Debugger to handle js files from a Node Js project.
    /// Since VsDebugTargetInfo and VsDebugTargetInfo4 are both structs we can't do anything smart, so there's
    /// a bunch of seemingly duplicated code.
    /// </summary>
    [ExportVsDebugLaunchTarget2(ProviderType, "nodeDbg", ProviderPriority.Highest)]
    internal sealed class NodeJsDebugLaunchTargetProvider : LaunchDebugTargetProvider, IVsDebugLaunchTargetProvider2
    {
        private const string ProviderType = "{91A08414-B8AA-4B09-91D2-0247218C25F4}";

        [ImportingConstructor]
        public NodeJsDebugLaunchTargetProvider(SVsServiceProvider serviceProvider, IVsFolderWorkspaceService workspaceService)
                : base(serviceProvider, workspaceService)
        {
        }

        void IVsDebugLaunchTargetProvider2.SetupDebugTargetInfo(ref VsDebugTargetInfo4 vsDebugTargetInfo, DebugLaunchActionContext debugLaunchContext)
        {
            var nodeExe = CheckNodeInstalledAndWarn(debugLaunchContext);

            var nodeVersion = Nodejs.GetNodeVersion(nodeExe);
            if (nodeVersion >= new Version(8, 0))
            {
                SetupDebugTargetInfoForInspectProtocol(ref vsDebugTargetInfo, debugLaunchContext, nodeExe);
                TelemetryHelper.LogDebuggingStarted("ChromeV2", nodeVersion.ToString(), isProject: false);
            }
            else
            {
                this.SetupDebugTargetInfoForNodeProtocol(ref vsDebugTargetInfo, debugLaunchContext, nodeExe);
                TelemetryHelper.LogDebuggingStarted("Node6", nodeVersion.ToString(), isProject: false);
            }
        }

        private void SetupDebugTargetInfoForNodeProtocol(ref VsDebugTargetInfo4 vsDebugTargetInfo, DebugLaunchActionContext debugLaunchContext, string nodeExe)
        {
            var target = $"\"{vsDebugTargetInfo.bstrExe}\"";
            if (debugLaunchContext.LaunchConfiguration.TryGetValue<string>(ScriptArgsKey, out var scriptArgs) && !string.IsNullOrWhiteSpace(scriptArgs))
            {
                target += " " + scriptArgs;
            }
            vsDebugTargetInfo.bstrArg = target;

            vsDebugTargetInfo.bstrExe = nodeExe;
            if (debugLaunchContext.LaunchConfiguration.TryGetValue<string>(NodeArgsKey, out var nodeArgs) && !string.IsNullOrWhiteSpace(nodeArgs))
            {
                AppendOption(ref vsDebugTargetInfo, AD7Engine.InterpreterOptions, nodeArgs);
            }

            if (debugLaunchContext.LaunchConfiguration.TryGetValue<string>(DebuggerPortKey, out var debuggerPort) && !string.IsNullOrWhiteSpace(debuggerPort))
            {
                AppendOption(ref vsDebugTargetInfo, AD7Engine.DebuggerPort, debuggerPort);
            }
            AppendOption(ref vsDebugTargetInfo, AD7Engine.WaitOnNormalExitSetting, "true"); // todo: make this an option

            vsDebugTargetInfo.guidLaunchDebugEngine = AD7Engine.DebugEngineGuid;
            vsDebugTargetInfo.LaunchFlags = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;

            void AppendOption(ref VsDebugTargetInfo4 dbgInfo, string option, string value)
            {
                if (!string.IsNullOrWhiteSpace(dbgInfo.bstrOptions))
                {
                    dbgInfo.bstrOptions += ";";
                }

                dbgInfo.bstrOptions += option + "=" + HttpUtility.UrlEncode(value);
            }
        }

        private void SetupDebugTargetInfoForInspectProtocol(ref VsDebugTargetInfo4 vsDebugTargetInfo, DebugLaunchActionContext debugLaunchContext, string nodeExe)
        {
            var target = vsDebugTargetInfo.bstrExe;
            var cwd = vsDebugTargetInfo.bstrCurDir;

            var jsonContent = GetJsonConfigurationForInspectProtocol(target, cwd, nodeExe, debugLaunchContext);

            vsDebugTargetInfo.dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            vsDebugTargetInfo.guidLaunchDebugEngine = NodejsProjectLauncher.WebKitDebuggerV2Guid;
            vsDebugTargetInfo.bstrOptions = jsonContent;
            vsDebugTargetInfo.LaunchFlags = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;
        }

        void IVsDebugLaunchTargetProvider2.UpdateContext(DebugLaunchActionContext debugLaunchContext)
        {
            // not needed
        }
    }
}
