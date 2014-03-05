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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;

namespace NpmTests {

    [TestClass]
    public class NpmSearchTests {

        private const string Filename_Original = "npmsearchfullcatalog.txt";
        private const string Filename_Npm143 = "npmsearchfullcat_npm143.txt";
        private const string Filename_Npm144 = "npmsearchfullcat_npm144.txt";

        [ClassInitialize]
        public static void Init(TestContext context) {
            NodejsTestData.Deploy();
        }

        private TextReader GetCatalogueReader(string filename) {
            return new StreamReader(TestData.GetPath(@"TestData\NpmSearchData\" + filename));
        }

        private void CheckPackage(
            IPackage package,
            string expectedName,
            string expectedDescription,
            string expectedAuthor,
            string expectedPublishDateTime,
            SemverVersion expectedVersion,
            IEnumerable<string> expectedKeywords) {

            Assert.AreEqual(expectedName, package.Name, "Invalid name.");
            Assert.AreEqual(expectedDescription, package.Description, "Invalid description.");
            string actualAuthorString = null == package.Author ? null : package.Author.Name;
            Assert.AreEqual(expectedAuthor, actualAuthorString, "Invalid author.");
            Assert.AreEqual(expectedPublishDateTime, package.PublishDateTimeString, "Invalid publish date/time.");
            Assert.AreEqual(expectedVersion, package.Version, "Invalid version.");

            if (null == expectedKeywords) {
                expectedKeywords = new List<string>();
            }

            var actual = new HashSet<string>();
            foreach (var keyword in package.Keywords) {
                actual.Add(keyword);
            }

            //  N.B. I don't check the *number* of keywords because some packages, for example
            //  pdfkit-memory, have a keyword list like this: "pdf pdf writer pdf generator graphics document vector".
            //  Clearly the author's intent is for the keywords to be:
            //  "pdf", "pdf writer", "pdf generator", "graphics", "document", "vector".
            //  However, either because the author misunderstands how keywords work, or because of
            //  a bug in the way npm search reports keywords (for our purposes, it doesn't matter which),
            //  what actually happens is the keyword "pdf" appears three times in the list.
            foreach (var keyword in expectedKeywords) {
                Assert.IsTrue(
                    actual.Contains(keyword),
                    string.Format("Missing keyword: {0}", keyword));
            }
        }

        private void CheckPackage(
            IList<IPackage> packages,
            IDictionary<string, IPackage> packagesByName,
            int expectedIndex,
            string expectedName,
            string expectedDescription,
            string expectedAuthor,
            string expectedPublishDateTime,
            SemverVersion expectedVersion,
            IEnumerable<string> expectedKeywords) {
            CheckPackage(
                packagesByName[expectedName],
                expectedName,
                expectedDescription,
                expectedAuthor,
                expectedPublishDateTime,
                expectedVersion,
                expectedKeywords);

            if (expectedIndex >= 0) {
                for (int index = 0, size = packages.Count; index < size; ++index ) {
                    if (packages[index].Name == expectedName) {
                        Assert.AreEqual(
                            expectedIndex,
                            index,
                            string.Format("Package '{0}' not at expected index in list.", expectedName));
                    }
                }

                CheckPackage(
                    packages[expectedIndex],
                    expectedName,
                    expectedDescription,
                    expectedAuthor,
                    expectedPublishDateTime,
                    expectedVersion,
                    expectedKeywords);
            }
        }

        private IList<IPackage> GetTestPackageList(
            string filename,
            out IDictionary<string, IPackage> byName) {
            IList<IPackage> target = new List<IPackage>();
            INpmSearchLexer lexer = NpmSearchParserFactory.CreateLexer();
            INpmSearchParser parser = NpmSearchParserFactory.CreateParser(lexer);
            parser.Package += (source, args) => target.Add(args.Package);

            using (var reader = GetCatalogueReader(filename)) {
                lexer.Lex(reader);
            }

            //  Do this after because package names can be split across multiple
            //  lines and therefore may change after the IPackage is initially created.
            IDictionary<string, IPackage> temp = new Dictionary<string, IPackage>();
            foreach (var package in target ) {
                temp[package.Name] = package;
            }

            byName = temp;
            return target;
        }

        private IPackageCatalog GetTestPackageCatalog(string filename) {
            IDictionary<string, IPackage> byName;
            return new MockPackageCatalog(GetTestPackageList(filename, out byName));
        }

        [TestMethod, Priority(0)]
        public void NpmPre143_CheckPackageCount() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Original, out byName);
            Assert.AreEqual(47365, target.Count, "Unexpected package count in catalogue list.");
        }

