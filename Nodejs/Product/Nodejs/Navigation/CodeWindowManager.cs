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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudioTools.Navigation {
    class CodeWindowManager : IVsCodeWindowManager {
        private readonly IVsCodeWindow _window;
        private readonly IWpfTextView _textView;
        private static readonly Dictionary<IWpfTextView, CodeWindowManager> _windows = new Dictionary<IWpfTextView, CodeWindowManager>();
#if FALSE
        private readonly EditFilter _filter;
        private DropDownBarClient _client;
#endif

        public CodeWindowManager(IVsCodeWindow codeWindow, IWpfTextView textView) {
            _window = codeWindow;
            _textView = textView;
        }

        public static void OnIdle(IOleComponentManager compMgr) {
            foreach (var window in _windows) {
                if (compMgr.FContinueIdle() == 0) {
                    break;
                }
#if FALSE
                window.Value._filter.DoIdle(compMgr);
#endif
            }
        }

        #region IVsCodeWindowManager Members

        public int AddAdornments() {
            _windows[_textView] = this;

            IVsTextView textView;

            if (ErrorHandler.Succeeded(_window.GetPrimaryView(out textView))) {
                OnNewView(textView);
            }

            if (ErrorHandler.Succeeded(_window.GetSecondaryView(out textView))) {
                OnNewView(textView);
            }
#if FALSE
            if (PythonToolsPackage.Instance.LangPrefs.NavigationBar) {
                return AddDropDownBar();
            }
#endif

            return VSConstants.S_OK;
        }
#if FALSE
        private int AddDropDownBar() {
            var pythonProjectEntry = _textView.TextBuffer.GetAnalysis() as IPythonProjectEntry;
            if (pythonProjectEntry == null) {
                return VSConstants.E_FAIL;
            }

            DropDownBarClient dropDown = _client = new DropDownBarClient(_textView, pythonProjectEntry);

            IVsDropdownBarManager manager = (IVsDropdownBarManager)_window;

            IVsDropdownBar dropDownBar;
            int hr = manager.GetDropdownBar(out dropDownBar);
            if (ErrorHandler.Succeeded(hr) && dropDownBar != null) {
                hr = manager.RemoveDropdownBar();
                if (!ErrorHandler.Succeeded(hr)) {
                    return hr;
                }
            }

            int res = manager.AddDropdownBar(2, dropDown);
            if (ErrorHandler.Succeeded(res)) {
                _textView.TextBuffer.Properties[typeof(DropDownBarClient)] = dropDown;
            }
            return res;
        }

        private int RemoveDropDownBar() {
            if (_client != null) {
                IVsDropdownBarManager manager = (IVsDropdownBarManager)_window;
                _client.Unregister();
                _client = null;
                _textView.TextBuffer.Properties.RemoveProperty(typeof(DropDownBarClient));
                return manager.RemoveDropdownBar();
            }
            return VSConstants.S_OK;
        }
#endif
        public int OnNewView(IVsTextView pView) {
            // TODO: We pass _textView which may not be right for split buffers, we need
            // to test the case where we split a text file and save it as an existing file?
            return VSConstants.S_OK;
        }

        public int RemoveAdornments() {
            _windows.Remove(_textView);
#if FALSE
            return RemoveDropDownBar();
#else
            return VSConstants.S_OK;
#endif
        }

        public static void ToggleNavigationBar(bool fEnable) {
#if FALSE
            foreach (var keyValue in _windows) {
                if (fEnable) {
                    ErrorHandler.ThrowOnFailure(keyValue.Value.AddDropDownBar());
                } else {
                    ErrorHandler.ThrowOnFailure(keyValue.Value.RemoveDropDownBar());
                }
            }
#endif
        }

        #endregion
    }

}
