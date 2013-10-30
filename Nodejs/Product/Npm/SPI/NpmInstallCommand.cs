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
            string pathToNpm = null) : base(fullPathToRootPackageDirectory, pathToNpm)
        {
            Arguments = string.Format(
                "install {0}@\"{1}\" --{2}",
                packageName,
                versionRange,
                type == DependencyType.Standard
                    ? "save"
                    : (type == DependencyType.Development ? "save-dev" : "save-optional"));
        }
    }
}
