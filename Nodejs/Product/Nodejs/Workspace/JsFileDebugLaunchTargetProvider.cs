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
    [ExportVsDebugLaunchTarget(ProviderType, new[] { ".js" }, ProviderPriority.Lowest)]
    internal sealed class JsFileDebugLaunchTargetProvider : LaunchDebugTargetProvider, IVsDebugLaunchTargetProvider, ILaunchDebugTargetProvider
    {
        private const string ProviderType = "{0C231F54-C3EF-4719-9CA5-102B1B63A8DC}";

        [ImportingConstructor]
        public JsFileDebugLaunchTargetProvider(SVsServiceProvider serviceProvider, IVsFolderWorkspaceService workspaceService)
            : base(serviceProvider, workspaceService)
        {
        }

        void IVsDebugLaunchTargetProvider.SetupDebugTargetInfo(ref VsDebugTargetInfo vsDebugTargetInfo, DebugLaunchActionContext debugLaunchContext)
        {
            var nodeExe = CheckNodeInstalledAndWarn(debugLaunchContext);

            var nodeVersion = Nodejs.GetNodeVersion(nodeExe);
            if (nodeVersion >= new Version(8, 0))
            {
                this.SetupDebugTargetInfoForInspectProtocol(ref vsDebugTargetInfo, debugLaunchContext, nodeExe);
                TelemetryHelper.LogDebuggingStarted("ChromeV2", nodeVersion.ToString(), isProject: false);
            }
            else
            {
                this.SetupDebugTargetInfoForNodeProtocol(ref vsDebugTargetInfo, debugLaunchContext, nodeExe);
                TelemetryHelper.LogDebuggingStarted("Node6", nodeVersion.ToString(), isProject: false);
            }
        }

        private void SetupDebugTargetInfoForNodeProtocol(ref VsDebugTargetInfo vsDebugTargetInfo, DebugLaunchActionContext debugLaunchContext, string nodeExe)
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

            vsDebugTargetInfo.clsidCustom = AD7Engine.DebugEngineGuid;
            vsDebugTargetInfo.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;

            void AppendOption(ref VsDebugTargetInfo dbgInfo, string option, string value)
            {
                if (!string.IsNullOrWhiteSpace(dbgInfo.bstrOptions))
                {
                    dbgInfo.bstrOptions += ";";
                }

                dbgInfo.bstrOptions += option + "=" + HttpUtility.UrlEncode(value);
            }
        }

        private void SetupDebugTargetInfoForInspectProtocol(ref VsDebugTargetInfo vsDebugTargetInfo, DebugLaunchActionContext debugLaunchContext, string nodeExe)
        {
            var target = vsDebugTargetInfo.bstrExe;
            var cwd = vsDebugTargetInfo.bstrCurDir;

            var jsonContent = GetJsonConfigurationForInspectProtocol(target, cwd, nodeExe, debugLaunchContext);

            vsDebugTargetInfo.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            vsDebugTargetInfo.clsidCustom = NodejsProjectLauncher.WebKitDebuggerV2Guid;
            vsDebugTargetInfo.bstrOptions = jsonContent;
            vsDebugTargetInfo.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;
        }

        bool ILaunchDebugTargetProvider.SupportsContext(IWorkspace workspaceContext, string targetFilePath) => true;

        void ILaunchDebugTargetProvider.LaunchDebugTarget(IWorkspace workspaceContext, IServiceProvider serviceProvider, DebugLaunchActionContext debugLaunchActionContext)
        {
            // nope
        }
    }
}
