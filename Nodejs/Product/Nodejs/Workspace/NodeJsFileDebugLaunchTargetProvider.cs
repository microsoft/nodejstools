// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Debug;
using Microsoft.VisualStudio.Workspace.Extensions.VS.Debug;

namespace Microsoft.NodejsTools.Workspace
{
    /// <summary>
    /// Extension to Vs Launch Debugger to handle js files from a Node Js project.
    /// This class is needed so the AnyCode implementation can figure out we want to debug .js files
    /// using the 'NodeJsFileDebugLaunchTargetProvider'. (Yes, that's a bug).
    /// </summary>
    [ExportVsDebugLaunchTarget(ProviderType, new[] { ".js" }, ProviderPriority.Lowest)]
    internal sealed class NodeJsFileDebugLaunchTargetProvider : IVsDebugLaunchTargetProvider, ILaunchDebugTargetProvider
    {
        private const string ProviderType = "{FD9C2F18-F625-410E-87D9-0B1A459E99DA}";

        public void SetupDebugTargetInfo(ref VsDebugTargetInfo vsDebugTargetInfo, DebugLaunchActionContext debugLaunchContext)
        {
            // not needed
        }

        bool ILaunchDebugTargetProvider.SupportsContext(IWorkspace workspaceContext, string targetFilePath) => true;

        void ILaunchDebugTargetProvider.LaunchDebugTarget(IWorkspace workspaceContext, IServiceProvider serviceProvider, DebugLaunchActionContext debugLaunchActionContext)
        {
            // nope
        }
    }
}
