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
using System.Linq;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal class Package : RootPackage, IPackage{
        private IRootPackage _parent;

        public Package(
            IRootPackage parent,
            string fullPathToRootDirectory,
            bool showMissingDevOptionalSubPackages)
            : base(fullPathToRootDirectory, showMissingDevOptionalSubPackages){
            _parent = parent;
        }

        public string RequestedVersionRange{
            get { return Version.ToString(); }
        }

        public bool IsListedInParentPackageJson{
            get{
                IPackageJson parentPackageJson = _parent.PackageJson;
                return _parent is IGlobalPackages ||
                       (null != parentPackageJson && parentPackageJson.AllDependencies.Contains(Name));
            }
        }

        public bool IsMissing{
            get { return IsListedInParentPackageJson && ! Directory.Exists(Path); }
        }

        public bool IsDevDependency{
            get{
                IPackageJson parentPackageJson = _parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.DevDependencies.Contains(Name);
            }
        }

        public bool IsOptionalDependency{
            get{
                IPackageJson parentPackageJson = _parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.OptionalDependencies.Contains(Name);
            }
        }

        public bool IsBundledDependency{
            get{
                IPackageJson parentPackageJson = _parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.BundledDependencies.Contains(Name);
            }
        }

        public PackageFlags Flags{
            get{
                return (! IsListedInParentPackageJson ? PackageFlags.NotListedAsDependency : 0)
                       | (IsMissing ? PackageFlags.Missing : 0)
                       | (IsDevDependency ? PackageFlags.Dev : 0)
                       | (IsOptionalDependency ? PackageFlags.Optional : 0)
                       | (IsBundledDependency ? PackageFlags.Bundled : 0);
            }
        }

        public override string ToString(){
            return string.Format("{0} {1}", Name, Version);
        }
    }
}