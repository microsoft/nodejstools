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

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// Generic tokenizer
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tokenizer")]
    interface ITokenizer<T> where T : ITextRange {
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
