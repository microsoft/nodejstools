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
    /// Represents an item that has a range in a text document
    /// </summary>
    interface ITextRange {
        /// <summary>
        /// Range start.
        /// </summary>
        int Start { get; }

        /// <summary>
        /// Range end.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "End")]
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
    interface IExpandableTextRange : ITextRange {
        /// <summary>
        /// Changes range boundaries by the given offsets
        /// </summary>
        void Expand(int startOffset, int endOffset);
    }
}
