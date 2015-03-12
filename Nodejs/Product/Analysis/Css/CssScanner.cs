// CssScanner.cs
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

namespace Microsoft.Ajax.Utilities
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Scanner takes input stream and breaks it into Tokens
    /// </summary>
    internal class CssScanner
    {
        #region Constant strings

        // these strings are NOT to be localized -- they are CSS language features!
        private const string c_scanIncludes = "~=";
        private const string c_dashMatch = "|=";
        private const string c_prefixMatch = "^=";
        private const string c_suffixMatch = "$=";
        private const string c_substringMatch = "*=";
        private const string c_commentStart = "<!--";
        private const string c_commentEnd = "-->";

        #endregion

        private TextReader m_reader;
        private string m_readAhead;

        private char m_currentChar;

        private string m_rawNumber;
        public string RawNumber { get { return m_rawNumber; } }

        private CssContext m_context;

        private static Regex s_leadingZeros = new Regex(
            "^0*([0-9]+?)$"
#if !SILVERLIGHT
            , RegexOptions.Compiled
#endif
            );
        private static Regex s_trailingZeros = new Regex(
            "^([0-9]+?)0*$"
#if !SILVERLIGHT
            , RegexOptions.Compiled
#endif
            );
        
        public bool AllowEmbeddedAspNetBlocks { get; set; }

        public bool GotEndOfLine { get; set; }

        private bool m_isAtEOF;// = false;
        public bool EndOfFile
        {
            get { return m_isAtEOF; }
        }

        public CssScanner(TextReader reader)
        {
            m_context = new CssContext();

            m_reader = reader;
            //m_readAhead = null;

            // get the first character
            NextChar();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="Big case statement")]
        public CssToken NextToken()
        {
            GotEndOfLine = false;

            // advance the context
            m_context.Advance();
            m_rawNumber = null;

            CssToken token = null;
            bool tryAgain;
            do
            {
                tryAgain = false;
                switch (m_currentChar)
                {
                    case '\0':
                        // end of file
                        m_isAtEOF = true;
                        break;

                    case '\r':
                    case '\n':
                    case '\f':
                        // we hit an end-of-line character, but treat it like any other whitespace
                        GotEndOfLine = true;
                        goto case ' ';

                    case ' ':
                    case '\t':
                        // no matter how much whitespace is actually in
                        // the stream, we're just going to encode a single
                        // space in the token itself
                        while (IsSpace(m_currentChar))
                        {
                            if (m_currentChar == '\r' || m_currentChar == '\n' || m_currentChar == '\f')
                            {
                                GotEndOfLine = true;
                            }

                            NextChar();
                        }
                        token = new CssToken(TokenType.Space, ' ', m_context);
                        break;

                    case '/':
                        token = ScanComment();
                        if (token == null)
                        {
                            // this could happen if we processed an ajaxmin directive.
                            // go around again and try for the next token
                            tryAgain = true;
                        }
                        break;

                    case '<':
                        if (AllowEmbeddedAspNetBlocks && PeekChar() == '%')
                        {
                            token = ScanAspNetBlock();
                        }
                        else
                        {
                            token = ScanCDO();
                        }
                        break;

                    case '-':
                        token = ScanCDC();
                        if (token == null)
                        {
                            // identifier in CSS2.1 and CSS3 can start with a hyphen
                            // to indicate vendor-specific identifiers.
                            string ident = GetIdent();
                            if (ident != null)
                            {
                                // vendor-specific identifier
                                // but first see if it's a vendor-specific function!
                                if (m_currentChar == '(')
                                {
                                    // it is -- consume the parenthesis; it's part of the token
                                    NextChar();
                                    token = new CssToken(TokenType.Function, "-" + ident + '(', m_context);
                                }
                                else
                                {
                                    // nope -- just a regular identifier
                                    token = new CssToken(TokenType.Identifier, "-" + ident, m_context);
                                }
                            }
                            else
                            {
                                // just a hyphen character
                                token = new CssToken(TokenType.Character, '-', m_context);
                            }
                        }
                        break;

                    case '~':
                        token = ScanIncludes();
                        break;

                    case '|':
                        token = ScanDashMatch();
                        break;

                    case '^':
                        token = ScanPrefixMatch();
                        break;

                    case '$':
                        token = ScanSuffixMatch();
                        break;

                    case '*':
                        token = ScanSubstringMatch();
                        break;

                    case '\'':
                    case '"':
                        token = ScanString();
                        break;

                    case '#':
                        token = ScanHash();
                        break;

                    case '@':
                        token = ScanAtKeyword();
                        break;

                    case '!':
                        token = ScanImportant();
                        break;

                    case 'U':
                    case 'u':
                        token = ScanUrl();
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
                    case '.':
                        token = ScanNum();
                        break;

                    default:
                        token = ScanIdent();
                        break;
                }
            } while (tryAgain);

            return token;
        }

        #region Scan... methods

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private CssToken ScanComment()
        {
            CssToken token = null;
            NextChar();
            if (m_currentChar == '*')
            {
                NextChar();
                // everything is a comment until we get to */
                StringBuilder sb = new StringBuilder();
                sb.Append("/*");

                bool terminated = false;
                while (m_currentChar != '\0')
                {
                    sb.Append(m_currentChar);
                    if (m_currentChar == '*' && PeekChar() == '/')
                    {
                        sb.Append('/');
                        NextChar(); // now points to /
                        NextChar(); // now points to following character

                        // check for comment-hack 2 -- NS4 sees /*/*//*/ as a single comment
                        // while everyone else properly parses that as two comments, which hides everything
                        // after this construct until the next comment. So this hack shows the stuff
                        // between ONLY to NS4. But we still want to crunch it, so if we just found
                        // a comment like /*/*/, check to see if the next characters are /*/. If so,
                        // treat it like the single comment NS4 sees.
                        // (and don't forget that if we want to keep them, we've turned them both into important comments)
                        if (sb.ToString() == "/*!/*/" && ReadString("/*/"))
                        {
                            // read string will leave the current character after the /*/ string,
                            // so add the part we just read to the string builder and we'll break
                            // out of the loop
                            sb.Append("/*/");
                        }
                        terminated = true;
                        break;
                    }
                    NextChar();
                }
                if (!terminated)
                {
                    ReportError(0, CssErrorCode.UnterminatedComment);
                }
                token = new CssToken(TokenType.Comment, sb.ToString(), m_context);
            }
            else if (m_currentChar == '/')
            {
                // we found '//' -- it's a JS-style single-line comment which isn't strictly
                // supported by CSS, but we're going to treat as a valid comment because 
                // developers like using them. We're not going to persist them, though -- we're
                // going to eat these comments, since they're not valid CSS.

                // first check for our special ///#source directive. 
                // We're on the second slash; see if the NEXT character is a third slash
                if (PeekChar() == '/')
                {
                    // found '///'
                    NextChar();

                    // now w're on the third slash; see if the NEXT character is a pound-sign
                    if (PeekChar() == '#')
                    {
                        // okay, we have ///#, which we are going to reserve for all AjaxMin directive comments.
                        // so the source better not have something meaningful for the rest of the line.
                        NextChar();

                        // now we're on the pound-sign. See if we have the source directive
                        if (ReadString("#SOURCE"))
                        {
                            // we have a source directive: ///#source line col file
                            // skip space
                            DirectiveSkipSpace();

                            // pull the line and column numbers. Must be positive integers greater than zero.
                            int line = DirectiveScanInteger();
                            if (line > 0)
                            {
                                DirectiveSkipSpace();
                                int column = DirectiveScanInteger();
                                if (column > 0)
                                {
                                    DirectiveSkipSpace();

                                    // the rest of the comment line is the file path.
                                    var sb = new StringBuilder();
                                    while (m_currentChar != '\n' && m_currentChar != '\r')
                                    {
                                        sb.Append(m_currentChar);
                                        DirectiveNextChar();
                                    }

                                    var fileContext = sb.ToString().TrimEnd();
                                    if (!string.IsNullOrEmpty(fileContext))
                                    {
                                        // we got a proper line, column, and non-blank path. reset our context
                                        // with the new line and column.
                                        this.OnContextChange(fileContext, line, column);

                                        // START SPECIAL PROCESSING
                                        // don't use NextChar here because that method updates the line/col position.
                                        // at this stage, we are processing a directive, and we may have set the line/col
                                        // that we're supposed to be at for the start of the next line. So make SURE we
                                        // don't update lin/col until we get to the next line.
                                        // skip anything remaining up to a line terminator
                                        while (m_currentChar != '\n' && m_currentChar != '\r')
                                        {
                                            DirectiveNextChar();
                                        }

                                        // then skip a SINGLE line terminator without advancing the line
                                        // (although a \r\n pair is a single line terminator)
                                        if (m_currentChar == '\n' || m_currentChar == '\f')
                                        {
                                            DirectiveNextChar();
                                        }
                                        else if (m_currentChar == '\r')
                                        {
                                            if (DirectiveNextChar() == '\n')
                                            {
                                                DirectiveNextChar();
                                            }
                                        }

                                        // return null here so we don't fall through and return a / character.
                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }

                // eat the comment up to, but not including, the next line terminator
                while (m_currentChar != '\n' && m_currentChar != '\r' && m_currentChar != '\0')
                {
                    NextChar();
                }

                // if we wanted to maintain these comments, we would set the token
                // variable to a new CssToken object of type comment. But we don't, so
                // just return null so that the scanner will go around again.
                return null;
            }

            if (token == null)
            {
                // not a comment
                token = new CssToken(TokenType.Character, '/', m_context);
            }

            return token;
        }

        private CssToken ScanAspNetBlock()
        {
            StringBuilder sb = new StringBuilder();
            char prev = ' ';
            while (m_currentChar != '\0' &&
                   !(m_currentChar == '>' &&
                     prev == '%'))
            {
                sb.Append(m_currentChar);
                prev = m_currentChar;
                NextChar();
            }
            if (m_currentChar != '\0')
            {
                sb.Append(m_currentChar);
                // Read the last '>'
                NextChar();
            }
            return new CssToken(TokenType.AspNetBlock, sb.ToString(), m_context);
        }

        private CssToken ScanCDO()
        {
            CssToken token = null;
            NextChar(); // points to !?
            if (m_currentChar == '!')
            {
                if (PeekChar() == '-')
                {
                    NextChar(); // points to -
                    if (PeekChar() == '-')
                    {
                        NextChar(); // points to second hyphen
                        NextChar();
                        token = new CssToken(TokenType.CommentOpen, c_commentStart, m_context);
                    }
                    else
                    {
                        // we want to just return the < character, but
                        // we're currently pointing to the first hyphen,
                        // so we need to add the ! to the read ahead buffer
                        PushChar('!');
                    }
                }
            }
            return (token != null ? token : token = new CssToken(TokenType.Character, '<', m_context));
        }

        private CssToken ScanCDC()
        {
            CssToken token = null;
            NextChar(); // points to second hyphen?
            if (m_currentChar == '-')
            {
                if (PeekChar() == '>')
                {
                    NextChar(); // points to >
                    NextChar();
                    token = new CssToken(TokenType.CommentClose, c_commentEnd, m_context);
                }
            }
            return token;
        }

        private CssToken ScanIncludes()
        {
            CssToken token = null;
            NextChar();
            if (m_currentChar == '=')
            {
                NextChar();
                token = new CssToken(TokenType.Includes, c_scanIncludes, m_context);
            }
            return (token != null ? token : new CssToken(TokenType.Character, '~', m_context));
        }

        private CssToken ScanDashMatch()
        {
            CssToken token = null;
            // if the next character is an equals sign, then we have a dash-match
            if (PeekChar() == '=')
            {
                // skip the two characters
                NextChar();
                NextChar();
                token = new CssToken(TokenType.DashMatch, c_dashMatch, m_context);
            }
            else
            {
                // see if this is the start of a namespace ident
                token = ScanIdent();
            }

            // if we haven't computed a token yet, it's just a character
            if (token == null)
            {
                NextChar();
                token = new CssToken(TokenType.Character, '|', m_context);
            }
            return token;
        }

        private CssToken ScanPrefixMatch()
        {
            CssToken token = null;
            NextChar();
            if (m_currentChar == '=')
            {
                NextChar();
                token = new CssToken(TokenType.PrefixMatch, c_prefixMatch, m_context);
            }
            return (token != null ? token : new CssToken(TokenType.Character, '^', m_context));
        }

        private CssToken ScanSuffixMatch()
        {
            CssToken token = null;
            NextChar();
            if (m_currentChar == '=')
            {
                NextChar();
                token = new CssToken(TokenType.SuffixMatch, c_suffixMatch, m_context);
            }
            return (token != null ? token : new CssToken(TokenType.Character, '$', m_context));
        }

        private CssToken ScanSubstringMatch()
        {
            CssToken token = null;
            if (PeekChar() == '=')
            {
                // skip the two characters and return a substring match
                NextChar();
                NextChar();
                token = new CssToken(TokenType.SubstringMatch, c_substringMatch, m_context);
            }
            else
            {
                // see if this asterisk is a namespace portion of an identifier
                token = ScanIdent();
            }
            if (token == null)
            {
                // skip the * and return a character token
                NextChar();
                token = new CssToken(TokenType.Character, '*', m_context);
            }
            return token;
        }

        private CssToken ScanString()
        {
            // get the string literal
            string stringLiteral = GetString();

            // the literal must include both delimiters to be valid, so it has to AT LEAST
            // be two characters long. And then the first and the last characters should be
            // the same
            bool isValidString = (stringLiteral.Length >= 2
                && stringLiteral[0] == stringLiteral[stringLiteral.Length - 1]);

            return new CssToken(
              (isValidString ? TokenType.String : TokenType.Error),
              stringLiteral,
              m_context
              );
        }

        private CssToken ScanHash()
        {
            NextChar();
            string name = GetName();
            return (
              name == null
              ? new CssToken(TokenType.Character, '#', m_context)
              : new CssToken(TokenType.Hash, '#' + name, m_context)
              );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private CssToken ScanAtKeyword()
        {
            NextChar();

            // by default we're just going to return a character token for the "@" sign -- unless it
            // is followed by an identifier, in which case it's an at-symbol.
            TokenType tokenType = TokenType.Character;

            // if the next character is a hyphen, then we're going to want to pull it off and see if the
            // NEXT token is an identifier. If it's not, we'll stuff the hyphen back into the read buffer.
            bool startsWithHyphen = m_currentChar == '-';
            if (startsWithHyphen)
            {
                NextChar();
            }

            string ident = GetIdent();
            if (ident != null)
            {
                // if this started with a hyphen, then we need to add it to the start of our identifier now
                if (startsWithHyphen)
                {
                    ident = '-' + ident;
                }

                switch (ident.ToUpperInvariant())
                {
                    case "IMPORT":
                        tokenType = TokenType.ImportSymbol;
                        break;

                    case "PAGE":
                        tokenType = TokenType.PageSymbol;
                        break;

                    case "MEDIA":
                        tokenType = TokenType.MediaSymbol;
                        break;

                    case "FONT-FACE":
                        tokenType = TokenType.FontFaceSymbol;
                        break;

                    case "CHARSET":
                        tokenType = TokenType.CharacterSetSymbol;
                        break;

                    case "NAMESPACE":
                        tokenType = TokenType.NamespaceSymbol;
                        break;

                    case "TOP-LEFT-CORNER":
                        tokenType = TokenType.TopLeftCornerSymbol;
                        break;

                    case "TOP-LEFT":
                        tokenType = TokenType.TopLeftSymbol;
                        break;

                    case "TOP-CENTER":
                        tokenType = TokenType.TopCenterSymbol;
                        break;

                    case "TOP-RIGHT":
                        tokenType = TokenType.TopRightSymbol;
                        break;

                    case "TOP-RIGHT-CORNER":
                        tokenType = TokenType.TopRightCornerSymbol;
                        break;

                    case "BOTTOM-LEFT-CORNER":
                        tokenType = TokenType.BottomLeftCornerSymbol;
                        break;

                    case "BOTTOM-LEFT":
                        tokenType = TokenType.BottomLeftSymbol;
                        break;

                    case "BOTTOM-CENTER":
                        tokenType = TokenType.BottomCenterSymbol;
                        break;

                    case "BOTTOM-RIGHT":
                        tokenType = TokenType.BottomRightSymbol;
                        break;

                    case "BOTTOM-RIGHT-CORNER":
                        tokenType = TokenType.BottomRightCornerSymbol;
                        break;

                    case "LEFT-TOP":
                        tokenType = TokenType.LeftTopSymbol;
                        break;

                    case "LEFT-MIDDLE":
                        tokenType = TokenType.LeftMiddleSymbol;
                        break;

                    case "LEFT-BOTTOM":
                        tokenType = TokenType.LeftBottomSymbol;
                        break;

                    case "RIGHT-TOP":
                        tokenType = TokenType.RightTopSymbol;
                        break;

                    case "RIGHT-MIDDLE":
                        tokenType = TokenType.RightMiddleSymbol;
                        break;

                    case "RIGHT-BOTTOM":
                        tokenType = TokenType.RightBottomSymbol;
                        break;

                    case "KEYFRAMES":
                    case "-MS-KEYFRAMES":
                    case "-MOZ-KEYFRAMES":
                    case "-WEBKIT-KEYFRAMES":
                        tokenType = TokenType.KeyFramesSymbol;
                        break;

                    default:
                        tokenType = TokenType.AtKeyword;
                        break;
                }
            }
            else if (startsWithHyphen)
            {
                // we didn't find an identifier after the "@-".
                // we're going to return a character token for the @, but we need to push the hyphen
                // back into the read buffer for next time
                PushChar('-');
            }

            return new CssToken(tokenType, '@' + (ident == null ? string.Empty : ident), m_context);
        }

        private CssToken ScanImportant()
        {
            CssToken token = null;
            NextChar();

            string w = GetW();
            if (char.ToUpperInvariant(m_currentChar) == 'I')
            {
                if (ReadString("IMPORTANT"))
                {
                    // no matter what the case or whether or not there is space between the ! and 
                    // the important, we're going to represent this token as having no space and all
                    // lower-case.
                    token = new CssToken(TokenType.ImportantSymbol, "!important", m_context);
                }
            }
            // if the token is still null but we had found some whitespace,
            // we need to push a whitespace char back onto the read-ahead
            if (token == null && w.Length > 0)
            {
                PushChar(' ');
            }

            return (token != null ? token : new CssToken(TokenType.Character, '!', m_context));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private CssToken ScanUnicodeRange()
        {
            // when called, the current character is the character *after* U+
            CssToken token = null;
            StringBuilder sb = new StringBuilder();
            sb.Append("U+");

            bool hasQuestions = false;
            int count = 0;
            bool leadingZero = true;
            int firstValue = 0;
            while (m_currentChar != '\0'
                && count < 6
                && (m_currentChar == '?' || (!hasQuestions && IsH(m_currentChar))))
            {
                // if this isn't a leading zero, reset the flag
                if (leadingZero && m_currentChar != '0')
                {
                    leadingZero = false;
                }

                if (m_currentChar == '?')
                {
                    hasQuestions = true;
                    
                    // assume the digit is an "F" for maximum value
                    firstValue = firstValue*16 + HValue('F');
                }
                else
                {
                    firstValue = firstValue*16 + HValue(m_currentChar);
                }

                if (!leadingZero)
                {
                    sb.Append(m_currentChar);
                }

                ++count;
                NextChar();
            }

            if (count > 0)
            {
                // if the unicode value is out of range, throw an error
                if (firstValue < 0 || 0x10ffff < firstValue)
                {
                    // throw an error
                    ReportError(0, CssErrorCode.InvalidUnicodeRange, sb.ToString());
                }

                // if we still have the leading zero flag, then all the numbers were zero
                // and we didn't output any of them.
                if (leadingZero)
                {
                    // add one zero to keep it proper
                    sb.Append('0');
                }

                if (hasQuestions)
                {
                    // if there are question marks, then we're done
                    token = new CssToken(
                        TokenType.UnicodeRange,
                        sb.ToString(),
                        m_context);
                }
                else if (m_currentChar == '-')
                {
                    sb.Append('-');
                    NextChar();

                    count = 0;
                    leadingZero = true;
                    int secondValue = 0;
                    while (m_currentChar != '\0' && count < 6 && IsH(m_currentChar))
                    {
                        // if this isn't a leading zero, reset the flag
                        if (leadingZero && m_currentChar != '0')
                        {
                            leadingZero = false;
                        }

                        secondValue = secondValue * 16 + HValue(m_currentChar);

                        if (!leadingZero)
                        {
                            sb.Append(m_currentChar);
                        }

                        ++count;
                        NextChar();
                    }

                    if (count > 0)
                    {
                        // if we still have the leading zero flag, then all the numbers were zero
                        // and we didn't output any of them.
                        if (leadingZero)
                        {
                            // add one zero to keep it proper
                            sb.Append('0');
                        }

                        // check to make sure the second value is within range
                        // AND is greater than the first
                        if (secondValue < 0 || 0x10ffff < secondValue
                            || firstValue >= secondValue)
                        {
                            // throw an error
                            ReportError(0, CssErrorCode.InvalidUnicodeRange, sb.ToString());
                        }

                        token = new CssToken(
                            TokenType.UnicodeRange,
                            sb.ToString(),
                            m_context);
                    }
                }
                else
                {
                    // single code-point with at least one character
                    token = new CssToken(
                        TokenType.UnicodeRange,
                        sb.ToString(),
                        m_context);
                }
            }

            // if we don't hve a unicode range,
            // we need to return an ident token from the U we already scanned
            if (token == null)
            {
                // push everything back onto the buffer
                PushString(sb.ToString());
                token = ScanIdent();
            }

            return token;
        }

        private CssToken ScanUrl()
        {
            CssToken token = null;
            if (PeekChar() == '+')
            {
                NextChar(); // now current is the +
                NextChar(); // now current is the first character after the +
                token = ScanUnicodeRange();
            }
            else if (ReadString("URL("))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("url(");

                GetW();

                string url = GetString();
                if (url == null)
                {
                    url = GetUrl();
                }

                if (url != null)
                {
                    sb.Append(url);
                    GetW();
                    if (m_currentChar == ')')
                    {
                        sb.Append(')');
                        NextChar();

                        token = new CssToken(
                          TokenType.Uri,
                          sb.ToString(),
                          m_context
                          );
                    }
                }
            }
            return (token != null ? token : ScanIdent());
        }

        private CssToken ScanNum()
        {
            CssToken token = null;
            string num = GetNum();
            if (num != null)
            {
                if (m_currentChar == '%')
                {
                    NextChar();

                    // let's always keep the percentage on the number, even if it's
                    // zero -- some things require it (like the rgb function)
                    token = new CssToken(
                      TokenType.Percentage,
                      num + '%',
                      m_context
                      );

                    // and make sure the "raw number" we keep has it as well
                    m_rawNumber += '%';
                }
                else
                {
                    string dimen = GetIdent();
                    if (dimen == null)
                    {
                        // if there is no identifier, it's a num.
                        token = new CssToken(TokenType.Number, num, m_context);
                    }
                    else
                    {
                        // add the dimension to the raw number
                        m_rawNumber += dimen;

                        // classify the dimension type
                        TokenType tokenType = TokenType.Dimension;
                        switch (dimen.ToUpperInvariant())
                        {
                            case "EM":          // font-size of the element
                            case "EX":          // x-height of the element's font
                            case "CH":          // width of the zero glyph in the element's font
                            case "REM":         // font-size of the root element
                            case "VW":          // viewport's width
                            case "VH":          // viewport's height
                            case "VM":          // viewport width or height, whichever is smaller of the two (use VMIN)
                            case "VMIN":        // minimum of the viewport's height and width
                            case "VMAX":        // maximum of the viewport's height and width
                            case "FR":          // fraction of available space
                            case "GR":          // grid unit
                            case "GD":          // text grid unit
                                tokenType = TokenType.RelativeLength;
                                break;

                            case "CM":          // centimeters
                            case "MM":          // millimeters
                            case "IN":          // inches (1in == 2.54cm)
                            case "PX":          // pixels (1px == 1/96in)
                            case "PT":          // points (1pt == 1/72in)
                            case "PC":          // picas (1pc == 12pt)
                                tokenType = TokenType.AbsoluteLength;
                                break;

                            case "DEG":         // degrees (360deg == 1 full circle)
                            case "GRAD":        // gradians (400grad == 1 full circle)
                            case "RAD":         // radians (2*pi radians == 1 full circle)
                            case "TURN":        // turns (1turn == 1 full circle)
                                tokenType = TokenType.Angle;
                                break;

                            case "MS":          // milliseconds
                            case "S":           // seconds
                                tokenType = TokenType.Time;
                                break;

                            case "DPI":         // dots per inch
                            case "DPCM":        // dots per centimeter
                            case "DPPX":        // dots per pixel
                                tokenType = TokenType.Resolution;
                                break;

                            case "HZ":          // hertz
                            case "KHZ":         // kilohertz
                                tokenType = TokenType.Frequency;
                                break;

                            case "DB":          // decibel
                            case "ST":          // semitones
                                tokenType = TokenType.Speech;
                                break;
                        }

                        // if the number is zero, it really doesn't matter what the dimensions are so we can remove it
                        // EXCEPT for Angles, Times, Frequencies, and Resolutions - they should not get their units
                        // stripped, since we can only do that for LENGTHS as per the specs.
                        // And percentages, since 0% is the same as 0. (true?)
                        // ALSO, if we don't recognize the dimension, leave it -- it could be a browser hack or some
                        // other intentional construct.
                        if (num == "0" 
                            && tokenType != TokenType.Dimension
                            && tokenType != TokenType.Angle
                            && tokenType != TokenType.Time
                            && tokenType != TokenType.Frequency
                            && tokenType != TokenType.Resolution)
                        {
                            token = new CssToken(TokenType.Number, num, m_context);
                        }
                        else
                        {
                            token = new CssToken(tokenType, num + dimen, m_context);
                        }
                    }
                }
            }
            else if (m_currentChar == '.')
            {
                token = new CssToken(TokenType.Character, '.', m_context);
                NextChar();
            }
            else
            {
                // this function is only called when the first character is 
                // a digit or a period. So this block should never execute, since
                // a digit will produce a num, and if it doesn't, the previous block
                // picks up the period.
                ReportError(1, CssErrorCode.UnexpectedNumberCharacter, m_currentChar);
            }

            return token;
        }

        private CssToken ScanIdent()
        {
            CssToken token = null;

            string ident = GetIdent();
            if (ident != null)
            {
                if (m_currentChar == '(')
                {
                    NextChar();
                    if (string.Compare(ident, "not", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        token = new CssToken(TokenType.Not, ident + '(', m_context);
                    }
                    else
                    {
                        token = new CssToken(TokenType.Function, ident + '(', m_context);
                    }
                }
                else if (string.Compare(ident, "progid", StringComparison.OrdinalIgnoreCase) == 0 && m_currentChar == ':')
                {
                    NextChar();
                    token = ScanProgId();
                }
                else
                {
                    token = new CssToken(TokenType.Identifier, ident, m_context);
                }
            }

            // if we failed somewhere in the processing...
            if (ident == null)
            {
                if (m_currentChar != '\0')
                {
                    // create a character token
                    token = new CssToken(TokenType.Character, m_currentChar, m_context);
                    NextChar();
                }
            }
            return token;
        }

        private CssToken ScanProgId()
        {
            CssToken token = null;
            StringBuilder sb = new StringBuilder();
            sb.Append("progid:");
            string ident = GetIdent();
            while (ident != null)
            {
                sb.Append(ident);
                if (m_currentChar == '.')
                {
                    sb.Append('.');
                    NextChar();
                }
                ident = GetIdent();
            }
            if (m_currentChar == '(')
            {
                sb.Append('(');
                NextChar();

                token = new CssToken(TokenType.ProgId, sb.ToString(), m_context);
            }
            else
            {
                ReportError(1, CssErrorCode.ExpectedOpenParenthesis);
            }
            return token;
        }

        #endregion

        #region Is... methods

        private static bool IsSpace(char ch)
        {
            switch (ch)
            {
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                case '\f':
                    return true;

                default:
                    return false;
            }
        }

        private static int HValue(char ch)
        {
            if ('0' <= ch && ch <= '9')
            {
                return ch - '0';
            }
            if ('a' <= ch && ch <= 'f')
            {
                return (ch - 'a') + 10;
            }
            if ('A' <= ch && ch <= 'F')
            {
                return (ch - 'A') + 10;
            }
            return 0;
        }

        public static bool IsH(char ch)
        {
            return (
              ('0' <= ch && ch <= '9')
              || ('a' <= ch && ch <= 'f')
              || ('A' <= ch && ch <= 'F')
              );
        }

        private static bool IsD(char ch)
        {
            return ('0' <= ch && ch <= '9');
        }

        private static bool IsNonAscii(char ch)
        {
            return (128 <= ch && ch <= 65535);
        }

        internal static bool IsNmChar(char ch)
        {
            return IsNonAscii(ch)
                  || (ch == '-')
                  || (ch == '_')
                  || ('0' <= ch && ch <= '9')
                  || ('a' <= ch && ch <= 'z')
                  || ('A' <= ch && ch <= 'Z');
        }

        #endregion

        #region Get... methods

        /// <summary>
        /// returns the VALUE of a unicode number, up to six hex digits
        /// </summary>
        /// <returns>int value representing up to 6 hex digits</returns>
        private int GetUnicodeEncodingValue(out bool follwedByWhitespace)
        {
            int unicodeValue = 0;

            // loop for no more than 6 hex characters
            int count = 0;
            while (m_currentChar != '\0' && count++ < 6 && IsH(m_currentChar))
            {
                unicodeValue = (unicodeValue * 16) + HValue(m_currentChar);
                NextChar();
            }

            // if there is a space character afterwards, skip it
            // (but only skip one space character if present)
            follwedByWhitespace = IsSpace(m_currentChar);
            if (follwedByWhitespace)
            {
                NextChar();
            }

            return unicodeValue;
        }

        private string GetUnicode()
        {
            string unicode = null;
            if (m_currentChar == '\\')
            {
                char ch = PeekChar();
                if (IsH(ch))
                {
                    // let's actually decode the escapes so another encoding
                    // format might actually save us some space.
                    // skip over the slash
                    NextChar();

                    // decode the hexadecimal digits at the current character point,
                    // up to six characters
                    bool followedByWhitespace;
                    int unicodeValue = GetUnicodeEncodingValue(out followedByWhitespace);
                    if (unicodeValue == 0x5c || unicodeValue == 0x20)
                    {
                        // okay, we have an escaped backslash or space. Ideally, if we were making an interpreter,
                        // this sequence would return the decoded "\" or " " character. HOWEVER, because we're NOT
                        // interpreting this code and instead trying to produce the equivalent source output,
                        // we need to SPECIAL CASE these escape sequences so that they always gets reproduced
                        // as "\5c" or "\20" in our output -- because we will not escape the "\" or " " characters 
                        // because they might be part of an actual escape sequence.
                        // So just return the escaped sequence as-is and don't forget to keep any whitespace that
                        // may follow.
                        unicode = (followedByWhitespace ? @"\{0:x} " : @"\{0:x}").FormatInvariant(unicodeValue);
                    }
                    else
                    {
                        // we shouldn't NEED to check for surrogate pairs here because
                        // the encoding is up to six digits, which encompasses all the
                        // available Unicode characters without having to resort to
                        // surrogate pairs. However, someone can always manually
                        // encode two surrogate pair values in their source.
                        if (0xd800 <= unicodeValue && unicodeValue <= 0xdbff)
                        {
                            // this is a high-surrogate value.
                            int hi = unicodeValue;
                            // the next encoding BETTER be a unicode value
                            if (m_currentChar == '\\' && IsH(PeekChar()))
                            {
                                // skip the slash
                                NextChar();
                                // get the lo value
                                int lo = GetUnicodeEncodingValue(out followedByWhitespace);
                                if (0xdc00 <= lo && lo <= 0xdfff)
                                {
                                    // combine the hi/lo pair into one character value
                                    unicodeValue = 0x10000
                                      + (hi - 0xd800) * 0x400
                                      + (lo - 0xdc00);
                                }
                                else
                                {
                                    // ERROR! not a valid unicode lower-surrogate value!
                                    ReportError(
                                      0,
                                      CssErrorCode.InvalidLowSurrogate, hi, lo
                                      );
                                }
                            }
                            else
                            {
                                // ERROR! the high-surrogate is not followed by a low surrogate!
                                ReportError(
                                  0,
                                  CssErrorCode.HighSurrogateNoLow, unicodeValue
                                  );
                            }
                        }

                        // get the unicode character. might be multiple characters because
                        // the 21-bit value stired in the int might be encoded into a surrogate pair.
                        //unicode = char.ConvertFromUtf32(unicodeValue);
                        unicode = ConvertUtf32ToUtf16(unicodeValue);
                    }
                }
            }
            return unicode;
        }

        private static string ConvertUtf32ToUtf16(int unicodeValue)
        {
#if !SILVERLIGHT
            return char.ConvertFromUtf32(unicodeValue);
#else
            string text;
            if (unicodeValue <= 0xffff)
            {
                if (0xd8000 <= unicodeValue && unicodeValue <= 0xdfff)
                {
                    throw new ArgumentException("UTF32 value cannot be in surrogate range");
                }
                else
                {
                    // single-character normal results
                    text = new string((char)unicodeValue, 1);
                }
            }
            else if (unicodeValue < 0x10ffff)
            {
                // need to calculate the surrogate pair representation
                unicodeValue -= 0x10000;
                text = new string(new char[2]
                {
                    (char)((unicodeValue >> 10) + 0xd800),
                    (char)((unicodeValue & 0x3ff) + 0xdc00)
                });
            }
            else
            {
                throw new ArgumentException("UTF32 value out of range");
            }
            return text;
#endif
        }

        private string GetEscape()
        {
            string escape = GetUnicode();
            if (escape == null && m_currentChar == '\\')
            {
                char ch = PeekChar();
                if ((' ' <= ch && ch <= '~')
                  || IsNonAscii(ch))
                {
                    NextChar();
                    NextChar();
                    return "\\" + ch;
                }
            }
            return escape;
        }

        private string GetNmStart()
        {
            string nmStart = GetEscape();
            if (nmStart == null)
            {
                if (IsNonAscii(m_currentChar)
                  || (m_currentChar == '_')
                  || ('a' <= m_currentChar && m_currentChar <= 'z')
                  || ('A' <= m_currentChar && m_currentChar <= 'Z'))
                {
                    // actually, CSS1 and CSS2 don't allow underscores in
                    // identifier names, especially not the first character
                    if (m_currentChar == '_')
                    {
                        ReportError(
                          4,
                          CssErrorCode.UnderscoreNotValid
                          );
                    }

                    nmStart = char.ToString(m_currentChar);
                    NextChar();
                }
            }
            return nmStart;
        }

        private string GetNmChar()
        {
            string nmChar = GetEscape();
            if (nmChar == null)
            {
                if (IsNmChar(m_currentChar))
                {
                    // actually, CSS1 and CSS2 don't allow underscores in
                    // identifier names.
                    if (m_currentChar == '_')
                    {
                        ReportError(
                          4,
                          CssErrorCode.UnderscoreNotValid
                          );
                    }
                    nmChar = char.ToString(m_currentChar);
                    NextChar();
                }
            }
            return nmChar;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private string GetString()
        {
            string str = null;
            if (m_currentChar == '\'' || m_currentChar == '"')
            {
                char delimiter = m_currentChar;
                NextChar();

                StringBuilder sb = new StringBuilder();
                sb.Append(delimiter);

                while (m_currentChar != '\0' && m_currentChar != delimiter)
                {
                    str = GetEscape();
                    if (str != null)
                    {
                        // if this is a one-character string, and that one character
                        // if the same as our string delimiter, then this was probably
                        // a unicode-encoded character. We will need to escape it in the
                        // output or the string will be invalid.
                        if (str.Length == 1 && str[0] == delimiter)
                        {
                            // instead of escaping it as unicode again (\22 or \27), we
                            // can save a byte by encoding is as \" or \'
                            str = "\\" + delimiter;
                        }
                        sb.Append(str);
                    }
                    else if (IsNonAscii(m_currentChar))
                    {
                        sb.Append(m_currentChar);
                        NextChar();
                    }
                    else if (m_currentChar == '\\')
                    {
                        NextChar();
                        str = GetNewline();
                        if (str != null)
                        {
                            // new-lines in strings are "for aesthetic or other reasons,"
                            // but are not actually part of the string. We can remove
                            // then to crunch the string a bit.
                            //sb.Append( '\\' );
                            //sb.Append( str );
                        }
                        else
                        {
                            // unexpected escape sequence
                            ReportError(
                              0,
                              CssErrorCode.UnexpectedEscape, m_currentChar
                              );
                        }
                    }
                    else if ((m_currentChar == ' ')
                      || (m_currentChar == '\t')
                      || (m_currentChar == '!')
                      || (m_currentChar == '#')
                      || (m_currentChar == '$')
                      || (m_currentChar == '%')
                      || (m_currentChar == '&')
                      || ('(' <= m_currentChar && m_currentChar <= '~')
                      || (m_currentChar == (delimiter == '"' ? '\'' : '"')))
                    {
                        // save the current character, add it to the builder we are keeping,
                        // and get the next character
                        var ch = m_currentChar;
                        sb.Append(m_currentChar);
                        NextChar();

                        // if we are allowing embedded ASP.NET blocks and that last character ws
                        // a less-than character and the current character is a percent...
                        if (AllowEmbeddedAspNetBlocks 
                            && ch == '<'
                            && m_currentChar == '%')
                        {
                            // we have the start of an ASP.NET block. Skip to the end of that block, which
                            // is determined by a closing "%>" string. When this function returns, the current
                            // character should be the first character AFTER the %>
                            SkipAspNetBlock(sb);
                        }
                    }
                    else if (m_currentChar == '\n'
                        || m_currentChar == '\r')
                    {
                        // unterminated string
                        GotEndOfLine = true;
                        ReportError(
                          0,
                          CssErrorCode.UnterminatedString, sb.ToString()
                          );
                        // add the newline to the string so it will line-break in the output
                        sb.AppendLine();

                        // skip the block of whitespace we just encountered so that the current
                        // character will be the first non-whitespace character after the bogus
                        // string
                        while (IsSpace(m_currentChar))
                        {
                            NextChar();
                        }
                        // return early
                        return sb.ToString();
                    }
                    else
                    {
                        // unexpected string character
                        ReportError(
                          0,
                          CssErrorCode.UnexpectedStringCharacter, m_currentChar
                          );
                    }
                }
                if (m_currentChar == delimiter)
                {
                    sb.Append(delimiter);
                    NextChar(); // pass delimiter
                }
                str = sb.ToString();
            }
            return str;
        }

        private void SkipAspNetBlock(StringBuilder sb)
        {
            // add the current character (should be the % character from the <% opening)
            sb.Append(m_currentChar);

            // loop until we find the %> closing sequence, adding everything inbetween to the string builder
            NextChar();

            var mightBeClosing = false;
            while (m_currentChar != '\0')
            {
                if (m_currentChar == '%')
                {
                    // might be the first character of a closing sequence
                    mightBeClosing = true;
                }
                else if (mightBeClosing && m_currentChar == '>')
                {
                    // we found hte closing sequence.
                    // output the closing >, advance the character pointer, and bail
                    sb.Append(m_currentChar);
                    NextChar();
                    break;
                }
                else
                {
                    // nah -- not a closing sequence
                    mightBeClosing = false;
                }

                sb.Append(m_currentChar);
                NextChar();
            }
        }

        private string GetIdent()
        {
            string ident = GetNmStart();
            if (ident != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(ident);
                while (m_currentChar != '\0' && (ident = GetNmChar()) != null)
                {
                    sb.Append(ident);
                }
                ident = sb.ToString();
            }
            return ident;
        }

        private string GetName()
        {
            string name = GetNmChar();
            if (name != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(name);
                while (m_currentChar != '\0' && (name = GetNmChar()) != null)
                {
                    sb.Append(name);
                }
                name = sb.ToString();
            }
            return name;
        }

        private string GetNum()
        {
            string num = null;
            string units = null;
            string fraction = null;
            bool hasDecimalPoint = false;

            if (IsD(m_currentChar))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(m_currentChar);
                NextChar();
                while (IsD(m_currentChar))
                {
                    sb.Append(m_currentChar);
                    NextChar();
                }
                units = sb.ToString();
            }
            if (m_currentChar == '.')
            {
                if (IsD(PeekChar()))
                {
                    hasDecimalPoint = true;
                    // move over the decimal point
                    NextChar();

                    StringBuilder sb = new StringBuilder();
                    // check for extra digits
                    while (IsD(m_currentChar))
                    {
                        sb.Append(m_currentChar);
                        NextChar();
                    }
                    fraction = sb.ToString();
                }
                else if (units != null)
                {
                    // REVIEW: it looks like there must be at least one digit
                    // after a decimal point, but let's let it slack a bit and
                    // let decimal point be a part of a number if it starts with
                    // digits
                    hasDecimalPoint = true;
                    ReportError(
                      2,
                      CssErrorCode.DecimalNoDigit
                      );
                    fraction = string.Empty;
                    NextChar();
                }
            }
            if (units != null || fraction != null)
            {
                // get the raw number. This SHOULD match the input exactly
                m_rawNumber = units ?? ""
                    + (hasDecimalPoint ? "." : "")
                    + fraction ?? "";

                //if (m_collapseNumbers)
                {
                    if (units != null)
                    {
                        // remove leading zeros from units
                        units = s_leadingZeros.Replace(units, "$1");
                    }
                    if (fraction != null)
                    {
                        // remove trailing zeros from fraction
                        fraction = s_trailingZeros.Replace(fraction, "$1");
                        // if the results is just a single zero, we're going
                        // to ignore the fractional part altogether
                        if (fraction == "0" || fraction.Length == 0)
                        {
                            fraction = null;
                        }
                    }
                    // if we have a fractional part and the units is zero, then
                    // we're going to ignore the units
                    if (fraction != null && units == "0")
                    {
                        units = null;
                    }

                    if (fraction == null)
                    {
                        num = units;

                        // if the fraction is null and the units is null, then
                        // we must have a zero, because we wouldn't have come into
                        // this block is one or the other wasn't null before we 
                        // started stripping zeros.
                        if (num == null)
                        {
                            num = "0";
                        }
                    }
                    else
                    {
                        num = units + '.' + fraction;
                    }
                }
                /*else
                {
                    // just use what we have
                    num = m_rawNumber;
                }*/
            }
            return num;
        }

        private string GetUrl()
        {
            StringBuilder sb = new StringBuilder();
            while (m_currentChar != '\0')
            {
                string escape = GetEscape();
                if (escape != null)
                {
                    sb.Append(escape);
                }
                else if (IsNonAscii(m_currentChar)
                  || (m_currentChar == '!')
                  || (m_currentChar == '#')
                  || (m_currentChar == '$')
                  || (m_currentChar == '%')
                  || (m_currentChar == '&')
                  || ('*' <= m_currentChar && m_currentChar <= '~'))
                {
                    sb.Append(m_currentChar);
                    NextChar();
                }
                else
                {
                    break;
                }
            }
            return sb.ToString();
        }

        private string GetW()
        {
            string w = string.Empty;
            if (IsSpace(m_currentChar))
            {
                w = " ";
                NextChar();
                while (IsSpace(m_currentChar))
                {
                    NextChar();
                }
            }
            return w;
        }

        private string GetNewline()
        {
            string nl = null;
            switch (m_currentChar)
            {
                case '\n':
                    NextChar();
                    nl = "\n";
                    break;

                case '\f':
                    NextChar();
                    nl = "\f";
                    break;

                case '\r':
                    NextChar();
                    if (m_currentChar == '\n')
                    {
                        NextChar();
                        nl = "\r\n";
                    }
                    else
                    {
                        nl = "\r";
                    }
                    break;

                default:
                    break;
            }
            return nl;
        }

        #endregion

        #region NextChar, Peek..., Push...

        private void NextChar()
        {
            if (m_readAhead != null)
            {
                m_currentChar = m_readAhead[0];
                if (m_readAhead.Length == 1)
                {
                    m_readAhead = null;
                }
                else
                {
                    m_readAhead = m_readAhead.Substring(1); ;
                }

                // REVIEW: we don't handle pushing newlines back into buffer
                m_context.End.NextChar();
            }
            else
            {
                int ch = m_reader.Read();
                if (ch < 0)
                {
                    m_currentChar = '\0';
                }
                else
                {
                    m_currentChar = (char)ch;
                    switch (m_currentChar)
                    {
                        case '\n':
                        case '\f':
                            m_context.End.NextLine();
                            break;

                        case '\r':
                            if (PeekChar() != '\n')
                            {
                                m_context.End.NextLine();
                            }
                            break;

                        default:
                            m_context.End.NextChar();
                            break;
                    }
                }
            }
        }

        public char PeekChar()
        {
            if (m_readAhead != null)
            {
                return m_readAhead[0];
            }

            int ch = m_reader.Peek();
            if (ch < 0)
            {
                return '\0';
            }

            return (char)ch;
        }

        // case-INsensitive string at the current location in the input stream
        private bool ReadString(string str)
        {
            // if the first character doesn't match, then we
            // know we're not the string, and we don't have to
            // push anything back on the stack because we haven't
            // gone anywhere yet
            if (char.ToUpperInvariant(m_currentChar) != char.ToUpperInvariant(str[0]))
            {
                return false;
            }

            // if we get far enough in the match, we'll start queuing up the 
            // matched characters so far, in case we end up not matching and need
            // to push them back in the read-ahead queue.
            StringBuilder sb = null;

            // we'll start peeking ahead so we have less to push
            // if we fail
            for (int ndx = 1; ndx < str.Length; ++ndx)
            {
                if (char.ToUpperInvariant(PeekChar()) != char.ToUpperInvariant(str[ndx]))
                {
                    // not this string. Push what we've matched
                    if (ndx > 1)
                    {
                        if (sb != null)
                        {
                            PushString(sb.ToString());
                        }
                    }
                    return false;
                }

                if (sb == null)
                {
                    // create the string builder -- we need it now.
                    // it won't be longer than the string we're trying to match.
                    sb = new StringBuilder(str.Length);
                }

                sb.Append(m_currentChar);
                NextChar();
            }
            NextChar();
            return true;
        }

        private void PushChar(char ch)
        {
            if (m_readAhead == null)
            {
                m_readAhead = char.ToString(m_currentChar);
                m_currentChar = ch;
            }
            else
            {
                m_readAhead = m_currentChar + m_readAhead;
                m_currentChar = ch;
            }
            // REVIEW: doesn't handle pushing a newline back onto the buffer
            m_context.End.PreviousChar();
        }

        private void PushString(string str)
        {
            if (str.Length > 0)
            {
                if (str.Length > 1)
                {
                    m_readAhead = str.Substring(1) + m_currentChar + m_readAhead;
                }
                else
                {
                    m_readAhead = m_currentChar + m_readAhead;
                }
                m_currentChar = str[0];
            }

            // REVIEW: doesn't handle pushing a newline back onto the buffer
            for (int ndx = 0; ndx < str.Length; ++ndx)
            {
                m_context.End.PreviousChar();
            }
        }

        #endregion

        #region special directive-processing (non-CSS) methods

        private char DirectiveNextChar()
        {
            var next = m_reader.Read();
            m_currentChar = next < 0 ? '\0' : (char)next;
            return m_currentChar;
        }

        private void DirectiveSkipSpace()
        {
            while (m_currentChar == ' ' || m_currentChar == '\t')
            {
                NextChar();
            }
        }

        private int DirectiveScanInteger()
        {
            // returns 0 if there is no number at the current position
            var number = 0;
            while ('0' <= m_currentChar && m_currentChar <= '9')
            {
                number = number * 10 + (m_currentChar - '0');
                NextChar();
            }

            return number;
        }

        #endregion

        #region Error handling

        private void ReportError(int severity, CssErrorCode error, params object[] args)
        {
            // guide: 0 == syntax error
            //        1 == the programmer probably did not intend to do this
            //        2 == this can lead to problems in the future.
            //        3 == this can lead to performance problems
            //        4 == this is just not right

            string message = CssStrings.ResourceManager.GetString(error.ToString(), CssStrings.Culture).FormatInvariant(args);
            OnScannerError(new CssScannerException(
                (int)error,
                severity,
                m_context.End.Line,
                m_context.End.Char,
                message
                ));
        }

        public event EventHandler<CssScannerErrorEventArgs> ScannerError;

        protected void OnScannerError(CssScannerException exc)
        {
            if (ScannerError != null)
            {
                ScannerError(this, new CssScannerErrorEventArgs(exc));
            }
        }

        public event EventHandler<CssScannerContextChangeEventArgs> ContextChange;

        protected void OnContextChange(string fileContext, int line, int column)
        {
            m_context.Reset(line, column);
            if (ContextChange != null)
            {
                ContextChange(this, new CssScannerContextChangeEventArgs(fileContext/*, line, column*/));
            }
        }

        #endregion
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public sealed class CssScannerException : CssException
    {
        private static readonly string s_originator = CssStrings.ScannerSubsystem;

        internal CssScannerException(int error, int severity, int line, int pos, string message)
            : base(error, s_originator, severity, line, pos, message)
        {
        }

        public CssScannerException()
            : base((int)CssErrorCode.UnknownError, s_originator, 1, 0, 0, CssStrings.UnknownError)
        {
        }

        public CssScannerException(string message)
            : base((int)CssErrorCode.UnknownError, s_originator, 1, 0, 0, message)
        {
        }

        public CssScannerException(string message, Exception innerException)
            : base((int)CssErrorCode.UnknownError, s_originator, 1, 0, 0, message, innerException)
        {
        }

#if !SILVERLIGHT
        private CssScannerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }

    internal class CssScannerErrorEventArgs : EventArgs
    {
        public CssScannerException Exception { get; private set; }

        public CssScannerErrorEventArgs(CssScannerException exc)
        {
            Exception = exc;
        }
    }

    internal class CssScannerContextChangeEventArgs : EventArgs
    {
        public string FileContext {get; private set;}
        //public int Line {get; private set;}
        //public int Column {get; private set;}

        public CssScannerContextChangeEventArgs(string fileContext/*, int line, int column*/)
        {
            FileContext = fileContext;
            //Line = line;
            //Column = column;
        }
    }
}