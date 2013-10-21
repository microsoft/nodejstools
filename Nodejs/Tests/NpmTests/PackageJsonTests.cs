using System;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace NpmTests
{
    [TestClass]
    public class PackageJsonTests
    {
        [TestMethod]
        public void TestReadNameAndVersion()
        {
            const string pkgJsonStr = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0""
}";

            var source = new MockPackageJsonSource( pkgJsonStr );
            IPackageJson pkgJson = PackageJsonFactory.Create(source);

            dynamic json = JsonConvert.DeserializeObject(pkgJsonStr);

            Assert.AreEqual(json.name.ToString(), pkgJson.Name, "Mismatched package names.");
            Assert.AreEqual(json.version.ToString(), pkgJson.Version.ToString(), "Mismatched version strings.");

            var expectedVersion = SemverVersion.Parse( json.version.ToString() );
            Assert.AreEqual( expectedVersion.Major, pkgJson.Version.Major, "Mismatched major versions." );
            Assert.AreEqual( expectedVersion.Minor, pkgJson.Version.Minor, "Mismatched minor versions." );
            Assert.AreEqual( expectedVersion.Patch, pkgJson.Version.Patch, "Mismatched patch versions." );
            Assert.AreEqual( expectedVersion, pkgJson.Version, "Mismatch semantic version." );
        }
    }
}
