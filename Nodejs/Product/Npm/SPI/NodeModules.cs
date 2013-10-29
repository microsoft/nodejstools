using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NodeModules : INodeModules
    {

        private readonly IList<IPackage> m_PackagesSorted = new List<IPackage>();
        private readonly IDictionary<string, IPackage> m_PackagesByName = new Dictionary<string, IPackage>(); 

        public NodeModules(IRootPackage parent)
        {
            var modulesBase = Path.Combine(parent.Path, "node_modules");
            if (Directory.Exists(modulesBase))
            {
                foreach (var moduleDir in Directory.EnumerateDirectories(modulesBase))
                {
                    AddModule(new Package(parent, moduleDir));
                }
            }

            var parentPackageJson = parent.PackageJson;
            if (null != parentPackageJson)
            {
                foreach (var dependency in parentPackageJson.AllDependencies)
                {
                    if (! Contains(dependency.Name))
                    {
                        AddModule(new Package(parent, Path.Combine(modulesBase, dependency.Name)));
                    }
                }
            }
        }

        private void AddModule(IPackage package)
        {
            m_PackagesSorted.Add(package);
            m_PackagesByName[package.Name] = package;
        }

        public int Count { get { return m_PackagesSorted.Count; } }

        public IPackage this[int index]
        {
            get { return m_PackagesSorted[index]; }
        }

        public IPackage this[string name]
        {
            get
            {
                IPackage pkg;
                m_PackagesByName.TryGetValue(name, out pkg);
                return pkg;
            }
        }

        public bool Contains(string name)
        {
            return this[name] != null;
        }
    }
}