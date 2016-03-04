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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TestUtilities;
using Microsoft.NodejsTools.Npm;
using System.Linq;
using Moq;

namespace NodejsTests {
    [TestClass]
    public class PackageSetTests {
        [TestMethod]
        public void ShouldBeEmptyForEmptySetOfPackages() {
            var p = new PackageSet(Enumerable.Empty<IPackage>());
            Assert.IsFalse(p.Packages.Any());
        }

        [TestMethod]
        public void ConcatShouldJoinAllPackagesWithoutDuplicates() {
            var p1 = Mock.Of<IPackage>();
            var p2 = Mock.Of<IPackage>();
            var p3 = Mock.Of<IPackage>();

            var set1 = new PackageSet(new[] { p1, p2 });
            var set2 = new PackageSet(new[] { p1, p3 });

            var joined = set1.Concat(set2);
            AssertUtil.ArrayEquals(new[] { p1, p2, p3 }, joined.Packages);
        }

        [TestMethod]
        public void DiffShouldReturnEmptyDiffForEmptyPackageSets() {
            var package1 = new PackageSet(Enumerable.Empty<IPackage>());
            var package2 = new PackageSet(Enumerable.Empty<IPackage>());

            var diff = package1.DiffAgainst(package2);
            Assert.IsFalse(diff.Added.Any());
            Assert.IsFalse(diff.Removed.Any());
        }

        [TestMethod]
        public void DiffShouldReturnAllAsAddedWhenEmpty() {
            var packageList = new [] {
                Mock.Of<IPackage>(),
                Mock.Of<IPackage>(),
                Mock.Of<IPackage>()
            };

            var package1 = new PackageSet(Enumerable.Empty<IPackage>());
            var package2 = new PackageSet(packageList);

            var diff = package1.DiffAgainst(package2);
            AssertUtil.ArrayEquals(packageList, diff.Added);
            Assert.IsFalse(diff.Removed.Any());
        }

        [TestMethod]
        public void DiffShouldReturnAllAsRemovedAgainstWhenDiffedAgainstEmpty() {
            var packageList = new [] {
                Mock.Of<IPackage>(),
                Mock.Of<IPackage>(),
                Mock.Of<IPackage>()
            };

            var package1 = new PackageSet(packageList);
            var package2 = new PackageSet(Enumerable.Empty<IPackage>());

            var diff = package1.DiffAgainst(package2);
            Assert.IsFalse(diff.Added.Any());
            AssertUtil.ArrayEquals(packageList, diff.Removed);
        }

        [TestMethod]
        public void DiffShouldReturnForPackagesSetsWithSharedPackages() {
            var p1 = Mock.Of<IPackage>();
            var p2 = Mock.Of<IPackage>();
            var p3 = Mock.Of<IPackage>();

            var package1 = new PackageSet(new [] { p1, p2 });
            var package2 = new PackageSet(new [] { p1, p3 });

            var diff = package1.DiffAgainst(package2);
            AssertUtil.ArrayEquals(new [] { p3 }, diff.Added);
            AssertUtil.ArrayEquals(new [] { p2 }, diff.Removed);
        }
        
        [TestMethod]
        public void DiffShouldNotMarkPackagesWithSameValuesAsDifferent() {
            var ver = new SemverVersion(1, 2, 3);
            var name = "package";

            var p1 = Mock.Of<IPackage>(x => x.Name == name && x.Version == ver);
            var p2 = Mock.Of<IPackage>();
            var newP1 = Mock.Of<IPackage>(x => x.Name == name && x.Version == ver);

            var package1 = new PackageSet(new [] { p1, p2 });
            var package2 = new PackageSet(new [] { newP1 });

            var diff = package1.DiffAgainst(package2);
            Assert.IsFalse(diff.Added.Any());
            AssertUtil.ArrayEquals(new [] { p2 }, diff.Removed);
        }

        [TestMethod]
        public void DiffShouldMarkPackagesWithSameNameButDifferentVersionsAsDifferent() {
            var p1Version = new SemverVersion(1, 2, 3);
            var p2Version = new SemverVersion(1, 2, 4);

            var name = "package";

            var p1 = Mock.Of<IPackage>(x => x.Name == name && x.Version == p1Version);
            var p2 = Mock.Of<IPackage>();
            var newP1 = Mock.Of<IPackage>(x => x.Name == name && x.Version == p2Version);

            var package1 = new PackageSet(new [] { p1, p2 });
            var package2 = new PackageSet(new [] { newP1 });

            var diff = package1.DiffAgainst(package2);
            AssertUtil.ArrayEquals(new [] { newP1 }, diff.Added);
            AssertUtil.ArrayEquals(new [] { p1, p2 }, diff.Removed);
        }
    }
}
