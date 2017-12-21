// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project
{
    internal class NodejsFolderNode : CommonFolderNode
    {
        private readonly CommonProjectNode _project;

        public NodejsFolderNode(CommonProjectNode root, ProjectElement element) : base(root, element)
        {
            this._project = root;
        }

        public override string Caption => base.Caption;

        public override void RemoveChild(HierarchyNode node)
        {
            base.RemoveChild(node);
        }

        internal override int IncludeInProject(bool includeChildren)
        {
            // Include node_modules folder is generally unecessary and can cause VS to hang.
            // http://nodejstools.codeplex.com/workitem/1432
            // Check if the folder is node_modules, and warn the user to ensure they don't run into this issue or at least set expectations appropriately.
            var nodeModulesPath = Path.Combine(this._project.FullPathToChildren, NodejsConstants.NodeModulesFolder);
            if (CommonUtils.IsSameDirectory(nodeModulesPath, this.ItemNode.Url))
            {
                Utilities.ShowMessageBox(
                    this._project.Site, Resources.IncludeNodeModulesContent, SR.ProductName, OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return VSConstants.S_OK;
            }
            return base.IncludeInProject(includeChildren);
        }
    }
}
