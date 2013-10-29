using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class RootPackage : IRootPackage
    {

        private IPackageJson m_PackageJson;

        public RootPackage(IPackageJsonSource packageJsonSource)
        {
            m_PackageJson = PackageJsonFactory.Create(packageJsonSource);
        }

        public IPackageJson PackageJson { get { return m_PackageJson; } }
        public string Name { get { return m_PackageJson.Name; } }
        public SemverVersion Version { get { return m_PackageJson.Version; } }

        public INodeModules Modules
        {
            get
            {
                return new NodeModules();
            }
        }
    }
}
