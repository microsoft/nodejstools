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
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.NodejsTools.Jade {
    class JadeLanguageInfo : IVsLanguageInfo {
        public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr) {
            ppCodeWinMgr = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer) {
            ppColorizer = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetFileExtensions(out string pbstrExtensions) {
            pbstrExtensions = string.Join(";", new[] { JadeContentTypeDefinition.JadeFileExtension, JadeContentTypeDefinition.PugFileExtension });
            return VSConstants.S_OK;
        }

        public int GetLanguageName(out string bstrName) {
            bstrName = JadeContentTypeDefinition.JadeLanguageName;
            return VSConstants.S_OK;
        }
    }
}
