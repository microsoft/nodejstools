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

namespace Microsoft.NodejsTools.Formatting {

    internal sealed class FormattingOptions {
        public int? SpacesPerIndent { get; set; }
        public string NewLine { get; set; }
        public bool OpenBracesOnNewLineForControl { get; set; }
        public bool OpenBracesOnNewLineForFunctions { get; set; }
        public bool SpaceAfterComma { get; set; }
        public bool SpaceAfterSemiColonInFor { get; set; }
        public bool SpaceBeforeAndAfterBinaryOperator { get; set; }
        public bool SpaceAfterKeywordsInControlFlowStatements { get; set; }
        public bool SpaceAfterFunctionInAnonymousFunctions { get; set; }
        public bool SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis { get; set; }

        public FormattingOptions() {
            SpacesPerIndent = 4;
            SpaceAfterComma = true;
            SpaceAfterSemiColonInFor = true;
            SpaceBeforeAndAfterBinaryOperator = true;
            SpaceAfterKeywordsInControlFlowStatements = true;
            SpaceAfterFunctionInAnonymousFunctions = true;
            NewLine = "\r\n";
        }
    }
}
