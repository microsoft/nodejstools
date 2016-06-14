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

namespace Microsoft.NodejsTools.Options {
    public partial class NodejsNpmOptionsControl : UserControl {
        public NodejsNpmOptionsControl() {
            InitializeComponent();
        }

        internal void SyncControlWithPageSettings(NodejsNpmOptionsPage page) {
            _showOutputWhenRunningNpm.Checked = page.ShowOutputWindowWhenExecutingNpm;
            _cacheClearedSuccessfully.Visible = false;
        }

        internal void SyncPageWithControlSettings(NodejsNpmOptionsPage page) {
            page.ShowOutputWindowWhenExecutingNpm = _showOutputWhenRunningNpm.Checked;
        }

        private void ClearCacheButton_Click(object sender, EventArgs e) {
            bool didClearNpmCache = DeleteCacheDirectory("npm cache", NodejsConstants.NpmCachePath);
            bool didClearTools = DeleteCacheDirectory("NTVS external tools", NodejsConstants.ExternalToolsPath);

            _cacheClearedSuccessfully.Visible = (didClearNpmCache && didClearTools);
        }

        private static bool DeleteCacheDirectory(string displayName, string cachePath) {
            try {
                Directory.Delete(cachePath, true);
               return true;
            } catch (DirectoryNotFoundException) {
                // Directory has already been deleted. Do nothing.
                return true;
            } catch (IOException exception) {
                // files are in use or path is too long
                MessageBox.Show(
                    string.Format("Cannot clear {0}. {1}", displayName, exception.Message),
                    string.Format("Cannot Clear {0}", displayName),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            } catch (Exception exception) {
                try {
                    ActivityLog.LogError(SR.ProductName, exception.ToString());
                } catch (InvalidOperationException) {
                    // Activity Log is unavailable.
                }

                MessageBox.Show(
                    string.Format("Cannot clear {0}. Try manually deleting the directory: {1}", displayName, cachePath),
                    string.Format("Cannot Clear {0}", displayName),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            return false;
        }
    }
}