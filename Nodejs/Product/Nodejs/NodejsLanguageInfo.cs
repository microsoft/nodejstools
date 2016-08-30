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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.NodejsTools {
    /// <summary>
    /// Minimal language service.  Implemented directly rather than using the Managed Package
    /// Framework because we don't want to provide colorization services.  Instead we use the
    /// new Visual Studio 2010 APIs to provide these services.  But we still need this to
    /// provide a code window manager so that we can have a navigation bar (actually we don't, this
    /// should be switched over to using our TextViewCreationListener instead).
    /// </summary>
    [Guid(Guids.NodejsLanguageInfoGuidString)]
    internal sealed class NodejsLanguageInfo : IVsLanguageInfo, IVsLanguageDebugInfo {
        private readonly IServiceProvider _serviceProvider;
        private readonly IComponentModel _componentModel;

        public NodejsLanguageInfo(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
            _componentModel = serviceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
        }

        public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr) {
            ppCodeWinMgr = null;
            return VSConstants.S_OK;
        }

        public int GetFileExtensions(out string pbstrExtensions) {
            // This is the same extension the language service was
            // registered as supporting.
            pbstrExtensions = NodejsConstants.JavaScriptExtension;
            return VSConstants.S_OK;
        }


        public int GetLanguageName(out string bstrName) {
            // This is the same name the language service was registered with.
            bstrName = NodejsConstants.JavaScript;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// GetColorizer is not implemented because we implement colorization using the new managed APIs.
        /// </summary>
        public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer) {
            ppColorizer = null;
            return VSConstants.E_FAIL;
        }

        public IServiceProvider ServiceProvider {
            get {
                return _serviceProvider;
            }
        }

        public int GetLanguageID(IVsTextBuffer pBuffer, int iLine, int iCol, out Guid pguidLanguageID) {
            pguidLanguageID = Guids.NodejsDebugLanguage;
            return VSConstants.S_OK;
        }

        public int GetLocationOfName(string pszName, out string pbstrMkDoc, TextSpan[] pspanLocation) {
            pbstrMkDoc = null;
            return VSConstants.E_FAIL;
        }

        public int GetNameOfLocation(IVsTextBuffer pBuffer, int iLine, int iCol, out string pbstrName, out int piLineOffset) {
            pbstrName = null;
            piLineOffset = 0;
            return VSConstants.E_FAIL;
        }

        public int GetProximityExpressions(IVsTextBuffer pBuffer, int iLine, int iCol, int cLines, out IVsEnumBSTR ppEnum) {
            ppEnum = null;
            return VSConstants.E_FAIL;
        }

        public int IsMappedLocation(IVsTextBuffer pBuffer, int iLine, int iCol) {
            return VSConstants.E_FAIL;
        }

        public int ResolveName(string pszName, uint dwFlags, out IVsEnumDebugName ppNames) {
            ppNames = null;
            return VSConstants.E_FAIL;
        }

        public int ValidateBreakpointLocation(IVsTextBuffer pBuffer, int iLine, int iCol, TextSpan[] pCodeSpan) {
            // per the docs, even if we don't intend to validate, we need to set the span info:
            // http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.textmanager.interop.ivslanguagedebuginfo.validatebreakpointlocation.aspx
            // 
            // Caution
            // Even if you do not intend to support the ValidateBreakpointLocation method but your 
            // language does support breakpoints, you must implement this method and return a span 
            // that contains the specified line and column; otherwise, breakpoints cannot be set 
            // anywhere except line 1. You can return E_NOTIMPL to indicate that you do not otherwise 
            // support this method but the span must always be set. 

            // http://pytools.codeplex.com/workitem/787
            // We were previously returning S_OK here indicating to VS that we have in fact validated
            // the breakpoint.  Validating breakpoints actually interacts and effectively disables
            // the "Highlight entire source line for breakpoints and current statement" option as instead
            // VS highlights the validated region.  So we return E_NOTIMPL here to indicate that we have 
            // not validated the breakpoint, and then VS will happily respect the option when we're in 
            // design mode.
            pCodeSpan[0].iStartLine = iLine;
            pCodeSpan[0].iEndLine = iLine;
            return VSConstants.E_NOTIMPL;
        }
    }
}
