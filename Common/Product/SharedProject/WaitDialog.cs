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

namespace Microsoft.VisualStudioTools.Project {
    sealed class WaitDialog : IDisposable {
        private readonly int _waitResult;
        private readonly IVsThreadedWaitDialog2 _waitDialog;

        public WaitDialog(string waitCaption, string waitMessage, IServiceProvider serviceProvider, int displayDelay = 1, bool isCancelable = false, bool showProgress = false) {
            _waitDialog = (IVsThreadedWaitDialog2)serviceProvider.GetService(typeof(SVsThreadedWaitDialog));
            _waitResult = _waitDialog.StartWaitDialog(
                waitCaption,
                waitMessage,
                null,
                null,
                null,
                displayDelay,
                isCancelable,
                showProgress
            );
        }

        public void UpdateProgress(int currentSteps, int totalSteps) {
            bool canceled;
            _waitDialog.UpdateProgress(
                null,
                null,
                null,
                currentSteps,
                totalSteps,
                false,
                out canceled
            );

        }

        public bool Canceled {
            get {
                bool canceled;
                ErrorHandler.ThrowOnFailure(_waitDialog.HasCanceled(out canceled));
                return canceled;
            }
        }

        #region IDisposable Members

        public void Dispose() {
            if (ErrorHandler.Succeeded(_waitResult)) {
                int cancelled = 0;
                _waitDialog.EndWaitDialog(out cancelled);
            }
        }

        #endregion
    }
}