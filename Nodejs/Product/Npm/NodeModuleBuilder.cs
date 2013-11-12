using System.Collections.Generic;
using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm{
    /// <summary>
    /// Mutable class for building immutable node module descriptions
    /// </summary>
    public class NodeModuleBuilder{
        private readonly List<IPackage> _dependencies;

        public NodeModuleBuilder(){
            _dependencies = new List<IPackage>();
        }

        public NodeModuleBuilder(IPackage module){
            Name = module.Name;
            Author = module.Author;
            Version = module.Version;
            RequestedVersionRange = module.RequestedVersionRange;
            Description = module.Description;
            Flags = module.Flags;
            _dependencies = new List<IPackage>();
            _dependencies.AddRange(module.Modules);
        }

        public IPerson Author { get; set; }

        public string Name { get; set; }

        public SemverVersion Version { get; set; }

        public string Description { get; set; }

        public IEnumerable<IPackage> Dependencies{
            get { return _dependencies; }
        }

        public PackageFlags Flags { get; set; }

        public string RequestedVersionRange { get; set; }

        public void AddDependency(IPackage module){
            _dependencies.Add(module);
        }

        public void AddDependencies(IEnumerable<IPackage> packages){
            _dependencies.AddRange(packages);
        }

        public IPackage Build(){
            PackageProxy proxy = new PackageProxy();
            proxy.Author = Author;
            proxy.Name = Name;
            proxy.Version = Version;
            proxy.Description = Description;
            proxy.RequestedVersionRange = RequestedVersionRange;
            proxy.Flags = Flags;

            var modules = new NodeModulesProxy();
            foreach (var dep in Dependencies){
                modules.AddModule(dep);
            }
            proxy.Modules = modules;
            return proxy;
        }
    }
}