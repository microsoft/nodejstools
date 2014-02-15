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

        [ClassInitialize]
        public static void Init(TestContext context) {
            NodejsTestData.Deploy();
        }

        private TextReader GetCatalogueReader() {
            return new StreamReader(TestData.GetPath(@"TestData\NpmSearchData\npmsearchfullcatalog.txt"));
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
            Assert.AreEqual(expectedAuthor, package.Author.Name, "Invalid author.");
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
            if (expectedIndex >= 0) {
                CheckPackage(
                    packages[expectedIndex],
                    expectedName,
                    expectedDescription,
                    expectedAuthor,
                    expectedPublishDateTime,
                    expectedVersion,
                    expectedKeywords);
            }
            CheckPackage(
                    packagesByName[expectedName],
                    expectedName,
                    expectedDescription,
                    expectedAuthor,
                    expectedPublishDateTime,
                    expectedVersion,
                    expectedKeywords);
        }

        private IList<IPackage> GetTestPackageList(out IDictionary<string, IPackage> byName) {
            IList<IPackage> target = new List<IPackage>();
            IDictionary<string, IPackage> temp = new Dictionary<string, IPackage>();
            INpmSearchLexer lexer = NpmSearchParserFactory.CreateLexer();
            INpmSearchParser parser = NpmSearchParserFactory.CreateParser(lexer);
            parser.Package += (source, args) => {
                target.Add(args.Package);
                temp[args.Package.Name] = args.Package;
            };

            using (var reader = GetCatalogueReader()) {
                lexer.Lex(reader);
            }

            byName = temp;
            return target;
        }

        private IPackageCatalog GetTestPackageCatalog() {
            IDictionary<string, IPackage> byName;
            return new MockPackageCatalog(GetTestPackageList(out byName));
        }
        
        [TestMethod, Priority(0)]
        public void TestParseModuleCatalogue() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(out byName);

            Assert.AreEqual(47365, target.Count, "Unexpected package count in catalogue list.");
            Assert.AreEqual(target.Count, byName.Count, "Number of packages should be same in list and dictionary.");

            //  First package in catelogue
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

            //  Last package in catalogue
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

            //  Version number with build and/or pre-release info
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

            //  Multiple authors listed
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

            //  Equals in description field
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

            //  Author is email address
            //  =            
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

        private IList<IPackage> GetFilteredPackageList(string filterString) {
            var catalog = GetTestPackageCatalog();
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
                } else if (package.Description.ToLower().Contains(filterString)) {
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
                } else if (package.Description.ToLower() == expectedMatch) {
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
