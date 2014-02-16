using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
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

        [SRCategoryAttribute(SR.General)]
        [LocDisplayName(SR.NpmPackageName)]
        [SRDescriptionAttribute(SR.NpmPackageNameDescription)]
        public string PackageName {
            get {
                return null == Package ? null : Package.Name;
            }
        }

        [SRCategoryAttribute(SR.CategoryVersion)]
        [LocDisplayName(SR.NpmPackageVersion)]
        [SRDescriptionAttribute(SR.NpmPackageVersionDescription)]
        public string PackageVersion {
            get {
                return null == Package ? null : Package.Version.ToString();
            }
        }

        [SRCategoryAttribute(SR.CategoryVersion)]
        [LocDisplayName(SR.NpmPackageRequestedVersionRange)]
        [SRDescriptionAttribute(SR.NpmPackageRequestedVersionRangeDescription)]
        public string RequestedVersionRange {
            get {
                var range = null == Package ? null : Package.RequestedVersionRange;
                return range ?? Resources.RequestedVersionRangeNone;
            }
        }

        [SRCategoryAttribute(SR.CategoryVersion)]
        [LocDisplayName(SR.NpmPackageNewVersionAvailable)]
        [SRDescriptionAttribute(SR.NpmPackageNewVersionAvailableDescription)]
        public string NewVersionAvailable {
            get {
                if (IsSubPackage) {
                    return Resources.NewVersionNotApplicableSubpackage;
                }

                return Resources.NewVersionUnknown;   //  TODO!!!
            }
        }

        [SRCategoryAttribute(SR.General)]
        [LocDisplayName(SR.NpmPackageDescription)]
        [SRDescriptionAttribute(SR.NpmPackageDescriptionDescription)]
        public string Description {
            get {
                return null == Package ? null : Package.Description;
            }
        }

        [SRCategoryAttribute(SR.General)]
        [LocDisplayName(SR.NpmPackageKeywords)]
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
        [LocDisplayName(SR.NpmPackageAuthor)]
        [SRDescriptionAttribute(SR.NpmPackageAuthorDescription)]
        public string Author {
            get {
                var author = null == Package ? null : Package.Author;
                return null == author ? null : author.ToString();
            }
        }

        [SRCategoryAttribute(SR.CategoryVersion)]
        [LocDisplayName(SR.NpmPackagePublishDateTime)]
        [SRDescriptionAttribute(SR.NpmPackagePublishDateTimeDescription)]
        public string PublishDateTime {
            get {
                return null == Package ? null : Package.PublishDateTimeString;
            }
        }

        [SRCategoryAttribute(SR.General)]
        [LocDisplayName(SR.NpmPackagePath)]
        [SRDescriptionAttribute(SR.NpmPackagePathDescription)]
        public string Path {
            get {
                return null == Package ? null : Package.Path;
            }
        }

        private bool IsGlobalInstall {
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

        private bool IsSubPackage {
            get {
                var node = DependencyNode as HierarchyNode;
                if (null != node && node.Parent is DependencyNode) {
                    return true;
                }
                return false;
            }
        }

        [SRCategoryAttribute(SR.General)]
        [LocDisplayName(SR.NpmPackageType)]
        [SRDescriptionAttribute(SR.NpmPackageTypeDescription)]
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

        [SRCategoryAttribute(SR.CategoryStatus)]
        [LocDisplayName(SR.NpmPackageIsListedInParentPackageJson)]
        [SRDescriptionAttribute(SR.NpmPackageIsListedInParentPackageJsonDescription)]
        public bool IsListedInParentPackageJson {
            get {
                return null != Package && Package.IsListedInParentPackageJson;
            }
        }

        [SRCategoryAttribute(SR.CategoryStatus)]
        [LocDisplayName(SR.NpmPackageIsMissing)]
        [SRDescriptionAttribute(SR.NpmPackageIsMissingDescription)]
        public bool IsMissing {
            get {
                return null != Package && Package.IsMissing;
            }
        }

        [SRCategoryAttribute(SR.CategoryStatus)]
        [LocDisplayName(SR.NpmPackageIsDevDependency)]
        [SRDescriptionAttribute(SR.NpmPackageIsDevDependencyDescription)]
        public bool IsDevDependency {
            get {
                return null != Package && Package.IsDevDependency;
            }
        }

        [SRCategoryAttribute(SR.CategoryStatus)]
        [LocDisplayName(SR.NpmPackageIsOptionalDependency)]
        [SRDescriptionAttribute(SR.NpmPackageIsOptionalDependencyDescription)]
        public bool IsOptionalDependency {
            get {
                return null != Package && Package.IsOptionalDependency;
            }
        }

        [SRCategoryAttribute(SR.CategoryStatus)]
        [LocDisplayName(SR.NpmPackageIsBundledDependency)]
        [SRDescriptionAttribute(SR.NpmPackageIsBundledDependencyDescription)]
        public bool IsBundledDependency {
            get {
                return null != Package && Package.IsBundledDependency;
            }
        }

    }
}
