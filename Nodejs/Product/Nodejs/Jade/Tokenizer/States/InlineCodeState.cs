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
        private void OnInlineCode() {
            if (_cs.CurrentChar == '-' || _cs.CurrentChar == '=')
                _cs.MoveToNextChar();

            // Make tokens for code keywords
            while (!_cs.IsEndOfStream()) {
                if (SkipWhiteSpace())
                    break;

                if (IsAtString()) {
                    HandleString();
                } else if (_cs.IsAnsiLetter()) {
                    var range = ParseIdentifier();
                    var ident = _cs.GetSubstringAt(range.Start, range.Length);

                    if (JadeCodeKeywords.IsKeyword(ident))
                        AddToken(JadeTokenType.CodeKeyword, range.Start, range.Length);
                } else if (_cs.CurrentChar == '=' || _cs.CurrentChar == '+' || _cs.CurrentChar == '*' || _cs.CurrentChar == '/' || _cs.CurrentChar == '-') {
                    AddToken(JadeTokenType.Operator, _cs.Position, 1);
                    _cs.MoveToNextChar();
                } else if (_cs.CurrentChar == '#' && _cs.NextChar == '{') {
                    var range = GetNonWSSequence('}', inclusive: true);
                    if (range.Length > 0) {
                        AddToken(JadeTokenType.Variable, range.Start, range.Length);
                    }
                } else {
                    _cs.MoveToNextChar();
                }
            }
        }
    }
}
