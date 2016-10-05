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
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Options {
    public partial class NodejsIntellisenseOptionsControl : UserControl {
        public NodejsIntellisenseOptionsControl() {
            InitializeComponent();
        }

        internal void SyncPageWithControlSettings(NodejsIntellisenseOptionsPage page) {
            _salsaLsIntellisenseOptionsControl.SyncPageWithControlSettings(page);
        }

        internal void SyncControlWithPageSettings(NodejsIntellisenseOptionsPage page) {
            _salsaLsIntellisenseOptionsControl.SyncControlWithPageSettings(page);
        }

        private void _analysisPreviewFeedbackLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("https://go.microsoft.com/fwlink/?LinkID=808344");
        }

        private void _intelliSenseModeDropdown_SelectedValueChanged(object sender, EventArgs e) {
            // IntelliSense Options are controlled by the built-in language service in DEV15+
#if DEV14
            _salsaLsIntellisenseOptionsControl.Visible = true;
#else
            _salsaLsIntellisenseOptionsControl.Visible = false;
#endif
        }
    }
}
