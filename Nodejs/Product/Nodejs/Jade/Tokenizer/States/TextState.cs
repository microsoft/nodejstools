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

namespace Microsoft.NodejsTools.Jade
{
    internal partial class JadeTokenizer : Tokenizer<JadeToken>
    {
        // plain text with possible #{foo} variable references
        private void OnText(bool strings, bool html, bool entities)
        {
            while (!this._cs.IsEndOfStream())
            {
                if (this._cs.IsAtNewLine())
                    break;

                if (this._cs.CurrentChar == '#' && this._cs.NextChar == '{')
                {
                    var range = GetNonWSSequence('}', inclusive: true);
                    if (range.Length > 0)
                    {
                        AddToken(JadeTokenType.Variable, range.Start, range.Length);
                    }
                }
                else if (this._cs.IsAtString() && strings)
                {
                    HandleString();
                }
                else if (this._cs.CurrentChar == '<' && (this._cs.NextChar == '/' || Char.IsLetter(this._cs.NextChar)) && html)
                {
                    OnHtml();
                }
                else if (this._cs.CurrentChar == '&' && entities)
                {
                    // entity check
                    this._cs.MoveToNextChar();

                    var range = GetNonWSSequence(';', inclusive: false);
                    if (this._cs.CurrentChar == ';')
                    {
                        var candidate = this._cs.GetSubstringAt(range.Start, range.Length);
                        char mappedChar;
                        if (EntityTable.IsEntity(candidate, out mappedChar))
                        {
                            this._cs.MoveToNextChar();
                            AddToken(JadeTokenType.Entity, range.Start - 1, range.Length + 2);
                        }
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
