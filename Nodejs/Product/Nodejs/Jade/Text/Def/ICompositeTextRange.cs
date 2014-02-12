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
    /// Represents a set of text ranges. It may or may not be actual collection
    /// internally, but it supports shifting its content according to the supplied start
    /// position and an offset. For example, an artifact is a composite range since internally
    /// it typically consists of left separator, inner range and right separator. However,
    /// artifact may not expose its inner parts as a collection of ranges.
    /// </summary>
    interface ICompositeTextRange : ITextRange {
        /// <summary>
        /// Shifts items in collection starting from given position by the specified offset.
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="offset">Offset to shift items by</param>
        void ShiftStartingFrom(int start, int offset);
    }
}
