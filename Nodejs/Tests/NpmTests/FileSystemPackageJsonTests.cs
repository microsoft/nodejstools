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

using System.IO;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests
{
    [TestClass]
    public class FileSystemPackageJsonTests : AbstractPackageJsonTests
    {
        [TestMethod, Priority(0)]
        public void ReadFromFile()
        {
            using (var manager = new TemporaryFileManager())
            {
                var dir = manager.GetNewTempDirectory();
                var path = Path.Combine(dir.FullName, "package.json");
                FilesystemPackageJsonTestHelpers.CreatePackageJson(path, PkgSimple);
                CheckPackage(PackageJsonFactory.Create(new FilePackageJsonSource(path)));
            }
        }

        [TestMethod, Priority(0)]
        public void ReadFromDirectory()
        {
            using (var manager = new TemporaryFileManager())
            {
                var dir = manager.GetNewTempDirectory();
                FilesystemPackageJsonTestHelpers.CreatePackageJson(Path.Combine(dir.FullName, "package.json"), PkgSimple);
                CheckPackage(PackageJsonFactory.Create(new DirectoryPackageJsonSource(dir.FullName)));
            }
        }

        private static void CheckPackage(IPackageJson pkg)
        {
            Assert.IsNotNull(pkg, "Package should not be null.");
            Assert.AreEqual("TestPkg", pkg.Name, "Package name mismatch.");
            Assert.AreEqual(SemverVersion.Parse("0.1.0"), pkg.Version, "Package version mismatch.");
        }
    }
}