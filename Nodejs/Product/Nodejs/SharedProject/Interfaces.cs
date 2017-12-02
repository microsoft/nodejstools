// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Interface for manipulating build dependency
    /// </summary>
    /// <remarks>Normally this should be an internal interface but since it shouldbe available for the aggregator it must be made public.</remarks>
    [ComVisible(true)]
    public interface IBuildDependencyUpdate
    {
        /// <summary>
        /// Defines a container for storing BuildDependencies
        /// </summary>

        IVsBuildDependency[] BuildDependencies
        {
            get;
        }

        /// <summary>
        /// Adds a BuildDependency to the container
        /// </summary>
        /// <param name="dependency">The dependency to add</param>
        void AddBuildDependency(IVsBuildDependency dependency);

        /// <summary>
        /// Removes the builddependency from teh container.
        /// </summary>
        /// <param name="dependency">The dependency to add</param>
        void RemoveBuildDependency(IVsBuildDependency dependency);
    }

    /// <summary>
    /// Provides access to the reference data container.
    /// </summary>
    internal interface IReferenceContainerProvider
    {
        IReferenceContainer GetReferenceContainer();
    }

    /// <summary>
    /// Defines a container for manipulating references
    /// </summary>
    internal interface IReferenceContainer
    {
        IList<ReferenceNode> EnumReferences();
        ReferenceNode AddReferenceFromSelectorData(VSCOMPONENTSELECTORDATA selectorData);
        void LoadReferencesFromBuildProject(MSBuild.Project buildProject);
    }

    /// <summary>
    /// Defines support for single file generator
    /// </summary>
    public interface ISingleFileGenerator
    {
        ///<summary>
        /// Runs the generator on the item represented by the document moniker.
        /// </summary>
        /// <param name="document"></param>
        void RunGenerator(string document);
    }
}
