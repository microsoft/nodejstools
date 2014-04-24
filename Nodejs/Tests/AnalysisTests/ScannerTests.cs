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
        }

        private static List<TokenWithSpan> ScanTokens(string code) {
            var scanner = new JSScanner(code);
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
