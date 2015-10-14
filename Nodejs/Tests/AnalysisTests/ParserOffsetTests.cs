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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalysisTests {
    [TestClass]
    public class ParserOffsetTests {

        [TestMethod, Priority(0)]
        public void TestArrayLiteral() {
            TestOneSnippet(
                "[1,2,3]",
                new NodeInfo(typeof(Block), 0, 7),
                new NodeInfo(typeof(ExpressionStatement), 0, 7),
                new NodeInfo(typeof(ArrayLiteral), 0, 7),
                new NodeInfo(typeof(ConstantWrapper), 1, 2),
                new NodeInfo(typeof(ConstantWrapper), 3, 4),
                new NodeInfo(typeof(ConstantWrapper), 5, 6)
            );
        }

        [TestMethod, Priority(0)]
        public void TestBinaryOperator() {
            TestOneSnippet(
                "1 + 2",
                new NodeInfo(typeof(Block), 0, 5),
                new NodeInfo(typeof(ExpressionStatement), 0, 5),
                new NodeInfo(typeof(BinaryOperator), 0, 5),
                new NodeInfo(typeof(ConstantWrapper), 0, 1),
                new NodeInfo(typeof(ConstantWrapper), 4, 5)
            );
            TestOneSnippet(
                "1+2",
                new NodeInfo(typeof(Block), 0, 3),
                new NodeInfo(typeof(ExpressionStatement), 0, 3),
                new NodeInfo(typeof(BinaryOperator), 0, 3),
                new NodeInfo(typeof(ConstantWrapper), 0, 1),
                new NodeInfo(typeof(ConstantWrapper), 2, 3)
            );
        }

        [TestMethod, Priority(0)]
        public void TestLoopBreakContinue() {
            TestOneSnippet(
                @"for(var i = 0; i<10; i++) {
    break;
    continue;
}",
                new NodeInfo(typeof(Block), 0, 57),
                new NodeInfo(typeof(ForNode), 0, 57),
                new NodeInfo(typeof(Var), 4, 13),
                new NodeInfo(typeof(VariableDeclaration), 8, 13),
                new NodeInfo(typeof(ConstantWrapper), 12, 13),
                new NodeInfo(typeof(BinaryOperator), 15, 19),
                new NodeInfo(typeof(Lookup), 15, 16),
                new NodeInfo(typeof(ConstantWrapper), 17, 19),
                new NodeInfo(typeof(Block), 26, 57),
                new NodeInfo(typeof(Break), 33, 39),
                new NodeInfo(typeof(ContinueNode), 45, 54),
                new NodeInfo(typeof(UnaryOperator), 21, 24),
                new NodeInfo(typeof(Lookup), 21, 22)
            );
        }

        [TestMethod, Priority(0)]
        public void TestCommaOperator() {
            TestOneSnippet("1,2,3",
                new NodeInfo(typeof(Block), 0, 5),
                new NodeInfo(typeof(ExpressionStatement), 0, 5),
                new NodeInfo(typeof(CommaOperator), 0, 5),
                new NodeInfo(typeof(ConstantWrapper), 0, 1),
                new NodeInfo(typeof(ConstantWrapper), 2, 3),
                new NodeInfo(typeof(ConstantWrapper), 4, 5)
            );
        }

        
        [TestMethod, Priority(0)]
        public void TestCallNode() {
            TestOneSnippet("f(1,2,3)",
                new NodeInfo(typeof(Block), 0, 8),
                new NodeInfo(typeof(ExpressionStatement), 0, 8),
                new NodeInfo(typeof(CallNode), 0, 8),
                new NodeInfo(typeof(Lookup), 0, 1),
                new NodeInfo(typeof(ConstantWrapper), 2, 3),
                new NodeInfo(typeof(ConstantWrapper), 4, 5),
                new NodeInfo(typeof(ConstantWrapper), 6, 7)
            );

            TestOneSnippet("new f(1,2,3)",
                new NodeInfo(typeof(Block), 0, 12),
                new NodeInfo(typeof(ExpressionStatement), 0, 12),
                new NodeInfo(typeof(CallNode), 0, 12),
                new NodeInfo(typeof(Lookup), 4, 5),
                new NodeInfo(typeof(ConstantWrapper), 6, 7),
                new NodeInfo(typeof(ConstantWrapper), 8, 9),
                new NodeInfo(typeof(ConstantWrapper), 10, 11)
            );

            TestOneSnippet("f[0]",
                new NodeInfo(typeof(Block), 0, 4),
                new NodeInfo(typeof(ExpressionStatement), 0, 4),
                new NodeInfo(typeof(CallNode), 0, 4),
                new NodeInfo(typeof(Lookup), 0, 1),
                new NodeInfo(typeof(ConstantWrapper), 2, 3)
            );
        }

        [TestMethod, Priority(0)]
        public void TestConditional() {
            TestOneSnippet("true ? 1 : 0",
                new NodeInfo(typeof(Block), 0, 12),
                new NodeInfo(typeof(ExpressionStatement), 0, 12),
                new NodeInfo(typeof(Conditional), 0, 12),
                new NodeInfo(typeof(ConstantWrapper), 0, 4),
                new NodeInfo(typeof(ConstantWrapper), 7, 8),
                new NodeInfo(typeof(ConstantWrapper), 11, 12)
            );

        }

        [TestMethod, Priority(0)]
        public void TestConstant() {
            TestOneSnippet("'abc'",
                new NodeInfo(typeof(Block), 0, 5),
                new NodeInfo(typeof(ExpressionStatement), 0, 5),
                new NodeInfo(typeof(DirectivePrologue), 0, 5)
            );
            TestOneSnippet("10.0",
                new NodeInfo(typeof(Block), 0, 4),
                new NodeInfo(typeof(ExpressionStatement), 0, 4),
                new NodeInfo(typeof(ConstantWrapper), 0, 4)
            );
        }

        [TestMethod, Priority(0)]
        public void TestConstStatement() {
            TestOneSnippet("const x = 42;",
                new NodeInfo(typeof(Block), 0, 13),
                new NodeInfo(typeof(LexicalDeclaration), 0, 13),
                new NodeInfo(typeof(VariableDeclaration), 6, 12),
                new NodeInfo(typeof(ConstantWrapper), 10, 12)
            );
        }

        [TestMethod, Priority(0)]
        public void TestDebuggerStatement() {
            TestOneSnippet("debugger;",
                new NodeInfo(typeof(Block), 0, 9),
                new NodeInfo(typeof(DebuggerNode), 0, 9)
            );
        }

        [TestMethod, Priority(0)]
        public void TestDoWhile() {
            TestOneSnippet(@"do {
}while(true);",
                new NodeInfo(typeof(Block), 0, 19),
                new NodeInfo(typeof(DoWhile), 0, 18),
                new NodeInfo(typeof(ConstantWrapper), 13, 17),
                new NodeInfo(typeof(Block), 3, 7)
            );
        }
        
        [TestMethod, Priority(0)]
        public void TestEmptyStatement() {
            TestOneSnippet(";",
                new NodeInfo(typeof(Block), 0, 1),
                new NodeInfo(typeof(EmptyStatement), 0, 1)
            );
        }

        [TestMethod, Priority(0)]
        public void TestForIn() {
            TestOneSnippet(@"for(var x in abc) {
}",
                new NodeInfo(typeof(Block), 0, 22),
                new NodeInfo(typeof(ForIn), 0, 22),
                new NodeInfo(typeof(Lookup), 13, 16),
                new NodeInfo(typeof(Var), 4, 9),
                new NodeInfo(typeof(VariableDeclaration), 8, 9),
                new NodeInfo(typeof(Block), 18, 22)

            );
        }

        [TestMethod, Priority(0)]
        public void TestFor() {
            TestOneSnippet(@"for(var x = 0; x<100; x++) {
}",
                new NodeInfo(typeof(Block), 0, 31),
                new NodeInfo(typeof(ForNode), 0, 31),
                new NodeInfo(typeof(Var), 4, 13),
                new NodeInfo(typeof(VariableDeclaration), 8, 13),
                new NodeInfo(typeof(ConstantWrapper), 12, 13),
                new NodeInfo(typeof(BinaryOperator), 15, 20),
                new NodeInfo(typeof(Lookup), 15, 16),
                new NodeInfo(typeof(ConstantWrapper), 17, 20),
                new NodeInfo(typeof(Block), 27, 31),
                new NodeInfo(typeof(UnaryOperator), 22, 25),
                new NodeInfo(typeof(Lookup), 22, 23)
            );
            TestOneSnippet(@"for(x = 0; x<100; x++) {
}",
                new NodeInfo(typeof(Block), 0, 27),
                new NodeInfo(typeof(ForNode), 0, 27),
                new NodeInfo(typeof(ExpressionStatement), 4, 9),
                new NodeInfo(typeof(BinaryOperator), 4, 9),
                new NodeInfo(typeof(Lookup), 4, 5),
                new NodeInfo(typeof(ConstantWrapper), 8, 9),
                new NodeInfo(typeof(BinaryOperator), 11, 16),
                new NodeInfo(typeof(Lookup), 11, 12),
                new NodeInfo(typeof(ConstantWrapper), 13, 16),
                new NodeInfo(typeof(Block), 23, 27),
                new NodeInfo(typeof(UnaryOperator), 18, 21),
                new NodeInfo(typeof(Lookup), 18, 19)
            );
        }

        [TestMethod, Priority(0)]
        public void TestFunction() {
            TestOneSnippet(@"function f(a, b, c) {
}",
                new NodeInfo(typeof(Block), 0, 24),
                new NodeInfo(typeof(FunctionObject), 0, 24, 9, 10, 19),
                new NodeInfo(typeof(ParameterDeclaration), 11, 12),
                new NodeInfo(typeof(ParameterDeclaration), 14, 15),
                new NodeInfo(typeof(ParameterDeclaration), 17, 18),
                new NodeInfo(typeof(Block), 20, 24)
            );
        }

        [TestMethod, Priority(0)]
        public void TestGetSet() {
            TestOneSnippet(@"x = {get abc () { 42 }}",
                new NodeInfo(typeof(Block), 0, 23),
                new NodeInfo(typeof(ExpressionStatement), 0, 23),
                new NodeInfo(typeof(BinaryOperator), 0, 23),
                new NodeInfo(typeof(Lookup), 0, 1),
                new NodeInfo(typeof(ObjectLiteral), 4, 23),
                new NodeInfo(typeof(ObjectLiteralProperty), 5, 22),
                new NodeInfo(typeof(GetterSetter), 5, 8),
                new NodeInfo(typeof(FunctionExpression), 5, 22),
                new NodeInfo(typeof(FunctionObject), 5, 22, 9, 13, 14),
                new NodeInfo(typeof(Block), 16, 22),
                new NodeInfo(typeof(ExpressionStatement), 18, 20),
                new NodeInfo(typeof(ConstantWrapper), 18, 20)
            );
            TestOneSnippet(@"x = {set abc (value) { }}",
                new NodeInfo(typeof(Block), 0, 25),
                new NodeInfo(typeof(ExpressionStatement), 0, 25),
                new NodeInfo(typeof(BinaryOperator), 0, 25),
                new NodeInfo(typeof(Lookup), 0, 1),
                new NodeInfo(typeof(ObjectLiteral), 4, 25),
                new NodeInfo(typeof(ObjectLiteralProperty), 5, 24),
                new NodeInfo(typeof(GetterSetter), 5, 8),
                new NodeInfo(typeof(FunctionExpression), 5, 24),
                new NodeInfo(typeof(FunctionObject), 5, 24, 9, 13, 20),
                new NodeInfo(typeof(ParameterDeclaration), 14, 19),
                new NodeInfo(typeof(Block), 21, 24)
            );
        }

        [TestMethod, Priority(0)]
        public void TestGrouping() {
            TestOneSnippet(@"(x)",
                new NodeInfo(typeof(Block), 0, 3),
                new NodeInfo(typeof(ExpressionStatement), 0, 3),
                new NodeInfo(typeof(GroupingOperator), 0, 3),
                new NodeInfo(typeof(Lookup), 1, 2)
            );
        }

        [TestMethod, Priority(0)]
        public void TestIf() {
            TestOneSnippet(@"if(true) {
} else {
}",
                new NodeInfo(typeof(Block), 0, 23),
                new NodeInfo(typeof(IfNode), 0, 23),
                new NodeInfo(typeof(ConstantWrapper), 3, 7),
                new NodeInfo(typeof(Block), 9, 13),
                new NodeInfo(typeof(Block), 19, 23)
            );
        }

        [TestMethod, Priority(0)]
        public void TestLabeledStatement() {
            TestOneSnippet(@"myLabel:
console.log('hi');",
                new NodeInfo(typeof(Block), 0, 28),
                new NodeInfo(typeof(LabeledStatement), 0, 28),
                new NodeInfo(typeof(ExpressionStatement), 10, 28),
                new NodeInfo(typeof(CallNode), 10, 27),
                new NodeInfo(typeof(Member), 10, 21),
                new NodeInfo(typeof(Lookup), 10, 17),
                new NodeInfo(typeof(ConstantWrapper), 22, 26)
            );
        }

        [TestMethod, Priority(0)]
        public void TestLexicalDeclaration() {
            TestOneSnippet(@"'use strict';
let x = 42;",
                new NodeInfo(typeof(Block), 0, 26),
                new NodeInfo(typeof(ExpressionStatement), 0, 12),
                new NodeInfo(typeof(DirectivePrologue), 0, 13),
                new NodeInfo(typeof(LexicalDeclaration), 15, 26),
                new NodeInfo(typeof(VariableDeclaration), 19, 25),
                new NodeInfo(typeof(ConstantWrapper), 23, 25)
            );
        }

        [TestMethod, Priority(0)]
        public void TestBogusLexicalDeclaration() {
            TestOneSnippet(@"let x = 42;",
                new NodeInfo(typeof(Block), 0, 11),
                new NodeInfo(typeof(ExpressionStatement), 0, 3),
                new NodeInfo(typeof(Lookup), 0, 3),
                new NodeInfo(typeof(ExpressionStatement), 4, 11),
                new NodeInfo(typeof(BinaryOperator), 4, 10),
                new NodeInfo(typeof(Lookup), 4, 5),
                new NodeInfo(typeof(ConstantWrapper), 8, 10)
            );
        }

        [TestMethod, Priority(0)]
        public void TestLookup() {
            TestOneSnippet(@"x",
                new NodeInfo(typeof(Block), 0, 1),
                new NodeInfo(typeof(ExpressionStatement), 0, 1),
                new NodeInfo(typeof(Lookup), 0, 1)
            );
            TestOneSnippet(@"xyz",
                new NodeInfo(typeof(Block), 0, 3),
                new NodeInfo(typeof(ExpressionStatement), 0, 3),
                new NodeInfo(typeof(Lookup), 0, 3)
            );
        }

        [TestMethod, Priority(0)]
        public void TestMember() {
            TestOneSnippet(@"x.abc",
                new NodeInfo(typeof(Block), 0, 5),
                new NodeInfo(typeof(ExpressionStatement), 0, 5),
                new NodeInfo(typeof(Member), 0, 5),
                new NodeInfo(typeof(Lookup), 0, 1)
            );
        }

        [TestMethod, Priority(0)]
        public void TestObjectLiteral() {
            TestOneSnippet(@"{abc:42, aaa:100}",
                new NodeInfo(typeof(Block), 0, 17),
                new NodeInfo(typeof(Block), 0, 17),
                new NodeInfo(typeof(LabeledStatement), 1, 12),
                new NodeInfo(typeof(ExpressionStatement), 5, 12),
                new NodeInfo(typeof(CommaOperator), 5, 12),
                new NodeInfo(typeof(ConstantWrapper), 5, 7),
                new NodeInfo(typeof(Lookup), 9, 12)
            );
        }

        [TestMethod, Priority(0)]
        public void TestRegexpLiteral() {
            TestOneSnippet(@"/foo/",
                new NodeInfo(typeof(Block), 0, 5),
                new NodeInfo(typeof(ExpressionStatement), 0, 5),
                new NodeInfo(typeof(RegExpLiteral), 0, 5)
            );
        }

        [TestMethod, Priority(0)]
        public void TestReturn() {
            TestOneSnippet(@"function f() {
    return 42;
}",
                new NodeInfo(typeof(Block), 0, 33),
                new NodeInfo(typeof(FunctionObject), 0, 33, 9, 10, 11),
                new NodeInfo(typeof(Block), 13, 33),
                new NodeInfo(typeof(ReturnNode), 20, 30),
                new NodeInfo(typeof(ConstantWrapper), 27, 29)
            );
            TestOneSnippet(@"function f() {
    return;
}",
                new NodeInfo(typeof(Block), 0, 30),
                new NodeInfo(typeof(FunctionObject), 0, 30, 9, 10, 11),
                new NodeInfo(typeof(Block), 13, 30),
                new NodeInfo(typeof(ReturnNode), 20, 27)
            );
        }

        [TestMethod, Priority(0)]
        public void TestSwitch() {
            TestOneSnippet(@"switch(abc) {
    case 42:
        break;
    case 'abc':
        break;
    default:
        break;
}",
                new NodeInfo(typeof(Block), 0, 109),
                new NodeInfo(typeof(Switch), 0, 109),
                new NodeInfo(typeof(Lookup), 7, 10),
                new NodeInfo(typeof(SwitchCase), 19, 43),
                new NodeInfo(typeof(ConstantWrapper), 24, 26),
                new NodeInfo(typeof(Block), 37, 43),
                new NodeInfo(typeof(Break), 37, 43),
                new NodeInfo(typeof(SwitchCase), 49, 76),
                new NodeInfo(typeof(ConstantWrapper), 54, 59),
                new NodeInfo(typeof(Block), 70, 76),
                new NodeInfo(typeof(Break), 70, 76),
                new NodeInfo(typeof(SwitchCase), 82, 106),
                new NodeInfo(typeof(Block), 100, 106),
                new NodeInfo(typeof(Break), 100, 106)
            );
        }

        [TestMethod, Priority(0)]
        public void TestThis() {
            TestOneSnippet(@"var x = this.foo;",
                new NodeInfo(typeof(Block), 0, 17),
                new NodeInfo(typeof(Var), 0, 17),
                new NodeInfo(typeof(VariableDeclaration), 4, 16),
                new NodeInfo(typeof(Member), 8, 16),
                new NodeInfo(typeof(ThisLiteral), 8, 12)
            );
        }

        [TestMethod, Priority(0)]
        public void TestThrow() {
            TestOneSnippet(@"throw 'error';",
                new NodeInfo(typeof(Block), 0, 14),
                new NodeInfo(typeof(ThrowNode), 0, 14),
                new NodeInfo(typeof(ConstantWrapper), 6, 13)
            );
        }

        [TestMethod, Priority(0)]
        public void TestTryCatch() {
            TestOneSnippet(@"try {
    x
} catch(arg) {
}",
                new NodeInfo(typeof(Block), 0, 31),
                new NodeInfo(typeof(TryNode), 0, 31),
                new NodeInfo(typeof(Block), 4, 15),
                new NodeInfo(typeof(ExpressionStatement), 11, 12),
                new NodeInfo(typeof(Lookup), 11, 12),
                new NodeInfo(typeof(ParameterDeclaration), 22, 25),
                new NodeInfo(typeof(Block), 27, 31)
            );
        }

        [TestMethod, Priority(0)]
        public void TestTryFinally() {
            TestOneSnippet(@"try {
    x
} finally {
}",
                new NodeInfo(typeof(Block), 0, 28),
                new NodeInfo(typeof(TryNode), 0, 28),
                new NodeInfo(typeof(Block), 4, 15),
                new NodeInfo(typeof(ExpressionStatement), 11, 12),
                new NodeInfo(typeof(Lookup), 11, 12),
                new NodeInfo(typeof(Block), 24, 28)
            );
        }

        [TestMethod, Priority(0)]
        public void TestTryCatchFinally() {
            TestOneSnippet(@"try {
    x
}catch(arg) {
} finally {
}",
                new NodeInfo(typeof(Block), 0, 43),
                new NodeInfo(typeof(TryNode), 0, 43),
                new NodeInfo(typeof(Block), 4, 15),
                new NodeInfo(typeof(ExpressionStatement), 11, 12),
                new NodeInfo(typeof(Lookup), 11, 12),
                new NodeInfo(typeof(ParameterDeclaration), 21, 24),
                new NodeInfo(typeof(Block), 26, 30),
                new NodeInfo(typeof(Block), 39, 43)
            );
        }

        [TestMethod, Priority(0)]
        public void TestVariableDeclaration() {
            TestOneSnippet(@"var abc = 42, foo = 100;",
                new NodeInfo(typeof(Block), 0, 24),
                new NodeInfo(typeof(Var), 0, 24),
                new NodeInfo(typeof(VariableDeclaration), 4, 12),
                new NodeInfo(typeof(ConstantWrapper), 10, 12),
                new NodeInfo(typeof(VariableDeclaration), 14, 23),
                new NodeInfo(typeof(ConstantWrapper), 20, 23)
            );
        }

        [TestMethod, Priority(0)]
        public void TestUnaryOperator() {
            TestOneSnippet(@"+42",
                new NodeInfo(typeof(Block), 0, 3),
                new NodeInfo(typeof(ExpressionStatement), 0, 3),
                new NodeInfo(typeof(UnaryOperator), 0, 3),
                new NodeInfo(typeof(ConstantWrapper), 1, 3)
            );
        }

        [TestMethod, Priority(0)]
        public void TestWhileNode() {
            TestOneSnippet(@"while(foo) { hi; }",
                new NodeInfo(typeof(Block), 0, 18),
                new NodeInfo(typeof(WhileNode), 0, 18),
                new NodeInfo(typeof(Lookup), 6, 9),
                new NodeInfo(typeof(Block), 11, 18),
                new NodeInfo(typeof(ExpressionStatement), 13, 16),
                new NodeInfo(typeof(Lookup), 13, 15)
            );
        }

        [TestMethod, Priority(0)]
        public void TestWithNode() {
            TestOneSnippet(@"with(foo) { hi; }",
                new NodeInfo(typeof(Block), 0, 17),
                new NodeInfo(typeof(WithNode), 0, 17),
                new NodeInfo(typeof(Lookup), 5, 8),
                new NodeInfo(typeof(Block), 10, 17),
                new NodeInfo(typeof(ExpressionStatement), 12, 15),
                new NodeInfo(typeof(Lookup), 12, 14)
            );
        }

        [TestMethod, Priority(0)]
        public void TestFunctionExpression() {
            TestOneSnippet(@"x = function() { }",
                new NodeInfo(typeof(Block), 0, 18),
                new NodeInfo(typeof(ExpressionStatement), 0, 18),
                new NodeInfo(typeof(BinaryOperator), 0, 18),
                new NodeInfo(typeof(Lookup), 0, 1),
                new NodeInfo(typeof(FunctionExpression), 4, 18),
                new NodeInfo(typeof(FunctionObject), 4, 18, 0, 12, 13),
                new NodeInfo(typeof(Block), 15, 18)
            );
        }

        [TestMethod, Priority(0)]
        public void TestFunctionExpressionWithName() {
            TestOneSnippet(@"x = function name() { }",
                new NodeInfo(typeof(Block), 0, 23),
                new NodeInfo(typeof(ExpressionStatement), 0, 23),
                new NodeInfo(typeof(BinaryOperator), 0, 23),
                new NodeInfo(typeof(Lookup), 0, 1),
                new NodeInfo(typeof(FunctionExpression), 4, 23),
                new NodeInfo(typeof(FunctionObject), 4, 23, 13, 17, 18),
                new NodeInfo(typeof(Block), 20, 23)
            );
        }

        private void TestOneSnippet(string code, params NodeInfo[] nodes) {
            bool success = false;
            var result = ParseTreeWalker.Parse(code);
            try {
                Assert.AreEqual(nodes.Length, result.Count);
                for (int i = 0; i < nodes.Length; i++) {
                    Assert.AreEqual(nodes[i], result[i]);
                }
                success = true;
            } finally {
                if (!success) {
                    for (int i = 0; i < result.Count; i++) {
                        Console.Write(result[i]);
                        if (i == result.Count - 1) {
                            Console.WriteLine();
                        } else {
                            Console.WriteLine(",");
                        }
                    }
                }
            }
        }

        class NodeInfo {
            public readonly Type NodeType;
            public readonly int Start, End;
            public readonly int[] ExtraIndicies;

            public NodeInfo(Type type, int start, int end, params int[] extraIndicies) {
                NodeType = type;
                Start = start;
                End = end;
                ExtraIndicies = extraIndicies;
            }

            public override int GetHashCode() {
                // not a good hash code, but we don't need it, we never hash these.
                return NodeType.GetHashCode();
            }

            public override bool Equals(object obj) {
                NodeInfo other = obj as NodeInfo;
                if (other == null) {
                    return false;
                }
                if (other.NodeType == NodeType &&
                    other.Start == Start &&
                    other.End == End) {
                    if (other.ExtraIndicies.Length == ExtraIndicies.Length) {
                        for (int i = 0; i < other.ExtraIndicies.Length; i++) {
                            if (other.ExtraIndicies[i] != ExtraIndicies[i]) {
                                return false;
                            }
                        }
                        return true;
                    }
                    return false;
                }
                return false;
            }

            public override string ToString() {
                if (ExtraIndicies == null || ExtraIndicies.Length == 0) {
                    return String.Format("new NodeInfo(typeof({0}), {1}, {2})", NodeType.Name, Start, End);
                }

                var res = String.Format("new NodeInfo(typeof({0}), {1}, {2}", NodeType.Name, Start, End);
                for (int i = 0; i < ExtraIndicies.Length; i++) {
                    res = res + ", " + ExtraIndicies[i];
                }
                res += ")";
                return res;
            }
        }

        class ParseTreeWalker : AstVisitor {
            public readonly List<NodeInfo> Nodes = new List<NodeInfo>();
            private readonly JsAst _tree;

            public ParseTreeWalker(JsAst tree) {
                _tree = tree;
            }

            public static List<NodeInfo> Parse(string code) {
                var ast = ParseCode(code);
                var walker = new ParseTreeWalker(ast);
                ast.Walk(walker);
                return walker.Nodes;
            }

            private static JsAst ParseCode(string code) {
                var parser = new JSParser(code);
                var ast = parser.Parse(new CodeSettings());
                return ast;
            }

            private void AddNode(Node node, params int[] extraIndicies) {
                Nodes.Add(
                    new NodeInfo(
                        node.GetType(), 
                        node.GetStartIndex(_tree.LocationResolver), 
                        node.GetEndIndex(_tree.LocationResolver), 
                        extraIndicies
                    )
                );
            }

            public override bool Walk(ArrayLiteral node) { AddNode(node); return true; }
            public override bool Walk(BinaryOperator node) { AddNode(node); return true; }
            public override bool Walk(CommaOperator node) { AddNode(node); return true; }
            public override bool Walk(Block node) { AddNode(node); return true; }
            public override bool Walk(Break node) { AddNode(node); return true; }
            public override bool Walk(CallNode node) { AddNode(node); return true; }
            public override bool Walk(Conditional node) { AddNode(node); return true; }
            public override bool Walk(ConstantWrapper node) { AddNode(node); return true; }
            public override bool Walk(ConstStatement node) { AddNode(node); return true; }
            public override bool Walk(ContinueNode node) { AddNode(node); return true; }
            public override bool Walk(DebuggerNode node) { AddNode(node); return true; }
            public override bool Walk(DirectivePrologue node) { AddNode(node); return true; }
            public override bool Walk(DoWhile node) { AddNode(node); return true; }
            public override bool Walk(EmptyStatement node) { AddNode(node); return true; }
            public override bool Walk(ForIn node) { AddNode(node); return true; }
            public override bool Walk(ForNode node) { AddNode(node); return true; }
            public override bool Walk(FunctionObject node) { AddNode(node, node.GetNameSpan(_tree.LocationResolver).Start, node.ParameterStart, node.ParameterEnd); return true; }
            public override bool Walk(GetterSetter node) { AddNode(node); return true; }
            public override bool Walk(GroupingOperator node) { AddNode(node); return true; }
            public override bool Walk(IfNode node) { AddNode(node); return true; }
            public override bool Walk(LabeledStatement node) { AddNode(node); return true; }
            public override bool Walk(LexicalDeclaration node) { AddNode(node); return true; }
            public override bool Walk(Lookup node) { AddNode(node); return true; }
            public override bool Walk(Member node) { AddNode(node); return true; }
            public override bool Walk(ObjectLiteral node) { AddNode(node); return true; }
            public override bool Walk(ObjectLiteralField node) { AddNode(node); return true; }
            public override bool Walk(ObjectLiteralProperty node) { AddNode(node); return true; }
            public override bool Walk(ParameterDeclaration node) { AddNode(node); return true; }
            public override bool Walk(RegExpLiteral node) { AddNode(node); return true; }
            public override bool Walk(ReturnNode node) { AddNode(node); return true; }
            public override bool Walk(Switch node) { AddNode(node); return true; }
            public override bool Walk(SwitchCase node) { AddNode(node); return true; }
            public override bool Walk(ThisLiteral node) { AddNode(node); return true; }
            public override bool Walk(ThrowNode node) { AddNode(node); return true; }
            public override bool Walk(TryNode node) { AddNode(node); return true; }
            public override bool Walk(Var node) { AddNode(node); return true; }
            public override bool Walk(VariableDeclaration node) { AddNode(node); return true; }
            public override bool Walk(UnaryOperator node) { AddNode(node); return true; }
            public override bool Walk(WhileNode node) { AddNode(node); return true; }
            public override bool Walk(WithNode node) { AddNode(node); return true; }
            public override bool Walk(JsAst jsAst) { return true; }
            public override bool Walk(FunctionExpression functionExpression) { AddNode(functionExpression); return true; }
            public override bool Walk(ExpressionStatement node) { AddNode(node); return true; }

        }
    }

}
