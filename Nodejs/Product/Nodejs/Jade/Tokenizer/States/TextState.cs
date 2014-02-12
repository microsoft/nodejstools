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

using System;

namespace Microsoft.NodejsTools.Jade {
    internal partial class JadeTokenizer : Tokenizer<JadeToken> {
        // plain text with possible #{foo} variable references
        private void OnText(bool strings, bool html, bool entities) {
            while (!_cs.IsEndOfStream()) {
                if (_cs.IsAtNewLine())
                    break;

                if (_cs.CurrentChar == '#' && _cs.NextChar == '{') {
                    var range = GetNonWSSequence('}', inclusive: true);
                    if (range.Length > 0) {
                        AddToken(JadeTokenType.Variable, range.Start, range.Length);
                    }
                } else if (_cs.IsAtString() && strings) {
                    HandleString();
                } else if (_cs.CurrentChar == '<' && (_cs.NextChar == '/' || Char.IsLetter(_cs.NextChar)) && html) {
                    OnHtml();
                } else if (_cs.CurrentChar == '&' && entities) {
                    // entity check
                    _cs.MoveToNextChar();

                    var range = GetNonWSSequence(';', inclusive: false);
                    if (_cs.CurrentChar == ';') {
                        var candidate = _cs.GetSubstringAt(range.Start, range.Length);
                        char mappedChar;
                        if (EntityTable.IsEntity(candidate, out mappedChar)) {
                            _cs.MoveToNextChar();
                            AddToken(JadeTokenType.Entity, range.Start - 1, range.Length + 2);
                        }
                    }
                } else {
                    _cs.MoveToNextChar();
                }
            }
        }
    }
}
