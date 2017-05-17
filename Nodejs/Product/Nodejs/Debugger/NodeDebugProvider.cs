// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Setup.Configuration;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.Extensions.VS.Debug;
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

        public void SetupDebugTargetInfo(ref VsDebugTargetInfo vsDebugTargetInfo, DebugLaunchActionContext debugLaunchContext)
        {
            var nodeExe = debugLaunchContext.LaunchConfiguration.GetValue<string>(NodeExeKey, defaultValue: Nodejs.GetPathToNodeExecutableFromEnvironment());

            var nodeVersion = Nodejs.GetNodeVersion(nodeExe);
            if (nodeVersion >= new Version(8, 0) || NodejsProjectLauncher.CheckDebugProtocolOption())
            {
                SetupDebugTargetInfoForWebkitV2Protocol(ref vsDebugTargetInfo, debugLaunchContext, nodeExe);
            }
            else
            {
                this.SetupDebugTargetInfoForNodeProtocol(ref vsDebugTargetInfo, debugLaunchContext, nodeExe);
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

            var setupConfiguration = new SetupConfiguration();
            var setupInstance = setupConfiguration.GetInstanceForCurrentProcess();
            var visualStudioInstallationInstanceID = setupInstance.GetInstanceId();

            // The Node2Adapter depends on features only in Node v6+, so the old v5.4 version of node will not suffice for this scenario
            // This node.exe will be the one used by the node2 debug adapter, not the one used to host the user code.
            var pathToNodeExe = Path.Combine(setupInstance.GetInstallationPath(), "JavaScript\\Node.JS\\v6.4.0_x86\\Node.exe");

            // We check the registry to see if any parameters for the node.exe invocation have been specified (like "--inspect"), and append them if we find them.
            var nodeParams = NodejsProjectLauncher.CheckForRegistrySpecifiedNodeParams();
            if (!string.IsNullOrEmpty(nodeParams))
            {
                pathToNodeExe = pathToNodeExe + " " + nodeParams;
            }

            var pathToNode2DebugAdapterRuntime = Environment.ExpandEnvironmentVariables(@"""%ALLUSERSPROFILE%\" +
                    $@"Microsoft\VisualStudio\NodeAdapter\{visualStudioInstallationInstanceID}\extension\out\src\nodeDebug.js""");

            string trimmedPathToNode2DebugAdapter = pathToNode2DebugAdapterRuntime.Replace("\"", "");
            if (!File.Exists(trimmedPathToNode2DebugAdapter))
            {
                pathToNode2DebugAdapterRuntime = Environment.ExpandEnvironmentVariables(@"""%ALLUSERSPROFILE%\" +
                    $@"Microsoft\VisualStudio\NodeAdapter\{visualStudioInstallationInstanceID}\out\src\nodeDebug.js""");
            }

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
                new JProperty("diagnosticLogging", NodejsProjectLauncher.CheckEnableDiagnosticLoggingOption()),
                new JProperty("sourceMaps", true),
                new JProperty("stopOnEntry", true),
                new JProperty("$adapter", pathToNodeExe),
                new JProperty("$adapterArgs", pathToNode2DebugAdapterRuntime));

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

