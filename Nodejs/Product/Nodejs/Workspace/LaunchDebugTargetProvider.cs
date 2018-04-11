// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.VSIntegration.Contracts;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Workspace
{
    internal abstract class LaunchDebugTargetProvider
    {
        // debugging property names
        protected const string NodeExeKey = "nodeExe";
        protected const string NodeArgsKey = "nodeArgs";
        protected const string ScriptArgsKey = "scriptArgs";
        protected const string DebuggerPortKey = "debuggerPort";

        protected readonly SVsServiceProvider ServiceProvider;

        protected readonly IVsFolderWorkspaceService WorkspaceService;

        protected LaunchDebugTargetProvider(SVsServiceProvider serviceProvider, IVsFolderWorkspaceService workspaceService)
        {
            this.ServiceProvider = serviceProvider;
            this.WorkspaceService = workspaceService;
        }

        protected string CheckNodeInstalledAndWarn(DebugLaunchActionContext debugLaunchContext)
        {
            var nodeExe = debugLaunchContext.LaunchConfiguration.GetValue(NodeExeKey, defaultValue: Nodejs.GetPathToNodeExecutableFromEnvironment());

            // Similar to the project case, as long as we have a value we're good
            if (!string.IsNullOrEmpty(nodeExe))
            {
                return nodeExe;
            }

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

        protected static string GetJsonConfigurationForInspectProtocol(string target, string workingDir, string nodeExe, DebugLaunchActionContext debugLaunchContext)
        {
            var debuggerPort = debugLaunchContext.LaunchConfiguration.GetValue(DebuggerPortKey, defaultValue: NodejsConstants.DefaultDebuggerPort);
            var runtimeArguments = ConvertArguments(debugLaunchContext.LaunchConfiguration.GetValue<string>(NodeArgsKey, defaultValue: null));
            // If we supply the port argument we also need to manually add --inspect-brk=port to the runtime arguments
            runtimeArguments = runtimeArguments.Append($"--inspect-brk=${debuggerPort}");
            var scriptArguments = ConvertArguments(debugLaunchContext.LaunchConfiguration.GetValue<string>(ScriptArgsKey, defaultValue: null));

            var configuration = new JObject(
                new JProperty("name", "Debug Node.js program from Visual Studio"),
                new JProperty("type", "node2"),
                new JProperty("request", "launch"),
                new JProperty("program", target),
                new JProperty("args", scriptArguments),
                new JProperty("runtimeExecutable", nodeExe),
                new JProperty("runtimeArgs", runtimeArguments),
                new JProperty("port", debuggerPort),
                new JProperty("cwd", workingDir),
                new JProperty("console", "externalTerminal"),
                new JProperty("trace", NodejsProjectLauncher.CheckEnableDiagnosticLoggingOption()),
                new JProperty("sourceMaps", true),
                new JProperty("stopOnEntry", true));

            return configuration.ToString();
        }

        protected static string[] ConvertArguments(string argumentString)
        {
            return argumentString?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        }
    }
}
