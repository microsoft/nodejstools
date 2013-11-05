using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal abstract class AbstractNodeModules : INodeModules
    {
        protected readonly List<IPackage> m_PackagesSorted = new List<IPackage>();
        private readonly IDictionary<string, IPackage> m_PackagesByName = new Dictionary<string, IPackage>();

        protected virtual void AddModule(IPackage package)
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

        public IEnumerator<IPackage> GetEnumerator()
        {
            return m_PackagesSorted.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
    }
}
