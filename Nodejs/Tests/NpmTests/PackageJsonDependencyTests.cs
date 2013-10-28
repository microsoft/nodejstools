using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests
{

    [TestClass]
    public class PackageJsonDependencyTests :AbstractPackageJsonTests
    {
        /*
         { "dependencies" :
          { "foo" : "1.0.0 - 2.9999.9999"
          , "bar" : ">=1.0.2 <2.1.2"
          , "baz" : ">1.0.2 <=2.3.4"
          , "boo" : "2.0.1"
          , "qux" : "<1.0.0 || >=2.3.1 <2.4.5 || >=2.5.2 <3.0.0"
          , "asd" : "http://asdf.com/asdf.tar.gz"
          , "til" : "~1.2"
          , "elf" : "~1.2.3"
          , "two" : "2.x"
          , "thr" : "3.3.x"
          }
        }
         */

        private const string PkgDependencies = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0"",
    ""dependencies"" :
          { ""foo"" : ""1.0.0 - 2.9999.9999""
          , ""bar"" : "">=1.0.2 <2.1.2""
          , ""baz"" : "">1.0.2 <=2.3.4""
          , ""boo"" : ""2.0.1""
          , ""qux"" : ""<1.0.0 || >=2.3.1 <2.4.5 || >=2.5.2 <3.0.0""
          , ""asd"" : ""http://asdf.com/asdf.tar.gz""
          , ""til"" : ""~1.2""
          , ""elf"" : ""~1.2.3""
          , ""two"" : ""2.x""
          , ""thr"" : ""3.3.x""
        , ""git"" : ""git://github.com/user/project.git#commit-ish""
        , ""gitssh"" : ""git+ssh://user@hostname:project.git#commit-ish""
        , ""gitssh2"" : ""git+ssh://user@hostname/project.git#commit-ish""
        , ""githttp"" : ""git+http://user@hostname/project/blah.git#commit-ish""
        , ""githttps"" : ""git+https://user@hostname/project/blah.git#commit-ish""
        , ""github"" : ""username/projectname""
          }
}";

        private const string PkgAllTheDependencies = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0"",
    ""dependencies"" :
          { ""foo"" : ""1.0.0 - 2.9999.9999""
          , ""bar"" : "">=1.0.2 <2.1.2""
          , ""baz"" : "">1.0.2 <=2.3.4""
          , ""boo"" : ""2.0.1""
          , ""qux"" : ""<1.0.0 || >=2.3.1 <2.4.5 || >=2.5.2 <3.0.0""
          , ""asd"" : ""http://asdf.com/asdf.tar.gz""
          , ""til"" : ""~1.2""
          , ""elf"" : ""~1.2.3""
          , ""two"" : ""2.x""
          , ""thr"" : ""3.3.x""
        , ""git"" : ""git://github.com/user/project.git#commit-ish""
        , ""gitssh"" : ""git+ssh://user@hostname:project.git#commit-ish""
        , ""gitssh2"" : ""git+ssh://user@hostname/project.git#commit-ish""
        , ""githttp"" : ""git+http://user@hostname/project/blah.git#commit-ish""
        , ""githttps"" : ""git+https://user@hostname/project/blah.git#commit-ish""
        , ""github"" : ""username/projectname""
          },
    ""devDependencies"" :
          { ""devfoo"" : ""1.0.0 - 2.9999.9999""
          , ""devbar"" : "">=1.0.2 <2.1.2""
          , ""devbaz"" : "">1.0.2 <=2.3.4""
          , ""devboo"" : ""2.0.1""
          , ""devqux"" : ""<1.0.0 || >=2.3.1 <2.4.5 || >=2.5.2 <3.0.0""
          , ""devasd"" : ""http://asdf.com/asdf.tar.gz""
          , ""devtil"" : ""~1.2""
          , ""develf"" : ""~1.2.3""
          , ""devtwo"" : ""2.x""
          , ""devthr"" : ""3.3.x""
        , ""devgit"" : ""git://github.com/user/project.git#commit-ish""
        , ""devgitssh"" : ""git+ssh://user@hostname:project.git#commit-ish""
        , ""devgitssh2"" : ""git+ssh://user@hostname/project.git#commit-ish""
        , ""devgithttp"" : ""git+http://user@hostname/project/blah.git#commit-ish""
        , ""devgithttps"" : ""git+https://user@hostname/project/blah.git#commit-ish""
        , ""devgithub"" : ""username/projectname""
          },
    ""bundledDependencies"" :
          [ ""foo""
          , ""bar""
          , ""baz""
          , ""boo""
          , ""qux""
          , ""asd""
          , ""til""
          , ""elf""
          , ""two""
          , ""thr""
        , ""git""
        , ""gitssh""
        , ""gitssh2""
        , ""githttp""
        , ""githttps""
        , ""github""
          ],
    ""optionalDependencies"" :
          { ""optionalfoo"" : ""1.0.0 - 2.9999.9999""
          , ""optionalbar"" : "">=1.0.2 <2.1.2""
          , ""optionalbaz"" : "">1.0.2 <=2.3.4""
          , ""optionalboo"" : ""2.0.1""
          , ""optionalqux"" : ""<1.0.0 || >=2.3.1 <2.4.5 || >=2.5.2 <3.0.0""
          , ""optionalasd"" : ""http://asdf.com/asdf.tar.gz""
          , ""optionaltil"" : ""~1.2""
          , ""optionalelf"" : ""~1.2.3""
          , ""optionaltwo"" : ""2.x""
          , ""optionalthr"" : ""3.3.x""
        , ""optionalgit"" : ""git://github.com/user/project.git#commit-ish""
        , ""optionalgitssh"" : ""git+ssh://user@hostname:project.git#commit-ish""
        , ""optionalgitssh2"" : ""git+ssh://user@hostname/project.git#commit-ish""
        , ""optionalgithttp"" : ""git+http://user@hostname/project/blah.git#commit-ish""
        , ""optionalgithttps"" : ""git+https://user@hostname/project/blah.git#commit-ish""
        , ""optionalgithub"" : ""username/projectname""
          }
}";

        private const string PkgBundleDependencies = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0"",
    ""dependencies"" :
          { ""foo"" : ""1.0.0 - 2.9999.9999""
          , ""bar"" : "">=1.0.2 <2.1.2""
          , ""baz"" : "">1.0.2 <=2.3.4""
          , ""boo"" : ""2.0.1""
          , ""qux"" : ""<1.0.0 || >=2.3.1 <2.4.5 || >=2.5.2 <3.0.0""
          , ""asd"" : ""http://asdf.com/asdf.tar.gz""
          , ""til"" : ""~1.2""
          , ""elf"" : ""~1.2.3""
          , ""two"" : ""2.x""
          , ""thr"" : ""3.3.x""
        , ""git"" : ""git://github.com/user/project.git#commit-ish""
        , ""gitssh"" : ""git+ssh://user@hostname:project.git#commit-ish""
        , ""gitssh2"" : ""git+ssh://user@hostname/project.git#commit-ish""
        , ""githttp"" : ""git+http://user@hostname/project/blah.git#commit-ish""
        , ""githttps"" : ""git+https://user@hostname/project/blah.git#commit-ish""
        , ""github"" : ""username/projectname""
          },
    ""devDependencies"" :
          { ""devfoo"" : ""1.0.0 - 2.9999.9999""
          , ""devbar"" : "">=1.0.2 <2.1.2""
          , ""devbaz"" : "">1.0.2 <=2.3.4""
          , ""devboo"" : ""2.0.1""
          , ""devqux"" : ""<1.0.0 || >=2.3.1 <2.4.5 || >=2.5.2 <3.0.0""
          , ""devasd"" : ""http://asdf.com/asdf.tar.gz""
          , ""devtil"" : ""~1.2""
          , ""develf"" : ""~1.2.3""
          , ""devtwo"" : ""2.x""
          , ""devthr"" : ""3.3.x""
        , ""devgit"" : ""git://github.com/user/project.git#commit-ish""
        , ""devgitssh"" : ""git+ssh://user@hostname:project.git#commit-ish""
        , ""devgitssh2"" : ""git+ssh://user@hostname/project.git#commit-ish""
        , ""devgithttp"" : ""git+http://user@hostname/project/blah.git#commit-ish""
        , ""devgithttps"" : ""git+https://user@hostname/project/blah.git#commit-ish""
        , ""devgithub"" : ""username/projectname""
          },
    ""bundleDependencies"" :
          [ ""foo""
          , ""bar""
          , ""baz""
          , ""boo""
          , ""qux""
          , ""asd""
          , ""til""
          , ""elf""
          , ""two""
          , ""thr""
        , ""git""
        , ""gitssh""
        , ""gitssh2""
        , ""githttp""
        , ""githttps""
        , ""github""
          ]
}";

        private static readonly string[][] VersionRangeDependencies = new[]
                {
                    new[] {"foo", "1.0.0 - 2.9999.9999"}
                    , new[] {"bar", ">=1.0.2 <2.1.2"}
                    , new[] {"baz", ">1.0.2 <=2.3.4"}
                    , new[] {"boo", "2.0.1"}
                    , new[] {"qux", "<1.0.0 || >=2.3.1 <2.4.5 || >=2.5.2 <3.0.0"}
                    , new[] {"til", "~1.2"}
                    , new[] {"elf", "~1.2.3"}
                    , new[] {"two", "2.x"}
                    , new[] {"thr", "3.3.x"}
                };

        private static readonly string[][] UrlDependencies = new[]
                {
                    new[] {"asd", "http://asdf.com/asdf.tar.gz"}
                    , new[] {"git", "git://github.com/user/project.git#commit-ish"}
                    , new[] {"gitssh", "git+ssh://user@hostname:project.git#commit-ish"}
                    , new[] {"gitssh2", "git+ssh://user@hostname/project.git#commit-ish"}
                    , new[] {"githttp", "git+http://user@hostname/project/blah.git#commit-ish"}
                    , new[] {"githttps", "git+https://user@hostname/project/blah.git#commit-ish"}
                    , new[] {"github", "username/projectname"}
                };

        private static readonly string[][] OptionalVersionRangeDependencies = new[]
                {
                    new[] {"optionalfoo", "1.0.0 - 2.9999.9999"}
                    , new[] {"optionalbar", ">=1.0.2 <2.1.2"}
                    , new[] {"optionalbaz", ">1.0.2 <=2.3.4"}
                    , new[] {"optionalboo", "2.0.1"}
                    , new[] {"optionalqux", "<1.0.0 || >=2.3.1 <2.4.5 || >=2.5.2 <3.0.0"}
                    , new[] {"optionaltil", "~1.2"}
                    , new[] {"optionalelf", "~1.2.3"}
                    , new[] {"optionaltwo", "2.x"}
                    , new[] {"optionalthr", "3.3.x"}
                };

        private static readonly string[][] OptionalUrlDependencies = new[]
                {
                    new[] {"optionalasd", "http://asdf.com/asdf.tar.gz"}
                    , new[] {"optionalgit", "git://github.com/user/project.git#commit-ish"}
                    , new[] {"optionalgitssh", "git+ssh://user@hostname:project.git#commit-ish"}
                    , new[] {"optionalgitssh2", "git+ssh://user@hostname/project.git#commit-ish"}
                    , new[] {"optionalgithttp", "git+http://user@hostname/project/blah.git#commit-ish"}
                    , new[] {"optionalgithttps", "git+https://user@hostname/project/blah.git#commit-ish"}
                    , new[] {"optionalgithub", "username/projectname"}
                };

        private static readonly string[][] DevVersionRangeDependencies = new[]
                {
                    new[] {"devfoo", "1.0.0 - 2.9999.9999"}
                    , new[] {"devbar", ">=1.0.2 <2.1.2"}
                    , new[] {"devbaz", ">1.0.2 <=2.3.4"}
                    , new[] {"devboo", "2.0.1"}
                    , new[] {"devqux", "<1.0.0 || >=2.3.1 <2.4.5 || >=2.5.2 <3.0.0"}
                    , new[] {"devtil", "~1.2"}
                    , new[] {"develf", "~1.2.3"}
                    , new[] {"devtwo", "2.x"}
                    , new[] {"devthr", "3.3.x"}
                };

        private static readonly string[][] DevUrlDependencies = new[]
                {
                    new[] {"devasd", "http://asdf.com/asdf.tar.gz"}
                    , new[] {"devgit", "git://github.com/user/project.git#commit-ish"}
                    , new[] {"devgitssh", "git+ssh://user@hostname:project.git#commit-ish"}
                    , new[] {"devgitssh2", "git+ssh://user@hostname/project.git#commit-ish"}
                    , new[] {"devgithttp", "git+http://user@hostname/project/blah.git#commit-ish"}
                    , new[] {"devgithttps", "git+https://user@hostname/project/blah.git#commit-ish"}
                    , new[] {"devgithub", "username/projectname"}
                };

        private static readonly string[] ExpectedBundledDependencies = new[]
                {
                    "foo"
                    , "bar"
                    , "baz"
                    , "boo"
                    , "qux"
                    , "til"
                    , "elf"
                    , "two"
                    , "thr"
                    , "asd"
                    , "git"
                    , "gitssh"
                    , "gitssh2"
                    , "githttp"
                    , "githttps"
                    , "github"
                };

        private void CheckEmptyDependenciesNotNull(IDependencies dependencies)
        {
            Assert.IsNotNull(dependencies, "Dependencies should not be null.");
            Assert.AreEqual(0, dependencies.Count, "Should not be any dependencies.");
        }

        [TestMethod]
        public void TestReadEmptyDependenciesNotNull()
        {
            CheckEmptyDependenciesNotNull(LoadFrom(PkgSimple).Dependencies);
        }

        [TestMethod]
        public void TestReadEmptyDevDependenciesNotNull()
        {
            CheckEmptyDependenciesNotNull(LoadFrom(PkgSimple).DevDependencies);
        }

        [TestMethod]
        public void TestReadEmptyOptionalDependenciesNotNull()
        {
            CheckEmptyDependenciesNotNull(LoadFrom(PkgSimple).OptionalDependencies);
        }

        private void CheckDependencies(IDictionary<string, IDependency> retrieved, IEnumerable<string[]> expected, int expectedCount)
        {
            Assert.AreEqual(expectedCount, retrieved.Count, "Retrieved dependency count mismatch.");
            foreach (var pair in expected)
            {
                var dependency = retrieved[pair[0]];
                Assert.IsNotNull(
                    dependency,
                    string.Format("Should have found a dependency on package '{0}'.", pair[0]));

                if (pair[1].IndexOf('/') < 0) //  i.e., the dependency isn't specified with a URL
                {
                    Assert.IsNull(
                        dependency.Url,
                        string.Format("Dependency on package '{0}' should not specify a URL.", pair[0]));

                    Assert.AreEqual(
                        pair[1],
                        dependency.VersionRangeText,
                        string.Format("Version range mismatch for package '{0}'.", pair[0]));
                }
                else
                {
                    Assert.IsNull(dependency.VersionRangeText, "Dependency with URL should not specify version range text.");
                    IDependencyUrl url = dependency.Url;
                    Assert.IsNotNull(url, "Dependency URL should not be null.");
                    string address = url.Address;
                    Assert.IsNotNull(address, "Dependency URL address should not be null.");
                    var index = address.IndexOf("://");
                    if (index < 0)
                    {
                        Assert.AreEqual(DependencyUrlType.GitHub, url.Type, "Dependency URL type should be GitHub");
                    }
                    else
                    {
                        var prefix = address.Substring(0, index);
                        switch (prefix)
                        {
                            case "http":
                                Assert.AreEqual(DependencyUrlType.Http, url.Type, "Dependency URL type should be Http.");
                                break;

                            case "git":
                                Assert.AreEqual(DependencyUrlType.Git, url.Type, "Dependency URL type should be Git.");
                                break;

                            case "git+ssh":
                                Assert.AreEqual(DependencyUrlType.GitSsh, url.Type, "Dependency URL type should be GitSsh.");
                                break;

                            case "git+http":
                                Assert.AreEqual(DependencyUrlType.GitHttp, url.Type, "Dependency URL type should be GitHttp.");
                                break;

                            case "git+https":
                                Assert.AreEqual(DependencyUrlType.GitHttps, url.Type, "Dependency URL type should be GitHttps.");
                                break;

                            default:
                                Assert.Fail(string.Format("Unrecognised URL prefix: {0}", prefix));
                                break;
                        }
                    }
                }
            }
        }

        private void CheckDependencies(IDictionary<string, IDependency> retrieved, IEnumerable<string[]> expected)
        {
            CheckDependencies(retrieved, expected, 16);
        }

        private IDictionary<string, IDependency> ReadDependencies(IDependencies dependencies, int expectedCount)
        {
            Assert.AreEqual(expectedCount, dependencies.Count, "Dependency count mismatch.");

            var retrieved = new Dictionary<string, IDependency>();
            foreach (var dependency in dependencies)
            {
                retrieved[dependency.Name] = dependency;
            }
            return retrieved;
        }

        private IDictionary<string, IDependency> ReadDependencies(IDependencies dependencies)
        {
            return ReadDependencies(dependencies, 16);
        }

        private IDictionary<string, IDependency> ReadDependencies()
        {
            return ReadDependencies(LoadFrom(PkgDependencies).Dependencies);            
        }

        private IDictionary<string, IDependency> ReadDevDependencies()
        {
            return ReadDependencies(LoadFrom(PkgAllTheDependencies).DevDependencies);
        }

        private IDictionary<string, IDependency> ReadOptionalDependencies()
        {
            return ReadDependencies(LoadFrom(PkgAllTheDependencies).OptionalDependencies);
        }

        private IDictionary<string, IDependency> ReadAllDependencies()
        {
            return ReadDependencies(LoadFrom(PkgAllTheDependencies).AllDependencies, 48);
        }

        [TestMethod]
        public void TestReadDependenciesWithVersionRange()
        {
            CheckDependencies(ReadDependencies(), VersionRangeDependencies);
        }

        [TestMethod]
        public void TestReadDependenciesWithUrls()
        {
            CheckDependencies(ReadDependencies(), UrlDependencies);
        }

        [TestMethod]
        public void TestReadDevDependenciesWithVersionRange()
        {
            CheckDependencies(ReadDevDependencies(), DevVersionRangeDependencies);
        }

        [TestMethod]
        public void TestReadDevDependenciesWithUrls()
        {
            CheckDependencies(ReadDevDependencies(), DevUrlDependencies);
        }

        [TestMethod]
        public void TestReadBundledDependencies()
        {
            CheckStringArrayContents(LoadFrom(PkgAllTheDependencies).BundledDependencies, 16, ExpectedBundledDependencies);
        }

        [TestMethod]
        public void TestReadBundleDependencies()
        {
            CheckStringArrayContents(LoadFrom(PkgBundleDependencies).BundledDependencies, 16, ExpectedBundledDependencies);
        }

        [TestMethod]
        public void TestReadOptionalDependenciesWithVersionRange()
        {
            CheckDependencies(ReadOptionalDependencies(), OptionalVersionRangeDependencies);
        }

        [TestMethod]
        public void TestReadOptionalDependenciesWithUrls()
        {
            CheckDependencies(ReadOptionalDependencies(), OptionalUrlDependencies);
        }

        [TestMethod]
        public void TestReadAllDependencies()
        {
            CheckDependencies(
                ReadAllDependencies(), 
                UrlDependencies.Concat(VersionRangeDependencies).Concat(DevUrlDependencies).Concat(DevVersionRangeDependencies).Concat(OptionalUrlDependencies).Concat(OptionalVersionRangeDependencies),
                48);
        }
    }
}
