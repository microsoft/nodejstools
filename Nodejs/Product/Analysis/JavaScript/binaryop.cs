// binaryop.cs
//
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
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{

    public class BinaryOperator : Expression
    {
        private Expression m_operand1;
        private Expression m_operand2;

        public Expression Operand1 
        {
            get { return m_operand1; }
            set
            {
                m_operand1.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_operand1 = value;
                m_operand1.IfNotNull(n => n.Parent = this);
            }
        }

        public Expression Operand2 
        {
            get { return m_operand2; }
            set
            {
                m_operand2.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_operand2 = value;
                m_operand2.IfNotNull(n => n.Parent = this);
            }
        }

        public JSToken OperatorToken { get; set; }

        public BinaryOperator(IndexSpan span)
            : base(span)
        {
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Operand1, Operand2);
            }
        }

        public override void Walk(AstVisitor walker) {
            if (walker.Walk(this)) {
                m_operand1.Walk(walker);
                m_operand2.Walk(walker);
            }
            walker.PostWalk(this);
        }
        
        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (Operand1 == oldNode)
            {
                Operand1 = (Expression)newNode;
                return true;
            }
            if (Operand2 == oldNode)
            {
                Operand2 = (Expression)newNode;
                return true;
            }
            return false;
        }

        public void SwapOperands()
        {
            // swap the operands -- we don't need to go through ReplaceChild or the
            // property setters because we don't need to change the Parent pointers 
            // or anything like that.
            Expression temp = m_operand1;
            m_operand1 = m_operand2;
            m_operand2 = temp;
        }

        public bool IsAssign
        {
            get
            {
                switch(OperatorToken)
                {
                    case JSToken.Assign:
                    case JSToken.PlusAssign:
                    case JSToken.MinusAssign:
                    case JSToken.MultiplyAssign:
                    case JSToken.DivideAssign:
                    case JSToken.ModuloAssign:
                    case JSToken.BitwiseAndAssign:
                    case JSToken.BitwiseOrAssign:
                    case JSToken.BitwiseXorAssign:
                    case JSToken.LeftShiftAssign:
                    case JSToken.RightShiftAssign:
                    case JSToken.UnsignedRightShiftAssign:
                        return true;

                    default:
                        return false;
                }
            }
        }

        internal override string GetFunctionGuess(Node target)
        {
            return Operand2 == target
                ? IsAssign ? Operand1.GetFunctionGuess(this) : Parent.GetFunctionGuess(this)
                : string.Empty;
        }

        public override string ToString()
        {
            return (Operand1 == null ? "<null>" : Operand1.ToString())
                + ' ' + OperatorToken.ToString() + ' '
                + (Operand2 == null ? "<null>" : Operand2.ToString());
        }
    }
}
