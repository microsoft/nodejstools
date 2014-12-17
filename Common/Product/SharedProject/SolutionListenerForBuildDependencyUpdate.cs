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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.VisualStudioTools.Project {
    /// <summary>
    /// The purpose of this class is to set a build dependency from a modeling project to all its sub projects
    /// </summary>
    class SolutionListenerForBuildDependencyUpdate : SolutionListener {
        #region ctors
        public SolutionListenerForBuildDependencyUpdate(IServiceProvider serviceProvider)
            : base(serviceProvider) {
        }
        #endregion

        #region overridden methods
        /// <summary>
        /// Update build dependency list if solution is fully loaded
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="added"></param>
        /// <returns></returns>
        public override int OnAfterOpenProject(IVsHierarchy hierarchy, int added) {
            // Return from here if we are at load time
            if (added == 0) {
                return VSConstants.S_OK;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called at load time when solution has finished opening.
        /// </summary>
        /// <param name="pUnkReserved">reserved</param>
        /// <param name="fNewSolution">true if this is a new solution</param>
        /// <returns></returns>
        public override int OnAfterOpenSolution(object pUnkReserved, int fNewSolution) {
            return VSConstants.S_OK;
        }
        #endregion

    }
}
