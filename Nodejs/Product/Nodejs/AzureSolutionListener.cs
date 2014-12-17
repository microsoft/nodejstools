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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;

namespace Microsoft.NodejsTools {
    class AzureSolutionListener : IVsSolutionEvents3, IDisposable {
        public readonly List<IVsHierarchy> OpenedHierarchies = new List<IVsHierarchy>();
        private readonly IServiceProvider _serviceProvider;
        private readonly IVsSolution _solution;
        private readonly uint _eventsCookie;
        private bool _isDisposed;

        public AzureSolutionListener(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
            _solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            ErrorHandler.ThrowOnFailure(_solution.AdviseSolutionEvents(this, out _eventsCookie));
        }

        public int OnAfterOpenProject(IVsHierarchy hierarchy, int added) {
            if (added != 0) {
                OpenedHierarchies.Add(hierarchy);
            }
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterCloseSolution(object pUnkReserved) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterClosingChildren(IVsHierarchy pHierarchy) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterMergeSolution(object pUnkReserved) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterOpeningChildren(IVsHierarchy pHierarchy) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnBeforeCloseSolution(object pUnkReserved) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnBeforeClosingChildren(IVsHierarchy pHierarchy) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// The IDispose interface Dispose method for disposing the object determinastically.
        /// </summary>
        public void Dispose() {
            Dispose(true);
        }

        /// <summary>
        /// The method that does the cleanup.
        /// </summary>
        /// <param name="disposing"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsSolution.UnadviseSolutionEvents(System.UInt32)")]
        protected virtual void Dispose(bool disposing) {
            // Everybody can go here.
            if (!_isDisposed) {
                // Synchronize calls to the Dispose simulteniously.
                lock (this) {
                    if (disposing && 
                        _eventsCookie != (uint)ShellConstants.VSCOOKIE_NIL && 
                        _solution != null) {
                        _solution.UnadviseSolutionEvents(_eventsCookie);
                    }

                    _isDisposed = true;
                }
            }
        }
    }
}
