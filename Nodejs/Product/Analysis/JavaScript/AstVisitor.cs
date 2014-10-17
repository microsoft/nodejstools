// IVisitor.cs
//
// Copyright 2011 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Microsoft.NodejsTools.Parsing {
    internal class AstVisitor {
        public virtual bool Walk(ArrayLiteral node) { return true; }
        public virtual bool Walk(BinaryOperator node) { return true; }
        public virtual bool Walk(CommaOperator node) { return true; }
        public virtual bool Walk(Block node) { return true; }
        public virtual bool Walk(Break node) { return true; }
        public virtual bool Walk(CallNode node) { return true; }
        public virtual bool Walk(Conditional node) { return true; }
        public virtual bool Walk(ConstantWrapper node) { return true; }
        public virtual bool Walk(ConstStatement node) { return true; }
        public virtual bool Walk(ContinueNode node) { return true; }
        public virtual bool Walk(DebuggerNode node) { return true; }
        public virtual bool Walk(DirectivePrologue node) { return true; }
        public virtual bool Walk(DoWhile node) { return true; }
        public virtual bool Walk(EmptyStatement node) { return true; }
        public virtual bool Walk(ForIn node) { return true; }
        public virtual bool Walk(ForNode node) { return true; }
        public virtual bool Walk(FunctionObject node) { return true; }
        public virtual bool Walk(GetterSetter node) { return true; }
        public virtual bool Walk(GroupingOperator node) { return true; }
        public virtual bool Walk(IfNode node) { return true; }
        public virtual bool Walk(LabeledStatement node) { return true; }
        public virtual bool Walk(LexicalDeclaration node) { return true; }
        public virtual bool Walk(Lookup node) { return true; }
        public virtual bool Walk(Member node) { return true; }
        public virtual bool Walk(ObjectLiteral node) { return true; }
        public virtual bool Walk(ObjectLiteralField node) { return true; }
        public virtual bool Walk(ObjectLiteralProperty node) { return true; }
        public virtual bool Walk(ParameterDeclaration node) { return true; }
        public virtual bool Walk(RegExpLiteral node) { return true; }
        public virtual bool Walk(ReturnNode node) { return true; }
        public virtual bool Walk(Switch node) { return true; }
        public virtual bool Walk(SwitchCase node) { return true; }
        public virtual bool Walk(ThisLiteral node) { return true; }
        public virtual bool Walk(ThrowNode node) { return true; }
        public virtual bool Walk(TryNode node) { return true; }
        public virtual bool Walk(Var node) { return true; }
        public virtual bool Walk(VariableDeclaration node) { return true; }
        public virtual bool Walk(UnaryOperator node) { return true; }
        public virtual bool Walk(WhileNode node) { return true; }
        public virtual bool Walk(WithNode node) { return true; }
        public virtual bool Walk(YieldExpression node) { return true; }
        public virtual bool Walk(JsAst jsAst) { return true; }
        public virtual bool Walk(FunctionExpression functionExpression) { return true; }
        public virtual bool Walk(ExpressionStatement node) { return true; }
        

        public virtual void PostWalk(ArrayLiteral node) { }
        public virtual void PostWalk(BinaryOperator node) { }
        public virtual void PostWalk(CommaOperator node) { }
        public virtual void PostWalk(Block node) { }
        public virtual void PostWalk(Break node) { }
        public virtual void PostWalk(CallNode node) { }
        public virtual void PostWalk(Conditional node) { }
        public virtual void PostWalk(ConstantWrapper node) { }
        public virtual void PostWalk(ConstStatement node) { }
        public virtual void PostWalk(ContinueNode node) { }
        public virtual void PostWalk(DebuggerNode node) { }
        public virtual void PostWalk(DirectivePrologue node) { }
        public virtual void PostWalk(DoWhile node) { }
        public virtual void PostWalk(EmptyStatement node) { }
        public virtual void PostWalk(ForIn node) { }
        public virtual void PostWalk(ForNode node) { }
        public virtual void PostWalk(FunctionObject node) { }
        public virtual void PostWalk(GetterSetter node) { }
        public virtual void PostWalk(GroupingOperator node) { }
        public virtual void PostWalk(IfNode node) { }
        public virtual void PostWalk(LabeledStatement node) { }
        public virtual void PostWalk(LexicalDeclaration node) { }
        public virtual void PostWalk(Lookup node) { }
        public virtual void PostWalk(Member node) { }
        public virtual void PostWalk(ObjectLiteral node) { }
        public virtual void PostWalk(ObjectLiteralField node) { }
        public virtual void PostWalk(ObjectLiteralProperty node) { }
        public virtual void PostWalk(ParameterDeclaration node) { }
        public virtual void PostWalk(RegExpLiteral node) { }
        public virtual void PostWalk(ReturnNode node) { }
        public virtual void PostWalk(Switch node) { }
        public virtual void PostWalk(SwitchCase node) { }
        public virtual void PostWalk(ThisLiteral node) { }
        public virtual void PostWalk(ThrowNode node) { }
        public virtual void PostWalk(TryNode node) { }
        public virtual void PostWalk(Var node) { }
        public virtual void PostWalk(VariableDeclaration node) { }
        public virtual void PostWalk(UnaryOperator node) { }
        public virtual void PostWalk(WhileNode node) { }
        public virtual void PostWalk(WithNode node) { }
        public virtual void PostWalk(YieldExpression node) { }
        public virtual void PostWalk(JsAst jsAst) { }
        public virtual void PostWalk(FunctionExpression functionExpression) { }
        public virtual void PostWalk(ExpressionStatement node) { }
    }
}
