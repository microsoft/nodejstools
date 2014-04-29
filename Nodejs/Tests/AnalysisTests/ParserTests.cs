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
using System.Linq;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalysisTests {
    [TestClass]
    public class ParserTests {
        /// <summary>

        /// IdentifierName ::
        ///     IdentifierStart
        ///     IdentifierName IdentifierPart
        /// IdentifierStart ::
        ///     UnicodeLetter
        ///     $
        ///     _
        ///     \ UnicodeEscapeSequence
        ///IdentifierPart ::
        ///     IdentifierStart
        ///     UnicodeCombiningMark
        ///     UnicodeDigit
        ///     UnicodeConnectorPunctuation
        ///     \u200C (ZWNJ)
        ///     \u200D (ZWJ)
        /// </summary>
        [TestMethod]
        public void IdentifierNames() {
            const string identifiers = @"
$
$f
_
_f
";
            CheckAst(
                ParseCode(identifiers),
                CheckBlock(
                    CheckLookupStmt("$"),
                    CheckLookupStmt("$f"),
                    CheckLookupStmt("_"),
                    CheckLookupStmt("_f")
                )
            );
        }

        [TestMethod]
        public void TestUnicodeIdentifier() {
            const string identifiers = @"
\u1234abc
\u1234\u1235abc
abc\u1234
";
            CheckAst(
                ParseCode(identifiers),
                CheckBlock(
                    CheckLookupStmt("\u1234abc"),
                    CheckLookupStmt("\u1234\u1235abc"),
                    CheckLookupStmt("abc\u1234")
                )
            );
        }

        [TestMethod]
        public void TestBlock() {
            const string code = @"
{ 1; 2 }";
            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckBlock(
                        CheckConstantStmt(One),
                        CheckConstantStmt(Two)
                    )
                )
            );
        }

        [TestMethod]
        public void TestDebugger() {
            const string code = @"
debugger;";
            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckDebuggerStmt()
                )
            );
        }

        [TestMethod]
        public void TestSwitch() {
            const string code = @"
switch(abc) {
    case 0:
    case ""abc""
        1;
        break;
    default:
        2;
        break;
}";
            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckSwitch(
                        CheckLookup("abc"),
                        CheckCase(
                            Zero,
                            CheckBlock()
                        ),
                        CheckCase(
                            CheckConstant("abc"),
                            CheckBlock(
                                CheckConstantStmt(1.0),
                                CheckBreak()
                            )
                        ),
                        CheckCase(
                            null,
                            CheckBlock(
                                CheckConstantStmt(2.0),
                                CheckBreak()
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void TestThrow() {
            const string code = @"
throw ""abc"";
throw;
";
            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckThrow(CheckConstant("abc")),
                    CheckThrow()
                )
            );
        }

        [TestMethod]
        public void TestWith() {
            const string code = @"
with(abc) {
    1;
}
";
            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckWith(
                        CheckLookup("abc"),
                        CheckBlock(
                            CheckConstantStmt(1.0)
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void TestConditional() {
            const string identifiers = @"
1 ? 2 : 3
";
            CheckAst(
                ParseCode(identifiers),
                CheckBlock(
                    CheckExprStmt(
                        CheckConditional(
                            One,
                            Two,
                            Three
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void TestVoid() {
            const string identifiers = @"
void 1
";
            CheckAst(
                ParseCode(identifiers),
                CheckBlock(
                    CheckExprStmt(
                        CheckUnary(
                            JSToken.Void,
                            One
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void TestTypeOf() {
            const string identifiers = @"
typeof 1
";
            CheckAst(
                ParseCode(identifiers),
                CheckBlock(
                    CheckExprStmt(
                        CheckUnary(
                            JSToken.TypeOf,
                            One
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void TestUnary() {
            const string identifiers = @"
+1;
-1;
~1;
!1;
delete abc;
++abc;
--abc;
";
            CheckAst(
                ParseCode(identifiers),
                CheckBlock(
                    CheckUnaryStmt(JSToken.Plus, One),
                    CheckUnaryStmt(JSToken.Minus, One),
                    CheckUnaryStmt(JSToken.BitwiseNot, One),
                    CheckUnaryStmt(JSToken.LogicalNot, One),
                    CheckUnaryStmt(JSToken.Delete, CheckLookup("abc")),
                    CheckUnaryStmt(JSToken.Increment, CheckLookup("abc")),
                    CheckUnaryStmt(JSToken.Decrement, CheckLookup("abc"))
                )
            );
        }

        [TestMethod]
        public void TestUnaryPostFix() {
            const string identifiers = @"
abc++;
abc--;
";
            CheckAst(
                ParseCode(identifiers),
                CheckBlock(
                    CheckUnaryStmt(JSToken.Increment, CheckLookup("abc"), isPostFix:true),
                    CheckUnaryStmt(JSToken.Decrement, CheckLookup("abc"), isPostFix: true)
                )
            );
        }

        [TestMethod]
        public void TestGrouping() {
            const string identifiers = @"
(1)
";
            CheckAst(
                ParseCode(identifiers),
                CheckBlock(
                    CheckExprStmt(CheckGrouping(One))
                )
            );
        }

        [TestMethod]
        public void TestArrayLiteral() {
            const string identifiers = @"
[1,2,3];
[1,2,3,]
";
            CheckAst(
                ParseCode(identifiers),
                CheckBlock(
                    CheckExprStmt(CheckArrayLiteral(One, Two, Three)),
                    CheckExprStmt(CheckArrayLiteral(One, Two, Three))
                )
            );
        }

        [TestMethod]
        public void TestObjectLiteral() {
            // Expression statements can't start with { as it's ambiguous with block,
            // so we do an assignment here
            const string identifiers = @"
x = {'abc':1}
x = {'abc':1,}
x = {abc:1}
x = {42:1,}
x = {get abc () { 42 }}
x = {set abc (value) { 42 }}
";
            CheckAst(
                ParseCode(identifiers),
                CheckBlock(
                    CheckBinaryStmt(
                        JSToken.Assign,
                        CheckLookup("x"),
                        CheckObjectLiteral(Property("abc", One))
                    ),
                    CheckBinaryStmt(
                        JSToken.Assign,
                        CheckLookup("x"),
                        CheckObjectLiteral(Property("abc", One))
                    ),
                    CheckBinaryStmt(
                        JSToken.Assign,
                        CheckLookup("x"),
                        CheckObjectLiteral(Property("abc", One))
                    ),
                    CheckBinaryStmt(
                        JSToken.Assign,
                        CheckLookup("x"),
                        CheckObjectLiteral(Property(42.0, One))
                    ),
                    CheckBinaryStmt(
                        JSToken.Assign,
                        CheckLookup("abc"),
                        CheckObjectLiteral(
                            GetterSetter(
                                true, 
                                "abc", 
                                CheckFunctionExpr(
                                    CheckFunctionObject(
                                        null,
                                        CheckBlock()
                                    )
                                )
                            )
                        )
                    ),
                    CheckBinaryStmt(
                        JSToken.Assign,
                        CheckLookup("abc"),
                        CheckObjectLiteral(
                            GetterSetter(
                                false,
                                "abc",
                                CheckFunctionExpr(
                                    CheckFunctionObject(
                                        null,
                                        CheckBlock(),
                                        CheckParameterDeclaration("value")
                                    )
                                )
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void TestPrecedence() {
            const string code = @"
1 + 2 * 3;
1 + 2 / 3;
1 + 2 % 3;
1 - 2 * 3;
1 - 2 / 3;
1 - 2 % 3;
1 << 2 + 3;
1 << 2 - 3;
1 >> 2 + 3;
1 >> 2 - 3;
1 >>> 2 + 3;
1 >>> 2 - 3;
1 < 2 >> 3;
1 > 2 >> 3;
1 <= 2 >> 3;
1 >= 2 >> 3;
1 instanceof 2 >> 3;
1 in 2 >> 3;
1 == 2 < 3;
1 != 2 < 3;
1 === 2 < 3;
1 !== 2 < 3;
1 & 2 == 3;
1 ^ 2 & 3;
1 | 2 ^ 3;
1 && 2 | 3
1 || 2 && 3
1 ? 2 : 3 || 4
";
            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckPrecedence(JSToken.Plus, JSToken.Multiply),
                    CheckPrecedence(JSToken.Plus, JSToken.Divide),
                    CheckPrecedence(JSToken.Plus, JSToken.Modulo),
                    CheckPrecedence(JSToken.Minus, JSToken.Multiply),
                    CheckPrecedence(JSToken.Minus, JSToken.Divide),
                    CheckPrecedence(JSToken.Minus, JSToken.Modulo),
                    CheckPrecedence(JSToken.LeftShift, JSToken.Plus),
                    CheckPrecedence(JSToken.LeftShift, JSToken.Minus),
                    CheckPrecedence(JSToken.RightShift, JSToken.Plus),
                    CheckPrecedence(JSToken.RightShift, JSToken.Minus),
                    CheckPrecedence(JSToken.UnsignedRightShift, JSToken.Plus),
                    CheckPrecedence(JSToken.UnsignedRightShift, JSToken.Minus),
                    CheckPrecedence(JSToken.LessThan, JSToken.RightShift),
                    CheckPrecedence(JSToken.GreaterThan, JSToken.RightShift),
                    CheckPrecedence(JSToken.LessThanEqual, JSToken.RightShift),
                    CheckPrecedence(JSToken.GreaterThanEqual, JSToken.RightShift),
                    CheckPrecedence(JSToken.InstanceOf, JSToken.RightShift),
                    CheckPrecedence(JSToken.In, JSToken.RightShift),
                    CheckPrecedence(JSToken.Equal, JSToken.LessThan),
                    CheckPrecedence(JSToken.NotEqual, JSToken.LessThan),
                    CheckPrecedence(JSToken.StrictEqual, JSToken.LessThan),
                    CheckPrecedence(JSToken.StrictNotEqual, JSToken.LessThan),
                    CheckPrecedence(JSToken.BitwiseAnd, JSToken.Equal),
                    CheckPrecedence(JSToken.BitwiseXor, JSToken.BitwiseAnd),
                    CheckPrecedence(JSToken.BitwiseOr, JSToken.BitwiseXor),
                    CheckPrecedence(JSToken.LogicalAnd, JSToken.BitwiseOr),
                    CheckPrecedence(JSToken.LogicalOr, JSToken.BitwiseAnd),
                    CheckExprStmt(
                        CheckConditional(
                            One,
                            Two,
                            CheckBinary(JSToken.LogicalOr, Three, Four)
                        )
                    )
                )
            );
        }

        private static Action<Statement> CheckPrecedence(JSToken lower, JSToken higher) {
            return CheckBinaryStmt(
                lower,
                One,
                CheckBinary(
                    higher,
                    Two,
                    Three
                )
            );
        }

        /// <summary>
        /// NullLiteral
        ///     null
        /// BooleanLiteral
        ///     true false
        /// </summary>
        [TestMethod]
        public void SimpleLiterals() {
            const string identifiers = @"
null
true
false
";
            CheckAst(
                ParseCode(identifiers),
                CheckBlock(
                    CheckConstantStmt(null),
                    CheckConstantStmt(true),
                    CheckConstantStmt(false)
                )
            );
        }

        /// <summary>
        /// NumericLiteral
        ///     DecimalLiteral
        ///     HexIntegerLiteral
        ///     
        /// DecimalLiteral ::
        ///     DecimalIntegerLiteral . DecimalDigitsopt ExponentPartopt
        ///     . DecimalDigits ExponentPartopt
        ///     DecimalIntegerLiteral ExponentPartopt
        /// DecimalIntegerLiteral ::
        ///     0
        ///     NonZeroDigit DecimalDigitsopt
        /// DecimalDigits ::
        ///     DecimalDigit
        ///     DecimalDigits DecimalDigit
        /// DecimalDigit :: one of
        ///     0 1 2 3 4 5 6 7 8 9
        /// NonZeroDigit :: one of
        ///     1 2 3 4 5 6 7 8 9
        /// ExponentPart ::
        ///     ExponentIndicator SignedInteger
        /// ExponentIndicator :: one of
        ///     e E
        /// SignedInteger ::
        ///     DecimalDigits
        ///     + DecimalDigits
        ///     - DecimalDigits
        /// HexIntegerLiteral ::
        ///     0x HexDigit
        ///     0X HexDigit
        ///     HexIntegerLiteral HexDigit
        /// HexDigit :: one of
        ///     0 1 2 3 4 5 6 7 8 9 a b c d e f A B C D E F
        /// </summary>
        [TestMethod]
        public void TestNumericLiterals() {
            const string numbers = @"
0
1
2
3
4
5
6
7
8
9
0.0
1.1
10
0e1
0e+1
0e-1
0x0
0X0
0x1
0x2
0x3
0x4
0x5
0x6
0x7
0x8
0x9
0xA
0xB
0xC
0xD
0xE
0xF
0x10
";
            CheckAst(
                ParseCode(numbers),
                CheckBlock(
                    CheckConstantStmts(
                        0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 0.0,
                        1.1, 10.0, 0e1, 0e+1, 0e-1, 0.0, 0.0, 1.0, 2.0, 3.0,
                        4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0,
                        13.0, 14.0, 15.0, 16.0
                    )
                )
            );
        }

        /// <summary>
        /// StringLiteral ::
        ///     " DoubleStringCharactersopt "
        ///     ' SingleStringCharactersopt '
        /// DoubleStringCharacters ::
        ///     DoubleStringCharacter DoubleStringCharactersopt
        /// SingleStringCharacters ::
        ///     SingleStringCharacter SingleStringCharactersopt
        /// DoubleStringCharacter ::
        ///     SourceCharacter but not one of " or \ or LineTerminator
        ///     \ EscapeSequence
        ///     LineContinuation
        /// SingleStringCharacter ::
        ///     SourceCharacter but not one of ' or \ or LineTerminator
        ///     \ EscapeSequence
        ///     LineContinuation
        /// LineContinuation ::
        ///     \ LineTerminatorSequence
        /// EscapeSequence ::
        ///     CharacterEscapeSequence
        ///     0 [lookahead ∉ DecimalDigit]
        ///     HexEscapeSequence
        ///     UnicodeEscapeSequence
        /// CharacterEscapeSequence ::
        ///     SingleEscapeCharacter
        ///     NonEscapeCharacter
        /// SingleEscapeCharacter :: one of
        ///     ' " \ b f n r t v
        /// NonEscapeCharacter ::
        ///     SourceCharacter but not one of EscapeCharacter or LineTerminator
        /// EscapeCharacter ::
        ///     SingleEscapeCharacter
        ///     DecimalDigit
        ///     x
        ///     u
        /// HexEscapeSequence ::
        ///     x HexDigit HexDigit
        /// UnicodeEscapeSequence ::
        ///     u HexDigit HexDigit HexDigit HexDigit
        /// </summary>
        [TestMethod]
        public void TestStringLiterals() {
            const string strings = @"
""hello""
'hello'
""hel\
lo""
""\""""
'\''
'\\'
""\\""
'\b'
'\f'
'\n'
'\r'
'\t'
'\v'
'\x42'
'\u0042'
'\Z'
";
            CheckAst(
                ParseCode(strings),
                CheckBlock(
                    CheckConstantStmts(
                        "hello", "hello", "hello", "\"", "'",
                        "\\", "\\",
                        "\b", "\f", "\n", "\r", "\t", "\v",
                        "\x42", "\u0042",
                        "Z"
                    )
                )
            );
        }

        /// <summary>
        /// RegularExpressionLiteral ::
        ///     / RegularExpressionBody / RegularExpressionFlags
        /// RegularExpressionBody ::
        ///     RegularExpressionFirstChar RegularExpressionChars
        /// RegularExpressionChars ::
        ///     [empty]
        ///     RegularExpressionChars RegularExpressionChar
        /// RegularExpressionFirstChar ::
        ///     RegularExpressionNonTerminator but not one of * or \ or / or [
        ///     RegularExpressionBackslashSequence
        ///     RegularExpressionClass
        /// RegularExpressionChar ::
        ///     RegularExpressionNonTerminator but not one of \ or / or [
        ///     RegularExpressionBackslashSequence
        ///     RegularExpressionClass
        /// RegularExpressionBackslashSequence ::
        ///     \ RegularExpressionNonTerminator
        /// RegularExpressionNonTerminator ::
        ///     SourceCharacter but not LineTerminator
        /// RegularExpressionClass ::
        ///     [ RegularExpressionClassChars ]
        /// RegularExpressionClassChars ::
        ///     [empty]
        ///     RegularExpressionClassChars RegularExpressionClassChar
        /// RegularExpressionClassChar ::
        ///     RegularExpressionNonTerminator but not one of ] or \
        ///     RegularExpressionBackslashSequence
        /// RegularExpressionFlags ::
        ///     [empty]
        ///     RegularExpressionFlags IdentifierPart
        /// </summary>
        [TestMethod]
        public void TestRegexLiterals() {
            // TODO: More test cases
            const string regexs = @"
/abc/;
/abc/i;
/[abc]/;
/(?:)/
";
            CheckAst(
                ParseCode(regexs),
                CheckBlock(
                    CheckRegExs(
                        "abc",
                        new RegEx("abc", "i"),
                        "[abc]",
                        "(?:)"
                    )
                )
            );
        }

        [TestMethod]
        public void TestVariableDeclaration() {
            const string code = @"
var i = 0, j = 1;
";
            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckVar(
                        CheckVarDecl("i", Zero),
                        CheckVarDecl("j", One)
                    )
                )
            );
        }

        [TestMethod]
        public void TestEmptyStatement() {
            const string code = @"
;
";
            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckEmptyStmt()
                )
            );
        }

        [TestMethod]
        public void TestIfStatement() {
            const string code = @"
if(true) {
    1
}else{
    2
}

if(true) {
    1
}

";
            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckIfStmt(
                        True,
                        CheckBlock(CheckConstantStmt(One)),
                        CheckBlock(CheckConstantStmt(Two))
                    ),
                    CheckIfStmt(
                        True,
                        CheckBlock(CheckConstantStmt(One))
                    )
                )
            );
        }


        [TestMethod]
        public void TestForStatement() {
            const string ForCode = @"
for(var i = 0; i<4; i++) { }
";
            CheckAst(
                ParseCode(ForCode),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void TestForNoVarStatement() {
            const string ForCode = @"
for(i = 0; i<4; i++) { }
";
            CheckAst(
                ParseCode(ForCode),
                CheckBlock(
                    CheckForStmt(
                        CheckExprStmt(
                            CheckBinary(
                                JSToken.Assign,
                                CheckLookup("i"),
                                Zero
                            )
                        ),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void TestForInStatement() {
            const string ForCode = @"
for(var i in []) { }
for(i in []) { }
";
            CheckAst(
                ParseCode(ForCode),
                CheckBlock(
                    CheckForInStmt(
                        CheckVar(CheckVarDecl("i")),
                        CheckArrayLiteral(),
                        CheckBlock()
                    ),
                    CheckForInStmt(
                        CheckExprStmt(CheckLookup("i")),
                        CheckArrayLiteral(),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void TestForContinueStatement() {
            const string ForCode = @"
for(var i = 0; i<4; i++) { continue; }
";
            CheckAst(
                ParseCode(ForCode),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock(
                            CheckContinue()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void TestForBreakStatement() {
            const string ForCode = @"
for(var i = 0; i<4; i++) { break; }
";
            CheckAst(
                ParseCode(ForCode),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock(
                            CheckBreak()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void TestForContinueLabelStatement() {
            const string ForCode = @"
for(var i = 0; i<4; i++) { continue myLabel; }
myLabel:
";
            CheckAst(
                ParseCode(ForCode),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock(
                            CheckContinue("myLabel")
                        )
                    ),
                    CheckLabel("myLabel")
                )
            );
        }

        [TestMethod]
        public void TestForBreakLabelStatement() {
            const string ForCode = @"
for(var i = 0; i<4; i++) { break myLabel; }
myLabel:
";
            CheckAst(
                ParseCode(ForCode),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock(
                            CheckBreak("myLabel")
                        )
                    ),
                    CheckLabel("myLabel")
                )
            );
        }

        [TestMethod]
        public void TryStatement() {
            const string ForCode = @"
try {
}catch(err) {
}
";
            CheckAst(
                ParseCode(ForCode),
                CheckBlock(
                    CheckTryCatch(
                        CheckBlock(),
                        CheckParameterDeclaration("err"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void WhileStatement() {
            const string ForCode = @"
while(i<0) { 2; }
";
            CheckAst(
                ParseCode(ForCode),
                CheckBlock(
                    CheckWhileStmt(
                        CheckLessThan(I, Zero),
                        CheckBlock(CheckConstantStmt(Two))
                    )
                )
            );
        }

        [TestMethod]
        public void TestUseStrictDirective() {
            const string code = @"'use strict';
42";

            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckExprStmt(
                        CheckDirectivePrologue("use strict", true)
                    ),
                    CheckConstantStmt(42.0)
                )
            );
        }

        [TestMethod]
        public void DoWhileStatement() {
            const string ForCode = @"
do {
    2
}while(i<1);
";
            CheckAst(
                ParseCode(ForCode),
                CheckBlock(
                    CheckDoWhileStmt(
                        CheckBlock(CheckConstantStmt(Two)),
                        CheckLessThan(I, Zero)
                    )
                )
            );
        }

        #region Checker helpers

        private static Action<Statement> Pass = CheckEmptyStmt();
        private static Action<Expression> Zero = CheckConstant(0.0);
        private static Action<Expression> One = CheckConstant(1.0);
        private static Action<Expression> Two = CheckConstant(2.0);
        private static Action<Expression> Three = CheckConstant(3.0);
        private static Action<Expression> Four = CheckConstant(4.0);
        private static Action<Expression> None = CheckConstant(null);
        private static Action<Expression> True = CheckConstant(true);
        private static Action<Expression> False = CheckConstant(false);
        private static Action<Expression> I = CheckLookup("i");

        private Action<Statement> CheckIfStmt(Action<Expression> condition, Action<Statement> trueBlock, Action<Statement> falseBlock = null) {
            return stmt => {
                Assert.AreEqual(typeof(IfNode), stmt.GetType());

                var ifNode = (IfNode)stmt;
                condition(ifNode.Condition);
                trueBlock(ifNode.TrueBlock);
                if (falseBlock != null) {
                    falseBlock(ifNode.FalseBlock);
                } else {
                    Assert.AreEqual(null, ifNode.FalseBlock);
                }
            };
        }

        private Action<Statement> CheckTryCatch(Action<Statement> tryBody, Action<ParameterDeclaration> name, Action<Statement> catchBody) {
            return stmt => {
                Assert.AreEqual(typeof(TryNode), stmt.GetType());

                var tryNode = (TryNode)stmt;
                tryBody(tryNode.TryBlock);
                name(tryNode.CatchParameter);
                catchBody(tryNode.CatchBlock);
            };
        }

        private Action<ParameterDeclaration> CheckParameterDeclaration(string name) {
            return decl => {
                Assert.AreEqual(typeof(ParameterDeclaration), decl.GetType());

                var paramDecl = (ParameterDeclaration)decl;
                Assert.AreEqual(name, paramDecl.Name);
            };
        }

        private Action<Statement> CheckWhileStmt(Action<Expression> condition, Action<Statement> body) {
            return stmt => {
                Assert.AreEqual(typeof(WhileNode), stmt.GetType());

                var whileStmt = (WhileNode)stmt;
                condition(whileStmt.Condition);
                body(whileStmt.Body);
            };
        }

        private Action<Statement> CheckDoWhileStmt(Action<Statement> body, Action<Expression> condition) {
            return stmt => {
                Assert.AreEqual(typeof(DoWhile), stmt.GetType());
                var whileStmt = (DoWhile)stmt;

                body(whileStmt.Body);
                condition(whileStmt.Condition);
            };
        }

        private static Action<Statement> CheckEmptyStmt() {
            return expr => {
                Assert.AreEqual(typeof(EmptyStatement), expr.GetType());
            };
        }

        private static Action<Expression> CheckLessThan(Action<Expression> operand1, Action<Expression> operand2) {
            return expr => CheckBinary(JSToken.LessThan, operand1, operand2);
        }

        private static Action<Expression> CheckBinary(JSToken token, Action<Expression> operand1, Action<Expression> operand2) {
            return expr => {
                Assert.AreEqual(typeof(BinaryOperator), expr.GetType());
                var bin = (BinaryOperator)expr;
                operand1(bin.Operand1);
                operand2(bin.Operand2);
            };
        }

        private static Action<Statement> CheckBinaryStmt(JSToken token, Action<Expression> operand1, Action<Expression> operand2) {
            return expr => CheckExprStmt(CheckBinary(token, operand1, operand2));
        }

        private static Action<Expression> CheckIncrement(Action<Expression> operand, bool isPostFix = true) {
            return expr => CheckUnary(JSToken.Increment, operand, isPostFix);
        }

        private static Action<Statement> CheckUnaryStmt(JSToken token, Action<Expression> operand, bool isPostFix = false) {
            return expr => CheckExprStmt(CheckUnary(token, operand, isPostFix));
        }

        private static Action<Expression> CheckUnary(JSToken token, Action<Expression> operand, bool isPostFix = false) {
            return expr => {
                Assert.AreEqual(typeof(UnaryOperator), expr.GetType());
                var bin = (UnaryOperator)expr;
                operand(bin.Operand);
            };
        }

        private static Action<Expression> CheckLookup(string name) {
            return expr => {
                Assert.AreEqual(typeof(Lookup), expr.GetType());
                var nameExpr = (Lookup)expr;
                Assert.AreEqual(nameExpr.Name, name);
            };
        }

        private static Action<Statement> CheckLookupStmt(string name) {
            return stmt => CheckExprStmt(CheckLookup(name));
        }

        private static Action<Statement> CheckVar(params Action<VariableDeclaration>[] decls) {
            return expr => {
                Assert.AreEqual(typeof(Var), expr.GetType());
                var varNode = (Var)expr;
                Assert.AreEqual(varNode.Count, decls.Length);
                for (int i = 0; i < decls.Length; i++) {
                    decls[i](varNode[i]);
                }
            };
        }

        private static Action<VariableDeclaration> CheckVarDecl(string name, Action<Expression> initializer = null) {
            return decl => {
                Assert.AreEqual(decl.Name, name);
                if (initializer != null) {
                    initializer(decl.Initializer);
                } else {
                    Assert.AreEqual(null, decl.Initializer);
                }
            };
        }

        private static Action<Expression> CheckConstant(object value) {
            return expr => {
                Assert.AreEqual(typeof(ConstantWrapper), expr.GetType());

                Assert.AreEqual(value, ((ConstantWrapper)expr).Value);
            };
        }

        private static Action<Statement> CheckConstantStmt(object value) {
            return stmt => CheckExprStmt(CheckConstant(value));
        }

        private static Action<Statement>[] CheckConstantStmts(params object[] value) {
            return value.Select(x => CheckConstantStmt(x)).ToArray();
        }

        class RegEx {
            public readonly string Pattern;
            public readonly string Switches;

            public RegEx(string pattern, string switches) {
                Pattern = pattern;
                Switches = switches;
            }

            public static implicit operator RegEx(string pattern) {
                return new RegEx(pattern, null);
            }
        }

        private static Action<Statement>[] CheckRegExs(params RegEx[] values) {
            return values.Select(x => CheckRegExStmt(x.Pattern, x.Switches)).ToArray();
        }

        private static Action<Statement> CheckRegExStmt(string pattern, string switches = null) {
            return stmt => CheckExprStmt(CheckRegEx(pattern, switches));
        }

        private static Action<Expression> CheckRegEx(string pattern, string switches = null) {
            return expr => {
                Assert.AreEqual(typeof(RegExpLiteral), expr.GetType());

                var regex = (RegExpLiteral)expr;
                Assert.AreEqual(pattern, regex.Pattern);
                Assert.AreEqual(switches, regex.PatternSwitches);
            };
        }

        private static Action<Statement> CheckExprStmt(Action<Expression> expr) {
            return stmt => {
                Assert.AreEqual(typeof(ExpressionStatement), stmt.GetType());

                var exprStmt = (ExpressionStatement)stmt;
                expr(exprStmt.Expression);
            };
        }

        private static Action<Statement> CheckForStmt(Action<Statement> init, Action<Expression> condition, Action<Expression> increment, Action<Statement> body) {
            return stmt => {
                Assert.AreEqual(typeof(ForNode), stmt.GetType());
                ForNode forStmt = (ForNode)stmt;

                init(forStmt.Initializer);
                condition(forStmt.Condition);
                increment(forStmt.Incrementer);
                body(forStmt.Body);
            };
        }

        private static Action<Statement> CheckBlock(params Action<Statement>[] statements) {
            return stmt => {
                Assert.AreEqual(typeof(Block), stmt.GetType());
                var block = (Block)stmt;
                Assert.AreEqual(statements.Length, block.Count);
                for (int i = 0; i < block.Count; i++) {
                    try {
                        statements[i](block[i]);
                    } catch (AssertFailedException e) {
                        throw new AssertFailedException(String.Format("Block Item {0}: {1}", i, e.Message), e);
                    }
                }
            };
        }

        private static Action<Statement> CheckDebuggerStmt() {
            return stmt => {
                Assert.AreEqual(typeof(DebuggerNode), stmt.GetType());
            };
        }

        private static Action<Statement> CheckContinue(string label = null) {
            return stmt => {
                Assert.AreEqual(typeof(ContinueNode), stmt.GetType());

                var contStmt = (ContinueNode)stmt;
                if (label != null) {
                    Assert.AreEqual(label, contStmt.Label);
                } else {
                    Assert.AreEqual(null, contStmt.Label);
                }
            };
        }
        private static Action<Statement> CheckBreak(string label = null) {
            return stmt => {
                Assert.AreEqual(typeof(Break), stmt.GetType());

                var breakStmt = (Break)stmt;
                if (label != null) {
                    Assert.AreEqual(label, breakStmt.Label);
                } else {
                    Assert.AreEqual(null, breakStmt.Label);
                }
            };
        }

        private static Action<Statement> CheckLabel(string label, Action<Statement> statement = null) {
            return stmt => {
                Assert.AreEqual(typeof(LabeledStatement), stmt.GetType());

                var labelStmt = (LabeledStatement)stmt;
                Assert.AreEqual(label, labelStmt.Label);
                if (statement != null) {
                    statement(labelStmt.Statement);
                } else {
                    Assert.AreEqual(null, labelStmt.Statement);
                }
            };
        }

        private static Action<Statement> CheckSwitch(Action<Expression> expr, params Action<SwitchCase>[] cases) {
            return stmt => {
                Assert.AreEqual(typeof(Switch), stmt.GetType());

                var switchStmt = (Switch)stmt;
                expr(switchStmt.Expression);
                Assert.AreEqual(cases.Length, switchStmt.Cases.Count);
                for (int i = 0; i < cases.Length; i++) {
                    cases[i](switchStmt.Cases[i]);
                }
            };
        }

        private static Action<SwitchCase> CheckCase(Action<Expression> expr, Action<Statement> body) {
            return stmt => {
                Assert.AreEqual(typeof(SwitchCase), stmt.GetType());

                var switchCase = (SwitchCase)stmt;
                if (expr != null) {
                    expr(switchCase.CaseValue);
                } else {
                    Assert.IsNull(switchCase.CaseValue, "expected default case");
                }
                body(switchCase.Statements);                
            };
        }

        private Action<Statement> CheckThrow(Action<Expression> action = null) {
            return stmt => {
                Assert.AreEqual(typeof(ThrowNode), stmt.GetType());

                var throwNode = (ThrowNode)stmt;
                if (action != null) {
                    action(throwNode.Operand);
                } else {
                    Assert.IsNull(throwNode.Operand, "expected no throw operand");
                }
            };
        }

        private Action<Statement> CheckWith(Action<Expression> expr, Action<Statement> body) {
            return stmt => {
                Assert.AreEqual(typeof(WithNode), stmt.GetType());

                var withNode = (WithNode)stmt;
                expr(withNode.WithObject);
                body(withNode.Body);
            };
        }

        private Action<Expression> CheckConditional(Action<Expression> condition, Action<Expression> ifTrue, Action<Expression> ifFalse) {
            return expr => {
                Assert.AreEqual(typeof(Conditional), expr.GetType());

                var cond = (Conditional)expr;
                condition(cond.Condition);
                ifTrue(cond.TrueExpression);
                ifFalse(cond.FalseExpression);
            };
        }

        private Action<Expression> CheckGrouping(Action<Expression> operand) {
            return expr => {
                Assert.AreEqual(typeof(GroupingOperator), expr.GetType());

                var grouping = (GroupingOperator)expr;
                operand(grouping.Operand);
            };
        }

        private Action<Expression> CheckArrayLiteral(params Action<Expression>[] values) {
            return expr => {
                Assert.AreEqual(typeof(ArrayLiteral), expr.GetType());

                var array = (ArrayLiteral)expr;
                Assert.AreEqual(values.Length, array.Elements.Count);
                for (int i = 0; i < values.Length; i++) {
                    values[i](array.Elements[i]);
                }
            };            
        }

        private Action<Expression> CheckObjectLiteral(params Action<ObjectLiteralProperty>[] values) {
            return expr => {
                Assert.AreEqual(typeof(ObjectLiteral), expr.GetType());

                var array = (ObjectLiteral)expr;
                Assert.AreEqual(values.Length, array.Properties.Count);
                for (int i = 0; i < values.Length; i++) {
                    values[i](array.Properties[i]);
                }
            };
        }

        private Action<ObjectLiteralProperty> Property(object name, Action<Expression> value) {
            return expr => {
                Assert.AreEqual(typeof(ObjectLiteralProperty), expr.GetType());

                var prop = (ObjectLiteralProperty)expr;
                Assert.AreEqual(name, prop.Name.Value);
                value(prop.Value);                
            };
        }

        private Action<ObjectLiteralProperty> GetterSetter(bool isGetter, string name, Action<Expression> value) {
            return expr => {
                Assert.AreEqual(typeof(ObjectLiteralProperty), expr.GetType());

                var prop = (ObjectLiteralProperty)expr;
                value(prop.Value);

                Assert.AreEqual(typeof(GetterSetter), prop.Name.GetType());
                var getterSetter = (GetterSetter)prop.Name;
                Assert.AreEqual(name, prop.Name.Value);
                Assert.AreEqual(isGetter, getterSetter.IsGetter);
            };
        }

        private Action<Statement> CheckFunctionObject(string name, Action<Statement> body, params Action<ParameterDeclaration>[] args) {
            return stmt => {
                Assert.AreEqual(typeof(FunctionObject), stmt.GetType());

                var func = (FunctionObject)stmt;
                Assert.AreEqual(name, func.Name);

                body(func.Body);
                Assert.AreEqual(args.Length, func.ParameterDeclarations.Count);
                for (int i = 0; i < func.ParameterDeclarations.Count; i++) {
                    args[i](func.ParameterDeclarations[i]);
                }
            };
        }

        private Action<Expression> CheckFunctionExpr(Action<Statement> functionObject) {
            return expr => {
                Assert.AreEqual(typeof(FunctionExpression), expr.GetType());

                var funcExpr = (FunctionExpression)expr;

                functionObject(funcExpr.Function);
            };
        }

        private Action<Statement> CheckForInStmt(Action<Statement> decl, Action<Expression> collection, Action<Statement> body) {
            return stmt => {
                Assert.AreEqual(typeof(ForIn), stmt.GetType());

                var forIn = (ForIn)stmt;
                decl(forIn.Variable);
                collection(forIn.Collection);
            };
        }


        private Action<Expression> CheckDirectivePrologue(string value, bool useStrict = false) {
            return expr => {
                Assert.AreEqual(typeof(DirectivePrologue), expr.GetType());

                Assert.AreEqual(value, ((DirectivePrologue)expr).Value);
                Assert.AreEqual(useStrict, ((DirectivePrologue)expr).UseStrict);
            };
        }

        #endregion

        private static JsAst ParseCode(string code) {
            var parser = new JSParser(code);
            var ast = parser.Parse(new CodeSettings());
            return ast;
        }

        private void CheckAst(JsAst ast, Action<Statement> checkBody) {
            checkBody(ast.Block);
        }

    }
}
