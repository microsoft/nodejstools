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

namespace Microsoft.NodejsTools.Jade {
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tokenizer")]
    abstract class Tokenizer<T> : ITokenizer<T> where T : ITextRange {
        protected bool CComments { get; set; }
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpp")]
        protected bool CppComments { get; set; }
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpp")]
        protected bool MultilineCppComments { get; set; }
        protected bool SingleQuotedStrings { get; set; }
        protected bool DoubleQuotedStrings { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        protected CharacterStream _cs { get; set; }
        protected TextRangeCollection<T> Tokens { get; set; }

        protected Tokenizer() {
            CComments = true;
            CppComments = true;
            MultilineCppComments = false;
            SingleQuotedStrings = true;
            DoubleQuotedStrings = true;
        }

        public ReadOnlyTextRangeCollection<T> Tokenize(ITextProvider textProvider, int start, int length) {
            return Tokenize(textProvider, start, length, false);
        }

        public virtual ReadOnlyTextRangeCollection<T> Tokenize(ITextProvider textProvider, int start, int length, bool excludePartialTokens) {
            Debug.Assert(start >= 0 && length >= 0 && start + length <= textProvider.Length);

            _cs = new CharacterStream(textProvider);
            _cs.Position = start;

            Tokens = new TextRangeCollection<T>();

            while (!_cs.IsEndOfStream()) {
                // Keep on adding tokens...
                AddNextToken();

                if (_cs.Position >= start + length)
                    break;
            }

            if (excludePartialTokens) {
                int end = start + length;

                // Exclude tokens that are beyond the specified range
                int i;
                for (i = Tokens.Count - 1; i >= 0; i--) {
                    if (Tokens[i].End <= end)
                        break;
                }

                i++;

                if (i < Tokens.Count)
                    Tokens.RemoveRange(i, Tokens.Count - i);
            }

            var collection = new ReadOnlyTextRangeCollection<T>(Tokens);
            Tokens = null;

            return collection;
        }

        protected abstract T GetCommentToken(int start, int length);
        protected abstract T GetStringToken(int start, int length);

        protected virtual bool AddNextToken() {
            SkipWhiteSpace();

            if (_cs.IsEndOfStream())
                return true;

            switch (_cs.CurrentChar) {
                case '\'':
                    if (SingleQuotedStrings) {
                        HandleString();
                        return true;
                    }
                    break;

                case '\"':
                    if (DoubleQuotedStrings) {
                        HandleString();
                        return true;
                    }
                    break;

                case '/':
                    if (_cs.NextChar == '/' && CppComments) {
                        HandleCppComment();
                        return true;
                    } else if (_cs.NextChar == '*' && CComments) {
                        HandleCComment();
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Processes comments (// or /*)
        /// </summary>
        /// <returns>True if comment included new line characters</returns>
        protected virtual bool HandleComment(bool multiline) {
            if (_cs.CurrentChar == '/') {
                if (CComments && _cs.NextChar == '*') {
                    HandleCComment();
                    return false;
                } else if (CppComments && _cs.NextChar == '/') {
                    return HandleCppComment(multiline);
                }
            }

            return false;
        }

        /// <summary>
        /// Processes C++ style comments (//)
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpp")]
        protected bool HandleCppComment() {
            return HandleCppComment(false);
        }

        /// <summary>
        /// Processes C++ style comments (//)
        /// </summary>
        /// <returns>True if comment included new line characters</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpp")]
        protected bool HandleCppComment(bool multiline = false) {
            // SaSS version can span more than one line like this (indented):
            //
            // // This comment will not appear in the CSS output.
            //      This is nested beneath the comment as well,
            //      so it also won't appear.

            int start = _cs.Position;
            int baseIndent = 0;

            if (MultilineCppComments) {
                baseIndent = CalculateLineIndent();
                multiline = true;

                // only standalone // comments can span more than one line
                for (int i = _cs.Position - 1; i >= 0; i--) {
                    char ch = _cs[i];

                    if (ch == '\r' || ch == '\n')
                        break;

                    if (!Char.IsWhiteSpace(ch)) {
                        multiline = false;
                        break;
                    }
                }
            } else {
                multiline = false;
            }

            _cs.Advance(2); // skip over //

            while (!_cs.IsEndOfStream()) {
                int eolPosition = _cs.Position;

                if (_cs.IsAtNewLine()) {
                    if (multiline) {
                        // skip '\r'
                        _cs.MoveToNextChar();

                        // Skip '\n' 
                        if (_cs.IsAtNewLine())
                            _cs.MoveToNextChar();

                        SkipToNonWhiteSpaceOrEndOfLine();
                        if (_cs.IsEndOfStream()) {
                            _cs.Position = eolPosition;
                            break;
                        }

                        var lineIndent = CalculateLineIndent();
                        if (lineIndent <= baseIndent) {
                            // Ignore empty lines, they do not break current block
                            if (lineIndent == 0 && _cs.IsAtNewLine()) {
                                continue;
                            } else {
                                _cs.Position = eolPosition;
                                break;
                            }
                        }
                    } else {
                        break;
                    }
                }

                _cs.MoveToNextChar();
            }

            int length = _cs.Position - start;
            if (length > 0)
                Tokens.Add(GetCommentToken(start, length));

            return true;
        }

        /// <summary>
        /// Processes C-style comments (/* */)
        /// </summary>
        /// <returns>True if comment includes new line characters</returns>
        protected virtual void HandleCComment() {
            int start = _cs.Position;

            _cs.Advance(2);

            while (!_cs.IsEndOfStream()) {
                if (_cs.CurrentChar == '*' && _cs.NextChar == '/') {
                    _cs.Advance(2);
                    break;
                }

                _cs.MoveToNextChar();
            }

            int length = _cs.Position - start;

            if (length > 0)
                Tokens.Add(GetCommentToken(start, length));
        }

        /// <summary>
        /// Handles single or double quoted strings
        /// </summary>
        protected virtual ITextRange HandleString(bool addToken = true) {
            int start = _cs.Position;
            char quote = _cs.CurrentChar;

            // since the escape char is exactly the string openning char we say we start in escaped mode
            // it will get reset by the first char regardless what it is, but it will keep the '' case honest
            _cs.MoveToNextChar();

            while (!_cs.IsEndOfStream() && !_cs.IsAtNewLine()) {
                if (_cs.CurrentChar == '\\' && _cs.NextChar == quote) {
                    _cs.Advance(2);
                }

                if (_cs.CurrentChar == quote) {
                    _cs.MoveToNextChar();
                    break;
                }

                _cs.MoveToNextChar();
            }

            var range = TextRange.FromBounds(start, _cs.Position);
            if (range.Length > 0)
                Tokens.Add(GetStringToken(start, range.Length));

            return range;
        }

        /// <summary>
        /// Collects all characters up to the next whitespace
        /// </summary>
        /// <returns>Sequence range</returns>
        protected ITextRange GetNonWSSequence() {
            return GetNonWSSequence('\0', inclusive: false);
        }

        /// <summary>
        /// Collects all characters up to the next whitespace
        /// </summary>
        /// <param name="terminator">Terminator character</param>
        /// <param name="inclusive">True if sequence includes the terminator, 
        /// false if advance should stop at the terminator character</param>
        /// <returns>Sequence range</returns>
        protected virtual ITextRange GetNonWSSequence(char terminator, bool inclusive) {
            int start = _cs.Position;

            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace()) {
                if (_cs.CurrentChar == terminator && terminator != '\0') {
                    if (inclusive)
                        _cs.MoveToNextChar();

                    break;
                }

                _cs.MoveToNextChar();
            }

            return TextRange.FromBounds(start, _cs.Position);
        }

        /// <summary>
        /// Collects all characters up to the next whitespace always
        /// including the current character
        /// </summary>
        /// <returns>Sequence range</returns>
        protected ITextRange GetNonWSSequence(string terminators) {
            int start = _cs.Position;

            _cs.MoveToNextChar();

            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace()) {
                if (terminators.IndexOf(_cs.CurrentChar) != -1)
                    return TextRange.FromBounds(start, _cs.Position);

                _cs.MoveToNextChar();
            }

            return TextRange.FromBounds(start, _cs.Position);
        }

        /// <summary>
        /// Collects 'identifier' sequence. Identifier consists of ANSI characters and decimal digits.
        /// </summary>
        /// <returns>Identifier range</returns>
        protected virtual ITextRange ParseIdentifier() {
            int start = _cs.Position;

            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace() &&
                  (_cs.IsAnsiLetter() || _cs.IsDecimal() || _cs.CurrentChar == '_')) {
                _cs.MoveToNextChar();
            }

            return TextRange.FromBounds(start, _cs.Position);
        }

        /// <summary>
        /// Determines amount of leading whitespace in the current line
        /// </summary>
        protected int CalculateLineIndent() {
            int baseIndent = -1;

            // Find base tag indent
            for (int pos = _cs.Position - 1; pos >= 0 && baseIndent < 0; pos--) {
                if (_cs[pos] == '\n' || _cs[pos] == '\r') {
                    pos++;
                    for (int j = pos; j < _cs.Position + 1; j++) {
                        if (j == _cs.Position || !Char.IsWhiteSpace(_cs[j])) {
                            baseIndent = j - pos;
                            break;
                        }
                    }

                    break;
                }
            }

            return baseIndent >= 0 ? baseIndent : 0;
        }

        /// <summary>
        /// Skips over all whitespace. Stops at non-whitespace character or end of file.
        /// </summary>
        /// <returns>True if whitespace included newline characters</returns>
        protected virtual bool SkipWhiteSpace() {
            bool newLine = false;

            while (!_cs.IsEndOfStream()) {
                if (!_cs.IsWhiteSpace())
                    break;

                if (_cs.IsAtNewLine())
                    newLine = true;

                _cs.MoveToNextChar();
            }

            return newLine;
        }

        /// <summary>
        /// Advances character stream to the next end of line.
        /// </summary>
        protected virtual void SkipToEndOfLine() {
            while (!_cs.IsEndOfStream() && !_cs.IsAtNewLine()) {
                _cs.MoveToNextChar();
            }
        }

        /// <summary>
        /// Advances character stream to the next end of line, comment or end of file.
        /// </summary>
        protected virtual void SkipToEndOfLineOrComment() {
            while (!_cs.IsEndOfStream() && !_cs.IsAtNewLine() && !IsAtComment()) {
                _cs.MoveToNextChar();
            }
        }

        /// <summary>
        /// Advances character stream to the next whitespace, comment or end of file.
        /// </summary>
        protected virtual void SkipToWhiteSpaceOrComment() {
            while (!_cs.IsEndOfStream()) {
                if (_cs.IsWhiteSpace() || IsAtComment())
                    break;

                _cs.MoveToNextChar();
            }
        }

        /// <summary>
        /// Advances character stream to the next whitespace
        /// </summary>
        protected virtual void SkipToWhiteSpace() {
            while (!_cs.IsEndOfStream()) {
                if (_cs.IsWhiteSpace())
                    break;

                _cs.MoveToNextChar();
            }
        }

        /// <summary>
        /// Advances character stream to the next non-whitespace character
        /// </summary>
        protected virtual void SkipToNonWhiteSpaceOrEndOfLine() {
            while (!_cs.IsEndOfStream()) {
                if (!_cs.IsWhiteSpace() || _cs.IsAtNewLine())
                    break;

                _cs.MoveToNextChar();
            }
        }

        /// <summary>
        /// Checks if character stream is at a comment sequence (// or /*)
        /// </summary>
        protected virtual bool IsAtComment() {
            return (_cs.CurrentChar == '/' && ((_cs.NextChar == '/' && CppComments) || (_cs.NextChar == '*' && CComments)));
        }

        /// <summary>
        /// Checks if character stream is at a string (' or ")
        /// </summary>
        protected virtual bool IsAtString() {
            return ((_cs.CurrentChar == '\'' && SingleQuotedStrings) || (_cs.CurrentChar == '\"' && DoubleQuotedStrings));
        }

        /// <summary>
        /// Determines if remaning part of the line is all whitespace
        /// </summary>
        protected virtual bool IsAllWhiteSpaceBeforeEndOfLine(int position) {
            bool allWS = true;

            for (int i = position; i < _cs.Length; i++) {
                char ch = _cs[i];

                if (ch == '\r' || ch == '\n')
                    break;

                if (!Char.IsWhiteSpace(ch)) {
                    allWS = false;
                    break;
                }
            }

            return allWS;
        }
    }
}
