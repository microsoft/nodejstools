/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/


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