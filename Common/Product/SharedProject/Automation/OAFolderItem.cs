// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE;

namespace Microsoft.VisualStudioTools.Project.Automation
{
    /// <summary>
    /// Represents an automation object for a folder in a project
    /// </summary>
    [ComVisible(true)]
    public class OAFolderItem : OAProjectItem
    {
        #region ctors
        internal OAFolderItem(OAProject project, FolderNode node)
            : base(project, node)
        {
        }

        #endregion

        private new FolderNode Node => (FolderNode)base.Node;

        #region overridden methods
        public override ProjectItems Collection
        {
            get
            {
                ProjectItems items = new OAProjectItems(this.Project, this.Node.Parent);
                return items;
            }
        }

        public override ProjectItems ProjectItems => new OAProjectItems(this.Project, this.Node);
        #endregion
    }
}
