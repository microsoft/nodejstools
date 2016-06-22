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
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.NodejsTools {
    class LanguagePreferences : IVsTextManagerEvents4 {
        internal LANGPREFERENCES3 _preferences;

        public LanguagePreferences(LANGPREFERENCES3 preferences) {
            _preferences = preferences;
        }

        #region IVsTextManagerEvents2 Members

        public int OnRegisterMarkerType(int iMarkerType) {
            return VSConstants.S_OK;
        }

        public int OnRegisterView(IVsTextView pView) {
            return VSConstants.S_OK;
        }

        public int OnReplaceAllInFilesBegin() {
            return VSConstants.S_OK;
        }

        public int OnReplaceAllInFilesEnd() {
            return VSConstants.S_OK;
        }

        public int OnUnregisterView(IVsTextView pView) {
            return VSConstants.S_OK;
        }

        public int OnUserPreferencesChanged4(VIEWPREFERENCES3[] pViewPrefs, LANGPREFERENCES3[] pLangPrefs, FONTCOLORPREFERENCES2[] pColorPrefs) {
            IVsTextManager4 textMgr = (IVsTextManager4)NodejsPackage.Instance.GetService(typeof(SVsTextManager));

           if (pLangPrefs != null && pLangPrefs.Length > 0 && pLangPrefs[0].guidLang == _preferences.guidLang) {
                _preferences.IndentStyle = pLangPrefs[0].IndentStyle;
                _preferences.fAutoListMembers = pLangPrefs[0].fAutoListMembers;
                _preferences.fAutoListParams = pLangPrefs[0].fAutoListParams;
                _preferences.fHideAdvancedAutoListMembers = pLangPrefs[0].fHideAdvancedAutoListMembers;

                // Synchronize settings back to TS language service
                pLangPrefs[0].guidLang = Guids.TypeScriptLanguageInfo;
                textMgr.SetUserPreferences4(null, pLangPrefs, null);
            }
            return VSConstants.S_OK;
        }

        #endregion

        #region Options

        public vsIndentStyle IndentMode {
            get {
                return _preferences.IndentStyle;
            }
        }

        public bool NavigationBar {
            get {
                return _preferences.fDropdownBar != 0;
            }
        }

        public bool HideAdvancedMembers {
            get {
                return _preferences.fHideAdvancedAutoListMembers != 0;
            }
        }

        public bool AutoListMembers {
            get {
                return _preferences.fAutoListMembers != 0;
            }
        }

        public bool AutoListParams {
            get {
                return _preferences.fAutoListParams != 0;
            }
        }


        #endregion
    }
}
