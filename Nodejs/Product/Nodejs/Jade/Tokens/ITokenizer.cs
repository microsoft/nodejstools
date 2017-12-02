// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Generic tokenizer
    /// </summary>
    internal interface ITokenizer<T> where T : ITextRange
    {
        /// <summary>
        /// Tokenize text from a given provider
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="start">Start position</param>
        /// <param name="length">Length of fragent to tokenize</param>
        /// <param name="excludePartialTokens">True if tokenizeer should exclude partial token that may intersect end of the specified span</param>
        /// <returns>Collection of tokens</returns>
        ReadOnlyTextRangeCollection<T> Tokenize(ITextProvider textProvider, int start, int length, bool excludePartialTokens);

        /// <summary>
        /// Tokenize text from a given provider
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="start">Start position</param>
        /// <param name="length">Length of fragent to tokenize</param>
        /// <returns>Collection of tokens</returns>
        ReadOnlyTextRangeCollection<T> Tokenize(ITextProvider textProvider, int start, int length);
    }
}
