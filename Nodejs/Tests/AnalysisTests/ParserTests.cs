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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace AnalysisTests {
    [TestClass]
    public class ParserTests {
        [TestMethod]
        public void TestCodeSettings() {
            var settings1 = new CodeSettings();
            var settings2 = new CodeSettings() { AllowShebangLine = true, SourceMode = JavaScriptSourceMode.Expression, StrictMode = true };
            var settings3 = new CodeSettings() { KnownGlobalNamesList = "foo;bar" };

            foreach (var curSetting in new[] { settings1, settings2, settings3 }) {
                var other = curSetting.Clone();
                Assert.AreEqual(curSetting.AllowShebangLine, other.AllowShebangLine);
                Assert.AreEqual(curSetting.ConstStatementsMozilla, other.ConstStatementsMozilla);
                Assert.AreEqual(curSetting.KnownGlobalNamesList, other.KnownGlobalNamesList);
                Assert.AreEqual(curSetting.SourceMode, other.SourceMode);
                Assert.AreEqual(curSetting.StrictMode, other.StrictMode);                
            }
        }

        [TestMethod]
        public void TestSourceSpan() {
            var span = new SourceSpan(
                new SourceLocation(1, 1, 1),
                new SourceLocation(100, 2, 100)
            );

            var span2 = span;

            Assert.IsTrue(span.Start < span.End);
            AssertUtil.Throws<ArgumentException>(() => new SourceSpan(span.End, span.Start));
            AssertUtil.Throws<ArgumentException>(() => new SourceSpan(span.End, SourceLocation.Invalid));
            AssertUtil.Throws<ArgumentException>(() => new SourceSpan(SourceLocation.Invalid, span.Start));
            Assert.AreEqual(99, span.Length);
            Assert.IsTrue(span == span2);
            Assert.IsFalse(span != span2);
            Assert.IsTrue(span.Equals(span2));
        }


        [TestMethod]
        public void TestParseExpression() {
            const string code = @"
42
";
            CheckAst(
                ParseExpression(
                    code,
                    false
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckConstant(42.0)
                    )
                )
            );
        }


        [TestMethod]
        public void ErrorCallBadArgs() {
            const string code = @"
foo(abc.'foo')
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(10, 5))
                ),
                CheckBlock(                
                    CheckExprStmt(
                        CheckCall(
                            CheckLookup("foo"),
                            CheckLookup("abc")
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorCallBadArgs2() {
            const string code = @"
foo(abc.'foo' else function)
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(10, 5)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(29, 1))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckCall(
                            CheckLookup("foo"),
                            CheckLookup("abc")
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void TestMemberKeyword() {
            const string code = @"
foo.get
";
            CheckAst(
                ParseCode(
                    code
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckMember("get", CheckLookup("foo"))
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorGroupingNoCloseParen() {
            const string code = @"
(foo
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoRightParenthesis, true, new IndexSpan(8, 0))
                ),
                CheckBlock(
                )
            );
        }

        [TestMethod]
        public void ErrorGroupingNoSkipToken() {
            const string code = @"
(else function)
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(3, 4)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(16, 1))
                ),
                CheckBlock(
                )
            );
        }

        [TestMethod]
        public void ErrorPostfixNoSkipToken() {
            const string code = @"
(else++)
foo
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(3, 4))
                ),
                CheckBlock(
                )
            );
        }


        [TestMethod]
        public void ErrorDoubleOverflow() {
            const string code = @"
1e100000
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NumericOverflow, true, new IndexSpan(2, 8))
                ),
                CheckBlock(
                    CheckConstantStmt(new InvalidNumericErrorValue("1e100000"))
                )
            );
        }

        [TestMethod]
        public void WarningDoubleBoundaries() {
            const string code = @"
1.7976931348623157E+308;
-1.7976931348623157E+308
";
            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.NumericMaximum, false, new IndexSpan(2, 23)),
                    new ErrorInfo(JSError.NumericMaximum, false, new IndexSpan(29, 23))
                ),
                CheckBlock(
                    CheckConstantStmt(1.79769e+308),
                    CheckConstantStmt(-1.79769e+308)
                )
            );
        }

        [TestMethod]
        public void ErrorWithStatementBody() {
            const string code = @"
with(abc) {
    else
}
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.InvalidElse, true, new IndexSpan(19, 4))
                ),
                CheckBlock(
                    CheckWith(
                        CheckLookup("abc"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorWithStatementBody2() {
            const string code = @"
with(abc) {
    throw else function
}
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(25, 4)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(40, 1))
                ),
                CheckBlock(
                    CheckWith(
                        CheckLookup("abc"),
                        CheckBlock(
                            CheckThrow(IsNullExpr)
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void WarningWithStatementNoCurlys() {
            const string code = @"
with(abc) 
    foo
";
            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.StatementBlockExpected, false, new IndexSpan(18, 0)),
                    new ErrorInfo(JSError.UndeclaredVariable, false, new IndexSpan(7, 3))
                ),
                CheckBlock(
                    CheckWith(
                        CheckLookup("abc"),
                        CheckBlock(
                            CheckLookupStmt("foo")
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorWithStatement() {
            const string code = @"
with(abc.'foo') {
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(11, 5))
                ),
                CheckBlock(
                    CheckWith(
                        CheckLookup("abc"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorWithStatement2() {
            const string code = @"
with(else) {
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(7, 4))
                ),
                CheckBlock(
                    CheckWith(
                        CheckConstant(true),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorWithStatement3() {
            const string code = @"
with(else function) {
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(7, 4)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(20, 1)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(25, 1))
                ),
                CheckBlock(
                )
            );
        }

        [TestMethod]
        public void ErrorWithStatementNoParens() {
            const string code = @"
with abc {
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoLeftParenthesis, true, new IndexSpan(7, 3)),
                    new ErrorInfo(JSError.NoRightParenthesis, true, new IndexSpan(11, 1))
                ),
                CheckBlock(
                    CheckWith(
                        CheckLookup("abc"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorArrayLiteralBad() {
            const string code = @"
i = [foo foo]";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoRightBracket, true, new IndexSpan(11, 3))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckAssign(
                            I,
                            CheckArrayLiteral(
                                CheckLookup("foo")
                            )
                        )
                    ),
                    CheckLookupStmt("foo")
                )
            );
        }

        [TestMethod]
        public void ErrorArrayLiteralBad2() {
            const string code = @"
i = [foo.'abc']";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(11, 5))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckAssign(
                            I,
                            CheckArrayLiteral(
                                CheckLookup("foo")
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorArrayLiteralBad3() {
            const string code = @"
i = [foo.'abc' else function]";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(11, 5)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(30, 1))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckLookup("i")
                    )
                )
            );
        }

        [TestMethod]
        public void ArrayLiteralEmpty() {
            const string code = @"
i = []";
            CheckAst(
                ParseCode(
                    code
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckAssign(
                            I,
                            CheckArrayLiteral(
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ArrayLiteralMissing() {
            const string code = @"
i = [1,,]";
            CheckAst(
                ParseCode(
                    code
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckAssign(
                            I,
                            CheckArrayLiteral(
                                One,
                                CheckConstant(Missing.Value),
                                CheckConstant(Missing.Value)
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorFunctionBadBody3() {
            const string code = @"
function foo() {
    throw else function
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(30, 4)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(45, 1))
                ),
                CheckBlock(
                    CheckFunctionObject(
                        "foo",
                        CheckBlock(
                            CheckThrow(IsNullExpr)
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorFunctionBadBody2() {
            const string code = @"
function foo() {
    else function
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.InvalidElse, true, new IndexSpan(24, 4)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(39, 1))
                ),
                CheckBlock(
                    CheckFunctionObject(
                        "foo",
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorFunctionBadBody() {
            const string code = @"
function foo() {
    else
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.InvalidElse, true, new IndexSpan(24, 4))
                ),
                CheckBlock(
                    CheckFunctionObject(
                        "foo",
                        CheckBlock()
                    )
                )
            );
        }


        [TestMethod]
        public void ErrorFunctionBadArgList2() {
            const string code = @"
function foo(arg if) {
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoComma, true, new IndexSpan(19, 2)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(26, 1))
                ),
                CheckBlock(
                )
            );
        }

        [TestMethod]
        public void ErrorFunctionTypeDefinition() {
            const string code = @"
function foo(arg arg) {
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoCommaOrTypeDefinitionError, true, new IndexSpan(19, 3))
                ),
                CheckBlock(
                    CheckFunctionObject(
                        "foo",
                        CheckBlock(),
                        CheckParameterDeclaration("arg"),
                        CheckParameterDeclaration("arg")
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorFunctionBadArgList() {
            const string code = @"
function foo(] ) {
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(15, 1))
                ),
                CheckBlock(
                    CheckFunctionObject(
                        "foo",
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorFunctionExtraComma() {
            const string code = @"
function foo(, ) {
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(15, 1))
                ),
                CheckBlock(
                    CheckFunctionObject(
                        "foo",
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorFunctionNoRightParen() {
            const string code = @"
function foo( {
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoRightParenthesis, true, new IndexSpan(16, 1))
                ),
                CheckBlock(
                    CheckFunctionObject(
                        "foo",
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorFunctionNoRightParen2() {
            const string code = @"
function foo(abc {
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoRightParenthesis, true, new IndexSpan(19, 1))
                ),
                CheckBlock(
                    CheckFunctionObject(
                        "foo",
                        CheckBlock(),
                        CheckParameterDeclaration("abc")
                    )
                )
            );
        }

        [TestMethod]
        public void TestFunctionStrictMode() {
            const string code = @"
function abc() {
    'use strict';
}";
            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckFunctionObject(
                        "abc",
                        CheckBlock(
                            CheckExprStmt(
                                CheckDirectivePrologue("use strict", useStrict: true)
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void TestFunctionKeywordIdentifier() {
            const string code = @"
function get() {
}";
            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckFunctionObject(
                        "get",
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorTryCatchNoTryOrFinally() {
            const string code = @"
try {
}
foo";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoCatch, true, new IndexSpan(12, 3))
                ),
                CheckBlock(
                    CheckTryFinally(
                        CheckBlock(),
                        CheckBlock()
                    ),
                    CheckLookupStmt("foo")
                )
            );
        }

        [TestMethod]
        public void ErrorTryCatchBadCatch() {
            const string code = @"
try {
} catch(foo) {
    else
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.InvalidElse, true, new IndexSpan(29, 4))
                ),
                CheckBlock(
                    CheckTryCatch(
                        CheckBlock(),
                        CheckParameterDeclaration("foo"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorTryCatchBadCatch2() {
            const string code = @"
try {
} catch(foo) {
    else function
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.InvalidElse, true, new IndexSpan(29, 4)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(44, 1))
                ),
                CheckBlock(
                    CheckBlock()
                )
            );
        }

        [TestMethod]
        public void ErrorTryCatchNonIdentifierKeyword() {
            const string code = @"
try {
} catch(function) {
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(17, 8)),
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(25, 1)),
                    new ErrorInfo(JSError.ErrorEndOfFile, true, new IndexSpan(31, 0))
                ),
                CheckBlock(
                )
            );
        }


        [TestMethod]
        public void ErrorTryCatchIdentifierKeyword() {
            const string code = @"
try {
} catch(get) {
}";
            CheckAst(
                ParseCode(
                    code
                ),
                CheckBlock(
                    CheckTryCatch(
                        CheckBlock(),
                        CheckParameterDeclaration("get"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorTryCatchNoParens() {
            const string code = @"
try {
} catch quox {
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoLeftParenthesis, true, new IndexSpan(17, 4)),
                    new ErrorInfo(JSError.NoRightParenthesis, true, new IndexSpan(22, 1))
                ),
                CheckBlock(
                    CheckTryCatch(
                        CheckBlock(),
                        CheckParameterDeclaration("quox"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorTryBadBody() {
            const string code = @"
try {
    else
} catch(quox) {
    foo
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.InvalidElse, true, new IndexSpan(13, 4))
                ),
                CheckBlock(
                    CheckTryCatch(
                        CheckBlock(),
                        CheckParameterDeclaration("quox"),
                        CheckBlock(CheckLookupStmt("foo"))
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorTryBadBody2() {
            const string code = @"
try {
    else function
} catch(quox) {
    foo
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.InvalidElse, true, new IndexSpan(13, 4)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(28, 1)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(54, 1))
                ),
                CheckBlock(
                    CheckBlock(),
                    CheckLookupStmt("foo")
                )
            );
        }


        [TestMethod]
        public void ErrorTryMissingCurlys() {
            const string code = @"
try
    foo
catch(quox)
    foo
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoLeftCurly, true, new IndexSpan(11, 3)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(16, 5)),
                    new ErrorInfo(JSError.NoLeftCurly, true, new IndexSpan(33, 3))
                ),
                CheckBlock(
                    CheckTryCatch(
                        CheckBlock(CheckLookupStmt("foo")),
                        CheckParameterDeclaration("quox"),
                        CheckBlock(CheckLookupStmt("foo"))
                    )
                )
            );
        }

        [TestMethod]
        public void WarningThrowSemicolonInsertion() {
            const string code = @"
throw foo
foo";
            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.SemicolonInsertion, false, new IndexSpan(11, 0)),
                    new ErrorInfo(JSError.UndeclaredVariable, false, new IndexSpan(8, 3))
                ),
                CheckBlock(
                    CheckThrow(
                        CheckLookup("foo")
                    ),
                    CheckLookupStmt("foo")
                )
            );
        }

        [TestMethod]
        public void ErrorThrowNoSemicolon() {
            const string code = @"
throw foo foo";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(12, 3))
                ),
                CheckBlock(
                    CheckThrow(
                        CheckLookup("foo")
                    ),
                    CheckLookupStmt("foo"),
                    CheckLookupStmt("foo")
                )
            );
        }

        [TestMethod]
        public void ErrorThrowBadExpression2() {
            const string code = @"
throw foo.'abc' foo";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(12, 5))
                ),
                CheckBlock(
                )
            );
        }

        [TestMethod]
        public void ErrorThrowBadExpression() {
            const string code = @"
throw foo.'abc'
foo";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(12, 5))
                ),
                CheckBlock(
                    CheckThrow(
                        CheckLookup("foo")
                    ),
                    CheckLookupStmt("foo")
                )
            );
        }

        [TestMethod]
        public void ErrorSwitchCaseBadStatement() {
            const string code = @"
switch(foo){
    case 0:
        else        
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.InvalidElse, true, new IndexSpan(37, 4))
                ),
                CheckBlock(
                    CheckSwitch(
                        CheckLookup("foo"),
                        CheckCase(
                            Zero,
                            CheckBlock()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorSwitchCaseBadCase2() {
            const string code = @"
switch(foo){
    case foo.'abc' else function
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(29, 5)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(50, 1))
                ),
                CheckBlock(
                    CheckSwitch(
                        CheckLookup("foo")
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorSwitchCaseBadCase() {
            const string code = @"
switch(foo){
    case foo.'abc':
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(29, 5))
                ),
                CheckBlock(
                    CheckSwitch(
                        CheckLookup("foo"),
                        CheckCase(
                            CheckLookup("foo"),
                            CheckBlock()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorSwitchCaseMissingColon() {
            const string code = @"
switch(foo){
    case 0
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoColon, true, new IndexSpan(28, 1))
                ),
                CheckBlock(
                    CheckSwitch(
                        CheckLookup("foo"),
                        CheckCase(
                            Zero,
                            CheckBlock()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorSwitchNoCasesOrDefault() {
            const string code = @"
switch(foo){
    foo
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.BadSwitch, true, new IndexSpan(20, 3))
                ),
                CheckBlock(
                    CheckSwitch(
                        CheckLookup("foo"),
                        CheckCase(
                            null,
                            CheckBlock(
                                CheckLookupStmt("foo")
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorSwitchDuplicateDefault() {
            const string code = @"
switch(foo){
    default:
    default:
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.DupDefault, true, new IndexSpan(34, 7))
                ),
                CheckBlock(
                    CheckSwitch(
                        CheckLookup("foo"),
                        CheckCase(
                            null,
                            CheckBlock()
                        ),
                        CheckCase(
                            null,
                            CheckBlock()
                        )
                    )
                )
            );
        }


        [TestMethod]
        public void ErrorSwitchBadExpressionNoLeftCurly() {
            const string code = @"
switch(foo.'abc') 
    case 0:
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(13, 5)),
                    new ErrorInfo(JSError.NoLeftCurly, true, new IndexSpan(26, 4))
                ),
                CheckBlock(
                    CheckSwitch(
                        CheckLookup("foo"),
                        CheckCase(
                            Zero,
                            CheckBlock()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorSwitchBadExpression() {
            const string code = @"
switch(foo.'abc') {
    case 0:
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(13, 5))
                ),
                CheckBlock(
                    CheckSwitch(
                        CheckLookup("foo"),
                        CheckCase(
                            Zero,
                            CheckBlock()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorSwitchBadExpression2() {
            const string code = @"
switch(else) {
    case 0:
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(9, 4))
                ),
                CheckBlock(
                    CheckSwitch(
                        CheckConstant(true),
                        CheckCase(
                            Zero,
                            CheckBlock()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorSwitchBadExpression3() {
            const string code = @"
switch(else function) {
    case 0:
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(9, 4)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(22, 1)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(31, 4))
                ),
                CheckBlock(
                )
            );
        }

        [TestMethod]
        public void ErrorSwitchNoLeftCurly() {
            const string code = @"
switch(abc) 
    case 0:
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoLeftCurly, true, new IndexSpan(20, 4))
                ),
                CheckBlock(
                    CheckSwitch(
                        CheckLookup("abc"),
                        CheckCase(
                            Zero,
                            CheckBlock()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorSwitchNoParens() {
            const string code = @"
switch abc {
    case 0:
}";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoLeftParenthesis, true, new IndexSpan(9, 3)),
                    new ErrorInfo(JSError.NoRightParenthesis, true, new IndexSpan(13, 1))
                ),
                CheckBlock(
                    CheckSwitch(
                        CheckLookup("abc"),
                        CheckCase(
                            Zero,
                            CheckBlock()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorBracketInvalidExpression() {
            const string code = @"
i[foo.'abc'];";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(8, 5))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckIndex(
                            I,
                            CheckLookup("foo")
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorBracketInvalidExpression2() {
            const string code = @"
i[foo.'abc' function];";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(8, 5)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(22, 1))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckIndex(
                            I,
                            CheckLookup("foo")
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorBracketInvalidExpression3() {
            const string code = @"
i[else function];";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(4, 4)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(17, 1))
                ),
                CheckBlock(
                    CheckExprStmt(
                        I
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorBracketInvalidExpression4() {
            const string code = @"
i[function];";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoLeftCurly, true, new IndexSpan(13, 1)),
                    new ErrorInfo(JSError.ErrorEndOfFile, true, new IndexSpan(14, 0)),
                    new ErrorInfo(JSError.UnclosedFunction, true, new IndexSpan(4, 8))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckIndex(
                            I,
                            CheckFunctionExpr(
                                CheckFunctionObject(
                                    null,
                                    CheckBlock(),
                                    null
                                )
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void WarningOctalLiteral() {
            const string code = @"
i = 0100;";
            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.OctalLiteralsDeprecated, false, new IndexSpan(6, 4)),
                    new ErrorInfo(JSError.OctalLiteralsDeprecated, false, new IndexSpan(6, 4)),
                    new ErrorInfo(JSError.UndeclaredVariable, false, new IndexSpan(2, 1))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckAssign(
                            I,
                            CheckConstant(64.0)
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorObjectLiteralNoColon() {
            const string code = @"
i = {abc:42, foo 0};";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoColon, true, new IndexSpan(19, 1))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckAssign(
                            I,
                            CheckObjectLiteral(
                                Property("abc", CheckConstant(42.0)),
                                Property("foo", Zero)
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorObjectLiteralNoRightCurly() {
            const string code = @"
i = {abc:42, foo:0
foo
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoRightCurly, true, new IndexSpan(22, 3))
                ),
                CheckBlock(
                    CheckLookupStmt("i"),
                    CheckLookupStmt("foo")
                )
            );
        }

        [TestMethod]
        public void ErrorObjectLiteralNoComma() {
            const string code = @"
i = {abc:42, foo:0 foo
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoComma, true, new IndexSpan(21, 3))
                ),
                CheckBlock(
                )
            );
        }

        [TestMethod]
        public void ErrorObjectLiteralBadExpression() {
            const string code = @"
i = {abc:foo.'abc'};";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(15, 5))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckAssign(
                            I,
                            CheckObjectLiteral(
                                Property("abc", CheckLookup("foo"))
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorObjectLiteralBadExpression2() {
            const string code = @"
i = {abc:foo.'abc' else };";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(15, 5))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckAssign(
                            I,
                            CheckObjectLiteral(
                                Property("abc", CheckLookup("foo"))
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorObjectLiteralBadExpression3() {
            const string code = @"
i = {abc:foo.'abc' else, foo:42 };";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(15, 5))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckAssign(
                            I,
                            CheckObjectLiteral(
                                Property("abc", CheckLookup("foo")),
                                Property("foo", CheckConstant(42.0))
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorObjectLiteralNoMemberIdentifier() {
            const string code = @"
i = {abc:foo, [:42};";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoMemberIdentifier, true, new IndexSpan(16, 1))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckAssign(
                            I,
                            CheckObjectLiteral(
                                Property("abc", CheckLookup("foo")),
                                Property("[", CheckConstant(42.0))
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorObjectLiteralInvalidNumeric() {
            const string code = @"
i = {abc:foo, 1e100000:42};";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NumericOverflow, true, new IndexSpan(16, 8))
                ),
                CheckBlock(
                    CheckExprStmt(
                        CheckAssign(
                            I,
                            CheckObjectLiteral(
                                Property("abc", CheckLookup("foo")),
                                Property(new InvalidNumericErrorValue("1e100000"), CheckConstant(42.0))
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void VariableDeclarationBadInitializer2() {
            const string code = @"
var i = foo.'abc' else function;";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(14, 5))
                ),
                CheckBlock(
                    CheckEmptyStmt()
                )
            );
        }

        [TestMethod]
        public void VariableDeclarationBadInitializer3() {
            const string code = @"
var j = 0, i = foo.'abc' else function;";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(21, 5))
                ),
                CheckBlock(
                    CheckVar(
                        CheckVarDecl("j", Zero),
                        CheckVarDecl("i", CheckLookup("foo"))
                    ),
                    CheckEmptyStmt()
                )
            );
        }

        [TestMethod]
        public void VariableDeclarationBadInitializer() {
            const string code = @"
var i = foo.'abc';";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(14, 5))
                ),
                CheckBlock(
                    CheckVar(
                        CheckVarDecl("i", CheckLookup("foo"))
                    )
                )
            );
        }

        [TestMethod]
        public void VariableDeclarationEqual() {
            const string code = @"
var i == 1;";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoEqual, true, new IndexSpan(8, 2))
                ),
                CheckBlock(
                    CheckVar(
                        CheckVarDecl("i", One)
                    )
                )
            );
        }


        [TestMethod]
        public void ErrorDebuggerMissingSemiColon() {
            const string code = @"
debugger foo";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(11, 3))
                ),
                CheckBlock(
                    CheckDebuggerStmt(),
                    CheckLookupStmt("foo")
                )
            );
        }

        [TestMethod]
        public void WarningDebuggerMissingSemiColon() {
            const string code = @"
debugger
foo";
            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.SemicolonInsertion, false, new IndexSpan(10, 0)),
                    new ErrorInfo(JSError.UndeclaredVariable, false, new IndexSpan(12, 3))
                ),
                CheckBlock(
                    CheckDebuggerStmt(),
                    CheckLookupStmt("foo")
                )
            );
        }

        [TestMethod]
        public void ErrorDuplicateLabel() {
            const string code = @"
label:
label:
blah;
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.BadLabel, true, new IndexSpan(10, 5))
                ),
                CheckBlock(
                    CheckLabel(
                        "label",
                        CheckBlock()
                    ),
                    CheckLookupStmt("blah")
                )
            );
        }

        [TestMethod]
        public void ErrorBadContinueBadLabel() {
            const string code = @"
foo:
continue foo
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.BadContinue, true, new IndexSpan(8, 12))
                ),
                CheckBlock(
                    CheckLabel(
                        "foo",
                        CheckContinue("foo")
                    )
                )
            );
        }

        [TestMethod]
        public void WarningContinueSemicolonInsertion() {
            const string code = @"
for(var i = 0; i<4; i++) {
    continue
    blah
}
";
            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.SemicolonInsertion, false, new IndexSpan(42, 0)),
                    new ErrorInfo(JSError.UndeclaredVariable, false, new IndexSpan(48, 4))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock(
                            CheckContinue(),
                            CheckLookupStmt("blah")
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void WarningBreakSemicolonInsertion() {
            const string code = @"
for(var i = 0; i<4; i++) {
    break
    blah
}
";
            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.SemicolonInsertion, false, new IndexSpan(39, 0)),
                    new ErrorInfo(JSError.UndeclaredVariable, false, new IndexSpan(45, 4))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock(
                            CheckBreak(),
                            CheckLookupStmt("blah")
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorContinueNoSemicolon() {
            const string code = @"
for(var i = 0; i<4; i++) {
    continue if(true) { }
}
";
            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(43, 2))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock(
                            CheckContinue(),
                            CheckIfStmt(True, CheckBlock())
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorBreakNoSemicolon() {
            const string code = @"
for(var i = 0; i<4; i++) {
    break if(true) { }
}
";
            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(40, 2))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock(
                            CheckBreak(),
                            CheckIfStmt(True, CheckBlock())
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorBadContinue() {
            const string code = @"
continue
blah
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.BadContinue, true, new IndexSpan(2, 8))
                ),
                CheckBlock(
                    CheckBlock(),
                    CheckLookupStmt("blah")
                )
            );
        }

        [TestMethod]
        public void ErrorBadBreak() {
            const string code = @"
break
blah
";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.BadBreak, true, new IndexSpan(2, 5))
                ),
                CheckBlock(
                    CheckBlock(),
                    CheckLookupStmt("blah")
                )
            );
        }

        [TestMethod]
        public void WarningWithNoParens() {
            var code = @"
with abc {
}
";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.NoLeftParenthesis, true, new IndexSpan(7, 3)),
                    new ErrorInfo(JSError.NoRightParenthesis, true, new IndexSpan(11, 1)),
                    new ErrorInfo(JSError.UndeclaredVariable, false, new IndexSpan(7, 3))
                ),
                CheckBlock(
                    CheckWith(
                        CheckLookup("abc"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void WarningReturnNoSemicolon() {
            var code = @"
return 4 4
";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(11, 1))
                ),
                CheckBlock(
                    CheckReturn(Four),
                    CheckExprStmt(Four),
                    CheckExprStmt(Four)
                )
            );
        }

        [TestMethod]
        public void WarningReturnSemiColonInsertion() {
            var code = @"
return 4
4
";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.SemicolonInsertion, false, new IndexSpan(10, 0))
                ),
                CheckBlock(
                    CheckReturn(Four),
                    CheckExprStmt(Four)
                )
            );
        }

        [TestMethod]
        public void RecoveryReturnBadOperand() {
            var code = @"
return foo.'abc'

{}";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(13, 5)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(23, 1))
                ),
                CheckBlock(
                    CheckReturn(
                        CheckLookup("foo")
                    )
                )
            );
        }

        [TestMethod]
        public void RecoverReturnBadOperand2() {
            var code = @"
return else

{}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(9, 4)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(18, 1))
                ),
                CheckBlock(
                    CheckReturn()
                )
            );
        }

        [TestMethod]
        public void RecoveryReturnBadOperand3() {
            var code = @"
return else function";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(9, 4))
                ),
                CheckBlock(
                    CheckReturn()
                )
            );
        }

        [TestMethod]
        public void ErrorIfMissingParens() {
            var code = @"
if true {
}";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.NoLeftParenthesis, true, new IndexSpan(5, 4)),
                    new ErrorInfo(JSError.NoRightParenthesis, true, new IndexSpan(10, 1))
                ),
                CheckBlock(
                    CheckIfStmt(
                        True,
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void WarningIfSuspectAssign() {
            var code = @"
if(i = 1) {
}";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.SuspectAssignment, false, new IndexSpan(5, 5)),
                    new ErrorInfo(JSError.UndeclaredVariable, false, new IndexSpan(5, 1))
                ),
                CheckBlock(
                    CheckIfStmt(
                        CheckAssign(I, One),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void WarningIfSuspectSemicolon() {
            var code = @"
if(true);";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.SuspectSemicolon, false, new IndexSpan(10, 1))
                ),
                CheckBlock(
                    CheckIfStmt(
                        True,
                        CheckBlock(
                            CheckEmptyStmt()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void WarningIfElseSuspectSemicolon() {
            var code = @"
if(true) { }
else;";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.SuspectSemicolon, false, new IndexSpan(20, 1))
                ),
                CheckBlock(
                    CheckIfStmt(
                        True,
                        CheckBlock(
                        ),
                        CheckBlock(
                            CheckEmptyStmt()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void WarningIfElseStatementBlock() {
            var code = @"
if(true) { }
else i";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.StatementBlockExpected, false, new IndexSpan(21, 0)),
                    new ErrorInfo(JSError.UndeclaredVariable, false, new IndexSpan(21, 1))
                ),
                CheckBlock(
                    CheckIfStmt(
                        True,
                        CheckBlock(
                        ),
                        CheckBlock(
                            CheckExprStmt(I)
                        )
                    )
                )
            );
        }


        [TestMethod]
        public void RecoveryIfBadConditional() {
            var code = @"
if(foo.'abc') {
}";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(9, 5))
                ),
                CheckBlock(
                    CheckIfStmt(
                        CheckLookup("foo"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryIfBadConditional2() {
            var code = @"
if(else) {
}";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(5, 4))
                ),
                CheckBlock(
                    CheckIfStmt(
                        True,
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryIfBadConditional3() {
            var code = @"
if(else function) {
}";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(5, 4)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(18, 1)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(23, 1))
                ),
                CheckBlock(
                )
            );
        }

        [TestMethod]
        public void RecoveryIfBadBody() {
            var code = @"
if(true) 
    foo.'abc'

{ }";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(21, 5)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(32, 1))
                ),
                CheckBlock(
                    CheckIfStmt(
                        True,
                        CheckBlock(
                            CheckExprStmt(CheckLookup("foo"))
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryIfBadBody2() {
            var code = @"
if(true) 
    }

{ }";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(17, 1)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(24, 1))
                ),
                CheckBlock(
                    CheckIfStmt(
                        True,
                        CheckBlock(
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryIfBadBody3() {
            var code = @"
if(true) 
    } function

{ }";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(17, 1))
                ),
                CheckBlock(
                    CheckIfStmt(
                        True,
                        CheckBlock(
                        )
                    ),
                    CheckBlock()
                )
            );
        }

        [TestMethod]
        public void RecoveryIfElseBadBody() {
            var code = @"
if(true) {
} else
    foo.'abc'

{ }";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(30, 5)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(41, 1))
                ),
                CheckBlock(
                    CheckIfStmt(
                        True,
                        CheckBlock(                            
                        ),
                        CheckBlock(
                            CheckExprStmt(CheckLookup("foo"))
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryIfElseBadBody2() {
            var code = @"
if(true)  {
} else
    }

{ }";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(27, 1)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(34, 1))
                ),
                CheckBlock(
                    CheckIfStmt(
                        True,
                        CheckBlock(
                        ),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryIfElseBadBody3() {
            var code = @"
if(true) {
} else
    } function

{ }";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(26, 1))
                ),
                CheckBlock(
                    CheckIfStmt(
                        True,
                        CheckBlock(
                        ),
                        CheckBlock()
                    ),
                    CheckBlock()
                )
            );
        }

        [TestMethod]
        public void WarningWhileSuspectAssignment() {
            var code = @"
while(i = 4) {
}
";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.SuspectAssignment, false, new IndexSpan(8, 5)),
                    new ErrorInfo(JSError.UndeclaredVariable, false, new IndexSpan(8, 1))
                ),
                CheckBlock(
                    CheckWhileStmt(
                        CheckAssign(I, Four),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryWhileBadBody() {
            var code = @"
while(true) 
    foo.'abc'

{
}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(24, 5)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(36, 1))
                ),
                CheckBlock(
                    CheckWhileStmt(
                        True,
                        CheckBlock(
                            CheckExprStmt(CheckLookup("foo"))
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryWhileBadBody2() {
            var code = @"
while(true) 
    else

{ }
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.InvalidElse, true, new IndexSpan(20, 4)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(30, 1))
                ),
                CheckBlock(
                    CheckWhileStmt(
                        True,
                        CheckBlock(
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryWhileBadCondition() {
            var code = @"
while(foo.'abc') {
}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(12, 5))
                ),
                CheckBlock(
                    CheckWhileStmt(
                        CheckLookup("foo"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryWhileBadCondition2() {
            var code = @"
while(else) {
}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(8, 4))
                ),
                CheckBlock(
                    CheckWhileStmt(
                        False,
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryWhileBadCondition3() {
            var code = @"
while(else function) {
}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(8, 4)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(21, 1)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(26, 1))
                ),
                CheckBlock(
                )
            );
        }

        [TestMethod]
        public void ErrorWhileNoOpenParens() {
            var code = @"
while true {
}
";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.NoLeftParenthesis, true, new IndexSpan(8, 4)),
                    new ErrorInfo(JSError.NoRightParenthesis, true, new IndexSpan(13, 1))
                ),
                CheckBlock(
                    CheckWhileStmt(
                        True,
                        CheckBlock()
                    )
                )
            );
        }


        [TestMethod]
        public void RecoveryDoWhileBadCondition() {
            var code = @"
do {
    true
} while(foo.'abc');
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(30, 5))
                ),
                CheckBlock(
                    CheckDoWhileStmt(
                        CheckBlock(
                            CheckExprStmt(True)
                        ),
                        CheckLookup("foo")
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryDoWhileBadCondition2() {
            var code = @"
do {
    true
} while(< function() { });
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(26, 1)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(37, 1)),
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(39, 1)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(42, 1))
                ),
                CheckBlock(
                    CheckDoWhileStmt(
                        CheckBlock(
                            CheckExprStmt(True)
                        ),
                        CheckConstant(false)
                    ),
                    CheckExprStmt(CheckGrouping(IsNullExpr)),
                    CheckBlock()
                )
            );
        }

        [TestMethod]
        public void ErrorDoWhileMissngParens() {
            var code = @"
do {
    true
} while true;
";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.NoLeftParenthesis, true, new IndexSpan(26, 4)),
                    new ErrorInfo(JSError.NoRightParenthesis, true, new IndexSpan(30, 1))
                ),
                CheckBlock(
                    CheckDoWhileStmt(
                        CheckBlock(
                            CheckExprStmt(True)
                        ),
                        True
                    )
                )
            );
        }

        [TestMethod]
        public void ErrorDoWhileNoWhile() {
            var code = @"
do {
    true
} (true);
";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.NoWhile, true, new IndexSpan(20, 1))
                ),
                CheckBlock(
                    CheckDoWhileStmt(
                        CheckBlock(
                            CheckExprStmt(True)
                        ),
                        True
                    )
                )
            );
        }


        [TestMethod]
        public void WarningDoWhileMissingOpenCurly() {
            var code = @"
do 
    true
while(true);
";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.StatementBlockExpected, false, new IndexSpan(11, 0))
                ),
                CheckBlock(
                    CheckDoWhileStmt(
                        CheckBlock(
                            CheckExprStmt(True)
                        ),
                        True
                    )
                )
            );
        }

        [TestMethod]
        public void WarningDoWhileSuspectAssignment() {
            var code = @"
do {
    true
} while(i = 4);
";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.SuspectAssignment, false, new IndexSpan(26, 5)),
                    new ErrorInfo(JSError.UndeclaredVariable, false, new IndexSpan(26, 1))
                ),
                CheckBlock(
                    CheckDoWhileStmt(
                        CheckBlock(
                            CheckExprStmt(True)
                        ),
                        CheckAssign(
                            I,
                            Four
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryDoWhileBadBody() {
            var code = @"
do 
    foo.'abc'
while(true);
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(15, 5))
                ),
                CheckBlock(
                    CheckDoWhileStmt(
                        CheckBlock(
                            CheckExprStmt(CheckLookup("foo"))
                        ),
                        True
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryDoWhileBadBody2() {
            var code = @"
do
    else
while(true);
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.InvalidElse, true, new IndexSpan(10, 4))
                ),
                CheckBlock(
                    CheckDoWhileStmt(
                        CheckBlock(),
                        True
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryDoWhileBadBody3() {
            var code = @"
do
    else function
while(true);
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.InvalidElse, true, new IndexSpan(10, 4))
                ),
                CheckBlock(
                    CheckDoWhileStmt(
                        CheckBlock(),
                        False
                    ),
                    CheckWhileStmt(
                        True,
                        CheckBlock(
                            CheckEmptyStmt()
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void WarningForAssignment() {
            var code = @"
for(var i = 0; i = 4; i++) {
}
";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.SuspectAssignment, false, new IndexSpan(17, 5))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckAssign(I, Four),
                        CheckIncrement(I),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void WarningForStatementBlock() {
            var code = @"
for(var i = 0; i < 4; i++) 
    i++;
";

            CheckAst(
                ParseCode(
                    code,
                    true,
                    new ErrorInfo(JSError.StatementBlockExpected, false, new IndexSpan(35, 0))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock(
                            CheckExprStmt(
                                CheckIncrement(I)
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryForIn() {
            var code = @"
for(x in ) {
}

";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(11, 1))
                ),
                CheckBlock(
                    CheckForInStmt(
                        CheckExprStmt(CheckLookup("x")),
                        CheckConstant(true),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryForInBadCollection() {
            var code = @"
for(x in foo.'abc') {
}

";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(15, 5))
                ),
                CheckBlock(
                    CheckForInStmt(
                        CheckExprStmt(CheckLookup("x")),
                        CheckLookup("foo"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryForIn2() {
            var code = @"
for(x in < function() { }) {
}

";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(11, 1)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(22, 1)),
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(24, 1)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(27, 1)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(32, 1))
                ),
                CheckBlock(
                    CheckExprStmt(CheckGrouping(IsNullExpr)),
                    CheckBlock()
                )
            );
        }

        [TestMethod]
        public void RecoveryForInBadBody() {
            // foo.'abc' will trigger a recovery w/ an expression which can be
            // promoted to an expression statement
            var code = @"
for(x in abc) 
    foo.'abc'

{
}

";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(26, 5)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(38, 1))
                ),
                CheckBlock(
                    CheckForInStmt(
                        CheckExprStmt(CheckLookup("x")),
                        CheckLookup("abc"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryForInBadBody2() {
            // } will trigger a recovery w/ an expression
            var code = @"
for(x in abc) 
    }

{
}

";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(22, 1)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(30, 1))
                ),
                CheckBlock(
                    CheckForInStmt(
                        CheckExprStmt(CheckLookup("x")),
                        CheckLookup("abc"),
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryForNoClosingParen() {
            // } will trigger a recovery w/ an expression
            var code = @"
for(x in abc {
}

";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoRightParenthesis, true, new IndexSpan(15, 1))
                ),
                CheckBlock(
                    CheckForInStmt(
                        CheckExprStmt(CheckLookup("x")),
                        CheckLookup("abc"),
                        CheckBlock()
                    )
                )
            );
        }


        [TestMethod]
        public void RecoveryForBadBody() {
            // foo.'abc' will trigger a recovery w/ an expression which can be
            // promoted to an expression statement
            var code = @"
for(var i = 0; i< 4; i++) 
    foo.'abc'

{
}

";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(38, 5)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(50, 1))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock(
                            CheckExprStmt(
                                CheckLookup("foo")
                            )
                        )
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryForBadBody2() {
            // } will trigger a recovery w/ an expression
            var code = @"
for(var i = 0; i< 4; i++) 
    }

{
}

";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(34, 1)),
                    new ErrorInfo(JSError.SyntaxError, true, new IndexSpan(42, 1))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock(
                        )
                    )
                )
            );
        }


        [TestMethod]
        public void RecoveryForNoOpenParen() {
            var code = @"
for var i = 0; i<4; i++) {
}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoLeftParenthesis, true, new IndexSpan(6, 3))
                ),
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
        public void RecoveryForMissingClosingParen() {
            var code = @"
for(var i = 0; i<4; i++ {
}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoRightParenthesis, true, new IndexSpan(26, 1))
                ),
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
        public void RecoveryForMissingIncrement() {
            var code = @"
for(var i = 0; i<4) {
}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(20, 1))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        IsNullExpr,
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryForMissingConditionColon() {
            var code = @"
for(var i = 0 : ) {
}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(16, 1))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckConstant(true),
                        IsNullExpr,
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryForMissingConditionColonSemicolon() {
            var code = @"
for(var i = 0 : ; i < 4) {
}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(16, 1)),
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(25, 1))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        IsNullExpr,
                        CheckBlock()
                    )
                )
            );
        }

        [TestMethod]
        public void RecoveryForUnknownFollowingToken() {
            var code = @"
for(var i = 0, j = 0; < function() { } {
}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(24, 1)),
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(35, 1)),
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(37, 1))
                ),
                CheckBlock(
                    CheckExprStmt(CheckGrouping(IsNullExpr)),
                    CheckBlock(),
                    CheckBlock()
                )
            );
        }

        [TestMethod]
        public void RecoveryForMissingConditionMultipleVars() {
            var code = @"
for(var i = 0, j = 0) {
}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoSemicolon, true, new IndexSpan(22, 1))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero), CheckVarDecl("j", Zero)),
                        CheckConstant(true),
                        IsNullExpr,
                        CheckBlock()
                    )
                )
            );
        }

        /// <summary>
        /// let not in strict mode should be fine...
        /// </summary>
        [TestMethod]
        public void RecoveryForMissingCondition() {
            var code = @"
for(var i = 0;) {
}
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(16, 1))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckConstant(true),
                        IsNullExpr,
                        CheckBlock()
                    )
                )
            );

            

            code = @"
for(var i = 0;
";

            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.ExpressionExpected, true, new IndexSpan(18, 0))
                ),
                CheckBlock(
                )
            );
        }

        /// <summary>
        /// let not in strict mode should be fine...
        /// </summary>
        [TestMethod]
        public void ForMultipleVariables() {
            var code = @"
for(var i = 0, j = 1; i<4; i++) {
}
";

            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(
                            CheckVarDecl("i", Zero),
                            CheckVarDecl("j", One)
                        ),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock()
                    )
                )
            );
        }

        /// <summary>
        /// let not in strict mode should be fine...
        /// </summary>
        [TestMethod]
        public void LetNotInStrictMode() {
            var code = @"
    let = _(let).succ();
";

            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckBinaryStmt(
                        JSToken.Assign,
                        CheckLookup("let"),
                        CheckCall(
                            CheckMember(
                                "succ",
                                CheckCall(
                                    CheckLookup("_"),
                                    CheckLookup("let")
                                )
                            )
                        )
                    )
                )
            );
        }

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
    case ""abc"":
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
        public void TestVariableDeclarationKeyword() {
            const string code = @"
var get = 0;";
            CheckAst(
                ParseCode(code),
                CheckBlock(
                    CheckVar(
                        CheckVarDecl("get", Zero)
                    )
                )
            );
        }

        [TestMethod]
        public void TestVariableDeclarationError() {
            const string code = @"
var function = 0;";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(6, 8))
                ),
                CheckBlock(
                    CheckVar(
                        CheckVarDecl("function", Zero)
                    )
                )
            );
        }

        [TestMethod]
        public void TestVariableDeclarationError2() {
            const string code = @"
var 42 = 0;";
            CheckAst(
                ParseCode(
                    code,
                    new ErrorInfo(JSError.NoIdentifier, true, new IndexSpan(6, 2))
                ),
                CheckBlock(
                    CheckVar(),
                    CheckExprStmt(CheckConstant(42.0)),
                    CheckExprStmt(CheckAssign(CheckConstant(42.0), Zero))
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
        public void TestForStatementLexical() {
            const string ForCode = @"
for(const i = 0; false; ) { }
";
            CheckAst(
                ParseCode(ForCode),
                CheckBlock(
                    CheckForStmt(
                        CheckLexicalDecl(CheckVarDecl("i", Zero)),
                        CheckConstant(false),
                        IsNullExpr,
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
                ParseCode(
                    ForCode,
                    new ErrorInfo(JSError.NoLabel, true, new IndexSpan(38, 7))
                ),
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
        public void TestForContinueLabelStatement2() {
            const string ForCode = @"
for(var i = 0; i<4; i++) { 
myLabel:
continue myLabel; 
}
";
            CheckAst(
                ParseCode(
                    ForCode,
                    new ErrorInfo(JSError.BadContinue, true, new IndexSpan(41, 16))
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("i", Zero)),
                        CheckLessThan(I, Four),
                        CheckIncrement(I),
                        CheckBlock(
                            CheckLabel("myLabel", CheckContinue("myLabel"))                            
                        )
                    )                    
                )
            );
        }

        [TestMethod]
        public void TestForContinueLabelStatement3() {
            const string ForCode = @"
for(var j = 0; j<4; j++) {
    myLabel:
    for(var i = 0; i<4; i++) { 
        continue myLabel; 
    }
}
";
            CheckAst(
                ParseCode(
                    ForCode
                ),
                CheckBlock(
                    CheckForStmt(
                        CheckVar(CheckVarDecl("j", Zero)),
                        CheckLessThan(J, Four),
                        CheckIncrement(J),
                        CheckBlock(
                            CheckLabel(
                                "myLabel",
                                CheckForStmt(
                                    CheckVar(CheckVarDecl("i", Zero)),
                                    CheckLessThan(I, Four),
                                    CheckIncrement(I),
                                    CheckBlock(
                                        CheckContinue("myLabel")
                                    )
                                )
                            )
                        )
                    )
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
                ParseCode(
                    ForCode,
                    new ErrorInfo(JSError.NoLabel, true, new IndexSpan(35, 7))
                ),
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
                        CheckLessThan(I, One)
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
        private static Action<Expression> Null = CheckConstant(null);
        private static Action<Expression> True = CheckConstant(true);
        private static Action<Expression> False = CheckConstant(false);
        private static Action<Expression> I = CheckLookup("i");
        private static Action<Expression> J = CheckLookup("j");

        private static Action<Expression> IsNullExpr = expr => Assert.IsNull(expr);
        private static Action<Statement> IsNullStmt = stmt => Assert.IsNull(stmt);

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

        private Action<Statement> CheckTryFinally(Action<Statement> tryBody, Action<Statement> finallyBody) {
            return stmt => {
                Assert.AreEqual(typeof(TryNode), stmt.GetType());

                var tryNode = (TryNode)stmt;
                tryBody(tryNode.TryBlock);
                Assert.IsNull(tryNode.CatchBlock);
                Assert.IsNull(tryNode.CatchParameter);
                finallyBody(tryNode.FinallyBlock);
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
            return CheckBinary(JSToken.LessThan, operand1, operand2);
        }

        private static Action<Expression> CheckAssign(Action<Expression> operand1, Action<Expression> operand2) {
            return CheckBinary(JSToken.Assign, operand1, operand2);
        }

        private static Action<Expression> CheckBinary(JSToken token, Action<Expression> operand1, Action<Expression> operand2) {
            return expr => {
                Assert.AreEqual(typeof(BinaryOperator), expr.GetType());
                var bin = (BinaryOperator)expr;
                Assert.AreEqual(token, bin.OperatorToken);
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

        private static Action<Expression> CheckCall(Action<Expression> function, params Action<Expression>[] args) {
            return expr => {
                Assert.AreEqual(typeof(CallNode), expr.GetType());


                var callNode = (CallNode)expr;
                function(callNode.Function);

                Assert.IsFalse(callNode.IsConstructor);
                Assert.IsFalse(callNode.InBrackets);

                Assert.AreEqual(args.Length, callNode.Arguments.Count);
                for (int i = 0; i < args.Length; i++) {
                    args[i](callNode.Arguments[i]);
                }
            };
        }

        private static Action<Expression> CheckIndex(Action<Expression> function, params Action<Expression>[] args) {
            return expr => {
                Assert.AreEqual(typeof(CallNode), expr.GetType());


                var callNode = (CallNode)expr;
                function(callNode.Function);

                Assert.IsFalse(callNode.IsConstructor);
                Assert.IsTrue(callNode.InBrackets);

                Assert.AreEqual(args.Length, callNode.Arguments.Count);
                for (int i = 0; i < args.Length; i++) {
                    args[i](callNode.Arguments[i]);
                }
            };
        }

        private static Action<Expression> CheckMember(string name, Action<Expression> root) {
            return expr => {
                Assert.AreEqual(typeof(Member), expr.GetType());

                var member = (Member)expr;
                Assert.AreEqual(name, member.Name);
                root(member.Root);
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

        private static Action<Statement> CheckLexicalDecl(params Action<VariableDeclaration>[] decls) {
            return expr => {
                Assert.AreEqual(typeof(LexicalDeclaration), expr.GetType());
                var varNode = (LexicalDeclaration)expr;
                Assert.AreEqual(varNode.Count, decls.Length);
                for (int i = 0; i < decls.Length; i++) {
                    decls[i](varNode[i]);
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
                        throw new AssertFailedException(String.Format("Block Item {0}: {1}", i, e.Message + Environment.NewLine + e.StackTrace.ToString()), e);
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
                if (func.ParameterDeclarations != null) {
                Assert.AreEqual(args.Length, func.ParameterDeclarations.Count);
                for (int i = 0; i < func.ParameterDeclarations.Count; i++) {
                    args[i](func.ParameterDeclarations[i]);
                }
                } else {
                    Assert.IsNull(args);
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
        private Action<Statement> CheckReturn(Action<Expression> operand = null) {
            return stmt => {
                Assert.AreEqual(typeof(ReturnNode), stmt.GetType());

                var ret = (ReturnNode)stmt;
                if (ret.Operand == null) {
                    Assert.IsNull(operand);
                } else {
                    Assert.IsNotNull(operand);
                    operand(ret.Operand);
                }
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

        private static JsAst ParseCode(string code, params ErrorInfo[] errors) {
            return ParseCode(code, false, errors);
        }

        private static JsAst ParseCode(string code, bool collectWarnings, params ErrorInfo[] errors) {
            CollectingErrorSink errorSink = new CollectingErrorSink(collectWarnings);

            var parser = new JSParser(code, errorSink);
            var ast = parser.Parse(new CodeSettings());

            errorSink.CheckErrors(errors);
            return ast;
        }

        private static JsAst ParseExpression(string code, bool collectWarnings, params ErrorInfo[] errors) {
            CollectingErrorSink errorSink = new CollectingErrorSink(collectWarnings);

            var parser = new JSParser(code, errorSink);
            var ast = parser.Parse(new CodeSettings() { SourceMode = JavaScriptSourceMode.Expression });

            errorSink.CheckErrors(errors);
            return ast;
        }
        

        private void CheckAst(JsAst ast, Action<Statement> checkBody) {
            checkBody(ast.Block);

            ast.Walk(new TestVisitor());
        }

        class TestVisitor : AstVisitor {
            private void TestNode(Node node) {
                if (node != null) {
                    Assert.IsNotNull(node.ToString());
                    foreach (var child in node.Children) {
                        Assert.IsNotNull(child);
                    }

                    IEnumerable enumerable = node as IEnumerable;
                    if (enumerable != null) {
                        foreach (var value in enumerable) {
                            Assert.IsNotNull(value);
                        }
                    }
                }
            }

            public override bool Walk(ArrayLiteral node) { TestNode(node); TestNode(node.Elements);  return base.Walk(node); }
            public override bool Walk(BinaryOperator node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(CommaOperator node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(Block node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(Break node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(CallNode node) { TestNode(node); TestNode(node.Arguments);  return base.Walk(node); }
            public override bool Walk(Conditional node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(ConstantWrapper node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(ConstStatement node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(ContinueNode node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(DebuggerNode node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(DirectivePrologue node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(DoWhile node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(EmptyStatement node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(ForIn node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(ForNode node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(FunctionObject node) { TestNode(node); TestNode(node.ParameterDeclarations);  return base.Walk(node); }
            public override bool Walk(GetterSetter node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(GroupingOperator node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(IfNode node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(LabeledStatement node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(LexicalDeclaration node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(Lookup node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(Member node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(ObjectLiteral node) { TestNode(node); TestNode(node.Properties);  return base.Walk(node); }
            public override bool Walk(ObjectLiteralField node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(ObjectLiteralProperty node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(ParameterDeclaration node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(RegExpLiteral node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(ReturnNode node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(Switch node) { TestNode(node); TestNode(node.Cases); return base.Walk(node); }
            public override bool Walk(SwitchCase node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(ThisLiteral node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(ThrowNode node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(TryNode node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(Var node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(VariableDeclaration node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(UnaryOperator node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(WhileNode node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(WithNode node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(JsAst jsAst) { TestNode(jsAst); return base.Walk(jsAst); }
            public override bool Walk<T>(AstNodeList<T> node) { TestNode(node); return base.Walk(node); }
            public override bool Walk(FunctionExpression functionExpression) { TestNode(functionExpression); return base.Walk(functionExpression); }
            public override bool Walk(ExpressionStatement node) { TestNode(node); return base.Walk(node); }
        }
    }

    class ErrorInfo {
        public readonly bool IsError;
        public readonly JSError Error;
        public readonly IndexSpan Span;

        public ErrorInfo(JSError error, bool isError, IndexSpan span) {
            Error = error;
            IsError = isError;
            Span = span;
        }

        public override string ToString() {
            return String.Format("{0}: JSError.{1} {2}",
                IsError ? "Error" : "Warning",
                Error,
                Span
            );
        }
    }

    class CollectingErrorSink : ErrorSink {
        private readonly List<ErrorInfo> _errors = new List<ErrorInfo>();
        public bool _collectWarnings;

        public CollectingErrorSink(bool collectWarnings = false) {
            _collectWarnings = collectWarnings;
        }

        public override void OnError(JScriptExceptionEventArgs error) {
            error.Error.ToString();
            var result = new ErrorInfo(error.Error.ErrorCode, error.Error.IsError, error.Exception.Span);

            if (error.Error.IsError || _collectWarnings) {
                _errors.Add(result);
            }
        }

        public void CheckErrors(ErrorInfo[] errors) {
            bool success = false;
            try {
                Assert.AreEqual(errors.Length, Errors.Count);
                for (int i = 0; i < errors.Length; i++) {
                    Assert.AreEqual(errors[i].ToString(), Errors[i].ToString());
                }
                success = true;
            } finally {
                if (!success) {
                    foreach (var error in Errors) {
                        Console.WriteLine(
                            "new ErrorInfo(JSError.{0}, {1}, new IndexSpan({2}, {3})),",
                            error.Error,
                            error.IsError ? "true" : "false",
                            error.Span.Start,
                            error.Span.Length
                        );
                    }
                }
            }
        }

        public List<ErrorInfo> Errors {
            get {
                return _errors;
            }
        }
    }
}
