// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    [ComVisible(true)]
    internal class CommonProjectConfig : ProjectConfig
    {
        private readonly CommonProjectNode/*!*/ _project;

        public CommonProjectConfig(CommonProjectNode/*!*/ project, string configuration)
            : base(project, configuration)
        {
            this._project = project;
        }

        public override int DebugLaunch(uint flags)
        {
            var starter = this._project.GetLauncher();

            var launchFlags = (__VSDBGLAUNCHFLAGS)flags;
            if ((launchFlags & __VSDBGLAUNCHFLAGS.DBGLAUNCH_NoDebug) == __VSDBGLAUNCHFLAGS.DBGLAUNCH_NoDebug)
            {
                //Start project with no debugger
                return starter.LaunchProject(false);
            }
            else
            {
                //Start project with debugger 
                return starter.LaunchProject(true);
            }
        }
    }
}
