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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.NodejsTools.Jade
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tokenizer")]
    internal partial class JadeTokenizer : Tokenizer<JadeToken>
    {
        private bool _attributeState = false;
        private readonly ITextBuffer _jsBuffer, _cssBuffer;
        private readonly ITagger<ClassificationTag> _jsTagger;
        private readonly IClassifier _cssClassifier;

        public JadeTokenizer(JadeClassifierProvider provider)
        {
            CComments = false;
            MultilineCppComments = true;
            if (provider != null && provider.JsTaggerProvider != null)
            {
                _jsBuffer = provider.BufferFactoryService.CreateTextBuffer(provider.JsContentType);
                _jsTagger = provider.JsTaggerProvider.CreateTagger<ClassificationTag>(_jsBuffer);
            }
            if (provider != null && provider.CssClassifierProvider != null)
            {
                _cssBuffer = provider.BufferFactoryService.CreateTextBuffer(provider.CssContentType);
                _cssClassifier = provider.CssClassifierProvider.GetClassifier(_cssBuffer);
            }
        }

        #region Tokenizer overrides
        protected override bool AddNextToken()
        {
            SkipWhiteSpace();

            if (_cs.IsEndOfStream())
                return true;

            // C++ style comments must be placed on their own line
            if (IsAtComment())
            {
                HandleCppComment(multiline: true);
                return true;
            }

            // We should always be in the beginning of the line here.
            if (_cs.CurrentChar == '!' && _cs.NextChar == '!' && _cs.LookAhead(2) == '!')
            {
                SkipToEndOfLine();
                return true;
            }

            if (_cs.CurrentChar == ':')
            {
                OnFilter();
                return true;
            }

            if (_cs.CurrentChar == '-' || _cs.CurrentChar == '=')
            {
                // Inline code
                // - if (moo)
                // - else
                OnInlineCode();
                return true;
            }

            if (_cs.CurrentChar == '|')
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
            if (_attributeState)
                return new JadeToken(JadeTokenType.AttributeValue, start, length);

            return new JadeToken(JadeTokenType.String, start, length);
        }
        #endregion

        private void SkipToEndOfBlock(int blockIndent, bool text)
        {
            int indent;

            // Move  to the next line
            while (!_cs.IsEndOfStream())
            {
                SkipToWhiteSpace(); // typically at eol now

                // Skip ws and eol, if any
                if (SkipWhiteSpace() && !_cs.IsEndOfStream())
                {
                    // Check if indentation changed back to base
                    indent = CalculateLineIndent();

                    if (indent <= blockIndent)
                        break;

                    if (text)
                        OnText(strings: false, html: true, entities: true);
                }
            }
        }

        protected override ITextRange HandleString(bool addToken = true)
        {
            int start = _cs.Position;
            char quote = _cs.CurrentChar;

            // since the escape char is exactly the string openning char we say we start in escaped mode
            // it will get reset by the first char regardless what it is, but it will keep the '' case honest
            _cs.MoveToNextChar();

            while (!_cs.IsEndOfStream() && !_cs.IsAtNewLine())
            {
                if (_cs.CurrentChar == '\\' && _cs.NextChar == quote)
                {
                    _cs.Advance(2);
                }

                if (_cs.CurrentChar == quote)
                {
                    _cs.MoveToNextChar();
                    break;
                }

                if (_cs.CurrentChar == '<' && (_cs.NextChar == '/' || Char.IsLetter(_cs.NextChar)))
                {
                    if (_cs.Position > start)
                        Tokens.Add(GetStringToken(start, _cs.Position - start));

                    OnHtml();

                    start = _cs.Position;
                }
                else
                {
                    _cs.MoveToNextChar();
                }
            }

            var range = TextRange.FromBounds(start, _cs.Position);
            if (range.Length > 0)
                Tokens.Add(GetStringToken(start, range.Length));

            return range;
        }

        private ITextRange GetAttribute()
        {
            int start = _cs.Position;

            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace() &&
                  (_cs.IsAnsiLetter() || _cs.IsDecimal() ||
                  _cs.CurrentChar == '_' || _cs.CurrentChar == '-' ||
                  _cs.CurrentChar == ':') || _cs.CurrentChar == '.')
            {
                _cs.MoveToNextChar();
            }

            return TextRange.FromBounds(start, _cs.Position);
        }

        private ITextRange GetAttributeValue()
        {
            int start = _cs.Position;

            while (!_cs.IsEndOfStream() && !_cs.IsWhiteSpace() && _cs.CurrentChar != ')')
            {
                _cs.MoveToNextChar();
            }

            return TextRange.FromBounds(start, _cs.Position);
        }

        private void AddToken(JadeTokenType type, int start, int length)
        {
            var token = new JadeToken(type, start, length);
            Tokens.Add(token);
        }

        private void AddToken(IClassificationType type, int start, int length)
        {
            var token = new JadeToken(JadeTokenType.None, type, start, length);
            Tokens.Add(token);
        }
    }
}
