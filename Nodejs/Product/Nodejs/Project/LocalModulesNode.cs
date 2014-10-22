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
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    internal class LocalModulesNode : AbstractNpmNode {

        /// <summary>
        /// The caption to display for this node
        /// </summary>
        private string _caption;

        private NodeModulesNode _parent;
        IEnumerable<IPackage> _packages = new List<IPackage>();


        public LocalModulesNode(NodejsProjectNode root, NodeModulesNode parent, string caption, string virtualName, DependencyType dependencyType)
            : base(root) {
            _parent = parent;
            _caption = caption;
            VirtualName = virtualName;
            PackagesDependencyType = dependencyType;
        }

        public DependencyType PackagesDependencyType { get; private set; }

        public string VirtualName { get; private set; }

        public override string Url {
            get { return VirtualName; }
        }

        public override string Caption {
            get { return _caption; }
        }

        public override int SortPriority {
            get { return -1; /* DefaultSortOrderNode.FolderNode; */ }
        }

        internal IEnumerable<IPackage> Packages {
            get {
                return _packages;
            }
            set {
                _packages = value;
                IsVisible = value == null || value.Any();
                this.ProjectMgr.OnInvalidateItems(_parent);
            }
        }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result) {
            if (cmdGroup == Guids.NodejsCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidNpmUpdateModules:
                        if (_parent.IsCurrentStateASuppressCommandsMode()) {
                            result = QueryStatusResult.SUPPORTED;
                        }
                        else {
                            if (AllChildren.Any()) {
                                result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                            }
                            else {
                                result = QueryStatusResult.SUPPORTED;
                            }
                        }
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmInstallModules:
                    case PkgCmdId.cmdidNpmInstallSingleMissingModule:
                    case PkgCmdId.cmdidNpmUninstallModule:
                    case PkgCmdId.cmdidNpmUpdateSingleModule:
                        result = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                        return VSConstants.S_OK;
                }
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            if (cmdGroup == Guids.NodejsCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidNpmUpdateModules:
                        var t = _parent.UpdateModules(AllChildren.ToList());
                        return VSConstants.S_OK;
                }
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        public override void ManageNpmModules() {
            _parent.ManageModules(PackagesDependencyType);
        }
    }
}