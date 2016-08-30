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

namespace Microsoft.NodejsTools.Options {
    public partial class NodejsFormattingGeneralOptionsControl : UserControl {
        public NodejsFormattingGeneralOptionsControl() {
            InitializeComponent();
        }

        internal void SyncControlWithPageSettings(NodejsFormattingGeneralOptionsPage page) {
            _formatOnCloseBrace.Checked = page.FormatOnCloseBrace;
            _formatOnEnter.Checked = page.FormatOnEnter;
            _formatOnPaste.Checked = page.FormatOnPaste;
            _formatOnSemicolon.Checked = page.FormatOnSemiColon;
        }

        internal void SyncPageWithControlSettings(NodejsFormattingGeneralOptionsPage page) {
            page.FormatOnCloseBrace = _formatOnCloseBrace.Checked;
            page.FormatOnEnter = _formatOnEnter.Checked;
            page.FormatOnPaste = _formatOnPaste.Checked;
            page.FormatOnSemiColon = _formatOnSemicolon.Checked;
        }
    }
}
