using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm
{
    public class NpmSearchParserFactory
    {
        public static INpmSearchLexer CreateLexer(){
            return new NpmSearchLexer();
        }

        public static INpmSearchParser CreateParser(INpmSearchLexer lexer){
            return new NpmSearchParser(lexer);
        }
    }
}
