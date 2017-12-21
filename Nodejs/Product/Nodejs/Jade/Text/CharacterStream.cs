// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Helper class that represents stream of characters for a parser or tokenizer
    /// </summary>
    internal class CharacterStream
    {
        private char _currentChar;
        private ITextProvider _text;
        private TextRange _range;
        private int _position = 0;
        private bool _isEndOfStream = false;

        #region Constructors

        [DebuggerStepThrough]
        public CharacterStream(ITextProvider textProvider)
            : this(textProvider, TextRange.FromBounds(0, textProvider.Length))
        {
        }

        public CharacterStream(ITextProvider textProvider, ITextRange range)
        {
            this._text = textProvider;

            var end = Math.Min(this._text.Length, range.End);

            this._range = TextRange.FromBounds(range.Start, end);

            this.Position = this._range.Start;
            this._currentChar = this._text[this._range.Start];
        }

        [DebuggerStepThrough]
        public CharacterStream(string text)
            : this(new TextStream(text))
        {
        }
        #endregion

        /// <summary>
        /// Text provider that supplies underlying text. May be a string, a text buffer or a buffer snapshot.
        /// </summary>
        public ITextProvider Text => this._text;
        /// <summary>
        /// Determines if current position is at the end of text
        /// </summary>
        /// <returns>True if position is at the end of stream</returns>
        public bool IsEndOfStream()
        {
            return this._isEndOfStream;
        }

        public int DistanceFromEnd => this._range.End - this.Position;
        /// <summary>
        /// Returns character at a given position. If position is beyond text limits, returns '\0'
        /// </summary>
        /// <param name="position">Stream position</param>
        public char this[int position]
        {
            get
            {
                return this._text[position];
            }
        }

        public string GetSubstringAt(int position, int length)
        {
            return this._text.GetText(new TextRange(position, length));
        }

        public int IndexOf(string text, int start, bool ignoreCase)
        {
            return this._text.IndexOf(text, start, ignoreCase);
        }

        public bool CompareTo(int position, int length, string text, bool ignoreCase)
        {
            return this._text.CompareTo(position, length, text, ignoreCase);
        }

        public char CurrentChar => this._currentChar;
        public char NextChar => this.Position + 1 < this._range.End ? this._text[this.Position + 1] : '\0';
        /// <summary>
        /// Returns characters at an offset from the current position
        /// </summary>
        /// <param name="offset">Offset from the current position</param>
        /// <returns>Character or '\0' if offset is beyond text boundaries</returns>
        public char LookAhead(int offset)
        {
            var pos = this.Position + offset;

            if (pos < 0 || pos >= this._text.Length)
            {
                return '\0';
            }

            return this._text[pos];
        }

        /// <summary>
        /// Current stream position
        /// </summary>
        public int Position
        {
            get
            {
                return this._position;
            }
            set
            {
                this._position = value;
                CheckBounds();
            }
        }

        /// <summary>
        /// Length of the stream
        /// </summary>
        public int Length => this._range.Length;
        /// <summary>
        /// Moves current position forward or backward
        /// </summary>
        /// <param name="offset">Offset to move by</param>
        public void Advance(int offset)
        {
            this.Position += offset;
        }

        /// <summary>
        /// Moves position to the next character if possible.
        /// </summary>
        public void MoveToNextChar()
        {
            Advance(1);
        }

        /// <summary>
        /// Detemines if current character is a whitespace
        /// </summary>
        public bool IsWhiteSpace()
        {
            // Char.IsWhiteSpace is slow
            return (this._currentChar == ' ' || this._currentChar == '\t' || this._currentChar == '\r' ||
                    this._currentChar == '\n' || this._currentChar == '\f' || this._currentChar == 0x200B);
        }

        /// <summary>
        /// Determines if current character starts a string (i.e. current character is a single or double quote).
        /// </summary>
        public bool IsAtString()
        {
            return (this._currentChar == '\'' || this._currentChar == '\"');
        }

        /// <summary>
        /// Determines if current character is a new line character
        /// </summary>
        public bool IsAtNewLine()
        {
            return IsNewLine(this._currentChar);
        }

        public static bool IsNewLine(char currentCharacter)
        {
            return (currentCharacter == '\n' || currentCharacter == '\r');
        }

        /// <summary>
        /// Determines if current character is an ANSI letter
        /// </summary>
        public bool IsAnsiLetter()
        {
            return IsAnsiLetter(this._currentChar);
        }

        /// <summary>
        /// Determines if current character is an ANSI letter
        /// </summary>
        public static bool IsAnsiLetter(char character)
        {
            return (character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z');
        }

        /// <summary>
        /// Determines if character is a hexadecimal digit
        /// </summary>
        public bool IsHex()
        {
            return IsDecimal() || (this._currentChar >= 'A' && this._currentChar <= 'F') || (this._currentChar >= 'a' && this._currentChar <= 'f');
        }

        public static bool IsHex(char character)
        {
            return IsDecimal(character) || (character >= 'A' && character <= 'F') || (character >= 'a' && character <= 'f');
        }

        /// <summary>
        /// Determines if character is a decimal digit
        /// </summary>
        public bool IsDecimal()
        {
            return IsDecimal(this._currentChar);
        }

        private void CheckBounds()
        {
            if (this._position < 0)
            {
                this._position = 0;
            }

            var maxPosition = Math.Min(this._text.Length, this._range.End);

            this._isEndOfStream = this._position >= maxPosition;
            if (this._isEndOfStream)
            {
                this._position = maxPosition;
            }

            this._currentChar = this._isEndOfStream ? '\0' : this._text[this.Position];
        }

        /// <summary>
        /// Determines if character is a decimal digit
        /// </summary>
        public static bool IsDecimal(char character)
        {
            return (character >= '0' && character <= '9');
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "@{0} ({1})", this.Position, this._text[this.Position]);
        }
    }
}
