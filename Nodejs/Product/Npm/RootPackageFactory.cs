using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm
{
    public static class RootPackageFactory
    {
        public static IRootPackage Create(
            string fullPathToRootDirectory,
            bool showMissingDevOptionalSubPackages = false)
        {
            return new RootPackage(
                fullPathToRootDirectory,
                showMissingDevOptionalSubPackages);
        }
    }
}
