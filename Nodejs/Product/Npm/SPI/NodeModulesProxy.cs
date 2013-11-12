namespace Microsoft.NodejsTools.Npm.SPI{
    internal class NodeModulesProxy : AbstractNodeModules{
        public new void AddModule(IPackage package){
            base.AddModule(package);
        }
    }
}