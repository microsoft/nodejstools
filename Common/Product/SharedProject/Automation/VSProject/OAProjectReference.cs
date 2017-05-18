// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

namespace Microsoft.VisualStudioTools.Project.Automation
{
    /// <summary>
    /// Represents a project reference of the solution
    /// </summary>
    [ComVisible(true)]
    public class OAProjectReference : OAReferenceBase
    {
        internal OAProjectReference(ProjectReferenceNode projectReference) :
            base(projectReference)
        {
        }

        internal new ProjectReferenceNode BaseReferenceNode => (ProjectReferenceNode)base.BaseReferenceNode;
        #region Reference override
        public override string Culture => string.Empty; public override string Name => this.BaseReferenceNode.ReferencedProjectName; public override string Identity => this.BaseReferenceNode.Caption;
        public override string Path => this.BaseReferenceNode.ReferencedProjectOutputPath;
        public override EnvDTE.Project SourceProject
        {
            get
            {
                if (Guid.Empty == this.BaseReferenceNode.ReferencedProjectGuid)
                {
                    return null;
                }
                var hierarchy = VsShellUtilities.GetHierarchy(this.BaseReferenceNode.ProjectMgr.Site, this.BaseReferenceNode.ReferencedProjectGuid);
                if (null == hierarchy)
                {
                    return null;
                }
                object extObject;
                if (Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObject)))
                {
                    return extObject as EnvDTE.Project;
                }
                return null;
            }
        }
        public override prjReferenceType Type => prjReferenceType.prjReferenceTypeAssembly; public override string Version => string.Empty;
        #endregion
    }
}
