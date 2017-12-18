// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Debug;

namespace Microsoft.NodejsTools.Workspace
{
    /// <summary>
    /// This configures the WorkSpace debug command to work as expected for files with a .js extension for both the "Debug", and "Debug and Launch Settings"
    /// menu options.
    /// This class is needed so the AnyCode implementation can figure out we want to debug .js files
    /// using the 'NodeJsFileDebugLaunchTargetProvider'. (Yes, that's a bug).
    /// </summary>
    [ExportLaunchConfigurationProvider(ProviderType, new[] { ".js" }, "nodeDbg")]
    public class NodeJsLaunchConfigurationProvider : ILaunchConfigurationProvider
    {
        private const string ProviderType = "{3911D825-1CA3-4D99-8308-A67C3987A39B}";

        public void CustomizeLaunchConfiguration(DebugLaunchActionContext debugLaunchActionContext, IPropertySettings launchSettings)
        {
            // noop
        }

        public bool IsDebugLaunchActionSupported(DebugLaunchActionContext debugLaunchActionContext) => true;
    }
}
