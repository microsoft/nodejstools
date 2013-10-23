using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Runtime;
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

        private const string PkgSimpleBugs = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0"",
    ""bugs"": ""http://www.mybugtracker.com/""
}";

        private const string PkgSingleLicenseType = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0"",
    ""license"" : ""BSD""
}";

        private const string PkgStartScript = @"{
    ""name"": ""ScriptPkg"",
    ""version"": ""1.2.3"",
    ""scripts"": {""start"": ""node server.js""}
}";

        private const string PkgLargeCompliant = @"{
   ""name"": ""mypackage"",
   ""version"": ""0.7.0"",
   ""description"": ""Sample package for CommonJS. This package demonstrates the required elements of a CommonJS package."",
   ""keywords"": [
       ""package"",
       ""example"" 
   ],
   ""homepage"": ""http://www.mypackagehomepage.com/"",
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
       ""email"": ""dev@example.com"",
       ""url"": ""http://www.example.com/bugs"" 
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
   ""files"" : [""server.js"", ""customlib.js"", ""path/to/subfolder""],
   ""directories"": {
       ""lib"": ""src/lib"",
       ""bin"": ""local/binaries"",
       ""jars"": ""java"" 
   } 
}";

        private const string PkgLargeNonCompliant = @"{
   ""name"": ""mypackage"",
   ""version"": ""0.7.0"",
   ""description"": ""Sample package for CommonJS. This package demonstrates the required elements of a CommonJS package."",
   ""keywords"": [
       ""package"",
       ""example"" 
   ],
   ""homepage"": ""http://www.mypackagehomepage.com/"",
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
            var pkg = LoadFrom(PkgLargeCompliant);
            Assert.AreEqual(
                "Sample package for CommonJS. This package demonstrates the required elements of a CommonJS package.",
                pkg.Description,
                "Description mismatch." );
        }

        [TestMethod]
        public void TestReadEmptyKeywordsCountZero()
        {
            var pkg = LoadFrom(PkgEmpty);
            var keywords = pkg.Keywords;
            Assert.IsNotNull(keywords, "Keywords should not be null - if there are no keywords an empty collection should be returned.");
            Assert.AreEqual(0, keywords.Count, "Keyword count should be 0.");
        }

        private static void CheckContains(ISet<string> retrieved, IEnumerable<string> expected)
        {
            foreach (var value in expected)
            {
                Assert.IsTrue(retrieved.Contains(value), string.Format("Expected to find value '{0}'.", value));
            }
        }

        [TestMethod]
        public void TestEnumerationOverKeywords()
        {
            var expected = new string[] {"package", "example"};
            var pkg = LoadFrom(PkgLargeCompliant);
            var keywords = pkg.Keywords;
            Assert.AreEqual(2, keywords.Count, "Keyword count mismatch.");

            var retrieved = new HashSet<string>();
            
            foreach (var keyword in keywords)
            {
                retrieved.Add(keyword);
            }
            CheckContains(retrieved, expected);

            retrieved = new HashSet<string>();
            for (int index = 0, size = keywords.Count; index < size; ++index)
            {
                retrieved.Add(keywords[index]);
            }
            CheckContains(retrieved, expected);
        }

        [TestMethod]
        public void TestReadNoHomepageNull()
        {
            var pkg = LoadFrom(PkgSimple);
            Assert.IsNull(pkg.Homepage, "Homepage should be null.");
        }

        [TestMethod]
        public void TestReadHomepage()
        {
            var pkg = LoadFrom(PkgLargeCompliant);
            Assert.AreEqual("http://www.mypackagehomepage.com/", pkg.Homepage, "Homepage mismatch.");
        }

        [TestMethod]
        public void TestReadNoBugsNull()
        {
            var pkg = LoadFrom(PkgSimple);
            Assert.IsNull(pkg.Bugs, "Bugs should be null.");
        }

        [TestMethod]
        public void TestReadBugsUrlOnly()
        {
            var pkg = LoadFrom(PkgSimpleBugs);
            var bugs = pkg.Bugs;
            Assert.IsNotNull(bugs, "Bugs should not be null.");
            Assert.AreEqual("http://www.mybugtracker.com/", bugs.Url, "Bugs URL mismatch.");
            Assert.IsNull(bugs.Email, "Bugs email should be null.");
        }

        private void TestReadBugsUrlAndEmail(string json)
        {
            var pkg = LoadFrom(json);
            var bugs = pkg.Bugs;
            Assert.IsNotNull(bugs, "Bugs should not be null.");
            Assert.AreEqual("http://www.example.com/bugs", bugs.Url, "Bugs URL mismatch.");
            Assert.AreEqual("dev@example.com", bugs.Email, "Bugs email mismatch.");
        }

        [TestMethod]
        public void TestReadBugsUrlAndEmailCompliant()
        {
            TestReadBugsUrlAndEmail(PkgLargeCompliant);
        }

        [TestMethod]
        public void TestReadBugsUrlAndEmailNonCompliant()
        {
            TestReadBugsUrlAndEmail(PkgLargeNonCompliant);
        }

        [TestMethod]
        public void TestReadNoLicensesEmpty()
        {
            var pkg = LoadFrom(PkgSimpleBugs);
            var licenses = pkg.Licenses;
            Assert.IsNotNull(licenses, "Licenses should not be null.");
            Assert.AreEqual(0, licenses.Count, "Should not be any licenses.");
        }

        [TestMethod]
        public void TestReadLicensesTypeOnly()
        {
            var pkg = LoadFrom(PkgSingleLicenseType);
            var licenses = pkg.Licenses;
            Assert.AreEqual(1, licenses.Count, "License count mismatch." );
            var license = licenses[0];
            Assert.IsNotNull(license, "License should not be null.");
            Assert.AreEqual("BSD", license.Type, "License type mismatch.");
        }

        [TestMethod]
        public void ReadLicensesTypeAndUrl()
        {
            var pkg = LoadFrom(PkgLargeCompliant);
            var licenses = pkg.Licenses;
            Assert.AreEqual(1, licenses.Count, "License count mismatch.");
            var license = licenses[0];
            Assert.IsNotNull(license, "License should not be null.");
            Assert.AreEqual("GPLv2", license.Type, "License type mismatch.");
            Assert.AreEqual("http://www.example.org/licenses/gpl.html", license.Url, "License URL mismatch.");
        }

        [TestMethod]
        public void TestReadEmptyFilesEmpty()
        {
            var pkg = LoadFrom(PkgSimple);
            var files = pkg.Files;
            Assert.IsNotNull(files, "Files should not be null.");
            Assert.AreEqual(0, files.Count, "Shouldn't be any files listed.");
        }

        [TestMethod]
        public void TestReadFiles()
        {
            var expected = new string[] { "server.js", "customlib.js", "path/to/subfolder" };
            

            var pkg = LoadFrom(PkgLargeCompliant);
            var files = pkg.Files;
            Assert.AreEqual(3, files.Count, "File count mismatch.");

            var retrieved = new HashSet<string>();
            foreach (string file in files)
            {
                retrieved.Add(file);
            }
            CheckContains(retrieved, expected);

            retrieved = new HashSet<string>();
            for (int index = 0, size = files.Count; index < size; ++index)
            {
                retrieved.Add(files[index]);
            }
            CheckContains(retrieved, expected);
        }

        //  TODO: authors, contributors, private
    }
}
