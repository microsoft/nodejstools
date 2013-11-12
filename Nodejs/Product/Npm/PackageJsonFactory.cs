using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm{
    public class PackageJsonFactory{
        public static IPackageJson Create(IPackageJsonSource source){
            return null == source.Package ? null : new PackageJson(source.Package);
        }
    }
}