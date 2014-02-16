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
using System.Text;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    internal class DependencyNode : HierarchyNode {
        private readonly NodejsProjectNode _projectNode;
        private readonly DependencyNode _parent;
        private readonly string _displayString;

        public DependencyNode(
            NodejsProjectNode root,
            DependencyNode parent,
            IPackage package)
            : base(root) {
            _projectNode = root;
            _parent = parent;
            Package = package;

            var buff = new StringBuilder(package.Name);
            if (package.IsMissing) {
                buff.Append(" (missing)");
            } else {
                buff.Append('@');
                buff.Append(package.Version);

                if (!package.IsListedInParentPackageJson) {
                    buff.Append(" (not listed in package.json)");
                } else if (package.IsDevDependency) {
                    buff.Append(" (dev)");
                } else if (package.IsOptionalDependency) {
                    buff.Append(" (optional)");
                }
            }

            if (package.IsBundledDependency) {
                buff.Append("[bundled]");
            }

            _displayString = buff.ToString();
            ExcludeNodeFromScc = true;
        }

        public IPackage Package { get; internal set; }

        #region HierarchyNode implementation

        private string GetRelativeUrlFragment() {
            var buff = new StringBuilder();
            if (null != _parent) {
                buff.Append(_parent.GetRelativeUrlFragment());
                buff.Append('/');
            }
            buff.Append("node_modules/");
            buff.Append(Package.Name);
            return buff.ToString();
        }

        public override string Url {
            get { return new Url(ProjectMgr.BaseURI, GetRelativeUrlFragment()).AbsoluteUrl; }
        }

        public override string Caption {
            get { return _displayString; }
        }

        public override Guid ItemTypeGuid {
            get { return VSConstants.GUID_ItemType_VirtualFolder; }
        }

        public override int MenuCommandId {
            get { return VsMenus.IDM_VS_CTXT_ITEMNODE; }
        }

        public override object GetIconHandle(bool open) {
            int imageIndex = _projectNode.ImageIndexDependency;
            if (Package.IsMissing) {
                if (Package.IsDevDependency) {
                    imageIndex = _projectNode.ImageIndexDependencyDevMissing;
                } else if (Package.IsOptionalDependency) {
                    imageIndex = _projectNode.ImageIndexDependencyOptionalMissing;
                } else if (Package.IsBundledDependency) {
                    imageIndex = _projectNode.ImageIndexDependencyBundledMissing;
                } else {
                    imageIndex = _projectNode.ImageIndexDependencyMissing;
                }
            } else {
                if (!Package.IsListedInParentPackageJson) {
                    imageIndex = _projectNode.ImageIndexDependencyNotListed;
                } else if (Package.IsDevDependency) {
                    imageIndex = _projectNode.ImageIndexDependencyDev;
                } else if (Package.IsOptionalDependency) {
                    imageIndex = _projectNode.ImageIndexDependnecyOptional;
                } else if (Package.IsBundledDependency) {
                    imageIndex = _projectNode.ImageIndexDependencyBundled;
                } else {
                    imageIndex = _projectNode.ImageIndexDependency;
                }
            }

            return _projectNode.ImageHandler.GetIconHandle(imageIndex);
        }

        public override string GetEditLabel() {
            return null;
        }

        protected override NodeProperties CreatePropertiesObject() {
            return new DependencyNodeProperties(this);
        }

        internal DependencyNodeProperties GetPropertiesObject() {
            return CreatePropertiesObject() as DependencyNodeProperties;
        }

        #endregion

        #region Command handling

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result) {
            //  Latter condition is because it's only valid to carry out npm operations
            //  on top level dependencies of the user's project, not sub-dependencies.
            //  Performing operations on sub-dependencies would just break things.
            if (cmdGroup == Guids.NodejsCmdSet && null == _parent) {
                switch (cmd) {
                    case PkgCmdId.cmdidNpmInstallSingleMissingModule:
                        if (null == _projectNode.ModulesNode
                            || _projectNode.ModulesNode.IsCurrentStateASuppressCommandsMode()) {
                            result = QueryStatusResult.SUPPORTED;
                        } else {
                            if (null != Package && Package.IsMissing) {
                                result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                            } else {
                                result = QueryStatusResult.SUPPORTED;
                            }
                        }
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmUpdateSingleModule:
                    case PkgCmdId.cmdidNpmUninstallModule:
                        if (null != _projectNode.ModulesNode &&
                            !_projectNode.ModulesNode.IsCurrentStateASuppressCommandsMode()) {
                            result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                        } else {
                            result = QueryStatusResult.SUPPORTED;
                        }
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmManageModules:
                    case PkgCmdId.cmdidNpmInstallModules:
                    case PkgCmdId.cmdidNpmUpdateModules:
                        result = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                        return VSConstants.S_OK;
                }
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            if (cmdGroup == Guids.NodejsCmdSet && null == _parent) {
                switch (cmd) {
                    case PkgCmdId.cmdidNpmInstallSingleMissingModule:
                        if (null != _projectNode.ModulesNode) {
                            _projectNode.ModulesNode.InstallMissingModule(this);
                        }
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmUninstallModule:
                        if (null != _projectNode.ModulesNode) {
                            _projectNode.ModulesNode.UninstallModule(this);
                        }
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmUpdateSingleModule:
                        if (null != _projectNode.ModulesNode) {
                            _projectNode.ModulesNode.UpdateModule(this);
                        }
                        return VSConstants.S_OK;
                }
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        #endregion
    }
}