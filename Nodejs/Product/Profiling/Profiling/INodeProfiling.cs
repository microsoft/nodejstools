// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Profiling
{
    [Guid("1310992E-71FC-4197-B9D7-45003C95CD75")]
    public interface INodeProfiling
    {
        INodeProfileSession GetSession(object item);

        /// <summary>
        /// Launches profiling for the provided project using the projects current settings.
        /// </summary>
        INodeProfileSession LaunchProject(EnvDTE.Project projectToProfile, bool openReport = true);

        /// <summary>
        /// Launches profiling for the provided process.  
        /// </summary>
        /// <param name="interpreter">
        /// A full path to an interpreter..
        /// </param>
        /// <param name="script">
        /// Path to the script to profile.
        /// </param>
        /// <param name="workingDir">Working directory to run script from.</param>
        /// <param name="arguments">Any additional arguments which should be provided to the process.</param>
        INodeProfileSession LaunchProcess(string interpreter, string script, string workingDir, string arguments, bool openReport = true);

        void RemoveSession(INodeProfileSession session, bool deleteFromDisk);

        bool IsProfiling
        {
            get;
        }
    }
}

