// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.NodejsTools.Jade
{
    internal class JadeToken : Token<JadeTokenType>
    {
        public readonly IClassificationType Classification;

        public JadeToken(JadeTokenType type, int start, int length)
            : base(type, start, length)
        {
        }

        public JadeToken(JadeTokenType type, IClassificationType classification, int start, int length)
            : base(type, start, length)
        {
            this.Classification = classification;
        }

        public override bool IsComment => this.TokenType == JadeTokenType.Comment;
        public override bool IsString => this.TokenType == JadeTokenType.String;
    }
}
