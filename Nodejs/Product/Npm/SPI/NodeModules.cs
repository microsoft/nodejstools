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

using System.IO;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NodeModules : AbstractNodeModules {
        public NodeModules(IRootPackage parent, bool showMissingDevOptionalSubPackages) {
            var modulesBase = Path.Combine(parent.Path, "node_modules");
            if (modulesBase.Length < NativeMethods.MAX_FOLDER_PATH && Directory.Exists(modulesBase)) {
                var bin = string.Format("{0}.bin", Path.DirectorySeparatorChar);
                foreach (var moduleDir in Directory.EnumerateDirectories(modulesBase)) {
                    if (moduleDir.Length < NativeMethods.MAX_FOLDER_PATH && !moduleDir.EndsWith(bin)) {
                        AddModule(new Package(parent, moduleDir, showMissingDevOptionalSubPackages));
                    }
                }
            }

            var parentPackageJson = parent.PackageJson;
            if (null != parentPackageJson) {
                foreach (var dependency in parentPackageJson.AllDependencies) {
                    Package module = null;
                    if (!Contains(dependency.Name)) {
                        var dependencyPath = Path.Combine(modulesBase, dependency.Name);
                        if (dependencyPath.Length < NativeMethods.MAX_FOLDER_PATH) {
                            module = new Package(
                                parent,
                                dependencyPath,
                                showMissingDevOptionalSubPackages);
                            if (parent as IPackage == null || !module.IsMissing || showMissingDevOptionalSubPackages) {
                                AddModule(module);
                            }
                        }
                    } else {
                        module = this[dependency.Name] as Package;
                    }

                    if (null != module) {
                        module.RequestedVersionRange = dependency.VersionRangeText;
                    }
                }
            }

            _packagesSorted.Sort(new PackageComparer());
        }
    }
}