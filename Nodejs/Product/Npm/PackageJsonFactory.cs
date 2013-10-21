using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm
{
    public class PackageJsonFactory
    {
        public static IPackageJson Create( IPackageJsonSource source )
        {
            return new PackageJson( source.Package );
        }
    }
}
