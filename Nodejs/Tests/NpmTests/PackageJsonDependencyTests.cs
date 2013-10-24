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

        protected const string PkgDependencies = @"{
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

        [TestMethod]
        public void TestReadEmptyDependenciesNotNull()
        {
            var pkg = LoadFrom(PkgSimple);
            var dependencies = pkg.Dependencies;
            Assert.IsNotNull(dependencies, "Dependencies should not be null.");
            Assert.AreEqual(0, dependencies.Count, "Should not be any dependencies.");
        }

        private void CheckDependencies(IDictionary<string, IDependency> retrieved)
        {
            Assert.AreEqual(16, retrieved.Count, "Retrieved dependency count mismatch.");
            foreach (var pair in new[]
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
            })
            {
                var dependency = retrieved[pair[0]];
                Assert.IsNotNull(
                    dependency,
                    string.Format("Should have found a dependency on package '{0}'.", pair[0]));

                Assert.IsNull(
                    dependency.Url,
                    string.Format("Dependency on package '{0}' should not specify a URL.", pair[0]));

                Assert.AreEqual(
                    pair[1],
                    dependency.VersionRangeText,
                    string.Format("Version range mismatch for package '{0}'.", pair[0]));
            }
        }

        [TestMethod]
        public void TestReadDependencies()
        {
            var pkg = LoadFrom(PkgDependencies);
            var dependencies = pkg.Dependencies;
            Assert.AreEqual(16, dependencies.Count, "Dependency count mismatch.");

            var retrieved = new Dictionary<string, IDependency>();
            foreach (var dependency in dependencies)
            {
                retrieved[dependency.Name] = dependency;
            }
            CheckDependencies(retrieved);
        }

    }
}
