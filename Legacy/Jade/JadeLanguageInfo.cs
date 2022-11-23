// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.NodejsTools.Jade
{
    internal class JadeLanguageInfo : IVsLanguageInfo
    {
        public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr)
        {
            ppCodeWinMgr = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer)
        {
            ppColorizer = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetFileExtensions(out string pbstrExtensions)
        {
            pbstrExtensions = string.Join(";", new[] { JadeContentTypeDefinition.JadeFileExtension, JadeContentTypeDefinition.PugFileExtension });
            return VSConstants.S_OK;
        }

        public int GetLanguageName(out string bstrName)
        {
            bstrName = JadeContentTypeDefinition.JadeLanguageName;
            return VSConstants.S_OK;
        }
    }
}
