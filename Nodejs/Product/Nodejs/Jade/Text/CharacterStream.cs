//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Helper class that represents stream of characters for a parser or tokenizer
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
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
            _text = textProvider;

            int end = Math.Min(_text.Length, range.End);

            _range = TextRange.FromBounds(range.Start, end);

            Position = _range.Start;
            _currentChar = _text[_range.Start];
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
        public ITextProvider Text
        {
            get { return _text; }
        }

        /// <summary>
        /// Determines if current position is at the end of text
        /// </summary>
        /// <returns>True if position is at the end of stream</returns>
        public bool IsEndOfStream()
        {
            return _isEndOfStream;
        }

        public int DistanceFromEnd
        {
            get { return _range.End - Position; }
        }

        /// <summary>
        /// Returns character at a given position. If position is beyond text limits, returns '\0'
        /// </summary>
        /// <param name="position">Stream position</param>
        public char this[int position]
        {
            get
            {
                return _text[position];
            }
        }

        public string GetSubstringAt(int position, int length)
        {
            return _text.GetText(new TextRange(position, length));
        }

        public int IndexOf(string text, int start, bool ignoreCase)
        {
            return _text.IndexOf(text, start, ignoreCase);
        }

        public bool CompareTo(int position, int length, string text, bool ignoreCase)
        {
            return _text.CompareTo(position, length, text, ignoreCase);
        }

        public char CurrentChar { get { return _currentChar; } }

        public char NextChar
        {
            get { return Position + 1 < _range.End ? _text[Position + 1] : '\0'; }
        }

        /// <summary>
        /// Returns characters at an offset from the current position
        /// </summary>
        /// <param name="offset">Offset from the current position</param>
        /// <returns>Character or '\0' if offset is beyond text boundaries</returns>
        public char LookAhead(int offset)
        {
            int pos = Position + offset;

            if (pos < 0 || pos >= _text.Length)
                return '\0';

            return _text[pos];
        }

        /// <summary>
        /// Current stream position
        /// </summary>
        public int Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                CheckBounds();
            }
        }

        /// <summary>
        /// Length of the stream
        /// </summary>
        public int Length
        {
            get { return _range.Length; }
        }

        /// <summary>
        /// Moves current position forward or backward
        /// </summary>
        /// <param name="offset">Offset to move by</param>
        public void Advance(int offset)
        {
            Position += offset;
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
            return (_currentChar == ' ' || _currentChar == '\t' || _currentChar == '\r' ||
                    _currentChar == '\n' || _currentChar == '\f' || _currentChar == 0x200B);
        }

        /// <summary>
        /// Determines if current character starts a string (i.e. current character is a single or double quote).
        /// </summary>
        public bool IsAtString()
        {
            return (_currentChar == '\'' || _currentChar == '\"');
        }

        /// <summary>
        /// Determines if current character is a new line character
        /// </summary>
        public bool IsAtNewLine()
        {
            return IsNewLine(_currentChar);
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
            return IsAnsiLetter(_currentChar);
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
            return IsDecimal() || (_currentChar >= 'A' && _currentChar <= 'F') || (_currentChar >= 'a' && _currentChar <= 'f');
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
            return IsDecimal(_currentChar);
        }

        private void CheckBounds()
        {
            if (_position < 0)
                _position = 0;

            int maxPosition = Math.Min(_text.Length, _range.End);

            _isEndOfStream = _position >= maxPosition;
            if (_isEndOfStream)
                _position = maxPosition;

            _currentChar = _isEndOfStream ? '\0' : _text[Position];
        }

        /// <summary>
        /// Determines if character is a decimal digit
        /// </summary>
        public static bool IsDecimal(char character)
        {
            return (character >= '0' && character <= '9');
        }

        [ExcludeFromCodeCoverage]
        [DebuggerStepThrough]
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "@{0} ({1})", Position, _text[Position]);
        }
    }
}
