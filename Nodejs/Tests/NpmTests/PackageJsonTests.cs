using System;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace NpmTests
{
    [TestClass]
    public class PackageJsonTests
    {
        private const string PkgSimple = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0""
}";
        
        [TestMethod]
        public void TestReadNameAndVersion()
        {
            var source = new MockPackageJsonSource( PkgSimple );
            IPackageJson pkgJson = PackageJsonFactory.Create(source);

            dynamic json = JsonConvert.DeserializeObject(PkgSimple);

            Assert.AreEqual(json.name.ToString(), pkgJson.Name, "Mismatched package names.");
            Assert.AreEqual(json.version.ToString(), pkgJson.Version.ToString(), "Mismatched version strings.");

            SemverVersionTestHelper.AssertVersionsEqual( 0, 1, 0, null, null, pkgJson.Version );
        }

        [TestMethod]
        public void TestGetEmptyScripts()
        {
            IPackageJson    pkg     = PackageJsonFactory.Create(new MockPackageJsonSource(PkgSimple));
            IScripts        scripts = pkg.Scripts;
            Assert.IsNotNull(scripts, "Scripts collection should not be null.");
            Assert.AreEqual(0, scripts.Count, "Shouldn't find any scripts.");
        }
    }
}
