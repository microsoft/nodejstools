//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalysisTests {
    [TestClass]
    public class ScannerTests {
        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestPartialScanning() {
            var code1 = "/* hello world ";
            var code2 = "   goodbye */";
            CollectingErrorSink errorSink = new CollectingErrorSink(true);
            var scanner = new JSScanner(code1, errorSink, new CodeSettings() { AllowShebangLine = true });
            var tokens = scanner.ReadTokens(Int32.MaxValue);
            VerifyTokens(
                tokens,
                new TokenInfo(JSToken.MultipleLineComment, new SourceLocation(0, 1, 1), new SourceLocation(15, 1, 16))
            );

            scanner.Initialize(code2, scanner.CurrentState, new SourceLocation(code1.Length, 2, 1));

            tokens = scanner.ReadTokens(Int32.MaxValue);
            VerifyTokens(
                tokens,
                new TokenInfo(JSToken.MultipleLineComment, new SourceLocation(15, 2, 16), new SourceLocation(28, 2, 29))
            );

            Assert.IsTrue(scanner.CurrentState.Equals(scanner.CurrentState));
            Assert.IsFalse(scanner.CurrentState.Equals(new object()));
            Assert.AreEqual(scanner.CurrentState.GetHashCode(), 2);
        }

        [TestMethod, Priority(0), TestCategory("UnitTest"), TestCategory("Ignore")]
        public void TestOperators() {
            var code = @"x %= 1
x &= 1
x *= 1
x += 1
x -= 1
x <<= 1
x >>= 1
x ^= 1
x |= 1
x /= 1
";

            VerifyTokens(
                ScanTokens(code),
                new TokenInfo(JSToken.Identifier, new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(1, 1, 2), new SourceLocation(2, 1, 3)),
                new TokenInfo(JSToken.ModuloAssign, new SourceLocation(2, 1, 3), new SourceLocation(4, 1, 5)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(4, 1, 5), new SourceLocation(5, 1, 6)),
                new TokenInfo(JSToken.IntegerLiteral, new SourceLocation(5, 1, 6), new SourceLocation(6, 1, 7)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(6, 1, 7), new SourceLocation(8, 2, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(8, 2, 1), new SourceLocation(9, 2, 2)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(9, 2, 2), new SourceLocation(10, 2, 3)),
                new TokenInfo(JSToken.BitwiseAndAssign, new SourceLocation(10, 2, 3), new SourceLocation(12, 2, 5)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(12, 2, 5), new SourceLocation(13, 2, 6)),
                new TokenInfo(JSToken.IntegerLiteral, new SourceLocation(13, 2, 6), new SourceLocation(14, 2, 7)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(14, 2, 7), new SourceLocation(16, 3, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(16, 3, 1), new SourceLocation(17, 3, 2)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(17, 3, 2), new SourceLocation(18, 3, 3)),
                new TokenInfo(JSToken.MultiplyAssign, new SourceLocation(18, 3, 3), new SourceLocation(20, 3, 5)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(20, 3, 5), new SourceLocation(21, 3, 6)),
                new TokenInfo(JSToken.IntegerLiteral, new SourceLocation(21, 3, 6), new SourceLocation(22, 3, 7)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(22, 3, 7), new SourceLocation(24, 4, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(24, 4, 1), new SourceLocation(25, 4, 2)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(25, 4, 2), new SourceLocation(26, 4, 3)),
                new TokenInfo(JSToken.PlusAssign, new SourceLocation(26, 4, 3), new SourceLocation(28, 4, 5)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(28, 4, 5), new SourceLocation(29, 4, 6)),
                new TokenInfo(JSToken.IntegerLiteral, new SourceLocation(29, 4, 6), new SourceLocation(30, 4, 7)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(30, 4, 7), new SourceLocation(32, 5, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(32, 5, 1), new SourceLocation(33, 5, 2)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(33, 5, 2), new SourceLocation(34, 5, 3)),
                new TokenInfo(JSToken.MinusAssign, new SourceLocation(34, 5, 3), new SourceLocation(36, 5, 5)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(36, 5, 5), new SourceLocation(37, 5, 6)),
                new TokenInfo(JSToken.IntegerLiteral, new SourceLocation(37, 5, 6), new SourceLocation(38, 5, 7)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(38, 5, 7), new SourceLocation(40, 6, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(40, 6, 1), new SourceLocation(41, 6, 2)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(41, 6, 2), new SourceLocation(42, 6, 3)),
                new TokenInfo(JSToken.LeftShiftAssign, new SourceLocation(42, 6, 3), new SourceLocation(45, 6, 6)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(45, 6, 6), new SourceLocation(46, 6, 7)),
                new TokenInfo(JSToken.IntegerLiteral, new SourceLocation(46, 6, 7), new SourceLocation(47, 6, 8)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(47, 6, 8), new SourceLocation(49, 7, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(49, 7, 1), new SourceLocation(50, 7, 2)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(50, 7, 2), new SourceLocation(51, 7, 3)),
                new TokenInfo(JSToken.RightShiftAssign, new SourceLocation(51, 7, 3), new SourceLocation(54, 7, 6)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(54, 7, 6), new SourceLocation(55, 7, 7)),
                new TokenInfo(JSToken.IntegerLiteral, new SourceLocation(55, 7, 7), new SourceLocation(56, 7, 8)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(56, 7, 8), new SourceLocation(58, 8, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(58, 8, 1), new SourceLocation(59, 8, 2)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(59, 8, 2), new SourceLocation(60, 8, 3)),
                new TokenInfo(JSToken.BitwiseXorAssign, new SourceLocation(60, 8, 3), new SourceLocation(62, 8, 5)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(62, 8, 5), new SourceLocation(63, 8, 6)),
                new TokenInfo(JSToken.IntegerLiteral, new SourceLocation(63, 8, 6), new SourceLocation(64, 8, 7)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(64, 8, 7), new SourceLocation(66, 9, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(66, 9, 1), new SourceLocation(67, 9, 2)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(67, 9, 2), new SourceLocation(68, 9, 3)),
                new TokenInfo(JSToken.BitwiseOrAssign, new SourceLocation(68, 9, 3), new SourceLocation(70, 9, 5)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(70, 9, 5), new SourceLocation(71, 9, 6)),
                new TokenInfo(JSToken.IntegerLiteral, new SourceLocation(71, 9, 6), new SourceLocation(72, 9, 7)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(72, 9, 7), new SourceLocation(74, 10, 1)),
                new TokenInfo(JSToken.Identifier, new SourceLocation(74, 10, 1), new SourceLocation(75, 10, 2)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(75, 10, 2), new SourceLocation(76, 10, 3)),
                new TokenInfo(JSToken.DivideAssign, new SourceLocation(76, 10, 3), new SourceLocation(78, 10, 5)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(78, 10, 5), new SourceLocation(79, 10, 6)),
                new TokenInfo(JSToken.IntegerLiteral, new SourceLocation(79, 10, 6), new SourceLocation(80, 10, 7)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(80, 10, 7), new SourceLocation(82, 11, 1))
        );
        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestNumericLiteral() {
            var code = @".123";

            VerifyTokens(
                ScanTokens(
                    code
                ),
                new TokenInfo(JSToken.NumericLiteral, new SourceLocation(0, 1, 1), new SourceLocation(4, 1, 5))
            );

        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestIllegalEscape() {
            var code = "\\while";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.IllegalChar, true, new IndexSpan(0, 1))
                ),
                new TokenInfo(JSToken.None, new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)),
                new TokenInfo(JSToken.While, new SourceLocation(1, 1, 2), new SourceLocation(6, 1, 7))
            );

        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestIllegalEscapeRead() {
            var code = "\\while";

            VerifyTokens(
                ReadTokens(
                    code,
                    new ErrorInfo(JSError.IllegalChar, true, new IndexSpan(0, 1))
                ),
                new TokenInfo(JSToken.None, new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)),
                new TokenInfo(JSToken.While, new SourceLocation(1, 1, 2), new SourceLocation(6, 1, 7))
            );

        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestLineTerminators() {
            var code = "\u2028 \u2029";

            VerifyTokens(
                ScanTokens(code),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(0, 1, 1), new SourceLocation(1, 2, 1)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(1, 2, 1), new SourceLocation(2, 2, 2)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(2, 2, 2), new SourceLocation(3, 3, 1))
            );

        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestUnicodeIdentifiers() {
            var code = "\u0257abc";

            VerifyTokens(
                ScanTokens(code),
                new TokenInfo(JSToken.Identifier, new SourceLocation(0, 1, 1), new SourceLocation(4, 1, 5))
            );

        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestUnicodeWhiteSpace() {
            var code = "\u00a0\u00a0";

            VerifyTokens(
                ScanTokens(code),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(0, 1, 1), new SourceLocation(2, 1, 3))
            );

        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestUnicodeIllegalCharacter() {
            var code = "`";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.IllegalChar, true, new IndexSpan(0, 1))
                ),
                new TokenInfo(JSToken.Error, new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2))
            );

        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestRegularExpressionLiteral() {
            var code = @"x = /foo/";

            VerifyTokens(
                ScanTokens(code),
                new TokenInfo(JSToken.Identifier, new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(1, 1, 2), new SourceLocation(2, 1, 3)),
                new TokenInfo(JSToken.Assign, new SourceLocation(2, 1, 3), new SourceLocation(3, 1, 4)),
                new TokenInfo(JSToken.WhiteSpace, new SourceLocation(3, 1, 4), new SourceLocation(4, 1, 5)),
                new TokenInfo(JSToken.RegularExpression, new SourceLocation(4, 1, 5), new SourceLocation(9, 1, 10))
            );
        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestBadNumericLiteral() {
            var code = @"1Z";

            VerifyTokens(
                ScanTokens(
                    code,
                    true,
                    new ErrorInfo(JSError.BadNumericLiteral, true, new IndexSpan(0, 2))
                ),
                new TokenInfo(JSToken.NumericLiteral, new SourceLocation(0, 1, 1), new SourceLocation(2, 1, 3))
            );

            code = @"1ZZ";

            VerifyTokens(
                ScanTokens(
                    code,
                    true,
                    new ErrorInfo(JSError.BadNumericLiteral, true, new IndexSpan(0, 3))
                ),
                new TokenInfo(JSToken.NumericLiteral, new SourceLocation(0, 1, 1), new SourceLocation(3, 1, 4))
            );
        }
        
        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestOctalStringLiterals() {
            var code = @"'\10'";

            VerifyTokens(
                ScanTokens(
                    code,
                    true,
                    new ErrorInfo(JSError.OctalLiteralsDeprecated, false, new IndexSpan(0, 4))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(5, 1, 6))
            );

            code = @"'\70'";

            VerifyTokens(
                ScanTokens(
                    code,
                    true,
                    new ErrorInfo(JSError.OctalLiteralsDeprecated, false, new IndexSpan(0, 4))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(5, 1, 6))
            );

            code = @"'\100'";

            VerifyTokens(
                ScanTokens(
                    code,
                    true,
                    new ErrorInfo(JSError.OctalLiteralsDeprecated, false, new IndexSpan(0, 5))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(6, 1, 7))
            );

            code = @"'\1'";

            VerifyTokens(
                ScanTokens(
                    code,
                    true,
                    new ErrorInfo(JSError.OctalLiteralsDeprecated, false, new IndexSpan(0, 3))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(4, 1, 5))
            );

            code = @"'\4'";

            VerifyTokens(
                ScanTokens(
                    code,
                    true,
                    new ErrorInfo(JSError.OctalLiteralsDeprecated, false, new IndexSpan(0, 3))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(4, 1, 5))
            );
        }

        [TestMethod, Priority(0), TestCategory("UnitTest"), TestCategory("Ignore")]
        public void TestUnterminatedString() {
            var code = @"'abc
";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.UnterminatedString, true, new IndexSpan(0, 6))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(4, 1, 5)),
                new TokenInfo(JSToken.EndOfLine, new SourceLocation(4, 1, 5), new SourceLocation(6, 2, 1))
            );

            code = @"'abc";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.UnterminatedString, true, new IndexSpan(0, 4))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(4, 1, 5))
            );
        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestHexEscape() {
            var code = "'\\xAA'";

            VerifyTokens(
                ScanTokens(
                    code
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(6, 1, 7))
            );
        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestHexEscape2() {
            var code = "'\\xaa'";

            VerifyTokens(
                ScanTokens(
                    code
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(6, 1, 7))
            );
        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestInvalidHexEscape() {
            var code = "'\\xZZ'";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.BadHexEscapeSequence, true, new IndexSpan(0, 5))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(6, 1, 7))
            );

            code = "'\\xZ'";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.BadHexEscapeSequence, true, new IndexSpan(0, 4))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(5, 1, 6))
            );

            code = "'\\x'";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.BadHexEscapeSequence, true, new IndexSpan(0, 3))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(4, 1, 5))
            );
        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestInvalidUnicodeEscape() {
            var code = "'\\uZZZZ'";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.BadHexEscapeSequence, true, new IndexSpan(0, 7))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(8, 1, 9))
            );

            code = "'\\uZZZ'";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.BadHexEscapeSequence, true, new IndexSpan(0, 6))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(7, 1, 8))
            );

            code = "'\\uZZ'";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.BadHexEscapeSequence, true, new IndexSpan(0, 5))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(6, 1, 7))
            );

            code = "'\\uZ'";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.BadHexEscapeSequence, true, new IndexSpan(0, 4))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(5, 1, 6))
            );

            code = "'\\u'";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.BadHexEscapeSequence, true, new IndexSpan(0, 3))
                ),
                new TokenInfo(JSToken.StringLiteral, new SourceLocation(0, 1, 1), new SourceLocation(4, 1, 5))
            );
        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void TestIllegalCharacter() {
            var code = "\0\0";

            VerifyTokens(
                ScanTokens(
                    code,
                    new ErrorInfo(JSError.IllegalChar, true, new IndexSpan(0, 1))
                ),
                new TokenInfo(JSToken.Error, new SourceLocation(0, 1, 1), new SourceLocation(1, 1, 2))
            );

        }

        [TestMethod, Priority(0), TestCategory("UnitTest"), TestCategory("Ignore")]
        public void TestMultilineComment() {
            var code = @"/*
hello world
*/";

            VerifyTokens(
                ScanTokens(code),
                new TokenInfo(JSToken.MultipleLineComment, new SourceLocation(0, 1, 1), new SourceLocation(19, 3, 3))
            );

        }

        [TestMethod, Priority(0), TestCategory("UnitTest"), TestCategory("Ignore")]
        public void TestMultilineCommentUnterminated() {
            var code = @"/*
hello world
";

            VerifyTokens(
                ScanTokens(code),
                new TokenInfo(JSToken.MultipleLineComment, new SourceLocation(0, 1, 1), new SourceLocation(17, 3, 1))
            );

        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
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

        private static List<TokenWithSpan> ScanTokens(string code, params ErrorInfo[] errors) {
            return ScanTokens(code, false, errors);
        }

        private static List<TokenWithSpan> ScanTokens(string code, bool collectWarnings, params ErrorInfo[] errors) {
            CollectingErrorSink errorSink = new CollectingErrorSink(collectWarnings);
            var scanner = new JSScanner(code, errorSink, new CodeSettings() { AllowShebangLine = true });
            List<TokenWithSpan> tokens = new List<TokenWithSpan>();
            for (TokenWithSpan curToken = scanner.ScanNextTokenWithSpan(true);
                curToken.Token != JSToken.EndOfFile;
                curToken = scanner.ScanNextTokenWithSpan(true)) {
                tokens.Add(curToken);
            }
            errorSink.CheckErrors(errors);
            return tokens;
        }

        private static List<TokenWithSpan> ReadTokens(string code, params ErrorInfo[] errors) {
            return ReadTokens(code, false, errors);
        }

        private static List<TokenWithSpan> ReadTokens(string code, bool collectWarnings, params ErrorInfo[] errors) {
            CollectingErrorSink errorSink = new CollectingErrorSink(collectWarnings);
            var scanner = new JSScanner(code, errorSink, new CodeSettings() { AllowShebangLine = true });
            var tokens = scanner.ReadTokens(Int32.MaxValue);
            errorSink.CheckErrors(errors);
            return tokens;
        }

        private void VerifyTokens(List<TokenWithSpan> tokens, params TokenInfo[] expected) {
            bool success = false;
            try {
                Assert.AreEqual(expected.Length, tokens.Count);
                for (int i = 0; i < expected.Length; i++) {
                    Assert.AreEqual(expected[i].TokenKind, tokens[i].Token);
                    Assert.AreEqual(expected[i].Start.Line, tokens[i].StartLine);
                    Assert.AreEqual(expected[i].Start.Column, tokens[i].StartColumn);
                    Assert.AreEqual(expected[i].Start.Index, tokens[i].Start);
                    Assert.AreEqual(expected[i].End.Line, tokens[i].EndLine);
                    Assert.AreEqual(expected[i].End.Column, tokens[i].EndColumn);
                    Assert.AreEqual(expected[i].End.Index, tokens[i].End);
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
                    token.Start,
                    token.StartLine,
                    token.StartColumn,
                    token.End,
                    token.EndLine,
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
