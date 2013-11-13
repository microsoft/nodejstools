using System.IO;

namespace Microsoft.NodejsTools.Npm{
    public interface INpmSearchLexer{
        void Lex(TextReader reader);
    }
}