// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.NodejsTools.ProjectWizard
{
    /// <summary>
    /// Provides access to NPM commands
    /// </summary>
    public interface INodePackageModulesCommands
    {
        /// <summary>
        /// Triggers installation of missing node package modules (npm)
        /// </summary>
        Task InstallMissingModulesAsync();
    }
}

