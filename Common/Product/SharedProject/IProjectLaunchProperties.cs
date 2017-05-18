// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Defines an interface for providing launch parameters.
    /// </summary>
    public interface IProjectLaunchProperties
    {
        /// <summary>
        /// Gets the arguments to launch the project with.
        /// </summary>
        string GetArguments();

        /// <summary>
        /// Gets the directory to launch the project in.
        /// </summary>
        string GetWorkingDirectory();

        /// <summary>
        /// Gets the environment variables to set.
        /// </summary>
        /// <param name="includeSearchPaths">
        /// True to also set search path variables.
        /// </param>
        IDictionary<string, string> GetEnvironment(bool includeSearchPaths);
    }
}
