// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Implements <seealso cref="ITextProvider"/> on a string
    /// </summary>
    internal class TextStream : ITextProvider
    {
        private string _text;

        // Array access (i.e. converting string to an array)
        // is faster, but takes more memory.

        [DebuggerStepThrough]
        public TextStream(string text)
        {
            this._text = text;
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return this._text;
        }

        #region ITextStream

        /// <summary>
        /// Text length
        /// </summary>
        public int Length => this._text.Length;
        /// <summary>
        /// Retrieves character at a given position
        /// </summary>
        public char this[int position]
        {
            get
            {
                if (position < 0 || position >= this._text.Length)
                {
                    return '\0';
                }

                return this._text[position];
            }
        }

        /// <summary>
        /// Retrieves a substring given start position and length
        /// </summary>
        public string GetText(int position, int length)
        {
            if (length == 0)
            {
                return string.Empty;
            }

            Debug.Assert(position >= 0 && length >= 0 && position + length <= this._text.Length);
            return this._text.Substring(position, length);
        }

        /// <summary>
        /// Retrieves substring given text range
        /// </summary>
        [DebuggerStepThrough]
        public string GetText(ITextRange range)
        {
            return GetText(range.Start, range.Length);
        }

        /// <summary>
        /// Searches text for a givne string starting at specified position
        /// </summary>
        /// <param name="stringToFind">String to find</param>
        /// <param name="startPosition">Starting position</param>
        /// <param name="ignoreCase">True if search should be case-insensitive</param>
        /// <returns>Character index of the first string appearance or -1 if string was not found</returns>
        public int IndexOf(string stringToFind, int startPosition, bool ignoreCase)
        {
            return this._text.IndexOf(stringToFind, startPosition, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Searches text for a given string within text fragment 
        /// that starts at specified position 
        /// </summary>
        /// <param name="stringToFind">String to find</param>
        /// <param name="range">Range to search in</param>
        /// <param name="ignoreCase">True if search should be case-insensitive</param>
        /// <returns>Character index of the first string appearance or -1 if string was not found</returns>
        public int IndexOf(string stringToFind, ITextRange range, bool ignoreCase)
        {
            if (range.Start + stringToFind.Length > this._text.Length)
            {
                return -1;
            }

            if (range.End > this._text.Length)
            {
                return -1;
            }

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return this._text.IndexOf(stringToFind, range.Start, range.Length, comparison);
        }

        public bool CompareTo(int position, int length, string compareTo, bool ignoreCase)
        {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return string.Compare(this._text, position, compareTo, 0, length, comparison) == 0;
        }

        public ITextProvider Clone()
        {
            return new TextStream(this._text);
        }

        public int Version => 0;
        // static string text provider does not fire text change event
#pragma warning disable 0067
        public event EventHandler<TextChangeEventArgs> OnTextChange;
#pragma warning restore 0067

        #endregion

        #region Dispose
        public void Dispose()
        {
            this._text = null;
        }
        #endregion
    }
}
