/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI {
    class NpmSearchParser : INpmSearchParser {

        private enum NextToken {
            Name = 0,
            Description,
            Author,
            DateTime,
            Version,
            Keywords
        }

        private INpmSearchLexer _lexer;
        private NodeModuleBuilder _builder;
        private NextToken _nextToken = NextToken.Name;

        public NpmSearchParser(INpmSearchLexer lexer) {
            _lexer = lexer;
            _lexer.Token += _lexer_Token;
        }

        void _lexer_Token(object sender, TokenEventArgs e) {
            if (null == _builder) {
                _builder = new NodeModuleBuilder();
            }

            if ((e.Flags & TokenFlags.Newline) == TokenFlags.Newline
                || (e.Flags & TokenFlags.ThatsAllFolks) == TokenFlags.ThatsAllFolks) {
                if (!string.IsNullOrEmpty(_builder.Name)) {
                    OnPackage(_builder.Build());
                }

                _builder.Reset();
                _nextToken = NextToken.Name;
            } else {
                switch (_nextToken) {
                    case NextToken.Name:
                        if ((e.Flags & TokenFlags.Whitespace) != TokenFlags.Whitespace) {
                            _builder.Name = e.Value;
                            _nextToken = NextToken.Description;
                        }
                        break;
                    case NextToken.Description:
                        if (e.LeadingEqualsCount != 1 || e.Value.Length == 1) {
                            _builder.AppendToDescription(e.Value);
                        } else {
                            _builder.AddAuthor(e.Value.Substring(1));
                            _nextToken = NextToken.Author;
                        }
                        break;
                    case NextToken.Author:
                        if (e.LeadingEqualsCount == 1) {
                            if (e.Value.Length > 1) {
                                _builder.AddAuthor(e.Value.Substring(1));
                            }
                        } else if ((e.Flags & TokenFlags.Digits) == TokenFlags.Digits
                                   && ((e.Flags & TokenFlags.Dashes) == TokenFlags.Dashes)) {
                            _builder.AppendToDate(e.Value);
                            _nextToken = NextToken.DateTime;
                        }
                        break;
                    case NextToken.DateTime:
                        if ((e.Flags & TokenFlags.Digits) == TokenFlags.Digits
                            && (e.Flags & TokenFlags.Colons) == TokenFlags.Colons) {
                            _builder.AppendToDate(e.Value);
                            _nextToken = NextToken.Version;
                        }
                        break;
                    case NextToken.Version:
                        if ((e.Flags & TokenFlags.Digits) == TokenFlags.Digits
                            && (e.Flags & TokenFlags.Dots) == TokenFlags.Dots) {
                            try {
                                _builder.Version = SemverVersion.Parse(e.Value);
                            } catch (SemverVersionFormatException) {
                                _builder.Version = new SemverVersion();
                                _builder.AddKeyword(e.Value);
                            }
                            _nextToken = NextToken.Keywords;
                        } else if ((e.Flags & TokenFlags.Whitespace) == TokenFlags.None) {
                            _builder.AddKeyword(e.Value);
                            _nextToken = NextToken.Keywords;
                        }
                        break;
                    case NextToken.Keywords:
                        if ((e.Flags & TokenFlags.Whitespace) == TokenFlags.None) {
                            _builder.AddKeyword(e.Value);
                        }
                        break;
                    default:
                        throw new InvalidOperationException(
                            string.Format("npm search results parser is in an invalid token expectation state: {0}", _nextToken));
                }
            }
        }

        public event EventHandler<PackageEventArgs> Package;

        private void OnPackage(IPackage package) {
            var handlers = Package;
            if (null != handlers) {
                handlers(this, new PackageEventArgs(package));
            }
        }
    }
}
