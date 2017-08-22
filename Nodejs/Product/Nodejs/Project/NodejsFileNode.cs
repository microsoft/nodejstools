// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project
{
    internal class NodejsFileNode : CommonFileNode
    {
        public NodejsFileNode(NodejsProjectNode root, ProjectElement e)
            : base(root, e)
        {
        }

        protected override ImageMoniker CodeFileIconMoniker => KnownMonikers.JSScript;

        internal override int IncludeInProject(bool includeChildren)
        {
            if (!this.ItemNode.IsExcluded)
            {
                return 0;
            }

            return base.IncludeInProject(includeChildren);
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            if (this.IsLinkFile)
            {
                return new NodejsLinkFileNodeProperties(this);
            }
            else if (this.IsNonMemberItem)
            {
                return new ExcludedFileNodeProperties(this);
            }

            return new NodejsIncludedFileNodeProperties(this);
        }

        public new NodejsProjectNode ProjectMgr => (NodejsProjectNode)base.ProjectMgr;
    }
}
