// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace NpmTests
{
    [TestClass]
    public class PackageJsonTests : AbstractPackageJsonTests
    {
        private const string PkgSimpleBugs = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0"",
    ""bugs"": ""http://www.mybugtracker.com/""
}";

        private const string PkgMisSpelledAuthor = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0"",
    ""author"": {
        ""misspelledname"": ""Firstname Lastname""
    }
}";

        private const string PkgSingleAuthorField = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0"",
    ""author"": {
        ""name"": ""Firstname Lastname""
    }
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
   ""author"": {
       ""name"": ""Firstname Lastname"",
       ""email"": ""firstname@lastname.com"",
       ""url"": ""http://firstnamelastname.com""
   },
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
   ""author"": {
       ""url"": ""http://firstnamelastname.com"",
       ""name"": ""Firstname Lastname""
   },
   ""keywords"": [
       ""package"",
       ""example"" 
   ],
   ""homepage"": [""http://www.mypackagehomepage.com/""],
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
        public void ReadNoNameNull()
        {
            var pkg = LoadFrom(PkgEmpty);
            Assert.IsNull(pkg.Name, "Name should be null.");
        }

        [TestMethod, Priority(0)]
        public void ReadNoVersionIsZeroed()
        {
            var pkg = LoadFrom(PkgEmpty);
            Assert.AreEqual(new SemverVersion(), pkg.Version, "Empty version mismatch.");
        }

        [TestMethod, Priority(0)]
        public void ReadNameAndVersion()
        {
            var pkgJson = LoadFrom(PkgSimple);

            dynamic json = JsonConvert.DeserializeObject(PkgSimple);

            Assert.AreEqual(json.name.ToString(), pkgJson.Name, "Mismatched package names.");
            Assert.AreEqual(json.version.ToString(), pkgJson.Version.ToString(), "Mismatched version strings.");

            SemverVersionTestHelper.AssertVersionsEqual(0, 1, 0, null, null, pkgJson.Version);
        }

        [TestMethod, Priority(0)]
        public void ReadNoDescriptionNull()
        {
            var pkg = LoadFrom(PkgEmpty);
            Assert.IsNull(pkg.Description, "Description should be null.");
        }

        [TestMethod, Priority(0)]
        public void ReadDescription()
        {
            var pkg = LoadFrom(PkgLargeCompliant);
            Assert.AreEqual(
                "Sample package for CommonJS. This package demonstrates the required elements of a CommonJS package.",
                pkg.Description,
                "Description mismatch.");
        }

        [TestMethod, Priority(0)]
        public void ReadEmptyKeywordsCountZero()
        {
            CheckEmptyArray(LoadFrom(PkgEmpty).Keywords);
        }

        [TestMethod, Priority(0)]
        public void EnumerationOverKeywords()
        {
            CheckStringArrayContents(
                LoadFrom(PkgLargeCompliant).Keywords,
                2,
                new[] { "package", "example" });
        }

        [TestMethod, Priority(0)]
        public void ReadNoHomepageEmpty()
        {
            var pkg = LoadFrom(PkgSimple);
            Assert.AreEqual(0, pkg.Homepages.Count, "Homepage should be empty.");
        }

        [TestMethod, Priority(0)]
        public void ReadHomepageCompliant()
        {
            var pkg = LoadFrom(PkgLargeCompliant);
            CheckStringArrayContents(
                pkg.Homepages,
                1,
                new[] { "http://www.mypackagehomepage.com/" });
        }

        [TestMethod, Priority(0)]
        public void ReadHomepageNonCompliant()
        {
            var pkg = LoadFrom(PkgLargeNonCompliant);
            CheckStringArrayContents(
                pkg.Homepages,
                1,
                new[] { "http://www.mypackagehomepage.com/" });
        }

        [TestMethod, Priority(0)]
        public void ReadEmptyFilesEmpty()
        {
            CheckEmptyArray(LoadFrom(PkgSimple).Files);
        }

        [TestMethod, Priority(0)]
        public void ReadFiles()
        {
            CheckStringArrayContents(
                LoadFrom(PkgLargeCompliant).Files,
                3,
                new[] { "server.js", "customlib.js", "path/to/subfolder" });
        }

        [TestMethod, Priority(0)]
        public void ReadEmptyAuthor()
        {
            Assert.IsNull(LoadFrom(PkgSimple).Author);
        }

        [TestMethod, Priority(0)]
        public void ReadMisspelledAuthor()
        {
            Assert.AreEqual(
                @"{
  ""misspelledname"": ""Firstname Lastname""
}",
                LoadFrom(PkgMisSpelledAuthor).Author.Name
            );
        }

        [TestMethod, Priority(0)]
        public void ReadSingleAuthorField()
        {
            Assert.AreEqual(
                "Firstname Lastname",
                LoadFrom(PkgSingleAuthorField).Author.Name
            );
        }

        [TestMethod, Priority(0)]
        public void ReadAuthorCompliant()
        {
            var compliantAuthor = LoadFrom(PkgLargeCompliant).Author;
            Assert.AreEqual("Firstname Lastname", compliantAuthor.Name);
            Assert.AreEqual("firstname@lastname.com", compliantAuthor.Email);
            Assert.AreEqual("http://firstnamelastname.com", compliantAuthor.Url);
        }

        [TestMethod, Priority(0)]
        public void ReadAuthorNonCompliant()
        {
            var nonCompliantAuthor = LoadFrom(PkgLargeNonCompliant).Author;
            Assert.AreEqual("Firstname Lastname", nonCompliantAuthor.Name);
            Assert.AreEqual("http://firstnamelastname.com", nonCompliantAuthor.Url);
            Assert.IsNull(nonCompliantAuthor.Email);
        }

        //  TODO: authors, contributors, private, main, bin, directories (hash), repository, config, 
    }
}

