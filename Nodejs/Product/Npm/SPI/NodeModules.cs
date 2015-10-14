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
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NodeModules : AbstractNodeModules {
        private Dictionary<string, ModuleInfo> _allModules;
        private readonly string[] _ignoredDirectories = { @"\.bin", @"\.staging" };

        public NodeModules(IRootPackage parent, bool showMissingDevOptionalSubPackages, Dictionary<string, ModuleInfo> allModulesToDepth = null, int depth = 0) {
            var modulesBase = Path.Combine(parent.Path, NodejsConstants.NodeModulesFolder);

            _allModules = allModulesToDepth ?? new Dictionary<string, ModuleInfo>();

            // This is the first time NodeModules is being created.
            // Iterate through directories to add everything that's known to be top-level.
            if (depth == 0) {
                Debug.Assert(_allModules.Count == 0, "Depth is 0, but top-level modules have already been added.");

                IEnumerable<string> topLevelDirectories = Enumerable.Empty<string>();
                try {
                    topLevelDirectories = Directory.EnumerateDirectories(modulesBase);
                } catch (IOException) {
                    // We want to handle DirectoryNotFound, DriveNotFound, PathTooLong
                } catch (UnauthorizedAccessException) {
                }

                // Go through every directory in node_modules, and see if it's required as a top-level dependency
                foreach (var moduleDir in topLevelDirectories) {
                    if (moduleDir.Length < NativeMethods.MAX_FOLDER_PATH && !_ignoredDirectories.Any(toIgnore => moduleDir.EndsWith(toIgnore))) {
                        IPackageJson packageJson;
                        try {
                            packageJson = PackageJsonFactory.Create(new DirectoryPackageJsonSource(moduleDir));
                        } catch (PackageJsonException) {
                            // Fail gracefully if there was an error parsing the package.json
                            Debug.Fail("Failed to parse package.json in {0}", moduleDir);
                            continue;
                        }

                        if (packageJson != null) {
                            if (packageJson.RequiredBy.Count() > 0) {
                                // All dependencies in npm v3 will have at least one element present in _requiredBy.
                                // _requiredBy dependencies that begin with hash characters represent top-level dependencies
                                foreach (var requiredBy in packageJson.RequiredBy) {
                                    if (requiredBy.StartsWith("#") || requiredBy == "/") {
                                        AddTopLevelModule(parent, showMissingDevOptionalSubPackages, moduleDir, depth);
                                        break;
                                    }
                                }
                            } else {
                                // This dependency is a top-level dependency not added by npm v3
                                AddTopLevelModule(parent, showMissingDevOptionalSubPackages, moduleDir, depth);
                            }
                        }
                    }
                }
            }

            if (modulesBase.Length < NativeMethods.MAX_FOLDER_PATH && parent.HasPackageJson) {
                // Iterate through all dependencies in the root package.json
                // Otherwise, only iterate through "dependencies" because iterating through optional, bundle, etc. dependencies
                // becomes unmanageable when they are already installed at the root of the project, and the performance impact
                // typically isn't worth the value add. 
                var dependencies = depth == 0 ? parent.PackageJson.AllDependencies : parent.PackageJson.Dependencies;
                foreach (var dependency in dependencies) {
                    var moduleDir = modulesBase;

                    // try to find folder by recursing up tree
                    do {
                        moduleDir = Path.Combine(moduleDir, dependency.Name);
                        if (AddModuleIfNotExists(parent, moduleDir, showMissingDevOptionalSubPackages, depth, dependency)) {
                            break;
                        }

                        var parentNodeModulesIndex = moduleDir.LastIndexOf(NodejsConstants.NodeModulesFolder, Math.Max(0, moduleDir.Length - NodejsConstants.NodeModulesFolder.Length - dependency.Name.Length - 1));
                        moduleDir = moduleDir.Substring(0, parentNodeModulesIndex + NodejsConstants.NodeModulesFolder.Length);
                    } while (moduleDir.Contains(NodejsConstants.NodeModulesFolder));
                }
            }

            _packagesSorted.Sort(new PackageComparer());
        }

        private void AddTopLevelModule(IRootPackage parent, bool showMissingDevOptionalSubPackages, string moduleDir, int depth) {
            Debug.Assert(depth == 0, "Depth should be 0 when adding a top level dependency");
            AddModuleIfNotExists(parent, moduleDir, showMissingDevOptionalSubPackages, depth);
        }

        private bool AddModuleIfNotExists(IRootPackage parent, string moduleDir, bool showMissingDevOptionalSubPackages, int depth, IDependency dependency = null) {
            depth++;

            ModuleInfo moduleInfo;
            _allModules.TryGetValue(moduleDir, out moduleInfo);

            if (moduleInfo != null) {
                // Update module information if the module already exists.
                if (moduleInfo.Depth > depth) {
                    moduleInfo.Depth = depth;
                }

                if (dependency != null) {
                    var existingPackage = this[dependency.Name] as Package;
                    if (existingPackage != null) {
                        existingPackage.RequestedVersionRange = dependency.VersionRangeText;
                    }
                }
            } else if (Directory.Exists(moduleDir) || depth == 1) {
                // Top-level modules are always added so we can include missing modules.
                moduleInfo = new ModuleInfo(depth);
                _allModules.Add(moduleDir, moduleInfo);
            } else {
                // The module directory wasn't found.
                return false;
            }

            IPackage package = moduleInfo.Package;
                        
            if (package == null || depth == 1 || !moduleInfo.RequiredBy.Contains(parent.Path)) {
                // Create a dummy value for the current package to prevent infinite loops
                moduleInfo.Package = new PackageProxy();

                moduleInfo.RequiredBy.Add(parent.Path);

                var pkg = new Package(parent, moduleDir, showMissingDevOptionalSubPackages, _allModules, depth);
                if (dependency != null) {
                    pkg.RequestedVersionRange = dependency.VersionRangeText;
                }

                package = moduleInfo.Package = pkg;
            }

            if (parent as IPackage == null || !package.IsMissing || showMissingDevOptionalSubPackages) {
                AddModule(package);
            }

            return true;
        }

        public override int GetDepth(string filepath) {
            var lastNodeModules = filepath.LastIndexOf(NodejsConstants.NodeModulesFolder + "\\");
            var directoryToSearch = filepath.IndexOf("\\", lastNodeModules + NodejsConstants.NodeModulesFolder.Length + 1);
            var directorySubString = directoryToSearch == -1 ? filepath : filepath.Substring(0, directoryToSearch);

            ModuleInfo value = null;
            _allModules.TryGetValue(directorySubString, out value);

            var depth = value != null ? value.Depth : 0;
            Debug.WriteLine("Module Depth: {0} [{1}]", filepath, depth);

            return depth;
        }
    }

    internal class ModuleInfo {
        public int Depth { get; set; }

        public IPackage Package { get; set; }

        public IList<string> RequiredBy { get; set; }

        internal ModuleInfo(int depth) {
            Depth = depth;
            RequiredBy = new List<string>();
        }
    }
}