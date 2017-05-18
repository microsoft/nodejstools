// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;

namespace Microsoft.NodejsTools.Project
{
    internal class NodejsFileNode : CommonFileNode
    {
        public NodejsFileNode(NodejsProjectNode root, ProjectElement e)
            : base(root, e)
        {
        }

        protected override void OnParentSet(HierarchyNode parent)
        {
            if (this.ProjectMgr == null)
            {
                return;
            }

            if (this.Url.EndsWith(NodejsConstants.TypeScriptDeclarationExtension, StringComparison.OrdinalIgnoreCase)
              && this.Url.StartsWith(Path.Combine(this.ProjectMgr.ProjectFolder, @"typings\"), StringComparison.OrdinalIgnoreCase))
            {
                this.ProjectMgr.Site.GetUIThread().Invoke(() =>
                {
                    this.IncludeInProject(true);
                });
            }
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
