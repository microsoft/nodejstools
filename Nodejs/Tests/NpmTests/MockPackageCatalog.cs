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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;

namespace NpmTests {
    public class MockPackageCatalog : IPackageCatalog {
        private IDictionary<string, IPackage> _byName = new Dictionary<string, IPackage>();
        private IList<IPackage> _results; 
 
        public MockPackageCatalog(IList<IPackage> results) {
            _results = results;
            LastRefreshed = DateTime.Now;

            foreach (var package in results) {
                _byName[package.Name] = package;
            }
        }

        public DateTime LastRefreshed { get; private set; }

        public Task<IEnumerable<IPackage>> GetCatalogPackagesAsync(string filterText) {
            return Task.FromResult(_results.AsEnumerable());
        }

        public IPackage this[string name] {
            get {
                IPackage match;
                _byName.TryGetValue(name, out match);
                return match;
            }
        }

        public long? ResultsCount {
            get { return _results.LongCount(); }
        }
    }
}
