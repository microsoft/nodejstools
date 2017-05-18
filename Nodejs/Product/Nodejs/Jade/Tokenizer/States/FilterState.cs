// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Jade
{
    internal partial class JadeTokenizer : Tokenizer<JadeToken>
    {
        private void OnFilter()
        {
            // :markdown
            this._cs.MoveToNextChar();

            var range = ParseIdentifier();
            if (range.Length > 0)
            {
                var text = this._cs.GetSubstringAt(range.Start, range.Length);
                if (JadeFilters.IsFilter(text))
                {
                    AddToken(JadeTokenType.Filter, range.Start - 1, range.Length + 1);

                    var blockIndent = CalculateLineIndent();
                    SkipToEndOfBlock(blockIndent, text: true);

                    return;
                }
            }

            SkipToEndOfLine();
        }
    }
}
