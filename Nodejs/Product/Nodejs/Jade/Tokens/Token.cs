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
    /// <summary>
    /// Implements <seealso cref="IToken"/>. Derives from <seealso cref="TextRange"/>
    /// </summary>
    /// <typeparam name="T">Token type (typically enum)</typeparam>
    class Token<T> : TextRange, IToken<T> {
        T _tokenType;

        /// <summary>
        /// Create token based on type and text range
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="range">Token range in the text provider</param>
        public Token(T tokenType, ITextRange range)
            : base(range) {
            _tokenType = tokenType;
        }

        /// <summary>
        /// Create token based on token type, start and end of the text range.
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="start">Range start</param>
        /// <param name="length">Range length</param>
        public Token(T tokenType, int start, int length)
            : base(start, length) {
            _tokenType = tokenType;
        }

        /// <summary>
        /// Create token based on token type, start and end of the text range.
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="start">Range start</param>
        /// <param name="end">Range end</param>
        public static Token<T> FromBounds(T tokenType, int start, int end) {
            return new Token<T>(tokenType, start, end - start);
        }

        /// <summary>
        /// Token type
        /// </summary>
        public virtual T TokenType {
            get { return _tokenType; }
        }

        /// <summary>
        /// Determines if token is a comment
        /// </summary>
        public virtual bool IsComment {
            get {
                return false;
            }
        }

        /// <summary>
        /// Determines if token is a string
        /// </summary>
        public virtual bool IsString {
            get {
                return false;
            }
        }

        /// <summary>
        /// Token is a number
        /// </summary>
        public virtual bool IsNumber {
            get {
                return false;
            }
        }

        /// <summary>
        /// Token is a punctuator (comma, semicolon, plus, minus, ...)
        /// </summary>
        public virtual bool IsPunctuator {
            get {
                return false;
            }
        }

        /// <summary>
        /// Token is a language keyword (if, do, while, for, ...)
        /// </summary>
        public virtual bool IsKeyword {
            get {
                return false;
            }
        }
    }
}