        [TestMethod, Priority(0)]
        public void NpmPre143_CheckListAndDictByNameSameSize() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Original, out byName);
            Assert.AreEqual(target.Count, byName.Count, "Number of packages should be same in list and dictionary.");
        }

        [TestMethod, Priority(0)]
        public void NpmPre143_CheckFirstPackageInCatalog_0() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Original, out byName);
            CheckPackage(
                target,
                byName,
                0,
                "0",
                "A levelup plugin that can be used implements conditional updates.",
                "dominictarr",
                "2013-02-03 06:26",
                new SemverVersion(),
                null);
        }

        [TestMethod, Priority(0)]
        public void NpmPre143_CheckLastPackageInCatalog_zzz() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Original, out byName);
            CheckPackage(
                target,
                byName,
                47364,
                "zzz",
                "Lightweight REST service container",
                "avayanis",
                "2013-03-26 06:15",
                new SemverVersion(0, 2, 0),
                null);
        }

        [TestMethod, Priority(0)]
        public void NpmPre143_CheckPackageWithBuildPreReleaseInfo() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Original, out byName);
            CheckPackage(
                target,
                byName,
                34413,
                "psc-cms-js",
                "js library for Psc CMS (pscheit/psc-cms). shim reposistory for builds.",
                "pscheit",
                "2013-09-30 21:53",
                SemverVersion.Parse("1.3.0-95847e2"),
                new[] { "cms", "framework" });
        }

        [TestMethod, Priority(0)]
        public void NpmPre143_CheckPackageMultipleAuthors() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Original, out byName);
            CheckPackage(
                target,
                byName,
                32499,
                "passport-wsfed-saml2",
                "SAML2 Protocol and WS-Fed library",
                "woloski jfromaniello",
                "2013-09-06 19:09",
                SemverVersion.Parse("0.8.1"),
                new[] { "saml", "wsfed", "passport", "auth0", "azure", "auth", "authn", "authentication", "identity", "adfs" });
        }

        [TestMethod, Priority(0)]
        public void NpmPre143_CheckPackageDescriptionContainsEquals() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Original, out byName);
            CheckPackage(
                target,
                byName,
                32254,
                "particularizable",
                "particularizable ================ `enumerable` was taken.",
                "elliottcable",
                "2013-06-11 22:48",
                SemverVersion.Parse("1.0.0"),
                null);
        }
        
        [TestMethod, Priority(0)]
        public void NpmPre143_CheckPackageAuthorAsEmailAddress() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Original, out byName);

            //  Author is email address
            CheckPackage(
                target,
                byName,
                32705,
                "pdfkit-memory",
                "A PDF generation library for Node.js",
                "trevor@kimenye.com",
                "2012-04-14 12:47",
                SemverVersion.Parse("0.0.2"),
                new[] { "pdf", "writer", "generator", "graphics", "document", "vector" });
        }

        private void CheckSensibleNumberOfNonZeroVersions(ICollection<IPackage> target) {
            int sensibleVersionCount = 0;
            var zero = SemverVersion.Parse("0.0.0");
            foreach (var package in target) {
                if (package.Version != zero) {
                    ++sensibleVersionCount;
                }
            }

            //  Let's say (it'll be much higher) but at least 25% of packages must have a sensible version number
            Assert.IsTrue(
                sensibleVersionCount > target.Count / 4,
                string.Format("There are only {0} packages with version numbers other than {1}", sensibleVersionCount, zero));
        }

        private void CheckOnlyOneOfEachPackage(IEnumerable<IPackage> target) {
            var packageCounts = new Dictionary<string, IList<IPackage>>();
            foreach (IPackage package in target) {
                if (!packageCounts.ContainsKey(package.Name)) {
                    packageCounts[package.Name] = new List<IPackage>();
                }
                packageCounts[package.Name].Add(package);
            }

            var moreThanOne = new List<IList<IPackage>>();
            foreach (string name in packageCounts.Keys) {
                if (packageCounts[name].Count > 1) {
                    moreThanOne.Add(packageCounts[name]);
                }
            }

            if (moreThanOne.Count > 0) {
                var buff = new StringBuilder();
                foreach (var list in moreThanOne) {
                    if (buff.Length > 0) {
                        buff.Append(", ");
                    }
                    buff.Append("[");
                    foreach (var package in list) {
                        if (buff[buff.Length - 1] != '[') {
                            buff.Append(", ");
                        }
                        buff.Append("{\"");
                        buff.Append(package.Name);
                        buff.Append("\", \"");
                        buff.Append(package.Version);
                        buff.Append("\", \"");
                        buff.Append(package.Description);
                        buff.Append("\", \"");
                        buff.Append(package.Author);
                        buff.Append("\", \"");
                        buff.Append(string.Join(" ", package.Keywords));
                        buff.Append("\"}");
                    }
                    buff.Append("]");
                }
                Assert.Fail(string.Format("Multiple package instances found: {0}", buff.ToString()));
            }
        }

        private void CheckNonZeroPackageVersionsExist(string testDataFilename) {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(testDataFilename, out byName);
            CheckSensibleNumberOfNonZeroVersions(target);
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckNonZeroPackageVersionsExist() {
            CheckNonZeroPackageVersionsExist(Filename_Npm143);
        }

        [TestMethod, Priority(0)]
        public void Npm144_CheckNonZeroPackageVersionsExist() {
            CheckNonZeroPackageVersionsExist(Filename_Npm144);
        }

        private void CheckCorrectPackageCount(string testDataFilename, int expectedCount) {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(testDataFilename, out byName);
            Assert.AreEqual(expectedCount, target.Count, "Unexpected package count in catalogue list.");
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckCorrectPackageCount() {
            CheckCorrectPackageCount(Filename_Npm143, 62068);
        }

        [TestMethod, Priority(0)]
        public void Npm144_CheckCorrectPackageCount() {
            CheckCorrectPackageCount(Filename_Npm144, 62208);
        }

        private void CheckNoDuplicatePackages(string testDataFilename) {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(testDataFilename, out byName);
            CheckOnlyOneOfEachPackage(target);
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckNoDuplicatePackages() {
            CheckNoDuplicatePackages(Filename_Npm143);
        }

        [TestMethod, Priority(0)]
        public void Npm144_CheckNoDuplicatePackages() {
            CheckNoDuplicatePackages(Filename_Npm144);
        }

        private void CheckListAndDictByNameSameSize(string testDataFilename) {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(testDataFilename, out byName);
            Assert.AreEqual(target.Count, byName.Count, "Number of packages should be same in list and dictionary.");
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckListAndDictByNameSameSize() {
            CheckListAndDictByNameSameSize(Filename_Npm143);
        }

        [TestMethod, Priority(0)]
        public void Npm144_CheckListAndDictByNameSameSize() {
            CheckListAndDictByNameSameSize(Filename_Npm144);
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckFirstPackageInCatalog_007() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Npm143, out byName);
            CheckPackage(
                target,
                byName,
                0,
                "007",
                "Returns a deep copy of an object with all functions…",
                "btford",
                "2013-07-29",
                new SemverVersion(0, 0, 2),
                new[] { "testing", "test", "mock", "spy" });
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckLastPackageInCatalog_zzz() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Npm143, out byName);
            CheckPackage(
                target,
                byName,
                62067,
                "zzz",
                "Lightweight REST service container",
                "avayanis",
                "2013-11-24",
                new SemverVersion(0, 3, 0),
                null);
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckPackageTruncatedBuildPreReleaseInfo() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Npm143, out byName);

            //  Version number with truncated build and/or pre-release info
            CheckPackage(
                target,
                byName,
                45322,
                "psc-cms-js",
                "js library for Psc CMS (pscheit/psc-cms). shim reposistory…",
                "pscheit",
                "2014-01-07",
                SemverVersion.Parse("1.3.0-9584…"),
                new[] { "cms", "framework" });
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckPackageMultipleAuthorsTruncated() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Npm143, out byName);

            //  Multiple authors listed
            CheckPackage(
                target,
                byName,
                42864,
                "passport-wsfed-saml2",
                "SAML2 Protocol and WS-Fed library",
                "woloski…",
                "2014-02-25",
                SemverVersion.Parse("0.8.7"),
                new[] { "saml", "wsfed", "passport", "auth0", "azure", "auth", "authn", "authentication", "identity", "adfs" });
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckPackageEqualsInDescription() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Npm143, out byName);
            CheckPackage(
                target,
                byName,
                42580,
                "particularizable",
                "particularizable ================ `enumerable` was taken.",
                "elliottcable",
                "2013-06-11",
                SemverVersion.Parse("1.0.0"),
                null);
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckPackageAuthorAsEmailAddress() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Npm143, out byName);
            CheckPackage(
                target,
                byName,
                43119,
                "pdfkit-memory",
                "A PDF generation library for Node.js",
                "trevor@kimenye.com",
                "2012-04-14",
                SemverVersion.Parse("0.0.2"),
                new[] { "pdf", "writer", "generator", "graphics", "document", "vector" });
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckPackageNoDescriptionAuthorVersion() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Npm143, out byName);

            // Packages around and including package with no description, author, or version - I can't believe the npm registry even allows this!
            CheckPackage(
                target,
                byName,
                14,
                "11zwheat",
                "Git powered javascript blog.",
                "sun11",
                "2012-11-17",
                SemverVersion.Parse("0.2.6"),
                null);
            CheckPackage(
                target,
                byName,
                15,
                "122",
                null,
                null,
                "2014-03-03",
                SemverVersion.Parse("0.0.0"),
                null);
            CheckPackage(
                target,
                byName,
                16,
                "123",
                "123",
                "feitian",
                "2013-12-20",
                SemverVersion.Parse("0.0.1"),
                new[] { "123" });
            CheckPackage(
                target,
                byName,
                17,
                "127-ssh",
                "Capture your command not found and tries to ssh",
                "romainberger",
                "2013-12-28",
                SemverVersion.Parse("0.1.2"),
                null);
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckPackageNoVersion() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Npm143, out byName);

            // No version, but everything else, plus package after
            CheckPackage(
                target,
                byName,
                30,
                "2co",
                "Module that will provide nodejs adapters for 2checkout API…",
                "biggora",
                "2012-04-02",
                SemverVersion.Parse("0.0.0"),
                new[] { "payments", "2checkout", "adapter", "gateway" });
            CheckPackage(
                target,
                byName,
                31,
                "2co-client",
                "A low-level HTTP client for the 2checkout API",
                "rakeshpai",
                "2013-11-06",
                SemverVersion.Parse("0.0.12"),
                new[] { "2checkout", "2co", "payment", "payment", "gateway" });
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckPackageNoDescription() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Npm143, out byName);

            // Check packages with and around results with blank description field
            CheckPackage(
                target,
                byName,
                250,
                "activator",
                "simple user activation and password reset for nodejs",
                "deitch",
                "2013-11-10",
                SemverVersion.Parse("0.2.8"),
                new[] { "express", "email", "sms", "activation", "nodejs", "node", "confirmation", "two-step" });
            CheckPackage(
                target,
                byName,
                251,
                "active-client",
                null,
                "s3u",
                "2011-01-03",
                SemverVersion.Parse("0.1.1"),
                null);
            CheckPackage(
                target,
                byName,
                252,
                "active-golf",
                null,
                "sberryman",
                "2014-02-12",
                SemverVersion.Parse("0.0.11"),
                null);
            CheckPackage(
                target,
                byName,
                253,
                "active-markdown",
                "A tool for generating reactive documents from markdown…",
                "alecperkins",
                "2013-05-04",
                SemverVersion.Parse("0.3.2"),
                null);
            CheckPackage(
                target,
                byName,
                254,
                "active-menu",
                "Facilitates the creation of menus in Node applications.",
                "persata",
                "2013-09-22",
                SemverVersion.Parse("0.1.2"),
                new[] { "menu", "ui", "express" });
        }

        [TestMethod, Priority(0)]
        public void Npm143_CheckPackageWithLongName() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(Filename_Npm143, out byName);

            //  This package's name wraps across two lines
            CheckPackage(
                target,
                byName,
                2425,
                "atropa-jasmine-spec-runner-generator-html",
                "A node module to generate Jasmine Spec Runner html pages.",
                "kastor",
                "2013-03-19",
                SemverVersion.Parse("0.1.0-2"),
                new[] { "atropa-jasmine-spec-runner-generator-html", "atropa", "utilities", "jasmine", "test" });
        }

        private IList<IPackage> GetFilteredPackageList(string filterString) {
            var catalog = GetTestPackageCatalog(Filename_Original);
            var filter = PackageCatalogFilterFactory.Create(catalog);
            return filter.Filter(filterString);
        }
            
        [TestMethod, Priority(0)]
        public void TestFilterString() {
            const string filterString = "express";
            var results = GetFilteredPackageList(filterString);
            Assert.IsTrue(results.Count > 0, string.Format("Should be some filter results for '{0}'.", filterString));
            foreach (var package in results) {
                bool match = false;
                if (package.Name.ToLower().Contains(filterString)) {
                    match = true;
                } else if (null != package.Description && package.Description.ToLower().Contains(filterString)) {
                    match = true;
                } else {
                    if (package.Keywords.Any(keyword => keyword.ToLower().Contains(filterString))) {
                        match = true;
                    }
                }

                Assert.IsTrue(match, string.Format("Found no match for filter string '{0}' in package '{1}'.", filterString, package.Name));
            }
        }

        private void CheckRegexFilterResults(string filterString, IList<IPackage> results) {
            const string expectedMatch = "express";
            Assert.IsTrue(results.Count > 0, string.Format("Should be some filter results for '{0}'.", filterString));
            foreach (var package in results) {
                bool match = false;
                if (package.Name.ToLower() == expectedMatch) {
                    match = true;
                } else if (null != package.Description && package.Description.ToLower() == expectedMatch) {
                    match = true;
                } else {
                    if (package.Keywords.Any(keyword => keyword.ToLower() == expectedMatch)) {
                        match = true;
                    }
                }

                Assert.IsTrue(match, string.Format("Found no match for filter regex '{0}' in package '{1}'.", filterString, package.Name));
            }
        }

        private void TestFilterRegex(string filterString) {
            CheckRegexFilterResults(filterString, GetFilteredPackageList(filterString));
        }

        [TestMethod, Priority(0)]
        public void TestFilterRegex() {
            TestFilterRegex("/^express$");
        }

        [TestMethod, Priority(0)]
        public void TestFilterRegexTrailingSlash() {
            TestFilterRegex("/^express$/");
        }
    }
}
