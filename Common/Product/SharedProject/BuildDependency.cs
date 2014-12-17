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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project {
    public class BuildDependency : IVsBuildDependency {
        Guid referencedProjectGuid = Guid.Empty;
        ProjectNode projectMgr = null;

        internal BuildDependency(ProjectNode projectMgr, Guid projectReference) {
            this.referencedProjectGuid = projectReference;
            this.projectMgr = projectMgr;
        }

        #region IVsBuildDependency methods
        public int get_CanonicalName(out string canonicalName) {
            canonicalName = null;
            return VSConstants.S_OK;
        }

        public int get_Type(out System.Guid guidType) {
            // All our dependencies are build projects
            guidType = VSConstants.GUID_VS_DEPTYPE_BUILD_PROJECT;
            return VSConstants.S_OK;
        }

        public int get_Description(out string description) {
            description = null;
            return VSConstants.S_OK;
        }

        [CLSCompliant(false)]
        public int get_HelpContext(out uint helpContext) {
            helpContext = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int get_HelpFile(out string helpFile) {
            helpFile = null;
            return VSConstants.E_NOTIMPL;
        }

        public int get_MustUpdateBefore(out int mustUpdateBefore) {
            // Must always update dependencies
            mustUpdateBefore = 1;

            return VSConstants.S_OK;
        }

        public int get_ReferredProject(out object unknownProject) {
            unknownProject = null;

            unknownProject = this.GetReferencedHierarchy();

            // If we cannot find the referenced hierarchy return S_FALSE.
            return (unknownProject == null) ? VSConstants.S_FALSE : VSConstants.S_OK;
        }

        #endregion

        #region helper methods
        private IVsHierarchy GetReferencedHierarchy() {
            IVsHierarchy hierarchy = null;

            if (this.referencedProjectGuid == Guid.Empty || this.projectMgr == null || this.projectMgr.IsClosed) {
                return hierarchy;
            }

            return VsShellUtilities.GetHierarchy(this.projectMgr.Site, this.referencedProjectGuid);

        }

        #endregion

    }
}
