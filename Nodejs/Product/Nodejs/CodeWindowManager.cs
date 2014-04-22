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
