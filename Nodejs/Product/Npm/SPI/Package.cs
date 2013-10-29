using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Package : RootPackage, IPackage
    {

        private IRootPackage m_Parent;

        public Package(IRootPackage parent, string fullPathToRootDirectory) : base(fullPathToRootDirectory)
        {
            m_Parent = parent;
        }

        public bool IsDependencyInParentPackageJson
        {
            get
            {
                IPackageJson parentPackageJson = m_Parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.AllDependencies.Contains(Name);
            }
        }

        public bool IsMissing
        {
            get
            {
                return IsDependencyInParentPackageJson && ! Directory.Exists(Path);
            }
        }

        public bool IsDevDependency
        {
            get
            {
                IPackageJson parentPackageJson = m_Parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.DevDependencies.Contains(Name);
            }
        }

        public bool IsOptionalDependency
        {
            get
            {
                IPackageJson parentPackageJson = m_Parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.OptionalDependencies.Contains(Name);
            }
        }

        public bool IsBundledDependency
        {
            get
            {
                IPackageJson parentPackageJson = m_Parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.BundledDependencies.Contains(Name);
            }
        }
    }
}
