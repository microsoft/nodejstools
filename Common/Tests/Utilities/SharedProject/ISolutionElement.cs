// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject
{
    /// <summary>
    /// Represents a solution element such as a project or solution folder.
    /// </summary>
    public interface ISolutionElement
    {
        /// <summary>
        /// Gets the name of the solution element
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The type guid for the project type or other solution element type such as a folder.
        /// </summary>
        Guid TypeGuid
        {
            get;
        }

        /// <summary>
        /// Gets the flags which control how the solution element is written to the
        /// solution file.
        /// </summary>
        SolutionElementFlags Flags
        {
            get;
        }

        /// <summary>
        /// Saves the solution element to disk at the specified location.  The
        /// impelementor can return the created project or null if the solution
        /// element doesn't create a project.
        /// </summary>
        MSBuild.Project Save(MSBuild.ProjectCollection collection, string location);
    }
}

