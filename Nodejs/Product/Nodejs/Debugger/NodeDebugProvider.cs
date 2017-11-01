// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Project;
using Microsoft.NodejsTools.Telemetry;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.Extensions.VS.Debug;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger
{
    /// <summary>
    /// Extension to Vs Launch Debugger to handle js files from a Node Js project
    /// </summary>
    [ExportVsDebugLaunchTarget(ProviderType, new[] { ".js" }, ProviderPriority.Highest)]
    internal class NodeJsDebugLaunchProvider : IVsDebugLaunchTargetProvider
    {
        private const string ProviderType = "6C01D598-DE83-4D5B-B7E5-757FBA8443DD";
        private const string NodeExeKey = "nodeExe";

        private const string NodeJsSchema =
@"{
  ""definitions"": {
    ""nodejs"": {
      ""type"": ""object"",
      ""properties"": {
        ""type"": {""type"": ""string"",""enum"": [ ""nodejs"" ]},
        ""nodeExe"": { ""type"": ""string"" }
      }
    },
    ""nodejsFile"": {
      ""allOf"": [
        { ""$ref"": ""#/definitions/default"" },
        { ""$ref"": ""#/definitions/nodejs"" }
      ]
    }
  },
    ""defaults"": {
        ""nodejs"": { ""$ref"": ""#/definitions/nodejs"" }
    },
    ""configuration"": ""#/definitions/nodejsFile""
}";

        [Import]
        public SVsServiceProvider ServiceProvider { get; set; }

        [Import]
        public IVsFolderWorkspaceService WorkspaceService { get; set; }

        public void SetupDebugTargetInfo(ref VsDebugTargetInfo vsDebugTargetInfo, DebugLaunchActionContext debugLaunchContext)
        {
            var nodeExe = debugLaunchContext.LaunchConfiguration.GetValue(NodeExeKey, defaultValue: Nodejs.GetPathToNodeExecutableFromEnvironment());

            if (string.IsNullOrEmpty(nodeExe))
            {
                var workspace = this.WorkspaceService.CurrentWorkspace;
                workspace.JTF.Run(async () =>
                {
                    await workspace.JTF.SwitchToMainThreadAsync();

                    VsShellUtilities.ShowMessageBox(this.ServiceProvider,
                        string.Format(Resources.NodejsNotInstalledAnyCode, LaunchConfigurationConstants.LaunchJsonFileName),
                        Resources.NodejsNotInstalledShort,
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                });

                // This isn't pretty but the only way to not get an additional
                // dialog box, after the one we show.
                throw new TaskCanceledException();
            }

            var nodeVersion = Nodejs.GetNodeVersion(nodeExe);
            if (nodeVersion >= new Version(8, 0) || NodejsProjectLauncher.CheckDebugProtocolOption())
            {
                SetupDebugTargetInfoForWebkitV2Protocol(ref vsDebugTargetInfo, debugLaunchContext, nodeExe);
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
            var target = vsDebugTargetInfo.bstrExe;
            vsDebugTargetInfo.bstrExe = nodeExe;
            var nodeJsArgs = vsDebugTargetInfo.bstrArg;
            vsDebugTargetInfo.bstrArg = "\"" + target + "\"";
            if (!string.IsNullOrEmpty(nodeJsArgs))
            {
                vsDebugTargetInfo.bstrArg += " ";
                vsDebugTargetInfo.bstrArg += nodeJsArgs;
            }

            vsDebugTargetInfo.clsidCustom = DebugEngine.AD7Engine.DebugEngineGuid;
            vsDebugTargetInfo.bstrOptions = "WAIT_ON_ABNORMAL_EXIT=true";
            vsDebugTargetInfo.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;
        }

        private void SetupDebugTargetInfoForWebkitV2Protocol(ref VsDebugTargetInfo vsDebugTargetInfo, DebugLaunchActionContext debugLaunchContext, string nodeExe)
        {
            // todo: refactor the debugging and process starting so we can re-use

            var target = vsDebugTargetInfo.bstrExe;
            var cwd = Path.GetDirectoryName(target); // Current working directory

            var configuration = new JObject(
                new JProperty("name", "Debug Node.js program from Visual Studio"),
                new JProperty("type", "node2"),
                new JProperty("request", "launch"),
                new JProperty("program", target),
                new JProperty("runtimeExecutable", nodeExe),
                new JProperty("cwd", cwd),
                new JProperty("console", "externalTerminal"),
                new JProperty("trace", NodejsProjectLauncher.CheckEnableDiagnosticLoggingOption()),
                new JProperty("sourceMaps", true),
                new JProperty("stopOnEntry", true));

            var jsonContent = configuration.ToString();

            vsDebugTargetInfo.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            vsDebugTargetInfo.clsidCustom = NodejsProjectLauncher.WebKitDebuggerV2Guid;
            vsDebugTargetInfo.bstrExe = target;
            vsDebugTargetInfo.bstrOptions = jsonContent;
            vsDebugTargetInfo.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;
        }

        [ExportLaunchConfigurationProvider(LaunchConfigurationProviderType, new[] { ".js" }, "nodejs", NodeJsSchema)]
        public class LaunchConfigurationProvider : ILaunchConfigurationProvider
        {
            private const string LaunchConfigurationProviderType = "1DB21619-2C53-4BEF-84E4-B1C4D6771A51";

            public void CustomizeLaunchConfiguration(DebugLaunchActionContext debugLaunchActionContext, IPropertySettings launchSettings)
            {
                // noop
            }

            /// <inheritdoc />
            public bool IsDebugLaunchActionSupported(DebugLaunchActionContext debugLaunchActionContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
