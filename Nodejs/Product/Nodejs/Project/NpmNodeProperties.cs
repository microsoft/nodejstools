// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("3C3BD073-2AB3-4E66-BBE9-C8B2D8A774D1")]
    public class NpmNodeProperties : NodeProperties
    {
        internal NpmNodeProperties(AbstractNpmNode node) : base(node) { }

        private AbstractNpmNode NpmNode => this.Node as AbstractNpmNode;
        public override string GetClassName()
        {
            return Resources.PropertiesClassNpm;
        }

        [SRCategory(SR.General)]
        [ResourcesDisplayName(nameof(Resources.NpmNodePackageInstallation))]
        [ResourcesDescription(nameof(Resources.NpmNodePackageInstallationDescription))]
        public string PackageInstallation => Resources.PackageInstallationLocal;

        [SRCategory(SR.General)]
        [ResourcesDisplayName(nameof(Resources.NpmNodePath))]
        [ResourcesDescription(nameof(Resources.NpmNodePathDescription))]
        public string Path
        {
            get
            {
                var node = this.NpmNode;
                if (null != node)
                {
                    if (node is NodeModulesNode local)
                    {
                        var root = local.RootPackage;
                        if (null != root)
                        {
                            return root.Path;
                        }
                    }
                }
                return null;
            }
        }
    }
}
