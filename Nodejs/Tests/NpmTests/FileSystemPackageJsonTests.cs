using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests
{

    [TestClass]
    public class FileSystemPackageJsonTests : AbstractPackageJsonTests
    {

        private TemporaryFileManager m_TemporaryFileManager;

        [TestInitialize]
        public void Init()
        {
            m_TemporaryFileManager  = new TemporaryFileManager();
        }

        private void CreatePackageJson(string filename, string json)
        {
            using (var fout = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = new StreamWriter(fout))
                {
                    writer.Write(json);
                }
            }
        }

        private static void CheckPackage(IPackageJson pkg)
        {
            Assert.IsNotNull(pkg, "Package should not be null.");
            Assert.AreEqual("TestPkg", pkg.Name, "Package name mismatch.");
            Assert.AreEqual(SemverVersion.Parse("0.1.0"), pkg.Version, "Package version mismatch.");
        }

        [TestMethod]
        public void TestReadFromFile()
        {
            var dir = m_TemporaryFileManager.GetNewTempDirectory();
            var path = Path.Combine(dir.FullName, "package.json");
            CreatePackageJson(path, PkgSimple);
            CheckPackage(PackageJsonFactory.Create(new FilePackageJsonSource(path)));
        }

        [TestMethod]
        public void TestReadFromDirectory()
        {
            var dir = m_TemporaryFileManager.GetNewTempDirectory();
            CreatePackageJson(Path.Combine(dir.FullName, "package.json"), PkgSimple);
            CheckPackage(PackageJsonFactory.Create(new DirectoryPackageJsonSource(dir.FullName)));
        }

        [TestCleanup]
        public void Cleanup()
        {
            m_TemporaryFileManager.Dispose();
        }
    }
}
