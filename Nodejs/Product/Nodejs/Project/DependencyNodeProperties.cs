/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

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

        private IPackageCatalog MostRecentlyLoadedCatalog {
            get {
                var controller = DependencyNode.NpmController;
                return null == controller ? null : controller.MostRecentlyLoadedCatalog;
            }
        }

        [SRCategoryAttribute(SR.CategoryVersion)]
        [SRDisplayName(SR.NpmPackageNewVersionAvailable)]
        [SRDescriptionAttribute(SR.NpmPackageNewVersionAvailableDescription)]
        public string NewVersionAvailable {
            get {
                if (IsSubPackage) {
                    return SR.GetString(SR.NewVersionNotApplicableSubpackage);
                }

                var package = Package;
                var catalog = MostRecentlyLoadedCatalog;
                if (null == catalog || null == package) {
                    return SR.GetString(SR.NewVersionUnknown);
                }

                var listed = catalog[package.Name];
                if (null == listed) {
                    return SR.GetString(SR.NewVersionUnknown);
                }

                return listed.Version > package.Version
                    ? SR.GetString(SR.NewVersionYes, listed.Version)
                    : SR.GetString(SR.NewVersionNo);
            }
        }

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
                    } else {
                        var global = controller.GlobalPackages;
                        if (null != global) {
                            var installed = global.Modules[package.Name];
                            return null == installed || installed.Version != package.Version
                                ? SR.GetString(SR.LinkStatusLocallyInstalled)
                                : SR.GetString(SR.LinkStatusLinkedFromGlobal);
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
