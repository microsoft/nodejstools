// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Describes a parse token. Parse token is a text range
    /// with a type that describes nature of the range.
    /// Derives from <seealso cref="ITextRange"/>
    /// </summary>
    /// <typeparam name="T">Token type (typically enum)</typeparam>
    internal interface IToken<T> : ITextRange
    {
        /// <summary>
        /// Type of the token
        /// </summary>
        T TokenType { get; }
    }
}
