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
using System.Diagnostics;

namespace Microsoft.NodejsTools.Jade {
    internal partial class JadeTokenizer : Tokenizer<JadeToken> {
        // plain text with possible #{foo} variable references
        private void OnHtml() {
            Debug.Assert(_cs.CurrentChar == '<' && (_cs.NextChar == '/' || Char.IsLetter(_cs.NextChar)));

            int length = _cs.NextChar == '/' ? 2 : 1;
            AddToken(JadeTokenType.AngleBracket, _cs.Position, length);
            _cs.Advance(length);

            var range = GetAttribute();
            if (range.Length > 0)
                AddToken(JadeTokenType.TagName, range.Start, range.Length);

            OnAttributes('>');

            if (_cs.LookAhead(-1) == '>')
                AddToken(JadeTokenType.AngleBracket, _cs.Position - 1, 1);
        }
    }
}
