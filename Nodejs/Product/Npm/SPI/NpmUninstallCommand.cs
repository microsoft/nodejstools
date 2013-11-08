using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NpmUninstallCommand : NpmCommand
    {
        public NpmUninstallCommand(
            string fullPathToRootPackageDirectory,
            string packageName,
            DependencyType type,
            bool global = false,
            string pathToNpm = null) : base(fullPathToRootPackageDirectory, pathToNpm)
        {
            Arguments = global
                ? string.Format("uninstall {0} --g", packageName)
                : string.Format(
                    "uninstall {0} --{1}",
                    packageName,
                    (type == DependencyType.Standard
                        ? "save"
                        : (type == DependencyType.Development ? "save-dev" : "save-optional")));
        }
    }
}
