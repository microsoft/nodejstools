using System;
using System.IO;

namespace Microsoft.NodejsTools.Npm{
    public interface INpmSearchLexer{
        void Lex(TextReader reader);
        event EventHandler<TokenEventArgs> Token;
    }
}