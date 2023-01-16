// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Represents a set of text ranges. It may or may not be actual collection
    /// internally, but it supports shifting its content according to the supplied start
    /// position and an offset. For example, an artifact is a composite range since internally
    /// it typically consists of left separator, inner range and right separator. However,
    /// artifact may not expose its inner parts as a collection of ranges.
    /// </summary>
    internal interface ICompositeTextRange : ITextRange
    {
        /// <summary>
        /// Shifts items in collection starting from given position by the specified offset.
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="offset">Offset to shift items by</param>
        void ShiftStartingFrom(int start, int offset);
    }
}
