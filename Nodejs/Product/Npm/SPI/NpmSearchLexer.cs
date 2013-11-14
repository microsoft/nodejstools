using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    class NpmSearchLexer : INpmSearchLexer
    {

        private void DiscardFirstLine(TextReader reader){
            int ch;
            do{
                ch = reader.Read();
            } while (ch >= 0 && ch != '\n');
        }

        public void Lex(TextReader reader){
            DiscardFirstLine(reader);

            var buff = new StringBuilder();
            var flags = TokenFlags.None;
            var leadingEqualsCount = 0;
            int raw;
            while ((raw = reader.Read()) >= 0){
                var ch = (char) raw;
                if (buff.Length > 0 && (ch =='\n'
                                        ||
                                        char.IsWhiteSpace(ch) && ((flags & TokenFlags.Whitespace) == 0 || (flags & TokenFlags.Newline) == TokenFlags.Newline)
                                        ||
                                        !char.IsWhiteSpace(ch) && ((flags & TokenFlags.Whitespace) == TokenFlags.Whitespace || (flags & TokenFlags.Newline) == TokenFlags.Newline)))
                {
                    OnToken(buff.ToString(), flags, leadingEqualsCount);
                    buff.Length = 0;
                    flags = TokenFlags.None;
                    leadingEqualsCount = 0;
                }

                if (ch == '='){
                    if (buff.Length == leadingEqualsCount){
                        ++leadingEqualsCount;
                        flags |= TokenFlags.LeadingEquals;
                    }
                }
                else if (char.IsDigit(ch)){
                    flags |= TokenFlags.Digits;
                }
                else if (char.IsLetter(ch)){
                    flags |= TokenFlags.Letters;
                }
                else if (char.IsWhiteSpace(ch)){
                    flags |= TokenFlags.Whitespace;
                }
                else if (ch == '.'){
                    flags |= TokenFlags.Dots;
                }
                else if (ch == ':'){
                    flags |= TokenFlags.Colons;
                }
                else if (ch == '-'){
                    flags |= TokenFlags.Dashes;
                }
                else if (ch != '\n'){
                    flags |= TokenFlags.Other;
                }

                if (ch == '\n'){
                    flags |= TokenFlags.Newline;
                }

                buff.Append(ch);
            }

            if (buff.Length > 0){
                OnToken(buff.ToString(), flags, leadingEqualsCount);
            }

            OnToken(null, TokenFlags.ThatsAllFolks, 0);
        }

        public event EventHandler<TokenEventArgs> Token;

        private void OnToken(
            string value,
            TokenFlags flags,
            int leadingEqualsCount){
            var handlers = Token;
            if (null != handlers){
                handlers(this, new TokenEventArgs(value, flags, leadingEqualsCount));
            }
        }
    }
}
