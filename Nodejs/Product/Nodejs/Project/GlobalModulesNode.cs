using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
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

        //  TODO: is there a way to support tooltips on hierarchy nodes?

        //  TODO: properties for this node should include location of global modules on disk

        //  TODO: ability to open explorer window in global modules folder? "Show in Windows Explorer"?
    }
}
