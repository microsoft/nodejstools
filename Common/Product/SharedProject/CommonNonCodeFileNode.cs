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
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project {
    internal class CommonNonCodeFileNode : CommonFileNode {
        public CommonNonCodeFileNode(CommonProjectNode root, ProjectElement e)
            : base(root, e) {
        }


        /// <summary>
        /// Open a file depending on the SubType property associated with the file item in the project file
        /// </summary>
        protected override void DoDefaultAction() {
            if ("WebBrowser".Equals(SubType, StringComparison.OrdinalIgnoreCase)) {
                CommonPackage.OpenVsWebBrowser(Url);
                return;
            }

            FileDocumentManager manager = this.GetDocumentManager() as FileDocumentManager;
            Utilities.CheckNotNull(manager, "Could not get the FileDocumentManager");

            Guid viewGuid = Guid.Empty;
            IVsWindowFrame frame;
            manager.Open(false, false, viewGuid, out frame, WindowFrameShowAction.Show);
        }

    }
}
