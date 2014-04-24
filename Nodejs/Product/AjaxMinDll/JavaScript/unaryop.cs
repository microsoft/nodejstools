// unaryop.cs
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

using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Parsing
{
    public class UnaryOperator : Expression
    {
        private Expression m_operand;

        public Expression Operand
        {
            get { return m_operand; }
            set
            {
                m_operand.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_operand = value;
                m_operand.IfNotNull(n => n.Parent = this);
            }
        }

        public TokenWithSpan OperatorContext { get; set; }

        public JSToken OperatorToken { get; set; }
        public bool IsPostfix { get; set; }
        public bool OperatorInConditionalCompilationComment { get; set; }
        public bool ConditionalCommentContainsOn { get; set; }

        public UnaryOperator(TokenWithSpan context, JSParser parser)
            : base(context, parser)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                m_operand.Walk(visitor);
            }
            visitor.PostWalk(this);
        }

        public override PrimitiveType FindPrimitiveType()
        {
            switch (OperatorToken)
            {
                case JSToken.TypeOf:
                    // typeof ALWAYS returns type string
                    return PrimitiveType.String;

                case JSToken.LogicalNot:
                    // ! always return boolean
                    return PrimitiveType.Boolean;

                case JSToken.Void:
                case JSToken.Delete:
                    // void returns undefined.
                    // delete returns number, but just return other
                    return PrimitiveType.Other;

                default:
                    // all other unary operators return a number
                    return PrimitiveType.Number;
            }
        }

        public override OperatorPrecedence Precedence
        {
            get
            {
                // assume unary precedence
                return OperatorPrecedence.Unary;
            }
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Operand);
            }
        }

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (Operand == oldNode)
            {
                Operand = (Expression)newNode;
                return true;
            }
            return false;
        }

        public override bool IsConstant
        {
            get
            {
                return Operand.IfNotNull(o => o.IsConstant);
            }
        }

        public override string ToString()
        {
            return OperatorToken.ToString() + " "
                + (Operand == null ? "<null>" : Operand.ToString());
        }
    }
}
