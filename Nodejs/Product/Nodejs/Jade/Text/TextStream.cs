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
    /// Implements <seealso cref="ITextProvider"/> on a string
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    class TextStream : ITextProvider {
        string _text;

        // Array access (i.e. converting string to an array)
        // is faster, but takes more memory.

        [DebuggerStepThrough]
        public TextStream(string text) {
            _text = text;
        }

        [DebuggerStepThrough]
        public override string ToString() {
            return _text;
        }

        #region ITextStream

        /// <summary>
        /// Text length
        /// </summary>
        public int Length {
            get { return _text.Length; }
        }

        /// <summary>
        /// Retrieves character at a given position
        /// </summary>
        public char this[int position] {
            get {
                if (position < 0 || position >= _text.Length)
                    return '\0';

                return _text[position];
            }
        }

        /// <summary>
        /// Retrieves a substring given start position and length
        /// </summary>
        public string GetText(int position, int length) {
            if (length == 0)
                return String.Empty;

            Debug.Assert(position >= 0 && length >= 0 && position + length <= _text.Length);
            return _text.Substring(position, length);
        }

        /// <summary>
        /// Retrieves substring given text range
        /// </summary>
        [DebuggerStepThrough]
        public string GetText(ITextRange range) {
            return GetText(range.Start, range.Length);
        }

        /// <summary>
        /// Searches text for a givne string starting at specified position
        /// </summary>
        /// <param name="stringToFind">String to find</param>
        /// <param name="startPosition">Starting position</param>
        /// <param name="ignoreCase">True if search should be case-insensitive</param>
        /// <returns>Character index of the first string appearance or -1 if string was not found</returns>
        public int IndexOf(string stringToFind, int startPosition, bool ignoreCase) {
            return _text.IndexOf(stringToFind, startPosition, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Searches text for a given string within text fragment 
        /// that starts at specified position 
        /// </summary>
        /// <param name="stringToFind">String to find</param>
        /// <param name="range">Range to search in</param>
        /// <param name="ignoreCase">True if search should be case-insensitive</param>
        /// <returns>Character index of the first string appearance or -1 if string was not found</returns>
        public int IndexOf(string stringToFind, ITextRange range, bool ignoreCase) {
            if (range.Start + stringToFind.Length > _text.Length)
                return -1;

            if (range.End > _text.Length)
                return -1;

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return _text.IndexOf(stringToFind, range.Start, range.Length, comparison);
        }

        public bool CompareTo(int position, int length, string compareTo, bool ignoreCase) {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return String.Compare(_text, position, compareTo, 0, length, comparison) == 0;
        }

        public ITextProvider Clone() {
            return new TextStream(_text);
        }

        public int Version { get { return 0; } }

        // static string text provider does not fire text change event
#pragma warning disable 0067
        public event EventHandler<TextChangeEventArgs> OnTextChange;
#pragma warning restore 0067

        #endregion

        #region Dispose
        public void Dispose() {
            _text = null;
        }
        #endregion
    }
}
