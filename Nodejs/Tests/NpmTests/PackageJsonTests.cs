using System;
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
            IPackageJson pkgJson = PackageJsonFactory.Create( source );

            dynamic json = JsonConvert.DeserializeObject( pkgJsonStr );

            Assert.AreEqual( json.name, pkgJson.Name, "Names should match." );
            Assert.AreEqual( json.version, pkgJson.Version.ToString() );
        }
    }
}
