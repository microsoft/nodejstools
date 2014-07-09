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

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class PackageProxy : IPackage {
        public INodeModules Modules { get; internal set; }
        public IPackageJson PackageJson { get; internal set; }
        public bool HasPackageJson { get; internal set; }
        public string Name { get; internal set; }
        public SemverVersion Version { get; internal set; }
        public IPerson Author { get; internal set; }
        public string Description { get; internal set; }
        public string Homepage { get; internal set; }
        public string Path { get; internal set; }
        public string PublishDateTimeString { get; internal set; }
        public string RequestedVersionRange { get; internal set; }
        public IEnumerable<string> Keywords { get; internal set; }
        public bool IsListedInParentPackageJson { get; internal set; }
        public bool IsMissing { get; internal set; }
        public bool IsDevDependency { get; internal set; }
        public bool IsOptionalDependency { get; internal set; }
        public bool IsBundledDependency { get; internal set; }
        public PackageFlags Flags { get; internal set; }
    }
}