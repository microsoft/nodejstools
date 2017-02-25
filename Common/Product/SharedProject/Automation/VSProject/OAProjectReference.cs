/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

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
