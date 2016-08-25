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

namespace Microsoft.NodejsTools.Parsing {
    [Serializable]
    internal sealed class Conditional : Expression
    {
        private Expression m_condition;
        private Expression m_trueExpression;
        private Expression m_falseExpression;

        public Expression Condition
        {
            get { return m_condition; }
            set
            {
                m_condition = value;
            }
        }

        public Expression TrueExpression
        {
            get { return m_trueExpression; }
            set
            {
                m_trueExpression = value;
            }
        }

        public Expression FalseExpression
        {
            get { return m_falseExpression; }
            set
            {
                m_falseExpression = value;
            }
        }

        public Conditional(EncodedSpan span)
            : base(span)
        {
        }

        public void SwapBranches()
        {
            var temp = m_trueExpression;
            m_trueExpression = m_falseExpression;
            m_falseExpression = temp;
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
    }
}