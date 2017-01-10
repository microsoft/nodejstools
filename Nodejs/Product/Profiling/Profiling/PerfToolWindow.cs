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
            Guid guidPerfMenuGroup = ProfilingGuids.NodejsProfilingCmdSet;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(tbh.AddToolbar(VSTWT_LOCATION.VSTWT_TOP, ref guidPerfMenuGroup, PkgCmdIDList.menuIdPerfToolbar));
        }

        public SessionsNode Sessions {
            get {
                return _sessions;
            }
        }
    }
}
