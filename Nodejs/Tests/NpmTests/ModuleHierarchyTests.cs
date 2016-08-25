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
using System.Linq;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Npm.SPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests {
    [TestClass]
    public class ModuleHierarchyTests : AbstractPackageJsonTests {
        protected const string PkgSingleDependency = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0"",
    ""dependencies"": {
        ""sax"": "">=0.1.0 <0.2.0""
    }
}";

        protected const string PkgSingleRecursiveDependency = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0"",
    ""dependencies"": {
        ""express"": ""4.0.0""
    }
}";

        [TestMethod, Priority(0)]
        public void ReadRootPackageNoDependencies() {
            using (var manager = new TemporaryFileManager()) {
                var rootDir = FilesystemPackageJsonTestHelpers.CreateRootPackage(manager, PkgSimple);
                var pkg = RootPackageFactory.Create(rootDir);
                Assert.IsNotNull(pkg, "Root package should not be null.");
                Assert.AreEqual(rootDir, pkg.Path, "Package path mismatch.");
                var json = pkg.PackageJson;
                Assert.IsNotNull(json, "package.json should not be null.");
                Assert.AreEqual(json.Name, pkg.Name, "Package name mismatch.");
                Assert.AreEqual(json.Version, pkg.Version, "Package version mismatch.");
                var modules = pkg.Modules;
                Assert.IsNotNull(modules, "Modules should not be null.");
                Assert.AreEqual(0, modules.Count, "Module count mismatch.");
            }
        }

        private static void RunNpmInstall(string rootDir) {
            new NpmInstallCommand(rootDir).ExecuteAsync().GetAwaiter().GetResult();
        }

        [TestMethod, Priority(0)]
        public void ReadRootPackageOneDependency() {
            using (var manager = new TemporaryFileManager()) {
                var rootDir = FilesystemPackageJsonTestHelpers.CreateRootPackage(manager, PkgSingleDependency);
                RunNpmInstall(rootDir);

                var pkg = RootPackageFactory.Create(rootDir);

                var json = pkg.PackageJson;
                var dependencies = json.AllDependencies;
                Assert.AreEqual(1, dependencies.Count, "Dependency count mismatch.");

                IDependency dep = dependencies["sax"];
                Assert.IsNotNull(dep, "sax dependency should not be null.");
                Assert.AreEqual(">=0.1.0 <0.2.0", dep.VersionRangeText, "Version range mismatch.");

                var modules = pkg.Modules;
                Assert.AreEqual(1, modules.Count, "Module count mismatch");

                IPackage module = modules[0];
                Assert.IsNotNull(module, "Module should not be null when retrieved by index.");
                module = modules["sax"];
                Assert.IsNotNull(module, "Module should not be null when retrieved by name.");

                Assert.AreEqual(modules[0], modules["sax"], "Modules should be same whether retrieved by name or index.");

                Assert.AreEqual("sax", module.Name, "Module name mismatch.");

                //  All of these should be indicated, in some way, in the Visual Studio treeview.

                Assert.IsNotNull(module.PackageJson, "Module package.json should not be null.");

                Assert.IsTrue(
                    module.IsListedInParentPackageJson,
                    "Should be listed as a dependency in parent package.json.");
                Assert.IsFalse(module.IsMissing, "Should not be marked as missing.");
                Assert.IsFalse(module.IsDevDependency, "Should not be marked as dev dependency.");
                Assert.IsFalse(module.IsOptionalDependency, "Should not be marked as optional dependency.");
                Assert.IsFalse(module.IsBundledDependency, "Should not be marked as bundled dependency.");

                //  Redundant?
                Assert.IsTrue(module.HasPackageJson, "Module should have its own package.json");
            }
        }

        [TestMethod, Priority(0)]
        public void ReadRootPackageMissingDependency() {
            using (var manager = new TemporaryFileManager()) {
                var rootDir = FilesystemPackageJsonTestHelpers.CreateRootPackage(manager, PkgSingleDependency);

                var pkg = RootPackageFactory.Create(rootDir);

                var json = pkg.PackageJson;
                var dependencies = json.AllDependencies;
                Assert.AreEqual(1, dependencies.Count, "Dependency count mismatch.");

                IDependency dep = dependencies["sax"];
                Assert.IsNotNull(dep, "sax dependency should not be null.");
                Assert.AreEqual(">=0.1.0 <0.2.0", dep.VersionRangeText, "Version range mismatch.");

                var modules = pkg.Modules;
                Assert.AreEqual(1, modules.Count, "Module count mismatch");

                IPackage module = modules[0];
                Assert.IsNotNull(module, "Module should not be null when retrieved by index.");
                module = modules["sax"];
                Assert.IsNotNull(module, "Module should not be null when retrieved by name.");

                Assert.AreEqual(modules[0], modules["sax"], "Modules should be same whether retrieved by name or index.");

                Assert.AreEqual("sax", module.Name, "Module name mismatch.");

                //  All of these should be indicated, in some way, in the Visual Studio treeview.

                Assert.IsNull(module.PackageJson, "Module package.json should be null for missing dependency.");

                Assert.IsTrue(
                    module.IsListedInParentPackageJson,
                    "Should be listed as a dependency in parent package.json.");
                Assert.IsTrue(module.IsMissing, "Should be marked as missing.");
                Assert.IsFalse(module.IsDevDependency, "Should not be marked as dev dependency.");
                Assert.IsFalse(module.IsOptionalDependency, "Should not be marked as optional dependency.");
                Assert.IsFalse(module.IsBundledDependency, "Should not be marked as bundled dependency.");

                //  Redundant?
                Assert.IsFalse(module.HasPackageJson, "Missing module should not have its own package.json");
            }
        }

        [TestMethod, Priority(0)]
        public void ReadRootDependencyRecursive() {
            using (var manager = new TemporaryFileManager()) {

                var rootDir = FilesystemPackageJsonTestHelpers.CreateRootPackage(manager, PkgSingleRecursiveDependency);
                RunNpmInstall(rootDir);

                var pkg = RootPackageFactory.Create(rootDir);

                var json = pkg.PackageJson;
                var dependencies = json.AllDependencies;
                Assert.AreEqual(1, dependencies.Count, "Dependency count mismatch.");

                IDependency dep = dependencies["express"];
                Assert.IsNotNull(dep, "express dependency should not be null.");
                Assert.AreEqual("4.0.0", dep.VersionRangeText, "Version range mismatch.");

                var modules = pkg.Modules;
                Assert.AreEqual(1, modules.Count, "Module count mismatch");

                IPackage module = modules[0];
                Assert.IsNotNull(module, "Module should not be null when retrieved by index.");
                module = modules["express"];
                Assert.IsNotNull(module, "Module should not be null when retrieved by name.");

                Assert.AreEqual(
                    modules[0],
                    modules["express"],
                    "Modules should be same whether retrieved by name or index.");

                Assert.AreEqual("express", module.Name, "Module name mismatch.");

                Assert.AreEqual("4.0.0", module.Version.ToString(), "Module version mismatch");

                var expectedModules = new string[] {
                "accepts",
                "buffer-crc32",
                "cookie",
                "cookie-signature",
                "debug",
                "escape-html",
                "fresh",
                "merge-descriptors",
                "methods",
                "parseurl",
                "path-to-regexp",
                "qs",
                "range-parser",
                "send",
                "serve-static",
                "type-is",
                "utils-merge"
            };

                modules = module.Modules;

                Console.WriteLine("module.Modules includes: {0}", string.Join(", ", modules.Select(m => m.Name)));

                Assert.AreEqual(module.PackageJson.Dependencies.Count, modules.Count, "Sub-module count mismatch.");
                foreach (var name in expectedModules) {
                    Console.WriteLine("Expecting {0}", name);
                    var current = modules[name];
                    Assert.IsNotNull(current, "Module should not be null when retrieved by name.");

                    Assert.AreEqual(name, current.Name, "Module name mismatch.");

                    Assert.IsNotNull(current.PackageJson, "Module package.json should not be null.");

                    Assert.IsTrue(
                        current.IsListedInParentPackageJson,
                        "Should be listed as a dependency in parent package.json.");
                    Assert.IsFalse(current.IsMissing, "Should not be marked as missing.");
                    Assert.IsFalse(current.IsDevDependency, "Should not be marked as dev dependency.");
                    Assert.IsFalse(current.IsOptionalDependency, "Should not be marked as optional dependency.");
                    Assert.IsFalse(current.IsBundledDependency, "Should not be marked as bundled dependency.");

                    //  Redundant?
                    Assert.IsTrue(current.HasPackageJson, "Module should have its own package.json");
                }
            }
        }
    }
}