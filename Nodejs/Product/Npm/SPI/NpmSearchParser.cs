using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    class NpmSearchParser : INpmSearchParser{

        private enum NextToken
        {
            Name = 0,
            Description,
            Author,
            Date,
            Version,
            Keywords
        }

        private INpmSearchLexer _lexer;
        private NodeModuleBuilder _builder;
        private NextToken _nextToken = NextToken.Name;

        public NpmSearchParser(INpmSearchLexer lexer){
            _lexer = lexer;
            _lexer.Token += _lexer_Token;
        }

        void _lexer_Token(object sender, TokenEventArgs e)
        {
            if (null == _builder){
                _builder = new NodeModuleBuilder();
            }

            if ((e.Flags & TokenFlags.Newline) == TokenFlags.Newline){
                if (!string.IsNullOrEmpty(_builder.Name)){
                    OnPackage(_builder.Build());
                }

                _builder.Reset();
                _nextToken = NextToken.Name;
            } else{
                switch (_nextToken){
                    case NextToken.Name:
                        if ((e.Flags & TokenFlags.Whitespace) != TokenFlags.Whitespace){
                            _builder.Name = e.Value;
                            _nextToken = NextToken.Description;
                        }
                        break;
                    case NextToken.Description:
                        if (e.LeadingEqualsCount != 1 || e.Value.Length == 1){
                            _builder.AppendToDescription(e.Value);
                        } else{
                            _builder.AddAuthor(e.Value);
                            _nextToken = NextToken.Author;
                        }
                        break;
                    case NextToken.Author:

                        break;
                    case NextToken.Date:
                        break;
                    case NextToken.Version:
                        break;
                    case NextToken.Keywords:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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
