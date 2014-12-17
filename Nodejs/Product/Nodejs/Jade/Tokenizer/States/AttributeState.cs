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


namespace Microsoft.NodejsTools.Jade {
    internal partial class JadeTokenizer : Tokenizer<JadeToken> {
        private void OnAttributes(char terminator) {
            _attributeState = true;

            if (_cs.CurrentChar == '(')
                _cs.MoveToNextChar();

            while (!_cs.IsEndOfStream()) {
                // newlines are permitted in attributes
                SkipWhiteSpace();

                if (_cs.CurrentChar == terminator) {
                    _cs.MoveToNextChar();
                    break;
                }

                if (_cs.IsAtString()) {
                    HandleString();
                    continue;
                }

                if (_cs.CurrentChar == ',') {
                    _cs.MoveToNextChar();
                    continue;
                }

                var range = GetAttribute();
                if (range.Length > 0) {
                    AddToken(JadeTokenType.AttributeName, range.Start, range.Length);

                    SkipWhiteSpace();

                    if (_cs.CurrentChar == '=') {
                        AddToken(JadeTokenType.Operator, _cs.Position, 1);
                        _cs.MoveToNextChar();

                        SkipWhiteSpace();

                        if (IsAtString()) {
                            range = HandleString();
                        } else if (_cs.IsAnsiLetter() || _cs.IsDecimal()) {
                            range = GetAttributeValue();
                            if (range.Length > 0)
                                AddToken(JadeTokenType.AttributeValue, range.Start, range.Length);
                        }
                    }
                } else {
                    SkipToWhiteSpace();
                }
            }

            _attributeState = false;
        }

        /// <summary>
        /// Collects 'identifier' sequence. Identifier consists of ANSI characters and decimal digits.
        /// </summary>
        /// <returns>Identifier range</returns>
        protected override ITextRange ParseIdentifier() {
            int start = _cs.Position;

            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace() &&
                  (_cs.IsAnsiLetter() || _cs.IsDecimal() || _cs.CurrentChar == '_')) {
                _cs.MoveToNextChar();
            }

            return TextRange.FromBounds(start, _cs.Position);
        }
    }
}