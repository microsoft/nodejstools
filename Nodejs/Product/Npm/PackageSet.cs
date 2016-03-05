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
using System.Linq;

namespace Microsoft.NodejsTools.Npm {
    /// <summary>
    /// Immutable collection of packages.
    /// </summary>
    public class PackageSet : IEnumerable<IPackage> {
        /// <summary>
        /// Different between a left package set and right package set.
        /// </summary>
        public class Diff {
            public static readonly Diff Empty =
                new Diff(
                    PackageSet.Empty,
                    Enumerable.Empty<IPackage>(),
                    Enumerable.Empty<IPackage>());

            private PackageSet _rightPackages;

            private IEnumerable<IPackage> _added;
            private IEnumerable<IPackage> _removed;

            public static Diff Create(PackageSet l, PackageSet r) {
                var added = r.Except(l, new PackageEqualityComparer());
                var removed = l.Except(r, new PackageEqualityComparer());
                return new Diff(r, added, removed);
            }

            private Diff(PackageSet r, IEnumerable<IPackage> added, IEnumerable<IPackage> removed) {
                _rightPackages = r;
                _added = added;
                _removed = removed;
            }

            public Diff Concat(Diff other) {
                return new Diff(
                    NewPackages.Concat(other.NewPackages),
                    Added.Concat(other.Added),
                    Removed.Concat(other.Removed));
            }

            /// <summary>
            /// Set of packages in R.
            /// </summary>
            public PackageSet NewPackages { get { return _rightPackages; } }

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
            _packages = packages.Distinct(new PackageEqualityComparer());
        }

        public IEnumerator<IPackage> GetEnumerator() {
            return _packages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _packages.GetEnumerator();
        }

        public PackageSet Concat(PackageSet other) {
            return new PackageSet(_packages.Concat(other));
        }

        /// <summary>
        /// Compare this package set to another package set, returning an added/removed
        /// diff of the two.
        /// </summary>
        /// <param name="other">Package set to compare against.</param>
        public Diff DiffAgainst(PackageSet other) {
            return Diff.Create(this, other);
        }

        public Diff DiffAgainst(IEnumerable<IPackage> other) {
            return DiffAgainst(new PackageSet(other));
        }
    }
}
