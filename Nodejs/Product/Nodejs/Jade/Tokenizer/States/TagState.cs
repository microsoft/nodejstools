// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Jade
{
    internal partial class JadeTokenizer : Tokenizer<JadeToken>
    {
        private void OnTag()
        {
            var ident = string.Empty;
            var blockIndent = CalculateLineIndent();

            // regular tag like
            // html
            //   body
            var range = ParseTagName();
            if (range.Length > 0)
            {
                ident = this._cs.GetSubstringAt(range.Start, range.Length);

                if (JadeTagKeywords.IsKeyword(ident))
                {
                    // extends, javascripts:, stylesheets:
                    var length = this._cs.CurrentChar == ':' ? range.Length + 1 : range.Length;
                    AddToken(JadeTokenType.TagKeyword, range.Start, length);

                    if (this._cs.CurrentChar != ':' && StringComparer.Ordinal.Equals(ident, "mixin"))
                    {
                        SkipWhiteSpace();

                        if (!this._cs.IsAtNewLine())
                        {
                            OnTag();
                        }
                    }
                    else
                    {
                        SkipToEndOfLine();
                    }

                    return;
                }

                if (this._cs.CurrentChar == ':')
                {
                    // Block expansion like 
                    //   li.first: a(href='#') foo
                    AddToken(JadeTokenType.TagName, range.Start, range.Length);
                    this._cs.MoveToNextChar();

                    SkipWhiteSpace();

                    if (!this._cs.IsAtNewLine())
                    {
                        OnTag();
                    }

                    return;
                }

                if (JadeCodeKeywords.IsKeyword(ident))
                {
                    AddToken(JadeTokenType.CodeKeyword, range.Start, range.Length);
                    OnInlineCode();
                    return;
                }

                AddToken(JadeTokenType.TagName, range.Start, range.Length);
            }

            while (!this._cs.IsWhiteSpace() && !this._cs.IsEndOfStream())
            {
                if (this._cs.CurrentChar == '.' && char.IsWhiteSpace(this._cs.NextChar))
                {
                    // If this is last ., then what follows is a text literal
                    if (StringComparer.OrdinalIgnoreCase.Equals(ident, "script"))
                    {
                        this._cs.MoveToNextChar();
                        OnScript(blockIndent);
                    }
                    else if (StringComparer.OrdinalIgnoreCase.Equals(ident, "style"))
                    {
                        this._cs.MoveToNextChar();
                        OnStyle(blockIndent);
                    }
                    else if (IsAllWhiteSpaceBeforeEndOfLine(this._cs.Position + 1))
                    {
                        SkipToEndOfBlock(blockIndent, text: true);
                    }
                    else
                    {
                        this._cs.MoveToNextChar();
                    }

                    return;
                }

                if (this._cs.CurrentChar == '(')
                {
                    OnAttributes(')');
                }

                if (this._cs.CurrentChar == '#' || this._cs.CurrentChar == '.')
                {
                    var isID = this._cs.CurrentChar == '#';
                    // container(a=b).bar or container(a=b)#bar
                    var selectorRange = GetNonWSSequence("(:=.#");

                    if (selectorRange.Length > 0)
                    {
                        AddToken(
                            isID ? JadeTokenType.IdLiteral : JadeTokenType.ClassLiteral,
                            selectorRange.Start,
                            selectorRange.Length
                        );

                        if (char.IsWhiteSpace(this._cs.CurrentChar) && this._cs.LookAhead(-1) == '.')
                        {
                            this._cs.Position--;
                        }
                    }
                }

                if (this._cs.CurrentChar != '.' && this._cs.CurrentChar != '#' && this._cs.CurrentChar != '(')
                {
                    break;
                }
            }

            if (this._cs.CurrentChar == ':')
            {
                // Block expansion like 
                //   li.first: a(href='#') foo

                this._cs.MoveToNextChar();
                SkipWhiteSpace();

                if (!this._cs.IsAtNewLine())
                {
                    OnTag();
                }

                return;
            }

            // There may be ws between tag name and = sign. However, = must be on the same line.
            var allWsToEol = IsAllWhiteSpaceBeforeEndOfLine(this._cs.Position);
            if (!allWsToEol)
            {
                SkipWhiteSpace();

                if (this._cs.CurrentChar == '=' || (this._cs.CurrentChar == '!' && this._cs.NextChar == '='))
                {
                    // Something like 'foo ='
                    var length = this._cs.CurrentChar == '!' ? 2 : 1;

                    AddToken(JadeTokenType.Operator, this._cs.Position, length);
                    this._cs.Advance(length);

                    OnInlineCode();
                }
                else
                {
                    OnText(strings: false, html: true, entities: true);
                }
            }
            else
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(ident, "script"))
                {
                    OnScript(blockIndent);
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(ident, "style"))
                {
                    OnStyle(blockIndent);
                }
                else
                {
                    SkipToEndOfLine();
                }
            }
        }

        protected ITextRange ParseTagName()
        {
            var start = this._cs.Position;
            var count = 0;

            while (!this._cs.IsEndOfStream() && !this._cs.IsWhiteSpace() &&
                  (this._cs.IsAnsiLetter() || this._cs.IsDecimal() || this._cs.CurrentChar == '_' || (count > 0 && this._cs.CurrentChar == '-')))
            {
                if (this._cs.CurrentChar == ':')
                {
                    if (this._cs.NextChar != '_' && (this._cs.NextChar < 'A' || this._cs.NextChar > 'z'))
                    {
                        break; // allow tags with namespaces
                    }
                }

                this._cs.MoveToNextChar();
                count++;
            }

            return TextRange.FromBounds(start, this._cs.Position);
        }
    }
}
