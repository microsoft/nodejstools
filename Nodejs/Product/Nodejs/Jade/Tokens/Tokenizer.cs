// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Jade
{
    internal abstract class Tokenizer<T> : ITokenizer<T> where T : ITextRange
    {
        protected bool CComments { get; set; }
        protected bool CppComments { get; set; }
        protected bool MultilineCppComments { get; set; }
        protected bool SingleQuotedStrings { get; set; }
        protected bool DoubleQuotedStrings { get; set; }

        protected CharacterStream _cs { get; set; }
        protected TextRangeCollection<T> Tokens { get; set; }

        protected Tokenizer()
        {
            this.CComments = true;
            this.CppComments = true;
            this.MultilineCppComments = false;
            this.SingleQuotedStrings = true;
            this.DoubleQuotedStrings = true;
        }

        public ReadOnlyTextRangeCollection<T> Tokenize(ITextProvider textProvider, int start, int length)
        {
            return Tokenize(textProvider, start, length, false);
        }

        public virtual ReadOnlyTextRangeCollection<T> Tokenize(ITextProvider textProvider, int start, int length, bool excludePartialTokens)
        {
            Debug.Assert(start >= 0 && length >= 0 && start + length <= textProvider.Length);

            this._cs = new CharacterStream(textProvider);
            this._cs.Position = start;

            this.Tokens = new TextRangeCollection<T>();

            while (!this._cs.IsEndOfStream())
            {
                // Keep on adding tokens...
                AddNextToken();

                if (this._cs.Position >= start + length)
                {
                    break;
                }
            }

            if (excludePartialTokens)
            {
                var end = start + length;

                // Exclude tokens that are beyond the specified range
                int i;
                for (i = this.Tokens.Count - 1; i >= 0; i--)
                {
                    if (this.Tokens[i].End <= end)
                    {
                        break;
                    }
                }

                i++;

                if (i < this.Tokens.Count)
                {
                    this.Tokens.RemoveRange(i, this.Tokens.Count - i);
                }
            }

            var collection = new ReadOnlyTextRangeCollection<T>(this.Tokens);
            this.Tokens = null;

            return collection;
        }

        protected abstract T GetCommentToken(int start, int length);
        protected abstract T GetStringToken(int start, int length);

        protected virtual bool AddNextToken()
        {
            SkipWhiteSpace();

            if (this._cs.IsEndOfStream())
            {
                return true;
            }

            switch (this._cs.CurrentChar)
            {
                case '\'':
                    if (this.SingleQuotedStrings)
                    {
                        HandleString();
                        return true;
                    }
                    break;

                case '\"':
                    if (this.DoubleQuotedStrings)
                    {
                        HandleString();
                        return true;
                    }
                    break;

                case '/':
                    if (this._cs.NextChar == '/' && this.CppComments)
                    {
                        HandleCppComment();
                        return true;
                    }
                    else if (this._cs.NextChar == '*' && this.CComments)
                    {
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
        protected virtual bool HandleComment(bool multiline)
        {
            if (this._cs.CurrentChar == '/')
            {
                if (this.CComments && this._cs.NextChar == '*')
                {
                    HandleCComment();
                    return false;
                }
                else if (this.CppComments && this._cs.NextChar == '/')
                {
                    return HandleCppComment(multiline);
                }
            }

            return false;
        }

        /// <summary>
        /// Processes C++ style comments (//)
        /// </summary>
        protected bool HandleCppComment()
        {
            return HandleCppComment(false);
        }

        /// <summary>
        /// Processes C++ style comments (//)
        /// </summary>
        /// <returns>True if comment included new line characters</returns>
        protected bool HandleCppComment(bool multiline = false)
        {
            // SaSS version can span more than one line like this (indented):
            //
            // // This comment will not appear in the CSS output.
            //      This is nested beneath the comment as well,
            //      so it also won't appear.

            var start = this._cs.Position;
            var baseIndent = 0;

            if (this.MultilineCppComments)
            {
                baseIndent = CalculateLineIndent();
                multiline = true;

                // only standalone // comments can span more than one line
                for (var i = this._cs.Position - 1; i >= 0; i--)
                {
                    var ch = this._cs[i];

                    if (ch == '\r' || ch == '\n')
                    {
                        break;
                    }

                    if (!char.IsWhiteSpace(ch))
                    {
                        multiline = false;
                        break;
                    }
                }
            }
            else
            {
                multiline = false;
            }

            this._cs.Advance(2); // skip over //

            while (!this._cs.IsEndOfStream())
            {
                var eolPosition = this._cs.Position;

                if (this._cs.IsAtNewLine())
                {
                    if (multiline)
                    {
                        // skip '\r'
                        this._cs.MoveToNextChar();

                        // Skip '\n' 
                        if (this._cs.IsAtNewLine())
                        {
                            this._cs.MoveToNextChar();
                        }

                        SkipToNonWhiteSpaceOrEndOfLine();
                        if (this._cs.IsEndOfStream())
                        {
                            this._cs.Position = eolPosition;
                            break;
                        }

                        var lineIndent = CalculateLineIndent();
                        if (lineIndent <= baseIndent)
                        {
                            // Ignore empty lines, they do not break current block
                            if (lineIndent == 0 && this._cs.IsAtNewLine())
                            {
                                continue;
                            }
                            else
                            {
                                this._cs.Position = eolPosition;
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                this._cs.MoveToNextChar();
            }

            var length = this._cs.Position - start;
            if (length > 0)
            {
                this.Tokens.Add(GetCommentToken(start, length));
            }

            return true;
        }

        /// <summary>
        /// Processes C-style comments (/* */)
        /// </summary>
        /// <returns>True if comment includes new line characters</returns>
        protected virtual void HandleCComment()
        {
            var start = this._cs.Position;

            this._cs.Advance(2);

            while (!this._cs.IsEndOfStream())
            {
                if (this._cs.CurrentChar == '*' && this._cs.NextChar == '/')
                {
                    this._cs.Advance(2);
                    break;
                }

                this._cs.MoveToNextChar();
            }

            var length = this._cs.Position - start;

            if (length > 0)
            {
                this.Tokens.Add(GetCommentToken(start, length));
            }
        }

        /// <summary>
        /// Handles single or double quoted strings
        /// </summary>
        protected virtual ITextRange HandleString(bool addToken = true)
        {
            var start = this._cs.Position;
            var quote = this._cs.CurrentChar;

            // since the escape char is exactly the string openning char we say we start in escaped mode
            // it will get reset by the first char regardless what it is, but it will keep the '' case honest
            this._cs.MoveToNextChar();

            while (!this._cs.IsEndOfStream() && !this._cs.IsAtNewLine())
            {
                if (this._cs.CurrentChar == '\\' && this._cs.NextChar == quote)
                {
                    this._cs.Advance(2);
                }

                if (this._cs.CurrentChar == quote)
                {
                    this._cs.MoveToNextChar();
                    break;
                }

                this._cs.MoveToNextChar();
            }

            var range = TextRange.FromBounds(start, this._cs.Position);
            if (range.Length > 0)
            {
                this.Tokens.Add(GetStringToken(start, range.Length));
            }

            return range;
        }

        /// <summary>
        /// Collects all characters up to the next whitespace
        /// </summary>
        /// <returns>Sequence range</returns>
        protected ITextRange GetNonWSSequence()
        {
            return GetNonWSSequence('\0', inclusive: false);
        }

        /// <summary>
        /// Collects all characters up to the next whitespace
        /// </summary>
        /// <param name="terminator">Terminator character</param>
        /// <param name="inclusive">True if sequence includes the terminator, 
        /// false if advance should stop at the terminator character</param>
        /// <returns>Sequence range</returns>
        protected virtual ITextRange GetNonWSSequence(char terminator, bool inclusive)
        {
            var start = this._cs.Position;

            while (!this._cs.IsEndOfStream() && !this._cs.IsWhiteSpace())
            {
                if (this._cs.CurrentChar == terminator && terminator != '\0')
                {
                    if (inclusive)
                    {
                        this._cs.MoveToNextChar();
                    }

                    break;
                }

                this._cs.MoveToNextChar();
            }

            return TextRange.FromBounds(start, this._cs.Position);
        }

        /// <summary>
        /// Collects all characters up to the next whitespace always
        /// including the current character
        /// </summary>
        /// <returns>Sequence range</returns>
        protected ITextRange GetNonWSSequence(string terminators)
        {
            var start = this._cs.Position;

            this._cs.MoveToNextChar();

            while (!this._cs.IsEndOfStream() && !this._cs.IsWhiteSpace())
            {
                if (terminators.IndexOf(this._cs.CurrentChar) != -1)
                {
                    return TextRange.FromBounds(start, this._cs.Position);
                }

                this._cs.MoveToNextChar();
            }

            return TextRange.FromBounds(start, this._cs.Position);
        }

        /// <summary>
        /// Collects 'identifier' sequence. Identifier consists of ANSI characters and decimal digits.
        /// </summary>
        /// <returns>Identifier range</returns>
        protected virtual ITextRange ParseIdentifier()
        {
            var start = this._cs.Position;

            while (!this._cs.IsEndOfStream() && !this._cs.IsWhiteSpace() &&
                  (this._cs.IsAnsiLetter() || this._cs.IsDecimal() || this._cs.CurrentChar == '_'))
            {
                this._cs.MoveToNextChar();
            }

            return TextRange.FromBounds(start, this._cs.Position);
        }

        /// <summary>
        /// Determines amount of leading whitespace in the current line
        /// </summary>
        protected int CalculateLineIndent()
        {
            var baseIndent = -1;

            // Find base tag indent
            for (var pos = this._cs.Position - 1; pos >= 0 && baseIndent < 0; pos--)
            {
                if (this._cs[pos] == '\n' || this._cs[pos] == '\r')
                {
                    pos++;
                    for (var j = pos; j < this._cs.Position + 1; j++)
                    {
                        if (j == this._cs.Position || !char.IsWhiteSpace(this._cs[j]))
                        {
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
        protected virtual bool SkipWhiteSpace()
        {
            var newLine = false;

            while (!this._cs.IsEndOfStream())
            {
                if (!this._cs.IsWhiteSpace())
                {
                    break;
                }

                if (this._cs.IsAtNewLine())
                {
                    newLine = true;
                }

                this._cs.MoveToNextChar();
            }

            return newLine;
        }

        /// <summary>
        /// Advances character stream to the next end of line.
        /// </summary>
        protected virtual void SkipToEndOfLine()
        {
            while (!this._cs.IsEndOfStream() && !this._cs.IsAtNewLine())
            {
                this._cs.MoveToNextChar();
            }
        }

        /// <summary>
        /// Advances character stream to the next end of line, comment or end of file.
        /// </summary>
        protected virtual void SkipToEndOfLineOrComment()
        {
            while (!this._cs.IsEndOfStream() && !this._cs.IsAtNewLine() && !IsAtComment())
            {
                this._cs.MoveToNextChar();
            }
        }

        /// <summary>
        /// Advances character stream to the next whitespace, comment or end of file.
        /// </summary>
        protected virtual void SkipToWhiteSpaceOrComment()
        {
            while (!this._cs.IsEndOfStream())
            {
                if (this._cs.IsWhiteSpace() || IsAtComment())
                {
                    break;
                }

                this._cs.MoveToNextChar();
            }
        }

        /// <summary>
        /// Advances character stream to the next whitespace
        /// </summary>
        protected virtual void SkipToWhiteSpace()
        {
            while (!this._cs.IsEndOfStream())
            {
                if (this._cs.IsWhiteSpace())
                {
                    break;
                }

                this._cs.MoveToNextChar();
            }
        }

        /// <summary>
        /// Advances character stream to the next non-whitespace character
        /// </summary>
        protected virtual void SkipToNonWhiteSpaceOrEndOfLine()
        {
            while (!this._cs.IsEndOfStream())
            {
                if (!this._cs.IsWhiteSpace() || this._cs.IsAtNewLine())
                {
                    break;
                }

                this._cs.MoveToNextChar();
            }
        }

        /// <summary>
        /// Checks if character stream is at a comment sequence (// or /*)
        /// </summary>
        protected virtual bool IsAtComment()
        {
            return (this._cs.CurrentChar == '/' && ((this._cs.NextChar == '/' && this.CppComments) || (this._cs.NextChar == '*' && this.CComments)));
        }

        /// <summary>
        /// Checks if character stream is at a string (' or ")
        /// </summary>
        protected virtual bool IsAtString()
        {
            return ((this._cs.CurrentChar == '\'' && this.SingleQuotedStrings) || (this._cs.CurrentChar == '\"' && this.DoubleQuotedStrings));
        }

        /// <summary>
        /// Determines if remaning part of the line is all whitespace
        /// </summary>
        protected virtual bool IsAllWhiteSpaceBeforeEndOfLine(int position)
        {
            var allWS = true;

            for (var i = position; i < this._cs.Length; i++)
            {
                var ch = this._cs[i];

                if (ch == '\r' || ch == '\n')
                {
                    break;
                }

                if (!char.IsWhiteSpace(ch))
                {
                    allWS = false;
                    break;
                }
            }

            return allWS;
        }
    }
}
