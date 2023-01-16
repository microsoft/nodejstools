// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.NodejsTools.Jade
{
    internal partial class JadeTokenizer : Tokenizer<JadeToken>
    {
        private bool _attributeState = false;
        private readonly ITextBuffer _jsBuffer, _cssBuffer;
        private readonly ITagger<ClassificationTag> _jsTagger;
        private readonly IClassifier _cssClassifier;

        public JadeTokenizer(JadeClassifierProvider provider)
        {
            this.CComments = false;
            this.MultilineCppComments = true;
            if (provider != null && provider.JsTaggerProvider != null)
            {
                this._jsBuffer = provider.BufferFactoryService.CreateTextBuffer(provider.JsContentType);
                this._jsTagger = provider.JsTaggerProvider.CreateTagger<ClassificationTag>(this._jsBuffer);
            }
            if (provider != null && provider.CssClassifierProvider != null)
            {
                this._cssBuffer = provider.BufferFactoryService.CreateTextBuffer(provider.CssContentType);
                this._cssClassifier = provider.CssClassifierProvider.GetClassifier(this._cssBuffer);
            }
        }

        #region Tokenizer overrides
        protected override bool AddNextToken()
        {
            SkipWhiteSpace();

            if (this._cs.IsEndOfStream())
            {
                return true;
            }

            // C++ style comments must be placed on their own line
            if (IsAtComment())
            {
                HandleCppComment(multiline: true);
                return true;
            }

            // We should always be in the beginning of the line here.
            if (this._cs.CurrentChar == '!' && this._cs.NextChar == '!' && this._cs.LookAhead(2) == '!')
            {
                SkipToEndOfLine();
                return true;
            }

            if (this._cs.CurrentChar == ':')
            {
                OnFilter();
                return true;
            }

            if (this._cs.CurrentChar == '-' || this._cs.CurrentChar == '=')
            {
                // Inline code
                // - if (moo)
                // - else
                OnInlineCode();
                return true;
            }

            if (this._cs.CurrentChar == '|')
            {
                // Text block
                OnText(strings: false, html: true, entities: true);
                return true;
            }

            OnTag();
            return true;
        }

        protected override JadeToken GetCommentToken(int start, int length)
        {
            return new JadeToken(JadeTokenType.Comment, start, length);
        }

        protected override JadeToken GetStringToken(int start, int length)
        {
            if (this._attributeState)
            {
                return new JadeToken(JadeTokenType.AttributeValue, start, length);
            }

            return new JadeToken(JadeTokenType.String, start, length);
        }
        #endregion

        private void SkipToEndOfBlock(int blockIndent, bool text)
        {
            int indent;

            // Move  to the next line
            while (!this._cs.IsEndOfStream())
            {
                SkipToWhiteSpace(); // typically at eol now

                // Skip ws and eol, if any
                if (SkipWhiteSpace() && !this._cs.IsEndOfStream())
                {
                    // Check if indentation changed back to base
                    indent = CalculateLineIndent();

                    if (indent <= blockIndent)
                    {
                        break;
                    }

                    if (text)
                    {
                        OnText(strings: false, html: true, entities: true);
                    }
                }
            }
        }

        protected override ITextRange HandleString(bool addToken = true)
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

                if (this._cs.CurrentChar == '<' && (this._cs.NextChar == '/' || char.IsLetter(this._cs.NextChar)))
                {
                    if (this._cs.Position > start)
                    {
                        this.Tokens.Add(GetStringToken(start, this._cs.Position - start));
                    }

                    OnHtml();

                    start = this._cs.Position;
                }
                else
                {
                    this._cs.MoveToNextChar();
                }
            }

            var range = TextRange.FromBounds(start, this._cs.Position);
            if (range.Length > 0)
            {
                this.Tokens.Add(GetStringToken(start, range.Length));
            }

            return range;
        }

        private ITextRange GetAttribute()
        {
            var start = this._cs.Position;

            while (!this._cs.IsEndOfStream() && !this._cs.IsWhiteSpace() &&
                  (this._cs.IsAnsiLetter() || this._cs.IsDecimal() ||
                  this._cs.CurrentChar == '_' || this._cs.CurrentChar == '-' ||
                  this._cs.CurrentChar == ':') || this._cs.CurrentChar == '.')
            {
                this._cs.MoveToNextChar();
            }

            return TextRange.FromBounds(start, this._cs.Position);
        }

        private ITextRange GetAttributeValue()
        {
            var start = this._cs.Position;

            while (!this._cs.IsEndOfStream() && !this._cs.IsWhiteSpace() && this._cs.CurrentChar != ')')
            {
                this._cs.MoveToNextChar();
            }

            return TextRange.FromBounds(start, this._cs.Position);
        }

        private void AddToken(JadeTokenType type, int start, int length)
        {
            var token = new JadeToken(type, start, length);
            this.Tokens.Add(token);
        }

        private void AddToken(IClassificationType type, int start, int length)
        {
            var token = new JadeToken(JadeTokenType.None, type, start, length);
            this.Tokens.Add(token);
        }
    }
}
