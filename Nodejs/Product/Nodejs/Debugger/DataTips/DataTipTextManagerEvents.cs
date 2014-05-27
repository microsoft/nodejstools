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
