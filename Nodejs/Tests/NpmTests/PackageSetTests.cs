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
        public void DiffShouldReturnEmptyDiffForEmptyPackageSets() {
            var package1 = new PackageSet(new List<IPackage>());
            var package2 = new PackageSet(new List<IPackage>());

            var diff = package1.Diff(package2);
            Assert.IsFalse(diff.Added.Any());
            Assert.IsFalse(diff.Removed.Any());
        }

        [TestMethod]
        public void DiffShouldReturnAllAsAddedWhenEmpty() {
            var packageList = new List<IPackage>() {
                new Mock<IPackage>().Object,
                new Mock<IPackage>().Object,
                new Mock<IPackage>().Object
            };

            var package1 = new PackageSet(new List<IPackage>());
            var package2 = new PackageSet(packageList);

            var diff = package1.Diff(package2);
            AssertUtil.ArrayEquals(packageList, diff.Added);
            Assert.IsFalse(diff.Removed.Any());
        }

        [TestMethod]
        public void DiffShouldReturnAllAsRemovedAgainstWhenDiffedAgainstEmpty() {
            var packageList = new List<IPackage>() {
                new Mock<IPackage>().Object,
                new Mock<IPackage>().Object,
                new Mock<IPackage>().Object
            };

            var package1 = new PackageSet(packageList);
            var package2 = new PackageSet(new List<IPackage>());

            var diff = package1.Diff(package2);
            Assert.IsFalse(diff.Added.Any());
            AssertUtil.ArrayEquals(packageList, diff.Removed);
        }

        [TestMethod]
        public void DiffShouldReturnForPackagesSetsWithSharedPackages() {
            var p1 = new Mock<IPackage>().Object;
            var p2 = new Mock<IPackage>().Object;
            var p3 = new Mock<IPackage>().Object;

            var package1 = new PackageSet(new List<IPackage>() { p1, p2 });
            var package2 = new PackageSet(new List<IPackage>() { p1, p3 });

            var diff = package1.Diff(package2);
            AssertUtil.ArrayEquals(new List<IPackage>() { p3 }, diff.Added);
            AssertUtil.ArrayEquals(new List<IPackage>() { p2 }, diff.Removed);
        }
        
        [TestMethod]
        public void DiffShouldNotMarkPackagesWithSameValuesAsDifferent() {
            var ver = new SemverVersion(1, 2, 3);
            var name = "package";

            var p1 = Mock.Of<IPackage>(x => x.Name == name && x.Version == ver);
            var p2 = Mock.Of<IPackage>();
            var newP1 = Mock.Of<IPackage>(x => x.Name == name && x.Version == ver);

            var package1 = new PackageSet(new List<IPackage>() { p1, p2 });
            var package2 = new PackageSet(new List<IPackage>() { newP1 });

            var diff = package1.Diff(package2);
            Assert.IsFalse(diff.Added.Any());
            AssertUtil.ArrayEquals(new List<IPackage>() { p2 }, diff.Removed);
        }

        [TestMethod]
        public void DiffShouldMarkPackagesWithSameNameButDifferentVersionsAsDifferent() {
            var p1Version = new SemverVersion(1, 2, 3);
            var p2Version = new SemverVersion(1, 2, 4);

            var name = "package";

            var p1 = Mock.Of<IPackage>(x => x.Name == name && x.Version == p1Version);
            var p2 = Mock.Of<IPackage>();
            var newP1 = Mock.Of<IPackage>(x => x.Name == name && x.Version == p2Version);

            var package1 = new PackageSet(new List<IPackage>() { p1, p2 });
            var package2 = new PackageSet(new List<IPackage>() { newP1 });

            var diff = package1.Diff(package2);
            AssertUtil.ArrayEquals(new List<IPackage>() { newP1 }, diff.Added);
            AssertUtil.ArrayEquals(new List<IPackage>() { p1, p2 }, diff.Removed);
        }
    }
}
