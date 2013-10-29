using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests
{

    [TestClass]
    public class ModuleHierarchyTests : AbstractFilesystemPackageJsonTests
    {

        private string CreateRootPackage(string json)
        {
            var dir = TempFileManager.GetNewTempDirectory();
            var path = Path.Combine(dir.FullName, "package.json");
            CreatePackageJson(path, json);
            return dir.FullName;
        }

        [TestMethod]
        public void TestReadRootPackageNoDependencies()
        {
            var rootDir = CreateRootPackage(PkgSimple);
            IRootPackage pkg = RootPackageFactory.Create(
                new DirectoryPackageJsonSource(rootDir));
            Assert.IsNotNull(pkg, "Root package should not be null.");
            IPackageJson json = pkg.PackageJson;
            Assert.IsNotNull(json, "package.json should not be null.");
            Assert.AreEqual(json.Name, pkg.Name, "Package name mismatch.");
            Assert.AreEqual(json.Version, pkg.Version, "Package version mismatch.");
            INodeModules modules = pkg.Modules;
            Assert.IsNotNull(modules, "Modules should not be null.");
            Assert.AreEqual(0, modules.Count, "Module count mismatch.");
        }
    }
}
