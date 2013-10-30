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
            string pathToNpm = null) : base(fullPathToRootPackageDirectory, pathToNpm)
        {
            Arguments = string.Format("uninstall {0} --save", packageName);
        }
    }
}
