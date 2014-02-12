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

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// Text change event arguments. This class abstracts text change information 
    /// allowing code that handles text changes to use <seealso cref="ITextProvider"/>
    /// rather than Visual Studio ITextBuffer or some other editor specific types.
    /// </summary>
    [ExcludeFromCodeCoverage]
    class TextChangeEventArgs : EventArgs {
        /// <summary>
        /// Start position of the change
        /// </summary>
        public int Start { get; private set; }

        /// <summary>
        /// Length of the fragment that was deleted or replaced.
        /// Zero if operation is 'insert' or 'paste' without selection.
        /// </summary>
        public int OldLength { get; private set; }

        /// <summary>
        /// Length of the new fragment. Zero if operation is 'delete'.
        /// </summary>
        public int NewLength { get; private set; }

        /// <summary>
        /// Snaphot before the change
        /// </summary>
        public ITextProvider OldText { get; private set; }

        /// <summary>
        /// Snapshot after the change
        /// </summary>
        public ITextProvider NewText { get; private set; }

        public TextChangeEventArgs(int start, int oldLength, int newLength)
            : this(start, oldLength, newLength, null, null) {
        }

        [DebuggerStepThrough]
        public TextChangeEventArgs(int start, int oldLength, int newLength, ITextProvider oldText, ITextProvider newText) {
            Start = start;
            OldLength = oldLength;
            NewLength = newLength;
            OldText = oldText;
            NewText = newText;
        }
    }
}
