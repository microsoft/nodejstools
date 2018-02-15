// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public interface INpmCommander : INpmLogSource, IDisposable
    {
        /// <summary>
        /// Executes npm install to install all packages in package.json.
        /// </summary>
        /// <returns></returns>
        Task<bool> Install();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="versionRange"></param>
        /// <param name="type"></param>
        /// <param name="saveToPackageJson"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> InstallPackageByVersionAsync(
            string packageName,
            string versionRange,
            DependencyType type,
            bool saveToPackageJson);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> UninstallPackageAsync(string packageName);

        Task<bool> UpdatePackagesAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packages"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> UpdatePackagesAsync(IEnumerable<IPackage> packages);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        Task<bool> ExecuteNpmCommandAsync(string arguments, bool showConsole);
    }
}
