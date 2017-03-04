// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject
{
    /// <summary>
    /// Updates the generated file before and/or after the project file is generated.
    /// 
    /// This can insert extra data into the project which is required for proper functioning
    /// of the project system.
    /// 
    /// Classes implementing this interface should be exported with a ProjectExtensionAttribute
    /// specifying which project type the processor applies to.
    /// </summary>
    public interface IProjectProcessor
    {
        /// <summary>
        /// Runs before any test case defined content is added to the project.
        /// 
        /// This should be used to setup must haves for your project system.  Individual
        /// test cases may override your defaults here as appropriate.
        /// </summary>
        void PreProcess(MSBuild.Project project);

        /// <summary>
        /// Runs after all test case defined content is added to the project.
        /// 
        /// This allows any post generation fixups which might be necessary for the project
        /// system.
        /// </summary>
        void PostProcess(MSBuild.Project project);
    }
}

