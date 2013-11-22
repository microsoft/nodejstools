/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace NpmTests {
    [TestClass]
    public class PackageJsonTests : AbstractPackageJsonTests {
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
   ""man"" : [ ""./man/foo.1"", ""./man/bar.1"" ],
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
   ""man"" : ""./man/foo.1"",
   ""directories"": {
       ""lib"": ""src/lib"",
       ""bin"": ""local/binaries"",
       ""jars"": ""java"" 
   } 
}";

        [TestMethod, Priority(0)]
        public void TestReadNoNameNull() {
            var pkg = LoadFrom(PkgEmpty);
            Assert.IsNull(pkg.Name, "Name should be null.");
        }

        [TestMethod, Priority(0)]
        public void TestReadNoVersionIsZeroed() {
            var pkg = LoadFrom(PkgEmpty);
            Assert.AreEqual(new SemverVersion(), pkg.Version, "Empty version mismatch.");
        }

        [TestMethod, Priority(0)]
        public void TestReadNameAndVersion() {
            var pkgJson = LoadFrom(PkgSimple);

            dynamic json = JsonConvert.DeserializeObject(PkgSimple);

            Assert.AreEqual(json.name.ToString(), pkgJson.Name, "Mismatched package names.");
            Assert.AreEqual(json.version.ToString(), pkgJson.Version.ToString(), "Mismatched version strings.");

            SemverVersionTestHelper.AssertVersionsEqual(0, 1, 0, null, null, pkgJson.Version);
        }

        [TestMethod, Priority(0)]
        public void TestGetEmptyScripts() {
            var pkg = LoadFrom(PkgSimple);
            var scripts = pkg.Scripts;
            Assert.IsNotNull(scripts, "Scripts collection should not be null.");
            Assert.AreEqual(0, scripts.Count, "Shouldn't find any scripts.");
        }

        [TestMethod, Priority(0)]
        public void TestReadSingleStartScript() {
            var pkg = LoadFrom(PkgStartScript);
            var scripts = pkg.Scripts;
            Assert.AreEqual(1, scripts.Count, "Should be a single script.");
            IScript start = scripts[ScriptName.Start];
            Assert.IsNotNull(start, "Start script should not be null.");
            Assert.AreEqual(ScriptName.Start, start.Name, "Script name mismatch.");
            Assert.AreEqual("node server.js", start.Code, "Script code mismatch.");
        }

        [TestMethod, Priority(0)]
        public void TestReadNonExistentScriptsNull() {
            var pkg = LoadFrom(PkgStartScript);
            var scripts = pkg.Scripts;

            foreach (var name in new[]{
                ScriptName.Install,
                ScriptName.Postinstall,
                ScriptName.Postpublish,
                ScriptName.Postrestart,
                ScriptName.Poststart,
                ScriptName.Poststop,
                ScriptName.Posttest
            }) {
                Assert.IsNull(scripts[name], string.Format("Script '{0}' should be null.", name));
            }
        }

        [TestMethod, Priority(0)]
        public void TestReadNoDescriptionNull() {
            var pkg = LoadFrom(PkgEmpty);
            Assert.IsNull(pkg.Description, "Description should be null.");
        }

        [TestMethod, Priority(0)]
        public void TestReadDescription() {
            var pkg = LoadFrom(PkgLargeCompliant);
            Assert.AreEqual(
                "Sample package for CommonJS. This package demonstrates the required elements of a CommonJS package.",
                pkg.Description,
                "Description mismatch.");
        }

        [TestMethod, Priority(0)]
        public void TestReadEmptyKeywordsCountZero() {
            CheckEmptyArray(LoadFrom(PkgEmpty).Keywords);
        }

        [TestMethod, Priority(0)]
        public void TestEnumerationOverKeywords() {
            CheckStringArrayContents(
                LoadFrom(PkgLargeCompliant).Keywords,
                2,
                new[] { "package", "example" });
        }

        [TestMethod, Priority(0)]
        public void TestReadNoHomepageNull() {
            var pkg = LoadFrom(PkgSimple);
            Assert.IsNull(pkg.Homepage, "Homepage should be null.");
        }

        [TestMethod, Priority(0)]
        public void TestReadHomepage() {
            var pkg = LoadFrom(PkgLargeCompliant);
            Assert.AreEqual("http://www.mypackagehomepage.com/", pkg.Homepage, "Homepage mismatch.");
        }

        [TestMethod, Priority(0)]
        public void TestReadNoBugsNull() {
            var pkg = LoadFrom(PkgSimple);
            Assert.IsNull(pkg.Bugs, "Bugs should be null.");
        }

        [TestMethod, Priority(0)]
        public void TestReadBugsUrlOnly() {
            var pkg = LoadFrom(PkgSimpleBugs);
            var bugs = pkg.Bugs;
            Assert.IsNotNull(bugs, "Bugs should not be null.");
            Assert.AreEqual("http://www.mybugtracker.com/", bugs.Url, "Bugs URL mismatch.");
            Assert.IsNull(bugs.Email, "Bugs email should be null.");
        }

        private void TestReadBugsUrlAndEmail(string json) {
            var pkg = LoadFrom(json);
            var bugs = pkg.Bugs;
            Assert.IsNotNull(bugs, "Bugs should not be null.");
            Assert.AreEqual("http://www.example.com/bugs", bugs.Url, "Bugs URL mismatch.");
            Assert.AreEqual("dev@example.com", bugs.Email, "Bugs email mismatch.");
        }

        [TestMethod, Priority(0)]
        public void TestReadBugsUrlAndEmailCompliant() {
            TestReadBugsUrlAndEmail(PkgLargeCompliant);
        }

        [TestMethod, Priority(0)]
        public void TestReadBugsUrlAndEmailNonCompliant() {
            TestReadBugsUrlAndEmail(PkgLargeNonCompliant);
        }

        [TestMethod, Priority(0)]
        public void TestReadNoLicensesEmpty() {
            var pkg = LoadFrom(PkgSimpleBugs);
            var licenses = pkg.Licenses;
            Assert.IsNotNull(licenses, "Licenses should not be null.");
            Assert.AreEqual(0, licenses.Count, "Should not be any licenses.");
        }

        [TestMethod, Priority(0)]
        public void TestReadLicensesTypeOnly() {
            var pkg = LoadFrom(PkgSingleLicenseType);
            var licenses = pkg.Licenses;
            Assert.AreEqual(1, licenses.Count, "License count mismatch.");
            var license = licenses[0];
            Assert.IsNotNull(license, "License should not be null.");
            Assert.AreEqual("BSD", license.Type, "License type mismatch.");
        }

        [TestMethod, Priority(0)]
        public void ReadLicensesTypeAndUrl() {
            var pkg = LoadFrom(PkgLargeCompliant);
            var licenses = pkg.Licenses;
            Assert.AreEqual(1, licenses.Count, "License count mismatch.");
            var license = licenses[0];
            Assert.IsNotNull(license, "License should not be null.");
            Assert.AreEqual("GPLv2", license.Type, "License type mismatch.");
            Assert.AreEqual("http://www.example.org/licenses/gpl.html", license.Url, "License URL mismatch.");
        }

        [TestMethod, Priority(0)]
        public void TestReadEmptyFilesEmpty() {
            CheckEmptyArray(LoadFrom(PkgSimple).Files);
        }

        [TestMethod, Priority(0)]
        public void TestReadFiles() {
            CheckStringArrayContents(
                LoadFrom(PkgLargeCompliant).Files,
                3,
                new[] { "server.js", "customlib.js", "path/to/subfolder" });
        }

        [TestMethod, Priority(0)]
        public void TestReadEmptyManEmpty() {
            CheckEmptyArray(LoadFrom(PkgSimple).Man);
        }

        [TestMethod, Priority(0)]
        public void TestReadSingleMan() {
            CheckStringArrayContents(
                LoadFrom(PkgLargeNonCompliant).Man,
                1,
                new[] { "./man/foo.1" });
        }

        [TestMethod, Priority(0)]
        public void TestReadMultiMan() {
            CheckStringArrayContents(
                LoadFrom(PkgLargeCompliant).Man,
                2,
                new[] { "./man/foo.1", "./man/bar.1" });
        }

        //  TODO: authors, contributors, private, main, bin, directories (hash), repository, config, 
    }
}