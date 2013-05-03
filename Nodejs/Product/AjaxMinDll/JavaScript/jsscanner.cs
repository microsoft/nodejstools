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
using System.Globalization;
using System.Text;

namespace Microsoft.Ajax.Utilities
{
    public sealed class JSScanner
    {
        #region static fields

        // keyword table
        private static readonly JSKeyword[] s_Keywords = JSKeyword.InitKeywords();

        private static readonly OperatorPrecedence[] s_OperatorsPrec = InitOperatorsPrec();

        #endregion

        #region private fields

        private string m_strSourceCode;

        private int m_endPos;

        private StringBuilder m_identifier;
        private bool m_literalIssues;

        // a list of strings that we can add new ones to or clear
        // depending on comments we may find in the source
        internal ICollection<string> DebugLookupCollection { get; set; }

        // for pre-processor
        private Dictionary<string, string> m_defines;

        private int m_startLinePosition;
        private int m_currentPosition;
        private int m_currentLine;
        private int m_lastPosOnBuilder;
        private int m_ifDirectiveLevel;
        private int m_conditionalCompilationIfLevel;
        private bool m_conditionalCompilationOn;
        private bool m_inConditionalComment;
        private bool m_inSingleLineComment;
        private bool m_inMultipleLineComment;
        private string m_decodedString;
        private Context m_currentToken;

        #endregion

        #region public properties

        public bool UsePreprocessorDefines { get; set; }

        public bool IgnoreConditionalCompilation { get; set; }

        public bool AllowEmbeddedAspNetBlocks { get; set; }

        public bool LiteralHasIssues { get { return m_literalIssues; } }

        public string StringLiteralValue { get { return m_decodedString; } }

        public int CurrentLine { get { return m_currentLine; } }

        public int StartLinePosition { get { return m_startLinePosition; } }

        public bool IsEndOfFile { get { return m_currentPosition >= m_endPos; } }

        public bool StripDebugCommentBlocks { get; set; }

        internal string Identifier
        {
            get
            {
                return m_identifier.Length > 0
                    ? m_identifier.ToString()
                    : m_currentToken.Code;
            }
        }

        #endregion

        #region public events

        public event EventHandler<GlobalDefineEventArgs> GlobalDefine;

        public event EventHandler<NewModuleEventArgs> NewModule;

        #endregion

        #region constructors

        public JSScanner(Context sourceContext)
        {
            if (sourceContext == null)
            {
                throw new ArgumentNullException("sourceContext");
            }

            m_startLinePosition = sourceContext.StartLinePosition;
            m_currentPosition = sourceContext.StartLinePosition;
            m_currentLine = sourceContext.StartLineNumber;
            m_currentToken = sourceContext.Clone();

            // just hold on to these values
            m_strSourceCode = sourceContext.Document.Source;
            m_endPos = sourceContext.EndPosition;

            // by default we want to use preprocessor defines
            // and strip debug comment blocks
            UsePreprocessorDefines = true;
            StripDebugCommentBlocks = true;

            // create a string builder that we'll keep reusing as we
            // scan identifiers. We'll build the unescaped name into it
            m_identifier = new StringBuilder(128);
        }

        private JSScanner(IDictionary<string, string> defines)
        {
            // copy the collection, but don't share it. We don't want
            // defines we find here to populate the collection of the original.
            SetPreprocessorDefines(defines);

            // stuff we don't want to copy
            m_decodedString = null;
            m_identifier = new StringBuilder(128);

            // create a new set so anything we find doesn't affect the original.
            DebugLookupCollection = new HashSet<string>();
        }

        public JSScanner Clone()
        {
            return new JSScanner(this.m_defines)
            {
                AllowEmbeddedAspNetBlocks = this.AllowEmbeddedAspNetBlocks,
                IgnoreConditionalCompilation = this.IgnoreConditionalCompilation,
                m_conditionalCompilationIfLevel = this.m_conditionalCompilationIfLevel,
                m_conditionalCompilationOn = this.m_conditionalCompilationOn,
                m_currentLine = this.m_currentLine,
                m_currentPosition = this.m_currentPosition,
                m_currentToken = this.m_currentToken.Clone(),
                m_endPos = this.m_endPos,
                m_ifDirectiveLevel = this.m_ifDirectiveLevel,
                m_inConditionalComment = this.m_inConditionalComment,
                m_inMultipleLineComment = this.m_inMultipleLineComment,
                m_inSingleLineComment = this.m_inSingleLineComment,
                m_lastPosOnBuilder = this.m_lastPosOnBuilder,
                m_startLinePosition = this.m_startLinePosition,
                m_strSourceCode = this.m_strSourceCode,
                UsePreprocessorDefines = this.UsePreprocessorDefines,
                StripDebugCommentBlocks = this.StripDebugCommentBlocks,
            };
        }

        #endregion

