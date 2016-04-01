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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class Dependencies : IDependencies {
        private IList<JObject> _dependencyProperties;

        public Dependencies(JObject package, params string[] dependencyPropertyNames) {
            _dependencyProperties = dependencyPropertyNames
                .Select(propertyName => package[propertyName] as JObject)
                .Where(x => x != null)
                .ToList();
        }

        public IEnumerator<IDependency> GetEnumerator() {
            foreach (var dependencies in _dependencyProperties) {
                if (dependencies != null) {
                    foreach (var property in dependencies.Properties()) {
                        yield return new Dependency(property.Name, property.Value.Value<string>());
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int Count {
            get { return this.Count(); }
        }

        public IDependency this[string name] {
            get {
                foreach (var dependencies in _dependencyProperties) {
                    var property = dependencies[name];
                    if (null != property) {
                        return new Dependency(name, property.Value<string>());
                    }
                }
                return null;
            }
        }

        public bool Contains(string name) {
            return this[name] != null;
        }
    }
}