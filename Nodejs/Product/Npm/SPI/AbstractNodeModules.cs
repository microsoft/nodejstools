using System.Collections;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal abstract class AbstractNodeModules : INodeModules{
        protected readonly List<IPackage> _packagesSorted = new List<IPackage>();
        private readonly IDictionary<string, IPackage> _packagesByName = new Dictionary<string, IPackage>();

        protected virtual void AddModule(IPackage package){
            _packagesSorted.Add(package);
            _packagesByName[package.Name] = package;
        }

        public int Count{
            get { return _packagesSorted.Count; }
        }

        public IPackage this[int index]{
            get { return _packagesSorted[index]; }
        }

        public IPackage this[string name]{
            get{
                IPackage pkg;
                _packagesByName.TryGetValue(name, out pkg);
                return pkg;
            }
        }

        public bool Contains(string name){
            return this[name] != null;
        }

        public IEnumerator<IPackage> GetEnumerator(){
            return _packagesSorted.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }
    }
}