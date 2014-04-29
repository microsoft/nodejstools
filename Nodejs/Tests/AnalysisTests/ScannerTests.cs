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
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalysisTests {
    [TestClass]
    public class ScannerTests {
        [TestMethod]
        public void TestPositions() {
            var code = "one\n\ntwo";

            VerifyTokens(
                ScanTokens(code),
                new TokenInfo(JSToken.Identifier, new SourceLocation(0, 1, 1), new SourceLocation(3, 1, 4)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(3, 1, 4), new SourceLocation(5, 3, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(5, 3, 1), new SourceLocation(8, 3, 4))
            );

            code = "one\r\ntwo";

            VerifyTokens(
                ScanTokens(code),
                new TokenInfo(JSToken.Identifier, new SourceLocation(0, 1, 1), new SourceLocation(3, 1, 4)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(3, 1, 4), new SourceLocation(5, 2, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(5, 2, 1), new SourceLocation(8, 2, 4))
            );

            code = "one\r\rtwo";

            VerifyTokens(
                ScanTokens(code),
                new TokenInfo(JSToken.Identifier, new SourceLocation(0, 1, 1), new SourceLocation(3, 1, 4)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(3, 1, 4), new SourceLocation(5, 3, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(5, 3, 1), new SourceLocation(8, 3, 4))
            );

            code = "// comment\none\n\ntwo";

            VerifyTokens(
                ScanTokens(code),
                new TokenInfo(JSToken.SingleLineComment, new SourceLocation(0, 1, 1), new SourceLocation(10, 1, 11)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(10, 1, 11), new SourceLocation(11, 2, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(11, 2, 1), new SourceLocation(14, 2, 4)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(14, 2, 4), new SourceLocation(16, 4, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(16, 4, 1), new SourceLocation(19, 4, 4))
            );

            code = "#!/use/bin/node\none\n\ntwo";

            VerifyTokens(
                ScanTokens(code),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(15, 1, 16), new SourceLocation(16, 2, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(16, 2, 1), new SourceLocation(19, 2, 4)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(19, 2, 4), new SourceLocation(21, 4, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(21, 4, 1), new SourceLocation(24, 4, 4))
            );
        }

        private static List<TokenWithSpan> ScanTokens(string code) {
            var scanner = new JSScanner(code, null, new CodeSettings() { AllowShebangLine = true });
            List<TokenWithSpan> tokens = new List<TokenWithSpan>();
            for (TokenWithSpan curToken = scanner.ScanNextToken(true);
                curToken.Token != JSToken.EndOfFile;
                curToken = scanner.ScanNextToken(true)) {
                tokens.Add(curToken);
            }
            return tokens;
        }

        private void VerifyTokens(List<TokenWithSpan> tokens, params TokenInfo[] expected) {
            bool success = false;
            try {
                Assert.AreEqual(expected.Length, tokens.Count);
                for (int i = 0; i < expected.Length; i++) {
                    Assert.AreEqual(expected[i].TokenKind, tokens[i].Token);
                    Assert.AreEqual(expected[i].Start.Line, tokens[i].StartLineNumber);
                    Assert.AreEqual(expected[i].Start.Column, tokens[i].StartColumn);
                    Assert.AreEqual(expected[i].Start.Index, tokens[i].StartPosition);
                    Assert.AreEqual(expected[i].End.Line, tokens[i].EndLineNumber);
                    Assert.AreEqual(expected[i].End.Column, tokens[i].EndColumn);
                    Assert.AreEqual(expected[i].End.Index, tokens[i].EndPosition);
                }
                success = true;
            } finally {
                if (!success) {
                    DumpTokens(tokens);
                }
            }
        }

        private static void DumpTokens(List<TokenWithSpan> tokens) {
            foreach (var token in tokens) {
                Console.WriteLine("new TokenInfo(JSToken.{0}, new SourceLocation({1}, {2}, {3}), new SourceLocation({4}, {5}, {6})),",
                    token.Token,
                    token.StartPosition,
                    token.StartLineNumber,
                    token.StartColumn,
                    token.EndPosition,
                    token.EndLineNumber,
                    token.EndColumn
                );
            }
        }

        class TokenInfo {
            public readonly JSToken TokenKind;
            public readonly SourceLocation Start, End;

            public TokenInfo(JSToken tokenKind, SourceLocation start, SourceLocation end) {
                TokenKind = tokenKind;
                Start = start;
                End = end;
            }
        }
    }
}
