//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Npm.SPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace NpmTests {

    [TestClass]
    [DeploymentItem(@"TestData\NpmSearchData\", "NpmSearchData")]
    // TODO: The below was causing failures when included. Figure out if/how this should be set. (Seems to work fine without it).
    //[DeploymentItem(@"sqlite3.dll")]
    public class NpmSearchTests {

        private const string PackageCacheAllJsonFilename = "packagecache.min.json";
        private const string PackageCacheSinceJsonFilename = "since_packages.object.json";
        private const string PackageCacheSinceJsonArrayFilename = "since_packages.array.json";
        private const string PackageCacheDirectory = @"NpmSearchData\NpmCache";
        private const string PackageCacheFilename = @"NpmSearchData\testpackagecache.sqlite";
        private const string RegistryUrl = "http://registry.npmjs.org";

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
            IEnumerable<SemverVersion> expectedVersions,
            IEnumerable<string> expectedKeywords) {

            Assert.AreEqual(expectedName, package.Name, "Invalid name.");
            Assert.AreEqual(expectedDescription, package.Description, "Invalid description.");
            string actualAuthorString = null == package.Author ? null : package.Author.Name;
            Assert.AreEqual(expectedAuthor, actualAuthorString, "Invalid author.");
            Assert.AreEqual(expectedPublishDateTime, package.PublishDateTimeString, "Invalid publish date/time.");
            Assert.AreEqual(expectedVersion, package.Version, "Invalid version.");

            // Sometimes authors include duplicate keywords in the list
            AssertUtil.ArrayEquals(package.Keywords.Distinct().ToList(), expectedKeywords.Distinct().ToList());
            AssertUtil.ArrayEquals(package.AvailableVersions.ToList(), expectedVersions.ToList());
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
            IEnumerable<SemverVersion> expectedVersions,
            IEnumerable<string> expectedKeywords) {
            CheckPackage(
                packagesByName[expectedName],
                expectedName,
                expectedDescription,
                expectedAuthor,
                expectedPublishDateTime,
                expectedVersion,
                expectedVersions,
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
                    expectedVersions,
                    expectedKeywords);
            }
        }

        private IList<IPackage> GetTestPackageList(
            string cachePath,
            out IDictionary<string, IPackage> byName) {
            IList<IPackage> target = new List<IPackage>();

            target = new NpmGetCatalogCommand(string.Empty, cachePath, false, RegistryUrl).GetCatalogPackagesAsync(string.Empty, new Uri(RegistryUrl)).GetAwaiter().GetResult().ToList();

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
            string databaseFilename = NpmGetCatalogCommand.DatabaseCacheFilename;
            string registryFilename = NpmGetCatalogCommand.RegistryCacheFilename;
            string cachePath = "CachePath";
            string registryDirectory = "registry";

            Uri registryUrl = new Uri(RegistryUrl);

            string catalogDatabaseFilename = Path.Combine(cachePath, databaseFilename);
            string registryDatabaseFilename = Path.Combine(cachePath, registryDirectory, registryFilename);
            string relativeRegistryDatabaseFilename = Path.Combine(registryDirectory, registryFilename);


            using (var reader = GetCatalogueReader(PackageCacheAllJsonFilename)) {
                var getCatalogCommand = new NpmGetCatalogCommand(string.Empty, cachePath, false, RegistryUrl);
                getCatalogCommand.CreateCatalogDatabaseAndInsertEntries(catalogDatabaseFilename, registryUrl, registryDirectory);
                new NpmGetCatalogCommand(string.Empty, cachePath, false).ParseResultsAndAddToDatabase(reader, registryDatabaseFilename, registryUrl.ToString());
            }

            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(cachePath, out byName);

            Assert.AreEqual(102, target.Count);

            CheckPackage(
                target,
                byName,
                93,
                "cordova",
                "Cordova command line interface tool",
                "Anis Kadri",
                "07/08/2014 17:55:34",
                SemverVersion.Parse("3.5.0-0.2.6"),
                new[] {
                    SemverVersion.Parse("3.5.0-0.2.6"),
                    SemverVersion.Parse("3.5.0-0.2.4")
                },
                new[] { "cordova", "client", "cli" }
                );
        }

        [TestMethod, Priority(0)]
        public void CheckDatabaseUpdate() {
            string cachePath = "NpmCacheUpdate";
            string registryPath = Path.Combine(cachePath, "registry", NpmGetCatalogCommand.RegistryCacheFilename);

            FileUtils.CopyDirectory(PackageCacheDirectory, cachePath);

            using (var reader = GetCatalogueReader(PackageCacheSinceJsonFilename)) {
                new NpmGetCatalogCommand(string.Empty, cachePath, false).ParseResultsAndAddToDatabase(reader, registryPath, RegistryUrl);
            }

            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(cachePath, out byName);

            Assert.AreEqual(89978, target.Count);

            // Package updated successfully
            CheckPackage(
                target,
                byName,
                13898,
                "cordova",
                "Cordova command line interface tool",
                "Anis Kadri",
                "10/16/2014 18:05:13",
                SemverVersion.Parse("4.0.0"),
                new[] {
                    SemverVersion.Parse("4.0.0"),
                    SemverVersion.Parse("3.6.0-0.2.8"),
                },
                new[] { "cordova", "client", "cli" }
                );

            // Package added successfully
            CheckPackage(
                target,
                byName,
                54151,
                "mytestpackage98",
                null,
                null,
                "08/14/2014 19:46:24",
                SemverVersion.Parse("0.1.3"),
                new[] {
                    SemverVersion.Parse("0.1.3")
                },
                Enumerable.Empty<string>()
                );

        }

        [TestMethod, Priority(0)]
        public void CheckDatabaseUpdateArray() {
            string cachePath = "NpmCacheUpdate";
            string registryPath = Path.Combine(cachePath, "registry", NpmGetCatalogCommand.RegistryCacheFilename);

            FileUtils.CopyDirectory(PackageCacheDirectory, cachePath);

            using (var reader = GetCatalogueReader(PackageCacheSinceJsonArrayFilename)) {
                new NpmGetCatalogCommand(string.Empty, cachePath, false).ParseResultsAndAddToDatabase(reader, registryPath, RegistryUrl);
            }

            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(cachePath, out byName);

            Assert.AreEqual(90066, target.Count);

            // Package updated successfully
            CheckPackage(
                target,
                byName,
                13986,
                "cordova",
                "Cordova command line interface tool",
                "Anis Kadri",
                "10/16/2014 18:05:13",
                SemverVersion.Parse("4.0.0"),
                new[] {
                    SemverVersion.Parse("4.0.0"),
                    SemverVersion.Parse("3.6.0-0.2.8"),
                },
                new[] { "cordova", "client", "cli" }
                );

            // Package added successfully
            CheckPackage(
                target,
                byName,
                253,
                "9e-sass-lint",
                "Makes sure you stick to our CSS rule order http://9elements.com/css-rule-order/",
                "Sascha Gehlich",
                "08/19/2015 08:21:25",
                SemverVersion.Parse("0.0.12"),
                new[] {
                    SemverVersion.Parse("0.0.12")
                },
                new[] { "sass", "css", "lint", "rules" }
                );

        }

        [TestMethod, Priority(0)]
        public void CheckPackageWithBuildPreReleaseInfo() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDirectory, out byName);
            CheckPackage(
                target,
                byName,
                65900,
                "psc-cms-js",
                "js library for Psc CMS (pscheit/psc-cms). shim reposistory for builds.",
                "Philipp Scheit",
                "01/07/2014 10:57:58",
                SemverVersion.Parse("1.3.0-517056d"),
                new[] {
                    SemverVersion.Parse("1.3.0-95847e2"),
                    SemverVersion.Parse("1.3.0-517056d"),
                    SemverVersion.Parse("1.4.0-e14fdf0")
                },
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
            var target = GetTestPackageList(PackageCacheDirectory, out byName);
            CheckSensibleNumberOfNonZeroVersions(target);
        }

        [TestMethod, Priority(0)]
        public void CheckCorrectPackageCount() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDirectory, out byName);
            Assert.AreEqual(89924, target.Count, "Unexpected package count in catalogue list.");
        }

        [TestMethod, Priority(0)]
        public void CheckNoDuplicatePackages() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDirectory, out byName);
            CheckOnlyOneOfEachPackage(target);
        }

        [TestMethod, Priority(0)]
        public void CheckListAndDictByNameSameSize() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDirectory, out byName);
            Assert.AreEqual(target.Count, byName.Count, "Number of packages should be same in list and dictionary.");
        }
        
        [TestMethod, Priority(0)]
        public void CheckFirstPackageInCatalog() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDirectory, out byName);
            CheckPackage(
                target,
                byName,
                0,
                "0",
                null,
                null,
                "06/17/2014 06:38:43",
                new SemverVersion(0, 0, 0),
                new[] {
                    new SemverVersion(0, 0, 0)
                },
                Enumerable.Empty<string>());
        }

        [TestMethod, Priority(0)]
        public void CheckLastPackageInCatalog_zzz() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDirectory, out byName);
            CheckPackage(
                target,
                byName,
                89923,
                "z_test",
                "zhanghao test",
                "zhanghao",
                "01/02/2014 03:28:23",
                new SemverVersion(1, 0, 0),
                new[] {
                    new SemverVersion(1, 0, 0)
                },
                Enumerable.Empty<string>());
        }

        [TestMethod, Priority(0)]
        public void CheckPackageEqualsInDescription() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDirectory, out byName);
            CheckPackage(
                target,
                byName,
                62060,
                "particularizable",
                "particularizable ================ `enumerable` was taken.",
                "ELLIOTTCABLE",
                "06/11/2013 22:48:35",
                SemverVersion.Parse("1.0.0"),
                new[] {
                    SemverVersion.Parse("1.0.0")
                },
                Enumerable.Empty<string>());
        }

        [TestMethod, Priority(0)]
        public void CheckPackageNoDescriptionAuthorVersion() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDirectory, out byName);

            CheckPackage(
                target,
                byName,
                25,
                "122",
                null,
                null,
                "03/03/2014 16:52:16",
                SemverVersion.UnknownVersion,
                Enumerable.Empty<SemverVersion>(),
                Enumerable.Empty<string>());
        }

        [TestMethod, Priority(0)]
        public void CheckPackageNoDescription() {
            IDictionary<string, IPackage> byName;
            var target = GetTestPackageList(PackageCacheDirectory, out byName);

            CheckPackage(
                target,
                byName,
                455,
                "active-client",
                null,
                "Subbu Allamaraju",
                "01/03/2011 21:21:12",
                SemverVersion.Parse("0.1.1"),
                new[] {
                    SemverVersion.Parse("0.1.1")
                },
                Enumerable.Empty<string>());
        }

        private IList<IPackage> GetFilteredPackageList(string filterString) {
            var filter = new DatabasePackageCatalogFilter(PackageCacheFilename);
            return filter.Filter(filterString).ToList();
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

        [TestMethod, Priority(0)]
        public void TestFilterStringWithHyphens() {
            const string
                filterStringWithHyphenMiddle = "grunt-contrib",
                filterStringWithHyphenSuffix = "amazing-",
                filterStringWithHyphenPrefix = "-grunt";

            var results = GetFilteredPackageList(filterStringWithHyphenMiddle);
            Assert.AreEqual(filterStringWithHyphenMiddle, results.First().Name, "Exact filter string match should be first in list.");

            results = GetFilteredPackageList(filterStringWithHyphenSuffix);
            Assert.AreEqual(filterStringWithHyphenSuffix, results.First().Name, "Exact filter string match (including suffix) should be first.");

            // Package names cannot begin with a hyphen, but we would expect the first result to at least include the filter string.
            results = GetFilteredPackageList(filterStringWithHyphenPrefix);
            Assert.IsTrue(results.First().Name.Contains(filterStringWithHyphenPrefix), "Filter string match (including prefix) should be first");
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
