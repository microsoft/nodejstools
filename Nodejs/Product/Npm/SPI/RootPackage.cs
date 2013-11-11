using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class RootPackage : IRootPackage
    {

        public RootPackage(
            string fullPathToRootDirectory,
            bool showMissingDevOptionalSubPackages)
        {
            Path = fullPathToRootDirectory;
            PackageJson = PackageJsonFactory.Create(new DirectoryPackageJsonSource(fullPathToRootDirectory));

            Modules = new NodeModules(this, showMissingDevOptionalSubPackages);
        }

        public IPackageJson PackageJson { get; private set; }
        public bool HasPackageJson { get { return null != PackageJson; } }

        public string Name
        {
            get
            {
                return null == PackageJson ? new DirectoryInfo(Path).Name : PackageJson.Name;
            }
        }

        public SemverVersion Version
        {
            get
            {
                return null == PackageJson ? new SemverVersion() : PackageJson.Version;
            }
        }

        public IPerson Author
        {
            get
            {
                return null == PackageJson ? null : PackageJson.Author;
            }
        }

        public string Description
        {
            get
            {
                return null == PackageJson ? null : PackageJson.Description;
            }
        }

        public string Path { get; private set; }

        public INodeModules Modules { get; private set; }
    }
}
