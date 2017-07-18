// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Implements <seealso cref="IToken"/>. Derives from <seealso cref="TextRange"/>
    /// </summary>
    /// <typeparam name="T">Token type (typically enum)</typeparam>
    internal class Token<T> : TextRange, IToken<T>
    {
        private T _tokenType;

        /// <summary>
        /// Create token based on type and text range
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="range">Token range in the text provider</param>
        public Token(T tokenType, ITextRange range)
            : base(range)
        {
            this._tokenType = tokenType;
        }

        /// <summary>
        /// Create token based on token type, start and end of the text range.
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="start">Range start</param>
        /// <param name="length">Range length</param>
        public Token(T tokenType, int start, int length)
            : base(start, length)
        {
            this._tokenType = tokenType;
        }

        /// <summary>
        /// Create token based on token type, start and end of the text range.
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="start">Range start</param>
        /// <param name="end">Range end</param>
        public static Token<T> FromBounds(T tokenType, int start, int end)
        {
            return new Token<T>(tokenType, start, end - start);
        }

        /// <summary>
        /// Token type
        /// </summary>
        public virtual T TokenType => this._tokenType;
        /// <summary>
        /// Determines if token is a comment
        /// </summary>
        public virtual bool IsComment => false;

        /// <summary>
        /// Determines if token is a string
        /// </summary>
        public virtual bool IsString => false;

        /// <summary>
        /// Token is a number
        /// </summary>
        public virtual bool IsNumber => false;

        /// <summary>
        /// Token is a punctuator (comma, semicolon, plus, minus, ...)
        /// </summary>
        public virtual bool IsPunctuator => false;

        /// <summary>
        /// Token is a language keyword (if, do, while, for, ...)
        /// </summary>
        public virtual bool IsKeyword => false;
    }
}
