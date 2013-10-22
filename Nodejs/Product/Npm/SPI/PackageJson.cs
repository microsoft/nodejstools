using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class PackageJson : IPackageJson
    {

        private dynamic m_Package;

        public PackageJson( dynamic package )
        {
            m_Package = package;
            Scripts = new Scripts();
        }

        public string Name
        {
            get { return m_Package.name.ToString(); }
        }

        public SemverVersion Version
        {
            get { return SemverVersion.Parse( m_Package.version.ToString() ); }
        }

        public IScripts Scripts { get; private set; }
    }
}
