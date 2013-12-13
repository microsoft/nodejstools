/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm {
    public interface INpmCommander : INpmLogSource, IDisposable {

        /// <summary>
        /// Fired whenever a command is completed, regardless of whether or not it was successful.
        /// </summary>
        event EventHandler CommandCompleted;

        /// <summary>
        /// Cancels the currently running command
        /// </summary>
        void CancelCurrentCommand();

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
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> InstallPackageByVersionAsync(
            string packageName,
            string versionRange,
            DependencyType type);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="versionRange"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> InstallGlobalPackageByVersionAsync(
            string packageName,
            string versionRange);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> UninstallPackageAsync(string packageName);

        Task<bool> UninstallGlobalPackageAsync(string packageName);

        Task<IList<IPackage>> SearchAsync(string searchText);

        Task<IPackageCatalog> GetCatalogueAsync(bool forceDownload);

        Task<bool> UpdatePackagesAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packages"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> UpdatePackagesAsync(IEnumerable<IPackage> packages);
    }
}
