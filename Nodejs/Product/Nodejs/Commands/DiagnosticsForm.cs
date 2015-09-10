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
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Commands {
    public partial class DiagnosticsForm : Form {
        public DiagnosticsForm(string content) {
            InitializeComponent();
            _textBox.Text = content;
        }

        public TextBox TextBox {
            get {
                return _textBox;
            }
        }

        private void _ok_Click(object sender, EventArgs e) {
            Close();
        }

        private void _copy_Click(object sender, EventArgs e) {
            _textBox.SelectAll();
            Clipboard.SetText(_textBox.SelectedText);
        }

        private void _diagnosticLoggingCheckbox_CheckedChanged(object sender, EventArgs e) {
            NodejsPackage.Instance.DiagnosticsOptionsPage.IsLiveDiagnosticsEnabled = _diagnosticLoggingCheckbox.Checked;
        }
    }
}
