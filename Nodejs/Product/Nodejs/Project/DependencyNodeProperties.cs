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
            return (IsSubPackage
                ? (IsGlobalInstall ? Resources.PropertiesClassGlobalSubPackage : Resources.PropertiesClassLocalSubPackage)
                : (IsGlobalInstall ? Resources.PropertiesClassGlobalPackage : Resources.PropertiesClassLocalPackage)
            );
        }

        [SRCategoryAttribute(NodeJsProjectSr.General)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageName)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageNameDescription)]
        public string PackageName {
            get {
                return null == Package ? null : Package.Name;
            }
        }

        [SRCategoryAttribute(NodeJsProjectSr.CategoryVersion)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageVersion)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageVersionDescription)]
        public string PackageVersion {
            get {
                return null == Package ? null : Package.Version.ToString();
            }
        }

        [SRCategoryAttribute(NodeJsProjectSr.CategoryVersion)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageRequestedVersionRange)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageRequestedVersionRangeDescription)]
        public string RequestedVersionRange {
            get {
                var range = null == Package ? null : Package.RequestedVersionRange;
                return range ?? Resources.RequestedVersionRangeNone;
            }
        }

        private IPackageCatalog MostRecentlyLoadedCatalog {
            get {
                var controller = DependencyNode.NpmController;
                return null == controller ? null : controller.MostRecentlyLoadedCatalog;
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

        [SRCategoryAttribute(NodeJsProjectSr.General)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageDescription)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageDescriptionDescription)]
        public string Description {
            get {
                return null == Package ? null : Package.Description;
            }
        }

        [SRCategoryAttribute(NodeJsProjectSr.General)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageKeywords)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageKeywordsDescription)]
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

        [SRCategoryAttribute(NodeJsProjectSr.General)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageAuthor)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageAuthorDescription)]
        public string Author {
            get {
                var author = null == Package ? null : Package.Author;
                return null == author ? null : author.ToString();
            }
        }

        [SRCategoryAttribute(NodeJsProjectSr.General)]
        [SRDisplayName(NodeJsProjectSr.NpmPackagePath)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackagePathDescription)]
        public string Path {
            get {
                return null == Package ? null : Package.Path;
            }
        }

        internal bool IsGlobalInstall {
            get {
                var node = DependencyNode as HierarchyNode;
                while (null != node) {
                    if (node is GlobalModulesNode) {
                        return true;
                    }

                    node = node.Parent;
                }
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

        [SRCategoryAttribute(NodeJsProjectSr.General)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageType)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageTypeDescription)]
        public string PackageType {
            get {
                if (IsGlobalInstall) {
                    return IsSubPackage
                        ? Resources.PackageTypeGlobalSubpackage
                        : Resources.PackageTypeGlobal;
                }

                return IsSubPackage
                    ? Resources.PackageTypeLocalSubpackage
                    : Resources.PackageTypeLocal;
            }
        }


        [SRCategoryAttribute(NodeJsProjectSr.General)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageLinkStatus)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageLinkStatusDescription)]
        public string LinkStatus {
            get {
                if (IsSubPackage) {
                    return Resources.LinkStatusNotApplicableSubPackages;
                }

                var package = Package;
                var controller = DependencyNode.NpmController;
                if (null != controller && null != package) {
                    if (IsGlobalInstall) {
                        var root = controller.RootPackage;
                        if (null != root) {
                            var local = root.Modules[package.Name];
                            return null == local || local.Version != package.Version
                                ? Resources.LinkStatusNotLinkedToProject
                                : Resources.LinkStatusLinkedToProject;
                        }
                    } else {
                        var global = controller.GlobalPackages;
                        if (null != global) {
                            var installed = global.Modules[package.Name];
                            return null == installed || installed.Version != package.Version
                                ? Resources.LinkStatusLocallyInstalled
                                : Resources.LinkStatusLinkedFromGlobal;
                        }
                    }
                }

                return Resources.LinkStatusUnknown;
            }
        }

        [SRCategoryAttribute(NodeJsProjectSr.CategoryStatus)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageIsListedInParentPackageJson)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageIsListedInParentPackageJsonDescription)]
        public bool IsListedInParentPackageJson {
            get {
                return null != Package && Package.IsListedInParentPackageJson;
            }
        }

        [SRCategoryAttribute(NodeJsProjectSr.CategoryStatus)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageIsMissing)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageIsMissingDescription)]
        public bool IsMissing {
            get {
                return null != Package && Package.IsMissing;
            }
        }

        [SRCategoryAttribute(NodeJsProjectSr.CategoryStatus)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageIsDevDependency)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageIsDevDependencyDescription)]
        public bool IsDevDependency {
            get {
                return null != Package && Package.IsDevDependency;
            }
        }

        [SRCategoryAttribute(NodeJsProjectSr.CategoryStatus)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageIsOptionalDependency)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageIsOptionalDependencyDescription)]
        public bool IsOptionalDependency {
            get {
                return null != Package && Package.IsOptionalDependency;
            }
        }

        [SRCategoryAttribute(NodeJsProjectSr.CategoryStatus)]
        [SRDisplayName(NodeJsProjectSr.NpmPackageIsBundledDependency)]
        [SRDescriptionAttribute(NodeJsProjectSr.NpmPackageIsBundledDependencyDescription)]
        public bool IsBundledDependency {
            get {
                return null != Package && Package.IsBundledDependency;
            }
        }

    }
}
