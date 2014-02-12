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
        private void OnFilter() {
            // :markdown
            _cs.MoveToNextChar();

            var range = ParseIdentifier();
            if (range.Length > 0) {
                var text = _cs.GetSubstringAt(range.Start, range.Length);
                if (JadeFilters.IsFilter(text)) {
                    AddToken(JadeTokenType.Filter, range.Start - 1, range.Length + 1);

                    int blockIndent = CalculateLineIndent();
                    SkipToEndOfBlock(blockIndent, text: true);

                    return;
                }
            }

            SkipToEndOfLine();
        }
    }
}