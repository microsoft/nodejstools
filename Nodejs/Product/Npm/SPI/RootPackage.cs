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

using System.IO;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal class RootPackage : IRootPackage{
        public RootPackage(
            string fullPathToRootDirectory,
            bool showMissingDevOptionalSubPackages){
            Path = fullPathToRootDirectory;
            PackageJson = PackageJsonFactory.Create(new DirectoryPackageJsonSource(fullPathToRootDirectory));

            Modules = new NodeModules(this, showMissingDevOptionalSubPackages);
        }

        public IPackageJson PackageJson { get; private set; }

        public bool HasPackageJson{
            get { return null != PackageJson; }
        }

        public string Name{
            get { return null == PackageJson ? new DirectoryInfo(Path).Name : PackageJson.Name; }
        }

        public SemverVersion Version{
            get { return null == PackageJson ? new SemverVersion() : PackageJson.Version; }
        }

        public IPerson Author{
            get { return null == PackageJson ? null : PackageJson.Author; }
        }

        public string Description{
            get { return null == PackageJson ? null : PackageJson.Description; }
        }

        public string Path { get; private set; }

        public INodeModules Modules { get; private set; }
    }
}