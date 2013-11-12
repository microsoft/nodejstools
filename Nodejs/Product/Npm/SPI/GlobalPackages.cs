namespace Microsoft.NodejsTools.Npm.SPI{
    internal class GlobalPackages : RootPackage, IGlobalPackages{
        public GlobalPackages(string fullPathToRootDirectory) : base(fullPathToRootDirectory, false){}
    }
}