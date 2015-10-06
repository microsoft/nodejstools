//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal abstract class AbstractNodeModules : INodeModules {
        protected readonly List<IPackage> _packagesSorted = new List<IPackage>();
        private readonly IDictionary<string, IPackage> _packagesByName = new Dictionary<string, IPackage>();

        protected virtual void AddModule(IPackage package) {
            if (package.Name != null && !_packagesByName.ContainsKey(package.Name)) {
                _packagesSorted.Add(package);
                _packagesByName[package.Name] = package;
            }
        }

        public int Count {
            get { return _packagesSorted.Count; }
        }

        public IPackage this[int index] {
            get { return _packagesSorted[index]; }
        }

        public IPackage this[string name] {
            get {
                IPackage pkg;
                _packagesByName.TryGetValue(name, out pkg);
                return pkg;
            }
        }

        public bool Contains(string name) {
            return this[name] != null;
        }

        public bool HasMissingModules {
            get {
                foreach (IPackage pkg in this) {
                    if (pkg.IsMissing) {
                        return true;
                    }
                }
                return false;
            }
        }

        public IEnumerator<IPackage> GetEnumerator() {
            return _packagesSorted.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public abstract int GetDepth(string filepath);
    }
}