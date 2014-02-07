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

using Microsoft.VisualStudio.Text.Classification;
namespace Microsoft.NodejsTools.Jade {
    class JadeToken : Token<JadeTokenType> {
        public readonly IClassificationType Classification;

        public JadeToken(JadeTokenType type, int start, int length)
            : base(type, start, length) {
        }

        public JadeToken(JadeTokenType type, IClassificationType classification, int start, int length)
            : base(type, start, length) {
            Classification = classification;
        }

        public override bool IsComment {
            get { return TokenType == JadeTokenType.Comment; }
        }

        public override bool IsString {
            get { return TokenType == JadeTokenType.String; }
        }
    }

}
