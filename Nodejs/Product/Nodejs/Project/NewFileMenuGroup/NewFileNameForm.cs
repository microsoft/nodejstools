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

namespace Microsoft.NodejsTools.Project
{
    public partial class NewFileNameForm : Form
    {
        public NewFileNameForm(string initialFileName)
        {
            InitializeComponent();

            TextBox.Text = initialFileName;
        }

        public TextBox TextBox
        {
            get {
                return textBox;
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (TextBox.Text.Trim().Length == 0) {
                okButton.Enabled = false;
            }
            else {
                okButton.Enabled = true;
            }
        }
    }
}
