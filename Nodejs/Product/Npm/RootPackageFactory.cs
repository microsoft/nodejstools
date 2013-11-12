using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm{
    public static class RootPackageFactory{
        public static IRootPackage Create(
            string fullPathToRootDirectory,
            bool showMissingDevOptionalSubPackages = false){
            return new RootPackage(
                fullPathToRootDirectory,
                showMissingDevOptionalSubPackages);
        }

        public static IGlobalPackages Create(
            string fullPathToGlobalPackages){
            return new GlobalPackages(
                fullPathToGlobalPackages);
        }
    }
}