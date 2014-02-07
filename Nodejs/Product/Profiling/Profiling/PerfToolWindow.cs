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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Profiling {
    [Guid(WindowGuid)]
    class PerfToolWindow : ToolWindowPane {
        internal const string Title = "Node.js Performance";
        internal const string WindowGuid = "FB2AB212-5D1F-4101-A442-2231B4195E97";
        private SessionsNode _sessions;
        internal static PerfToolWindow Instance;

        public PerfToolWindow() {
            ToolClsid = Guids.VsUIHierarchyWindow;
            Caption = Title;
            Instance = this;
        }
        
        public override void OnToolWindowCreated() {
            base.OnToolWindowCreated();

            var frame = (IVsWindowFrame)Frame;
            object ouhw;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out ouhw));

            // initialie w/ our hierarchy
            var hw = ouhw as IVsUIHierarchyWindow;
            _sessions = new SessionsNode(hw);
            object punk;
            ErrorHandler.ThrowOnFailure(hw.Init(
                _sessions,
                (uint)(__UIHWINFLAGS.UIHWF_SupportToolWindowToolbars |
                __UIHWINFLAGS.UIHWF_InitWithHiddenParentRoot |
                __UIHWINFLAGS.UIHWF_HandlesCmdsAsActiveHierarchy),
                out punk
            ));

            // add our toolbar which  is defined in our VSCT file
            object otbh;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_ToolbarHost, out otbh));
            IVsToolWindowToolbarHost tbh = otbh as IVsToolWindowToolbarHost;
            Guid guidPerfMenuGroup = Guids.NodejsProfilingCmdSet;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(tbh.AddToolbar(VSTWT_LOCATION.VSTWT_TOP, ref guidPerfMenuGroup, PkgCmdIDList.menuIdPerfToolbar));
        }

        public SessionsNode Sessions {
            get {
                return _sessions;
            }
        }
    }
}
