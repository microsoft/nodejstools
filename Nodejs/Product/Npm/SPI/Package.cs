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
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class Package : RootPackage, IPackage {
        private IRootPackage _parent;

        public Package(
            IRootPackage parent,
            string fullPathToRootDirectory,
            bool showMissingDevOptionalSubPackages,
            Dictionary<string, ModuleInfo> allModules = null,
            int depth = 0)
            : base(fullPathToRootDirectory, showMissingDevOptionalSubPackages, allModules, depth) {
            _parent = parent;
        }

        public string PublishDateTimeString { get { return null; } }

        public IEnumerable<SemverVersion> AvailableVersions { get { throw new NotImplementedException(); } }

        public string RequestedVersionRange { get; internal set; }

        public IEnumerable<string> Keywords {
            get {
                var keywords = null == PackageJson ? null : PackageJson.Keywords;
                return keywords ?? (IEnumerable<string>) new List<string>();
            }
        }

        public bool IsListedInParentPackageJson {
            get {
                IPackageJson parentPackageJson = _parent.PackageJson;
                return _parent is IGlobalPackages ||
                       (null != parentPackageJson && parentPackageJson.AllDependencies.Contains(Name));
            }
        }

        public bool IsMissing {
            get {
                if (!IsListedInParentPackageJson)
                    return false;
                // Limit execution time of check
                var task = new Task<bool>(() => Directory.Exists(Path));
                task.Start();
                return !task.Wait(250) || !task.Result;
            }
        }

        public bool IsDevDependency {
            get {
                IPackageJson parentPackageJson = _parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.DevDependencies.Contains(Name);
            }
        }

        public bool IsDependency {
            get {
                IPackageJson parentPackageJson = _parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.Dependencies.Contains(Name);
            }
        }

        public bool IsOptionalDependency {
            get {
                IPackageJson parentPackageJson = _parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.OptionalDependencies.Contains(Name);
            }
        }

        public bool IsBundledDependency {
            get {
                IPackageJson parentPackageJson = _parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.BundledDependencies.Contains(Name);
            }
        }

        public PackageFlags Flags {
            get {
                return (!IsListedInParentPackageJson ? PackageFlags.NotListedAsDependency : 0)
                       | (IsMissing ? PackageFlags.Missing : 0)
                       | (IsDevDependency ? PackageFlags.Dev : 0)
                       | (IsOptionalDependency ? PackageFlags.Optional : 0)
                       | (IsBundledDependency ? PackageFlags.Bundled : 0);
            }
        }

        public override string ToString() {
            return string.Format("{0} {1}", Name, Version);
        }
    }
}