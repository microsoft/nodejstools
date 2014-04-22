// Copyright 2010 Microsoft Corporation
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

using System;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Parsing
{
#if FALSE
    internal class NewParensVisitor : AstVisitor
    {
        private bool m_needsParens;// = false;
        private bool m_outerHasNoArguments;

        public static bool NeedsParens(Node expression, bool outerHasNoArguments)
        {
            var visitor = new NewParensVisitor(outerHasNoArguments);
            expression.Accept(visitor);
            return visitor.m_needsParens;
        }

        private NewParensVisitor(bool outerHasNoArguments)
        {
            // save whether or not the outer new-operator has any arguments itself
            m_outerHasNoArguments = outerHasNoArguments;
        }

        #region IVisitor Members

        public override void Visit(ArrayLiteral node)
        {
            // don't recurse; we don't need parens around this
        }

        public override void Visit(BinaryOperator node)
        {
            // lesser precedence than the new operator; use parens
            m_needsParens = true;
        }

        public override void Visit(CallNode node)
        {
            if (node != null)
            {
                if (node.InBrackets)
                {
                    // if this is a member-bracket operation, then *we* don't need parens, but we shoul
                    // recurse the function in case something in there does
                    node.Function.Accept(this);
                }
                else if (!node.IsConstructor)
                {
                    // we have parens for our call arguments, so we definitely
                    // need to be wrapped and there's no need to recurse
                    m_needsParens = true;
                }
                else
                {
                    // we are a new-operator - if we have any arguments then we're good to go
                    // because those arguments will be associated with us, not the outer new.
                    // but if we don't have any arguments, we might need to be wrapped in parens
                    // so any outer arguments don't get associated with us
                    if (node.Arguments == null || node.Arguments.Count == 0)
                    {
                        m_needsParens = !m_outerHasNoArguments;
                    }
                }
            }
            else
            {
                // shouldn't happen, but we're a call so let's wrap in parens
                m_needsParens = true;
            }
        }

        public override void Visit(ConditionalCompilationComment node)
        {
            if (node != null)
            {
                // recurse the children, but as soon as we get the flag set to true, bail
                foreach (var child in node.Children)
                {
                    child.Accept(this);
                    if (m_needsParens)
                    {
                        break;
                    }
                }
            }
        }

        public override void Visit(ConditionalCompilationElse node)
        {
            // preprocessor nodes are handled outside the real JavaScript parsing
        }

        public override void Visit(ConditionalCompilationElseIf node)
        {
            // preprocessor nodes are handled outside the real JavaScript parsing
        }

        public override void Visit(ConditionalCompilationEnd node)
        {
            // preprocessor nodes are handled outside the real JavaScript parsing
        }

        public override void Visit(ConditionalCompilationIf node)
        {
            // preprocessor nodes are handled outside the real JavaScript parsing
        }

        public override void Visit(ConditionalCompilationOn node)
        {
            // preprocessor nodes are handled outside the real JavaScript parsing
        }

        public override void Visit(ConditionalCompilationSet node)
        {
            // preprocessor nodes are handled outside the real JavaScript parsing
        }

        public override void Visit(Conditional node)
        {
            // lesser precedence than the new operator; use parens
            m_needsParens = true;
        }

        public override void Visit(ConstantWrapper node)
        {
            // we're good
        }

        public override void Visit(ConstantWrapperPP node)
        {
            // we're good
        }

        public override void Visit(CustomNode node)
        {
            // we're good
        }

        public override void Visit(FunctionObject node)
        {
            // we're good
        }

        public virtual void Visit(GroupingOperator node)
        {
            // definitely does NOT need parens, because we will
            // output parens ourselves. And don't bother recursing.
        }

        public override void Visit(ImportantComment node)
        {
            // don't recurse
        }

        public override void Visit(Lookup node)
        {
            // we're good
        }

        public override void Visit(Member node)
        {
            // need to recurse the collection
            if (node != null)
            {
                node.Root.Accept(this);
            }
        }

        public override void Visit(ObjectLiteral node)
        {
            // we're good
        }

        public override void Visit(ParameterDeclaration node)
        {
            // we're good
        }

        public override void Visit(RegExpLiteral node)
        {
            // we're good
        }

        public override void Visit(ThisLiteral node)
        {
            // we're good
        }

        public override void Visit(UnaryOperator node)
        {
            // lesser precedence than the new operator; use parens
            m_needsParens = true;
        }

        #endregion

        #region nodes we shouldn't hit

        //
        // expression elements we shouldn't get to
        //

        public override void Visit(AstNodeList node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(GetterSetter node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(ObjectLiteralField node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(ObjectLiteralProperty node)
        {
            Debug.Fail("shouldn't get here");
        }

        //
        // statements (we should only hit expressions)
        //

        public override void Visit(Block node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(Break node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(ConstStatement node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(ContinueNode node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(DebuggerNode node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(DirectivePrologue node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(DoWhile node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(EmptyStatement node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(ForIn node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(ForNode node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(IfNode node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(LabeledStatement node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(LexicalDeclaration node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(ReturnNode node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(Switch node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(SwitchCase node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(ThrowNode node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(TryNode node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(Var node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(VariableDeclaration node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(WhileNode node)
        {
            Debug.Fail("shouldn't get here");
        }

        public override void Visit(WithNode node)
        {
            Debug.Fail("shouldn't get here");
        }

        #endregion
    }
#endif
}
