// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
                {
                    break;
                }

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
                else if (this._cs.CurrentChar == '<' && (this._cs.NextChar == '/' || char.IsLetter(this._cs.NextChar)) && html)
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
                        if (EntityTable.IsEntity(candidate, out var mappedChar))
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
