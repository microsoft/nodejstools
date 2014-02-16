using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    internal class GlobalModulesNode : AbstractNpmNode {

        /// <summary>
        /// The caption to display for this node
        /// </summary>
        private const string _cCaption = "Global Modules";

        /// <summary>
        /// The virtual name of this node.
        /// </summary>
        public const string GlobalModulesVirtualName = "GlobalModules";

        private NodeModulesNode _parent;

        public GlobalModulesNode(NodejsProjectNode root, NodeModulesNode parent) : base(root) {
            _parent = parent;
        }

        public override object GetIconHandle(bool open) {
            return
                ProjectMgr.ImageHandler.GetIconHandle(
                    open ? (int)ProjectNode.ImageName.OpenReferenceFolder : (int)ProjectNode.ImageName.ReferenceFolder);
        }

        public override string Url{
            get { return GlobalModulesVirtualName; }
        }

        public override string Caption{ //  TODO: stick this string in a resource, along with the NodeModulesNode caption
            get { return _cCaption; }
        }

        internal IGlobalPackages GlobalPackages { get; set; }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result) {
            if (cmdGroup == Guids.NodejsCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidNpmManageModules:
                        result = _parent.IsCurrentStateASuppressCommandsMode()
                            ? QueryStatusResult.SUPPORTED
                            : QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmUpdateModules:
                        if (_parent.IsCurrentStateASuppressCommandsMode()) {
                            result = QueryStatusResult.SUPPORTED;
                        } else {
                            if (AllChildren.Any()) {
                                result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                            } else {
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
                    case PkgCmdId.cmdidNpmManageModules:
                        _parent.ManageModules();
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmUpdateModules:
                        _parent.UpdateModules(AllChildren.ToList());
                        return VSConstants.S_OK;
                }
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }
    }
}
