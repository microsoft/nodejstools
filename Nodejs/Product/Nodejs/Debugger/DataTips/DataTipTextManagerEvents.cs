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
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.NodejsTools.Debugger.DataTips {
    internal class DataTipTextManagerEvents : IVsTextManagerEvents {
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactory;

        public DataTipTextManagerEvents(IServiceProvider serviceProvider) {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            _editorAdaptersFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();
        }

        public void OnRegisterMarkerType(int iMarkerType) {
        }

        public void OnRegisterView(IVsTextView pView) {
            var wpfTextView = _editorAdaptersFactory.GetWpfTextView(pView);
            if (wpfTextView != null && wpfTextView.TextBuffer.ContentType.IsOfType(NodejsConstants.Nodejs)) {
                new DataTipTextViewFilter(pView);
            }
        }

        public void OnUnregisterView(IVsTextView pView) {
        }

        public void OnUserPreferencesChanged(VIEWPREFERENCES[] pViewPrefs, FRAMEPREFERENCES[] pFramePrefs, LANGPREFERENCES[] pLangPrefs, FONTCOLORPREFERENCES[] pColorPrefs) {
        }
    }
}
