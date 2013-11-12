using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class GlobalPackages : RootPackage, IGlobalPackages
    {
        public GlobalPackages(string fullPathToRootDirectory) : base(fullPathToRootDirectory, false)
        {
        }
    }
}
