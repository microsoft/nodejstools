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

namespace Microsoft.NodejsTools.Npm {
    public class PackageSet {
        public class PackageSetDiff {
            private IEnumerable<IPackage> _added;
            private IEnumerable<IPackage> _removed;

            internal PackageSetDiff(IEnumerable<IPackage> added, IEnumerable<IPackage> removed) {
                _added = added;
                _removed = removed;
            }

            public IEnumerable<IPackage> Added { get { return _added; } }
            public IEnumerable<IPackage> Removed { get { return _removed; } }
        }

        private List<IPackage> _packages;

        public PackageSet(List<IPackage> packages) {
            _packages = packages;
        }

        public IReadOnlyList<IPackage> Packages { get { return _packages; } }

        public PackageSetDiff Diff(PackageSet other) {
            var added = other.Packages.Except(_packages, new PackageComparer());
            var removed = _packages.Except(other.Packages, new PackageComparer());
            return new PackageSetDiff(added, removed);
        }

        class PackageComparer : EqualityComparer<IPackage> {
            public override bool Equals(IPackage p1, IPackage p2) {
                return p1.Name == p2.Name
                    && p1.Version == p2.Version
                    && p1.IsBundledDependency == p2.IsBundledDependency
                    && p1.IsDevDependency == p2.IsDevDependency
                    && p1.IsListedInParentPackageJson == p2.IsListedInParentPackageJson
                    && p1.IsMissing == p2.IsMissing
                    && p1.IsOptionalDependency == p2.IsOptionalDependency;
            }

            public override int GetHashCode(IPackage obj) {
                if (obj.Name == null)
                    return obj.GetHashCode();
                return obj.Name.GetHashCode();
            }
        }
    }
}
