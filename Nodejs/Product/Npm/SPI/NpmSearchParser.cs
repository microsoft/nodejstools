using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    class NpmSearchParser : INpmSearchParser{

        private INpmSearchLexer _lexer;

        public NpmSearchParser(INpmSearchLexer lexer){
            _lexer = lexer;
            _lexer.Token += _lexer_Token;
        }

        void _lexer_Token(object sender, TokenEventArgs e)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<PackageEventArgs> Package;

        private void OnPackage(IPackage package){
            var handlers = Package;
            if (null != handlers){
                handlers(this, new PackageEventArgs(package));
            }
        }
    }
}
