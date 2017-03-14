// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudioTools.Project.Automation
{
    /// <summary>
    /// Contains OAReferenceItem objects 
    /// </summary>
    [ComVisible(true), CLSCompliant(false)]
    public class OAReferenceFolderItem : OAProjectItem
    {
        #region ctors
        internal OAReferenceFolderItem(OAProject project, ReferenceContainerNode node)
            : base(project, node)
        {
        }

        #endregion

        private new ReferenceContainerNode Node => (ReferenceContainerNode)base.Node;

        #region overridden methods
        /// <summary>
        /// Returns the project items collection of all the references defined for this project.
        /// </summary>
        public override EnvDTE.ProjectItems ProjectItems => new OANavigableProjectItems(this.Project, this.Node);

        #endregion
    }
}

