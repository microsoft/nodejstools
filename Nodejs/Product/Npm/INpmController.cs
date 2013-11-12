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
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm{
    public interface INpmController : INpmLogSource{
        event EventHandler StartingRefresh;
        void Refresh();
        event EventHandler FinishedRefresh;
        IRootPackage RootPackage { get; }
        IGlobalPackages GlobalPackages { get; }
        Task<bool> InstallPackageByVersionAsync(string packageName, string versionRange, DependencyType type);
        Task<bool> InstallGlobalPackageByVersionAsync(string packageName, string versionRange);
        Task<bool> UninstallPackageAsync(string packageName);
        Task<bool> UninstallGlobalPackageAsync(string packageName);
        Task<IEnumerable<IPackage>> SearchAsync(string searchText);
        Task<IEnumerable<IPackage>> GetRepositoryCatalogueAsync();
        Task<bool> UpdatePackagesAsync();
        Task<bool> UpdatePackagesAsync(IEnumerable<IPackage> packages);
    }
}