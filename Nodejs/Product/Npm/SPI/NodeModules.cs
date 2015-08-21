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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NodeModules : AbstractNodeModules {
        private Dictionary<string, int> _allModules;

        public NodeModules(IRootPackage parent, bool showMissingDevOptionalSubPackages, Dictionary<string, int> allModules = null, int depth = 0) {
            var modulesBase = Path.Combine(parent.Path, NodejsConstants.NodeModulesFolder);

            _allModules = allModules;

            // This is the first time NodeModules is being created.
            // Iterate through directories to add everything that's known to be top-level.
            if (_allModules == null) {
                _allModules = new Dictionary<string, int>();

                IEnumerable<string> directories = null;
                try {
                    directories = Directory.EnumerateDirectories(modulesBase);
                } catch (IOException) {
                    // We want to handle DirectoryNotFound, DriveNotFound, PathTooLong
                } catch (UnauthorizedAccessException) {
                }

                if (directories != null) {
                    var bin = string.Format("{0}.bin", Path.DirectorySeparatorChar);
                    var staging = string.Format("{0}.staging", Path.DirectorySeparatorChar);

                    // Go through every directory in node_modules, and see if it's required as a top-level dependency
                    // We can check this by looking at whether it begins with a hash tag. 
                    foreach (var moduleDir in directories) {
                        if (moduleDir.Length < NativeMethods.MAX_FOLDER_PATH && !moduleDir.EndsWith(bin) && !moduleDir.EndsWith(staging)) {
                            var packageJson = PackageJsonFactory.Create(new DirectoryPackageJsonSource(moduleDir));

                            if (packageJson != null) {
                                var enumerator = packageJson.RequiredBy.GetEnumerator();
                                if (packageJson.RequiredBy.Count() > 0) {
                                    // All dependencies in npm v3 will have elements present in _requiredBy.
                                    // _requiredBy dependencies that begin with hash characters represent top-level dependencies
                                    while (enumerator.MoveNext()) {
                                        if (enumerator.Current.StartsWith("#")) {
                                            AddModule(parent, showMissingDevOptionalSubPackages, depth, moduleDir);
                                            break;
                                        }
                                    }
                                } else {
                                    // This dependency is a top-level dependency not added by npm v3
                                    AddModule(parent, showMissingDevOptionalSubPackages, depth, moduleDir);
                                }
                            }
                        }
                    }
                }
            }


            if (modulesBase.Length < NativeMethods.MAX_FOLDER_PATH) {
                if (null != parent.PackageJson) {
                    // Iterate through all dependencies in package.json
                    foreach (var dependency in parent.PackageJson.AllDependencies) {
                        var moduleDir = modulesBase;

                        // try to find folder by recursing up tree
                        do {
                            moduleDir = Path.Combine(moduleDir, dependency.Name);
                            if (_allModules.ContainsKey(moduleDir)) {
                                if (_allModules[moduleDir] > depth + 1) {
                                    _allModules[moduleDir] = depth + 1;
                                }
                                var package = this[dependency.Name] as Package;
                                if (package != null) {
                                    package.RequestedVersionRange = dependency.VersionRangeText;
                                }
                                // prevents infinite loops
                                break;
                            }

                            if (Directory.Exists(moduleDir)) {
                                _allModules.Add(moduleDir, depth + 1);
                                AddModule(new Package(parent, moduleDir, showMissingDevOptionalSubPackages, _allModules, depth + 1) {
                                    RequestedVersionRange = dependency.VersionRangeText
                                });

                                break;
                            }
                            
                            var parentNodeModulesIndex = moduleDir.LastIndexOf(NodejsConstants.NodeModulesFolder, 0, Math.Max(0, moduleDir.Length - NodejsConstants.NodeModulesFolder.Length));
                            moduleDir = moduleDir.Substring(0, parentNodeModulesIndex + NodejsConstants.NodeModulesFolder.Length);
                        } while (moduleDir.Contains(NodejsConstants.NodeModulesFolder));
                    }
                }
            }

            _packagesSorted.Sort(new PackageComparer());
        }

        private void AddModule(IRootPackage parent, bool showMissingDevOptionalSubPackages, int depth, string moduleDir) {
            if (_allModules.ContainsKey(moduleDir)) {
                if (_allModules[moduleDir] > depth + 1) {
                    _allModules[moduleDir] = depth + 1;
                }
            } else {
                _allModules.Add(moduleDir, depth + 1);
                AddModule(new Package(parent, moduleDir, showMissingDevOptionalSubPackages, _allModules, depth + 1));
            }
        }

        public override int GetDepth(string filepath) {
            int value;

            var lastNodeModules = filepath.LastIndexOf(NodejsConstants.NodeModulesFolder + "\\");
            var directoryToSearch = filepath.IndexOf("\\", lastNodeModules + NodejsConstants.NodeModulesFolder.Length);
            var directorySubString = directoryToSearch == -1 ? filepath : filepath.Substring(0, directoryToSearch);

            bool gotValue = _allModules.TryGetValue(directorySubString, out value);

            Debug.WriteLine(filepath + " : " + value);
            return gotValue ? value : 0;
        }
    }
}