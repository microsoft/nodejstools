using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm
{
    public class NpmControllerFactory
    {
        public static INpmController Create(string fullPathToRootPackageDirectory)
        {
            return new NpmController(fullPathToRootPackageDirectory);
        }
    }
}
