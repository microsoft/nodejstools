using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm{
    public class NpmControllerFactory{
        public static INpmController Create(
            string fullPathToRootPackageDirectory,
            bool showMissingDevOptionalSubPackages = false){
            return new NpmController(
                fullPathToRootPackageDirectory,
                showMissingDevOptionalSubPackages);
        }
    }
}