// conditional.cs
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
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{

    public sealed class Conditional : Expression
    {
        private Expression m_condition;
        private Expression m_trueExpression;
        private Expression m_falseExpression;

        public Expression Condition
        {
            get { return m_condition; }
            set
            {
                m_condition.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_condition = value;
                m_condition.IfNotNull(n => n.Parent = this);
            }
        }

        public Expression TrueExpression
        {
            get { return m_trueExpression; }
            set
            {
                m_trueExpression.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_trueExpression = value;
                m_trueExpression.IfNotNull(n => n.Parent = this);
            }
        }

        public Expression FalseExpression
        {
            get { return m_falseExpression; }
            set
            {
                m_falseExpression.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_falseExpression = value;
                m_falseExpression.IfNotNull(n => n.Parent = this);
            }
        }

        public TokenWithSpan QuestionContext { get; set; }
        public TokenWithSpan ColonContext { get; set; }

        public Conditional(TokenWithSpan context, JSParser parser)
            : base(context, parser)
        {
        }

        public override OperatorPrecedence Precedence
        {
            get
            {
                return OperatorPrecedence.Conditional;
            }
        }

        public void SwapBranches()
        {
            var temp = m_trueExpression;
            m_trueExpression = m_falseExpression;
            m_falseExpression = temp;
        }

        public override PrimitiveType FindPrimitiveType()
        {
            if (TrueExpression != null && FalseExpression != null)
            {
                // if the primitive type of both true and false expressions is the same, then
                // we know the primitive type. Otherwise we do not.
                PrimitiveType trueType = TrueExpression.FindPrimitiveType();
                if (trueType == FalseExpression.FindPrimitiveType())
                {
                    return trueType;
                }
            }

            // nope -- they don't match, so we don't know
            return PrimitiveType.Other;
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Condition, TrueExpression, FalseExpression);
            }
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                m_condition.Walk(visitor);
                m_trueExpression.Walk(visitor);
                m_falseExpression.Walk(visitor);
            }
            visitor.PostWalk(this);
        }

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (Condition == oldNode)
            {
                Condition = (Expression)newNode;
                return true;
            }
            if (TrueExpression == oldNode)
            {
                TrueExpression = (Expression)newNode;
                return true;
            }
            if (FalseExpression == oldNode)
            {
                FalseExpression = (Expression)newNode;
                return true;
            }
            return false;
        }
    }
}