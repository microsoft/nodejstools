using System;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace NpmTests
{
    [TestClass]
    public class PackageJsonTests
    {

        private const string PkgEmpty = "{}";

        private const string PkgSimple = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0""
}";

        private const string PkgStartScript = @"{
    ""name"": ""ScriptPkg"",
    ""version"": ""1.2.3"",
    ""scripts"": {""start"": ""node server.js""}
}";

        private const string PkgLarge = @"{
   ""name"": ""mypackage"",
   ""version"": ""0.7.0"",
   ""description"": ""Sample package for CommonJS. This package demonstrates the required elements of a CommonJS package."",
   ""keywords"": [
       ""package"",
       ""example"" 
   ],
   ""maintainers"": [
       {
           ""name"": ""Bill Smith"",
           ""email"": ""bills@example.com"",
           ""web"": ""http://www.example.com"" 
       } 
   ],
   ""contributors"": [
       {
           ""name"": ""Mary Brown"",
           ""email"": ""maryb@embedthis.com"",
           ""web"": ""http://www.embedthis.com"" 
       } 
   ],
   ""bugs"": {
       ""mail"": ""dev@example.com"",
       ""web"": ""http://www.example.com/bugs"" 
   },
   ""licenses"": [
       {
           ""type"": ""GPLv2"",
           ""url"": ""http://www.example.org/licenses/gpl.html"" 
       } 
   ],
   ""repositories"": [
       {
           ""type"": ""git"",
           ""url"": ""http://hg.example.com/mypackage.git"" 
       } 
   ],
   ""dependencies"": {
       ""webkit"": ""1.2"",
       ""ssl"": {
           ""gnutls"": [""1.0"", ""2.0""],
           ""openssl"": ""0.9.8"" 
       } 
   },
   ""implements"": [""cjs-module-0.3"", ""cjs-jsgi-0.1""],
   ""os"": [""linux"", ""macos"", ""win""],
   ""cpu"": [""x86"", ""ppc"", ""x86_64""],
   ""engines"": [""v8"", ""ejs"", ""node"", ""rhino""],
   ""scripts"": {
       ""install"": ""install.js"",
       ""uninstall"": ""uninstall.js"",
       ""build"": ""build.js"",
       ""test"": ""test.js"" 
   },
   ""directories"": {
       ""lib"": ""src/lib"",
       ""bin"": ""local/binaries"",
       ""jars"": ""java"" 
   } 
}";

        private IPackageJson LoadFrom(string json)
        {
            return PackageJsonFactory.Create(new MockPackageJsonSource(json));
        }

        [TestMethod]
        public void TestReadNoNameNull()
        {
            var pkg = LoadFrom(PkgEmpty);
            Assert.IsNull(pkg.Name, "Name should be null.");
        }

        [TestMethod]
        public void TestReadNoVersionIsZeroed()
        {
            var pkg = LoadFrom(PkgEmpty);
            Assert.AreEqual(new SemverVersion(), pkg.Version, "Empty version mismatch.");
        }

        [TestMethod]
        public void TestReadNameAndVersion()
        {
            var pkgJson = LoadFrom(PkgSimple);

            dynamic json = JsonConvert.DeserializeObject(PkgSimple);

            Assert.AreEqual(json.name.ToString(), pkgJson.Name, "Mismatched package names.");
            Assert.AreEqual(json.version.ToString(), pkgJson.Version.ToString(), "Mismatched version strings.");

            SemverVersionTestHelper.AssertVersionsEqual( 0, 1, 0, null, null, pkgJson.Version );
        }

        [TestMethod]
        public void TestGetEmptyScripts()
        {
            var pkg     = LoadFrom(PkgSimple);
            var scripts = pkg.Scripts;
            Assert.IsNotNull(scripts, "Scripts collection should not be null.");
            Assert.AreEqual(0, scripts.Count, "Shouldn't find any scripts.");
        }

        [TestMethod]
        public void TestReadSingleStartScript()
        {
            var pkg = LoadFrom(PkgStartScript);
            var scripts = pkg.Scripts;
            Assert.AreEqual(1, scripts.Count, "Should be a single script.");
            IScript start = scripts[ScriptName.Start];
            Assert.IsNotNull(start, "Start script should not be null.");
            Assert.AreEqual(ScriptName.Start, start.Name, "Script name mismatch.");
            Assert.AreEqual("node server.js", start.Code, "Script code mismatch.");
        }

        [TestMethod]
        public void TestReadNonExistentScriptsNull()
        {
            var pkg = LoadFrom(PkgStartScript);
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

        [TestMethod]
        public void TestReadNoDescriptionNull()
        {
            var pkg = LoadFrom(PkgEmpty);
            Assert.IsNull(pkg.Description, "Description should be null.");
        }

        [TestMethod]
        public void TestReadDescription()
        {
            var pkg = LoadFrom(PkgLarge);
            Assert.AreEqual(
                "Sample package for CommonJS. This package demonstrates the required elements of a CommonJS package.",
                pkg.Description,
                "Description mismatch." );
        }
    }
}
