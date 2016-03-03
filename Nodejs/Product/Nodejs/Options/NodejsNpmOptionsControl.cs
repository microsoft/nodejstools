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
using System.IO;
using System.Windows.Forms;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Options {
    public partial class NodejsNpmOptionsControl : UserControl {

        private string _npmCachePath;

        public NodejsNpmOptionsControl() {
            InitializeComponent();
        }


        internal void SyncControlWithPageSettings(NodejsNpmOptionsPage page) {
            _showOutputWhenRunningNpm.Checked = page.ShowOutputWindowWhenExecutingNpm;
            _npmCachePath = page.NpmCachePath;
            _cacheClearedSuccessfully.Visible = false;
        }

        internal void SyncPageWithControlSettings(NodejsNpmOptionsPage page) {
            page.ShowOutputWindowWhenExecutingNpm = _showOutputWhenRunningNpm.Checked;
        }

        private void ClearCacheButton_Click(object sender, EventArgs e) {
            try {
                Directory.Delete(_npmCachePath, true);                    
                _cacheClearedSuccessfully.Visible = true;
            } catch (DirectoryNotFoundException) {
                // Directory has already been deleted. Do nothing.
                _cacheClearedSuccessfully.Visible = true;
            } catch (IOException exception) {
                // files are in use or path is too long
                MessageBox.Show(
                           string.Format("Cannot clear npm cache. {0}", exception.Message),
                           "Cannot Clear npm Cache",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Information
                       );
            } catch (Exception exception) {
                try {
                    ActivityLog.LogError(NodeJsProjectSr.ProductName, exception.ToString());
                } catch (InvalidOperationException) {
                    // Activity Log is unavailable.
                }

                MessageBox.Show(
                           string.Format("Cannot clear npm cache. Try manually deleting the directory: {0}", _npmCachePath),
                           "Cannot Clear npm Cache",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Information
                       );
            }
        }
    }
}