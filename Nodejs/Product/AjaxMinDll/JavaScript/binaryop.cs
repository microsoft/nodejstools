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
        public TokenWithSpan OperatorContext { get; set; }

        public override TokenWithSpan TerminatingContext
        {
            get
            {
                // if we have one, return it. If not, see ifthe right-hand side has one
                return base.TerminatingContext ?? Operand2.IfNotNull(n => n.TerminatingContext);
            }
        }

        public BinaryOperator(TokenWithSpan context, JSParser parser)
            : base(context, parser)
        {
        }

        public override OperatorPrecedence Precedence
        {
            get 
            {
                switch (OperatorToken)
                {
                    case JSToken.Comma:
                        return OperatorPrecedence.Comma;

                    case JSToken.Assign:
                    case JSToken.BitwiseAndAssign:
                    case JSToken.BitwiseOrAssign:
                    case JSToken.BitwiseXorAssign:
                    case JSToken.DivideAssign:
                    case JSToken.LeftShiftAssign:
                    case JSToken.MinusAssign:
                    case JSToken.ModuloAssign:
                    case JSToken.MultiplyAssign:
                    case JSToken.RightShiftAssign:
                    case JSToken.UnsignedRightShiftAssign:
                    case JSToken.PlusAssign:
                        return OperatorPrecedence.Assignment;

                    case JSToken.LogicalOr:
                        return OperatorPrecedence.LogicalOr;

                    case JSToken.LogicalAnd:
                        return OperatorPrecedence.LogicalAnd;

                    case JSToken.BitwiseOr:
                        return OperatorPrecedence.BitwiseOr;

                    case JSToken.BitwiseXor:
                        return OperatorPrecedence.BitwiseXor;

                    case JSToken.BitwiseAnd:
                        return OperatorPrecedence.BitwiseAnd;

                    case JSToken.Equal:
                    case JSToken.NotEqual:
                    case JSToken.StrictEqual:
                    case JSToken.StrictNotEqual:
                        return OperatorPrecedence.Equality;

                    case JSToken.GreaterThan:
                    case JSToken.GreaterThanEqual:
                    case JSToken.In:
                    case JSToken.InstanceOf:
                    case JSToken.LessThan:
                    case JSToken.LessThanEqual:
                        return OperatorPrecedence.Relational;

                    case JSToken.LeftShift:
                    case JSToken.RightShift:
                    case JSToken.UnsignedRightShift:
                        return OperatorPrecedence.Shift;

                    case JSToken.Multiply:
                    case JSToken.Divide:
                    case JSToken.Modulo:
                        return OperatorPrecedence.Multiplicative;

                    case JSToken.Plus:
                    case JSToken.Minus:
                        return OperatorPrecedence.Additive;

                    default:
                        return OperatorPrecedence.None;
                }
            }
        }

        public override PrimitiveType FindPrimitiveType()
        {
            PrimitiveType leftType;
            PrimitiveType rightType;

            switch (OperatorToken)
            {
                case JSToken.Assign:

                case JSToken.BitwiseAnd:
                case JSToken.BitwiseAndAssign:
                case JSToken.BitwiseOr:
                case JSToken.BitwiseOrAssign:
                case JSToken.BitwiseXor:
                case JSToken.BitwiseXorAssign:
                case JSToken.Divide:
                case JSToken.DivideAssign:
                case JSToken.LeftShift:
                case JSToken.LeftShiftAssign:
                case JSToken.Minus:
                case JSToken.MinusAssign:
                case JSToken.Modulo:
                case JSToken.ModuloAssign:
                case JSToken.Multiply:
                case JSToken.MultiplyAssign:
                case JSToken.RightShift:
                case JSToken.RightShiftAssign:
                case JSToken.UnsignedRightShift:
                case JSToken.UnsignedRightShiftAssign:
                    // always returns a number
                    return PrimitiveType.Number;

                case JSToken.Equal:
                case JSToken.GreaterThan:
                case JSToken.GreaterThanEqual:
                case JSToken.In:
                case JSToken.InstanceOf:
                case JSToken.LessThan:
                case JSToken.LessThanEqual:
                case JSToken.NotEqual:
                case JSToken.StrictEqual:
                case JSToken.StrictNotEqual:
                    // always returns a boolean
                    return PrimitiveType.Boolean;

                case JSToken.PlusAssign:
                case JSToken.Plus:
                    // if either operand is known to be a string, then the result type is a string.
                    // otherwise the result is numeric if both types are known.
                    leftType = Operand1.FindPrimitiveType();
                    rightType = Operand2.FindPrimitiveType();

                    return (leftType == PrimitiveType.String || rightType == PrimitiveType.String)
                        ? PrimitiveType.String
                        : (leftType != PrimitiveType.Other && rightType != PrimitiveType.Other
                            ? PrimitiveType.Number
                            : PrimitiveType.Other);

                case JSToken.LogicalAnd:
                case JSToken.LogicalOr:
                    // these two are special. They return either the left or the right operand
                    // (depending on their values), so unless they are both known types AND the same,
                    // then we can't know for sure.
                    leftType = Operand1.FindPrimitiveType();
                    if (leftType != PrimitiveType.Other)
                    {
                        if (leftType == Operand2.FindPrimitiveType())
                        {
                            // they are both the same and neither is unknown
                            return leftType;
                        }
                    }

                    // if we get here, then we don't know the type
                    return PrimitiveType.Other;

                default:
                    // shouldn't get here....
                    return PrimitiveType.Other;
            }
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

        public override Node LeftHandSide
        {
            get
            {
                // the operand1 is on the left
                return Operand1.LeftHandSide;
            }
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

        /// <summary>
        /// Returns true if the expression contains an in-operator
        /// </summary>
        public override bool ContainsInOperator
        {
            get
            {
                // if we are an in-operator, then yeah: we contain one.
                // otherwise recurse the operands.
                return OperatorToken == JSToken.In
                    ? true
                    : Operand1.ContainsInOperator || Operand2.ContainsInOperator;
            }
        }

        public override bool IsConstant
        {
            get
            {
                return Operand1.IfNotNull(o => o.IsConstant) && Operand2.IfNotNull(o => o.IsConstant);
            }
        }

        public override string ToString()
        {
            return (Operand1 == null ? "<null>" : Operand1.ToString())
                + ' ' + OperatorToken.ToString() + ' '
                + (Operand2 == null ? "<null>" : Operand2.ToString());
        }
    }
}
