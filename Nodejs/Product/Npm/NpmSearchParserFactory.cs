using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public class NpmSearchParserFactory
    {
        public static INpmSearchLexer CreateLexer(){
            throw new NotImplementedException();
        }

        public static INpmSearchParser CreateParser(INpmSearchLexer lexer){
            throw new NotImplementedException();
        }
    }
}
