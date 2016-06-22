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

using System.Windows.Forms;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Options {
    public partial class SalsaLsIntellisenseOptionsControl : UserControl {
        public SalsaLsIntellisenseOptionsControl() {
            InitializeComponent();
        }

        internal void SyncPageWithControlSettings(NodejsIntellisenseOptionsPage page) {
            page.EnableAutomaticTypingsAcquisition = _enableAutomaticTypingsAcquisition.Checked;
            page.ShowTypingsInfoBar = _showTypingsInfoBar.Checked;
            page.SaveChangesToConfigFile = _saveChangesToConfigFile.Checked;
        }

        internal void SyncControlWithPageSettings(NodejsIntellisenseOptionsPage page) {
            _enableAutomaticTypingsAcquisition.Checked = page.EnableAutomaticTypingsAcquisition;
            _showTypingsInfoBar.Checked = page.ShowTypingsInfoBar;
            _saveChangesToConfigFile.Checked = page.SaveChangesToConfigFile;
        }

        private void typingsLearnMoreLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("http://go.microsoft.com/fwlink/?LinkID=808343");
        }
    }
}
