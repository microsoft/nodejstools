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
        private Dictionary<string, int> _allModulesToDepth;
        private readonly string[] _ignoredDirectories = { @"\.bin", @"\.staging" };

        public NodeModules(IRootPackage parent, bool showMissingDevOptionalSubPackages, Dictionary<string, int> allModulesToDepth = null, int depth = 0) {
            var modulesBase = Path.Combine(parent.Path, NodejsConstants.NodeModulesFolder);

            _allModulesToDepth = allModulesToDepth ?? new Dictionary<string, int>();

            // This is the first time NodeModules is being created.
            // Iterate through directories to add everything that's known to be top-level.
            if (depth == 0) {
                Debug.Assert(_allModulesToDepth.Count == 0, "Depth is 0, but top-level modules have already been added.");

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
                        var packageJson = PackageJsonFactory.Create(new DirectoryPackageJsonSource(moduleDir));

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
                // Iterate through all dependencies in package.json
                foreach (var dependency in parent.PackageJson.AllDependencies) {
                    var moduleDir = modulesBase;

                    // try to find folder by recursing up tree
                    do {
                        if (AddModuleIfNotExists(parent, dependency, moduleDir, showMissingDevOptionalSubPackages, depth)) {
                            break;
                        }

                        var parentNodeModulesIndex = moduleDir.LastIndexOf(NodejsConstants.NodeModulesFolder, Math.Max(0, moduleDir.Length - NodejsConstants.NodeModulesFolder.Length - 1));
                        moduleDir = moduleDir.Substring(0, parentNodeModulesIndex + NodejsConstants.NodeModulesFolder.Length);
                    } while (moduleDir.Contains(NodejsConstants.NodeModulesFolder));
                }
            }

            _packagesSorted.Sort(new PackageComparer());
        }

        private void AddTopLevelModule(IRootPackage parent, bool showMissingDevOptionalSubPackages, string moduleDir, int depth) {
            Debug.Assert(depth == 0, "Depth should be 0 when adding a top level dependency");

            depth++;

            if (_allModulesToDepth.ContainsKey(moduleDir)) {
                if (_allModulesToDepth[moduleDir] > depth) {
                    _allModulesToDepth[moduleDir] = depth;
                }
            } else {
                _allModulesToDepth.Add(moduleDir, depth);
            }

            if (Directory.Exists(moduleDir)) {
                AddModule(new Package(parent, moduleDir, showMissingDevOptionalSubPackages, _allModulesToDepth, depth));
            }
        }

        private bool AddModuleIfNotExists(IRootPackage parent, IDependency dependency, string moduleDir, bool showMissingDevOptionalSubPackages, int depth) {
            moduleDir = Path.Combine(moduleDir, dependency.Name);
            depth++;

            if (_allModulesToDepth.ContainsKey(moduleDir)) {
                if (_allModulesToDepth[moduleDir] > depth) {
                    _allModulesToDepth[moduleDir] = depth;
                }

                var package = this[dependency.Name] as Package;
                if (package != null) {
                    package.RequestedVersionRange = dependency.VersionRangeText;
                }
                // prevents infinite loops
                return true;
            }

            if (Directory.Exists(moduleDir)) {
                _allModulesToDepth.Add(moduleDir, depth);
                AddModule(new Package(parent, moduleDir, showMissingDevOptionalSubPackages, _allModulesToDepth, depth) {
                    RequestedVersionRange = dependency.VersionRangeText
                });
                return true;
            }

            return false;
        }

        public override int GetDepth(string filepath) {
            var lastNodeModules = filepath.LastIndexOf(NodejsConstants.NodeModulesFolder + "\\");
            var directoryToSearch = filepath.IndexOf("\\", lastNodeModules + NodejsConstants.NodeModulesFolder.Length);
            var directorySubString = directoryToSearch == -1 ? filepath : filepath.Substring(0, directoryToSearch);

            int value = 0;
            _allModulesToDepth.TryGetValue(directorySubString, out value);
            Debug.WriteLine(filepath + " : " + value);

            return value;
        }
    }
}