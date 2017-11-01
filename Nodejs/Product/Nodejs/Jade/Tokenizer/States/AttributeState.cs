// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Jade
{
    internal partial class JadeTokenizer : Tokenizer<JadeToken>
    {
        private void OnAttributes(char terminator)
        {
            this._attributeState = true;

            if (this._cs.CurrentChar == '(')
            {
                this._cs.MoveToNextChar();
            }

            while (!this._cs.IsEndOfStream())
            {
                // newlines are permitted in attributes
                SkipWhiteSpace();

                if (this._cs.CurrentChar == terminator)
                {
                    this._cs.MoveToNextChar();
                    break;
                }

                if (this._cs.IsAtString())
                {
                    HandleString();
                    continue;
                }

                if (this._cs.CurrentChar == ',')
                {
                    this._cs.MoveToNextChar();
                    continue;
                }

                var range = GetAttribute();
                if (range.Length > 0)
                {
                    AddToken(JadeTokenType.AttributeName, range.Start, range.Length);

                    SkipWhiteSpace();

                    if (this._cs.CurrentChar == '=')
                    {
                        AddToken(JadeTokenType.Operator, this._cs.Position, 1);
                        this._cs.MoveToNextChar();

                        SkipWhiteSpace();

                        if (IsAtString())
                        {
                            range = HandleString();
                        }
                        else if (this._cs.IsAnsiLetter() || this._cs.IsDecimal())
                        {
                            range = GetAttributeValue();
                            if (range.Length > 0)
                            {
                                AddToken(JadeTokenType.AttributeValue, range.Start, range.Length);
                            }
                        }
                    }
                }
                else
                {
                    SkipToWhiteSpace();
                }
            }

            this._attributeState = false;
        }

        /// <summary>
        /// Collects 'identifier' sequence. Identifier consists of ANSI characters and decimal digits.
        /// </summary>
        /// <returns>Identifier range</returns>
        protected override ITextRange ParseIdentifier()
        {
            var start = this._cs.Position;

            while (!this._cs.IsEndOfStream() && !this._cs.IsWhiteSpace() &&
                  (this._cs.IsAnsiLetter() || this._cs.IsDecimal() || this._cs.CurrentChar == '_'))
            {
                this._cs.MoveToNextChar();
            }

            return TextRange.FromBounds(start, this._cs.Position);
        }
    }
}
