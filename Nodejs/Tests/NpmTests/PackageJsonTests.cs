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

        private const string PkgStartScript = @"{
    ""name"": ""ScriptPkg"",
    ""version"": ""1.2.3"",
    ""scripts"": {""start"": ""node server.js""}
}";

        
        [TestMethod]
        public void TestReadNameAndVersion()
        {
            var source = new MockPackageJsonSource( PkgSimple );
            var pkgJson = PackageJsonFactory.Create(source);

            dynamic json = JsonConvert.DeserializeObject(PkgSimple);

            Assert.AreEqual(json.name.ToString(), pkgJson.Name, "Mismatched package names.");
            Assert.AreEqual(json.version.ToString(), pkgJson.Version.ToString(), "Mismatched version strings.");

            SemverVersionTestHelper.AssertVersionsEqual( 0, 1, 0, null, null, pkgJson.Version );
        }

        [TestMethod]
        public void TestGetEmptyScripts()
        {
            var pkg     = PackageJsonFactory.Create(new MockPackageJsonSource(PkgSimple));
            var scripts = pkg.Scripts;
            Assert.IsNotNull(scripts, "Scripts collection should not be null.");
            Assert.AreEqual(0, scripts.Count, "Shouldn't find any scripts.");
        }

        [TestMethod]
        public void TestReadSingleStartScript()
        {
            var pkg = PackageJsonFactory.Create(new MockPackageJsonSource(PkgStartScript));
            var scripts = pkg.Scripts;
            Assert.AreEqual(1, scripts.Count, "Should be a single script.");
            IScript start = scripts[ScriptName.Start];
            Assert.IsNotNull(start, "Start script should not be null.");
            Assert.AreEqual(ScriptName.Start, start.Name, "Script name mismatch.");
            Assert.AreEqual("node server.js", start.Code, "Script code mismatch.");
        }

        [TestMethod]
        public void TestReadNonExistentScripts()
        {
            var pkg = PackageJsonFactory.Create(new MockPackageJsonSource(PkgStartScript));
            var scripts = pkg.Scripts;

            foreach (var name in new string[]
            {
                ScriptName.Install,
                ScriptName.Postinstall,
                ScriptName.Postpublish,
                ScriptName.Postrestart,
                ScriptName.Poststart,
                ScriptName.Poststop,
                ScriptName.Posttest
            })
            {
                Assert.IsNull( scripts[ name ], string.Format( "Script '{0}' should be null.", name ) );
            }
        }
    }
}
