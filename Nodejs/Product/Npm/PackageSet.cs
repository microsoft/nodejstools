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

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.NodejsTools.Npm {
    /// <summary>
    /// Immutable collection of packages.
    /// </summary>
    public class PackageSet {
        /// <summary>
        /// Different between two package sets (called L and R for documentation purposes)
        /// </summary>
        public class Diff {
            public static readonly Diff Empty = new Diff(Enumerable.Empty<IPackage>(), Enumerable.Empty<IPackage>());

            private IEnumerable<IPackage> _added;
            private IEnumerable<IPackage> _removed;

            internal Diff(IEnumerable<IPackage> added, IEnumerable<IPackage> removed) {
                _added = added;
                _removed = removed;
            }

            public Diff Concat(Diff other) {
                return new Diff(
                    Added.Concat(other.Added),
                    Removed.Concat(other.Removed));
            }

            /// <summary>
            /// Elements that are in R that are not in L.
            /// </summary>
            public IEnumerable<IPackage> Added { get { return _added; } }

            /// <summary>
            /// Elements that are in L that are in in R.
            /// </summary>
            public IEnumerable<IPackage> Removed { get { return _removed; } }
        }

        public static PackageSet Empty = new PackageSet(Enumerable.Empty<IPackage>());

        private readonly IEnumerable<IPackage> _packages;

        public PackageSet(IEnumerable<IPackage> packages) {
            _packages = packages.Distinct(new PackageComparer());
        }

        public IEnumerable<IPackage> Packages { get { return _packages; } }

        public PackageSet Concat(PackageSet other) {
            return new PackageSet(Packages.Concat(other.Packages));
        }

        /// <summary>
        /// Compare this package set to another package set, returning an added/removed
        /// diff of the two.
        /// </summary>
        /// <param name="other">Package set to compare against.</param>
        public Diff DiffAgainst(PackageSet other) {
            var added = other.Packages.Except(_packages, new PackageComparer());
            var removed = _packages.Except(other.Packages, new PackageComparer());
            return new Diff(added, removed);
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
                if (obj.Name == null || obj.Version == null)
                    return obj.GetHashCode();
                return obj.Name.GetHashCode() ^ obj.Version.GetHashCode();
            }
        }
    }
}