        /// <summary>
        /// main method for the scanner; scans the next token from the input stream.
        /// </summary>
        /// <param name="scanForRegularExpressionLiterals">whether to try scanning a regexp when encountering a /</param>
        /// <returns>next token from the input</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "big case statement")]
        public Context ScanNextToken(bool scanForRegularExpressionLiterals)
        {
            var token = JSToken.None;

            m_currentToken.StartPosition = m_currentPosition;
            m_currentToken.StartLineNumber = m_currentLine;
            m_currentToken.StartLinePosition = m_startLinePosition;

            m_identifier.Length = 0;

            // our case switch should be pretty efficient -- it's 9-13 and 32-126. Thsose are the most common characters 
            // we will find in the code for the start of tokens.
            char c = GetChar(m_currentPosition++);
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
                    while (JSScanner.IsBlankSpace(GetChar(m_currentPosition)))
                    {
                        ++m_currentPosition;
                    }

                    break;

                case '!':
                    token = JSToken.LogicalNot;
                    if ('=' == GetChar(m_currentPosition))
                    {
                        m_currentPosition++;
                        token = JSToken.NotEqual;
                        if ('=' == GetChar(m_currentPosition))
                        {
                            m_currentPosition++;
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
                    if ('=' == GetChar(m_currentPosition))
                    {
                        m_currentPosition++;
                        token = JSToken.ModuloAssign;
                    }

                    break;

                case '&':
                    token = JSToken.BitwiseAnd;
                    c = GetChar(m_currentPosition);
                    if ('&' == c)
                    {
                        m_currentPosition++;
                        token = JSToken.LogicalAnd;
                    }
                    else if ('=' == c)
                    {
                        m_currentPosition++;
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
                    if ('=' == GetChar(m_currentPosition))
                    {
                        m_currentPosition++;
                        token = JSToken.MultiplyAssign;
                    }

                    break;

                case '+':
                    token = JSToken.Plus;
                    c = GetChar(m_currentPosition);
                    if ('+' == c)
                    {
                        m_currentPosition++;
                        token = JSToken.Increment;
                    }
                    else if ('=' == c)
                    {
                        m_currentPosition++;
                        token = JSToken.PlusAssign;
                    }

                    break;

                case ',':
                    token = JSToken.Comma;
                    break;

                case '-':
                    token = JSToken.Minus;
                    c = GetChar(m_currentPosition);
                    if ('-' == c)
                    {
                        m_currentPosition++;
                        token = JSToken.Decrement;
                    }
                    else if ('=' == c)
                    {
                        m_currentPosition++;
                        token = JSToken.MinusAssign;
                    }

                    break;

                case '.':
                    token = JSToken.AccessField;
                    c = GetChar(m_currentPosition);
                    if (JSScanner.IsDigit(c))
                    {
                        token = ScanNumber('.');
                    }

                    break;

                case '/':
                    token = JSToken.Divide;
                    c = GetChar(m_currentPosition);
                    switch (c)
                    {
                        case '/':
                            token = JSToken.SingleLineComment;
                            m_inSingleLineComment = true;
                            c = GetChar(++m_currentPosition);

                            // see if there is a THIRD slash character
                            if (c == '/')
                            {
                                // advance past the slash and see if we have one of our special preprocessing directives
                                ++m_currentPosition;
                                if (GetChar(m_currentPosition) == '#')
                                {
                                    // scan preprocessing directives
                                    token = JSToken.PreprocessorDirective;

                                    if (!ScanPreprocessingDirective())
                                    {
                                        // if it returns false, we don't want to skip the rest of
                                        // the comment line; just exit
                                        break;
                                    }
                                }
                            }
                            else if (c == '@' && !IgnoreConditionalCompilation)
                            {
                                // we got //@
                                // if we have not turned on conditional-compilation yet, then check to see if that's
                                // what we're trying to do now.
                                // we are currently on the @ -- start peeking from there
                                if (m_conditionalCompilationOn
                                    || CheckSubstring(m_currentPosition + 1, "cc_on"))
                                {
                                    // if the NEXT character is not an identifier character, then we need to skip
                                    // the @ character -- otherwise leave it there
                                    if (!IsValidIdentifierStart(GetChar(m_currentPosition + 1)))
                                    {
                                        ++m_currentPosition;
                                    }

                                    // we are now in a conditional comment
                                    m_inConditionalComment = true;
                                    token = JSToken.ConditionalCommentStart;
                                    break;
                                }
                            }

                            SkipSingleLineComment();

                            // if we're still in a multiple-line comment, then we must've been in
                            // a multi-line CONDITIONAL comment, in which case this normal one-line comment
                            // won't turn off conditional comments just because we hit the end of line.
                            if (!m_inMultipleLineComment && m_inConditionalComment)
                            {
                                m_inConditionalComment = false;
                                token = JSToken.ConditionalCommentEnd;
                            }

                            break;

                        case '*':
                            m_inMultipleLineComment = true;
                            if (GetChar(++m_currentPosition) == '@' && !IgnoreConditionalCompilation)
                            {
                                // we have /*@
                                // if we have not turned on conditional-compilation yet, then let's peek to see if the next
                                // few characters are cc_on -- if so, turn it on.
                                if (!m_conditionalCompilationOn)
                                {
                                    // we are currently on the @ -- start peeking from there
                                    if (!CheckSubstring(m_currentPosition + 1, "cc_on"))
                                    {
                                        // we aren't turning on conditional comments. We need to ignore this comment
                                        // as just another multi-line comment
                                        SkipMultilineComment();
                                        token = JSToken.MultipleLineComment;
                                        break;
                                    }
                                }

                                // if the NEXT character is not an identifier character, then we need to skip
                                // the @ character -- otherwise leave it there
                                if (!IsValidIdentifierStart(GetChar(m_currentPosition + 1)))
                                {
                                    ++m_currentPosition;
                                }

                                // we are now in a conditional comment
                                m_inConditionalComment = true;
                                token = JSToken.ConditionalCommentStart;
                                break;
                            }

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
                                    m_currentPosition++;
                                    token = JSToken.DivideAssign;
                                }
                            }
                            else if (c == '=')
                            {
                                m_currentPosition++;
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
                    if (AllowEmbeddedAspNetBlocks &&
                        '%' == GetChar(m_currentPosition))
                    {
                        token = ScanAspNetBlock();
                    }
                    else
                    {
                        token = JSToken.LessThan;
                        if ('<' == GetChar(m_currentPosition))
                        {
                            m_currentPosition++;
                            token = JSToken.LeftShift;
                        }

                        if ('=' == GetChar(m_currentPosition))
                        {
                            m_currentPosition++;
                            if (token == JSToken.LessThan)
                            {
                                token = JSToken.LessThanEqual;
                            }
                            else
                            {
                                token = JSToken.LeftShiftAssign;
                            }
                        }
                    }

                    break;

                case '=':
                    token = JSToken.Assign;
                    if ('=' == GetChar(m_currentPosition))
                    {
                        m_currentPosition++;
                        token = JSToken.Equal;
                        if ('=' == GetChar(m_currentPosition))
                        {
                            m_currentPosition++;
                            token = JSToken.StrictEqual;
                        }
                    }

                    break;

                case '>':
                    token = JSToken.GreaterThan;
                    if ('>' == GetChar(m_currentPosition))
                    {
                        m_currentPosition++;
                        token = JSToken.RightShift;
                        if ('>' == GetChar(m_currentPosition))
                        {
                            m_currentPosition++;
                            token = JSToken.UnsignedRightShift;
                        }
                    }

                    if ('=' == GetChar(m_currentPosition))
                    {
                        m_currentPosition++;
                        token = token == JSToken.GreaterThan ? JSToken.GreaterThanEqual
                            : token == JSToken.RightShift ? JSToken.RightShiftAssign
                            : token == JSToken.UnsignedRightShift ? JSToken.UnsignedRightShiftAssign
                            : JSToken.Error;
                    }

                    break;

                case '?':
                    token = JSToken.ConditionalIf;
                    break;

                case '@':
                    if (IgnoreConditionalCompilation)
                    {
                        // if the switch to ignore conditional compilation is on, then we don't know
                        // anything about conditional-compilation statements, and the @-sign character
                        // is illegal at this spot.
                        token = IllegalCharacter();
                        break;
                    }

                    // see if the @-sign is immediately followed by an identifier. If it is,
                    // we'll see which one so we can tell if it's a conditional-compilation statement
                    // need to make sure the context INCLUDES the @ sign
                    int startPosition = m_currentPosition;
                    m_currentToken.StartPosition = startPosition - 1;
                    m_currentToken.StartLineNumber = m_currentLine;
                    m_currentToken.StartLinePosition = m_startLinePosition;
                    ScanIdentifier();
                    switch (m_currentPosition - startPosition)
                    {
                        case 0:
                            // look for '@*/'.
                            if ('*' == GetChar(m_currentPosition) && '/' == GetChar(++m_currentPosition))
                            {
                                m_currentPosition++;
                                m_inMultipleLineComment = false;
                                m_inConditionalComment = false;
                                token = JSToken.ConditionalCommentEnd;
                                break;
                            }

                            // otherwise we just have a @ sitting by itself!
                            // throw an error and loop back to the next token.
                            token = IllegalCharacter();
                            break;

                        case 2:
                            if (CheckSubstring(startPosition, "if"))
                            {
                                token = JSToken.ConditionalCompilationIf;

                                // increment the if-level
                                ++m_conditionalCompilationIfLevel;

                                // if we're not in a conditional comment and we haven't explicitly
                                // turned on conditional compilation when we encounter
                                // a @if statement, then we can implicitly turn it on.
                                if (!m_inConditionalComment && !m_conditionalCompilationOn)
                                {
                                    m_conditionalCompilationOn = true;
                                }

                                break;
                            }

                            // the string isn't a known preprocessor command, so 
                            // fall into the default processing to handle it as a variable name
                            goto default;

                        case 3:
                            if (CheckSubstring(startPosition, "set"))
                            {
                                token = JSToken.ConditionalCompilationSet;

                                // if we're not in a conditional comment and we haven't explicitly
                                // turned on conditional compilation when we encounter
                                // a @set statement, then we can implicitly turn it on.
                                if (!m_inConditionalComment && !m_conditionalCompilationOn)
                                {
                                    m_conditionalCompilationOn = true;
                                }

                                break;
                            }

                            if (CheckSubstring(startPosition, "end"))
                            {
                                token = JSToken.ConditionalCompilationEnd;
                                if (m_conditionalCompilationIfLevel > 0)
                                {
                                    // down one more @if level
                                    m_conditionalCompilationIfLevel--;
                                }
                                else
                                {
                                    // not corresponding @if -- invalid @end statement
                                    HandleError(JSError.CCInvalidEnd);
                                }

                                break;
                            }

                            // the string isn't a known preprocessor command, so 
                            // fall into the default processing to handle it as a variable name
                            goto default;

                        case 4:
                            if (CheckSubstring(startPosition, "else"))
                            {
                                token = JSToken.ConditionalCompilationElse;

                                // if we don't have a corresponding @if statement, then throw and error
                                // (but keep processing)
                                if (m_conditionalCompilationIfLevel <= 0)
                                {
                                    HandleError(JSError.CCInvalidElse);
                                }

                                break;
                            }

                            if (CheckSubstring(startPosition, "elif"))
                            {
                                token = JSToken.ConditionalCompilationElseIf;

                                // if we don't have a corresponding @if statement, then throw and error
                                // (but keep processing)
                                if (m_conditionalCompilationIfLevel <= 0)
                                {
                                    HandleError(JSError.CCInvalidElseIf);
                                }

                                break;
                            }

                            // the string isn't a known preprocessor command, so 
                            // fall into the default processing to handle it as a variable name
                            goto default;

                        case 5:
                            if (CheckSubstring(startPosition, "cc_on"))
                            {
                                // turn it on and return the @cc_on token
                                m_conditionalCompilationOn = true;
                                token = JSToken.ConditionalCompilationOn;
                                break;
                            }

                            // the string isn't a known preprocessor command, so 
                            // fall into the default processing to handle it as a variable name
                            goto default;

                        default:
                            // we have @[id], where [id] is a valid identifier.
                            // if we haven't explicitly turned on conditional compilation,
                            // we'll keep processing, but we need to fire an error to indicate
                            // that the code should turn it on first.
                            if (!m_conditionalCompilationOn)
                            {
                                HandleError(JSError.CCOff);
                            }

                            token = JSToken.ConditionalCompilationVariable;
                            break;
                    }

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
                    if (PeekUnicodeEscape(m_currentPosition, ref c))
                    {
                        // advance past the escape characters
                        m_currentPosition += 5;

                        // valid unicode escape sequence
                        if (IsValidIdentifierStart(c))
                        {
                            // use the unescaped character as the first character of the
                            // decoded identifier, and current character is now the last position
                            // on the builder
                            m_identifier.Append(c);
                            m_lastPosOnBuilder = m_currentPosition;

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
                        if (IsValidIdentifierStart(GetChar(m_currentPosition)))
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
                    if ('=' == GetChar(m_currentPosition))
                    {
                        m_currentPosition++;
                        token = JSToken.BitwiseXorAssign;
                    }

                    break;

                case '#':
                case '`':
                    token = IllegalCharacter();
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
                    c = GetChar(m_currentPosition);
                    if ('|' == c)
                    {
                        m_currentPosition++;
                        token = JSToken.LogicalOr;
                    }
                    else if ('=' == c)
                    {
                        m_currentPosition++;
                        token = JSToken.BitwiseOrAssign;
                    }

                    break;

                case '}':
                    token = JSToken.RightCurly;
                    break;

                case '~':
                    token = JSToken.BitwiseNot;
                    break;

                default:
                    if (c == '\0')
                    {
                        if (IsEndOfFile)
                        {
                            token = JSToken.EndOfFile;
                            m_currentPosition--;
                            if (m_conditionalCompilationIfLevel > 0)
                            {
                                m_currentToken.EndLineNumber = m_currentLine;
                                m_currentToken.EndLinePosition = m_startLinePosition;
                                m_currentToken.EndPosition = m_currentPosition;
                                HandleError(JSError.NoCCEnd);
                            }
                        }
                        else
                        {
                            token = IllegalCharacter();
                        }
                    }
                    else if (c == '\u2028' || c == '\u2029')
                    {
                        // more line terminator
                        token = ScanLineTerminator(c);
                    }
                    else if (IsValidIdentifierStart(c))
                    {
                        token = JSToken.Identifier;
                        ScanIdentifier();
                    }
                    else if (IsBlankSpace(c))
                    {
                        // we are asking for raw tokens, and this is the start of a stretch of whitespace.
                        // advance to the end of the whitespace, and return that as the token
                        while (JSScanner.IsBlankSpace(GetChar(m_currentPosition)))
                        {
                            ++m_currentPosition;
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
            m_currentToken.EndLineNumber = m_currentLine;
            m_currentToken.EndLinePosition = m_startLinePosition;
            m_currentToken.EndPosition = m_currentPosition;

            // this is now the current token
            m_currentToken.Token = token;
            return m_currentToken;
        }

        private JSToken ScanLineTerminator(char ch)
        {
            // line terminator
            var token = JSToken.EndOfLine;
            if (m_inConditionalComment && m_inSingleLineComment)
            {
                // if we are in a single-line conditional comment, then we want
                // to return the end of comment token WITHOUT moving past the end of line 
                // characters
                token = JSToken.ConditionalCommentEnd;
                m_inConditionalComment = m_inSingleLineComment = false;
            }
            else
            {
                if (ch == '\r')
                {
                    // \r\n is a valid SINGLE line-terminator. So if the \r is
                    // followed by a \n, we only want to process a single line terminator.
                    if (GetChar(m_currentPosition) == '\n')
                    {
                        m_currentPosition++;
                    }
                }

                m_currentLine++;
                m_startLinePosition = m_currentPosition;

                // keep multiple line terminators together in a single token.
                // so get the current character after this last line terminator
                // and then keep looping until we hit something that isn't one.
                while ((ch = GetChar(m_currentPosition)) == '\r' || ch == '\n' || ch == '\u2028' || ch == '\u2029')
                {
                    if (ch == '\r')
                    {
                        // skip over the \r and if the next one is an \n skip over it, too
                        if (GetChar(++m_currentPosition) == '\n')
                        {
                            ++m_currentPosition;
                        }
                    }
                    else
                    {
                        // skip over any other non-\r character
                        ++m_currentPosition;
                    }

                    // increment the line number and reset the start position
                    m_currentLine++;
                    m_startLinePosition = m_currentPosition;
                }
            }

            // if we WERE in a single-line comment, we aren't anymore!
            m_inSingleLineComment = false;
            return token;
        }

        private JSToken IllegalCharacter()
        {
            m_currentToken.EndLineNumber = m_currentLine;
            m_currentToken.EndLinePosition = m_startLinePosition;
            m_currentToken.EndPosition = m_currentPosition;

            HandleError(JSError.IllegalChar);
            return JSToken.Error;
        }

        /// <summary>
        /// Set the list of preprocessor defined names and values
        /// </summary>
        /// <param name="defines">dictionary of name/value pairs</param>
        public void SetPreprocessorDefines(IDictionary<string, string> defines)
        {
            // this is a destructive set, blowing away any previous list
            if (defines != null && defines.Count > 0)
            {
                // create a new dictionary, case-INsensitive
                m_defines = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                // add an entry for each unique, valid name passed to us.
                foreach (var nameValuePair in defines)
                {
                    if (JSScanner.IsValidIdentifier(nameValuePair.Key) && !m_defines.ContainsKey(nameValuePair.Key))
                    {
                        m_defines.Add(nameValuePair.Key, nameValuePair.Value);
                    }
                }
            }
            else
            {
                // we have no defined names
                m_defines = null;
            }
        }

        private void OnGlobalDefine(string name)
        {
            if (GlobalDefine != null)
            {
                GlobalDefine(this, new GlobalDefineEventArgs() { Name = name });
            }
        }

        private void OnNewModule(string newModule)
        {
            if (NewModule != null)
            {
                NewModule(this, new NewModuleEventArgs { Module = newModule });
            }
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

                            case JSToken.Native:
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
            var startIndex = m_currentPosition;
            for (int ndx = 0; ndx < target.Length; ++ndx)
            {
                if (target[ndx] != char.ToUpperInvariant(GetChar(startIndex + ndx)))
                {
                    // no match
                    return false;
                }
            }

            // if we got here, the strings match. Advance the current position over it
            m_currentPosition += target.Length;
            return true;
        }

        private char GetChar(int index)
        {
            if (index < m_endPos)
            {
                return m_strSourceCode[index];
            }

            return '\0';
        }

        private void ScanIdentifier()
        {
            for (;;)
            {
                char c = GetChar(m_currentPosition);
                if (!IsIdentifierPartChar(c))
                {
                    break;
                }

                ++m_currentPosition;
            }

            if (AllowEmbeddedAspNetBlocks
                && CheckSubstring(m_currentPosition, "<%="))
            {
                // the identifier has an ASP.NET <%= ... %> block as part of it.
                // move the current position to the opening % character and call 
                // the method that will parse it from there.
                ++m_currentPosition;
                ScanAspNetBlock();
            }

            if (m_lastPosOnBuilder > 0)
            {
                m_identifier.Append(m_strSourceCode.Substring(m_lastPosOnBuilder, m_currentPosition - m_lastPosOnBuilder));
                m_lastPosOnBuilder = 0;
            }
        }

        private JSToken ScanKeyword(JSKeyword keyword)
        {
            for (;;)
            {
                char c = GetChar(m_currentPosition);
                if ('a' <= c && c <= 'z')
                {
                    m_currentPosition++;
                    continue;
                }

                if (IsIdentifierPartChar(c) 
                    || (AllowEmbeddedAspNetBlocks && CheckSubstring(m_currentPosition, "<%=")))
                {
                    ScanIdentifier();
                    return JSToken.Identifier;
                }

                break;
            }

            return keyword.GetKeyword(m_currentToken, m_currentPosition - m_currentToken.StartPosition);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private JSToken ScanNumber(char leadChar)
        {
            bool noMoreDot = '.' == leadChar;
            JSToken token = noMoreDot ? JSToken.NumericLiteral : JSToken.IntegerLiteral;
            bool exponent = false;
            char c;
            m_literalIssues = false;

            if ('0' == leadChar)
            {
                // c is now the character AFTER the leading zero
                c = GetChar(m_currentPosition);
                if ('x' == c || 'X' == c)
                {
                    if (JSScanner.IsHexDigit(GetChar(m_currentPosition + 1)))
                    {
                        while (JSScanner.IsHexDigit(GetChar(++m_currentPosition)))
                        {
                            // empty
                        }
                    }

                    return CheckForNumericBadEnding(token);
                }
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
                else if ('0' <= c && c <= '7')
                {
                    // this is a zero followed by a digit between 0 and 7.
                    // This could be interpreted as an octal literal, which isn't strictly supported.
                    while ('0' <= c && c <= '7')
                    {
                        c = GetChar(++m_currentPosition);
                    }

                    // return the integer token with issues, which should cause it to be output
                    // as-is and not combined with other literals or anything.
                    m_literalIssues = true;
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
                c = GetChar(m_currentPosition);
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
                        char e = GetChar(m_currentPosition - 1);
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

                m_currentPosition++;
            }

            c = GetChar(m_currentPosition - 1);
            if ('+' == c || '-' == c)
            {
                m_currentPosition--;
                c = GetChar(m_currentPosition - 1);
            }

            if ('e' == c || 'E' == c)
            {
                m_currentPosition--;
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
            char ch = GetChar(m_currentPosition);
            if (('0' <= ch && ch <= '9') || IsValidIdentifierStart(ch))
            {
                // we know that next character is invalid, so skip it and 
                // anything else after it that's identifier-like, and throw an error.
                ++m_currentPosition;
                while (IsValidIdentifierPart(GetChar(m_currentPosition)))
                {
                    ++m_currentPosition;
                }

                m_literalIssues = true;
                HandleError(JSError.BadNumericLiteral);
                token = JSToken.NumericLiteral;
            }

            return token;
        }

        internal String ScanRegExp()
        {
            int pos = m_currentPosition;
            bool isEscape = false;
            bool isInSet = false;
            char c;
            while (!IsEndLineOrEOF(c = GetChar(m_currentPosition++), 0))
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
                    if (pos == m_currentPosition)
                    {
                        return null;
                    }

                    m_currentToken.EndPosition = m_currentPosition;
                    m_currentToken.EndLinePosition = m_startLinePosition;
                    m_currentToken.EndLineNumber = m_currentLine;
                    return m_strSourceCode.Substring(
                        m_currentToken.StartPosition + 1,
                        m_currentToken.EndPosition - m_currentToken.StartPosition - 2);
                }
                else if (c == '\\')
                {
                    isEscape = true;
                }
            }

            // reset and return null. Assume it is not a reg exp
            m_currentPosition = pos;
            return null;
        }

        internal String ScanRegExpFlags()
        {
            int pos = m_currentPosition;
            while (JSScanner.IsAsciiLetter(GetChar(m_currentPosition)))
            {
                m_currentPosition++;
            }

            if (pos != m_currentPosition)
            {
                m_currentToken.EndPosition = m_currentPosition;
                m_currentToken.EndLineNumber = m_currentLine;
                m_currentToken.EndLinePosition = m_startLinePosition;
                return m_strSourceCode.Substring(pos, m_currentToken.EndPosition - pos);
            }

            return null;
        }

        /// <summary>
        /// Scans for the end of an Asp.Net block.
        ///  On exit this.currentPos will be at the next char to scan after the asp.net block.
        /// </summary>
        private JSToken ScanAspNetBlock()
        {
            // assume we find an asp.net block
            var tokenType = JSToken.AspNetBlock;

            // the current position is the % that opens the <%.
            // advance to the next character and save it because we will want 
            // to know whether it's an equals-sign later
            var thirdChar = GetChar(++m_currentPosition);

            // advance to the next character
            ++m_currentPosition;

            // loop until we find a > with a % before it (%>)
            while (!(this.GetChar(this.m_currentPosition - 1) == '%' &&
                     this.GetChar(this.m_currentPosition) == '>') ||
                     IsEndOfFile)
            {
                this.m_currentPosition++;
            }

            // we should be at the > of the %> right now.
            // set the end point of this token
            m_currentToken.EndPosition = m_currentPosition + 1;
            m_currentToken.EndLineNumber = m_currentLine;
            m_currentToken.EndLinePosition = m_startLinePosition;

            // see if we found an unterminated asp.net block
            if (IsEndOfFile)
            {
                HandleError(JSError.UnterminatedAspNetBlock);
            }
            else
            {
                // Eat the last >.
                this.m_currentPosition++;

                if (thirdChar == '=')
                {
                    // this is a <%= ... %> token.
                    // we're going to treat this like an identifier
                    tokenType = JSToken.Identifier;

                    // now, if the next character is an identifier part
                    // then skip to the end of the identifier. And if this is
                    // another <%= then skip to the end (%>)
                    if (IsValidIdentifierPart(GetChar(m_currentPosition))
                        || CheckSubstring(m_currentPosition, "<%="))
                    {
                        // and do it however many times we need
                        while (true)
                        {
                            if (IsValidIdentifierPart(GetChar(m_currentPosition)))
                            {
                                // skip to the end of the identifier part
                                while (IsValidIdentifierPart(GetChar(++m_currentPosition)))
                                {
                                    // loop
                                }

                                // when we get here, the current position is the first
                                // character that ISN"T an identifier-part. That means everything 
                                // UP TO this point must have been on the 
                                // same line, so we only need to update the position
                                m_currentToken.EndPosition = m_currentPosition;
                            }
                            else if (CheckSubstring(m_currentPosition, "<%="))
                            {
                                // skip forward four characters -- the minimum position
                                // for the closing %>
                                m_currentPosition += 4;

                                // and keep looping until we find it
                                while (!(this.GetChar(this.m_currentPosition - 1) == '%' &&
                                         this.GetChar(this.m_currentPosition) == '>') ||
                                         IsEndOfFile)
                                {
                                    this.m_currentPosition++;
                                }

                                // update the end of the token
                                m_currentToken.EndPosition = m_currentPosition + 1;
                                m_currentToken.EndLineNumber = m_currentLine;
                                m_currentToken.EndLinePosition = m_startLinePosition;

                                // we should be at the > of the %> right now.
                                // see if we found an unterminated asp.net block
                                if (IsEndOfFile)
                                {
                                    HandleError(JSError.UnterminatedAspNetBlock);
                                }
                                else
                                {
                                    // skip the > and go around another time
                                    ++m_currentPosition;
                                }
                            }
                            else
                            {
                                // neither an identifer part nor another <%= sequence,
                                // so we're done here
                                break;
                            }
                        }
                    }
                }
            }

            return tokenType;
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
            int start = m_currentPosition;
            m_decodedString = null;
            m_literalIssues = false;
            StringBuilder result = null;

            char ch;
            while((ch = GetChar(m_currentPosition++)) != delimiter)
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
                        //m_literalIssues = true;
                        HandleError(JSError.UnterminatedString);

                        // back up to the start of the line terminator
                        --m_currentPosition;
                        if (GetChar(m_currentPosition - 1) == '\r')
                        {
                            --m_currentPosition;
                        }

                        break;
                    }

                    if ('\0' == ch)
                    {
                        // whether it's a null literal character within the string or an
                        // actual end of file, this string literal has issues....
                        m_literalIssues = true;

                        if (IsEndOfFile)
                        {
                            m_currentPosition--;
                            HandleError(JSError.UnterminatedString);
                            break;
                        }

                    }

                    if (AllowEmbeddedAspNetBlocks
                        && ch == '<'
                        && GetChar(m_currentPosition) == '%')
                    {
                        // start of an ASP.NET block INSIDE a string literal.
                        // just skip the entire ASP.NET block -- move forward until
                        // we find the closing %> delimiter, then we'll continue on
                        // with the next character.
                        SkipAspNetReplacement();

                        // asp.net blocks insides strings can cause issues
                        m_literalIssues = true;
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
                    if (m_currentPosition - start - 1 > 0)
                    {
                        // append all the non escape chars to the string builder
                        result.Append(m_strSourceCode, start, m_currentPosition - start - 1);
                    }

                    // state variable to be reset
                    bool seqOfThree = false;
                    bool isValidHex;
                    int escapeStart;
                    int esc = 0;

                    ch = GetChar(m_currentPosition++);
                    switch (ch)
                    {
                        // line terminator crap
                        case '\r':
                            if ('\n' == GetChar(m_currentPosition))
                            {
                                m_currentPosition++;
                            }

                            goto case '\n';

                        case '\n':
                        case '\u2028':
                        case '\u2029':
                            m_currentLine++;
                            m_startLinePosition = m_currentPosition;
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
                            m_literalIssues = true;
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
                            escapeStart = m_currentPosition - 2;
                            isValidHex = true;
                            if (!ScanHexDigit(ref esc))
                            {
                                isValidHex = false;
                                
                                // if that invalid character (which the scan function skipped over)
                                // was a delimiter, back up!
                                if (GetChar(m_currentPosition - 1) == delimiter)
                                {
                                    --m_currentPosition;
                                }
                            }

                            if (!ScanHexDigit(ref esc))
                            {
                                isValidHex = false;

                                // if that invalid character (which the scan function skipped over)
                                // was a delimiter, back up!
                                if (GetChar(m_currentPosition - 1) == delimiter)
                                {
                                    --m_currentPosition;
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
                                result.Append(m_strSourceCode.Substring(escapeStart, m_currentPosition - escapeStart));
                                m_literalIssues = true;
                                HandleError(JSError.BadHexEscapeSequence);
                            }
                            break;

                        // unicode escape sequence /uHHHH
                        case 'u':
                            // save the start of the escape in case we fail
                            escapeStart = m_currentPosition - 2;
                            isValidHex = true;
                            if (!ScanHexDigit(ref esc))
                            {
                                isValidHex = false;
                                
                                // if that invalid character (which the scan function skipped over)
                                // was a delimiter, back up!
                                if (GetChar(m_currentPosition - 1) == delimiter)
                                {
                                    --m_currentPosition;
                                }
                            }

                            if (!ScanHexDigit(ref esc))
                            {
                                isValidHex = false;
                                
                                // if that invalid character (which the scan function skipped over)
                                // was a delimiter, back up!
                                if (GetChar(m_currentPosition - 1) == delimiter)
                                {
                                    --m_currentPosition;
                                }
                            }

                            if (!ScanHexDigit(ref esc))
                            {
                                isValidHex = false;
                                
                                // if that invalid character (which the scan function skipped over)
                                // was a delimiter, back up!
                                if (GetChar(m_currentPosition - 1) == delimiter)
                                {
                                    --m_currentPosition;
                                }
                            }

                            if (!ScanHexDigit(ref esc))
                            {
                                isValidHex = false;
                                
                                // if that invalid character (which the scan function skipped over)
                                // was a delimiter, back up!
                                if (GetChar(m_currentPosition - 1) == delimiter)
                                {
                                    --m_currentPosition;
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
                                result.Append(m_strSourceCode.Substring(escapeStart, m_currentPosition - escapeStart));
                                m_literalIssues = true;
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
                            m_literalIssues = true;

                            // esc is reset at the beginning of the loop and it is used to check that we did not go through the cases 1, 2 or 3
                            if (!seqOfThree)
                            {
                                esc = (ch - '0') << 3;
                            }

                            ch = GetChar(m_currentPosition++);
                            if ('0' <= ch && ch <= '7')
                            {
                                if (seqOfThree)
                                {
                                    esc |= (ch - '0') << 3;
                                    ch = GetChar(m_currentPosition++);
                                    if ('0' <= ch && ch <= '7')
                                    {
                                        esc |= ch - '0';
                                        result.Append((char)esc);
                                    }
                                    else
                                    {
                                        result.Append((char)(esc >> 3));

                                        // do not skip over this char we have to read it back
                                        --m_currentPosition;
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
                                --m_currentPosition;
                            }

                            HandleError(JSError.OctalLiteralsDeprecated);
                            break;

                        default:
                            // not an octal number, ignore the escape '/' and simply append the current char
                            result.Append(ch);
                            break;
                    }

                    start = m_currentPosition;
                }
            }

            // update the unescaped string
            if (null != result)
            {
                if (m_currentPosition - start - 1 > 0)
                {
                    // append all the non escape chars to the string builder
                    result.Append(m_strSourceCode, start, m_currentPosition - start - 1);
                }
                m_decodedString = result.ToString();
            }
            else if (m_currentPosition == m_currentToken.StartPosition + 1)
            {
                // empty unterminated string!
                m_decodedString = string.Empty;
            }
            else
            {
                // might be an unterminated string, so make sure that last character is the terminator
                int numDelimiters = (GetChar(m_currentPosition - 1) == delimiter ? 2 : 1);
                m_decodedString = m_strSourceCode.Substring(m_currentToken.StartPosition + 1, m_currentPosition - m_currentToken.StartPosition - numDelimiters);
            }
        }

        private bool ScanHexDigit(ref int esc)
        {
            // get the current character and advance the pointer assuming it's good
            var ch = GetChar(m_currentPosition++);

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
            ++m_currentPosition;

            char ch;
            while ((ch = GetChar(m_currentPosition++)) != '\0' || !IsEndOfFile)
            {
                if (ch == '%'
                    && GetChar(m_currentPosition) == '>')
                {
                    // found the closing delimiter -- the current position in on the >
                    // so we need to advance to the next character and break out of the loop
                    ++m_currentPosition;
                    break;
                }
            }
        }

        private void SkipSingleLineComment()
        {
            // skip up to the terminator, but don't include them.
            // the single-line comment does NOT include the line terminator!
            SkipToEndOfLine();

            // no longer in a single-line comment
            m_inSingleLineComment = false;

            // fix up the end of the token
            m_currentToken.EndPosition = m_currentPosition;
            m_currentToken.EndLinePosition = m_startLinePosition;
            m_currentToken.EndLineNumber = m_currentLine;
        }

        private void SkipToEndOfLine()
        {
            var c = GetChar(m_currentPosition);
            while (c != 0
                && c != '\n'
                && c != '\r'
                && c != '\x2028'
                && c != '\x2029')
            {
                c = GetChar(++m_currentPosition);
            }
        }

        private void SkipOneLineTerminator()
        {
            var c = GetChar(m_currentPosition);
            if (c == '\r')
            {
                // skip over the \r; and if it's followed by a \n, skip it, too
                if (GetChar(++m_currentPosition) == '\n')
                {
                    ++m_currentPosition;
                }

                m_currentLine++;
                m_startLinePosition = m_currentPosition;
            }
            else if (c == '\n'
                || c == '\x2028'
                || c == '\x2029')
            {
                // skip over the single line-feed character
                ++m_currentPosition;

                m_currentLine++;
                m_startLinePosition = m_currentPosition;
            }
        }

        // this method is public because it's used from the authoring code
        public void SkipMultilineComment()
        {
            for (; ; )
            {
                char c = GetChar(m_currentPosition);
                while ('*' == c)
                {
                    c = GetChar(++m_currentPosition);
                    if ('/' == c)
                    {
                        // get past the trailing slash
                        m_currentPosition++;

                        // no longer in a multiline comment; fix up the end of the current token.
                        m_inMultipleLineComment = false;
                        m_currentToken.EndPosition = m_currentPosition;
                        m_currentToken.EndLinePosition = m_startLinePosition;
                        m_currentToken.EndLineNumber = m_currentLine;
                        return;
                    }

                    if ('\0' == c)
                    {
                        break;
                    }
                    
                    if (IsLineTerminator(c, 1))
                    {
                        c = GetChar(++m_currentPosition);
                        m_currentLine++;
                        m_startLinePosition = m_currentPosition + 1;
                    }
                }

                if ('\0' == c && IsEndOfFile)
                {
                    break;
                }

                if (IsLineTerminator(c, 1))
                {
                    m_currentLine++;
                    m_startLinePosition = m_currentPosition + 1;
                }

                ++m_currentPosition;
            }

            // if we are here we got EOF
            m_currentToken.EndPosition = m_currentPosition;
            m_currentToken.EndLinePosition = m_startLinePosition;
            m_currentToken.EndLineNumber = m_currentLine;
            throw new ScannerException(JSError.NoCommentEnd);
        }

        private void SkipBlanks()
        {
            char c = GetChar(m_currentPosition);
            while (JSScanner.IsBlankSpace(c))
            {
                c = GetChar(++m_currentPosition);
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
                    if (0x0A == GetChar(m_currentPosition + increment))
                    {
                        m_currentPosition++;
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
                return IsEndLineOrEOF(GetChar(m_currentPosition), 0);
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
                if ('u' == GetChar(m_currentPosition + 1))
                {
                    char h1 = GetChar(m_currentPosition + 2);
                    if (IsHexDigit(h1))
                    {
                        char h2 = GetChar(m_currentPosition + 3);
                        if (IsHexDigit(h2))
                        {
                            char h3 = GetChar(m_currentPosition + 4);
                            if (IsHexDigit(h3))
                            {
                                char h4 = GetChar(m_currentPosition + 5);
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
                int startPosition = (m_lastPosOnBuilder > 0) ? m_lastPosOnBuilder : m_currentToken.StartPosition;
                if (m_currentPosition - startPosition > 0)
                {
                    m_identifier.Append(m_strSourceCode.Substring(startPosition, m_currentPosition - startPosition));
                }

                m_identifier.Append(c);
                m_currentPosition += 5;
                m_lastPosOnBuilder = m_currentPosition + 1;
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

        private string PPScanIdentifier(bool forceUpper)
        {
            string identifier = null;

            // start at the current position
            var startPos = m_currentPosition;

            // see if the first character is a valid identifier start
            if (JSScanner.IsValidIdentifierStart(GetChar(startPos)))
            {
                // it is -- skip to the next character
                ++m_currentPosition;

                // and keep going as long as we have valid part characters
                while (JSScanner.IsValidIdentifierPart(GetChar(m_currentPosition)))
                {
                    ++m_currentPosition;
                }
            }

            // if we advanced at all, return the code we scanned. Otherwise return null
            if (m_currentPosition > startPos)
            {
                identifier = m_strSourceCode.Substring(startPos, m_currentPosition - startPos);
                if (forceUpper)
                {
                    identifier = identifier.ToUpperInvariant();
                }
            }

            return identifier;
        }

        private bool PPScanInteger(out int intValue)
        {
            var startPos = m_currentPosition;
            while (IsDigit(GetChar(m_currentPosition)))
            {
                ++m_currentPosition;
            }

            var success = false;
            if (m_currentPosition > startPos)
            {
                success = int.TryParse(m_strSourceCode.Substring(startPos, m_currentPosition - startPos), out intValue);
            }
            else
            {
                intValue = 0;
            }

            return success;
        }

        private int PPSkipToDirective(params string[] endStrings)
        {
            // save the current position - if we hit an EOF before we find a directive
            // we're looking for, we'll use this as the end of the error context so we
            // don't have the whole rest of the file printed out with the error.
            var endPosition = m_currentPosition;
            var endLineNum = m_currentLine;
            var endLinePos = m_startLinePosition;

            while (true)
            {
                char c = GetChar(m_currentPosition++);
                switch (c)
                {
                    // EOF
                    case '\0':
                        if (IsEndOfFile)
                        {
                            // adjust the scanner state
                            m_currentPosition--;
                            m_currentToken.EndPosition = m_currentPosition;
                            m_currentToken.EndLineNumber = m_currentLine;
                            m_currentToken.EndLinePosition = m_startLinePosition;

                            // create a clone of the current token and set the ending to be the end of the
                            // directive for which we're trying to find an end. Use THAT context for the 
                            // error context. Then throw an exception so we can bail.
                            var contextError = m_currentToken.Clone();
                            contextError.EndPosition = endPosition;
                            contextError.EndLineNumber = endLineNum;
                            contextError.EndLinePosition = endLinePos;
                            contextError.HandleError(string.CompareOrdinal(endStrings[0], "#ENDDEBUG") == 0 
                                ? JSError.NoEndDebugDirective 
                                : JSError.NoEndIfDirective);
                            throw new ScannerException(JSError.ErrorEndOfFile);
                        }

                        break;

                    // line terminator crap
                    case '\r':
                        if (GetChar(m_currentPosition) == '\n')
                        {
                            m_currentPosition++;
                        }

                        m_currentLine++;
                        m_startLinePosition = m_currentPosition;
                        break;
                    case '\n':
                        m_currentLine++;
                        m_startLinePosition = m_currentPosition;
                        break;
                    case '\u2028':
                        m_currentLine++;
                        m_startLinePosition = m_currentPosition;
                        break;
                    case '\u2029':
                        m_currentLine++;
                        m_startLinePosition = m_currentPosition;
                        break;

                    // check for /// (and then followed by any one of the substrings passed to us)
                    case '/':
                        if (CheckSubstring(m_currentPosition, "//"))
                        {
                            // skip it
                            m_currentPosition += 2;

                            // check to see if this is the start of ANOTHER preprocessor construct. If it
                            // is, then it's a NESTED statement and we'll need to recursively skip the 
                            // whole thing so everything stays on track
                            if (CheckCaseInsensitiveSubstring("#IFDEF")
                                || CheckCaseInsensitiveSubstring("#IFNDEF")
                                || CheckCaseInsensitiveSubstring("#IF"))
                            {
                                PPSkipToDirective("#ENDIF");
                            }
                            else
                            {
                                // now check each of the ending strings that were passed to us to see if one of
                                // them is a match
                                for (var ndx = 0; ndx < endStrings.Length; ++ndx)
                                {
                                    if (CheckCaseInsensitiveSubstring(endStrings[ndx]))
                                    {
                                        // found the ending string
                                        return ndx;
                                    }
                                }

                                // not something we're looking for -- but is it a simple ///#END?
                                if (CheckCaseInsensitiveSubstring("#END"))
                                {
                                    // if the current character is not whitespace, then it's not a simple "#END"
                                    c = GetChar(m_currentPosition);
                                    if (IsBlankSpace(c) || IsAtEndOfLine)
                                    {
                                        // it is! Well, we were expecting either #ENDIF or #ENDDEBUG, but we found just an #END.
                                        // that's not how the syntax is SUPPOSED to go. But let's let it fly.
                                        // the ending token is always the first one.
                                        return 0;
                                    }
                                }
                            }
                        }

                        break;
                }
            }
        }

        // returns true we should skip the rest of this line,
        // or false if we are already processed the whole thing.
        private bool ScanPreprocessingDirective()
        {
            // check for some AjaxMin preprocessor comments
            if (CheckCaseInsensitiveSubstring("#GLOBALS"))
            {
                return ScanGlobalsDirective();
            }
            else if (CheckCaseInsensitiveSubstring("#SOURCE"))
            {
                return ScanSourceDirective();
            }
            else if (UsePreprocessorDefines)
            {
                if (CheckCaseInsensitiveSubstring("#DEBUG"))
                {
                    return ScanDebugDirective();
                }
                else if (CheckCaseInsensitiveSubstring("#IF"))
                {
                    return ScanIfDirective();
                }
                else if (CheckCaseInsensitiveSubstring("#ELSE") && m_ifDirectiveLevel > 0)
                {
                    return ScanElseDirective();
                }
                else if (CheckCaseInsensitiveSubstring("#ENDIF") && m_ifDirectiveLevel > 0)
                {
                    return ScanEndIfDirective();
                }
                else if (CheckCaseInsensitiveSubstring("#DEFINE"))
                {
                    return ScanDefineDirective();
                }
                else if (CheckCaseInsensitiveSubstring("#UNDEF"))
                {
                    return ScanUndefineDirective();
                }
            }

            return true;
        }

        private bool ScanGlobalsDirective()
        {
            // found ///#GLOBALS comment
            SkipBlanks();

            // should be one or more space-separated identifiers
            while (!IsAtEndOfLine)
            {
                var identifier = PPScanIdentifier(false);
                if (identifier != null)
                {
                    OnGlobalDefine(identifier);
                }

                SkipBlanks();
            }

            return true;
        }

        private bool ScanSourceDirective()
        {
            // found ///#SOURCE comment
            SkipBlanks();

            // pull the line, the column, and the source path off the line
            var linePos = 0;
            var colPos = 0;

            // line number is first
            if (PPScanInteger(out linePos))
            {
                SkipBlanks();

                // column number is second
                if (PPScanInteger(out colPos))
                {
                    SkipBlanks();

                    // the path should be the last part of the line.
                    // skip to the end and then use the part between.
                    var ndxStart = m_currentPosition;
                    SkipToEndOfLine();
                    if (m_currentPosition > ndxStart)
                    {
                        // there is a non-blank source token.
                        // so we have the line and the column and the source.
                        // use them. Remember, though: we stopped BEFORE the line terminator,
                        // so read ONE line terminator for the end of this line.
                        SkipOneLineTerminator();

                        // change the file context
                        var newModule = m_strSourceCode.Substring(ndxStart, m_currentPosition - ndxStart).TrimEnd();
                        m_currentToken.ChangeFileContext(newModule);

                        // adjust the line number
                        this.m_currentLine = linePos;

                        // the start line position is the current position less the column position.
                        // and because the column position in the comment is one-based, add one to get 
                        // back to zero-based: current - (col - 1)
                        this.m_startLinePosition = m_currentPosition - colPos + 1;

                        // the source offset for both start and end is now the current position.
                        // this is assuming that a single token doesn't span across source files, which isn't
                        // entirely true, since a string literal or multi-line comment (for example) may do just that.
                        this.m_currentToken.SourceOffsetStart = this.m_currentToken.SourceOffsetEnd = m_currentPosition;

                        // alert anyone (the parser) that we encountered the start of a new module
                        OnNewModule(newModule);

                        // return false because we are all set on the next line and DON'T want the
                        // line skipped.
                        return false;
                    }
                }
            }

            // something isn't right; skip the rest of this line
            return true;
        }

        private bool ScanIfDirective()
        {
            // we know we start with #IF -- see if it's #IFDEF or #IFNDEF
            var isIfDef = CheckCaseInsensitiveSubstring("DEF");
            var isIfNotDef = !isIfDef && CheckCaseInsensitiveSubstring("NDEF");

            // skip past the token and any blanks
            SkipBlanks();

            // if we encountered a line-break here, then ignore this directive
            if (!IsAtEndOfLine)
            {
                // get an identifier from the input
                var identifier = PPScanIdentifier(true);
                if (!string.IsNullOrEmpty(identifier))
                {
                    // set a state so that if we hit an #ELSE directive, we skip to #ENDIF
                    ++m_ifDirectiveLevel;

                    // if there is a dictionary AND the identifier is in it, then the identifier IS defined.
                    // if there is not dictionary OR the identifier is NOT in it, then it is NOT defined.
                    var isDefined = (m_defines != null && m_defines.ContainsKey(identifier));

                    // skip any blanks
                    SkipBlanks();

                    // if we are at the end of the line, or if this was an #IFDEF or #IFNDEF, then
                    // we have enough information to act
                    if (isIfDef || isIfNotDef || IsAtEndOfLine)
                    {
                        // either #IFDEF identifier, #IFNDEF identifier, or #IF identifier.
                        // this will simply test for whether or not it's defined
                        var conditionIsTrue = (!isIfNotDef && isDefined) || (isIfNotDef && !isDefined);

                        // if the condition is true, we just keep processing and when we hit the #END we're done,
                        // or if we hit an #ELSE we skip to the #END. But if we are not true, we need to skip to
                        // the #ELSE or #END directly.
                        if (!conditionIsTrue)
                        {
                            // the condition is FALSE!
                            // skip to #ELSE or #ENDIF and continue processing normally.
                            // (make sure the end if always the first one)
                            if (PPSkipToDirective("#ENDIF", "#ELSE") == 0)
                            {
                                // encountered the #ENDIF directive, so we know to reset the flag
                                --m_ifDirectiveLevel;
                            }
                        }
                    }
                    else
                    {
                        // this is an #IF and we have something after the identifier.
                        // it better be an operator or we'll ignore this comment.
                        var operation = CheckForOperator(PPOperators.Instance);
                        if (operation != null)
                        {
                            // skip any whitespace
                            SkipBlanks();

                            // save the current index -- this is either a non-whitespace character or the EOL.
                            // if it wasn't the EOL, skip to it now
                            var ndxStart = m_currentPosition;
                            if (!IsAtEndOfLine)
                            {
                                SkipToEndOfLine();
                            }

                            // the value to compare against is the substring between the start and the current.
                            // (and could be empty)
                            var compareTo = m_strSourceCode.Substring(ndxStart, m_currentPosition - ndxStart);

                            // now do the comparison and see if it's true. If the identifier isn't even defined, then
                            // the condition is false.
                            var conditionIsTrue = isDefined && operation(m_defines[identifier], compareTo.TrimEnd());

                            // if the condition is true, we just keep processing and when we hit the #END we're done,
                            // or if we hit an #ELSE we skip to the #END. But if we are not true, we need to skip to
                            // the #ELSE or #END directly.
                            if (!conditionIsTrue)
                            {
                                // the condition is FALSE!
                                // skip to #ELSE or #ENDIF and continue processing normally.
                                // (make sure the end if always the first one)
                                if (PPSkipToDirective("#ENDIF", "#ELSE") == 0)
                                {
                                    // encountered the #ENDIF directive, so we know to reset the flag
                                    --m_ifDirectiveLevel;
                                }
                            }
                        }
                    }
                }
            }

            return true;
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

        private bool ScanElseDirective()
        {
            // reset the state that says we were in an #IFDEF construct
            --m_ifDirectiveLevel;

            // ...then we now want to skip until the #ENDIF directive
            PPSkipToDirective("#ENDIF");
            return true;
        }

        private bool ScanEndIfDirective()
        {
            // reset the state that says we were in an #IFDEF construct
            --m_ifDirectiveLevel;
            return true;
        }

        private bool ScanDefineDirective()
        {
            // skip past the token and any blanks
            SkipBlanks();

            // if we encountered a line-break here, then ignore this directive
            if (!IsAtEndOfLine)
            {
                // get an identifier from the input
                var identifier = PPScanIdentifier(true);
                if (!string.IsNullOrEmpty(identifier))
                {
                    // see if we're assigning a value
                    string value = string.Empty;
                    SkipBlanks();
                    if (GetChar(m_currentPosition) == '=')
                    {
                        // we are! get the rest of the line as the trimmed string
                        var ndxStart = ++m_currentPosition;
                        SkipToEndOfLine();
                        value = m_strSourceCode.Substring(ndxStart, m_currentPosition - ndxStart).Trim();
                    }

                    // if there is no dictionary of defines yet, create one now
                    if (m_defines == null)
                    {
                        m_defines = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }

                    // if the identifier is not already in the dictionary, add it now
                    if (!m_defines.ContainsKey(identifier))
                    {
                        m_defines.Add(identifier, value);
                    }
                    else
                    {
                        // it already exists -- just set the value
                        m_defines[identifier] = value;
                    }
                }
            }

            return true;
        }

        private bool ScanUndefineDirective()
        {
            // skip past the token and any blanks
            SkipBlanks();

            // if we encountered a line-break here, then ignore this directive
            if (!IsAtEndOfLine)
            {
                // get an identifier from the input
                var identifier = PPScanIdentifier(true);

                // if there was an identifier and we have a dictionary of "defines" and the
                // identifier is in that dictionary...
                if (!string.IsNullOrEmpty(identifier))
                {
                    if (m_defines != null && m_defines.ContainsKey(identifier))
                    {
                        // remove the identifier from the "defines" dictionary
                        m_defines.Remove(identifier);
                    }
                }
            }

            return true;
        }

        private bool ScanDebugDirective()
        {
            // advance to the next character. If it's an equal sign, then this
            // debug comment is setting a debug namespace, not marking debug code.
            if (GetChar(m_currentPosition) == '=')
            {
                // we have ///#DEBUG=
                // get the namespace after the equal sign
                ++m_currentPosition;
                var identifier = PPScanIdentifier(false);
                if (identifier == null)
                {
                    // nothing. clear the debug namespaces
                    DebugLookupCollection.Clear();
                }
                else
                {
                    // this first identifier is the root namespace for the debug object.
                    // let's also treat it as a known global.
                    OnGlobalDefine(identifier);

                    // see if we have a period and keep looping to get IDENT(.IDENT)*
                    while (GetChar(m_currentPosition) == '.')
                    {
                        ++m_currentPosition;
                        var nextIdentifier = PPScanIdentifier(false);
                        if (nextIdentifier != null)
                        {
                            identifier += '.' + nextIdentifier;
                        }
                        else
                        {
                            // problem with the formatting -- ignore this comment
                            identifier = null;
                            break;
                        }
                    }

                    if (identifier != null)
                    {
                        // add the identifier to the debug list
                        DebugLookupCollection.Add(identifier);
                    }
                }
            }
            else if (StripDebugCommentBlocks && (m_defines == null || !m_defines.ContainsKey("DEBUG")))
            {
                // NOT a debug namespace assignment comment, so this is the start
                // of a debug block. If we are skipping debug blocks (the DEBUG name
                // is not defined AND StripDebugCommentBlocks is TRUE), then start skipping now.
                // skip until we hit ///#ENDDEBUG
                PPSkipToDirective("#ENDDEBUG");
            }

            return true;
        }

        private void HandleError(JSError error)
        {
            m_currentToken.EndPosition = m_currentPosition;
            m_currentToken.EndLinePosition = m_startLinePosition;
            m_currentToken.EndLineNumber = m_currentLine;
            m_currentToken.HandleError(error);
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

        internal static OperatorPrecedence GetOperatorPrecedence(Context op)
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
        /// Class for associating an operator with a function for ///#IF directives
        /// in a lazy-loaded manner. Doesn't create and initialize the dictionary
        /// until the scanner actually encounters syntax that needs it.
        /// The keys are sorted by length, decreasing (longest operators first).
        /// </summary>
        private sealed class PPOperators : SortedDictionary<string, Func<string, string, bool>>
        {
            private PPOperators()
                : base(new LengthComparer())
            {
                // add the operator information
                this.Add("==", PPIsEqual);
                this.Add("!=", PPIsNotEqual);
                this.Add("===", PPIsStrictEqual);
                this.Add("!==", PPIsNotStrictEqual);
                this.Add("<", PPIsLessThan);
                this.Add(">", PPIsGreaterThan);
                this.Add("<=", PPIsLessThanOrEqual);
                this.Add(">=", PPIsGreaterThanOrEqual);
            }

            #region thread-safe lazy-loading property and nested class

            public static PPOperators Instance
            {
                get
                {
                    return Nested.Instance;
                }
            }

            private static class Nested
            {
                internal static readonly PPOperators Instance = new PPOperators();
            }

            #endregion

            #region sorting class

            /// <summary>
            /// Sorting class for the sorted dictionary base to make sure the operators are
            /// enumerated with the LONGEST strings first, before the shorter strings.
            /// </summary>
            private class LengthComparer : Comparer<string>
            {
                public override int Compare(string x, string y)
                {
                    var delta = x != null && y != null ? y.Length - x.Length : 0;
                    return delta != 0 ? delta : string.CompareOrdinal(x, y);
                }
            }

            #endregion

            #region condition execution methods

            private static bool PPIsStrictEqual(string left, string right)
            {
                // strict comparison is only a string compare -- no conversion to float if
                // the string comparison fails.
                return string.Compare(left, right, StringComparison.OrdinalIgnoreCase) == 0;
            }

            private static bool PPIsNotStrictEqual(string left, string right)
            {
                // strict comparison is only a string compare -- no conversion to float if
                // the string comparison fails.
                return string.Compare(left, right, StringComparison.OrdinalIgnoreCase) != 0;
            }

            private static bool PPIsEqual(string left, string right)
            {
                // first see if a string compare works
                var isTrue = string.Compare(left, right, StringComparison.OrdinalIgnoreCase) == 0;

                // if not, then try converting both sides to double and doing the comparison
                if (!isTrue)
                {
                    double leftNumeric, rightNumeric;
                    if (ConvertToNumeric(left, right, out leftNumeric, out rightNumeric))
                    {
                        // they both converted successfully
                        isTrue = leftNumeric == rightNumeric;
                    }
                }

                return isTrue;
            }

            private static bool PPIsNotEqual(string left, string right)
            {
                // first see if a string compare works
                var isTrue = string.Compare(left, right, StringComparison.OrdinalIgnoreCase) != 0;

                // if they AREN'T equal, then try converting both sides to double and doing the comparison
                if (isTrue)
                {
                    double leftNumeric, rightNumeric;
                    if (ConvertToNumeric(left, right, out leftNumeric, out rightNumeric))
                    {
                        // they both converted successfully
                        isTrue = leftNumeric != rightNumeric;
                    }
                }

                return isTrue;
            }

            private static bool PPIsLessThan(string left, string right)
            {
                // only numeric comparisons
                bool isTrue = false;
                double leftNumeric, rightNumeric;
                if (ConvertToNumeric(left, right, out leftNumeric, out rightNumeric))
                {
                    // they both converted successfully
                    isTrue = leftNumeric < rightNumeric;
                }

                return isTrue;
            }

            private static bool PPIsGreaterThan(string left, string right)
            {
                // only numeric comparisons
                bool isTrue = false;
                double leftNumeric, rightNumeric;
                if (ConvertToNumeric(left, right, out leftNumeric, out rightNumeric))
                {
                    // they both converted successfully
                    isTrue = leftNumeric > rightNumeric;
                }

                return isTrue;
            }

            private static bool PPIsLessThanOrEqual(string left, string right)
            {
                // only numeric comparisons
                bool isTrue = false;
                double leftNumeric, rightNumeric;
                if (ConvertToNumeric(left, right, out leftNumeric, out rightNumeric))
                {
                    // they both converted successfully
                    isTrue = leftNumeric <= rightNumeric;
                }

                return isTrue;
            }

            private static bool PPIsGreaterThanOrEqual(string left, string right)
            {
                // only numeric comparisons
                bool isTrue = false;
                double leftNumeric, rightNumeric;
                if (ConvertToNumeric(left, right, out leftNumeric, out rightNumeric))
                {
                    // they both converted successfully
                    isTrue = leftNumeric >= rightNumeric;
                }

                return isTrue;
            }

            #endregion

            #region static helper methods

            /// <summary>
            /// Try converting the two strings to doubles
            /// </summary>
            /// <param name="left">first string</param>
            /// <param name="right">second string</param>
            /// <param name="leftNumeric">first string converted to double</param>
            /// <param name="rightNumeric">second string converted to double</param>
            /// <returns>true if the conversion was successful; false otherwise</returns>
            private static bool ConvertToNumeric(string left, string right, out double leftNumeric, out double rightNumeric)
            {
                rightNumeric = default(double);
                return double.TryParse(left, NumberStyles.Any, CultureInfo.InvariantCulture, out leftNumeric)
                    && double.TryParse(right, NumberStyles.Any, CultureInfo.InvariantCulture, out rightNumeric);
            }

            #endregion
        }
    }

    public class GlobalDefineEventArgs : EventArgs
    {
        public string Name { get; set; }
    }

    public class NewModuleEventArgs : EventArgs
    {
        public string Module { get; set; }
    }
}
