// jsscanner.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    public sealed class JSScanner
    {
        #region static fields

        // keyword table
        private static readonly JSKeyword[] s_Keywords = JSKeyword.InitKeywords();

        private static readonly OperatorPrecedence[] s_OperatorsPrec = InitOperatorsPrec();

        #endregion

        #region private fields

        private string _source;
        private StringBuilder _identifier; // the identifier we're building up for the current token
        private int _lastPosOnBuilder; // the position of the last identifier character appended into m_identifier
        private string _decodedString; // the string literal we're building up for the current token
        private int _currentPosition;
        private List<int> _newLineLocations;
        private SourceLocation _initialLocation;
        private State _state;
        internal IndexResolver _indexResolver;
        private int _tokenStartIndex, _tokenEndIndex;

        private readonly ErrorSink _errorSink;

        #endregion

        #region public properties

        /// <summary>
        /// Gets/sets the current parser state for restarting
        /// the scanner.
        /// </summary>
        public object CurrentState {
            get {
                return _state;
            }
        }

        public string StringLiteralValue { get { return _decodedString; } }

        public bool IsEndOfFile { get { return _currentPosition >= _source.Length; } }

        internal string Identifier
        {
            get
            {
                return _identifier.Length > 0
                    ? _identifier.ToString() :
                    CurrentTokenString();
            }
        }

        private string CurrentTokenString() {
            return _source.Substring(_tokenStartIndex, _tokenEndIndex - _tokenStartIndex);
        }

        #endregion

        #region constructors

        
        public JSScanner(string source, ErrorSink errorSink = null) {
            _errorSink = errorSink ?? new ErrorSink();
            _currentPosition = 0;

            Initialize(source, null, new SourceLocation(0, 1, 1));
            
            // create a string builder that we'll keep reusing as we
            // scan identifiers. We'll build the unescaped name into it
            _identifier = new StringBuilder(128);
        }

        public JSScanner() {
            // stuff we don't want to copy
            _decodedString = null;
            _identifier = new StringBuilder(128);
            _errorSink = new ErrorSink();
        }

        /// <summary>
        /// Initializes the scanner for parsing from a previous location
        /// where it left off.  DocumentContext provides the source code,
        /// state provides the previous state.
        /// </summary>
        public void Initialize(string source, object state, SourceLocation initialLocation) {
            if (state != null && !(state is State)) {
                throw new InvalidOperationException("state object must have come from JSScanner.CurrentState");
            }

            _source = source;
            _initialLocation = initialLocation;
            _newLineLocations = new List<int>();
            _indexResolver = new IndexResolver(_newLineLocations, initialLocation);

            _currentPosition = 0;
            if (state == null) {
                _state = new State();
            } else {
                _state = (State)state;
            }
        }

        public void Uninitialize() {
            _source = null;
        }

        public JSScanner Clone()
        {
            return new JSScanner(_source, _errorSink)
            {
                _currentPosition = this._currentPosition,
                _lastPosOnBuilder = this._lastPosOnBuilder,
                _source = this._source,
            };
        }

        #endregion

        /// <summary>
        /// main method for the scanner; scans the next token from the input stream.
        /// </summary>
        /// <param name="scanForRegularExpressionLiterals">whether to try scanning a regexp when encountering a /</param>
        /// <returns>next token from the input</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "big case statement")]
        public TokenWithSpan ScanNextToken(bool scanForRegularExpressionLiterals)
        {
            var token = JSToken.None;

            _tokenStartIndex = _currentPosition;

            _identifier.Length = 0;

            if (_state.UnterminatedComment) {
                SkipMultilineComment();
                token = JSToken.MultipleLineComment;
                _tokenEndIndex = _currentPosition;
                return MakeContext(token);
            }

            // our case switch should be pretty efficient -- it's 9-13 and 32-126. Thsose are the most common characters 
            // we will find in the code for the start of tokens.
            char c = GetChar(_currentPosition++);
            switch (c)
            {
                case '\n':
                case '\r':
                    token = ScanLineTerminator(c);
                    break;

                case '\t':
                case '\v':
                case '\f':
                case ' ':
                    // we are asking for raw tokens, and this is the start of a stretch of whitespace.
                    // advance to the end of the whitespace, and return that as the token
                    token = JSToken.WhiteSpace;
                    while (JSScanner.IsBlankSpace(GetChar(_currentPosition)))
                    {
                        ++_currentPosition;
                    }

                    break;

                case '!':
                    token = JSToken.LogicalNot;
                    if ('=' == GetChar(_currentPosition))
                    {
                        _currentPosition++;
                        token = JSToken.NotEqual;
                        if ('=' == GetChar(_currentPosition))
                        {
                            _currentPosition++;
                            token = JSToken.StrictNotEqual;
                        }
                    }

                    break;

                case '"':
                case '\'':
                    token = JSToken.StringLiteral;
                    ScanString(c);
                    break;

                case '$':
                case '_':
                    ScanIdentifier();
                    token = JSToken.Identifier;
                    break;

                case '%':
                    token = JSToken.Modulo;
                    if ('=' == GetChar(_currentPosition))
                    {
                        _currentPosition++;
                        token = JSToken.ModuloAssign;
                    }

                    break;

                case '&':
                    token = JSToken.BitwiseAnd;
                    c = GetChar(_currentPosition);
                    if ('&' == c)
                    {
                        _currentPosition++;
                        token = JSToken.LogicalAnd;
                    }
                    else if ('=' == c)
                    {
                        _currentPosition++;
                        token = JSToken.BitwiseAndAssign;
                    }

                    break;

                case '(':
                    token = JSToken.LeftParenthesis;
                    break;

                case ')':
                    token = JSToken.RightParenthesis;
                    break;

                case '*':
                    token = JSToken.Multiply;
                    if ('=' == GetChar(_currentPosition))
                    {
                        _currentPosition++;
                        token = JSToken.MultiplyAssign;
                    }

                    break;

                case '+':
                    token = JSToken.Plus;
                    c = GetChar(_currentPosition);
                    if ('+' == c)
                    {
                        _currentPosition++;
                        token = JSToken.Increment;
                    }
                    else if ('=' == c)
                    {
                        _currentPosition++;
                        token = JSToken.PlusAssign;
                    }

                    break;

                case ',':
                    token = JSToken.Comma;
                    break;

                case '-':
                    token = JSToken.Minus;
                    c = GetChar(_currentPosition);
                    if ('-' == c)
                    {
                        _currentPosition++;
                        token = JSToken.Decrement;
                    }
                    else if ('=' == c)
                    {
                        _currentPosition++;
                        token = JSToken.MinusAssign;
                    }

                    break;

                case '.':
                    token = JSToken.AccessField;
                    c = GetChar(_currentPosition);
                    if (JSScanner.IsDigit(c))
                    {
                        token = ScanNumber('.');
                    }

                    break;

                case '/':
                    token = JSToken.Divide;
                    c = GetChar(_currentPosition);
                    switch (c)
                    {
                        case '/':
                            token = JSToken.SingleLineComment;
                            c = GetChar(++_currentPosition);


                            SkipSingleLineComment();

                            break;

                        case '*':
                            SkipMultilineComment();
                            token = JSToken.MultipleLineComment;
                            break;

                        default:
                            // if we were passed the hint that we prefer regular expressions
                            // over divide operators, then try parsing one now.
                            if (scanForRegularExpressionLiterals)
                            {
                                // we think this is probably a regular expression.
                                // if it is...
                                if (ScanRegExp() != null)
                                {
                                    // also scan the flags (if any)
                                    ScanRegExpFlags();
                                    token = JSToken.RegularExpression;
                                }
                                else if (c == '=')
                                {
                                    _currentPosition++;
                                    token = JSToken.DivideAssign;
                                }
                            }
                            else if (c == '=')
                            {
                                _currentPosition++;
                                token = JSToken.DivideAssign;
                            }
                            break;
                    }

                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    token = ScanNumber(c);
                    break;

                case ':':
                    token = JSToken.Colon;
                    break;

                case ';':
                    token = JSToken.Semicolon;
                    break;

                case '<':
                    token = JSToken.LessThan;
                    if ('<' == GetChar(_currentPosition))
                    {
                        _currentPosition++;
                        token = JSToken.LeftShift;
                    }

                    if ('=' == GetChar(_currentPosition))
                    {
                        _currentPosition++;
                        if (token == JSToken.LessThan)
                        {
                            token = JSToken.LessThanEqual;
                        }
                        else
                        {
                            token = JSToken.LeftShiftAssign;
                        }
                    }

                    break;

                case '=':
                    token = JSToken.Assign;
                    if ('=' == GetChar(_currentPosition))
                    {
                        _currentPosition++;
                        token = JSToken.Equal;
                        if ('=' == GetChar(_currentPosition))
                        {
                            _currentPosition++;
                            token = JSToken.StrictEqual;
                        }
                    }

                    break;

                case '>':
                    token = JSToken.GreaterThan;
                    if ('>' == GetChar(_currentPosition))
                    {
                        _currentPosition++;
                        token = JSToken.RightShift;
                        if ('>' == GetChar(_currentPosition))
                        {
                            _currentPosition++;
                            token = JSToken.UnsignedRightShift;
                        }
                    }

                    if ('=' == GetChar(_currentPosition))
                    {
                        _currentPosition++;
                        token = token == JSToken.GreaterThan ? JSToken.GreaterThanEqual
                            : token == JSToken.RightShift ? JSToken.RightShiftAssign
                            : token == JSToken.UnsignedRightShift ? JSToken.UnsignedRightShiftAssign
                            : JSToken.Error;
                    }

                    break;

                case '?':
                    token = JSToken.ConditionalIf;
                    break;

                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                    token = JSToken.Identifier;
                    ScanIdentifier();
                    break;

                case '[':
                    token = JSToken.LeftBracket;
                    break;

                case '\\':
                    // try decoding a unicode escape sequence. We read the backslash and
                    // now the "current" character is the "u"
                    if (PeekUnicodeEscape(_currentPosition, ref c))
                    {
                        // advance past the escape characters
                        _currentPosition += 5;

                        // valid unicode escape sequence
                        if (IsValidIdentifierStart(c))
                        {
                            // use the unescaped character as the first character of the
                            // decoded identifier, and current character is now the last position
                            // on the builder
                            _identifier.Append(c);
                            _lastPosOnBuilder = _currentPosition;

                            // scan the rest of the identifier
                            ScanIdentifier();

                            // because it STARTS with an escaped character it cannot be a keyword
                            token = JSToken.Identifier;
                            break;
                        }
                    }
                    else
                    {
                        // not a valid unicode escape sequence
                        // see if the next character is a valid identifier character
                        if (IsValidIdentifierStart(GetChar(_currentPosition)))
                        {
                            // we're going to just assume this is an escaped identifier character
                            // because some older browsers allow things like \foo ("foo") and 
                            // \while to be an identifer "while" and not the reserved word
                            ScanIdentifier();
                            token = JSToken.Identifier;
                            break;
                        }
                    }

                    HandleError(JSError.IllegalChar);
                    break;

                case ']':
                    token = JSToken.RightBracket;
                    break;

                case '^':
                    token = JSToken.BitwiseXor;
                    if ('=' == GetChar(_currentPosition))
                    {
                        _currentPosition++;
                        token = JSToken.BitwiseXorAssign;
                    }

                    break;

                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                    JSKeyword keyword = s_Keywords[c - 'a'];
                    if (null != keyword)
                    {
                        token = ScanKeyword(keyword);
                    }
                    else
                    {
                        token = JSToken.Identifier;
                        ScanIdentifier();
                    }
                    break;

                case '{':
                    token = JSToken.LeftCurly;
                    break;

                case '|':
                    token = JSToken.BitwiseOr;
                    c = GetChar(_currentPosition);
                    if ('|' == c)
                    {
                        _currentPosition++;
                        token = JSToken.LogicalOr;
                    }
                    else if ('=' == c)
                    {
                        _currentPosition++;
                        token = JSToken.BitwiseOrAssign;
                    }

                    break;

                case '}':
                    token = JSToken.RightCurly;
                    break;

                case '~':
                    token = JSToken.BitwiseNot;
                    break;

                case '\0':
                    if (IsEndOfFile)
                    {
                        token = JSToken.EndOfFile;
                        _currentPosition--;
                    }
                    else
                    {
                        token = IllegalCharacter();
                    }
                    break;
                case '\u2028':
                case '\u2029':
                    // more line terminator
                    token = ScanLineTerminator(c);
                    break;
                default:
                    if (IsValidIdentifierStart(c))
                    {
                        token = JSToken.Identifier;
                        ScanIdentifier();
                    }
                    else if (IsBlankSpace(c))
                    {
                        // we are asking for raw tokens, and this is the start of a stretch of whitespace.
                        // advance to the end of the whitespace, and return that as the token
                        while (JSScanner.IsBlankSpace(GetChar(_currentPosition)))
                        {
                            ++_currentPosition;
                        }
                        token = JSToken.WhiteSpace;
                    }
                    else
                    {
                        token = IllegalCharacter();
                    }

                    break;
            }

            // fix up the end of the token
            _tokenEndIndex = _currentPosition;            
            return MakeContext(token);
        }

        public List<TokenWithSpan> ReadTokens(int characterCount) {
            List<TokenWithSpan> tokens = new List<TokenWithSpan>();

            int start = _currentPosition;

            while (_currentPosition- start < characterCount) {
                var token = ScanNextToken(true);
                tokens.Add(token);
                if (IsEndOfFile) {
                    break;
                }
            }

            return tokens;
        }

        
        private TokenWithSpan MakeContext(JSToken token) {
#if FALSE
            var startLoc = IndexToLocation(_tokenStartIndex);
            var endLoc = IndexToLocation(_tokenEndIndex);

            Debug.Assert(startLoc.Line == _state.TokenStartLineNumber);
            Debug.Assert(endLoc.Line == _state.TokenEndLineNumber);
            Debug.Assert(startLoc.Column == _state.TokenStartLinePosition);
            Debug.Assert(endLoc.Column == _state.TokenEndLinePosition);
#endif

            return new TokenWithSpan(
                _indexResolver,
                _tokenStartIndex + _initialLocation.Index,
                _tokenEndIndex + _initialLocation.Index,
                token                
            );

#if FALSE
            return new Context(
                _state.TokenStartLineNumber,
                _state.TokenStartLinePosition,
                _tokenStartIndex,
                _state.TokenEndLineNumber,
                _state.TokenEndLinePosition,
                _tokenEndIndex,
                token
            );
#endif
        }

        private JSToken ScanLineTerminator(char ch)
        {
            // line terminator
            var token = JSToken.EndOfLine;
            if (ch == '\r')
            {
                // \r\n is a valid SINGLE line-terminator. So if the \r is
                // followed by a \n, we only want to process a single line terminator.
                if (GetChar(_currentPosition) == '\n')
                {
                    _currentPosition++;
                }
            }
            _newLineLocations.Add(_currentPosition);

            // keep multiple line terminators together in a single token.
            // so get the current character after this last line terminator
            // and then keep looping until we hit something that isn't one.
            while ((ch = GetChar(_currentPosition)) == '\r' || ch == '\n' || ch == '\u2028' || ch == '\u2029')
            {
                if (ch == '\r')
                {
                    // skip over the \r and if the next one is an \n skip over it, too
                    if (GetChar(++_currentPosition) == '\n')
                    {
                        ++_currentPosition;
                    }
                }
                else
                {
                    // skip over any other non-\r character
                    ++_currentPosition;
                }
                _newLineLocations.Add(_currentPosition);
            }

            return token;
        }

        private JSToken IllegalCharacter()
        {
            _tokenEndIndex = _currentPosition;

            HandleError(JSError.IllegalChar);
            return JSToken.Error;
        }

        public static bool IsKeyword(string name, bool strictMode)
        {
            bool isKeyword = false;

            // get the index into the keywords array by taking the first letter of the string
            // and subtracting the character 'a' from it. Use a negative number if the string
            // is null or empty
            if (!string.IsNullOrEmpty(name))
            {
                int index = name[0] - 'a';

                // only proceed if the index is within the array length
                if (0 <= index && index < s_Keywords.Length)
                {
                    // get the head of the list for this index (if any)
                    JSKeyword keyword = s_Keywords[name[0] - 'a'];
                    if (keyword != null)
                    {
                        // switch off the token
                        switch (keyword.GetKeyword(name, 0, name.Length))
                        {
                            case JSToken.Get:
                            case JSToken.Set:
                            case JSToken.Identifier:
                                // never considered keywords
                                isKeyword = false;
                                break;

                            case JSToken.Implements:
                            case JSToken.Interface:
                            case JSToken.Let:
                            case JSToken.Package:
                            case JSToken.Private:
                            case JSToken.Protected:
                            case JSToken.Public:
                            case JSToken.Static:
                            case JSToken.Yield:
                                // in strict mode, these ARE keywords, otherwise they are okay
                                // to be identifiers
                                isKeyword = strictMode;
                                break;

                            default:
                                // no other tokens can be identifiers.
                                // apparently never allowed for Chrome, so we want to treat it
                                // differently, too
                                isKeyword = true;
                                break;
                        }
                    }
                }
            }

            return isKeyword;
        }

        private bool CheckSubstring(int startIndex, string target)
        {
            for (int ndx = 0; ndx < target.Length; ++ndx)
            {
                if (target[ndx] != GetChar(startIndex + ndx))
                {
                    // no match
                    return false;
                }
            }

            // if we got here, the strings match
            return true;
        }

        private bool CheckCaseInsensitiveSubstring(string target)
        {
            var startIndex = _currentPosition;
            for (int ndx = 0; ndx < target.Length; ++ndx)
            {
                if (target[ndx] != char.ToUpperInvariant(GetChar(startIndex + ndx)))
                {
                    // no match
                    return false;
                }
            }

            // if we got here, the strings match. Advance the current position over it
            _currentPosition += target.Length;
            return true;
        }

        private char GetChar(int index)
        {
            if (index < _source.Length)
            {
                return _source[index];
            }

            return '\0';
        }

        private void ScanIdentifier()
        {
            for (;;)
            {
                char c = GetChar(_currentPosition);
                if (!IsIdentifierPartChar(c))
                {
                    break;
                }

                ++_currentPosition;
            }

            

            if (_lastPosOnBuilder > 0)
            {
                _identifier.Append(_source.Substring(_lastPosOnBuilder, _currentPosition - _lastPosOnBuilder));
                _lastPosOnBuilder = 0;
            }
        }

        private JSToken ScanKeyword(JSKeyword keyword)
        {
            for (;;)
            {
                char c = GetChar(_currentPosition);
                if ('a' <= c && c <= 'z')
                {
                    _currentPosition++;
                    continue;
                }

                if (IsIdentifierPartChar(c))
                {
                    ScanIdentifier();
                    return JSToken.Identifier;
                }

                break;
            }

            return keyword.GetKeyword(
                _source,
                _tokenStartIndex,
                _currentPosition - _tokenStartIndex
            );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private JSToken ScanNumber(char leadChar)
        {
            bool noMoreDot = '.' == leadChar;
            JSToken token = noMoreDot ? JSToken.NumericLiteral : JSToken.IntegerLiteral;
            bool exponent = false;
            char c;

            if ('0' == leadChar)
            {
                // c is now the character AFTER the leading zero
                c = GetChar(_currentPosition);
                if ('x' == c || 'X' == c)
                {
                    if (JSScanner.IsHexDigit(GetChar(_currentPosition + 1)))
                    {
                        while (JSScanner.IsHexDigit(GetChar(++_currentPosition)))
                        {
                            // empty
                        }
                    }

                    return CheckForNumericBadEnding(token);
                }
#if FALSE
                else if ('b' == c || 'B' == c)
                {
                    // ES6 binary literal?
                    c = GetChar(m_currentPosition + 1);
                    if (c == '1' || c == '0')
                    {
                        while ('0' == (c = GetChar(++m_currentPosition)) || c == '1')
                        {
                            // iterator handled in the condition
                        }
                    }

                    return CheckForNumericBadEnding(token);
                }
                else if ('o' == c || 'O' == c)
                {
                    // ES6 octal literal?
                    c = GetChar(m_currentPosition + 1);
                    if ('0' <= c && c <= '7')
                    {
                        while ('0' <= (c = GetChar(++m_currentPosition)) && c <= '7')
                        {
                            // iterator handled in the condition
                        }
                    }

                    return CheckForNumericBadEnding(token);
                }
#endif
                else if ('0' <= c && c <= '7')
                {
                    // this is a zero followed by a digit between 0 and 7.
                    // This could be interpreted as an octal literal, which isn't strictly supported.
                    while ('0' <= c && c <= '7')
                    {
                        c = GetChar(++_currentPosition);
                    }

                    // return the integer token with issues, which should cause it to be output
                    // as-is and not combined with other literals or anything.
                    HandleError(JSError.OctalLiteralsDeprecated);
                    return token;
                }
                else if (c != 'e' && c != 'E' && IsValidIdentifierStart(c))
                {
                    // invalid for an integer (in this case '0') the be followed by
                    // an identifier part. The 'e' is okay, though, because that will
                    // be the exponent part.
                    // we know the 0 and the next character are both invalid, so skip them and 
                    // anything else after it that's identifier-like, and throw an error.
                    return CheckForNumericBadEnding(token);
                }
            }

            for (;;)
            {
                c = GetChar(_currentPosition);
                if (!JSScanner.IsDigit(c))
                {
                    if ('.' == c)
                    {
                        if (noMoreDot)
                        {
                            break;
                        }

                        noMoreDot = true;
                        token = JSToken.NumericLiteral;
                    }
                    else if ('e' == c || 'E' == c)
                    {
                        if (exponent)
                        {
                            break;
                        }

                        exponent = true;
                        token = JSToken.NumericLiteral;
                    }
                    else if ('+' == c || '-' == c)
                    {
                        char e = GetChar(_currentPosition - 1);
                        if ('e' != e && 'E' != e)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                _currentPosition++;
            }

            c = GetChar(_currentPosition - 1);
            if ('+' == c || '-' == c)
            {
                _currentPosition--;
                c = GetChar(_currentPosition - 1);
            }

            if ('e' == c || 'E' == c)
            {
                _currentPosition--;
            }

            // it is invalid for a numeric literal to be immediately followed by another
            // digit or an identifier start character. So check for those and return an
            // invalid numeric literal if true.
            return CheckForNumericBadEnding(token);
        }

        private JSToken CheckForNumericBadEnding(JSToken token)
        {
            // it is invalid for a numeric literal to be immediately followed by another
            // digit or an identifier start character. So check for those cases and return an
            // invalid numeric literal if true.
            char ch = GetChar(_currentPosition);
            if (('0' <= ch && ch <= '9') || IsValidIdentifierStart(ch))
            {
                // we know that next character is invalid, so skip it and 
                // anything else after it that's identifier-like, and throw an error.
                ++_currentPosition;
                while (IsValidIdentifierPart(GetChar(_currentPosition)))
                {
                    ++_currentPosition;
                }

                HandleError(JSError.BadNumericLiteral);
                token = JSToken.NumericLiteral;
            }

            return token;
        }

        internal String ScanRegExp()
        {
            int pos = _currentPosition;
            bool isEscape = false;
            bool isInSet = false;
            char c;
            while (!IsEndLineOrEOF(c = GetChar(_currentPosition++), 0))
            {
                if (isEscape)
                {
                    isEscape = false;
                }
                else if (c == '[')
                {
                    isInSet = true;
                }
                else if (isInSet)
                {
                    if (c == ']')
                    {
                        isInSet = false;
                    }
                }
                else if (c == '/')
                {
                    if (pos == _currentPosition)
                    {
                        return null;
                    }

                    _tokenEndIndex = _currentPosition;
                    return _source.Substring(
                        _tokenStartIndex + 1,
                        _tokenEndIndex - _tokenStartIndex - 2
                    );
                }
                else if (c == '\\')
                {
                    isEscape = true;
                }
            }

            // reset and return null. Assume it is not a reg exp
            _currentPosition = pos;
            return null;
        }

        internal String ScanRegExpFlags()
        {
            int pos = _currentPosition;
            while (JSScanner.IsAsciiLetter(GetChar(_currentPosition)))
            {
                _currentPosition++;
            }

            if (pos != _currentPosition)
            {
                _tokenEndIndex = _currentPosition;
                return _source.Substring(pos, _tokenEndIndex - pos);
            }

            return null;
        }

        //--------------------------------------------------------------------------------------------------
        // ScanString
        //
        //  Scan a string dealing with escape sequences.
        //  On exit this.escapedString will contain the string with all escape sequences replaced
        //  On exit this.currentPos must be at the next char to scan after the string
        //  This method wiil report an error when the string is unterminated or for a bad escape sequence
        //--------------------------------------------------------------------------------------------------
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void ScanString(char delimiter)
        {
            int start = _currentPosition;
            _decodedString = null;
            StringBuilder result = null;

            char ch;
            while((ch = GetChar(_currentPosition++)) != delimiter)
            {
                if (ch != '\\')
                {
                    // this is the common non escape case
                    if (IsLineTerminator(ch, 0))
                    {
                        // TODO: we want to flag this string as unterminated *and having issues*,
                        // and then somehow output a line-break in the output to duplicate the
                        // source. However, we will need to figure out how to NOT combine the statement
                        // with the next statement. For instance:
                        //      var x = "unterminated
                        //      var y = 42;
                        // should NOT get combined to: var x="unterminated,y=42;
                        // (same for moving it inside for-statements, combining expression statements, etc.)
                        HandleError(JSError.UnterminatedString);

                        // back up to the start of the line terminator
                        --_currentPosition;
                        if (GetChar(_currentPosition - 1) == '\r')
                        {
                            --_currentPosition;
                        }

                        break;
                    }

                    if ('\0' == ch)
                    {
                        // whether it's a null literal character within the string or an
                        // actual end of file, this string literal has issues....
                        if (IsEndOfFile)
                        {
                            _currentPosition--;
                            HandleError(JSError.UnterminatedString);
                            break;
                        }
                    }
                }
                else
                {
                    // ESCAPE CASE

                    // got an escape of some sort. Have to use the StringBuilder
                    if (null == result)
                    {
                        result = new StringBuilder(128);
                    }

                    // start points to the first position that has not been written to the StringBuilder.
                    // The first time we get in here that position is the beginning of the string, after that
                    // is the character immediately following the escape sequence
                    if (_currentPosition - start - 1 > 0)
                    {
                        // append all the non escape chars to the string builder
                        result.Append(_source, start, _currentPosition - start - 1);
                    }

                    // state variable to be reset
                    bool seqOfThree = false;
                    bool isValidHex;
                    int escapeStart;
                    int esc = 0;

                    ch = GetChar(_currentPosition++);
                    switch (ch)
                    {
                        // line terminator crap
                        case '\r':
                            if ('\n' == GetChar(_currentPosition))
                            {
                                _currentPosition++;
                            }

                            goto case '\n';

                        case '\n':
                        case '\u2028':
                        case '\u2029':
                            _newLineLocations.Add(_currentPosition);
                            break;

                        // classic single char escape sequences
                        case 'b':
                            result.Append((char)8);
                            break;

                        case 't':
                            result.Append((char)9);
                            break;

                        case 'n':
                            result.Append((char)10);
                            break;

                        case 'v':
                            // \v inside strings can cause issues
                            result.Append((char)11);
                            break;

                        case 'f':
                            result.Append((char)12);
                            break;

                        case 'r':
                            result.Append((char)13);
                            break;

                        case '"':
                            result.Append('"');
                            break;

                        case '\'':
                            result.Append('\'');
                            break;

                        case '\\':
                            result.Append('\\');
                            break;

                        // hexadecimal escape sequence /xHH
                        case 'x':
                            // save the start of the escape in case we fail
                            escapeStart = _currentPosition - 2;
                            isValidHex = true;
                            if (!ScanHexDigit(ref esc))
                            {
                                isValidHex = false;
                                
                                // if that invalid character (which the scan function skipped over)
                                // was a delimiter, back up!
                                if (GetChar(_currentPosition - 1) == delimiter)
                                {
                                    --_currentPosition;
                                }
                            }

                            if (!ScanHexDigit(ref esc))
                            {
                                isValidHex = false;

                                // if that invalid character (which the scan function skipped over)
                                // was a delimiter, back up!
                                if (GetChar(_currentPosition - 1) == delimiter)
                                {
                                    --_currentPosition;
                                }
                            }

                            if (isValidHex)
                            {
                                // valid; use the unescaped character
                                result.Append((char)esc);
                            }
                            else
                            {
                                // wasn't valid -- keep the original and flag this 
                                // as having issues
                                result.Append(_source.Substring(escapeStart, _currentPosition - escapeStart));
                                HandleError(JSError.BadHexEscapeSequence);
                            }
                            break;

                        // unicode escape sequence /uHHHH
                        case 'u':
                            // save the start of the escape in case we fail
                            escapeStart = _currentPosition - 2;
                            isValidHex = true;
                            if (!ScanHexDigit(ref esc))
                            {
                                isValidHex = false;
                                
                                // if that invalid character (which the scan function skipped over)
                                // was a delimiter, back up!
                                if (GetChar(_currentPosition - 1) == delimiter)
                                {
                                    --_currentPosition;
                                }
                            }

                            if (!ScanHexDigit(ref esc))
                            {
                                isValidHex = false;
                                
                                // if that invalid character (which the scan function skipped over)
                                // was a delimiter, back up!
                                if (GetChar(_currentPosition - 1) == delimiter)
                                {
                                    --_currentPosition;
                                }
                            }

                            if (!ScanHexDigit(ref esc))
                            {
                                isValidHex = false;
                                
                                // if that invalid character (which the scan function skipped over)
                                // was a delimiter, back up!
                                if (GetChar(_currentPosition - 1) == delimiter)
                                {
                                    --_currentPosition;
                                }
                            }

                            if (!ScanHexDigit(ref esc))
                            {
                                isValidHex = false;
                                
                                // if that invalid character (which the scan function skipped over)
                                // was a delimiter, back up!
                                if (GetChar(_currentPosition - 1) == delimiter)
                                {
                                    --_currentPosition;
                                }
                            }

                            if (isValidHex)
                            {
                                // valid; use the unescaped character
                                result.Append((char)esc);
                            }
                            else
                            {
                                // wasn't valid -- keep the original
                                result.Append(_source.Substring(escapeStart, _currentPosition - escapeStart));
                                HandleError(JSError.BadHexEscapeSequence);
                            }
                            break;

                        case '0':
                        case '1':
                        case '2':
                        case '3':
                            seqOfThree = true;
                            esc = (ch - '0') << 6;
                            goto case '4';

                        case '4':
                        case '5':
                        case '6':
                        case '7':
                            // octal literals inside strings can cause issues

                            // esc is reset at the beginning of the loop and it is used to check that we did not go through the cases 1, 2 or 3
                            if (!seqOfThree)
                            {
                                esc = (ch - '0') << 3;
                            }

                            ch = GetChar(_currentPosition++);
                            if ('0' <= ch && ch <= '7')
                            {
                                if (seqOfThree)
                                {
                                    esc |= (ch - '0') << 3;
                                    ch = GetChar(_currentPosition++);
                                    if ('0' <= ch && ch <= '7')
                                    {
                                        esc |= ch - '0';
                                        result.Append((char)esc);
                                    }
                                    else
                                    {
                                        result.Append((char)(esc >> 3));

                                        // do not skip over this char we have to read it back
                                        --_currentPosition;
                                    }
                                }
                                else
                                {
                                    esc |= ch - '0';
                                    result.Append((char)esc);
                                }
                            }
                            else
                            {
                                if (seqOfThree)
                                {
                                    result.Append((char)(esc >> 6));
                                }
                                else
                                {
                                    result.Append((char)(esc >> 3));
                                }

                                // do not skip over this char we have to read it back
                                --_currentPosition;
                            }

                            HandleError(JSError.OctalLiteralsDeprecated);
                            break;

                        default:
                            // not an octal number, ignore the escape '/' and simply append the current char
                            result.Append(ch);
                            break;
                    }

                    start = _currentPosition;
                }
            }

            // update the unescaped string
            if (null != result)
            {
                if (_currentPosition - start - 1 > 0)
                {
                    // append all the non escape chars to the string builder
                    result.Append(_source, start, _currentPosition - start - 1);
                }
                _decodedString = result.ToString();
            }
            else if (_currentPosition == _tokenStartIndex + 1)
            {
                // empty unterminated string!
                _decodedString = string.Empty;
            }
            else
            {
                // might be an unterminated string, so make sure that last character is the terminator
                int numDelimiters = (GetChar(_currentPosition - 1) == delimiter ? 2 : 1);
                _decodedString = _source.Substring(_tokenStartIndex + 1, _currentPosition - _tokenStartIndex - numDelimiters);
            }
        }

        private bool ScanHexDigit(ref int esc)
        {
            // get the current character and advance the pointer assuming it's good
            var ch = GetChar(_currentPosition++);

            // merge the hex digit's value into the unescaped value
            var isGoodValue = true;
            if (char.IsDigit(ch))
            {
                esc = esc << 4 | (ch - '0');
            }
            else if ('A' <= ch && ch <= 'F')
            {
                esc = esc << 4 | (ch - 'A' + 10);
            }
            else if ('a' <= ch && ch <= 'f')
            {
                esc = esc << 4 | (ch - 'a' + 10);
            }
            else
            {
                // not a valid hex character!
                isGoodValue = false;
            }

            // good to go
            return isGoodValue;
        }

        private void SkipAspNetReplacement()
        {
            // the current position is on the % of the opening delimiter, so
            // advance the pointer forward to the first character AFTER the opening
            // delimiter, then keep skipping
            // forward until we find the closing %>. Be sure to set the current pointer
            // to the NEXT character AFTER the > when we find it.
            ++_currentPosition;

            char ch;
            while ((ch = GetChar(_currentPosition++)) != '\0' || !IsEndOfFile)
            {
                if (ch == '%'
                    && GetChar(_currentPosition) == '>')
                {
                    // found the closing delimiter -- the current position in on the >
                    // so we need to advance to the next character and break out of the loop
                    ++_currentPosition;
                    break;
                }
            }
        }

        private void SkipSingleLineComment()
        {
            // skip up to the terminator, but don't include them.
            // the single-line comment does NOT include the line terminator!
            SkipToEndOfLine();

            // fix up the end of the token
            _tokenEndIndex = _currentPosition;
        }

        private void SkipToEndOfLine()
        {
            var c = GetChar(_currentPosition);
            while (c != 0
                && c != '\n'
                && c != '\r'
                && c != '\x2028'
                && c != '\x2029')
            {
                c = GetChar(++_currentPosition);
            }
        }

        private void SkipOneLineTerminator()
        {
            var c = GetChar(_currentPosition);
            if (c == '\r')
            {
                // skip over the \r; and if it's followed by a \n, skip it, too
                if (GetChar(++_currentPosition) == '\n')
                {
                    ++_currentPosition;
                }
                _newLineLocations.Add(_currentPosition);
            }
            else if (c == '\n'
                || c == '\x2028'
                || c == '\x2029')
            {
                // skip over the single line-feed character
                ++_currentPosition;
                _newLineLocations.Add(_currentPosition);
            }
        }

        // this method is public because it's used from the authoring code
        public void SkipMultilineComment()
        {
            for (; ; )
            {
                char c = GetChar(_currentPosition);
                while ('*' == c)
                {
                    c = GetChar(++_currentPosition);
                    if ('/' == c)
                    {
                        // get past the trailing slash
                        _currentPosition++;

                        // no longer in a multiline comment; fix up the end of the current token.
                        _tokenEndIndex = _currentPosition;
                        _state.UnterminatedComment = false;
                        return;
                    }

                    if ('\0' == c)
                    {
                        break;
                    }
                    
                    if (IsLineTerminator(c, 1))
                    {
                        c = GetChar(++_currentPosition);
                        _newLineLocations.Add(_currentPosition);
                    }
                }

                if ('\0' == c && IsEndOfFile)
                {
                    break;
                }

                if (IsLineTerminator(c, 1))
                {
                    _newLineLocations.Add(_currentPosition + 1);
                }

                ++_currentPosition;
            }

            // if we are here we got EOF
            _tokenEndIndex = _currentPosition;
            _state.UnterminatedComment = true;
            //throw new ScannerException(JSError.NoCommentEnd);
        }

        private void SkipBlanks()
        {
            char c = GetChar(_currentPosition);
            while (JSScanner.IsBlankSpace(c))
            {
                c = GetChar(++_currentPosition);
            }
        }

        private static bool IsBlankSpace(char c)
        {
            switch (c)
            {
                case '\u0009':
                case '\u000b':
                case '\u000c':
                case '\u0020':
                case '\u00a0':
                case '\ufeff': // BOM - byte order mark
                    return true;

                default:
                    return (c < 128) ? false : char.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator;
            }
        }

        private bool IsLineTerminator(char c, int increment)
        {
            switch (c)
            {
                case '\u000d':
                    // treat 0x0D0x0A as a single character
                    if (0x0A == GetChar(_currentPosition + increment))
                    {
                        _currentPosition++;
                    }

                    return true;

                case '\u000a':
                    return true;

                case '\u2028':
                    return true;

                case '\u2029':
                    return true;

                default:
                    return false;
            }
        }

        private bool IsEndLineOrEOF(char c, int increment)
        {
            return IsLineTerminator(c, increment) || '\0' == c && IsEndOfFile;
        }

        private bool IsAtEndOfLine
        {
            get
            {
                return IsEndLineOrEOF(GetChar(_currentPosition), 0);
            }
        }

        private static int GetHexValue(char hex)
        {
            int hexValue;
            if ('0' <= hex && hex <= '9')
            {
                hexValue = hex - '0';
            }
            else if ('a' <= hex && hex <= 'f')
            {
                hexValue = hex - 'a' + 10;
            }
            else
            {
                hexValue = hex - 'A' + 10;
            }

            return hexValue;
        }

        // assumes all unicode characters in the string -- NO escape sequences
        public static bool IsValidIdentifier(string name)
        {
            bool isValid = false;
            if (!string.IsNullOrEmpty(name))
            {
                if (IsValidIdentifierStart(name[0]))
                {
                    // loop through all the rest
                    for (int ndx = 1; ndx < name.Length; ++ndx)
                    {
                        char ch = name[ndx];
                        if (!IsValidIdentifierPart(ch))
                        {
                            // fail!
                            return false;
                        }
                    }

                    // if we get here, everything is okay
                    isValid = true;
                }
            }

            return isValid;
        }

        // assumes all unicode characters in the string -- NO escape sequences
        public static bool IsSafeIdentifier(string name)
        {
            bool isValid = false;
            if (!string.IsNullOrEmpty(name))
            {
                if (IsSafeIdentifierStart(name[0]))
                {
                    // loop through all the rest
                    for (int ndx = 1; ndx < name.Length; ++ndx)
                    {
                        char ch = name[ndx];
                        if (!IsSafeIdentifierPart(ch))
                        {
                            // fail!
                            return false;
                        }
                    }

                    // if we get here, everything is okay
                    isValid = true;
                }
            }

            return isValid;
        }

        // unescaped unicode characters
        public static bool IsValidIdentifierStart(char letter)
        {
            if (('a' <= letter && letter <= 'z') || ('A' <= letter && letter <= 'Z') || letter == '_' || letter == '$')
            {
                // good
                return true;
            }

            if (letter >= 128)
            {
                // check the unicode category
                UnicodeCategory cat = char.GetUnicodeCategory(letter);
                switch (cat)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.LetterNumber:
                        // okay
                        return true;
                }
            }

            return false;
        }

        // unescaped unicode characters.
        // the same as the "IsValid" method, except various browsers have problems with some
        // of the Unicode characters in the ModifierLetter, OtherLetter, and LetterNumber categories.
        public static bool IsSafeIdentifierStart(char letter)
        {
            if (('a' <= letter && letter <= 'z') || ('A' <= letter && letter <= 'Z') || letter == '_' || letter == '$')
            {
                // good
                return true;
            }

            return false;
        }

        public static bool IsValidIdentifierPart(string text)
        {
            var isValid = false;

            // pull the first character from the string, which may be an escape character
            if (!string.IsNullOrEmpty(text))
            {
                char ch = text[0];
                if (ch == '\\')
                {
                    PeekUnicodeEscape(text, ref ch);
                }

                isValid = IsValidIdentifierPart(ch);
            }

            return isValid;
        }

        // unescaped unicode characters
        public static bool IsValidIdentifierPart(char letter)
        {
            // look for valid ranges
            // 0x200c = ZWNJ - zero-width non-joiner
            // 0x200d = ZWJ - zero-width joiner
            if (('a' <= letter && letter <= 'z')
                || ('A' <= letter && letter <= 'Z')
                || ('0' <= letter && letter <= '9')
                || letter == '_'
                || letter == '$'
                || letter == 0x200c    
                || letter == 0x200d)   
            {
                return true;
            }

            if (letter >= 128)
            {
                UnicodeCategory unicodeCategory = Char.GetUnicodeCategory(letter);
                switch (unicodeCategory)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.LetterNumber:
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.ConnectorPunctuation:
                        return true;
                }
            }

            return false;
        }

        // unescaped unicode characters.
        // the same as the "IsValid" method, except various browsers have problems with some
        // of the Unicode characters in the ModifierLetter, OtherLetter, LetterNumber,
        // NonSpacingMark, SpacingCombiningMark, DecimalDigitNumber, and ConnectorPunctuation categories.
        public static bool IsSafeIdentifierPart(char letter)
        {
            // look for valid ranges
            if (('a' <= letter && letter <= 'z')
                || ('A' <= letter && letter <= 'Z')
                || ('0' <= letter && letter <= '9')
                || letter == '_'
                || letter == '$')
            {
                return true;
            }

            return false;
        }

        // pulling unescaped characters off the input stream
        internal bool IsIdentifierPartChar(char c)
        {
            return IsIdentifierStartChar(ref c) || IsValidIdentifierPart(c);
        }

        private static void PeekUnicodeEscape(string str, ref char ch)
        {
            // if the length isn't at least six characters starting with a backslash, do nothing
            if (!string.IsNullOrEmpty(str) && ch == '\\' && str.Length >= 6)
            {
                if (str[1] == 'u' 
                    && IsHexDigit(str[2])
                    && IsHexDigit(str[3])
                    && IsHexDigit(str[4])
                    && IsHexDigit(str[5]))
                {
                    ch = (char)(GetHexValue(str[2]) << 12 | GetHexValue(str[3]) << 8 | GetHexValue(str[4]) << 4 | GetHexValue(str[5]));
                }
            }
        }

        private bool PeekUnicodeEscape(int index, ref char ch)
        {
            bool isEscapeChar = false;

            // call this only if we had just read a backslash and the pointer is
            // now at the next character, presumably the 'u'
            if ('u' == GetChar(index))
            {
                char h1 = GetChar(index + 1);
                if (IsHexDigit(h1))
                {
                    char h2 = GetChar(index + 2);
                    if (IsHexDigit(h2))
                    {
                        char h3 = GetChar(index + 3);
                        if (IsHexDigit(h3))
                        {
                            char h4 = GetChar(index + 4);
                            if (IsHexDigit(h4))
                            {
                                // this IS a unicode escape, so compute the new character value
                                // and adjust the current position
                                isEscapeChar = true;
                                ch = (char)(GetHexValue(h1) << 12 | GetHexValue(h2) << 8 | GetHexValue(h3) << 4 | GetHexValue(h4));
                            }
                        }
                    }
                }
            }

            return isEscapeChar;
        }

        // pulling unescaped characters off the input stream
        internal bool IsIdentifierStartChar(ref char c)
        {
            bool isEscapeChar = false;
            if ('\\' == c)
            {
                if ('u' == GetChar(_currentPosition + 1))
                {
                    char h1 = GetChar(_currentPosition + 2);
                    if (IsHexDigit(h1))
                    {
                        char h2 = GetChar(_currentPosition + 3);
                        if (IsHexDigit(h2))
                        {
                            char h3 = GetChar(_currentPosition + 4);
                            if (IsHexDigit(h3))
                            {
                                char h4 = GetChar(_currentPosition + 5);
                                if (IsHexDigit(h4))
                                {
                                    isEscapeChar = true;
                                    c = (char)(GetHexValue(h1) << 12 | GetHexValue(h2) << 8 | GetHexValue(h3) << 4 | GetHexValue(h4));
                                }
                            }
                        }
                    }
                }
            }

            if (!IsValidIdentifierStart(c))
            {
                return false;
            }

            // if we get here, we're a good character!
            if (isEscapeChar)
            {
                int startPosition = (_lastPosOnBuilder > 0) ? _lastPosOnBuilder : _tokenStartIndex;
                if (_currentPosition - startPosition > 0)
                {
                    _identifier.Append(_source.Substring(startPosition, _currentPosition - startPosition));
                }

                _identifier.Append(c);
                _currentPosition += 5;
                _lastPosOnBuilder = _currentPosition + 1;
            }

            return true;
        }

        internal static bool IsDigit(char c)
        {
            return '0' <= c && c <= '9';
        }

        internal static bool IsHexDigit(char c)
        {
            return ('0' <= c && c <= '9') || ('A' <= c && c <= 'F') || ('a' <= c && c <= 'f');
        }

        internal static bool IsAsciiLetter(char c)
        {
            return ('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z');
        }

        private Func<string,string,bool> CheckForOperator(SortedDictionary<string, Func<string,string,bool>> operators)
        {
            // we need to make SURE we are checking the longer strings before we check the
            // shorter strings, because if the source is === and we check for ==, we'll pop positive
            // for it and miss that last =. 
            foreach (var entry in operators)
            {
                if (CheckCaseInsensitiveSubstring(entry.Key))
                {
                    // found it! return the comparison function for this text
                    return entry.Value;
                }
            }

            // if we got here, we didn't find anything we were looking for
            return null;
        }

        private void HandleError(JSError error)
        {
            _tokenEndIndex = _currentPosition;
            var errorEx = new JScriptException(
                error, 
                MakeContext(JSToken.None)
            );

            _errorSink.OnCompilerError(
                errorEx
            );
        }

        /// <summary>
        /// Given an assignment operator (=, +=, -=, *=, /=, %=, &amp;=, |=, ^=, &lt;&lt;=, &gt;&gt;=, &gt;&gt;&gt;=), strip
        /// the assignment to return (+, -, *, /, %, &amp;, |, ^, &lt;&lt;, &gt;&gt;, &gt;&gt;&gt;). For all other operators,
        /// include the normal assign (=), just return the same operator token.
        /// This only works if the two groups of tokens are actually defined in those orders!!! 
        /// </summary>
        /// <param name="assignOp"></param>
        /// <returns></returns>
        internal static JSToken StripAssignment(JSToken assignOp)
        {
            // gotta be an assignment operator
            if (IsAssignmentOperator(assignOp))
            {
                // get the delta from assign (=), which is the first assignment operator
                int delta = assignOp - JSToken.Assign;

                // assign (=) will be zero -- we don't want to modify that one, so if
                // the delta is GREATER than zero...
                if (delta > 0)
                {
                    // add it to the plus token, less one since the delta for +=
                    // will be one.
                    assignOp = JSToken.Plus + delta - 1;
                }
            }

            return assignOp;
        }

        internal static bool IsAssignmentOperator(JSToken token)
        {
            return JSToken.Assign <= token && token <= JSToken.LastAssign;
        }

        internal static bool IsRightAssociativeOperator(JSToken token)
        {
            return JSToken.Assign <= token && token <= JSToken.ConditionalIf;
        }

        // This function return whether an operator is processable in ParseExpression.
        // Comma is out of this list and so are the unary ops
        internal static bool IsProcessableOperator(JSToken token)
        {
            return JSToken.FirstBinaryOperator <= token && token <= JSToken.ConditionalIf;
        }

        internal static OperatorPrecedence GetOperatorPrecedence(TokenWithSpan op)
        {
            return op == null || op.Token == JSToken.None ? OperatorPrecedence.None : JSScanner.s_OperatorsPrec[op.Token - JSToken.FirstBinaryOperator];
        }

        //internal static OperatorPrecedence GetOperatorPrecedence(JSToken op)
        //{
        //    return JSScanner.s_OperatorsPrec[op - JSToken.FirstBinaryOperator];
        //}

        private static OperatorPrecedence[] InitOperatorsPrec()
        {
            OperatorPrecedence[] operatorsPrec = new OperatorPrecedence[JSToken.LastOperator - JSToken.FirstBinaryOperator + 1];

            operatorsPrec[JSToken.Plus - JSToken.FirstBinaryOperator] = OperatorPrecedence.Additive;
            operatorsPrec[JSToken.Minus - JSToken.FirstBinaryOperator] = OperatorPrecedence.Additive;

            operatorsPrec[JSToken.LogicalOr - JSToken.FirstBinaryOperator] = OperatorPrecedence.LogicalOr;
            operatorsPrec[JSToken.LogicalAnd - JSToken.FirstBinaryOperator] = OperatorPrecedence.LogicalAnd;
            operatorsPrec[JSToken.BitwiseOr - JSToken.FirstBinaryOperator] = OperatorPrecedence.BitwiseOr;
            operatorsPrec[JSToken.BitwiseXor - JSToken.FirstBinaryOperator] = OperatorPrecedence.BitwiseXor;
            operatorsPrec[JSToken.BitwiseAnd - JSToken.FirstBinaryOperator] = OperatorPrecedence.BitwiseAnd;

            operatorsPrec[JSToken.Equal - JSToken.FirstBinaryOperator] = OperatorPrecedence.Equality;
            operatorsPrec[JSToken.NotEqual - JSToken.FirstBinaryOperator] = OperatorPrecedence.Equality;
            operatorsPrec[JSToken.StrictEqual - JSToken.FirstBinaryOperator] = OperatorPrecedence.Equality;
            operatorsPrec[JSToken.StrictNotEqual - JSToken.FirstBinaryOperator] = OperatorPrecedence.Equality;

            operatorsPrec[JSToken.InstanceOf - JSToken.FirstBinaryOperator] = OperatorPrecedence.Relational;
            operatorsPrec[JSToken.In - JSToken.FirstBinaryOperator] = OperatorPrecedence.Relational;
            operatorsPrec[JSToken.GreaterThan - JSToken.FirstBinaryOperator] = OperatorPrecedence.Relational;
            operatorsPrec[JSToken.LessThan - JSToken.FirstBinaryOperator] = OperatorPrecedence.Relational;
            operatorsPrec[JSToken.LessThanEqual - JSToken.FirstBinaryOperator] = OperatorPrecedence.Relational;
            operatorsPrec[JSToken.GreaterThanEqual - JSToken.FirstBinaryOperator] = OperatorPrecedence.Relational;

            operatorsPrec[JSToken.LeftShift - JSToken.FirstBinaryOperator] = OperatorPrecedence.Shift;
            operatorsPrec[JSToken.RightShift - JSToken.FirstBinaryOperator] = OperatorPrecedence.Shift;
            operatorsPrec[JSToken.UnsignedRightShift - JSToken.FirstBinaryOperator] = OperatorPrecedence.Shift;

            operatorsPrec[JSToken.Multiply - JSToken.FirstBinaryOperator] = OperatorPrecedence.Multiplicative;
            operatorsPrec[JSToken.Divide - JSToken.FirstBinaryOperator] = OperatorPrecedence.Multiplicative;
            operatorsPrec[JSToken.Modulo - JSToken.FirstBinaryOperator] = OperatorPrecedence.Multiplicative;

            operatorsPrec[JSToken.Assign - JSToken.FirstBinaryOperator] = OperatorPrecedence.Assignment;
            operatorsPrec[JSToken.PlusAssign - JSToken.FirstBinaryOperator] = OperatorPrecedence.Assignment;
            operatorsPrec[JSToken.MinusAssign - JSToken.FirstBinaryOperator] = OperatorPrecedence.Assignment;
            operatorsPrec[JSToken.MultiplyAssign - JSToken.FirstBinaryOperator] = OperatorPrecedence.Assignment;
            operatorsPrec[JSToken.DivideAssign - JSToken.FirstBinaryOperator] = OperatorPrecedence.Assignment;
            operatorsPrec[JSToken.BitwiseAndAssign - JSToken.FirstBinaryOperator] = OperatorPrecedence.Assignment;
            operatorsPrec[JSToken.BitwiseOrAssign - JSToken.FirstBinaryOperator] = OperatorPrecedence.Assignment;
            operatorsPrec[JSToken.BitwiseXorAssign - JSToken.FirstBinaryOperator] = OperatorPrecedence.Assignment;
            operatorsPrec[JSToken.ModuloAssign - JSToken.FirstBinaryOperator] = OperatorPrecedence.Assignment;
            operatorsPrec[JSToken.LeftShiftAssign - JSToken.FirstBinaryOperator] = OperatorPrecedence.Assignment;
            operatorsPrec[JSToken.RightShiftAssign - JSToken.FirstBinaryOperator] = OperatorPrecedence.Assignment;
            operatorsPrec[JSToken.UnsignedRightShiftAssign - JSToken.FirstBinaryOperator] = OperatorPrecedence.Assignment;

            operatorsPrec[JSToken.ConditionalIf - JSToken.FirstBinaryOperator] = OperatorPrecedence.Conditional;
            operatorsPrec[JSToken.Colon - JSToken.FirstBinaryOperator] = OperatorPrecedence.Conditional;

            operatorsPrec[JSToken.Comma - JSToken.FirstBinaryOperator] = OperatorPrecedence.Comma;

            return operatorsPrec;
        }

        /// <summary>
        /// Mutable parser state.  This is the state which changes
        /// as we parse.  The current state can be captured to enable
        /// re-starting of tokenization if the previous lines have
        /// not changed.
        /// </summary>
        struct State : IEquatable<State> {
            // TODO: If this stays as a single bool we should just
            // re-use pre-boxed versions so the state's super cheap.
            public bool UnterminatedComment;

            public override int GetHashCode() {
                return UnterminatedComment ? 1 : 0;
            }

            public override bool Equals(object obj) {
                if (obj is State) {
                    return Equals((State)obj);
                }
                return false;
            }

            public bool Equals(State other) {
                return other.UnterminatedComment == UnterminatedComment;
            }
        }
    }

    public class ErrorEventArgs : EventArgs {
        public readonly JScriptException Error;

        public ErrorEventArgs(JScriptException errorEx) {
            Error = errorEx;
        }
    }
}
