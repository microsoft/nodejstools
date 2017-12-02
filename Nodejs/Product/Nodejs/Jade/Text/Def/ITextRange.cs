// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Represents an item that has a range in a text document
    /// </summary>
    internal interface ITextRange
    {
        /// <summary>
        /// Range start.
        /// </summary>
        int Start { get; }

        /// <summary>
        /// Range end.
        /// </summary>
        int End { get; }

        /// <summary>
        /// Length of the range.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Determines if range contains given position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Tru if position is inside the range</returns>
        bool Contains(int position);

        /// <summary>
        /// Shifts range by a given offset.
        /// </summary>
        void Shift(int offset);
    }

    /// <summary>
    /// Represents an item that has a range in a text document
    /// </summary>
    internal interface IExpandableTextRange : ITextRange
    {
        /// <summary>
        /// Changes range boundaries by the given offsets
        /// </summary>
        void Expand(int startOffset, int endOffset);
    }
}
