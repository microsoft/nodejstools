// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project
{
    internal class LocalModulesNode : AbstractNpmNode
    {
        /// <summary>
        /// The caption to display for this node
        /// </summary>
        private string _caption;

        private NodeModulesNode _parent;
        private IEnumerable<IPackage> _packages = new List<IPackage>();

        public LocalModulesNode(NodejsProjectNode root, NodeModulesNode parent, string caption, string virtualName, DependencyType dependencyType)
            : base(root)
        {
            this._parent = parent;
            this._caption = caption;
            this.VirtualName = virtualName;
            this.PackagesDependencyType = dependencyType;
        }

        public DependencyType PackagesDependencyType { get; }

        public string VirtualName { get; }

        public override string Url => this.VirtualName;
        public override string Caption => this._caption;
        public override int SortPriority => -1; /* DefaultSortOrderNode.FolderNode; */
        internal IEnumerable<IPackage> Packages
        {
            get
            {
                return this._packages;
            }
            set
            {
                this._packages = value;
                this.IsVisible = value == null || value.Any();
                this.ProjectMgr.OnInvalidateItems(this._parent);
            }
        }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (cmdGroup == Guids.NodejsNpmCmdSet)
            {
                switch (cmd)
                {
                    case PkgCmdId.cmdidNpmUpdateModules:
                        if (this._parent.IsCurrentStateASuppressCommandsMode())
                        {
                            result = QueryStatusResult.SUPPORTED;
                        }
                        else
                        {
                            if (this.AllChildren.Any())
                            {
                                result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                            }
                            else
                            {
                                result = QueryStatusResult.SUPPORTED;
                            }
                        }
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmInstallModules:
                    case PkgCmdId.cmdidNpmInstallSingleMissingModule:
                    case PkgCmdId.cmdidNpmUninstallModule:
                    case PkgCmdId.cmdidNpmUpdateSingleModule:
                    case PkgCmdId.cmdidNpmOpenModuleHomepage:
                        result = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                        return VSConstants.S_OK;
                }
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (cmdGroup == Guids.NodejsNpmCmdSet)
            {
                switch (cmd)
                {
                    case PkgCmdId.cmdidNpmUpdateModules:
                        var t = this._parent.UpdateModules(this.AllChildren.ToList());
                        return VSConstants.S_OK;
                }
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        public override void ManageNpmModules()
        {
            this._parent.ManageModules(this.PackagesDependencyType);
        }
    }
}
