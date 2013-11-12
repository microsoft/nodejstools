using System.IO;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal class NodeModules : AbstractNodeModules{
        public NodeModules(IRootPackage parent, bool showMissingDevOptionalSubPackages){
            var modulesBase = Path.Combine(parent.Path, "node_modules");
            if (Directory.Exists(modulesBase)){
                var bin = string.Format("{0}.bin", Path.DirectorySeparatorChar);
                foreach (var moduleDir in Directory.EnumerateDirectories(modulesBase)){
                    if (! moduleDir.EndsWith(bin)){
                        AddModule(new Package(parent, moduleDir, showMissingDevOptionalSubPackages));
                    }
                }
            }

            var parentPackageJson = parent.PackageJson;
            if (null != parentPackageJson){
                foreach (var dependency in parentPackageJson.AllDependencies){
                    if (! Contains(dependency.Name)){
                        var module = new Package(
                            parent,
                            Path.Combine(modulesBase, dependency.Name),
                            showMissingDevOptionalSubPackages);
                        if (parent as IPackage == null || !module.IsMissing || showMissingDevOptionalSubPackages){
                            AddModule(module);
                        }
                    }
                }
            }

            _packagesSorted.Sort(new PackageComparer());
        }
    }
}