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

using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudioTools.Project {
    /// <summary>
    /// Defines abstract package.
    /// </summary>
    [ComVisible(true)]

    public abstract class ProjectPackage : Microsoft.VisualStudio.Shell.Package {
        #region fields
        /// <summary>
        /// This is the place to register all the solution listeners.
        /// </summary>
        private List<SolutionListener> solutionListeners = new List<SolutionListener>();
        #endregion

        #region properties
        /// <summary>
        /// Add your listener to this list. They should be added in the overridden Initialize befaore calling the base.
        /// </summary>
        internal IList<SolutionListener> SolutionListeners {
            get {
                return this.solutionListeners;
            }
        }
        #endregion

        #region methods
        protected override void Initialize() {
            base.Initialize();

            // Subscribe to the solution events
            this.solutionListeners.Add(new SolutionListenerForProjectOpen(this));
            this.solutionListeners.Add(new SolutionListenerForBuildDependencyUpdate(this));

            foreach (SolutionListener solutionListener in this.solutionListeners) {
                solutionListener.Init();
            }
        }

        protected override void Dispose(bool disposing) {
            // Unadvise solution listeners.
            try {
                if (disposing) {
                    foreach (SolutionListener solutionListener in this.solutionListeners) {
                        solutionListener.Dispose();
                    }
                }
            } finally {

                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
