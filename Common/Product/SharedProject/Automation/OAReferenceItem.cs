// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudioTools.Project.Automation
{
    /// <summary>
    /// Represents the automation object equivalent to a ReferenceNode object
    /// </summary>
    [ComVisible(true), CLSCompliant(false)]
    public class OAReferenceItem : OAProjectItem
    {
        #region ctors
        internal OAReferenceItem(OAProject project, ReferenceNode node)
            : base(project, node)
        {
        }

        #endregion

        private new ReferenceNode Node => (ReferenceNode)base.Node;

        #region overridden methods
        /// <summary>
        /// Not implemented. If called throws invalid operation exception.
        /// </summary>
        public override void Delete()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Not implemented. If called throws invalid operation exception.
        /// </summary>
        /// <param name="viewKind"> A Constants. vsViewKind indicating the type of view to use.</param>
        /// <returns></returns>
        public override EnvDTE.Window Open(string viewKind)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Gets or sets the name of the object.
        /// </summary>
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets the ProjectItems collection containing the ProjectItem object supporting this property.
        /// </summary>
        public override EnvDTE.ProjectItems Collection
        {
            get
            {
                // Get the parent node (ReferenceContainerNode)
                var parentNode = this.Node.Parent as ReferenceContainerNode;
                Debug.Assert(parentNode != null, "Failed to get the parent node");

                // Get the ProjectItems object for the parent node
                if (parentNode != null)
                {
                    // The root node for the project
                    return ((OAReferenceFolderItem)parentNode.GetAutomationObject()).ProjectItems;
                }

                return null;
            }
        }
        #endregion
    }
}

