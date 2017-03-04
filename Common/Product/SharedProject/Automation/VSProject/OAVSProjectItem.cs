// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE;
using VSLangProj;

namespace Microsoft.VisualStudioTools.Project.Automation
{
    /// <summary>
    /// Represents a language-specific project item
    /// </summary>
    [ComVisible(true)]
    public class OAVSProjectItem : VSProjectItem
    {
        #region fields
        private FileNode fileNode;
        #endregion

        #region ctors
        internal OAVSProjectItem(FileNode fileNode)
        {
            this.FileNode = fileNode;
        }
        #endregion

        #region VSProjectItem Members

        public virtual EnvDTE.Project ContainingProject => this.fileNode.ProjectMgr.GetAutomationObject() as EnvDTE.Project;
        public virtual ProjectItem ProjectItem => this.fileNode.GetAutomationObject() as ProjectItem;
        public virtual DTE DTE => (DTE)this.fileNode.ProjectMgr.Site.GetService(typeof(DTE));
        public void RunCustomTool()
        {
        }

        #endregion

        #region public properties
        /// <summary>
        /// File Node property
        /// </summary>
        internal FileNode FileNode
        {
            get
            {
                return this.fileNode;
            }
            set
            {
                this.fileNode = value;
            }
        }
        #endregion

    }
}

