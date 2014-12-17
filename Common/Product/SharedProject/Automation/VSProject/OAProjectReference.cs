//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

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

        internal new ProjectReferenceNode BaseReferenceNode {
            get { return (ProjectReferenceNode)base.BaseReferenceNode; }
        }

        #region Reference override
        public override string Culture
        {
            get { return string.Empty; }
        }
        public override string Name
        {
            get { return BaseReferenceNode.ReferencedProjectName; }
        }
        public override string Identity
        {
            get
            {
                return BaseReferenceNode.Caption;
            }
        }
        public override string Path
        {
            get
            {
                return BaseReferenceNode.ReferencedProjectOutputPath;
            }
        }
        public override EnvDTE.Project SourceProject
        {
            get
            {
                if (Guid.Empty == BaseReferenceNode.ReferencedProjectGuid)
                {
                    return null;
                }
                IVsHierarchy hierarchy = VsShellUtilities.GetHierarchy(BaseReferenceNode.ProjectMgr.Site, BaseReferenceNode.ReferencedProjectGuid);
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
        public override prjReferenceType Type
        {
            // TODO: Write the code that finds out the type of the output of the source project.
            get { return prjReferenceType.prjReferenceTypeAssembly; }
        }
        public override string Version
        {
            get { return string.Empty; }
        }
        #endregion
    }
}
