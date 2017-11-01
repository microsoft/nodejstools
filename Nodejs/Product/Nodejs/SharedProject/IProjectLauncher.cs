// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Defines an interface for launching a project or a file with or without debugging.
    /// </summary>
    public interface IProjectLauncher
    {
        /// <summary>
        /// Starts a project with or without debugging.
        /// </summary>
        /// <returns>HRESULT indicating success or failure.</returns>
        int LaunchProject(bool debug);

        /// <summary>
        /// Starts a file in a project with or without debugging.
        /// </summary>
        /// <returns>HRESULT indicating success or failure.</returns>
        int LaunchFile(string file, bool debug);
    }
}
