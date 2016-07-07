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

using System.Runtime.InteropServices;
using System.Text;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("CA6C9721-2F64-4A1F-99C9-C087F698CB34")]
    public class DependencyNodeProperties : NodeProperties {
        internal DependencyNodeProperties(DependencyNode node) : base(node) {}

        private DependencyNode DependencyNode { get { return Node as DependencyNode; } }

        private IPackage Package { get { return DependencyNode.Package; } }

        public override string GetClassName() {
            return SR.GetString(IsSubPackage
                ? (IsGlobalInstall ? SR.PropertiesClassGlobalSubPackage : SR.PropertiesClassLocalSubPackage)
                : (IsGlobalInstall ? SR.PropertiesClassGlobalPackage : SR.PropertiesClassLocalPackage)
            );
        }

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.NpmPackageName)]
        [SRDescriptionAttribute(SR.NpmPackageNameDescription)]
        public string PackageName {
            get {
                return null == Package ? null : Package.Name;
            }
        }

        [SRCategoryAttribute(SR.CategoryVersion)]
        [SRDisplayName(SR.NpmPackageVersion)]
        [SRDescriptionAttribute(SR.NpmPackageVersionDescription)]
        public string PackageVersion {
            get {
                return null == Package ? null : Package.Version.ToString();
            }
        }

        [SRCategoryAttribute(SR.CategoryVersion)]
        [SRDisplayName(SR.NpmPackageRequestedVersionRange)]
        [SRDescriptionAttribute(SR.NpmPackageRequestedVersionRangeDescription)]
        public string RequestedVersionRange {
            get {
                var range = null == Package ? null : Package.RequestedVersionRange;
                return range ?? SR.GetString(SR.RequestedVersionRangeNone);
            }
        }

        // TODO Retrieving the package information is currently too slow to include in properties pane.
        //[SRCategoryAttribute(SR.CategoryVersion)]
        //[SRDisplayName(SR.NpmPackageNewVersionAvailable)]
        //[SRDescriptionAttribute(SR.NpmPackageNewVersionAvailableDescription)]
        //public string NewVersionAvailable {
        //    get {
        //        if (IsSubPackage) {
        //            return SR.GetString(SR.NewVersionNotApplicableSubpackage);
        //        }

        //        var package = Package;
        //        var catalog = MostRecentlyLoadedCatalog;
        //        if (null == catalog || null == package) {
        //            return SR.GetString(SR.NewVersionUnknown);
        //        }

        //        var listed = catalog[package.Name];
        //        if (null == listed) {
        //            return SR.GetString(SR.NewVersionUnknown);
        //        }

        //        return listed.Version > package.Version
        //            ? SR.GetString(SR.NewVersionYes, listed.Version)
        //            : SR.GetString(SR.NewVersionNo);
        //    }
        //}

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.NpmPackageDescription)]
        [SRDescriptionAttribute(SR.NpmPackageDescriptionDescription)]
        public string Description {
            get {
                return null == Package ? null : Package.Description;
            }
        }

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.NpmPackageKeywords)]
        [SRDescriptionAttribute(SR.NpmPackageKeywordsDescription)]
        public string Keywords {
            get {
                if (null == Package) {
                    return null;
                }

                var buff = new StringBuilder();
                foreach (var keyword in Package.Keywords) {
                    if (buff.Length > 0) {
                        buff.Append(", ");
                    }
                    buff.Append(keyword);
                }
                return buff.ToString();
            }
        }

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.NpmPackageAuthor)]
        [SRDescriptionAttribute(SR.NpmPackageAuthorDescription)]
        public string Author {
            get {
                var author = null == Package ? null : Package.Author;
                return null == author ? null : author.ToString();
            }
        }

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.NpmPackagePath)]
        [SRDescriptionAttribute(SR.NpmPackagePathDescription)]
        public string Path {
            get {
                return null == Package ? null : Package.Path;
            }
        }

        internal bool IsGlobalInstall {
            get {
                return false;
            }
        }

        internal bool IsSubPackage {
            get {
                var node = DependencyNode as HierarchyNode;
                if (null != node && node.Parent is DependencyNode) {
                    return true;
                }
                return false;
            }
        }

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.NpmPackageType)]
        [SRDescriptionAttribute(SR.NpmPackageTypeDescription)]
        public string PackageType {
            get {
                if (IsGlobalInstall) {
                    return IsSubPackage
                        ? SR.GetString(SR.PackageTypeGlobalSubpackage)
                        : SR.GetString(SR.PackageTypeGlobal);
                }

                return IsSubPackage
                    ? SR.GetString(SR.PackageTypeLocalSubpackage)
                    : SR.GetString(SR.PackageTypeLocal);
            }
        }


        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.NpmPackageLinkStatus)]
        [SRDescriptionAttribute(SR.NpmPackageLinkStatusDescription)]
        public string LinkStatus {
            get {
                if (IsSubPackage) {
                    return SR.GetString(SR.LinkStatusNotApplicableSubPackages);
                }

                var package = Package;
                var controller = DependencyNode.NpmController;
                if (null != controller && null != package) {
                    if (IsGlobalInstall) {
                        var root = controller.RootPackage;
                        if (null != root) {
                            var local = root.Modules[package.Name];
                            return null == local || local.Version != package.Version
                                ? SR.GetString(SR.LinkStatusNotLinkedToProject)
                                : SR.GetString(SR.LinkStatusLinkedToProject);
                        }
                    }
                }

                return SR.GetString(SR.LinkStatusUnknown);
            }
        }

        [SRCategoryAttribute(SR.CategoryStatus)]
        [SRDisplayName(SR.NpmPackageIsListedInParentPackageJson)]
        [SRDescriptionAttribute(SR.NpmPackageIsListedInParentPackageJsonDescription)]
        public bool IsListedInParentPackageJson {
            get {
                return null != Package && Package.IsListedInParentPackageJson;
            }
        }

        [SRCategoryAttribute(SR.CategoryStatus)]
        [SRDisplayName(SR.NpmPackageIsMissing)]
        [SRDescriptionAttribute(SR.NpmPackageIsMissingDescription)]
        public bool IsMissing {
            get {
                return null != Package && Package.IsMissing;
            }
        }

        [SRCategoryAttribute(SR.CategoryStatus)]
        [SRDisplayName(SR.NpmPackageIsDevDependency)]
        [SRDescriptionAttribute(SR.NpmPackageIsDevDependencyDescription)]
        public bool IsDevDependency {
            get {
                return null != Package && Package.IsDevDependency;
            }
        }

        [SRCategoryAttribute(SR.CategoryStatus)]
        [SRDisplayName(SR.NpmPackageIsOptionalDependency)]
        [SRDescriptionAttribute(SR.NpmPackageIsOptionalDependencyDescription)]
        public bool IsOptionalDependency {
            get {
                return null != Package && Package.IsOptionalDependency;
            }
        }

        [SRCategoryAttribute(SR.CategoryStatus)]
        [SRDisplayName(SR.NpmPackageIsBundledDependency)]
        [SRDescriptionAttribute(SR.NpmPackageIsBundledDependencyDescription)]
        public bool IsBundledDependency {
            get {
                return null != Package && Package.IsBundledDependency;
            }
        }

    }
}
