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

namespace Microsoft.NodejsTools.Jade
{
    internal partial class JadeTokenizer : Tokenizer<JadeToken>
    {
        private void OnInlineCode()
        {
            if (this._cs.CurrentChar == '-' || this._cs.CurrentChar == '=')
                this._cs.MoveToNextChar();

            // Make tokens for code keywords
            while (!this._cs.IsEndOfStream())
            {
                if (SkipWhiteSpace())
                    break;

                if (IsAtString())
                {
                    HandleString();
                }
                else if (this._cs.IsAnsiLetter())
                {
                    var range = ParseIdentifier();
                    var ident = this._cs.GetSubstringAt(range.Start, range.Length);

                    if (JadeCodeKeywords.IsKeyword(ident))
                        AddToken(JadeTokenType.CodeKeyword, range.Start, range.Length);
                }
                else if (this._cs.CurrentChar == '=' || this._cs.CurrentChar == '+' || this._cs.CurrentChar == '*' || this._cs.CurrentChar == '/' || this._cs.CurrentChar == '-')
                {
                    AddToken(JadeTokenType.Operator, this._cs.Position, 1);
                    this._cs.MoveToNextChar();
                }
                else if (this._cs.CurrentChar == '#' && this._cs.NextChar == '{')
                {
                    var range = GetNonWSSequence('}', inclusive: true);
                    if (range.Length > 0)
                    {
                        AddToken(JadeTokenType.Variable, range.Start, range.Length);
                    }
                }
                else
                {
                    this._cs.MoveToNextChar();
                }
            }
        }
    }
}
