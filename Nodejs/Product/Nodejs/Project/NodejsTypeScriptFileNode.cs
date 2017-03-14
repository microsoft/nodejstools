// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;

namespace Microsoft.NodejsTools.Project
{
    internal class NodejsTypeScriptFileNode : NodejsFileNode
    {
        public NodejsTypeScriptFileNode(NodejsProjectNode root, ProjectElement e)
            : base(root, e)
        {
        }

        protected override ImageMoniker CodeFileIconMoniker => KnownMonikers.TSFileNode;

        protected override NodeProperties CreatePropertiesObject()
        {
            if (this.IsLinkFile)
            {
                return new NodejsTypeScriptLinkFileNodeProperties(this);
            }
            else if (this.IsNonMemberItem)
            {
                return new ExcludedFileNodeProperties(this);
            }

            return new NodejsTypeScriptFileNodeProperties(this);
        }

        public new NodejsProjectNode ProjectMgr => (NodejsProjectNode)base.ProjectMgr;
    }
}

