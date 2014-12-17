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

namespace Microsoft.NodejsTools.Options {
    public partial class NodejsFormattingBracesOptionsControl : UserControl {
        public NodejsFormattingBracesOptionsControl() {
            InitializeComponent();
        }

        internal void SyncPageWithControlSettings(NodejsFormattingBracesOptionsPage page) {
            page.BraceOnNewLineForControlBlocks = _newLineForControlBlocks.Checked;
            page.BraceOnNewLineForFunctions = _newLineForFunctions.Checked;
        }

        internal void SyncControlWithPageSettings(NodejsFormattingBracesOptionsPage page) {
            _newLineForControlBlocks.Checked = page.BraceOnNewLineForControlBlocks;
            _newLineForFunctions.Checked = page.BraceOnNewLineForFunctions;
        }
    }
}
