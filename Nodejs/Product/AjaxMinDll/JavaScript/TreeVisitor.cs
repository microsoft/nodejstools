// TreeVisitor.cs
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

namespace Microsoft.NodejsTools.Parsing
{
#if FALSE
    public class TreeVisitor : AstVisitor
    {
        public TreeVisitor() { }

        #region IVisitor Members

        public override void Visit(ArrayLiteral node)
        {
            if (node != null)
            {
                foreach (var childNode in node.Children)
                {
                    childNode.Accept(this);
                }
            }
        }

        public override void Visit<T>(AstNodeList<T> node) 
        {
            if (node != null)
            {
                foreach (var childNode in node.Children)
                {
                    childNode.Accept(this);
                }
            }
        }

        public override void Visit(BinaryOperator node)
        {
            if (node != null)
            {
                if (node.Operand1 != null)
                {
                    node.Operand1.Accept(this);
                }

                if (node.Operand2 != null)
                {
                    node.Operand2.Accept(this);
                }
            }
        }

        public override void Visit(Block node)
        {
            if (node != null)
            {
                foreach (var childNode in node.Children)
                {
                    childNode.Accept(this);
                }
            }
        }

        public override void Visit(Break node)
        {
            if (node != null)
            {
                // no children
            }
        }

        public override void Visit(CallNode node)
        {
            if (node != null)
            {
                if (node.Function != null)
                {
                    node.Function.Accept(this);
                }

                if (node.Arguments != null)
                {
                    node.Arguments.Accept(this);
                }
            }
        }
#if FALSE
        public override void Visit(ConditionalCompilationComment node)
        {
            if (node != null)
            {
                if (node.Statements != null)
                {
                    node.Statements.Accept(this);
                }
            }
        }

        public override void Visit(ConditionalCompilationElse node)
        {
            // no children
        }

        public override void Visit(ConditionalCompilationElseIf node)
        {
            if (node != null)
            {
                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }
            }
        }

        public override void Visit(ConditionalCompilationEnd node)
        {
            // no children
        }

        public override void Visit(ConditionalCompilationIf node)
        {
            if (node != null)
            {
                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }
            }
        }

        public override void Visit(ConditionalCompilationOn node)
        {
            // no children
        }

        public override void Visit(ConditionalCompilationSet node)
        {
            if (node != null)
            {
                if (node.Value != null)
                {
                    node.Value.Accept(this);
                }
            }
        }
