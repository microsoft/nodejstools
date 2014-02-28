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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("3C3BD073-2AB3-4E66-BBE9-C8B2D8A774D1")]
    public class NpmNodeProperties : NodeProperties {
        internal NpmNodeProperties(AbstractNpmNode node) : base(node) {}

        private AbstractNpmNode NpmNode {
            get { return Node as AbstractNpmNode; }
        }

        private bool IsGlobalNode {
            get { return NpmNode is GlobalModulesNode; }
        }

        public override string GetClassName() {
            return IsGlobalNode ? Resources.PropertiesClassGlobal : Resources.PropertiesClassNpm;
        }

        [SRCategoryAttribute(SR.General)]
        [LocDisplayName(SR.NpmNodePackageInstallation)]
        [SRDescriptionAttribute(SR.NpmNodePackageInstallationDescription)]
        public string PackageInstallation {
            get {
                return IsGlobalNode
                    ? Resources.PackageInstallationGlobal
                    : Resources.PackageInstallationLocal;
            }
        }

        [SRCategoryAttribute(SR.General)]
        [LocDisplayName(SR.NpmNodePath)]
        [SRDescriptionAttribute(SR.NpmNodePathDescription)]
        public string Path {
            get {
                var node = NpmNode;
                if (null != node) {
                    var local = node as NodeModulesNode;
                    if (null != local) {
                        var root = local.RootPackage;
                        if (null != root) {
                            return root.Path;
                        }
                    } else {
                        var glob = node as GlobalModulesNode;
                        if (null != glob) {
                            var packages = glob.GlobalPackages;
                            if (null != packages) {
                                return packages.Path;
                            }
                        }
                    }
                }
                return null;
            }
        }
    }
}
