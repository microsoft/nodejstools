using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NpmInstallCommand : NpmCommand
    {
        public NpmInstallCommand(
            string fullPathToRootPackageDirectory,
            string packageName,
            string versionRange,
            DependencyType type,
            bool global = false,
            string pathToNpm = null) : base(fullPathToRootPackageDirectory, pathToNpm)
        {
            Arguments = string.Format(
                "install {0} -{1}",
                string.IsNullOrEmpty(versionRange) ? packageName : string.Format("{0}@\"{1}\"", packageName, versionRange),
                global
                    ? "g"
                    : (type == DependencyType.Standard
                        ? "-save"
                        : (type == DependencyType.Development ? "-save-dev" : "-save-optional")));
        }
    }
}
