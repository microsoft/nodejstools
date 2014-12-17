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
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.NodejsTools {
    class CodeWindowManager : IVsCodeWindowManager, IVsCodeWindowEvents {
        private readonly IVsEditorAdaptersFactoryService _adapterService;
        
        public CodeWindowManager(IVsEditorAdaptersFactoryService adapterService) {
            _adapterService = adapterService;
        }

#if FALSE
        internal static void AddSkipFilter(IVsEditorAdaptersFactoryService adapterService, IVsTextView primaryView) {
            var skipJsFilter = new SkipJsLsFilter(primaryView);
            var wpfView = adapterService.GetWpfTextView(primaryView);
            wpfView.Properties[typeof(SkipJsLsFilter)] = skipJsFilter;
        }
#endif

        public int OnNewView(IVsTextView pView) {
#if FALSE
            AddSkipFilter(_adapterService, pView);
#endif
            return VSConstants.S_OK;
        }

        public int OnCloseView(IVsTextView pView) {
#if FALSE
            RemoveSkipFilter(pView);
#endif
            return VSConstants.S_OK;
        }

#if FALSE
        private void RemoveSkipFilter(IVsTextView pView) {
            var wpfView = _adapterService.GetWpfTextView(pView);
            wpfView.Properties.RemoveProperty(typeof(SkipJsLsFilter));
        }
#endif

        public int AddAdornments() {
            return VSConstants.S_OK;
        }

        public int RemoveAdornments() {
            return VSConstants.S_OK;
        }
    }
}
