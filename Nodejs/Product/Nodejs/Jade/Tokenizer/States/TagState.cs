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
        private void OnTag() {
            string ident = String.Empty;
            int blockIndent = CalculateLineIndent();

            // regular tag like
            // html
            //   body
            var range = ParseTagName();
            if (range.Length > 0) {
                ident = _cs.GetSubstringAt(range.Start, range.Length);

                if (JadeTagKeywords.IsKeyword(ident)) {
                    // extends, javascripts:, stylesheets:
                    int length = _cs.CurrentChar == ':' ? range.Length + 1 : range.Length;
                    AddToken(JadeTokenType.TagKeyword, range.Start, length);

                    if (_cs.CurrentChar != ':' && String.Compare(ident, "mixin", StringComparison.Ordinal) == 0) {
                        SkipWhiteSpace();

                        if (!_cs.IsAtNewLine())
                            OnTag();
                    } else {
                        SkipToEndOfLine();
                    }

                    return;
                }

                if (_cs.CurrentChar == ':') {
                    // Block expansion like 
                    //   li.first: a(href='#') foo
                    AddToken(JadeTokenType.TagName, range.Start, range.Length);
                    _cs.MoveToNextChar();

                    SkipWhiteSpace();

                    if (!_cs.IsAtNewLine())
                        OnTag();

                    return;
                }

                if (JadeCodeKeywords.IsKeyword(ident)) {
                    AddToken(JadeTokenType.CodeKeyword, range.Start, range.Length);
                    OnInlineCode();
                    return;
                }

                AddToken(JadeTokenType.TagName, range.Start, range.Length);
            }

            if (_cs.CurrentChar == '#' || (_cs.CurrentChar == '.' && !Char.IsWhiteSpace(_cs.NextChar))) {
                // #container or .bar or #container.bar
                range = GetNonWSSequence("(:");
                if (range.Length > 0) {
                    AddToken(JadeTokenType.JadeSelector, range.Start, range.Length);

                    if (Char.IsWhiteSpace(_cs.CurrentChar) && _cs.LookAhead(-1) == '.')
                        _cs.Position--;
                }
            }

            // Check if identifier is followed by ( or =. If so, its a tag name.
            if (range.Length > 0) {
                while (!_cs.IsWhiteSpace() && !_cs.IsEndOfStream()) {
                    if (_cs.CurrentChar == '.' && Char.IsWhiteSpace(_cs.NextChar)) {
                        // If this is last ., then what follows is a text literal
                        if (String.Compare(ident, "script", StringComparison.OrdinalIgnoreCase) == 0) {
                            _cs.MoveToNextChar();
                            OnScript(blockIndent);
                        } else if (String.Compare(ident, "style", StringComparison.OrdinalIgnoreCase) == 0) {
                            _cs.MoveToNextChar();
                            OnStyle(blockIndent);
                        } else if (IsAllWhiteSpaceBeforeEndOfLine(_cs.Position + 1)) {
                            SkipToEndOfBlock(blockIndent, text: true);
                        }

                        return;
                    }

                    if (_cs.CurrentChar == '(') {
                        OnAttributes(')');
                    }

                    if (_cs.CurrentChar == '#' || _cs.CurrentChar == '.') {
                        // container(a=b).bar
                        var selectorRange = GetNonWSSequence("(:");

                        if (selectorRange.Length > 0) {
                            AddToken(JadeTokenType.JadeSelector, selectorRange.Start, selectorRange.Length);

                            if (Char.IsWhiteSpace(_cs.CurrentChar) && _cs.LookAhead(-1) == '.') {
                                _cs.Position--;
                            }
                        }
                    }

                    if (_cs.CurrentChar != '.' && _cs.CurrentChar != '#' && _cs.CurrentChar != '(')
                        break;
                }

                if (_cs.CurrentChar == ':') {
                    // Block expansion like 
                    //   li.first: a(href='#') foo

                    _cs.MoveToNextChar();
                    SkipWhiteSpace();

                    if (!_cs.IsAtNewLine())
                        OnTag();

                    return;
                }

            }

            // There may be ws between tag name and = sign. However, = must be on the same line.
            bool allWsToEol = IsAllWhiteSpaceBeforeEndOfLine(_cs.Position);
            if (!allWsToEol) {
                SkipWhiteSpace();

                if (_cs.CurrentChar == '=' || (_cs.CurrentChar == '!' && _cs.NextChar == '=')) {
                    // Something like 'foo ='
                    int length = _cs.CurrentChar == '!' ? 2 : 1;

                    AddToken(JadeTokenType.Operator, _cs.Position, length);
                    _cs.Advance(length);

                    OnInlineCode();
                } else {
                    OnText(strings: false, html: true, entities: true);
                }
            } else {
                if (String.Compare(ident, "script", StringComparison.OrdinalIgnoreCase) == 0) {
                    OnScript(blockIndent);
                } else if (String.Compare(ident, "style", StringComparison.OrdinalIgnoreCase) == 0) {
                    OnStyle(blockIndent);
                } else {
                    SkipToEndOfLine();
                }
            }
        }

        protected ITextRange ParseTagName() {
            int start = _cs.Position;
            int count = 0;

            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace() &&
                  (_cs.IsAnsiLetter() || _cs.IsDecimal() || _cs.CurrentChar == '_' || (count > 0 && _cs.CurrentChar == '-'))) {
                if (_cs.CurrentChar == ':') {
                    if (_cs.NextChar != '_' && (_cs.NextChar < 'A' || _cs.NextChar > 'z'))
                        break; // allow tags with namespaces
                }

                _cs.MoveToNextChar();
                count++;
            }

            return TextRange.FromBounds(start, _cs.Position);
        }
    }
}