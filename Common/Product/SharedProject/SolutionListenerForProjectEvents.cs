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
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// This class triggers the project events for "our" hierrachies.
    /// </summary>
    internal class SolutionListenerForProjectEvents : SolutionListener
    {
        #region events
        /// Event raised just after the project file opened.
        /// </summary>
        public event EventHandler<AfterProjectFileOpenedEventArgs> AfterProjectFileOpened;

        /// <summary>
        /// Event raised before the project file closed.
        /// </summary>
        public event EventHandler<BeforeProjectFileClosedEventArgs> BeforeProjectFileClosed;
        #endregion

        #region ctor
        internal SolutionListenerForProjectEvents(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region overridden methods
        public override int OnAfterOpenProject(IVsHierarchy hierarchy, int added)
        {
            return VSConstants.S_OK;
        }

        public override int OnBeforeCloseProject(IVsHierarchy hierarchy, int removed)
        {
            return VSConstants.S_OK;
        }
        #endregion

        #region helpers
        /// <summary>
        /// Raises after project file opened event.
        /// </summary>
        /// <param name="added">True if the project is added to the solution after the solution is opened. false if the project is added to the solution while the solution is being opened.</param>
        private void RaiseAfterProjectFileOpened(bool added)
        {
            // Save event in temporary variable to avoid race condition.
            EventHandler<AfterProjectFileOpenedEventArgs> tempEvent = this.AfterProjectFileOpened;
            if (tempEvent != null)
            {
                tempEvent(this, new AfterProjectFileOpenedEventArgs());
            }
        }




        /// <summary>
        /// Raises the before  project file closed event.
        /// </summary>
        /// <param name="added">true if the project was removed from the solution before the solution was closed. false if the project was removed from the solution while the solution was being closed.</param>
        private void RaiseBeforeProjectFileClosed(IVsHierarchy hierarchy, bool removed)
        {
            // Save event in temporary variable to avoid race condition.
            EventHandler<BeforeProjectFileClosedEventArgs> tempEvent = this.BeforeProjectFileClosed;
            if (tempEvent != null)
            {
                tempEvent(this, new BeforeProjectFileClosedEventArgs(hierarchy, removed));
            }
        }
    }
        #endregion
}
