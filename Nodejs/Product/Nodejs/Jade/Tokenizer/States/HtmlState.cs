// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Jade
{
    internal partial class JadeTokenizer : Tokenizer<JadeToken>
    {
        // plain text with possible #{foo} variable references
        private void OnHtml()
        {
            Debug.Assert(this._cs.CurrentChar == '<' && (this._cs.NextChar == '/' || char.IsLetter(this._cs.NextChar)));

            var length = this._cs.NextChar == '/' ? 2 : 1;
            AddToken(JadeTokenType.AngleBracket, this._cs.Position, length);
            this._cs.Advance(length);

            var range = GetAttribute();
            if (range.Length > 0)
            {
                AddToken(JadeTokenType.TagName, range.Start, range.Length);
            }

            OnAttributes('>');

            if (this._cs.LookAhead(-1) == '>')
            {
                AddToken(JadeTokenType.AngleBracket, this._cs.Position - 1, 1);
            }
        }
    }
}
