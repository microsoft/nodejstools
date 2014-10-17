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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Npm.SPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NpmTests.TestUtilities;

namespace NpmTests {

    [TestClass]
    [DeploymentItem(@"TestData\NpmSearchData\", "NpmSearchData")]
    [DeploymentItem(@"sqlite3.dll")]
    public class NpmSearchTests {

        private const string PackageCacheAllJsonFilename = "packagecache.json";
        private const string PackageCacheSinceJsonFilename = "since_packages.json";
        private const string PackageCacheDatabaseFilename = @"NpmSearchData\packagecache.sqlite";

        [ClassInitialize]
        public static void Init(TestContext context) {
            AssertListener.Initialize();
        }

        private TextReader GetCatalogueReader(string filename) {
            return new StreamReader(string.Format(@"NpmSearchData\{0}", filename));
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

            target = NpmGetCatalogCommand.ReadResultsFromDatabase(filename);

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
        public void CheckDatabaseCreation() {
            string databaseFilename = "packagecache_create.sqlite";
            using (var reader = GetCatalogueReader(PackageCacheAllJsonFilename)) {
                new NpmGetCatalogCommand(string.Empty, null, false).ParseResultsAndAddToDatabase(reader, databaseFilename);
            }

            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(databaseFilename, out byName);

            Assert.AreEqual(89924, target.Count);

            CheckPackage(
                target,
                byName,
                13890,
                "cordova",
                "Cordova command line interface tool",
                "Anis Kadri",
                "07/08/2014 17:55:34",
                SemverVersion.Parse("3.5.0-0.2.6"),
                new[] { "cordova", "client", "cli" }
                );
        }

        [TestMethod, Priority(0)]
        public void CheckDatabaseUpdate() {
            string databaseCopyFilename = "packagecache_update.sqlite";
            File.Copy(PackageCacheDatabaseFilename, databaseCopyFilename);
            using (var reader = GetCatalogueReader(PackageCacheSinceJsonFilename)) {
                new NpmGetCatalogCommand(string.Empty, null, false).ParseResultsAndAddToDatabase(reader, databaseCopyFilename);
            }

            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(databaseCopyFilename, out byName);

            Assert.AreEqual(104627, target.Count);

            // Package updated successfully
            CheckPackage(
                target,
                byName,
                16086,
                "cordova",
                "Cordova command line interface tool",
                "Anis Kadri",
                "10/16/2014 18:05:13",
                SemverVersion.Parse("4.0.0"),
                new[] { "cordova", "client", "cli"}
                );

            // Package added successfully
            CheckPackage(
                target,
                byName,
                62899,
                "mytestpackage98",
                null,
                null,
                "08/14/2014 19:46:24",
                SemverVersion.Parse("0.1.3"),
                new string[0]
                );

        }
        
        [TestMethod, Priority(0)]
        public void CheckPackageWithBuildPreReleaseInfo() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDatabaseFilename, out byName);
            CheckPackage(
                target,
                byName,
                65900,
                "psc-cms-js",
                "js library for Psc CMS (pscheit/psc-cms). shim reposistory for builds.",
                "Philipp Scheit",
                "01/07/2014 10:57:58",
                SemverVersion.Parse("1.3.0-517056d"),
                new[] { "cms", "framework" });
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

        [TestMethod, Priority(0)]
        public void CheckNonZeroPackageVersionsExist() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDatabaseFilename, out byName);
            CheckSensibleNumberOfNonZeroVersions(target);
        }

        [TestMethod, Priority(0)]
        public void CheckCorrectPackageCount() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDatabaseFilename, out byName);
            Assert.AreEqual(89924, target.Count, "Unexpected package count in catalogue list.");
        }

        [TestMethod, Priority(0)]
        public void CheckNoDuplicatePackages() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDatabaseFilename, out byName);
            CheckOnlyOneOfEachPackage(target);
        }

        [TestMethod, Priority(0)]
        public void CheckListAndDictByNameSameSize() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDatabaseFilename, out byName);
            Assert.AreEqual(target.Count, byName.Count, "Number of packages should be same in list and dictionary.");
        }
        
        [TestMethod, Priority(0)]
        public void CheckFirstPackageInCatalog() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDatabaseFilename, out byName);
            CheckPackage(
                target,
                byName,
                0,
                "0",
                null,
                null,
                "06/17/2014 06:38:43",
                new SemverVersion(0, 0, 0),
                new string[] { });
        }

        [TestMethod, Priority(0)]
        public void CheckLastPackageInCatalog_zzz() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDatabaseFilename, out byName);
            CheckPackage(
                target,
                byName,
                89923,
                "z_test",
                "zhanghao test",
                "zhanghao",
                "01/02/2014 03:28:23",
                new SemverVersion(1, 0, 0),
                null);
        }

        [TestMethod, Priority(0)]
        public void CheckPackageEqualsInDescription() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDatabaseFilename, out byName);
            CheckPackage(
                target,
                byName,
                62060,
                "particularizable",
                "particularizable ================ `enumerable` was taken.",
                "ELLIOTTCABLE",
                "06/11/2013 22:48:35",
                SemverVersion.Parse("1.0.0"),
                null);
        }

        [TestMethod, Priority(0)]
        public void CheckPackageNoDescriptionAuthorVersion() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDatabaseFilename, out byName);

            CheckPackage(
                target,
                byName,
                25,
                "122",
                null,
                null,
                "03/03/2014 16:52:16",
                SemverVersion.Parse("0.0.0"),
                null);
        }

        [TestMethod, Priority(0)]
        public void CheckPackageNoVersion() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDatabaseFilename, out byName);

            CheckPackage(
                target,
                byName,
                56,
                "2co",
                "Module that will provide nodejs adapters for 2checkout API payment gateway",
                "Aleksej Gordejev",
                "04/21/2014 14:31:49",
                SemverVersion.Parse("0.0.4"),
                new[] { "payments", "2checkout", "adapter", "gateway" });
        }

        [TestMethod, Priority(0)]
        public void CheckPackageNoDescription() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDatabaseFilename, out byName);

            CheckPackage(
                target,
                byName,
                455,
                "active-client",
                null,
                "Subbu Allamaraju",
                "01/03/2011 21:21:12",
                SemverVersion.Parse("0.1.1"),
                null);
        }

        private IList<IPackage> GetFilteredPackageList(string filterString) {
            var catalog = GetTestPackageCatalog(PackageCacheDatabaseFilename);
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