#endif
        public override void Visit(Conditional node)
        {
            if (node != null)
            {
                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }

                if (node.TrueExpression != null)
                {
                    node.TrueExpression.Accept(this);
                }

                if (node.FalseExpression != null)
                {
                    node.FalseExpression.Accept(this);
                }
            }
        }

        public override void Visit(ConstantWrapper node)
        {
            // no children
        }

        public override void Visit(ConstantWrapperPP node)
        {
            // no children
        }

        public override void Visit(ConstStatement node)
        {
            if (node != null)
            {
                foreach (var childNode in node.Children)
                {
                    childNode.Accept(this);
                }
            }
        }

        public override void Visit(ContinueNode node)
        {
            if (node != null)
            {
                // no children
            }
        }

        public override void Visit(DebuggerNode node)
        {
            // no children
        }

        public override void Visit(DirectivePrologue node)
        {
            // no children
        }

        public override void Visit(DoWhile node)
        {
            if (node != null)
            {
                if (node.Body != null)
                {
                    node.Body.Accept(this);
                }

                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }
            }
        }

        public override void Visit(EmptyStatement node)
        {
            // no children
        }

        public override void Visit(ForIn node)
        {
            if (node != null)
            {
                if (node.Variable != null)
                {
                    node.Variable.Accept(this);
                }

                if (node.Collection != null)
                {
                    node.Collection.Accept(this);
                }

                if (node.Body != null)
                {
                    node.Body.Accept(this);
                }
            }
        }

        public override void Visit(ForNode node)
        {
            if (node != null)
            {
                if (node.Initializer != null)
                {
                    node.Initializer.Accept(this);
                }

                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }

                if (node.Incrementer != null)
                {
                    node.Incrementer.Accept(this);
                }

                if (node.Body != null)
                {
                    node.Body.Accept(this);
                }
            }
        }

        public override void Visit(FunctionObject node)
        {
            if (node != null)
            {
                if (node.Body != null)
                {
                    node.Body.Accept(this);
                }
            }
        }

        public override void Visit(GetterSetter node)
        {
            // no children
        }

        public override void Visit(GroupingOperator node)
        {
            if (node != null && node.Operand != null)
            {
                node.Operand.Accept(this);
            }
        }

        public override void Visit(IfNode node)
        {
            if (node != null)
            {
                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }

                if (node.TrueBlock != null)
                {
                    node.TrueBlock.Accept(this);
                }

                if (node.FalseBlock != null)
                {
                    node.FalseBlock.Accept(this);
                }
            }
        }

        public override void Visit(ImportantComment node)
        {
            // no children
        }

        public override void Visit(LabeledStatement node)
        {
            if (node != null)
            {
                if (node.Statement != null)
                {
                    node.Statement.Accept(this);
                }
            }
        }

        public override void Visit(LexicalDeclaration node)
        {
            if (node != null)
            {
                foreach (var childNode in node.Children)
                {
                    childNode.Accept(this);
                }
            }
        }

        public override void Visit(Lookup node)
        {
            // no children
        }

        public override void Visit(Member node)
        {
            if (node != null)
            {
                if (node.Root != null)
                {
                    node.Root.Accept(this);
                }
            }
        }

        public override void Visit(ObjectLiteral node)
        {
            if (node != null)
            {
                if (node.Properties != null)
                {
                    node.Properties.Accept(this);
                }
            }
        }

        public override void Visit(ObjectLiteralField node)
        {
            // no children
        }

        public override void Visit(ObjectLiteralProperty node)
        {
            if (node != null)
            {
                if (node.Name != null)
                {
                    node.Name.Accept(this);
                }

                if (node.Value != null)
                {
                    node.Value.Accept(this);
                }
            }
        }

        public override void Visit(ParameterDeclaration node)
        {
            // no children
        }

        public override void Visit(RegExpLiteral node)
        {
            // no children
        }

        public override void Visit(ReturnNode node)
        {
            if (node != null)
            {
                if (node.Operand != null)
                {
                    node.Operand.Accept(this);
                }
            }
        }

        public override void Visit(Switch node)
        {
            if (node != null)
            {
                if (node.Expression != null)
                {
                    node.Expression.Accept(this);
                }

                if (node.Cases != null)
                {
                    node.Cases.Accept(this);
                }
            }
        }

        public override void Visit(SwitchCase node)
        {
            if (node != null)
            {
                if (node.CaseValue != null)
                {
                    node.CaseValue.Accept(this);
                }

                if (node.Statements != null)
                {
                    node.Statements.Accept(this);
                }
            }
        }

        public override void Visit(ThisLiteral node)
        {
            // no children
        }

        public override void Visit(ThrowNode node)
        {
            if (node != null)
            {
                if (node.Operand != null)
                {
                    node.Operand.Accept(this);
                }
            }
        }

        public override void Visit(TryNode node)
        {
            if (node != null)
            {
                if (node.TryBlock != null)
                {
                    node.TryBlock.Accept(this);
                }

                if (node.CatchBlock != null)
                {
                    node.CatchBlock.Accept(this);
                }

                if (node.FinallyBlock != null)
                {
                    node.FinallyBlock.Accept(this);
                }
            }
        }

        public override void Visit(Var node)
        {
            if (node != null)
            {
                foreach (var childNode in node.Children)
                {
                    childNode.Accept(this);
                }
            }
        }

        public override void Visit(VariableDeclaration node)
        {
            if (node != null)
            {
                if (node.Initializer != null)
                {
                    node.Initializer.Accept(this);
                }
            }
        }

        public override void Visit(UnaryOperator node)
        {
            if (node != null)
            {
                if (node.Operand != null)
                {
                    node.Operand.Accept(this);
                }
            }
        }

        public override void Visit(WhileNode node)
        {
            if (node != null)
            {
                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }

                if (node.Body != null)
                {
                    node.Body.Accept(this);
                }
            }
        }

        public override void Visit(WithNode node)
        {
            if (node != null)
            {
                if (node.WithObject != null)
                {
                    node.WithObject.Accept(this);
                }

                if (node.Body != null)
                {
                    node.Body.Accept(this);
                }
            }
        }

        #endregion

        public override void Visit(CommaOperator node)
        {
            throw new System.NotImplementedException();
        }

        public override void Visit(JsAst jsAst)
        {
            throw new System.NotImplementedException();
        }

        public override void Visit(FunctionExpression functionExpression)
        {
            throw new System.NotImplementedException();
        }
    }
#endif
}
