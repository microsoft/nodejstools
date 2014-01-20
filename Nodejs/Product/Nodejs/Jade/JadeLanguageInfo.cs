/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

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
            pbstrExtensions = JadeContentTypeDefinition.JadeFileExtension;
            return VSConstants.S_OK;
        }

        public int GetLanguageName(out string bstrName) {
            bstrName = JadeContentTypeDefinition.JadeLanguageName;
            return VSConstants.S_OK;
        }
    }
}
