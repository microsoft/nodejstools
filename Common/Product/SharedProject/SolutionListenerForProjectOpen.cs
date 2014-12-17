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

using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project.Automation;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.VisualStudioTools.Project {


    internal class SolutionListenerForProjectOpen : SolutionListener {
        public SolutionListenerForProjectOpen(IServiceProvider serviceProvider)
            : base(serviceProvider) {
        }

        public override int OnAfterOpenProject(IVsHierarchy hierarchy, int added) {
            // If this is our project, notify it that it has been opened.
            if (hierarchy.GetProject() != null) {
                var oaProject = hierarchy.GetProject() as OAProject;
                if (oaProject != null && oaProject.Project is ProjectNode) {
                    ((ProjectNode)oaProject.Project).OnAfterProjectOpen();
                }
            }

            // If this is a new project and our project. We use here that it is only our project that will implemnet the "internal"  IBuildDependencyOnProjectContainer.
            if (added != 0 && hierarchy is IBuildDependencyUpdate) {
                IVsUIHierarchy uiHierarchy = hierarchy as IVsUIHierarchy;
                Debug.Assert(uiHierarchy != null, "The ProjectNode should implement IVsUIHierarchy");
                if (uiHierarchy == null) {
                    return VSConstants.E_FAIL;
                }
                // Expand and select project node
                IVsUIHierarchyWindow uiWindow = UIHierarchyUtilities.GetUIHierarchyWindow(this.ServiceProvider, HierarchyNode.SolutionExplorer);
                if (uiWindow != null) {
                    __VSHIERARCHYITEMSTATE state;
                    uint stateAsInt;
                    if (uiWindow.GetItemState(uiHierarchy, VSConstants.VSITEMID_ROOT, (uint)__VSHIERARCHYITEMSTATE.HIS_Expanded, out stateAsInt) == VSConstants.S_OK) {
                        state = (__VSHIERARCHYITEMSTATE)stateAsInt;
                        if (state != __VSHIERARCHYITEMSTATE.HIS_Expanded) {
                            int hr;
                            hr = uiWindow.ExpandItem(uiHierarchy, VSConstants.VSITEMID_ROOT, EXPANDFLAGS.EXPF_ExpandParentsToShowItem);
                            if (ErrorHandler.Failed(hr))
                                Trace.WriteLine("Failed to expand project node");
                            hr = uiWindow.ExpandItem(uiHierarchy, VSConstants.VSITEMID_ROOT, EXPANDFLAGS.EXPF_SelectItem);
                            if (ErrorHandler.Failed(hr))
                                Trace.WriteLine("Failed to select project node");

                            return hr;
                        }
                    }
                }
            }
            return VSConstants.S_OK;
        }
    }
}
