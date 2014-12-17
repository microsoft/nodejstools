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
    public partial class NodejsFormattingSpacingOptionsControl : UserControl {
        public NodejsFormattingSpacingOptionsControl() {
            InitializeComponent();
        }

        internal void SyncControlWithPageSettings(NodejsFormattingSpacingOptionsPage page) {
            _spaceAfterCommaDelimiter.Checked = page.SpaceAfterComma;
            _spaceAfterFunction.Checked = page.SpaceAfterFunctionKeywordForAnonymousFunctions;
            _spacesAfterKeywordsInControlFlow.Checked = page.SpaceAfterKeywordsInControlFlow;
            _nonEmptyParenthesis.Checked = page.SpaceAfterOpeningAndBeforeClosingNonEmptyParens;
            _afterSemicolonFor.Checked = page.SpaceAfterSemicolonInFor;
            _binaryOperators.Checked = page.SpaceBeforeAndAfterBinaryOperator;
        }

        internal void SyncPageWithControlSettings(NodejsFormattingSpacingOptionsPage page) {
            page.SpaceAfterComma = _spaceAfterCommaDelimiter.Checked;
            page.SpaceAfterFunctionKeywordForAnonymousFunctions = _spaceAfterFunction.Checked;
            page.SpaceAfterKeywordsInControlFlow = _spacesAfterKeywordsInControlFlow.Checked;
            page.SpaceAfterOpeningAndBeforeClosingNonEmptyParens = _nonEmptyParenthesis.Checked;
            page.SpaceAfterSemicolonInFor = _afterSemicolonFor.Checked;
            page.SpaceBeforeAndAfterBinaryOperator = _binaryOperators.Checked;
        }
    }
}
